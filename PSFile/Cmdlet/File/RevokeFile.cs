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
    /// <summary>
    /// ファイルへのアクセス権や属性を剥奪
    /// TestGenerator : Test-File -Path ～ -Access ""    (ALLの場合)
    ///                 Test-File -Path ～ -Account ～   (一部アクセス権を剥奪した場合)
    ///                 Test-File -Path ～ -Attributes ～ (属性を剥奪した場合)
    /// </summary>
    [Cmdlet(VerbsSecurity.Revoke, "File")]
    public class RevokeFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        public SwitchParameter All { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;
        [Parameter]
        public SwitchParameter RemoveSecurityBlock { get; set; }
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
            FileSecurity security = File.GetAccessControl(FilePath);

            //  アクセス権を剥奪
            if (All)
            {
                //  テスト自動生成
                _generator.FileAccess(FilePath, "", false);

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
                    _generator.FileAccount(FilePath, account);

                    if (Account.Contains("\\") && account.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                        !Account.Contains("\\") && account.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
            }

            if (isChange) { File.SetAccessControl(FilePath, security); }

            //  ファイル属性を剥奪
            if (!string.IsNullOrEmpty(_Attributes))
            {
                //  テスト自動生成
                _generator.FileAttributes(FilePath, _Attributes, true);

                FileAttributes nowAttr = File.GetAttributes(FilePath);
                FileAttributes delAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                File.SetAttributes(FilePath, nowAttr & (~delAttr));
            }

            //  セキュリティブロックの解除
            if (RemoveSecurityBlock)
            {
                //  テスト自動生成
                _generator.FileSecurityBlock(FilePath, false);

                FileControl.RemoveSecurityBlock(FilePath);
            }

            WriteObject(new FileSummary(FilePath, true));
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
