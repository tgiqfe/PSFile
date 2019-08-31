using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PSFile
{
    /// <summary>
    /// Test-****、Compare-**** テストを自動生成
    /// </summary>
    class TestGenerator
    {
        public string OutputFile { get; set; }
        private bool _writable = false;

        public TestGenerator() { }
        public TestGenerator(string outputFile)
        {
            this.OutputFile = outputFile;
            if (OutputFile != null)
            {
                _writable = true;
                if (!Directory.Exists(Path.GetDirectoryName(OutputFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
                }
                if (File.Exists(outputFile) && new FileInfo(outputFile).Length > 0)
                {
                    WriteCode("");
                }
            }
        }

        /// <summary>
        /// テストコードを出力
        /// </summary>
        /// <param name="code"></param>
        private void WriteCode(string code)
        {
            if (_writable)
            {
                using (StreamWriter sw = new StreamWriter(OutputFile, true, Encoding.GetEncoding("Shift_JIS")))
                {
                    sw.WriteLine(code);
                }
            }
        }

        #region File

        public void FilePath(string path)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1}",
                path, Item.PATH));
        }
        public void FileHash(string path, string hash)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Hash \"{2}\"",
                path, Item.HASH, hash));
        }
        public void FileAccess(string path, string access, bool isContain)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Access \"{2}\" -TestMode {3}",
                path, Item.ACCESS, access, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void FileOwner(string path, string owner)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Owner \"{2}\"",
                path, Item.OWNER, owner));
        }
        public void FileAttributes(string path, string attributes, bool isContain)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Attributes \"{2}\" -TestMode {3}",
                path, Item.ATTRIBUTES, attributes, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void FileAttributes(string path, FileAttributes attributes, bool isContain)
        {
            FileAttributes(
                path,
                "@(\"" + string.Join("\", \"", Functions.SplitComma(attributes.ToString())) + "\")",
                isContain);
        }

        public void FileInherited(string path, bool inherited)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Inherited ${2}",
                path, Item.INHERITED, inherited));
        }
        public void FileSecurityBlock(string path, bool securityBlock)
        {
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -SecurityBlock ${2}",
                path, Item.SECURITYBLOCK, securityBlock));
        }

        #endregion


    }
}
