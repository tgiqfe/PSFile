using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.AccessControl;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using PSFile.Serialize;

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
        [Parameter, ValidateSet(Item.XML, Item.JSON, Item.YML, Item.TXT)]
        public string DataType { get; set; } = Item.TXT;

        private bool hasPath = true;
        private bool hasName = true;
        private bool hasValue = true;

        private List<RegistryKeyNameValue> KNVList = null;

        protected override void BeginProcessing()
        {
            _SearchTarget = Item.CheckCase(SearchTarget);
            if (!string.IsNullOrEmpty(_SearchTarget))
            {
                hasPath = _SearchTarget.Contains(Item.PATH);
                hasName = _SearchTarget.Contains(Item.NAME);
                hasValue = _SearchTarget.Contains(Item.VALUE);
            }
            DataType = Item.CheckCase(DataType);

            KNVList = new List<RegistryKeyNameValue>();
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                SearchKeyNameValue(regKey);
            }

            switch (DataType)
            {
                case Item.XML:
                    WriteObject(
                        DataSerializer.Serialize<List<RegistryKeyNameValue>>(KNVList, PSFile.Serialize.DataType.Xml));
                    break;
                case Item.JSON:
                    WriteObject(
                        DataSerializer.Serialize<List<RegistryKeyNameValue>>(KNVList, PSFile.Serialize.DataType.Json));
                    break;
                case Item.YML:
                    WriteObject(
                        DataSerializer.Serialize<List<RegistryKeyNameValue>>(KNVList, PSFile.Serialize.DataType.Yml));
                    break;
                default:
                    WriteObject(KNVList);
                    break;
            }
        }

        private void SearchKeyNameValue(RegistryKey targetKey)
        {
            RegistryKeyNameValue regKNV = new RegistryKeyNameValue();

            //  レジストリキーをチェック
            if (hasPath && (targetKey.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                regKNV.AddKey(targetKey);
            }

            //  レジストリ値をチェック
            foreach (string valueName in targetKey.GetValueNames())
            {
                RegistryValueKind valueKind = targetKey.GetValueKind(valueName);

                //  名前チェック
                if (hasName && (valueName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    regKNV.AddValue(
                        targetKey,
                        valueName,
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true));
                    continue;
                }

                //  値チェック
                if (hasValue && (
                    valueKind == RegistryValueKind.String ||
                    valueKind == RegistryValueKind.MultiString ||
                    valueKind == RegistryValueKind.ExpandString))
                {
                    string tempValueName = string.IsNullOrEmpty(valueName) ? "(既定)" : valueName;
                    string tempStringValue =
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true);
                    if (tempStringValue.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        regKNV.AddValue(targetKey, tempValueName, tempStringValue);
                    }
                }
            }

            if (regKNV.Enabled)
            {
                KNVList.Add(regKNV);
            }

            foreach (string keyName in targetKey.GetSubKeyNames())
            {
                using (RegistryKey subTargetKey = targetKey.OpenSubKey(keyName, false))
                {
                    SearchKeyNameValue(subTargetKey);
                }
            }
        }
    }
}
