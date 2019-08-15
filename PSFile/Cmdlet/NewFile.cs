using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;

namespace PSFile
{
    [Cmdlet(VerbsCommon.New, "File")]
    public class NewFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        /*
        [Parameter]
        public DateTime? CreationTime { get; set; }
        [Parameter]
        public DateTime? LastWriteTime { get; set; }
        [Parameter]
        public DateTime? LastAccessTime { get; set; }
        */

        protected override void ProcessRecord()
        {
            File.CreateText(Path).Close();
        }
    }
}
