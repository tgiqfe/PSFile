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
        [ValidateSet(Item.PATH, Item.ACCESS, Item.OWNER, Item.INHERITED, Item.CREATIONTIME, Item.LASTWRITETIME, Item.ATTRIBUTES, Item.SIZE)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        public DateTime? CreationTime { get; set; }
        [Parameter]
        public DateTime? LastWriteTime { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;
        [Parameter]
        public long? Size { get; set; }
        [Parameter]
        public bool? Inherited { get; set; }

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
        /// 解析優先度：Access -> Owner -> CreationTime -> LastWriteTime -> Attributes -> Size -> Inherited -> Path
        /// </summary>
        private void DetectTargetParameter()
        {
            if (Target == null)
            {
                if (!string.IsNullOrEmpty(Access))
                {
                    Target = Item.ACCESS;
                }
                else if (!string.IsNullOrEmpty(Owner))
                {
                    Target = Item.OWNER;
                }
                else if (CreationTime != null)
                {
                    Target = Item.CREATIONTIME;
                }
                else if (LastWriteTime != null)
                {
                    Target = Item.LASTWRITETIME;
                }
                else if (!string.IsNullOrEmpty(_Attributes))
                {
                    Target = Item.ATTRIBUTES;
                }
                else if (Size != null)
                {
                    Target = Item.SIZE;
                }
                else if (Inherited != null)
                {
                    Target = Item.INHERITED;
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

            //  Accessチェック
            if (Target == Item.ACCESS)
            {
                if (TestMode == Item.CONTAIN)
                {
                    string tempAccess = new DirectorySummary(Path, false, true, true, true, true, true).Access;
                    string[] tempAccessArray = tempAccess.Split('/');
                    foreach (string accessString in Access.Split('/'))
                    {
                        retValue = tempAccessArray.Any(x => DirectoryControl.IsMatchAccess(x, accessString));
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
                    List<string> accessListA = new List<string>();
                    accessListA.AddRange(tempAccess.Split('/'));

                    List<string> accessListB = new List<string>();
                    accessListB.AddRange(Access.Split('/'));

                    if (accessListA.Count == accessListB.Count)
                    {
                        for (int i = accessListA.Count - 1; i >= 0; i--)
                        {
                            string matchString =
                                accessListB.FirstOrDefault(x => DirectoryControl.IsMatchAccess(x, accessListA[i]));
                            if (matchString != null)
                            {
                                accessListB.Remove(matchString);
                            }
                        }
                        retValue = accessListB.Count == 0;
                    }
                    else
                    {
                        retValue = false;
                    }

                    if (!retValue)
                    {
                        Console.Error.WriteLine("アクセス権不一致： {0} / {1}", Access, tempAccess);
                    }
                }
            }

            //  Owner
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

            //  CreationTime
            if (Target == Item.CREATIONTIME)
            {
                DateTime tempDate = (DateTime)new DirectorySummary(Path, true, false, true, true, true, true).CreationTime;
                retValue = tempDate == CreationTime;
                if (!retValue)
                {
                    Console.Error.WriteLine("作成日時不一致： {0} / {1}", CreationTime, tempDate);
                }
            }

            //  LastWriteTime
            if (Target == Item.LASTWRITETIME)
            {
                DateTime tempDate = (DateTime)new DirectorySummary(Path, true, false, true, true, true, true).LastWriteTime;
                retValue = tempDate == LastWriteTime;
                if (!retValue)
                {
                    Console.Error.WriteLine("更新日時不一致： {0} / {1}", LastWriteTime, tempDate);
                }
            }

            //  Attributes
            if (Target == Item.ATTRIBUTES)
            {
                string tempAttribute = new DirectorySummary(Path, true, true, false, true, true, true).Attributes;
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

            //  Size
            if (Target == Item.SIZE)
            {
                long tempSize = (long)new DirectorySummary(Path, true, true, true, false, true, true).Size;
                retValue = tempSize == Size;
                if (!retValue)
                {
                    Console.Error.WriteLine("サイズ不一致： {0} / {1}", Size, tempSize);
                }
            }

            //  Inherited
            if (Target == Item.INHERITED)
            {
                bool tempInherit = (bool)new DirectorySummary(Path, false, true, true, true, true, true).Inherited;
                retValue = tempInherit == Inherited;
                if (!retValue)
                {
                    Console.Error.WriteLine("継承設定不一致： {0} / {1}", Inherited, tempInherit);
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
