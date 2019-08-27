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
            bool isChange = false;
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                RegistrySecurity security = regKey.GetAccessControl();
                AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                if (All)
                {
                    foreach (RegistryAccessRule rule in rules)
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
                else
                {
                    foreach (RegistryAccessRule rule in rules)
                    {
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
                    }
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
