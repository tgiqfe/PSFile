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
    [Cmdlet(VerbsData.Compare, "File")]
    public class CompareFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
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

        protected override void ProcessRecord()
        {
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "PowerReg");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元ファイルのサマリを取得
            List<FileSummary> compare_ref = GetSummaryList(Path,
                IgnoreSecurity, IgnoreTime, IgnoreHash, IgnoreAttributes, IgnoreSize, IgnoreSecurityBlock);
            string text_ref = JsonConvert.SerializeObject(compare_ref);
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(tempDir, "compre_ref.json"),
                false, Encoding.UTF8))
            {
                sw.WriteLine(text_ref);
            }

            //  比較先ファイルのサマリを取得
            List<FileSummary> compare_dif = GetSummaryList(Difference,
                IgnoreSecurity, IgnoreTime, IgnoreHash, IgnoreAttributes, IgnoreSize, IgnoreSecurityBlock);
            string text_dif = JsonConvert.SerializeObject(compare_dif);
            using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(tempDir, "compre_dif.json"),
                false, Encoding.UTF8))
            {
                sw.WriteLine(text_dif);
            }

            int retVal = string.Compare(text_ref, text_dif);
            WriteObject(retVal);
        }

        private List<FileSummary> GetSummaryList(string path,
            bool ignoreSecurity, bool ignoreTime, bool ignoreHash, bool ignoreAttributes, bool ignoreSize, bool ignoreSecurityBlock)
        {
            FileSummary summary = new FileSummary(path, path.Length,
                ignoreSecurity, ignoreTime, ignoreHash, ignoreAttributes, ignoreSize, ignoreSecurityBlock);
            summary.Name = "";

            List<FileSummary> summaryList = new List<FileSummary>();
            summaryList.Add(summary);

            return summaryList;
        }
    }
}
