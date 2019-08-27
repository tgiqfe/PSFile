using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsData.Compare, "Registry")]
    public class CompareRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Difference { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreValues { get; set; }

        protected override void ProcessRecord()
        {
            List<RegistrySummary> compare_ref = GetPRegList(Path, IgnoreSecurity, IgnoreValues);
            List<RegistrySummary> compare_dif = GetPRegList(Difference, IgnoreSecurity, IgnoreValues);
            int retVal = string.Compare(
                JsonConvert.SerializeObject(compare_ref),
                JsonConvert.SerializeObject(compare_dif));
            WriteObject(retVal);
        }

        /// <summary>
        /// RegistryKeyInfoリストを取得
        /// </summary>
        /// <param name="path">レジストリキーのパス</param>
        /// <param name="ignoreSecurity">セキュリティ情報を除外して比較</param>
        /// <param name="ignoreValues">レジストリ値を場外して比較</param>
        /// <returns></returns>
        private List<RegistrySummary> GetPRegList(string path, bool ignoreSecurity, bool ignoreValues)
        {
            int startKeyLength = 0;
            List<RegistrySummary> pregList = new List<RegistrySummary>();
            Action<RegistryKey> getPReg = null;
            getPReg = (targetKey) =>
            {
                pregList.Add(new RegistrySummary(targetKey, startKeyLength, ignoreSecurity, ignoreValues));
                foreach (string keyName in targetKey.GetSubKeyNames())
                {
                    using (RegistryKey subTargetKey = targetKey.OpenSubKey(keyName, false))
                    {
                        getPReg(subTargetKey);
                    }
                }
            };
            using (RegistryKey startKey = RegistryControl.GetRegistryKey(path, false, false))
            {
                startKeyLength = startKey.Name.Length;
                getPReg(startKey);
            }
            return pregList;
        }

    }
}
