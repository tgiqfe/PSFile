using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

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
                DataSerializer.Serialize<List<FileSummary>>(fsList, Console.Out, DataType);
            }
            else
            {
                DataSerializer.Serialize<List<FileSummary>>(fsList, Output);
            }
        }
    }
}
