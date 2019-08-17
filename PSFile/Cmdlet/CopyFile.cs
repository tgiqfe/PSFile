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
    [Cmdlet(VerbsCommon.Copy, "File")]
    public class CopyFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination { get; set; }
        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(Destination) || Destination.EndsWith("\\"))
            {
                Destination = System.IO.Path.Combine(Destination, System.IO.Path.GetFileName(Path));
            }

            try
            {
                /*
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(Destination)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Destination));
                }
                */
                FileSystem.CopyFile(Path, Destination, Force);
                //File.Copy(Path, Destination, Force);
            }
            catch (UnauthorizedAccessException)
            {
                //  読み取り専用で失敗した場合は、読み取り専用属性を削除してもう一度コピー
                if (Force)
                {
                    new FileInfo(Destination).IsReadOnly = false;
                    File.Copy(Path, Destination, Force);
                }
            }

            WriteObject(new FileSummary(Destination, true));
        }
    }
}
