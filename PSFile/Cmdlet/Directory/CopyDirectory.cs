using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダーのコピー。結局Robocopyを使うのが一番無難だったので。
    /// アクセス権のコピーは行わないことにします。なので /COPY:DAT
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Directory")]
    public class CopyDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination { get; set; }
        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if (Destination.EndsWith("\\"))
            {
                Destination = System.IO.Path.Combine(Destination, System.IO.Path.GetFileName(Path));
            }
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "robocopy.exe";
                proc.StartInfo.Arguments = Force ?
                    $"\"{Path}\" \"{Destination}\" /MIR /COPY:DAT /XJD /XJF /R:0 /W:0 /NP" :
                    $"\"{Path}\" \"{Destination}\" /COPY:DAT /XJD /XJF /R:0 /W:0 /NP";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }

            WriteObject(new FileSummary(Destination, true));
        }
    }
}
