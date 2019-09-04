using System;
using Microsoft.Win32;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリキーにアクセス権を追加
    /// TestGenerator : Test-Registry -Path ～ -Access ～
    ///                 Test-Registry -Path ～ -Inherited ～
    /// </summary>
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
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            AccessControl = Item.CheckCase(AccessControl);
            _Rights = Item.CheckCase(Rights);

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                if (regKey == null) { return; }

                RegistrySecurity security = null;

                //  Account, Rights, AccessControlから追加
                if (!string.IsNullOrEmpty(Account))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }
                    string accessString = string.Format("{0};{1};{2};{3};{4}",
                        Account,
                        _Rights,
                        Recursive ? Item.CONTAINERINHERIT + ", " + Item.OBJECTINHERIT : Item.NONE,
                        Item.NONE,
                        AccessControl);

                    //  テスト自動生成
                    _generator.RegistryAccess(Path, accessString, true);

                    foreach(RegistryAccessRule addRule in RegistryControl.StringToAccessRules(accessString))
                    {
                        security.AddAccessRule(addRule);
                    }

                    /*
                    RegistryAccessRule rule = new RegistryAccessRule(
                        new NTAccount(Account),
                        (RegistryRights)Enum.Parse(typeof(RegistryRights), _Rights),
                        Recursive ?
                            InheritanceFlags.ContainerInherit :
                            InheritanceFlags.None,
                        PropagationFlags.None,
                        (AccessControlType)Enum.Parse(typeof(AccessControlType), AccessControl));
                    security.AddAccessRule(rule);
                    */
                }

                //  Access文字列からの設定
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  テスト自動生成
                    _generator.RegistryAccess(Path, Access, true);

                    foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(rule);
                    }
                }

                //  上位からのアクセス権継承の設定変更
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  テスト自動生成
                    _generator.RegistryInherited(Path, Inherited == Item.ENABLE);

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

                if (security != null) { regKey.SetAccessControl(security); }

                WriteObject(new RegistrySummary(regKey, true));
            }
        }
    }
}
