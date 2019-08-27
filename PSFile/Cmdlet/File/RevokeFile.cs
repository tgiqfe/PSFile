using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Management.Automation;
using System.IO;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsSecurity.Revoke, "File")]
    public class RevokeFile : PSCmdlet
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
        [Parameter]
        public SwitchParameter RemoveSecurityBlock { get; set; }

        protected override void BeginProcessing()
        {
            _Attributes = Item.CheckCase(Attributes);
        }

        protected override void ProcessRecord()
        {
            bool isChange = false;
            FileSecurity security = File.GetAccessControl(Path);

            //  アクセス権を剥奪
            if (All)
            {
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    security.RemoveAccessRule(rule);
                    isChange = true;
                }
            }
            else if (!string.IsNullOrEmpty(Account))
            {
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    string account = rule.IdentityReference.Value;
                    if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                        !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
            }

            if (isChange) { File.SetAccessControl(Path, security); }

            //  ファイル属性を剥奪
            if (!string.IsNullOrEmpty(_Attributes))
            {
                FileAttributes nowAttr = File.GetAttributes(Path);
                FileAttributes delAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                File.SetAttributes(Path, nowAttr & (~delAttr));
            }

            //  セキュリティブロックの解除
            if (RemoveSecurityBlock)
            {
                FileControl.RemoveSecurityBlock(Path);
            }

            WriteObject(new FileSummary(Path, true));
        }
    }
}
