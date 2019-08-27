using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "Registry")]
    public class NewRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, true, true))
            {
                if (Inherited != Item.NONE || !string.IsNullOrEmpty(Access))
                {
                    RegistrySecurity security = regKey.GetAccessControl();

                    //  上位からのアクセス権継承の設定変更
                    switch (Inherited)
                    {
                        case Item.ENABLE:
                            security.SetAccessRuleProtection(false, false);
                            break;
                        case Item.DISABLE:
                            security.SetAccessRuleProtection(true, true);
                            break;
                        case Item.REMOVE:
                            security.SetAccessRuleProtection(true, false);
                            break;
                    }

                    //  Access文字列からのアクセス権設定
                    if (!string.IsNullOrEmpty(Access))
                    {
                        /*
                        foreach (string ruleString in
                            Access.Contains("/") ? Access.Split('/') : new string[1] { Access })
                        {
                            security.SetAccessRule(RegistryControl.StringToAccessRule(ruleString));
                        }*/
                        foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                        {
                            security.SetAccessRule(rule);
                        }
                    }
                    regKey.SetAccessControl(security);
                }
            }

            //  所有者変更
            if (Owner != null)
            {
                //  埋め込みのsubinacl.exeを展開
                string tempDir = System.IO.Path.Combine(
                    Environment.ExpandEnvironmentVariables("%TEMP%"),
                    "PowerReg");
                string subinacl = System.IO.Path.Combine(tempDir, "subinacl.exe");
                if (!File.Exists(subinacl))
                {
                    EmbeddedResource.Expand(tempDir);
                }

                //  管理者実行確認
                Message.CheckAdmin();

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/subkeyreg \"{Path}\" /owner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
    }
}
