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
    /// ファイルをコピー
    /// TestGenerator : Test-File -Path ～
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "File")]
    public class CopyFile : PSCmdlet
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
            _generator.FileCompare(FilePath, Destination, false, true, false, false, false, false);

            try
            {
                FileSystem.CopyFile(FilePath, Destination, Force);
            }
            catch (UnauthorizedAccessException)
            {
                //  読み取り専用で失敗した場合は、読み取り専用属性を削除してもう一度コピー
                if (Force)
                {
                    new FileInfo(Destination).IsReadOnly = false;
                    FileSystem.CopyFile(FilePath, Destination, Force);
                }
            }

            WriteObject(new FileSummary(Destination, true));
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
