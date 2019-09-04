using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// ロードしたレジストリキーをアンロード
    /// </summary>
    [Cmdlet(VerbsCommon.Close, "Registry")]
    public class CloseRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            //  管理者実行確認
            Functions.CheckAdmin();

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if (regKey == null) { return; }
            }

            //  テスト自動生成
            _generator.RegistryPath(Path);

            string keyName = Path.Substring(Path.IndexOf("\\") + 1);
            RegistryHive.UnLoad(keyName);

            //  アンロード成功確認
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                if (regKey == null) { return; }
            }

            //  アンロード失敗時の再アンロード用コマンド
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"unload \"{Path}\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
        }
    }
}
