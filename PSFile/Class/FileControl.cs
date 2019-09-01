using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace PSFile
{
    class FileControl
    {

        /// <summary>
        /// ファイルのセキュリティブロック解除
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);
        public static void RemoveSecurityBlock(string path)
        {
            DeleteFile(path + ":Zone.Identifier");
        }

        /// <summary>
        /// 文字列からFileSystemAccessのListを取得
        /// </summary>
        /// <param name="accessString">Access文字列</param>
        /// <returns></returns>
        public static List<FileSystemAccessRule> StringToAccessRules(string accessString)
        {
            List<FileSystemAccessRule> ruleList = new List<FileSystemAccessRule>();
            foreach (string ruleStr in accessString.Split('/'))
            {
                string[] fields = ruleStr.Split(';');
                if (fields.Length >= 3)
                {
                    ruleList.Add(new FileSystemAccessRule(
                        new NTAccount(fields[0]),
                        Enum.TryParse(fields[1], out FileSystemRights tempRights) ? tempRights : FileSystemRights.ReadAndExecute,
                        Enum.TryParse(fields[2], out AccessControlType tempType) ? tempType : AccessControlType.Allow));
                }
            }
            return ruleList;
        }

        /// <summary>
        /// AccessRuleの配列から文字列を取得
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static string AccessRulesToString(AuthorizationRuleCollection rules)
        {
            List<string> fileAccessRuleList = new List<string>();
            foreach (FileSystemAccessRule rule in rules)
            {
                string tempRights = rule.FileSystemRights == FileSystemRights.FullControl ?
                    Item.FULLCONTROL :
                    (rule.FileSystemRights & (~FileSystemRights.Synchronize)).ToString();

                fileAccessRuleList.Add(string.Format(
                    "{0};{1};{2}",
                    rule.IdentityReference.Value,
                    tempRights,
                    rule.AccessControlType));
            }
            return string.Join("/", fileAccessRuleList);
        }
    }
}
