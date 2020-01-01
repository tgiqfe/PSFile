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
    /// フォルダーの移動
    /// TestGenerator : Test-Directory -Path で移動元フォルダー無しを確認
    ///                 Test-Directory -Path で移動先フォルダー有りを確認
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "Directory")]
    public class MoveDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
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
            if (Destination.EndsWith("\\"))
            {
                Destination = System.IO.Path.Combine(Destination, System.IO.Path.GetFileName(DirectoryPath));
            }

            bool ret = Functions.CheckChildItem(DirectoryPath, Destination);
            if (!ret)
            {
                //  テスト自動生成
                _generator.DirectoryPath(DirectoryPath);
                _generator.DirectoryPath(Destination);

                FileSystem.MoveDirectory(DirectoryPath, Destination, Force);
            }
            WriteObject(new DirectorySummary(Destination, true));
        }
    }
}
