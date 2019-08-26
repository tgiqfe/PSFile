using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using PATH = System.IO.Path;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "Directory")]
    public class RemoveDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public SwitchParameter SendToRecycleBin { get; set; }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(Path))
            {
                FileSystem.DeleteDirectory(
                    Path,
                    UIOption.OnlyErrorDialogs, 
                    SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                    UICancelOption.DoNothing);
            }
        }
    }
}
