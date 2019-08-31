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
        [Parameter(Position = 1)]
        public string Difference { get; set; }
        [Parameter]
        [ValidateSet(Item.PATH, Item.SIZE, Item.CREATIONTIME, Item.LASTWRITETIME, Item.LASTACCESSTIME)]
        public string Target { get; set; }
        [Parameter]
        public DateTime? CreationTime { get; set; }
        [Parameter]
        public DateTime? LastWriteTime { get; set; }
        [Parameter]
        public long? Size { get; set; }
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

        protected override void BeginProcessing()
        {
            DetectTargetParameter();
        }

        /// <summary>
        /// Targetパラメータの自動解析
        /// 解析優先度： Size -> CreationTime -> LastWriteTime -> Path
        /// </summary>
        private void DetectTargetParameter()
        {
            if (Target == null)
            {
                if (Size != null)
                {
                    Target = Item.SIZE;
                }
                else if (CreationTime != null)
                {
                    Target = Item.CREATIONTIME;
                }
                else if (LastWriteTime != null)
                {
                    Target = Item.LASTWRITETIME;
                }
                else
                {
                    Target = Item.PATH;
                }
            }
        }

        protected override void ProcessRecord()
        {
            //  Size比較
            if (Target == Item.SIZE)
            {
                long tempSize = (long)new FileSummary(Path, true, true, true, true, false, true).Size;
                WriteObject(tempSize.CompareTo(Size));
                return;
            }

            //  CreationTime比較
            if (Target == Item.CREATIONTIME)
            {
                DateTime tempDate = (DateTime)new FileSummary(Path, true, false, true, true, true, true).CreationTime;
                tempDate = tempDate.AddTicks(-(tempDate.Ticks % TimeSpan.TicksPerSecond));
                WriteObject(tempDate.CompareTo(CreationTime));
                return;
            }

            //  LastWriteTime比較
            if (Target == Item.LASTWRITETIME)
            {
                DateTime tempDate = (DateTime)new FileSummary(Path, true, false, true, true, true, true).LastWriteTime;
                tempDate = tempDate.AddTicks(-(tempDate.Ticks % TimeSpan.TicksPerSecond));
                WriteObject(tempDate.CompareTo(LastWriteTime));
                return;
            }

            //  Path比較
            #region Compare Path
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "PowerReg");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元ファイルのサマリを取得
            List<FileSummary> compare_ref = GetSummaryList(Path,
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
            #endregion
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
