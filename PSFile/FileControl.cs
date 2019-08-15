using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile
{
    class FileControl
    {
        /// <summary>
        /// 文字列からFileSystemAccessのListを取得
        /// </summary>
        /// <param name="ruleString">Access文字列</param>
        /// <returns></returns>
        public static List<FileSystemAccessRule> StringToAccessRules(string ruleString)
        {
            List<FileSystemAccessRule> ruleList = new List<FileSystemAccessRule>();
            foreach (string ruleStr in GlobalParam.reg_Delimitor.Split(ruleString))
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
    }
}
