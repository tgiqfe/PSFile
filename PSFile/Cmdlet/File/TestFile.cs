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
        [ValidateSet(Item.PATH, Item.HASH, Item.ACCESS, Item.OWNER, Item.ATTRIBUTES, Item.INHERITED, Item.SECURITYBLOCK)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Hash { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;
        [Parameter]
        public bool? IsInherited { get; set; }
        [Parameter]
        public bool? SecurityBlock { get; set; }

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
        /// 解析優先度： Hash -> Access -> Owner -> Attributes -> Inherited -> SecurityBlock -> Path
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
                else if (!string.IsNullOrEmpty(Owner))
                {
                    Target = Item.OWNER;
                }
                else if (!string.IsNullOrEmpty(_Attributes))
                {
                    Target = Item.ATTRIBUTES;
                }
                else if (IsInherited != null)
                {
                    Target = Item.INHERITED;
                }
                else if (SecurityBlock != null)
                {
                    Target = Item.SECURITYBLOCK;
                }
                else
                {
                    Target = Item.PATH;
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
            if (Target == Item.PATH)
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
                    foreach (string accessString in Access.Split('/'))
                    {
                        retValue = tempAccessArray.Any(x => x.Equals(accessString, StringComparison.OrdinalIgnoreCase));
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

            //  所有者チェック
            if (Target == Item.OWNER)
            {
                string tempOwner = new FileSummary(Path, false, true, true, true, true, true).Owner;
                retValue = Owner.Contains("\\") && tempOwner.Equals(Owner, StringComparison.OrdinalIgnoreCase) ||
                    !Owner.Contains("\\") && tempOwner.EndsWith("\\" + Owner, StringComparison.OrdinalIgnoreCase);
                if (!retValue)
                {
                    Console.Error.WriteLine("所有者情報不一致： {0} / {1}", Owner, tempOwner);
                }
            }

            //  属性チェック
            if (Target == Item.ATTRIBUTES)
            {
                string tempAttribute = new FileSummary(Path, true, true, true, false, true, true).Attributes;
                if (TestMode == Item.CONTAIN)
                {
                    string[] tempAttribArray = Functions.SplitComma(tempAttribute);
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

            //  継承設定チェック
            if (Target == Item.INHERITED)
            {
                bool tempInherit = (bool)new FileSummary(Path, false, true, true, true, true, true).Inherited;
                retValue = tempInherit == IsInherited;
                if (!retValue)
                {
                    Console.Error.WriteLine("継承設定不一致： {0} / {1}", IsInherited, tempInherit);
                }
                return;
            }

            //  セキュリティブロックチェック
            if (Target == Item.SECURITYBLOCK)
            {
                bool? tempSecurityBlock = new FileSummary(Path, true, true, true, true, true, false).SecurityBlock;
                retValue = tempSecurityBlock == SecurityBlock;
                if (!retValue)
                {
                    Console.Error.WriteLine("セキュリティブロック設定一致： {0} / {1}", SecurityBlock, tempSecurityBlock);
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
