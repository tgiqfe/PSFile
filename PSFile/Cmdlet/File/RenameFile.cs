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
    /// <summary>
    /// ファイルの名前変更
    /// TestGenerator : Test-File -Path (変更前の名前の不在確認)
    ///                 Test-File -PAth (変更後の確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "File")]
    public class RenameFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string NewName { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            if (NewName.Contains("\\"))
            {
                NewName = System.IO.Path.GetFileName(NewName);
            }
            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), NewName);

            //  テスト自動生成
            _generator.FilePath(Path);
            _generator.FilePath(newPath);

            if (File.Exists(Path))
            {
                FileSystem.RenameFile(Path, NewName);
            }
            WriteObject(new FileSummary(newPath, true));
        }
    }
}
