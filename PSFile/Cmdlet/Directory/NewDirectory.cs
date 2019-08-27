using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Management.Automation;
using System.Diagnostics;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "Directory")]
    public class NewDirectory : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter]
        public string Access { get; set; }
        [Parameter]
        public string Owner { get; set; }
        [Parameter]
        [ValidateSet(Item.NONE, Item.ENABLE, Item.DISABLE, Item.REMOVE)]
        public string Inherited { get; set; } = Item.NONE;
        [Parameter]
        public DateTime? CreationTime { get; set; }
        [Parameter]
        public DateTime? LastWriteTime { get; set; }
        [Parameter]
        public string[] Attributes { get; set; }
        private string _Attributes = null;

        protected override void BeginProcessing()
        {
            Inherited = Item.CheckCase(Inherited);
            _Attributes = Item.CheckCase(Attributes);
        }

        protected override void ProcessRecord()
        {
            if (Directory.Exists(Path)){ return; }

            Directory.CreateDirectory(Path);

            DirectorySecurity security = null;

            //  Access設定
            if (!string.IsNullOrEmpty(Access))
            {
                if (security == null) { security = Directory.GetAccessControl(Path); }
                foreach (FileSystemAccessRule removeRule in security.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    security.RemoveAccessRule(removeRule);
                }
                foreach (FileSystemAccessRule addRule in DirectoryControl.StringToAccessRules(Access))
                {
                    security.AddAccessRule(addRule);
                }
            }

            //  Owner設定
            if (!string.IsNullOrEmpty(Owner))
            {
                //  埋め込みのsubinacl.exeを展開
                /*
                string tempDir = System.IO.Path.Combine(
                    Environment.ExpandEnvironmentVariables("%TEMP%"),
                    "PowerReg");
                string subinacl = System.IO.Path.Combine(tempDir, "subinacl.exe");
                if (!File.Exists(subinacl))
                {
                    EmbeddedResource.Expand(tempDir);
                }
                */
                string subinacl = EmbeddedResource.GetSubinacl("PowerReg");

                //  管理者実行確認
                Message.CheckAdmin();

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = subinacl;
                    proc.StartInfo.Arguments = $"/subdirectories \"{Path}\" /setowner=\"{Owner}\"";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            //  Inherited設定
            if (Inherited != Item.NONE)
            {
                if (security == null) { security = Directory.GetAccessControl(Path); }
                switch (Inherited)
                {
                    case Item.ENABLE:
                        security.SetAccessRuleProtection(false, false);
                        break;
                    case Item.DISABLE:
                        security.SetAccessRuleProtection(true, true);
                        break;
                    case Item.REMOVE:
                        security.SetAccessRuleProtection(true, false);
                        break;
                }
            }

            if (security != null) { Directory.SetAccessControl(Path, security); }

            //  作成日時
            if (CreationTime != null)
            {
                Directory.SetCreationTime(Path, (DateTime)CreationTime);
            }

            //  更新一時
            if (LastWriteTime != null)
            {
                Directory.SetLastWriteTime(Path, (DateTime)LastWriteTime);
            }

            //  フォルダー属性
            if (!string.IsNullOrEmpty(_Attributes))
            {
                if (!_Attributes.Contains(Item.DIRECTORY))
                {
                    _Attributes += ", " + Item.DIRECTORY;
                }
                File.SetAttributes(Path, (FileAttributes)Enum.Parse(typeof(FileAttributes), _Attributes));
            }

            WriteObject(new DirectorySummary(Path, true));
        }
    }
}
