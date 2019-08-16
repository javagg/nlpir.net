using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Lingjoin.Global
{
    public abstract class Base
    {
        public string RootDir => rootDir;

        public string DataDir => dataDir;

        private string DataDirFull => $"{Path.Combine(dataDir, "data")}{Path.PathSeparator}";

        public abstract string DllFileName { get; }

        public abstract string AuthorizationFileName { get; }

        protected abstract bool InitFunctions();

        protected Base(params string[] otherDllNames)
        {
            CopyAuthorization();
            CopyOtherDll(otherDllNames);
            if (!File.Exists(DllDir + DllFileName))
            {
                throw new Exception("相关组件DLL不存在：" + DllFileName);
            }

            if (!DllWrapper.LoadDll(DllDir + DllFileName))
            {
                throw new Exception("相关组件DLL加载失败：" + DllFileName);
            }

            if (!InitFunctions())
            {
                throw new Exception("相关组件方法加载失败：" + DllFileName);
            }
        }

        ~Base()
        {
            DllWrapper.UnLoadDll();
        }

        protected void CopyAuthorization()
        {
            if (string.IsNullOrWhiteSpace(AuthorizationFileName))
            {
                return;
            }

            string text = AuthorizationDir + AuthorizationFileName;
            string text2 = this.DataDirFull + AuthorizationFileName;
            if (File.Exists(text) && Directory.Exists(DataDirFull) && text != text2)
            {
                File.Copy(text, text2, true);
            }
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000021B4 File Offset: 0x000003B4
        protected virtual void CopyOtherDll(params string[] dllNames)
        {
            if (dllNames.Length == 0)
            {
                return;
            }

            foreach (string text in dllNames)
            {
                string text2;
                if (string.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath))
                {
                    text2 = text;
                }
                else
                {
                    text2 = AppDomain.CurrentDomain.RelativeSearchPath + "\\" + text;
                }

                string text3 = Base.DllDir + text;
                if (!File.Exists(text2))
                {
                    if (!File.Exists(text3))
                    {
                        throw new Exception("相关组件DLL不存在：" + text);
                    }

                    File.Copy(text3, text2, true);
                }
            }
        }

        protected DllWrapper DllWrapper = new DllWrapper();

        protected static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        private static readonly string rootDir = AppSettings["RootDirPath"] ?? "";

        private static readonly string DllDir = rootDir + AppSettings["LibPath"];

        private static readonly string AuthorizationDir =
            rootDir + (AppSettings["AuthorizationPath"] ?? "..\\Authorization\\");

        private static readonly string dataDir = rootDir + AppSettings["DataDirPath"];
    }


    public static class DateTimeExtensions
    {
        // Token: 0x06000015 RID: 21 RVA: 0x000024E8 File Offset: 0x000006E8
        public static long CurrentMilliseconds(this DateTime d)
        {
            return (long) (d.ToUniversalTime() - DateTimeExtensions.Jan1St1970).TotalMilliseconds;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002510 File Offset: 0x00000710
        public static long CurrentSeconds(this DateTime d)
        {
            return (long) (d.ToUniversalTime() - DateTimeExtensions.Jan1St1970).TotalSeconds;
        }

        // Token: 0x0400000C RID: 12
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public class DllWrapper
    {
        [DllImport("Kernel32")]
        public static extern IntPtr LoadLibrary(string funcname);

        [DllImport("Kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string funcname);

        [DllImport("Kernel32")]
        public static extern IntPtr FreeLibrary(IntPtr handle);

        public bool LoadDll(string lpFileName)
        {
            hModule = LoadLibrary(lpFileName);
            return !(hModule == IntPtr.Zero);
        }

        public void UnLoadDll()
        {
            FreeLibrary(hModule);
            this.hModule = IntPtr.Zero;
            this.farProc = IntPtr.Zero;
        }

        public Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
        {
            IntPtr procAddress = GetProcAddress(dllModule, functionName);
            if (procAddress == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.GetDelegateForFunctionPointer(procAddress, t);
        }

        public IntPtr hModule = IntPtr.Zero;

        public IntPtr farProc = IntPtr.Zero;
    }

    public static class Helper
    {
        public static string ToEncode(this string source, string fromEncodeName, string toEncodeName)
        {
            return fromEncodeName == toEncodeName
                ? source
                : Encoding.GetEncoding(toEncodeName).GetString(Encoding.GetEncoding(fromEncodeName).GetBytes(source));
        }

        public static string CombineTwoResult(string result1, string result2)
        {
            XElement xelement = XElement.Parse(result1);
            XContainer xcontainer = XElement.Parse(result2);
            string value = xelement.Element("Result-Number").Element("Total-Number").Value;
            string value2 = xcontainer.Element("Result-Number").Element("Total-Number").Value;
            IEnumerable<XElement> first = xelement.Elements("Result").Elements("Document").ToArray<XElement>();
            XElement[] second = xelement.Elements("Result").Elements("Document").ToArray<XElement>();
            XElement[] array = first.Union(second, XElementComparer.Instance).ToArray<XElement>();
            XElement xelement2 = new XElement(xelement.Name, new object[]
            {
                xelement.Elements("Result-Number"),
                array
            });
            xelement2.Element("Result-Number").Element("Total-Number").SetValue(int.Parse(value) + int.Parse(value2));
            xelement2.Element("Result-Number").Element("Return-Number").SetValue(array.Length);
            return xelement2.ToString();
        }
    }

    public enum ModePass
    {
        ByValue = 1,
        ByRef
    }


    public class XElementComparer : IEqualityComparer<XElement>
    {
        public static XElementComparer Instance { get; } = new XElementComparer();

        private XElementComparer()
        {
        }

        public bool Equals(XElement x, XElement y)
        {
            return x == y || (x != null && y != null && x.Element("doc_id") != null && y.Element("doc_id") != null &&
                              x.Element("doc_id") == y.Element("doc_id"));
        }

        public int GetHashCode(XElement node)
        {
            XElement xelement = (node != null) ? node.Element("doc_id") : null;
            if (xelement == null)
            {
                return 0;
            }

            return xelement.GetHashCode();
        }
    }
}