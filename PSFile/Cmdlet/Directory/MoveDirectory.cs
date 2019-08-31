using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using Microsoft.VisualBasic.FileIO;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Move, "Directory")]
    public class MoveDirectory : PSCmdlet
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

            bool ret = Functions.CheckChildItem(Path, Destination);
            if (!ret)
            {
                FileSystem.MoveDirectory(Path, Destination, Force);
            }
            WriteObject(new DirectorySummary(Destination, true));
        }
    }
}
