using System;
using Microsoft.Win32;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsSecurity.Grant, "Registry")]
    public class GrantRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Position = 1)]
        public string Access { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public string[] Rights { get; set; } = new string[1] { Item.READKEY };
        private string _Rights = null;
        [Parameter]
        public SwitchParameter Recursive { get; set; }
        [Parameter]
        [ValidateSet(Item.ALLOW, Item.DENY)]
        public string AccessControl { get; set; } = Item.ALLOW;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            AccessControl = Item.CheckCase(AccessControl);
            _Rights = Item.CheckCase(Rights);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                if (regKey == null) { return; }

                //RegistrySecurity security = regKey.GetAccessControl();
                RegistrySecurity security = null;

                //  アクセス権設定
                if (!string.IsNullOrEmpty(Account))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    RegistryAccessRule rule = new RegistryAccessRule(
                        new NTAccount(Account),
                        (RegistryRights)Enum.Parse(typeof(RegistryRights), _Rights),
                        Recursive ?
                            InheritanceFlags.ContainerInherit :
                            InheritanceFlags.None,
                        PropagationFlags.None,
                        (AccessControlType)Enum.Parse(typeof(AccessControlType), AccessControl));

                    security.SetAccessRule(rule);
                }

                //  Access文字列からの設定
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  Grantでアクセス権追加なのに、Setで上書きしてしまっていませんか?

                    foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                    {
                        security.SetAccessRule(rule);
                    }
                }

                //  上位からのアクセス権継承の設定変更
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }
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
                }

                if (security == null) { regKey.SetAccessControl(security); }

                WriteObject(new RegistrySummary(regKey, true));
            }
        }
    }
}
