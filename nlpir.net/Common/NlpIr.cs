using System.Runtime.InteropServices;

namespace NlpIr
{
    public static class Constants
    {
       public static int GBK_CODE = 0;
       public static int UTF8_CODE = 1;
       public static int BIG5_CODE = 2;
       public static int GBK_FANTI_CODE = 3;

       public static int ICT_POS_MAP_SECOND = 0;
       public static int ICT_POS_MAP_FIRST = 1;
       public static int PKU_POS_MAP_SECOND = 2;
       public static int PKU_POS_MAP_FIRST = 3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Result
    {
        public int Start;  //start position,词语在输入句子中的开始位置
        public int Length; //length,词语的长度
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public char[] sPOS;//word type，词性ID值，可以快速的获取词性表
        public fixed byte PosData[40];
        public int Pos;//词性标注的编号
        public int WordId; //该词的内部ID号，如果是未登录词，设成0或者-1
        public int WordType; //区分用户词典;1，是用户词典中的词；0，非用户词典中的词
        public int Weight;//word weight,read weight
    };
    
    public static class Natives
    {
        [DllImport(Libraries.SystemNative, EntryPoint = "NLPIR_Init")]
        public static extern bool Init(string dataPath, int encode, string licenseCode);
        
        [DllImport(Libraries.SystemNative, EntryPoint = "NLPIR_Exit")]
        public static extern bool Exit();

        [DllImport(Libraries.SystemNative, EntryPoint = "NLPIR_ParagraphProcess")]
        public static extern string ParagraphProcess(string src, int posTagged);       
    }    
}