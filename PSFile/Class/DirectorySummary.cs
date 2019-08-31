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
        public bool? Inherited { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastWriteTime { get; set; }
        public string Attributes { get; set; }
        public long? Size { get; set; }
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
            if (isLoad && Directory.Exists(_Path))
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
            if (Directory.Exists(_Path))
            {
                if (!ignoreSecurity) { LoadSecurity(); }
                if (!ignoreTime) { LoadTime(); }
                if (!ignoreAttributes) { LoadAttributes(); }
                if (!ignoreSize) { LoadSize(); }
                if (!ignoreFiles) { LoadFiles(ignoreSecurity, ignoreTime, ignoreAttributes, ignoreSize, isLightFiles); }
            }
        }
        public DirectorySummary(string path, int rootPathLength,
            bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool ignoreFiles, bool isLightFiles)
        {
            this.Path = path.Substring(rootPathLength);
            this.Name = System.IO.Path.GetFileName(path);
            _Path = path;
            if (Directory.Exists(_Path))
            {
                if (!ignoreSecurity) { LoadSecurity(); }
                if (!ignoreTime) { LoadTime(); }
                if (!ignoreAttributes) { LoadAttributes(); }
                if (!ignoreSize) { LoadSize(); }
                if (!ignoreFiles) { LoadFiles(rootPathLength, ignoreSecurity, ignoreTime, ignoreAttributes, ignoreSize, isLightFiles); }
            }
        }

        /// <summary>
        /// Access, Owner, Inheritedの情報を読み込み
        /// </summary>
        public void LoadSecurity()
        {
            DirectorySecurity security = Directory.GetAccessControl(_Path);

            //  Access
            this.Access = DirectoryControl.AccessRulesToString(security.GetAccessRules(true, false, typeof(NTAccount)));

            //  Owner
            this.Owner = security.GetOwner(typeof(NTAccount)).Value;

            //  Inherited
            this.Inherited = !security.AreAccessRulesProtected;
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
            LoadFiles(false, false, false, false, false);
        }
        /// <summary>
        /// 配下ファイルのFileSummaryを取得。
        /// </summary>
        /// <param name="isLightFiles">Hash,SecurityBlockをスキップするかどうか</param>
        private void LoadFiles(bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool isLightFiles)
        {
            Files = new List<FileSummary>();
            foreach (string fileName in Directory.GetFiles(_Path))
            {
                Files.Add(new FileSummary(fileName,
                    ignoreSecurity, ignoreTime, isLightFiles, ignoreAttributes, ignoreSize, isLightFiles));
            }
        }
        /// <summary>
        /// 配下ファイルのFileSummaryを取得。
        /// </summary>
        /// <param name="isLightFiles">Hash,SecurityBlockをスキップするかどうか</param>
        /// <param name="rootPathLength">Compare-Directory用。指定した値だけ、Pathの文字列を削る</param>
        private void LoadFiles(int rootPathLength, bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool isLightFiles)
        {
            Files = new List<FileSummary>();
            foreach (string fileName in Directory.GetFiles(_Path))
            {
                Files.Add(new FileSummary(fileName, rootPathLength,
                    ignoreSecurity, ignoreTime, isLightFiles, ignoreAttributes, ignoreSize, isLightFiles));
            }
        }

    }
}
