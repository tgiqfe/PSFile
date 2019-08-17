using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Rename, "File")]
    public class RenameFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            if (NewName.Contains("\\"))
            {
                NewName = System.IO.Path.GetFileName(NewName);
            }
            FileSystem.RenameFile(Path, NewName);

            string newPath = System.IO.Path. Combine(System.IO.Path.GetDirectoryName(Path), NewName);
            WriteObject(new FileSummary(newPath, true));
        }
    }
}
