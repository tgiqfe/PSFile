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
    /// <summary>
    /// フォルダーを名前変更
    /// TestGenerator : Test-Directory -Path (変更前の名前の不在確認)
    ///                 Test-Directory -PAth (変更後の確認)
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Directory")]
    public class RenameDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string DirectoryPath { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string NewName { get; set; }
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
            if (NewName.Contains("\\"))
            {
                NewName = System.IO.Path.GetFileName(NewName);
            }
            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(DirectoryPath), NewName);

            //  テスト自動生成
            _generator.DirectoryPath(DirectoryPath);
            _generator.DirectoryPath(newPath);

            if (Directory.Exists(DirectoryPath))
            {
                FileSystem.RenameDirectory(DirectoryPath, NewName);
            }
            WriteObject(new DirectorySummary(newPath, true));
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
