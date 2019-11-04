using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリへの各種設定
    /// TestGenerator : Test-Registry -Path ～
    ///                 Test-Registry -Path ～ -Access ～
    ///                 Test-Registry -Path ～ -Owner ～
    ///                 Test-Registry -Path ～ -Inherited ～
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "Registry")]
    public class SetRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string RegistryPath { get; set; }
        [Parameter(Position = 1)]
        public string Name { get; set; }
        [Parameter(Position = 2)]
        public string Value { get; set; }
        [Parameter(Position = 3)]
        [ValidateSet(Item.REG_SZ, Item.REG_BINARY, Item.REG_DWORD, Item.REG_QWORD, Item.REG_MULTI_SZ, Item.REG_EXPAND_SZ, Item.REG_NONE)]
        public string Type { get; set; } = Item.REG_SZ;
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Chroot { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            Type = Item.CheckCase(Type);
            Inherited = Item.CheckCase(Inherited);

            if (Chroot != null)
            {
                string keyName = RegistryPath.Substring(RegistryPath.IndexOf("\\") + 1);
                RegistryPath = System.IO.Path.Combine(Chroot, keyName);
            }

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, true, true))
            {
                if (regKey == null) { return; }

                RegistrySecurity security = null;

                //  Access文字列からの設定
                //  ""で全アクセス権設定を削除
                if (Access != null)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }
                    foreach (RegistryAccessRule removeRule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        security.RemoveAccessRule(removeRule);
                    }

                    //  テスト自動生成
                    _generator.RegistryAccess(RegistryPath, Access, false);

                    if (Access != string.Empty)     //  このif文分岐が無くても同じ挙動するけれど、一応記述
                    {
                        foreach (RegistryAccessRule addRule in RegistryControl.StringToAccessRules(Access))
                        {
                            security.AddAccessRule(addRule);
                        }
                    }
                }

                //  上位からのアクセス権継承の設定変更
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  テスト自動生成
                    _generator.RegistryInherited(RegistryPath, Inherited == Item.ENABLE);

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

                if (security != null) { regKey.SetAccessControl(security); }
            }

            //  所有者変更
            if (Owner != null)
            {
                string subinacl = EmbeddedResource.GetSubinacl(Item.APPLICATION_NAME);

                //  管理者実行確認
                Functions.CheckAdmin();

                //  テスト自動生成
                _generator.RegistryOwner(RegistryPath, Owner);

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/subkeyreg \"{RegistryPath}\" /owner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            //  レジストリ値の設定
            if (Name != null)
            {
                //  テスト自動生成
                _generator.RegistryType(RegistryPath, Name, Type);
                _generator.RegistryValue(RegistryPath, Name, Value);

                switch (Type)
                {
                    case Item.REG_SZ:
                        Registry.SetValue(RegistryPath, Name, Value, RegistryValueKind.String);
                        break;
                    case Item.REG_BINARY:
                        Registry.SetValue(RegistryPath, Name, RegistryControl.StringToRegBinary(Value), RegistryValueKind.Binary);
                        break;
                    case Item.REG_DWORD:
                        Registry.SetValue(RegistryPath, Name, int.Parse(Value), RegistryValueKind.DWord);
                        break;
                    case Item.REG_QWORD:
                        Registry.SetValue(RegistryPath, Name, long.Parse(Value), RegistryValueKind.QWord);
                        break;
                    case Item.REG_MULTI_SZ:
                        Registry.SetValue(RegistryPath, Name, Functions.SplitBQt0(Value), RegistryValueKind.MultiString);
                        break;
                    case Item.REG_EXPAND_SZ:
                        Registry.SetValue(RegistryPath, Name, Value, RegistryValueKind.ExpandString);
                        break;
                    case Item.REG_NONE:
                        Registry.SetValue(RegistryPath, Name, new byte[2] { 0, 0 }, RegistryValueKind.None);
                        break;
                }
            }

            /*  実行していて結構うっとおしいので、出力しないことにします。
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                WriteObject(new RegistrySummary(regKey, true));
            }
            */
        }
    }
}
