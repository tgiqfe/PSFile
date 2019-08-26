using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsSecurity.Revoke, "Directory")]
    public class RevokeDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        public SwitchParameter All { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;

        protected override void BeginProcessing()
        {
            _Attributes = Item.CheckCase(Attributes);
        }

        protected override void ProcessRecord()
        {
            DirectorySecurity security = null;

            //  アクセス権を剥奪
            if (All)
            {
                if (security == null) { security = Directory.GetAccessControl(Path); }
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    security.RemoveAccessRule(rule);
                }
            }
            else if (!string.IsNullOrEmpty(Account))
            {
                if (security == null) { security = Directory.GetAccessControl(Path); }
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    string account = rule.IdentityReference.Value;
                    if ((Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase)) ||
                        !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                    {
                        security.RemoveAccessRule(rule);
                    }
                }
            }

            if (security != null) { Directory.SetAccessControl(Path, security); }

            //  フォルダー属性を剥奪
            if (!string.IsNullOrEmpty(_Attributes))
            {
                FileAttributes nowAttr = File.GetAttributes(Path);
                FileAttributes delAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                File.SetAttributes(Path, nowAttr & (~delAttr));
            }

            WriteObject(new DirectorySummary(Path, true));
        }
    }
}
