using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using PSFile.Serialize;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// ファイル情報をエクスポートしてDirectorySummaryインスタンスをシリアライズ
    /// TestGenerator : 無し
    /// </summary>
    [Cmdlet(VerbsData.Export, "File")]
    public class ExportFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Output { get; set; }
        [Parameter]
        [ValidateSet(Item.XML, Item.JSON, Item.YML)]
        public string DataType { get; set; } = Item.JSON;

        protected override void BeginProcessing()
        {
            DataType = Item.CheckCase(DataType);
        }

        protected override void ProcessRecord()
        {
            List<FileSummary> fsList = new List<FileSummary>();
            fsList.Add(new FileSummary(Path, true));
            if (Output == null)
            {
                switch (DataType)
                {
                    case Item.XML:
                        WriteObject(DataSerializer.Serialize<List<FileSummary>>(fsList, Serialize.DataType.Xml));
                        break;
                    case Item.JSON:
                        WriteObject(DataSerializer.Serialize<List<FileSummary>>(fsList, Serialize.DataType.Json));
                        break;
                    case Item.YML:
                        WriteObject(DataSerializer.Serialize<List<FileSummary>>(fsList, Serialize.DataType.Yml));
                        break;
                }
                /*
                switch (DataType)
                {
                    case Item.XML:
                        DataSerializer.Serialize<List<FileSummary>>(fsList, Console.Out, PSFile.Serialize.DataType.Xml);
                        break;
                    case Item.JSON:
                        DataSerializer.Serialize<List<FileSummary>>(fsList, Console.Out, PSFile.Serialize.DataType.Json);
                        break;
                    case Item.YML:
                        DataSerializer.Serialize<List<FileSummary>>(fsList, Console.Out, PSFile.Serialize.DataType.Yml);
                        break;
                }
                */
            }
            else
            {
                DataSerializer.Serialize<List<FileSummary>>(fsList, Output);
            }
        }
    }
}
