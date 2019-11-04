using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリ削除
    /// TestGenerator : Test-Registry -Path ～ (不在確認)
    ///                 Test-Registry -Path ～ -Name ～ (不在確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Registry")]
    public class RemoveRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }
        [Parameter(Position = 1)]
        public string Name { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(RegistryPath, false, true))
            {
                if (Name == null)
                {
                    try
                    {
                        //  テスト自動生成
                        _generator.RegistryPath(RegistryPath);

                        regKey.DeleteSubKeyTree("");
                    }
                    catch
                    {
                        using (Process proc = new Process())
                        {
                            proc.StartInfo.FileName = "reg";
                            proc.StartInfo.Arguments = $"delete \"{RegistryPath}\" /f";
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                }
                else
                {
                    //  テスト自動生成
                    _generator.RegistryName(RegistryPath, Name);

                    regKey.DeleteValue(Name);
                }
            }
        }
    }
}
