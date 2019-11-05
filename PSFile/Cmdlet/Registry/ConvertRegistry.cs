using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.IO;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsData.Convert, "Registry")]
    public class ConvertRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }
        [Parameter(Position = 1)]
        public string Name { get; set; }
        [Parameter, Alias("File")]
        public string OutputFile { get; set; }
        [Parameter]
        public SwitchParameter Recursive { get; set; }

        protected override void ProcessRecord()
        {
            List<string> commandList = new List<string>();

            if (Name == null)
            {
                //  レジストリキーをSetコマンドへコンバート
                Action<RegistryKey> measureRegistry = null;
                measureRegistry = (targetKey) =>
                {
                    string[] valueNames = targetKey.GetValueNames();
                    if (valueNames.Length > 0)
                    {
                        //  レジストリ値の設定用コマンド
                        foreach (string valueName in targetKey.GetValueNames())
                        {
                            RegistryValueKind valueKind = targetKey.GetValueKind(valueName);
                            string regValue = RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true);
                            switch (RegistryControl.ValueKindToString(valueKind))
                            {
                                case Item.REG_SZ:
                                case Item.REG_MULTI_SZ:
                                case Item.REG_EXPAND_SZ:
                                case Item.REG_BINARY:
                                    regValue = string.Format("-Value \"{0}\" ", regValue);
                                    break;
                                case Item.REG_DWORD:
                                case Item.REG_QWORD:
                                    regValue = string.Format("-Value {0} ", regValue);
                                    break;
                                case Item.NONE:
                                    regValue = "";
                                    break;
                            }

                            commandList.Add(string.Format(
                                "Set-Registry -Path \"{0}\" -Name \"{1}\" {2}-Type {3}",
                                    targetKey, valueName, regValue, RegistryControl.ValueKindToString(valueKind)));
                        }
                    }
                    else
                    {
                        //  レジストリ値設定無し。空レジストリキー作成
                        commandList.Add(string.Format("New-Registry -Path \"{0}\"", targetKey));
                    }

                    //  配下のレジストリキーを再帰的にチェック
                    if (Recursive)
                    {
                        foreach (string keyName in targetKey.GetSubKeyNames())
                        {
                            using (RegistryKey subTargetKey = targetKey.OpenSubKey(keyName, false))
                            {
                                measureRegistry(subTargetKey);
                            }
                        }
                    }
                };
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
                {
                    measureRegistry(regKey);
                }
            }
            else
            {
                //  レジストリ値をSetコマンドへコンバート
                using (RegistryKey targetKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
                {
                    RegistryValueKind valueKind = targetKey.GetValueKind(Name);
                    string regValue = RegistryControl.RegistryValueToString(targetKey, Name, valueKind, true);
                    switch (RegistryControl.ValueKindToString(valueKind))
                    {
                        case Item.REG_SZ:
                        case Item.REG_MULTI_SZ:
                        case Item.REG_EXPAND_SZ:
                        case Item.REG_BINARY:
                            regValue = string.Format("-Value \"{0}\" ", regValue);
                            break;
                        case Item.REG_DWORD:
                        case Item.REG_QWORD:
                            regValue = string.Format("-Value {0} ", regValue);
                            break;
                        case Item.NONE:
                            regValue = "";
                            break;
                    }

                    commandList.Add(string.Format(
                        "Set-Registry -Path \"{0}\" -Name \"{1}\" {2}-Type {3}",
                            targetKey, Name, regValue, RegistryControl.ValueKindToString(valueKind)));
                }
            }

            //  コンソール/ファイルへ出力
            if (OutputFile == null)
            {
                WriteObject(commandList);
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(OutputFile, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    sw.WriteLine(string.Join("\r\n", commandList));
                }
            }
        }
    }
}
