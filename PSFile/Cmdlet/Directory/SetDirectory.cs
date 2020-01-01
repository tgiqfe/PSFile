using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダーへ各種設定
    /// TestGenerator : Test-Directory -Path ～
    ///                 Test-Directory -Path ～ -Access ～
    ///                 Test-Directory -Path ～ -Owner ～
    ///                 Test-Directory -Path ～ -Inherited ～
    ///                 Test-Directory -Path ～ -Attributes ～
    ///                 Test-Directory -Path ～ -CreationTime ～
    ///                 Test-Directory -Path ～ -LastWriteTime ～
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "Directory")]
    public class SetDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
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
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(DirectoryPath))
            {
                DirectorySecurity security = null;

                //  Access設定
                //  ""で全アクセス権設定を削除
                if (Access != null)
                {
                    if (security == null) { security = Directory.GetAccessControl(DirectoryPath); }

                    //  テスト自動生成
                    _generator.DirectoryAccess(DirectoryPath, Access, false);

                    foreach (FileSystemAccessRule removeRule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        security.RemoveAccessRule(removeRule);
                    }
                    if (Access != string.Empty)     //  このif文分岐が無くても同じ挙動するけれど、一応記述
                    {
                        foreach (FileSystemAccessRule addRule in DirectoryControl.StringToAccessRules(Access))
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
                    _generator.DirectoryOwner(DirectoryPath, Owner);

                    using (Process proc = new Process())
                    {
                        proc.StartInfo.FileName = subinacl;
                        proc.StartInfo.Arguments = $"/subdirectories \"{DirectoryPath}\" /setowner=\"{Owner}\"";
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        proc.Start();
                        proc.WaitForExit();
                    }
                }

                //  Inherited設定
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = Directory.GetAccessControl(DirectoryPath); }

                    //  テスト自動生成
                    _generator.DirectoryInherited(DirectoryPath, Inherited == Item.ENABLE);

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

                if (security != null) { Directory.SetAccessControl(DirectoryPath, security); }

                //  作成日時
                if (CreationTime != null)
                {
                    //  テスト自動生成
                    _generator.DirectoryCreationTime(DirectoryPath, (DateTime)CreationTime);

                    Directory.SetCreationTime(DirectoryPath, (DateTime)CreationTime);
                }

                //  更新一時
                if (LastWriteTime != null)
                {
                    //  テスト自動生成
                    _generator.DirectoryLastWriteTime(DirectoryPath, (DateTime)LastWriteTime);

                    Directory.SetLastWriteTime(DirectoryPath, (DateTime)LastWriteTime);
                }

                //  フォルダー属性
                if (!string.IsNullOrEmpty(_Attributes))
                {
                    //  テスト自動生成
                    _generator.DirectoryAttributes(DirectoryPath, _Attributes, false);

                    if (!_Attributes.Contains(Item.DIRECTORY))
                    {
                        _Attributes += ", " + Item.DIRECTORY;
                    }
                    File.SetAttributes(DirectoryPath, (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes));
                }

                /*  実行していて結構うっとおしいので、出力しないことにします。
                WriteObject(new DirectorySummary(Path, true));
                */
            }
        }
    }
}
