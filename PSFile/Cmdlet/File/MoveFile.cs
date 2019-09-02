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
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination { get; set; }
        [Parameter]
        public SwitchParameter Force { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(Destination) || Destination.EndsWith("\\"))
            {
                Destination = System.IO.Path.Combine(Destination, System.IO.Path.GetFileName(Path));
            }

            //  テスト自動生成
            _generator.FilePath(Path);
            _generator.FilePath(Destination);

            FileSystem.MoveFile(Path, Destination, Force);

            WriteObject(new FileSummary(Destination, true));
        }

    }
}
