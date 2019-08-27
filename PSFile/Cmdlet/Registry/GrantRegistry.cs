using System;
using Microsoft.Win32;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsSecurity.Grant, "Registry")]
    public class GrantRegistry :PSCmdlet
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
            bool isChange = false;

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                if (regKey == null) { return; }

                RegistrySecurity security = regKey.GetAccessControl();

                //  アクセス権設定
                if (!string.IsNullOrEmpty(Account))
                {
                    RegistryAccessRule rule = new RegistryAccessRule(
                        new NTAccount(Account),
                        (RegistryRights)Enum.Parse(typeof(RegistryRights), _Rights),
                        Recursive ?
                            InheritanceFlags.ContainerInherit :
                            InheritanceFlags.None,
                        PropagationFlags.None,
                        (AccessControlType)Enum.Parse(typeof(AccessControlType), AccessControl));

                    security.SetAccessRule(rule);
                    isChange = true;
                }

                //  Access文字列からの設定
                if (!string.IsNullOrEmpty(Access))
                {
                    /*
                    foreach (string ruleString in
                        Access.Contains("/") ? Access.Split('/') : new string[1] { Access })
                    {
                        security.SetAccessRule(RegistryControl.StringToAccessRule(ruleString));
                        isChange = true;
                    }
                    */
                    foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                    {
                        security.SetAccessRule(rule);
                        isChange = true;
                    }
                }

                //  上位からのアクセス権継承の設定変更
                switch (Inherited)
                {
                    case Item.ENABLE:
                        security.SetAccessRuleProtection(false, false);
                        isChange = true;
                        break;
                    case Item.DISABLE:
                        security.SetAccessRuleProtection(true, true);
                        isChange = true;
                        break;
                    case Item.REMOVE:
                        security.SetAccessRuleProtection(true, false);
                        isChange = true;
                        break;
                }

                if (isChange)
                {
                    regKey.SetAccessControl(security);
                }
            }

            WriteObject(new RegistrySummary(Path, true));
        }
    }
}
