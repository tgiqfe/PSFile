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
    [Cmdlet(VerbsCommon.Remove, "Registry")]
    public class RemoveRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Position = 1)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, true))
            {
                if (Name == null)
                {
                    try
                    {
                        regKey.DeleteSubKeyTree("");
                    }
                    catch
                    {
                        using (Process proc = new Process())
                        {
                            proc.StartInfo.FileName = "reg";
                            proc.StartInfo.Arguments = $"delete \"{Path}\" /f";
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                }
                else
                {
                    regKey.DeleteValue(Name);
                }
            }
        }
    }
}
