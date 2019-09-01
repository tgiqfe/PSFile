using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using Microsoft.Win32;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Test, "Registry")]
    public class TestRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Name { get; set; }
        [Parameter]
        public string Value { get; set; }
        [Parameter, ValidateSet(Item.REG_SZ, Item.REG_BINARY, Item.REG_DWORD, Item.REG_QWORD, Item.REG_MULTI_SZ, Item.REG_EXPAND_SZ, Item.REG_NONE)]
        public string Type { get; set; } = Item.REG_SZ;
        [Parameter, ValidateSet(Item.PATH, Item.NAME, Item.VALUE, Item.TYPE, Item.OWNER, Item.ACCESS, Item.INHERITED)]
        public string Target { get; set; }
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public bool? Inherited { get; set; }

        

        //  戻り値
        bool retValue = false;

        protected override void BeginProcessing()
        {
            Type = Item.CheckCase(Type);
            Target = Item.CheckCase(Target);
            TestMode = Item.CheckCase(TestMode);

            DetectTargetParameter();
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                //  レジストリキーの有無チェック
                if (regKey == null)
                {
                    //  全条件でキーの有無チェック
                    Console.Error.WriteLine("対象のレジストリキー (Path) 無し： {0}", Path.ToString());
                    return;
                }
                if (Target == Item.PATH)
                {
                    retValue = true;
                    return;
                }

                //  レジストリのパラメータ名/種類/値のチェック (長かったので別のメソッドに)
                if (Target == Item.NAME || Target == Item.TYPE || Target == Item.VALUE)
                {
                    CheckRegValue(regKey);
                    return;
                }

                //  所有者チェック
                RegistrySecurity security = regKey.GetAccessControl();
                if (Target == Item.OWNER)
                {
                    string owner = security.GetOwner(typeof(NTAccount)).Value;
                    retValue = owner == Owner;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("所有者名不一致： {0} / {1}", Owner, owner);
                    }
                    return;
                }

                //  アクセス権チェック
                if (Target == Item.ACCESS)
                {
                    if (TestMode == Item.CONTAIN)
                    {
                        //  Accessパラメータで指定したAccess文字列が、対象のレジストリキーに含まれているかチェック
                        //  Access文字列は複数の場合は、全て対象のレジストリキーに含まれているかをチェック
                        string tempAccess = new RegistrySummary(regKey, false, true).Access;
                        //string[] tempAccessArray = tempAccess.Contains("/") ? tempAccess.Split('/') : new string[1] { tempAccess };
                        string[] tempAccessArray = tempAccess.Split('/');
                        foreach (string accessString in Access.Split('/'))
                        {
                            //retValue = tempAccessArray.Any(x => x.Equals(accessString, StringComparison.OrdinalIgnoreCase));
                            retValue = tempAccessArray.Any(x => RegistryControl.IsMatchAccess(x, accessString));
                            if (!retValue)
                            {
                                Console.Error.WriteLine("指定のアクセス権無し： {0} / {1}", Access, tempAccess);
                                break;
                            }
                        }
                    }
                    else
                    {
                        string tempAccess = new RegistrySummary(regKey, false, true).Access;
                        List<string> accessListA = new List<string>();
                        accessListA.AddRange(tempAccess.Split('/'));

                        List<string> accessListB = new List<string>();
                        accessListB.AddRange(Access.Split('/'));

                        if (accessListA.Count == accessListB.Count)
                        {
                            for (int i = accessListA.Count - 1; i >= 0; i--)
                            {
                                string matchString =
                                    accessListB.FirstOrDefault(x => RegistryControl.IsMatchAccess(x, accessListA[i]));
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

                        //retValue = tempAccess == Access;
                        if (!retValue)
                        {
                            Console.Error.WriteLine("アクセス権不一致： {0} / {1}", Access, tempAccess);
                        }
                    }
                    return;
                }

                //  Inheritedチェック
                if (Target == Item.INHERITED)
                {
                    bool tempInherit = (bool)new RegistrySummary(regKey, false, true).Inherited;
                    retValue = tempInherit == Inherited;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("継承設定不一致： {0} / {1}", Inherited, tempInherit);
                    }
                    return;
                }
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(retValue);
        }

        /// <summary>
        /// Targetパラメータの自動解析
        /// 解析優先度：Value -> Type -> Name -> Owner -> Access -> Inherit -> Path
        /// </summary>
        private void DetectTargetParameter()
        {
            if (Target == null)
            {
                if (Name != null)
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        Target = Item.VALUE;
                    }
                    else if (!string.IsNullOrEmpty(Type))
                    {
                        Target = Item.TYPE;
                    }
                    else
                    {
                        Target = Item.NAME;
                    }
                }
                else if (!string.IsNullOrEmpty(Owner))
                {
                    Target = Item.OWNER;
                }
                else if (Access != null)
                {
                    Target = Item.ACCESS;
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

        /// <summary>
        /// レジストリのパラメータ名/種類/値のチェック
        /// 長かったので独立したメソッドに
        /// </summary>
        /// <param name="regKey">RegistryKeyインスタンス</param>
        private void CheckRegValue(RegistryKey regKey)
        {
            if (Name == null)
            {
                Console.Error.WriteLine("Name無し： {0}", Name);
                return;
            }
            try
            {
                RegistryValueKind valueKind = regKey.GetValueKind(Name);

                //  Name用チェック
                if (Target == Item.NAME)
                {
                    retValue = true;
                    return;
                }

                //  Type用チェック
                if (Target == Item.TYPE)
                {
                    //retValue = valueKind == RegistryControl.StringToValueKind(Type);
                    string tempVlueKind = RegistryControl.ValueKindToString(valueKind);
                    retValue = tempVlueKind == Type;
                    if (!retValue)
                    {
                        Console.Error.WriteLine(
                            "Type不一致： {0} / {1}", Type, tempVlueKind);
                    }
                    return;
                }

                //  Value用チェック
                if (valueKind == RegistryValueKind.Binary) { Value = Value.ToUpper(); }
                retValue = RegistryControl.RegistryValueToString(regKey, Name, valueKind, true) == Value;
                if (!retValue)
                {
                    Console.Error.WriteLine("Value不一致 ({0})： {1}",
                        RegistryControl.ValueKindToString(valueKind), Value);

                    Console.WriteLine(RegistryControl.RegistryValueToString(regKey, Name, valueKind, true));
                }
            }
            catch (IOException)
            {
                //  Name,Type,Valueの条件で名前の有無チェック
                Console.Error.WriteLine("Name無し： {0}", Name);
            }
        }
    }
}
