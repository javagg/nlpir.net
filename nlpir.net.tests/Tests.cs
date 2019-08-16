using System;
using System.IO;
using Xunit;

namespace NlpIr.Tests
{
    public class Tests
    {
        [Fact]
        public void TestLoadingAndUnloading()
        {
            var dataPath = Directory.GetCurrentDirectory();
            var result = Natives.Init(dataPath, Constants.UTF8_CODE, "");
            Assert.True(result);
            result = Natives.Exit();
            Assert.True(result);
        }
        
        [Fact]
        public void TestSegmentize()
        {
            var dataPath = Directory.GetCurrentDirectory();
            if (!Natives.Init(dataPath, Constants.UTF8_CODE, "")) return;
            var text = "在此处添加代码以启动应用程序";
            var result =  Natives.ParagraphProcess(text, false ? 1: 0);
            Assert.True("在 此处 添加 代码 以 启动 应用 程序 ".Equals(result));
            Natives.Exit();
        }
    }
}