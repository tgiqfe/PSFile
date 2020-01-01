using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダー情報を取得してDirectorySummaryインスタンスで返す
    /// TestGenerator : 無し
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Directory")]
    public class GetDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSecurity { get; set; }
        [Parameter]
        public SwitchParameter IgnoreTime { get; set; }
        [Parameter]
        public SwitchParameter IgnoreAttributes { get; set; }
        [Parameter]
        public SwitchParameter IgnoreSize { get; set; }
        [Parameter]
        public SwitchParameter IgnoreFiles { get; set; }
        [Parameter]
        public SwitchParameter IsLightFiles { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            DirectorySummary ds = new DirectorySummary(DirectoryPath,
                IgnoreSecurity, IgnoreTime, IgnoreAttributes, IgnoreSize, IgnoreFiles, IsLightFiles);
            WriteObject(ds, true);
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
