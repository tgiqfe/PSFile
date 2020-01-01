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
    /// フォルダー比較
    /// TestGenerator : 無し
    /// </summary>
    [Cmdlet(VerbsData.Compare, "Directory")]
    public class CompareDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Difference { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreTime { get; set; }
        [Parameter]
        public SwitchParameter IgnoreAttributes { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSize { get; set; }
        [Parameter]
        public SwitchParameter IgnoreFiles { get; set; }
        [Parameter]
        public SwitchParameter IsLightFiles { get; set; }

        protected override void ProcessRecord()
        {
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Item.APPLICATION_NAME);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元フォルダーのサマリを取得
            List<DirectorySummary> compare_ref = GetSummaryList(DirectoryPath,
                IgnoreSecurity, IgnoreTime, IgnoreAttributes, IgnoreSize, IgnoreFiles, IsLightFiles);
            string text_ref = JsonConvert.SerializeObject(compare_ref, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(
                System.IO.Path.Combine(tempDir, "compre_ref.json"), false, Encoding.UTF8))
            {
                sw.WriteLine(text_ref);
            }

            //  比較先フォルダーのサマリを取得
            List<DirectorySummary> compare_dif = GetSummaryList(Difference,
                IgnoreSecurity, IgnoreTime, IgnoreAttributes, IgnoreSize, IgnoreFiles, IsLightFiles);
            string text_dif = JsonConvert.SerializeObject(compare_dif, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(
                System.IO.Path.Combine(tempDir, "compre_dif.json"), false, Encoding.UTF8))
            {
                sw.WriteLine(text_dif);
            }

            int retValue = string.Compare(text_ref, text_dif);
            WriteObject(retValue);
        }

        /// <summary>
        /// DirectorySummaryリストを取得
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ignoreSecurity"></param>
        /// <param name="ignoreTime"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="ignoreSize"></param>
        /// <param name="ignoreFiles"></param>
        /// <param name="isLightFiles"></param>
        /// <returns></returns>
        private List<DirectorySummary> GetSummaryList(string path,
            bool ignoreSecurity, bool ignoreTime, bool ignoreAttributes, bool ignoreSize, bool ignoreFiles, bool isLightFiles)
        {
            List<DirectorySummary> summaryList = new List<DirectorySummary>();
            int startLength = 0;
            
            Action<string> getSummary = null;
            getSummary = (targetPath) =>
            {
                DirectorySummary summary = new DirectorySummary(targetPath, startLength,
                    ignoreSecurity, ignoreTime, ignoreAttributes, ignoreSize, ignoreFiles, isLightFiles);
                summary.Name = "";
                summaryList.Add(summary);

                foreach (string subTargetPath in Directory.GetDirectories(targetPath))
                {
                    getSummary(subTargetPath);
                }
            };

            if (Directory.Exists(path))
            {
                startLength = path.Length;
                getSummary(path);
            }

            return summaryList;
        }
    }
}
