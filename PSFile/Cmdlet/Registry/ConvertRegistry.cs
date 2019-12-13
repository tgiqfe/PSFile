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
        [Parameter]
        public SwitchParameter Dos { get; set; }

        protected override void ProcessRecord()
        {
            List<string> commandList = new List<string>();

            if (Name == null)
            {
                Action<RegistryKey> measureRegistry = null;
                measureRegistry = (targetKey) =>
                {
                    List<string> valueNameList = new List<string>(targetKey.GetValueNames());
                    valueNameList.Sort();
                    if (valueNameList.Count > 0)
                    {
                        valueNameList.ForEach(x =>
                            commandList.Add(Dos ?
                                CreateDosCommand(targetKey, x) :
                                CreateSetCommand(targetKey, x)));
                    }
                    else
                    {
                        //  レジストリ値設定無し。空レジストリキー作成
                        if (Dos)
                        {
                            commandList.Add(string.Format("reg add \"{0}\" /ve /f",
                                ReplaceDoller(targetKey.ToString())));
                            commandList.Add(string.Format("reg delete \"{0}\" /ve /f",
                                ReplaceDoller(targetKey.ToString())));
                        }
                        else
                        {
                            commandList.Add(string.Format("New-Registry -Path \"{0}\"",
                                ReplaceDoller(targetKey.ToString())));
                        }
                    }

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
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
                {
                    commandList.Add(Dos ?
                        CreateDosCommand(regKey, Name) :
                        CreateSetCommand(regKey, Name));
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

        private string ReplaceDoller(string sourceText)
        {
            return sourceText.Contains("$") ? sourceText.Replace("$", "`$") : sourceText;
        }

        /*
        /// <summary>
        /// レジストリキーをSetコマンドへコンバート
        /// </summary>
        /// <returns></returns>
        private List<string> RegKey_ToSetCommand()
        {
            List<string> commandList = new List<string>();

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
                            case Item.REG_NONE:
                                regValue = "";
                                break;
                        }
                        commandList.Add(string.Format(
                            "Set-Registry -Path \"{0}\" -Name \"{1}\" {2}-Type {3}",
                                ReplaceDoller(targetKey.ToString()),
                                ReplaceDoller(valueName),
                                ReplaceDoller(regValue),
                                RegistryControl.ValueKindToString(valueKind)));

                    }
                }
                else
                {
                    //  レジストリ値設定無し。空レジストリキー作成
                    commandList.Add(string.Format("New-Registry -Path \"{0}\"",
                ReplaceDoller(targetKey.ToString())));
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

            return commandList;
        }
        */

        /*
        /// <summary>
        /// レジストリ値をSetコマンドへコンバート
        /// </summary>
        /// <returns></returns>
        private List<string> RegValue_ToSetCommand()
        {
            List<string> commandList = new List<string>();

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
                    case Item.REG_NONE:
                        regValue = "";
                        break;
                }
                commandList.Add(string.Format(
                    "Set-Registry -Path \"{0}\" -Name \"{1}\" {2}-Type {3}",
                        ReplaceDoller(targetKey.ToString()),
                        ReplaceDoller(Name),
                        ReplaceDoller(regValue),
                        RegistryControl.ValueKindToString(valueKind)));
            }

            return commandList;
        }
        */

        /*
        /// <summary>
        /// レジストリキーをDosコマンドへコンバート
        /// </summary>
        /// <returns></returns>
        private List<string> RegKey_ToDosCommand()
        {
            List<string> commandList = new List<string>();

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
                                regValue = string.Format("\"{0}\"", regValue);
                                break;
                            case Item.REG_DWORD:
                            case Item.REG_QWORD:
                                regValue = string.Format("{0}", regValue);
                                break;
                            case Item.REG_NONE:
                                regValue = "";
                                break;
                        }
                        commandList.Add(string.Format(
                            "reg add \"{0}\" /v \"{1}\" /d {2} /t {3} /f",
                                ReplaceDoller(targetKey.ToString()),
                                ReplaceDoller(valueName),
                                ReplaceDoller(regValue),
                                RegistryControl.ValueKindToString(valueKind)));

                    }
                }
                else
                {
                    //  レジストリ値設定無し。空レジストリキー作成
                    commandList.Add(string.Format("reg add \"{0}\" /ve /f",
                ReplaceDoller(targetKey.ToString())));
                    commandList.Add(string.Format("reg delete \"{0}\" /ve /f",
                        ReplaceDoller(targetKey.ToString())));
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

            return commandList;
        }
        */

        /*
        /// <summary>
        /// レジストリ値をDosコマンドへコンバート
        /// </summary>
        /// <returns></returns>
        private List<string> RegValue_ToDosCommand()
        {
            List<string> commandList = new List<string>();

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
                        regValue = string.Format("\"{0}\"", regValue);
                        break;
                    case Item.REG_DWORD:
                    case Item.REG_QWORD:
                        regValue = string.Format("{0}", regValue);
                        break;
                    case Item.REG_NONE:
                        regValue = "";
                        break;
                }

                commandList.Add(string.Format(
                    "reg add \"{0}\" /v \"{1}\" /d {2} /t {3} /f",
                        ReplaceDoller(targetKey.ToString()),
                        ReplaceDoller(Name),
                        ReplaceDoller(regValue),
                        RegistryControl.ValueKindToString(valueKind)));
            }

            return commandList;
        }
        */

        /// <summary>
        /// レジストリ値をSetコマンドへコンバート
        /// </summary>
        /// <param name="targetKey"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        private string CreateSetCommand(RegistryKey targetKey, string valueName)
        {
            RegistryValueKind valueKind = targetKey.GetValueKind(valueName);
            string regValue = "";
            switch (valueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.MultiString:
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.Binary:
                    regValue = string.Format("\"{0}\"",
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true));
                    break;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    regValue =
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true);
                    break;
                case RegistryValueKind.None:
                default:
                    break;
            }
            return string.Format(
                "Set-Registry -Path \"{0}\" -Name \"{1}\" -Value {2} -Type {3}",
                    ReplaceDoller(targetKey.ToString()),
                    ReplaceDoller(valueName),
                    ReplaceDoller(regValue),
                    RegistryControl.ValueKindToString(valueKind));
        }

        /// <summary>
        /// レジストリ値をDOSコマンドへコンバート
        /// </summary>
        /// <param name="targetKey"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        private string CreateDosCommand(RegistryKey targetKey, string valueName)
        {
            RegistryValueKind valueKind = targetKey.GetValueKind(valueName);
            string regValue = "";
            switch (valueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.MultiString:
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.Binary:
                    regValue = string.Format("\"{0}\"",
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true));
                    break;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    regValue =
                        RegistryControl.RegistryValueToString(targetKey, valueName, valueKind, true);
                    break;
                case RegistryValueKind.None:
                default:
                    break;
            }
            return string.Format(
                "reg add \"{0}\" {1} /d {2} /t {3} /f",
                    targetKey,
                    valueName == "" ? "/ve" : $"/v \"{valueName}\"",
                    regValue,
                    RegistryControl.ValueKindToString(valueKind));
        }




    }
}
