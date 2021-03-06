﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリをロード
    /// </summary>
    [Cmdlet(VerbsData.Mount, "Registry")]
    public class MountRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }
        [Parameter(Position = 1), Alias("File")]
        public string DatFile { get; set; }
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

            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                if (regKey != null) { return; }
            }

            //  テスト自動生成
            _generator.RegistryPath(RegistryPath);

            string keyName = RegistryPath.Substring(RegistryPath.IndexOf("\\") + 1);
            RegistryHive.Load(keyName, DatFile);

            //  ロード成功確認
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                if (regKey != null)
                {
                    WriteObject(new RegistrySummary(regKey));
                    return;
                }
            }

            //  ロード失敗時の再ロード用コマンド
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"load \"{RegistryPath}\" \"{DatFile}\"";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                if (regKey != null)
                {
                    WriteObject(new RegistrySummary(regKey));
                }
            }
        }
    }
}
