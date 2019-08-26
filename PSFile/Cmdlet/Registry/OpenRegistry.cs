using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Open, "Registry")]
    public class OpenRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string File { get; set; }

        protected override void ProcessRecord()
        {
            //  管理者実行確認
            Message.CheckAdmin();

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if (regKey != null) { return; }
            }
            string keyName = Path.Substring(Path.IndexOf("\\") + 1);
            RegistryHive.Load(keyName, File);

            //  ロード成功確認
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if (regKey != null)
                {
                    WriteObject(new RegistrySummary(Path));
                    return;
                }
            }

            //  ロード失敗時の再ロード用コマンド
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"load \"{Path}\" \"{File}\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if (regKey != null)
                {
                    WriteObject(new RegistrySummary(Path));
                }
            }
        }
    }
}
