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
    /// フォルダーを名前変更
    /// TestGenerator : Test-Directory -Path (変更前の名前の不在確認)
    ///                 Test-Directory -PAth (変更後の確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Directory")]
    public class RenameDirectory : PSCmdlet
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
            _generator.DirectoryPath(newPath);
            _generator.DirectoryPath(Path);

            if (Directory.Exists(Path))
            {
                FileSystem.RenameDirectory(Path, NewName);
            }
            WriteObject(new DirectorySummary(newPath, true));
        }
    }
}
