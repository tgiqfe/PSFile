using Microsoft.Win32;
using System;
using System.IO;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリ情報を取得してRegistrySummaryインスタンスを返す
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Registry")]
    public class GetRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string RegistryPath { get; set; }
        [Parameter(Position = 1)]
        public string Name { get; set; }

        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreValues { get; set; }

        [Parameter]
        public SwitchParameter NoResolv { get; set; }
        [Parameter]
        public SwitchParameter RawValue { get; set; }

        protected override void ProcessRecord()
        {
            if (Name == null)
            {
                //  レジストリキーの取得
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
                {
                    WriteObject(new RegistrySummary(regKey, IgnoreSecurity, IgnoreValues));
                }
            }
            else
            {
                //  レジストリ値の取得
                using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
                {
                    RegistryValueKind valueKind = regKey.GetValueKind(Name);
                    if (RawValue)
                    {
                        switch (valueKind)
                        {
                            case RegistryValueKind.ExpandString:
                                WriteObject(NoResolv ?
                                    regKey.GetValue(Name, "", RegistryValueOptions.DoNotExpandEnvironmentNames) :
                                    regKey.GetValue(Name));
                                break;
                            case RegistryValueKind.None:
                                WriteObject(null);
                                break;
                            default:
                                WriteObject(regKey.GetValue(Name));
                                break;
                        }
                    }
                    else
                    {
                        WriteObject(RegistryControl.RegistryValueToString(regKey, Name, valueKind, NoResolv));
                    }
                }
            }
        }
    }
}
