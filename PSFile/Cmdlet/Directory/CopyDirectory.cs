using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダーのコピー。結局Robocopyを使うのが一番無難だったので。
    /// Copy-Registryでアクセス権のコピーを行っていないので、アクセス権は除外 /COPY:DAT
    /// TestGenerator : コピー先を「Test-Directory -Path ～」、
    ///                 コピー元/コピー先を「Compare-Directory -Path ～ -Difference ～」
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Directory")]
    public class CopyDirectory : PSCmdlet
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

            //  テスト自動生成
            _generator.DirectoryPath(DirectoryPath);
            _generator.DirectoryPath(Destination);
            _generator.DirectoryCompare(DirectoryPath, Destination, true, true, false, false, false, true);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "robocopy.exe";
                proc.StartInfo.Arguments = Force ?
                    $"\"{DirectoryPath}\" \"{Destination}\" /MIR /E /COPY:DAT /XJD /XJF /R:0 /W:0 /NP" :
                    $"\"{DirectoryPath}\" \"{Destination}\" /E /COPY:DAT /XJD /XJF /R:0 /W:0 /NP";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }

            WriteObject(new DirectorySummary(Destination, true));
        }
    }
}
