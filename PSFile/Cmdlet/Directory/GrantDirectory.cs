using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダーにアクセス権あるいは属性を追加
    /// TestGenerator : Test-Directory -Path ～ -Access ～
    ///                 Test-Directory -Path ～ -Attributes ～
    ///                 Test-Directory -Path ～ -Inherited ～
    /// </summary>
    [Cmdlet(VerbsSecurity.Grant, "Directory")]
    public class GrantDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter(Position = 1)]
        public string Access { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public string[] Rights { get; set; } = new string[1] { Item.READANDEXECUTE };
        private string _Rights = null;
        [Parameter]
        public SwitchParameter Recursive { get; set; }
        [Parameter]
        [ValidateSet(Item.ALLOW, Item.DENY)]
        public string AccessControl { get; set; } = Item.ALLOW;
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            AccessControl = Item.CheckCase(AccessControl);
            _Rights = Item.CheckCase(Rights);
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);

            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(DirectoryPath))
            {
                DirectorySecurity security = null;

                //  Account, Rights, AccessControlから追加
                if (!string.IsNullOrEmpty(Account))
                {
                    if (security == null) { security = Directory.GetAccessControl(DirectoryPath); }
                    string accessString = string.Format("{0};{1};{2};{3};{4}",
                        Account,
                        _Rights,
                        Recursive ? Item.CONTAINERINHERIT + ", " + Item.OBJECTINHERIT : Item.NONE,
                        Item.NONE,
                        AccessControl);
                    
                    //  テスト自動生成
                    _generator.DirectoryAccess(DirectoryPath, accessString, true);

                    foreach (FileSystemAccessRule addRule in DirectoryControl.StringToAccessRules(accessString))
                    {
                        security.AddAccessRule(addRule);
                    }
                }

                //  Access文字列で追加
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = Directory.GetAccessControl(DirectoryPath); }

                    //  テスト自動生成
                    _generator.DirectoryAccess(DirectoryPath, Access, true);

                    foreach (FileSystemAccessRule addRule in DirectoryControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(addRule);
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

                //  フォルダー属性を追加
                if (!string.IsNullOrEmpty(_Attributes))
                {
                    //  テスト自動生成
                    _generator.DirectoryAttributes(DirectoryPath, _Attributes, true);

                    FileAttributes nowAttr = File.GetAttributes(DirectoryPath);
                    FileAttributes addAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                    File.SetAttributes(DirectoryPath, nowAttr | addAttr);
                }

                WriteObject(new DirectorySummary(DirectoryPath, true));
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
