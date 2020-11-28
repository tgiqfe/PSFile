using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.IO;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// バイナリファイルを16進数英数字テキストに変換したものを取得
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "FileBinaryText")]
    public class GetFileBinaryText : PSCmdlet
    {
        const int BUFF_SIZE = 4096;

        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string FilePath { get; set; }
        [Parameter, Alias("Output")]
        public string OutputFile { get; set; }
        [Parameter]
        public int TextBlock { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (File.Exists(FilePath))
            {
                string retString = "";

                //  対象ファイルを読み込んでバイナリテキスト化
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                using (var ms = new MemoryStream())
                {
                    if (fs.Length < Int32.MaxValue)
                    {
                        byte[] buffer = new byte[BUFF_SIZE];
                        int readed = 0;
                        while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readed);
                        }
                        retString = BitConverter.ToString(ms.ToArray()).Replace("-", "");
                    }
                    else
                    {
                        Console.Error.WriteLine("Size Over.");
                        return;
                    }
                }

                //  必要であればテキストブロック化
                if (TextBlock > 0)
                {
                    int count = TextBlock;
                    using (var sr = new StringReader(retString))
                    {
                        int readed = 0;
                        StringBuilder sb = new StringBuilder();
                        char[] buffer = new char[count];
                        while ((readed = sr.Read(buffer, 0, count)) > 0)
                        {
                            sb.AppendLine(new string(buffer, 0, readed));
                        }
                        retString = sb.ToString();
                    }
                }

                //  ファイルへ出力
                if (!string.IsNullOrEmpty(OutputFile))
                {
                    using (var sw = new StreamWriter(OutputFile, false, Encoding.UTF8))
                    {
                        sw.Write(retString);
                    }
                }
                else
                {
                    WriteObject(retString);
                }
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
