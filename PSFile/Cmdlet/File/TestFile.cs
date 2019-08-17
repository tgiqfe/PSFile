using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Test, "File")]
    public class TestFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        [ValidateSet(Item.NAME, Item.HASH, Item.ACCESS, Item.ATTRIBUTE, Item.SECURITYBLOCK)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Hash { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Attributes { get; set; }
        [Parameter]
        public bool? SecurityBlock { get; set; }

        //  戻り値
        bool retValue = false;

        protected override void BeginProcessing()
        {
            Target = Item.CheckCase(Target);
            TestMode = Item.CheckCase(TestMode);

            DetectTargetParameter();
        }

        /// <summary>
        /// Targetパラエメータの自動解析
        /// 解析優先度： Hash -> Access -> Attributes -> SecurityBlock -> Name
        /// </summary>
        private void DetectTargetParameter()
        {
            if (Target == null)
            {
                if (!string.IsNullOrEmpty(Hash))
                {
                    Target = Item.HASH;
                }
                else if (!string.IsNullOrEmpty(Access))
                {
                    Target = Item.ACCESS;
                }
                else if (!string.IsNullOrEmpty(Attributes))
                {
                    Target = Item.ATTRIBUTE;
                }
                else if (SecurityBlock != null)
                {
                    Target = Item.SECURITYBLOCK;
                }
                else
                {
                    Target = Item.NAME;
                }
            }
        }

        protected override void ProcessRecord()
        {
            //  ファイルの有無チェック
            if (!File.Exists(Path))
            {
                //  全条件でファイルの有無チェック
                Console.Error.WriteLine("対象のファイル無し： {0}", Path);
                return;
            }
            if (Target == Item.NAME)
            {
                retValue = true;
                return;
            }

            //  Hashチェック
            if (Target == Item.HASH)
            {
                string hashString = new FileSummary(Path, true, true, false, true, true, true).Hash;
                retValue = hashString == Hash;
                if (!retValue)
                {
                    Console.Error.WriteLine("ハッシュ不一致： {0} / {1}", Hash, hashString);
                }
                return;
            }

            //  アクセス権チェック
            if (Target == Item.ACCESS)
            {
                if (TestMode == Item.CONTAIN)
                {
                    string tempAccess = new FileSummary(Path, false, true, true, true, true, true).Access;
                    string[] tempAccessArray = tempAccess.Split('/');
                    foreach (string ruleString in Access.Split('/'))
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
                    string tempAccess = new FileSummary(Path, false, true, true, true, true, true).Access;
                    retValue = tempAccess == Access;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("アクセス権不一致： {0} / {1}", Access, tempAccess);
                    }
                }
                return;
            }

            //  属性チェック
            if (Target == Item.ATTRIBUTE)
            {
                if (TestMode == Item.CONTAIN)
                {
                    string tempAttribute = new FileSummary(Path, true, true, true, false, true, true).Attributes;
                    string[] tempAttribArray = GlobalParam.reg_Delimitor.Split(tempAttribute);

                    foreach (string attribString in
                        Attributes.Split(new string[1] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        retValue = tempAttribArray.Any(x => x == attribString);
                        if (!retValue)
                        {
                            Console.Error.WriteLine("属性不一致： {0} / {1}", Attributes, tempAttribute);
                            break;
                        }
                    }
                }
                else
                {
                    string tempAttrib = new FileSummary(Path, true, true, true, false, true, true).Attributes;
                    retValue = tempAttrib == Attributes;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("属性不一致： {0} / {1}", Attributes, tempAttrib);
                    }
                }
                return;
            }

            //  セキュリティブロックチェック
            if (Target == Item.SECURITYBLOCK)
            {
                bool? tempSecurityBlock = new FileSummary(Path, true, true, true, true, true, false).IsSecurityBlock;
                retValue = tempSecurityBlock == SecurityBlock;
                if (!retValue)
                {
                    Console.Error.WriteLine("セキュリティブロック設定一致： {0} / {1}", SecurityBlock, tempSecurityBlock);
                }
                return;
            }
        }
    }
}
