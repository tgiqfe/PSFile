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
    /// ファイルにアクセス権あるいは属性を追加
    /// TestGenerator : Test-File -Path ～ -Access ～
    ///                 Test-File -Path ～ -Attributes ～
    ///                 Test-Directory -Path ～ -Inherited ～
    /// </summary>
    [Cmdlet(VerbsSecurity.Grant, "File")]
    public class GrantFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter(Position = 1)]
        public string Access { get; set; }
        [Parameter]
        public string Account { get; set; }
        [Parameter, ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public string[] Rights { get; set; } = new string[1] { Item.READANDEXECUTE };
        private string _Rights = null;
        [Parameter, ValidateSet(Item.ALLOW, Item.DENY)]
        public string AccessControl { get; set; } = Item.ALLOW;
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            AccessControl = Item.CheckCase(AccessControl);
            _Rights = Item.CheckCase(Rights);
            _Attributes = Item.CheckCase(Attributes);

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            if (File.Exists(FilePath))
            {
                FileSecurity security = null;

                //  Account, Rights, AccessControlから追加
                if (!string.IsNullOrEmpty(Account))
                {
                    if (security == null) { security = File.GetAccessControl(FilePath); }
                    string accessString = string.Format("{0};{1};{2}",
                        Account,
                        _Rights,
                        AccessControl);

                    //  テスト自動生成
                    _generator.FileAccess(FilePath, accessString, true);

                    foreach (FileSystemAccessRule addRule in FileControl.StringToAccessRules(accessString))
                    {
                        security.AddAccessRule(addRule);
                    }
                }

                //  Access文字列で追加
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = File.GetAccessControl(FilePath); }

                    //  テスト自動生成
                    _generator.FileAccess(FilePath, Access, true);

                    foreach (FileSystemAccessRule addRule in FileControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(addRule);
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

                //  ファイル属性を追加
                if (!string.IsNullOrEmpty(_Attributes))
                {
                    //  テスト自動生成
                    _generator.FileAttributes(FilePath, _Attributes, true);

                    FileAttributes nowAttr = File.GetAttributes(FilePath);
                    FileAttributes addAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                    File.SetAttributes(FilePath, nowAttr | addAttr);
                }

                WriteObject(new FileSummary(FilePath, true));
            }
        }
    }
}
