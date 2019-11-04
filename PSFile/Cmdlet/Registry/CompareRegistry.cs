using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Management.Automation;
using System.IO;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリキー同士を比較
    /// TestGenerator : 無し
    /// </summary>
    [Cmdlet(VerbsData.Compare, "Registry")]
    public class CompareRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string RegistryPath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Difference { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreValues { get; set; }

        protected override void ProcessRecord()
        {
            string tempDir = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Item.APPLICATION_NAME);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            //  比較元レジストリのサマリを取得
            List<RegistrySummary> compare_ref = GetSummaryList(RegistryPath, IgnoreSecurity, IgnoreValues);
            string text_ref = JsonConvert.SerializeObject(compare_ref, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(Path.Combine(tempDir, "compre_ref.json"),
                false, Encoding.UTF8))
            {
                sw.WriteLine(text_ref);
            }

            //  比較先レジストリのサマリを取得
            List<RegistrySummary> compare_dif = GetSummaryList(Difference, IgnoreSecurity, IgnoreValues);
            string text_dif = JsonConvert.SerializeObject(compare_dif, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(Path.Combine(tempDir, "compre_dif.json"),
                false, Encoding.UTF8))
            {
                sw.WriteLine(text_dif);
            }

            int retVal = string.Compare(text_ref, text_dif);
            WriteObject(retVal);
        }

        /// <summary>
        /// RegistrySummaryリストを取得
        /// </summary>
        /// <param name="path">レジストリキーのパス</param>
        /// <param name="ignoreSecurity">セキュリティ情報を除外して比較</param>
        /// <param name="ignoreValues">レジストリ値を場外して比較</param>
        /// <returns></returns>
        private List<RegistrySummary> GetSummaryList(string path, bool ignoreSecurity, bool ignoreValues)
        {
            int startLength = 0;
            List<RegistrySummary> summaryList = new List<RegistrySummary>();
            Action<RegistryKey> getSummary = null;
            getSummary = (targetPath) =>
            {
                RegistrySummary summary = new RegistrySummary(targetPath, startLength, ignoreSecurity, ignoreValues);
                summary.Name = "";
                summaryList.Add(summary);
                //summaryList.Add(new RegistrySummary(targetPath, startLength, ignoreSecurity, ignoreValues));

                foreach (string keyName in targetPath.GetSubKeyNames())
                {
                    using (RegistryKey subTargetKey = targetPath.OpenSubKey(keyName, false))
                    {
                        getSummary(subTargetKey);
                    }
                }
            };
            using (RegistryKey startKey = RegistryControl.GetRegistryKey(path, false, false))
            {
                startLength = startKey.Name.Length;
                getSummary(startKey);
            }
            return summaryList;
        }
    }
}
