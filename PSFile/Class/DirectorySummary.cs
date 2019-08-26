using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;
using System.IO;

namespace PSFile
{
    public class DirectorySummary
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? IsInherited { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastWriteTime { get; set; }
        public string Attributes { get; set; }
        public long Size { get; set; }
        public List<FileSummary> Files { get; set; }

        /// <summary>
        /// Compare-DirectoryでRootPathLengthの値だけ、Pathから削ることがあるので、
        /// 各種Pathへの操作は「_Path」に対して行うことにする。
        /// </summary>
        private string _Path;

        public DirectorySummary() { }
        public DirectorySummary(string path)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
        }
        public DirectorySummary(string path, bool isLoad)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (isLoad)
            {
                LoadSecurity();
                LoadTime();
                LoadAttributes();
                LoadSize();
                LoadFiles();
            }
        }
        public DirectorySummary(string path,
            bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool ignoreFiles, bool isLightFiles)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (!ignoreSecurity) { LoadSecurity(); }
            if (!ignoreTime) { LoadTime(); }
            if (!ignoreAttributes) { LoadAttributes(); }
            if (!ignoreSize) { LoadSize(); }
            if (!ignoreFiles) { LoadFiles(isLightFiles); }
        }
        public DirectorySummary(string path, int rootPathLength,
            bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool ignoreFiles, bool isLightFiles)
        {
            this.Path = path.Substring(rootPathLength);
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (!ignoreSecurity) { LoadSecurity(); }
            if (!ignoreTime) { LoadTime(); }
            if (!ignoreAttributes) { LoadAttributes(); }
            if (!ignoreSize) { LoadSize(); }
            if (!ignoreFiles) { LoadFiles(isLightFiles, rootPathLength); }
        }

        /// <summary>
        /// Access, Owner, Inheritedの情報を読み込み
        /// </summary>
        public void LoadSecurity()
        {
            DirectorySecurity security = Directory.GetAccessControl(_Path);

            //  Access
            /*
            List<string> directoryAccessRuleList = new List<string>();
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
            {
                directoryAccessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    rule.FileSystemRights,
                    rule.InheritanceFlags,
                    rule.PropagationFlags,
                    rule.AccessControlType));
            }
            this.Access = string.Join("/", directoryAccessRuleList);
            */
            this.Access = DirectoryControl.AccessRulesToString(security.GetAccessRules(true, false, typeof(NTAccount)));

            //  Owner
            this.Owner = security.GetOwner(typeof(NTAccount)).Value;

            //  Inherited
            this.IsInherited = !security.AreAccessRulesProtected;
        }

        /// <summary>
        /// フォルダーの作成日時,更新日時,アクセス日時を取得
        /// </summary>
        public void LoadTime()
        {
            this.CreationTime = Directory.GetCreationTime(_Path);
            this.LastWriteTime = Directory.GetLastWriteTime(_Path);
        }

        /// <summary>
        /// フォルダーの属性を取得
        /// </summary>
        public void LoadAttributes()
        {
            this.Attributes = File.GetAttributes(_Path).ToString();
        }

        /// <summary>
        /// 配下の全ファイルの合計サイズを取得
        /// </summary>
        public void LoadSize()
        {
            foreach (FileInfo fi in new DirectoryInfo(_Path).GetFiles("*", SearchOption.AllDirectories))
            {
                this.Size += fi.Length;
            }
        }

        /// <summary>
        /// 配下ファイルのFileSummaryを取得。Publicなので外部からの呼び出しもOK
        /// </summary>
        public void LoadFiles()
        {
            LoadFiles(false);
        }
        /// <summary>
        /// 配下ファイルのFileSummaryを取得。
        /// </summary>
        /// <param name="isLightFiles">Hash,SecurityBlockをスキップするかどうか</param>
        private void LoadFiles(bool isLightFiles)
        {
            Files = new List<FileSummary>();
            /*
            foreach (FileInfo fi in new DirectoryInfo(_Path).GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                Files.Add(new FileSummary(fi.FullName, false, false, isLightFiles, false, false, isLightFiles));
            }
            */
            foreach (string fileName in Directory.GetFiles(_Path))
            {
                Files.Add(new FileSummary(fileName, false, false, isLightFiles, false, false, isLightFiles));
            }
        }
        /// <summary>
        /// 配下ファイルのFileSummaryを取得。
        /// </summary>
        /// <param name="isLightFiles">Hash,SecurityBlockをスキップするかどうか</param>
        /// <param name="rootPathLength">Compare-Directory用。指定した値だけ、Pathの文字列を削る</param>
        private void LoadFiles(bool isLightFiles, int rootPathLength)
        {
            Files = new List<FileSummary>();
            /*
            foreach (FileInfo fi in new DirectoryInfo(_Path).GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                Files.Add(new FileSummary(fi.FullName, rootPathLength,
                    false, false, isLightFiles, false, false, isLightFiles));
            }
            */
            foreach(string fileName in Directory.GetFiles(_Path))
            {
                Files.Add(new FileSummary(fileName, rootPathLength,
                    false, false, isLightFiles, false, false, isLightFiles));
            }
        }

    }
}
