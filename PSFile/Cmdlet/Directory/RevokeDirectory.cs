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
    /// TestGenerator : Test-Directory -Path ～ -Access ""    (ALLの場合)
    ///                 Test-Directory -Path ～ -Account ～   (一部アクセス権を剥奪した場合)
    ///                 Test-Directory -Path ～ -Attributes ～ (属性を剥奪した場合)
    /// </summary>
    [Cmdlet(VerbsSecurity.Revoke, "Directory")]
    public class RevokeDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
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

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);

            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            bool isChange = false;
            DirectorySecurity security = Directory.GetAccessControl(DirectoryPath);

            //  アクセス権を剥奪
            if (All)
            {
                //  テスト自動生成
                _generator.DirectoryAccess(DirectoryPath, "", false);

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

                    //  テスト自動生成
                    _generator.DirectoryAccount(DirectoryPath, account);

                    if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                        !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
            }

            if (isChange) { Directory.SetAccessControl(DirectoryPath, security); }

            //  フォルダー属性を剥奪
            if (!string.IsNullOrEmpty(_Attributes))
            {
                //  テスト自動生成
                _generator.DirectoryAttributes(DirectoryPath, _Attributes, true);

                FileAttributes nowAttr = File.GetAttributes(DirectoryPath);
                FileAttributes delAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                File.SetAttributes(DirectoryPath, nowAttr & (~delAttr));
            }

            WriteObject(new DirectorySummary(DirectoryPath, true));
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
