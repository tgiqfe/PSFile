using System;
using Microsoft.Win32;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリへのアクセス権を剥奪
    /// TestGenerator : Test-Registry -Path ～ -Access ""    (ALLの場合)
    ///                 Test-Registry -Path ～ -Account ～   (一部アクセス権を剥奪した場合)
    /// </summary>
    [Cmdlet(VerbsSecurity.Revoke, "Registry")]
    public class RevokeRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        public SwitchParameter All { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                bool isChange = false;
                RegistrySecurity security = security = regKey.GetAccessControl();
                if (All)
                {
                    //  テスト自動生成
                    _generator.RegistryAccess(Path, "", false);

                    foreach (RegistryAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
                else
                {
                    foreach (RegistryAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        string account = rule.IdentityReference.Value;

                        //  テスト自動生成
                        _generator.RegistryAccount(Path, account);

                        if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                            !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }
                    }
                }

                if (isChange) { regKey.SetAccessControl(security); }

                WriteObject(new RegistrySummary(regKey, true));
            }
        }
    }
}
