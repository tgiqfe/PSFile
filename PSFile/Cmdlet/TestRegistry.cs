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
        [Parameter]
        [ValidateSet(Item.REG_SZ, Item.REG_BINARY, Item.REG_DWORD, Item.REG_QWORD, Item.REG_MULTI_SZ, Item.REG_EXPAND_SZ, Item.REG_NONE)]
        public string Type { get; set; } = Item.REG_SZ;
        [Parameter]
        [ValidateSet(Item.PATH, Item.NAME, Item.VALUE, Item.TYPE, Item.OWNER, Item.ACCESS, Item.INHERIT)]
        public string Target { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public bool? IsInherit { get; set; }

        /// <summary>
        /// いくつかのテスト項目で、モード切替が必要な場合の為のパラメータ
        /// </summary>
        [Parameter]
        [ValidateSet(Item.CONTAIN, Item.MATCH)]
        public string TestMode { get; set; }

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
                        //string tempAccess = RegistryControl.AccessToString(regKey);
                        string[] tempAccessArray = tempAccess.Contains("/") ? tempAccess.Split('/') : new string[1] { tempAccess };
                        foreach (string ruleString in
                            Access.Contains("/") ? Access.Split('/') : new string[1] { Access })
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
                        string tempAccess = new RegistrySummary(regKey, false, true).Access;
                        //string access = RegistryControl.AccessToString(regKey);
                        retValue = tempAccess == Access;
                        if (!retValue)
                        {
                            Console.Error.WriteLine("アクセス権不一致： {0} / {1}", Access, tempAccess);
                        }
                    }
                    return;
                }

                //  継承設定チェック
                if (Target == Item.INHERIT)
                {
                    //retValue = !security.AreAccessRulesProtected == IsInherit;
                    bool tempInherit = new RegistrySummary(regKey, false, true).Inherited;
                    retValue = tempInherit == IsInherit;
                    if (!retValue)
                    {
                        Console.Error.WriteLine("継承設定不一致： {0} / {1}", IsInherit, tempInherit);
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
                else if (IsInherit != null)
                {
                    Target = Item.INHERIT;
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
                    retValue = valueKind == RegistryControl.StringToValueKind(Type);
                    if (!retValue)
                    {
                        Console.Error.WriteLine(
                            "Type不一致： {0} / {1}", Type, RegistryControl.ValueKindToString(valueKind));
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
