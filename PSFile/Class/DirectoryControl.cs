using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile
{
    class DirectoryControl
    {

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
                if (fields.Length >= 5)
                {
                    ruleList.Add(new FileSystemAccessRule(
                        new NTAccount(fields[0]),
                        Enum.TryParse(fields[1], out FileSystemRights tempRights) ? tempRights : FileSystemRights.ReadAndExecute,
                        Enum.TryParse(fields[2], out InheritanceFlags tempInhrFlags) ? tempInhrFlags : InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        Enum.TryParse(fields[3], out PropagationFlags tempPrpgFlags) ? tempPrpgFlags : PropagationFlags.None,
                        Enum.TryParse(fields[4], out AccessControlType tempType) ? tempType : AccessControlType.Allow));
                }
            }
            return ruleList;
        }

        /// <summary>
        /// AccessRuleのリストから文字列を取得
        /// </summary>
        /// <param name="rules">AccessRuleのコレクション</param>
        /// <returns>Access文字列</returns>
        public static string AccessRulesToString(AuthorizationRuleCollection rules)
        {
            List<string> accessRuleList = new List<string>();
            foreach (FileSystemAccessRule rule in rules)
            {
                string tempRights = rule.FileSystemRights == FileSystemRights.FullControl ?
                    Item.FULLCONTROL :
                    (rule.FileSystemRights & (~FileSystemRights.Synchronize)).ToString();
                accessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    tempRights,
                    rule.InheritanceFlags,
                    rule.PropagationFlags,
                    rule.AccessControlType));
            }
            return string.Join("/", accessRuleList);
        }

        /// <summary>
        /// Access文字列2つの内容をチェックして一致確認
        /// </summary>
        /// <param name="accessStringA"></param>
        /// <param name="accessStringB"></param>
        /// <returns></returns>
        public static bool IsMatchAccess(string accessStringA, string accessStringB)
        {
            string[] accessStringArrayA = accessStringA.Split(';');
            string[] accessStringArrayB = accessStringB.Split(';');

            //  Accountチェック
            string accountA = accessStringArrayA[0];
            string accountB = accessStringArrayB[0];
            if (accountA.Contains("\\") && !accountB.Contains("\\"))
            {
                accountB = accountA.Substring(0, accountA.IndexOf("\\") + 1) + accountB;
            }
            if (!accountA.Contains("\\") && accountB.Contains("\\"))
            {
                accountA = accountB.Substring(0, accountB.IndexOf("\\") + 1) + accountA;
            }
            if (!accountA.Equals(accountB, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //  Rightsチェック
            string rightsA = Item.CheckCase(accessStringArrayA[1]);
            string rightsB = Item.CheckCase(accessStringArrayB[1]);
            rightsA = Enum.TryParse(rightsA, out FileSystemRights tempRightsA) ? tempRightsA.ToString() : "nullA";
            rightsB = Enum.TryParse(rightsB, out FileSystemRights tempRightsB) ? tempRightsB.ToString() : "nullB";
            if (rightsA != rightsB)
            {
                return false;
            }

            //  InheritanceFlagsチェック
            string ifA = Item.CheckCase(accessStringArrayA[2]);
            string ifB = Item.CheckCase(accessStringArrayB[2]);
            ifA = Enum.TryParse(ifA, out InheritanceFlags tempIFA) ? tempIFA.ToString() : "nullA";
            ifB = Enum.TryParse(ifB, out InheritanceFlags tempIFB) ? tempIFB.ToString() : "nullB";
            if (ifA != ifB)
            {
                return false;
            }

            //  PropagationFlagsチェック
            string pfA = Item.CheckCase(accessStringArrayA[3]);
            string pfB = Item.CheckCase(accessStringArrayB[3]);
            pfA = Enum.TryParse(pfA, out PropagationFlags tempPFA) ? tempPFA.ToString() : "nullA";
            pfB = Enum.TryParse(pfB, out PropagationFlags tempPFB) ? tempPFB.ToString() : "nullB";
            if (pfA != pfB)
            {
                return false;
            }

            //  AccessControlチェック
            string acA = Item.CheckCase(accessStringArrayA[4]);
            string acB = Item.CheckCase(accessStringArrayB[4]);
            acA = Enum.TryParse(acA, out AccessControlType tempACA) ? tempACA.ToString() : "nullA";
            acB = Enum.TryParse(acB, out AccessControlType tempACB) ? tempACB.ToString() : "nullB";
            if (acA != acB)
            {
                return false;
            }

            return true;
        }
    }
}
