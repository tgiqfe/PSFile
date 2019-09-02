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
            using (StreamWriter sw = new StreamWriter(OutputFile, true, Encoding.GetEncoding("Shift_JIS")))
            {
                sw.WriteLine(code);
            }
        }

        #region File

        public void FilePath(string path)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1}",
            path, Item.PATH));

        }
        public void FileHash(string path, string hash)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Hash \"{2}\"",
                path, Item.HASH, hash));
        }
        public void FileAccess(string path, string access, bool isContain)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Access \"{2}\" -TestMode {3}",
                path, Item.ACCESS, access, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void FileOwner(string path, string owner)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Owner \"{2}\"",
                path, Item.OWNER, owner));
        }
        public void FileAttributes(string path, string attributes, bool isContain)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Attributes \"{2}\" -TestMode {3}",
                path, Item.ATTRIBUTES, attributes, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void FileAttributes(string path, FileAttributes attributes, bool isContain)
        {
            if (!_writable) { return; }
            FileAttributes(
                path,
                "@(\"" + string.Join("\", \"", Functions.SplitComma(attributes.ToString())) + "\")",
                isContain);
        }
        public void FileInherited(string path, bool inherited)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Inherited ${2}",
                path, Item.INHERITED, inherited));
        }
        public void FileSecurityBlock(string path, bool securityBlock)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -SecurityBlock ${2}",
                path, Item.SECURITYBLOCK, securityBlock));
        }
        public void FileSize(string path, long size)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -Size {2}",
                path, Item.SIZE, size));
        }
        public void FileCreationTime(string path, DateTime creationTime)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -CreationTime \"{2}\"",
                path, Item.CREATIONTIME, creationTime.ToString("yyyy/MM/dd hh:mm:ss")));
        }
        public void FileLastWriteTime(string path, DateTime lastWriteTime)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-File -Path \"{0}\" -Target {1} -LastWriteTime \"{2}\"",
                path, Item.CREATIONTIME, lastWriteTime.ToString("yyyy/MM/dd hh:mm:ss")));
        }
        public void FileCompare(string path, string difference,
            bool ignoreSecurity, bool ignoreTime, bool ignoreHash, bool ignoreAttributes, bool ignoreSize, bool ignoreSecurityBlock)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Compare-File -Path \"{0}\" -Target {1} -Difference \"{2}\"",
                path, Item.PATH, difference));
            if (ignoreSecurity) { sb.Append(" -IgnoreSecurity"); }
            if (ignoreTime) { sb.Append(" -IgnoreTime"); }
            if (ignoreHash) { sb.Append(" -IgnoreHash"); }
            if (ignoreAttributes) { sb.Append(" -IgnoreAttributes"); }
            if (ignoreSize) { sb.Append(" -IgnoreSize"); }
            if (ignoreSecurityBlock) { sb.Append(" -IgnoreSecurityBlock"); }
            WriteCode(sb.ToString());
        }

        #endregion

        //  ================================================================================

        #region Directory

        public void DirectoryPath(string path)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1}",
                path, Item.PATH));
        }
        public void DirectoryAccess(string path, string access, bool isContain)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -Access \"{2}\" -TestMode {3}",
                path, Item.ACCESS, access, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void DirectoryOwner(string path, string owner)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -Owner \"{2}\"",
                path, Item.OWNER, owner));
        }
        public void DirectoryAttributes(string path, string attributes, bool isContain)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -Attributes \"{2}\" -TestMode {3}",
                path, Item.ATTRIBUTES, attributes, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void DirectoryAttributes(string path, FileAttributes attributes, bool isContain)
        {
            if (!_writable) { return; }
            DirectoryAttributes(
                path,
                "@(\"" + string.Join("\", \"", Functions.SplitComma(attributes.ToString())) + "\")",
                isContain);
        }
        public void DirectoryInherited(string path, bool inherited)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -Inherited ${2}",
                path, Item.INHERITED, inherited));
        }
        public void DirectorySize(string path, long size)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -Size {2}",
                path, Item.SIZE, size));
        }
        public void DirectoryCreationTime(string path, DateTime creationTime)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -CreationTime \"{2}\"",
                path, Item.CREATIONTIME, creationTime.ToString("yyyy/MM/dd hh:mm:ss")));
        }
        public void DirectoryLastWriteTime(string path, DateTime lastWriteTime)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Directory -Path \"{0}\" -Target {1} -LastWriteTime \"{2}\"",
                path, Item.CREATIONTIME, lastWriteTime.ToString("yyyy/MM/dd hh:mm:ss")));
        }
        public void DirectoryCompare(string path, string difference,
            bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool ignoreFiles, bool isLightFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Compare-Directory -Path \"{0}\" -Target {1} -Difference \"{2}\"",
                path, Item.PATH, difference));
            if (ignoreSecurity) { sb.Append(" -IgnoreSecurity"); }
            if (ignoreTime) { sb.Append(" -IgnoreTime"); }
            if (ignoreAttributes) { sb.Append(" -IgnoreAttributes"); }
            if (ignoreSize) { sb.Append(" -IgnoreSize"); }
            if (ignoreFiles) { sb.Append(" -IgnoreFiles"); }
            if (isLightFiles) { sb.Append(" -IsLightFiles"); }
            WriteCode(sb.ToString());
        }

        #endregion

        //  ================================================================================

        #region Registry

        public void RegistryPath(string path)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Registry -Path \"{0}\" -Target {1}",
                path, Item.PATH));
        }
        public void RegistryAccess(string path, string access, bool isContain)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Registry -Path \"{0}\" -Target {1} -Access \"{2}\" -TestMode {3}",
                path, Item.ACCESS, access, isContain ? Item.CONTAIN : Item.MATCH));
        }
        public void RegistryOwner(string path, string owner)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Registry -Path \"{0}\" -Target {1} -Owner \"{2}\"",
                path, Item.OWNER, owner));
        }
        public void RegistryInherited(string path, bool inherited)
        {
            if (!_writable) { return; }
            WriteCode(string.Format("Test-Registry -Path \"{0}\" -Target {1} -Inherited ${2}",
                path, Item.INHERITED, inherited));
        }

        public void RegistryCompare(string path, string difference,
            bool ignoreSecurity, bool ignoreValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Compare-Registry -Path \"{0}\" -Target {1} -Difference \"{2}\"",
                path, Item.PATH, difference));
            if (ignoreSecurity) { sb.Append(" -IgnoreSecurity"); }
            if (ignoreValues) { sb.Append(" -IgnoreValues"); }
            WriteCode(sb.ToString());
        }

        #endregion
    }
}
