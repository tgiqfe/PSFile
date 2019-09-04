using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// 新規レジストリキー作成
    /// TestGenerator : Test-Registry -Path ～
    ///                 Test-Registry -Path ～ -Access ～
    ///                 Test-Registry -Path ～ -Owner ～
    ///                 Test-Registry -Path ～ -Inherited ～
    /// </summary>
    [Cmdlet(VerbsCommon.New, "Registry")]
    public class NewRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Access { get; set; }
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
            Inherited = Item.CheckCase(Inherited);

            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if(regKey != null) { return; }
            }

            //  テスト自動生成
            _generator.RegistryPath(Path);

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, true, true))
            {
                RegistrySecurity security = null;

                //  Access文字列からの設定
                if (!string.IsNullOrEmpty(Access))
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  テスト自動生成
                    _generator.RegistryAccess(Path, Access, false);

                    foreach (RegistryAccessRule rule in RegistryControl.StringToAccessRules(Access))
                    {
                        security.AddAccessRule(rule);
                    }
                }

                //  上位からのアクセス権継承の設定変更
                if (Inherited != Item.NONE)
                {
                    if (security == null) { security = regKey.GetAccessControl(); }

                    //  テスト自動生成
                    _generator.RegistryInherited(Path, Inherited == Item.ENABLE);

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
                //  埋め込みのsubinacl.exeを展開
                string subinacl = EmbeddedResource.GetSubinacl(Item.APPLICATION_NAME);

                //  管理者実行確認
                Functions.CheckAdmin();

                //  テスト自動生成
                _generator.RegistryOwner(Path, Owner);

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/subkeyreg \"{Path}\" /owner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                WriteObject(new RegistrySummary(regKey, true));
            }
        }
    }
}
