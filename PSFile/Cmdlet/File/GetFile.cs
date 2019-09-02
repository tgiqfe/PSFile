using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// ファイル情報を取得してDirectorySummaryインスタンスで返す
    /// TestGenerator : 無し
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "File")]
    public class GetFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get;set; }
        [Parameter]
        public SwitchParameter IgnoreTime { get; set; }
        [Parameter]
        public SwitchParameter IgnoreHash { get; set; }
        [Parameter]
        public SwitchParameter IgnoreAttributes { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSize { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurityBlock { get; set; }

        protected override void ProcessRecord()
        {
            FileSummary fs = new FileSummary(Path,
                IgnoreSecurity, IgnoreTime, IgnoreHash, IgnoreAttributes, IgnoreSize, IgnoreSecurityBlock);
            WriteObject(fs);
        }
    }
}
