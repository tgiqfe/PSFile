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
    [Cmdlet(VerbsData.Compare, "Directory")]
    public class CompareDirectory : PSCmdlet
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
        public SwitchParameter IgnoreAttributes { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSize { get; set; }
        [Parameter]
        public SwitchParameter IgnoreFiles { get; set; }
        [Parameter]
        public SwitchParameter IsLightFiles { get; set; }

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
                long tempSize = (long)new DirectorySummary(Path, true, true, true, false, true, true).Size;
                WriteObject(tempSize.CompareTo(Size));
                return;
            }

            //  CreationTime比較
            if (Target == Item.CREATIONTIME)
            {
                DateTime tempDate = (DateTime)new DirectorySummary(Path, true, false, true, true, true, true).CreationTime;
                tempDate = tempDate.AddTicks(-(tempDate.Ticks % TimeSpan.TicksPerSecond));
                WriteObject(tempDate.CompareTo(CreationTime));
                return;
            }

            //  LastWriteTime比較
            if (Target == Item.LASTWRITETIME)
            {
                DateTime tempDate = (DateTime)new DirectorySummary(Path, true, false, true, true, true, true).CreationTime;
                tempDate = tempDate.AddTicks(-(tempDate.Ticks % TimeSpan.TicksPerSecond));
                WriteObject(tempDate.CompareTo(LastWriteTime));
                return;
            }

            #region Compare Path
            //  Path比較
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Item.APPLICATION_NAME);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元フォルダーのサマリを取得
            List<DirectorySummary> compare_ref = GetSummaryList(Path,
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
            #endregion
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
