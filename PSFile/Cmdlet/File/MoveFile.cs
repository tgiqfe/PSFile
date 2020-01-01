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
    /// <summary>
    /// ファイルの移動
    /// TestGenerator : Test-File -Path で移動元フォルダー無しを確認
    ///                 Test-File -Path で移動先フォルダー有りを確認
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "File")]
    public class MoveFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination { get; set; }
        [Parameter]
        public SwitchParameter Force { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);

            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(Destination) || Destination.EndsWith("\\"))
            {
                Destination = System.IO.Path.Combine(Destination, System.IO.Path.GetFileName(FilePath));
            }

            //  テスト自動生成
            _generator.FilePath(FilePath);
            _generator.FilePath(Destination);

            FileSystem.MoveFile(FilePath, Destination, Force);

            WriteObject(new FileSummary(Destination, true));
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
