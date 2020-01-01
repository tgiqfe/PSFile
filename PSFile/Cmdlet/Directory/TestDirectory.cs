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
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter]
        [ValidateSet(Item.PATH, Item.ACCESS, Item.ACCOUNT, Item.OWNER, Item.INHERITED, Item.CREATIONTIME, Item.LASTWRITETIME, Item.ATTRIBUTES, Item.SIZE)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Account { get; set; }
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
                if (Access != null)
                {
                    Target = Item.ACCESS;
                }
                else if (!string.IsNullOrEmpty(Account))
                {
                    Target = Item.ACCOUNT;
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
            if (!Directory.Exists(DirectoryPath))
            {
                Console.Error.WriteLine("対象のフォルダー無し： {0}", DirectoryPath);
                return;
            }
            if (Target == Item.PATH)
            {
                retValue = true;
                return;
            }

            //  Accessチェック
            if (Target == Item.ACCESS) { CheckAccess(); return; }

            //  Accountチェック
            if (Target == Item.ACCOUNT) { CheckAccount(); return; }

            //  Owner
            if (Target == Item.OWNER) { CheckOwner(); return; }

            //  CreationTime
            if (Target == Item.CREATIONTIME) { CheckCreationTime(); return; }

            //  LastWriteTime
            if (Target == Item.LASTWRITETIME) { CheckLastWriteTime(); return; }

            //  Attributes
            if (Target == Item.ATTRIBUTES) { CheckAttributes(); return; }

            //  Size
            if (Target == Item.SIZE) { CheckSize(); return; }

            //  Inherited
            if (Target == Item.INHERITED) { CheckInherited(); return; }
        }

        protected override void EndProcessing()
        {
            WriteObject(retValue);
        }

        /// <summary>
        /// Accessチェック
        /// </summary>
        private void CheckAccess()
        {
            string tempAccess = new DirectorySummary(DirectoryPath, false, true, true, true, true, true).Access;
            if (Access == string.Empty)
            {
                retValue = string.IsNullOrEmpty(tempAccess);
                if (!retValue)
                {
                    Console.Error.WriteLine("指定のアクセス権無し： \"{0}\" / \"{1}\"", Access, tempAccess);
                }
            }
            else if (TestMode == Item.CONTAIN)
            {
                //string tempAccess = new DirectorySummary(Path, false, true, true, true, true, true).Access;
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
                //string tempAccess = new DirectorySummary(Path, false, true, true, true, true, true).Access;
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

        /// <summary>
        /// Accountチェック
        /// </summary>
        private void CheckAccount()
        {
            string tempAccess = new DirectorySummary(DirectoryPath, false, true, true, true, true, true).Access;
            foreach (string tempAccessString in tempAccess.Split('/'))
            {
                string tempAccount = tempAccessString.Split(';')[0];
                retValue = Account.Contains("\\") && tempAccount.Equals(Account, StringComparison.OrdinalIgnoreCase) ||
                    !Account.Contains("\\") && tempAccount.EndsWith("\\" + Account, StringComparison.OrdinalIgnoreCase);
                if (retValue)
                {
                    break;
                }
            }
            if (!retValue)
            {
                Console.Error.WriteLine("対象アカウントのアクセス権無し： {0} / {1}", Account, tempAccess);
            }
        }

        /// <summary>
        /// Ownerチェック
        /// </summary>
        private void CheckOwner()
        {
            string tempOwner = new DirectorySummary(DirectoryPath, false, true, true, true, true, true).Owner;
            retValue = Owner.Contains("\\") && tempOwner.Equals(Owner, StringComparison.OrdinalIgnoreCase) ||
                !Owner.Contains("\\") && tempOwner.EndsWith("\\" + Owner, StringComparison.OrdinalIgnoreCase);
            if (!retValue)
            {
                Console.Error.WriteLine("所有者情報不一致： {0} / {1}", Owner, tempOwner);
            }
        }

        /// <summary>
        /// CreationTime
        /// </summary>
        private void CheckCreationTime()
        {
            DateTime tempDate = (DateTime)new DirectorySummary(DirectoryPath, true, false, true, true, true, true).CreationTime;
            retValue = tempDate == CreationTime;
            if (!retValue)
            {
                Console.Error.WriteLine("作成日時不一致： {0} / {1}", CreationTime, tempDate);
            }
        }

        /// <summary>
        /// LastWriteTime
        /// </summary>
        private void CheckLastWriteTime()
        {
            DateTime tempDate = (DateTime)new DirectorySummary(DirectoryPath, true, false, true, true, true, true).LastWriteTime;
            retValue = tempDate == LastWriteTime;
            if (!retValue)
            {
                Console.Error.WriteLine("更新日時不一致： {0} / {1}", LastWriteTime, tempDate);
            }
        }

        /// <summary>
        /// Attributesチェック
        /// </summary>
        private void CheckAttributes()
        {
            string tempAttribute = new DirectorySummary(DirectoryPath, true, true, false, true, true, true).Attributes;
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
        }

        /// <summary>
        /// Sizeチェック
        /// </summary>
        private void CheckSize()
        {
            long tempSize = (long)new DirectorySummary(DirectoryPath, true, true, true, false, true, true).Size;
            retValue = tempSize == Size;
            if (!retValue)
            {
                Console.Error.WriteLine("サイズ不一致： {0} / {1}", Size, tempSize);
            }
        }

        /// <summary>
        /// Inheritedチェック
        /// </summary>
        private void CheckInherited()
        {
            bool tempInherit = (bool)new DirectorySummary(DirectoryPath, false, true, true, true, true, true).Inherited;
            retValue = tempInherit == Inherited;
            if (!retValue)
            {
                Console.Error.WriteLine("継承設定不一致： {0} / {1}", Inherited, tempInherit);
            }
        }
    }
}
