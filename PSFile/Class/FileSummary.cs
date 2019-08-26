using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Diagnostics;

namespace PSFile
{
    public class FileSummary
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? IsInherited { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastWriteTime { get; set; }
        public DateTime? LastAccessTime { get; set; }
        public string Hash { get; set; }
        public string Attributes { get; set; }
        public long? Size { get; set; }
        public bool? IsSecurityBlock { get; set; }

        /// <summary>
        /// Compare-File、Compre-DirectoryでRootPathLengthの値だけ、Pathから削ることがあるので、
        /// 各種Pathへの操作は「_Path」に対して行うことにする。
        /// </summary>
        private string _Path;

        public FileSummary() { }
        public FileSummary(string path)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
        }
        public FileSummary(string path, bool isLoad)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (isLoad && File.Exists(_Path))
            {
                LoadSecurity();
                LoadTime();
                LoadHash();
                LoadAttributes();
                LoadSize();
                LoadSecurityBlock();
            }
        }
        public FileSummary(string path,
            bool ignoreSecurity, bool ignoreTime, bool ignoreHash, bool ignoreAttributes, bool ignoreSize, bool ignoreSecurityBlock)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (File.Exists(_Path))
            {
                if (!ignoreSecurity) { LoadSecurity(); }
                if (!ignoreTime) { LoadTime(); }
                if (!ignoreHash) { LoadHash(); }
                if (!ignoreAttributes) { LoadAttributes(); }
                if (!ignoreSize) { LoadSize(); }
                if (!ignoreSecurityBlock) { LoadSecurityBlock(); }
            }
        }
        public FileSummary(string path, int rootPathLength,
            bool ignoreSecurity, bool ignoreTime, bool ignoreHash, bool ignoreAttributes, bool ignoreSize, bool ignoreSecurityBlock)
        {
            this.Path = path.Substring(rootPathLength);
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (File.Exists(_Path))
            {
                if (!ignoreSecurity) { LoadSecurity(); }
                if (!ignoreTime) { LoadTime(); }
                if (!ignoreHash) { LoadHash(); }
                if (!ignoreAttributes) { LoadAttributes(); }
                if (!ignoreSize) { LoadSize(); }
                if (!ignoreSecurityBlock) { LoadSecurityBlock(); }
            }
        }

        /// <summary>
        /// Access, Owner, Inheritedの情報を読み込み
        /// </summary>
        public void LoadSecurity()
        {
            FileSecurity security = File.GetAccessControl(_Path);

            //  Access
            this.Access = FileControl.AccessRulesToString(security.GetAccessRules(true, false, typeof(NTAccount)));

            //  Owner
            this.Owner = security.GetOwner(typeof(NTAccount)).Value;

            //  Inherited
            this.IsInherited = !security.AreAccessRulesProtected;
        }

        /// <summary>
        /// ファイルの作成日時,更新日時,アクセス日時を取得
        /// </summary>
        public void LoadTime()
        {
            this.CreationTime = File.GetCreationTime(_Path);
            this.LastWriteTime = File.GetLastWriteTime(_Path);
            this.LastAccessTime = File.GetLastAccessTime(_Path);
        }

        /// <summary>
        /// Sha256でハッシュ値取得
        /// </summary>
        public void LoadHash()
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            using (FileStream fs = new FileStream(_Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] sha256bytes = sha256.ComputeHash(fs);
                sha256.Clear();
                this.Hash = BitConverter.ToString(sha256bytes).ToLower().Replace("-", "");
            }
        }

        /// <summary>
        /// ファイルの属性を取得
        /// </summary>
        public void LoadAttributes()
        {
            this.Attributes = File.GetAttributes(_Path).ToString();
        }

        /// <summary>
        /// ファイルサイズを取得
        /// </summary>
        public void LoadSize()
        {
            this.Size = new System.IO.FileInfo(_Path).Length;
        }

        /// <summary>
        /// セキュリティブロックの設定の有効/無効を取得
        /// </summary>
        public void LoadSecurityBlock()
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = $"/c more < \"{_Path}\":Zone.Identifier";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();

                string resultString = proc.StandardOutput.ReadToEnd();
                this.IsSecurityBlock = resultString.Contains("ZoneId=3");

                proc.WaitForExit();
            }
        }
    }
}
