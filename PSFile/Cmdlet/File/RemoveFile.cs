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
    /// ファイル削除
    /// TestGenerator : Test-File -Path ～ (不在確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "File")]
    public class RemoveFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter]
        public SwitchParameter SendToRecycleBin { get; set; }
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
            if (Path.GetFileName(FilePath).Contains("*"))
            {
                //  ファイル名にワイルドカードを含む場合
                foreach (string fileName in
                    Directory.GetFiles(Path.GetDirectoryName(FilePath), Path.GetFileName(FilePath), System.IO.SearchOption.TopDirectoryOnly))
                {
                    //  テスト自動生成
                    _generator.FilePath(fileName);

                    //deleteFileAction(fileName);
                    FileSystem.DeleteFile(
                        fileName,
                        UIOption.OnlyErrorDialogs,
                        SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                        UICancelOption.DoNothing);
                }
            }
            else if (File.Exists(FilePath))
            {
                //  テスト自動生成
                _generator.FilePath(FilePath);

                //deleteFileAction(Path);
                FileSystem.DeleteFile(
                    FilePath,
                    UIOption.OnlyErrorDialogs,
                    SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                    UICancelOption.DoNothing);
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
