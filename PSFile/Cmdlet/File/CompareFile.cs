using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Newtonsoft.Json;
using System.IO;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// ファイル情報を比較
    /// </summary>
    [Cmdlet(VerbsData.Compare, "File")]
    public class CompareFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Difference { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreTime { get; set; }
        [Parameter]
        public SwitchParameter IgnoreHash { get; set; }
        [Parameter]
        public SwitchParameter IgnoreAttributes { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSize { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurityBlock { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Item.APPLICATION_NAME);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元ファイルのサマリを取得
            List<FileSummary> compare_ref = GetSummaryList(FilePath,
                IgnoreSecurity, IgnoreTime, IgnoreHash, IgnoreAttributes, IgnoreSize, IgnoreSecurityBlock);
            string text_ref = JsonConvert.SerializeObject(compare_ref, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(
                System.IO.Path.Combine(tempDir, "compre_ref.json"), false, Encoding.UTF8))
            {
                sw.WriteLine(text_ref);
            }

            //  比較先ファイルのサマリを取得
            List<FileSummary> compare_dif = GetSummaryList(Difference,
                IgnoreSecurity, IgnoreTime, IgnoreHash, IgnoreAttributes, IgnoreSize, IgnoreSecurityBlock);
            string text_dif = JsonConvert.SerializeObject(compare_dif, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(
                System.IO.Path.Combine(tempDir, "compre_dif.json"), false, Encoding.UTF8))
            {
                sw.WriteLine(text_dif);
            }

            int retValue = string.Compare(text_ref, text_dif);
            WriteObject(retValue);
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }

        /// <summary>
        /// FileSummaryリストを取得
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ignoreSecurity"></param>
        /// <param name="ignoreTime"></param>
        /// <param name="ignoreHash"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="ignoreSize"></param>
        /// <param name="ignoreSecurityBlock"></param>
        /// <returns></returns>
        private List<FileSummary> GetSummaryList(string path,
            bool ignoreSecurity, bool ignoreTime, bool ignoreHash, bool ignoreAttributes, bool ignoreSize, bool ignoreSecurityBlock)
        {
            List<FileSummary> summaryList = new List<FileSummary>();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                FileSummary summary = new FileSummary(path, path.Length,
                    ignoreSecurity, ignoreTime, ignoreHash, ignoreAttributes, ignoreSize, ignoreSecurityBlock);
                summary.Name = "";
                summaryList.Add(summary);
            }
            return summaryList;
        }
    }
}
