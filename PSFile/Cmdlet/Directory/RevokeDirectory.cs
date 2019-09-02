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
    /// <summary>
    /// フォルダーへのアクセス権や属性を剥奪
    /// </summary>
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
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            bool isChange = false;
            DirectorySecurity security = Directory.GetAccessControl(Path);

            //  アクセス権を剥奪
            if (All)
            {
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    //  テスト自動生成
                    _generator.DirectoryAccount(Path, rule.IdentityReference.Value);

                    security.RemoveAccessRule(rule);
                    isChange = true;
                }
            }
            else if (!string.IsNullOrEmpty(Account))
            {
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    string account = rule.IdentityReference.Value;

                    //  テスト自動生成
                    _generator.DirectoryAccount(Path, account);

                    if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                        !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
            }

            if (isChange) { Directory.SetAccessControl(Path, security); }

            //  フォルダー属性を剥奪
            if (!string.IsNullOrEmpty(_Attributes))
            {
                //  テスト自動生成
                _generator.DirectoryAttributes(Path, _Attributes, true);

                FileAttributes nowAttr = File.GetAttributes(Path);
                FileAttributes delAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                File.SetAttributes(Path, nowAttr & (~delAttr));
            }

            WriteObject(new DirectorySummary(Path, true));
        }
    }
}
