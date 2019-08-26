﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Remove, "File")]
    public class RemoveFile : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public SwitchParameter SendToRecycleBin { get; set; }

        protected override void ProcessRecord()
        {
            Action<string> deleteFileAction = (filePath) =>
            {
                if (SendToRecycleBin)
                {
                    FileSystem.DeleteFile(
                        filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                }
                else
                {
                    FileSystem.DeleteFile(
                        filePath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently, UICancelOption.DoNothing);
                }
            };

            if (System.IO.Path.GetFileName(Path).Contains("*"))
            {
                //  ファイル名にワイルドカードを含む場合
                foreach (string fileName in
                    Directory.GetFiles(System.IO.Path.GetDirectoryName(Path), System.IO.Path.GetFileName(Path), System.IO.SearchOption.TopDirectoryOnly))
                {
                    deleteFileAction(fileName);
                }
            }
            else if (File.Exists(Path))
            {
                deleteFileAction(Path);
            }
        }
    }
}
