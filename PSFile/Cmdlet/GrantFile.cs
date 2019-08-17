using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Management.Automation;

namespace PSFile
{
    [Cmdlet(VerbsSecurity.Grant, "File")]
    public class GrantFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
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
        [ValidateSet(Item.ALLOW, Item.DENY)]
        public string AccessControl { get; set; } = Item.ALLOW;
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            AccessControl = Item.CheckCase(AccessControl);
            _Rights = Item.CheckCase(Rights);
            _Attributes = Item.CheckCase(Attributes);
        }

        protected override void ProcessRecord()
        {
            if (File.Exists(Path))
            {
                FileSecurity security = null;

                //  Account, Rights, AccessControlから追加
                if (!string.IsNullOrEmpty(Account))
                {
                    if (security == null) { security = File.GetAccessControl(Path); }
                    foreach (FileSystemAccessRule addRule in
                        FileControl.StringToAccessRules(string.Format("{0};{1};{2}", Account, _Rights, AccessControl)))
                    {
                        security.AddAccessRule(addRule);
                    }
                }

                //  Access文字列で追加
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = File.GetAccessControl(Path); }
                    foreach (FileSystemAccessRule addRule in FileControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(addRule);
                    }
                }

                //  Inherited設定
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = File.GetAccessControl(Path); }
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

                if (security != null) { File.SetAccessControl(Path, security); }

                //  ファイル属性を追加
                if (!string.IsNullOrEmpty(_Attributes))
                {
                    FileAttributes nowAttr = File.GetAttributes(Path);
                    FileAttributes addAttr = (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes);
                    File.SetAttributes(Path, nowAttr | addAttr);

                    /*
                    FileAttributes nowAttr = File.GetAttributes(Path);
                    string[] nowAttrArray = GlobalParam.reg_Delimitor.Split(nowAttr.ToString());
                    string[] addAttrArray = GlobalParam.reg_Delimitor.Split(_Attributes);
                    foreach (string addAttr in addAttrArray)
                    {
                        if (!nowAttrArray.Any(x => x == addAttr))
                        {
                            File.SetAttributes(Path, nowAttr | (FileAttributes)Enum.Parse(typeof(FileAttributes), addAttr));
                        }
                    }
                    */
                }

                WriteObject(new FileSummary(Path, true));
            }
        }
    }
}
