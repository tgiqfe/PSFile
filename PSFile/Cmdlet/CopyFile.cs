using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;

namespace PSFile
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
            try
            {
                File.Copy(Path, Destination, Force);
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
        }
    }
}
