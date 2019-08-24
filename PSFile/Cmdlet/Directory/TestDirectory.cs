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
    [Cmdlet(VerbsDiagnostic.Test, "Directory")]
    public class TestDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        [ValidateSet(Item.PATH, Item.ACCESS, Item.OWNER, Item.ATTRIBUTE)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;

        //  戻り値
        bool retValue = false;

        protected override void BeginProcessing()
        {
            Target = Item.CheckCase(Target);
            TestMode = Item.CheckCase(TestMode);
            _Attributes = Item.CheckCase(Attributes);

            DetectTargetParameter();
        }

        /// <summary>
        /// Targetパラメータの自動解析
        /// 解析優先度：Access -> Owner -> Attributes -> Path
        /// </summary>
        private void DetectTargetParameter()
        {
            if(Target == null)
            {
                if (!string.IsNullOrEmpty(Access))
                {
                    Target = Item.ACCESS;
                }
                else if (!string.IsNullOrEmpty(Owner))
                {
                    Target = Item.OWNER;
                }
                else if (!string.IsNullOrEmpty(_Attributes))
                {
                    Target = Item.ATTRIBUTE;
                }
                else
                {
                    Target = Item.PATH;
                }
            }
        }

        protected override void ProcessRecord()
        {
            //  フォルダーの有無チェック
            if (!Directory.Exists(Path))
            {
                //  全条件でフォルダーの有無チェック
                Console.Error.WriteLine("対象のフォルダー無し： {0}", Path);
                return;
            }
            if (Target == Item.PATH)
            {
                retValue = true;
            }

            //  アクセス権チェック
            if(Target == Item.ACCESS)
            {
                if (TestMode == Item.CONTAIN)
                {
                    string tempAccess = new DirectorySummary(Path, false, true, true, true, true, true).Access;
                    string[] tempAccessArray = tempAccess.Split('/');
                    foreach(string ruleString in Access.Split('/'))
                    {
                        retValue = tempAccessArray.Any(x => x.Equals(ruleString, StringComparison.OrdinalIgnoreCase));
                        if (!retValue)
                        {
                            Console.Error.WriteLine("指定のアクセス権無し： {0} / {1}", Access, tempAccess);
                            break;
                        }
                    }
                }
                else
                {
                    string tempAccess = new DirectorySummary(Path, false, true, true, true, true, true).Access;
                    retValue = tempAccess == Access;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("アクセス権不一致： {0} / {1}", Access, tempAccess);
                    }
                }
            }

            //  所有者チェック
            if (Target == Item.OWNER)
            {
                string tempOwner = new DirectorySummary(Path, false, true, true, true, true, true).Owner;
                retValue = Owner.Contains("\\") && tempOwner.Equals(Owner, StringComparison.OrdinalIgnoreCase) ||
                    !Owner.Contains("\\") && tempOwner.EndsWith("\\" + Owner, StringComparison.OrdinalIgnoreCase);
                if (!retValue)
                {
                    Console.Error.WriteLine("所有者情報不一致： {0} / {1}", Owner, tempOwner);
                }
            }

            //  属性チェック
            if (Target == Item.ATTRIBUTE)
            {
                string tempAttribute = new DirectorySummary(Path, true, true, false, true, true, true).Attributes;
                if (TestMode == Item.CONTAIN)
                {
                    string[] tempAttribArray = GlobalParam.reg_Delimitor.Split(tempAttribute);

                    foreach (string attribString in
                        _Attributes.Split(new string[1] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        retValue = tempAttribArray.Any(x => x == attribString);
                        if (!retValue)
                        {
                            Console.Error.WriteLine("属性不一致： {0} / {1}", _Attributes, tempAttribute);
                            break;
                        }
                    }
                }
                else
                {
                    retValue = tempAttribute == _Attributes;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("属性不一致： {0} / {1}", _Attributes, tempAttribute);
                    }
                }
                return;
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(retValue);
        }
    }
}
