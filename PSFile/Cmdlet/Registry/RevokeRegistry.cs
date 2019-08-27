using System;
using Microsoft.Win32;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsSecurity.Revoke, "Registry")]
    public class RevokeRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        public SwitchParameter All { get; set; }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                bool isChange = false;
                //RegistrySecurity security = regKey.GetAccessControl();
                RegistrySecurity security = security = regKey.GetAccessControl();
                //AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                if (All)
                {
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
                        if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                            !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }

                        /*
                        if (Account.Contains("\\") &&
                            rule.IdentityReference.Value.Equals(Account, StringComparison.OrdinalIgnoreCase))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }
                        else if (!Account.Contains("\\") &&
                             rule.IdentityReference.Value.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }
                        */
                    }
                }

                if (isChange) { regKey.SetAccessControl(security); }
            }
            WriteObject(new RegistrySummary(Path, true));
        }
    }
}
