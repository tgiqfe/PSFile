using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using PATH = System.IO.Path;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダー削除
    /// TestGenerator : Test-Directory -Path ～ (不在確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Directory")]
    public class RemoveDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter]
        public SwitchParameter SendToRecycleBin { get; set; }
        [Parameter]
        public string Test { get; set; }
        private TestGenerator _generator = null;

        protected override void BeginProcessing()
        {
            _generator = new TestGenerator(Test);
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(DirectoryPath))
            {
                //  テスト自動生成
                _generator.DirectoryPath(DirectoryPath);

                FileSystem.DeleteDirectory(
                    DirectoryPath,
                    UIOption.OnlyErrorDialogs, 
                    SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                    UICancelOption.DoNothing);
            }
        }
    }
}
