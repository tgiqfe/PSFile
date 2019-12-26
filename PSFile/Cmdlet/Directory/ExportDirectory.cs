using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using PSFile.Serialize;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// フォルダー情報をエクスポートしてDirectorySummaryインスタンスをシリアライズ
    /// TestGeneratro : 無し
    /// </summary>
    [Cmdlet(VerbsData.Export, "Directory")]
    public class ExportDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Output { get; set; }
        [Parameter]
        [ValidateSet(Item.XML, Item.JSON, Item.YML)]
        public string DataType { get; set; } = Item.JSON;
        [Parameter]
        public SwitchParameter IsLightFiles { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            DataType = Item.CheckCase(DataType);

            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            List<DirectorySummary> dsList = new List<DirectorySummary>();
            Action<string> getDirSummary = null;
            getDirSummary = (targetDirPath) =>
            {
                dsList.Add(new DirectorySummary(targetDirPath,false, false, false, false, false, IsLightFiles));
                foreach (DirectoryInfo di in new DirectoryInfo(targetDirPath).GetDirectories())
                {
                    getDirSummary(di.FullName);
                }
            };
            getDirSummary(Path);

            if(Output == null)
            {
                switch (DataType)
                {
                    case Item.XML:
                        WriteObject(DataSerializer.Serialize<List<DirectorySummary>>(dsList, Serialize.DataType.Xml));
                        break;
                    case Item.JSON:
                        WriteObject(DataSerializer.Serialize<List<DirectorySummary>>(dsList, Serialize.DataType.Json));
                        break;
                    case Item.YML:
                        WriteObject(DataSerializer.Serialize<List<DirectorySummary>>(dsList, Serialize.DataType.Yml));
                        break;
                }
            }
            else
            {
                DataSerializer.Serialize<List<DirectorySummary>>(dsList, Output);
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
