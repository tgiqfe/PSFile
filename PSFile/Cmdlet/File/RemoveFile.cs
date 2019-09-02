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
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
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
            /*
            Action<string> deleteFileAction = (filePath) =>
            {
                FileSystem.DeleteFile(
                    filePath,
                    UIOption.OnlyErrorDialogs,
                    SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                    UICancelOption.DoNothing);
            };
            */

            if (System.IO.Path.GetFileName(Path).Contains("*"))
            {
                //  ファイル名にワイルドカードを含む場合
                foreach (string fileName in
                    Directory.GetFiles(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileName(Path), System.IO.SearchOption.TopDirectoryOnly))
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
            else if (File.Exists(Path))
            {
                //  テスト自動生成
                _generator.FilePath(Path);

                //deleteFileAction(Path);
                FileSystem.DeleteFile(
                    Path,
                    UIOption.OnlyErrorDialogs,
                    SendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently,
                    UICancelOption.DoNothing);
            }
        }
    }
}
