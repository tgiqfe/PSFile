﻿using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Set, "Registry")]
    public class SetRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
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

        protected override void BeginProcessing()
        {
            Type = Item.CheckCase(Type);
            Inherited = Item.CheckCase(Inherited);

            if (Chroot != null)
            {
                string keyName = Path.Substring(Path.IndexOf("\\") + 1);
                Path = System.IO.Path.Combine(Chroot, keyName);
            }
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, true, true))
            {
                if (regKey == null) { return; }

                RegistrySecurity security = null;

                //  Access文字列からの設定
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }
                    foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(rule);
                    }
                }

                //  上位からのアクセス権継承の設定変更
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }
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

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/subkeyreg \"{Path}\" /owner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            //  レジストリ値の設定
            if (Name != null)
            {
                //  テスト自動生成
                switch (Type)
                {
                    case Item.REG_SZ:
                        Registry.SetValue(Path, Name, Value, RegistryValueKind.String);
                        break;
                    case Item.REG_BINARY:
                        Registry.SetValue(Path, Name, RegistryControl.StringToRegBinary(Value), RegistryValueKind.Binary);
                        break;
                    case Item.REG_DWORD:
                        Registry.SetValue(Path, Name, int.Parse(Value), RegistryValueKind.DWord);
                        break;
                    case Item.REG_QWORD:
                        Registry.SetValue(Path, Name, long.Parse(Value), RegistryValueKind.QWord);
                        break;
                    case Item.REG_MULTI_SZ:
                        Registry.SetValue(Path, Name, Functions.SplitBQt0(Value), RegistryValueKind.MultiString);
                        break;
                    case Item.REG_EXPAND_SZ:
                        Registry.SetValue(Path, Name, Value, RegistryValueKind.ExpandString);
                        break;
                    case Item.REG_NONE:
                        Registry.SetValue(Path, Name, new byte[2] { 0, 0 }, RegistryValueKind.None);
                        break;
                }
            }
        }
    }
}
