using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// ファイルへ各種設定
    /// TestGenerator : Test-File -Path ～
    ///                 Test-File -Path ～ -Access ～
    ///                 Test-File -Path ～ -Owner ～
    ///                 Test-File -Path ～ -Inherited ～
    ///                 Test-File -Path ～ -Attributes ～
    ///                 Test-File -Path ～ -CreationTime ～
    ///                 Test-File -Path ～ -LastWriteTime ～
    ///                 Test-File -Path ～ -SecurityBlock ～
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "File")]
    public class SetFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public DateTime? CreationTime { get; set; }
        [Parameter]
        public DateTime? LastWriteTime { get; set; }
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
            Inherited = Item.CheckCase(Inherited);
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);

            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (File.Exists(FilePath))
            {
                SetFileProcess();
            }
            //  ファイルｐアスのワイルドカード対応予定
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }

        private void SetFileProcess()
        {
            FileSecurity security = null;

            //  Access設定
            //  ""で全アクセス権設定を削除
            if (Access != null)
            {
                if (security == null) { security = File.GetAccessControl(FilePath); }

                //  テスト自動生成
                _generator.FileAccess(FilePath, Access, false);

                foreach (FileSystemAccessRule removeRule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    security.RemoveAccessRule(removeRule);
                }
                if (Access != string.Empty)     //  このif文分岐が無くても同じ挙動するけれど、一応記述
                {
                    foreach (FileSystemAccessRule addRule in FileControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(addRule);
                    }
                }
            }

            //  Owner設定
            if (!string.IsNullOrEmpty(Owner))
            {
                //  埋め込みのsubinacl.exeを展開
                string subinacl = EmbeddedResource.GetSubinacl(Item.APPLICATION_NAME);

                //  管理者実行確認
                Functions.CheckAdmin();

                //  テスト自動生成
                _generator.FileOwner(FilePath, Owner);

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/file \"{FilePath}\" /setowner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            //  Inherited設定
            if (Inherited != Item.NONE)
            {
                if (security == null) { security = File.GetAccessControl(FilePath); }

                //  テスト自動生成
                _generator.FileInherited(FilePath, Inherited == Item.ENABLE);

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

            if (security != null) { File.SetAccessControl(FilePath, security); }

            //  作成日時
            if (CreationTime != null)
            {
                //  テスト自動生成
                _generator.FileCreationTime(FilePath, (DateTime)CreationTime);

                File.SetCreationTime(FilePath, (DateTime)CreationTime);
            }

            //  更新一時
            if (LastWriteTime != null)
            {
                //  テスト自動生成
                _generator.FileLastWriteTime(FilePath, (DateTime)LastWriteTime);

                File.SetLastWriteTime(FilePath, (DateTime)LastWriteTime);
            }

            //  ファイル属性
            //if (!string.IsNullOrEmpty(Attributes))
            if (!string.IsNullOrEmpty(_Attributes))
            {
                //  テスト自動生成
                _generator.FileAttributes(FilePath, _Attributes, false);

                File.SetAttributes(FilePath, (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes));
            }

            //  セキュリティブロックの解除
            if (RemoveSecurityBlock)
            {
                //  テスト自動生成
                _generator.FileSecurityBlock(FilePath, false);

                FileControl.RemoveSecurityBlock(FilePath);
            }

            /*  実行していて結構うっとおしいので、出力しないことにします。
            WriteObject(new FileSummary(Path, true));
            */
        }
    }
}
