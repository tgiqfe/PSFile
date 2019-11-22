using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.AccessControl;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリから文字列を検索
    /// </summary>
    [Cmdlet(VerbsCommon.Search, "Registry")]
    public class SearchRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string SearchText { get; set; }
        [Parameter, ValidateSet(Item.PATH, Item.NAME, Item.VALUE)]
        public string[] SearchTarget { get; set; }
        private string _SearchTarget = null;
        private bool hasPath = true;
        private bool hasName = true;
        private bool hasValue = true;

        protected override void BeginProcessing()
        {
            _SearchTarget = Item.CheckCase(SearchTarget);
            if (!string.IsNullOrEmpty(_SearchTarget))
            {
                hasPath = _SearchTarget.Contains(Item.PATH);
                hasName = _SearchTarget.Contains(Item.NAME);
                hasValue = _SearchTarget.Contains(Item.VALUE);
            }
        }

        protected override void ProcessRecord()
        {
            List<string> resultList = new List<string>();
            Action<RegistryKey> searchRegistryPath = null;
            searchRegistryPath = (targetKey) =>
            {
                if (hasPath && (targetKey.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    resultList.Add(string.Format("[{0}]",
                        targetKey.ToString()));
                }
                foreach (string valueName in targetKey.GetValueNames())
                {
                    if (hasName && (valueName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        resultList.Add(string.Format("[{0}]\r\n  : {1}",
                            targetKey.ToString(), valueName));
                    }
                    if (hasValue)
                    {
                        RegistryValueKind valueKind = targetKey.GetValueKind(valueName);
                        switch (valueKind)
                        {
                            case RegistryValueKind.String:
                            case RegistryValueKind.MultiString:
                            case RegistryValueKind.ExpandString:
                                string tempStringValue =
                                    RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true);
                                if (tempStringValue.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    resultList.Add(string.Format("[{0}]\r\n  : {1}\r\n  - {2}",
                                        targetKey.ToString(), valueName, tempStringValue));
                                }
                                break;
                        }
                    }
                }
                foreach (string keyName in targetKey.GetSubKeyNames())
                {
                    using (RegistryKey subTargetKey = targetKey.OpenSubKey(keyName, false))
                    {
                        searchRegistryPath(subTargetKey);
                    }
                }
            };
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                searchRegistryPath(regKey);
            }

            WriteObject(resultList);
        }
    }
}
