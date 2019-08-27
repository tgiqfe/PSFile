using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsCommon.Copy, "Registry")]
    public class CopyRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }
        [Parameter(Mandatory = true, Position = 1)]
        public string Destination { get; set; }
        [Parameter]
        public string Name { get; set; }
        [Parameter]
        public string DestinationName { get; set; }

        protected override void ProcessRecord()
        {
            if (Message.CheckChildItem(Path, Destination)) { return; }

            if (Name == null)
            {
                //  レジストリキーをコピー
                CopyRegistryKey(Path, Destination);
            }
            else
            {
                //  レジストリ値をコピー
                CopyRegistryValue(Path, Destination, Name, DestinationName);
            }
        }

        //  レジストリキーをコピー
        private void CopyRegistryKey(string source, string destination)
        {
            Action<RegistryKey, RegistryKey> copyRegKey = null;
            copyRegKey = (srcKey, dstKey) =>
            {
                foreach (string paramName in srcKey.GetValueNames())
                {
                    RegistryValueKind valueKind = srcKey.GetValueKind(paramName);
                    dstKey.SetValue(
                        paramName,
                        valueKind == RegistryValueKind.ExpandString ?
                            srcKey.GetValue(paramName, "", RegistryValueOptions.DoNotExpandEnvironmentNames) :
                            srcKey.GetValue(paramName),
                        valueKind);
                }
                foreach (string keyName in srcKey.GetSubKeyNames())
                {
                    using (RegistryKey subSrcKey = srcKey.OpenSubKey(keyName, false))
                    using (RegistryKey subDstKey = dstKey.CreateSubKey(keyName, true))
                    {
                        try
                        {
                            copyRegKey(subSrcKey, subDstKey);
                        }
                        catch (System.Security.SecurityException)
                        {
                            Console.WriteLine("アクセス拒否：SecurityException\r\n" + keyName);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine("アクセス拒否：UnauthorizedAccessException\r\n" + keyName);
                        }
                        catch (ArgumentException)
                        {
                            //  無効なValueKindのレジストリ値への対策
                            //  reg copyコマンドでコピー実行
                            using (Process proc = new Process())
                            {
                                proc.StartInfo.FileName = "reg.exe";
                                proc.StartInfo.Arguments = $@"copy ""{subSrcKey.ToString()}"" ""{subDstKey.ToString()}"" /s /f";
                                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                proc.Start();
                                proc.WaitForExit();
                            }
                        }
                    }
                }
            };

            using (RegistryKey sourceKey = RegistryControl.GetRegistryKey(source, false, true))
            using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destination, true, true))
            {
                copyRegKey(sourceKey, destinationKey);
                //  コピー元を削除する場合
                //sourceKey.DeleteSubKeyTree("");
            }
        }

        //  レジストリ値をコピー
        private void CopyRegistryValue(string source, string destination, string name, string destinationName)
        {
            using (RegistryKey sourceKey = RegistryControl.GetRegistryKey(source, false, true))
            using (RegistryKey destinationKey = RegistryControl.GetRegistryKey(destination, true, true))
            {
                RegistryValueKind valueKind = sourceKey.GetValueKind(name);
                object sourceValue = valueKind == RegistryValueKind.ExpandString ?
                    sourceKey.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) :
                    sourceKey.GetValue(name);
                if (destinationName == null)
                {
                    destinationName = name;
                }

                destinationKey.SetValue(destinationName, sourceValue, valueKind);
                //  コピー元を削除する場合
                //sourceKey.DeleteValue(name);
            }
        }
    }
}
