using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;

namespace PSFile
{
    public class ConvertAccess
    {
        /// <summary>
        /// File,Directory,Registryの内部判別用
        /// </summary>
        enum ObjectType { File, Directory, Registry }

        #region String_AccessRules

        public static List<AccessRule> ToAccess_File(string ruleString)
        {
            return StringToAccessRules(ruleString, ObjectType.File);
        }
        public static List<AccessRule> ToAccess_Directory(string ruleString)
        {
            return StringToAccessRules(ruleString, ObjectType.Directory);
        }
        public static List<AccessRule> ToAccess_Registry(string ruleString)
        {
            return StringToAccessRules(ruleString, ObjectType.Registry);
        }

        /// <summary>
        /// 文字列からAccessRuleのListを取得
        /// </summary>
        /// <param name="ruleString">Access文字列</param>
        /// <param name="oTypeString">File,Directory,Registryのいずれか</param>
        /// <returns></returns>
        public static List<AccessRule> StringToAccessRules(string ruleString, string oTypeString)
        {
            switch (oTypeString.ToLower())
            {
                case "directory": return StringToAccessRules(ruleString, ObjectType.Directory);
                case "registry": return StringToAccessRules(ruleString, ObjectType.Registry);
                case "file":
                default: return StringToAccessRules(ruleString, ObjectType.File);
            }
        }

        /// <summary>
        /// 文字列からAccessRuleのListを取得
        /// </summary>
        /// <param name="ruleString">Access文字列</param>
        /// <param name="oType">File,Directory,Registryのいずれか</param>
        /// <returns></returns>
        private static List<AccessRule> StringToAccessRules(string ruleString, ObjectType oType)
        {
            List<AccessRule> ruleList = new List<AccessRule>();
            switch (oType)
            {
                case ObjectType.File:
                    foreach (string ruleStr in ruleString.Split('/'))
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
                    break;
                case ObjectType.Directory:
                    foreach (string ruleStr in ruleString.Split('/'))
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
                    break;
                case ObjectType.Registry:

                    //  Registry用の処理

                    break;
            }
            return ruleList;
        }

        #endregion

        #region AccessRules_string

        public static string ToString_File(AuthorizationRuleCollection rules)
        {
            return AccessRulesToString(rules, ObjectType.File);
        }
        public static string ToString_Directory(AuthorizationRuleCollection rules)
        {
            return AccessRulesToString(rules, ObjectType.Directory);
        }
        public static string ToString_Registry(AuthorizationRuleCollection rules)
        {
            return AccessRulesToString(rules, ObjectType.Registry);
        }

        /// <summary>
        /// AccessRuleのListからAccess文字列を取得
        /// </summary>
        /// <param name="rules">AccessRuleのコレクション</param>
        /// <param name="oType">File,Directory,Registryのいずれか</param>
        /// <returns></returns>
        public static string AccessRulesToString(AuthorizationRuleCollection rules, string oTypeString)
        {
            switch (oTypeString.ToLower())
            {
                case "directory": return AccessRulesToString(rules, ObjectType.Directory);
                case "registry": return AccessRulesToString(rules, ObjectType.Registry);
                case "file":
                default: return AccessRulesToString(rules, ObjectType.File);
            }
        }

        /// <summary>
        /// AccessRuleのListからAccess文字列を取得
        /// </summary>
        /// <param name="rules">AccessRuleのコレクション</param>
        /// <param name="oType">File,Directory,Registryのいずれか</param>
        /// <returns></returns>
        private static string AccessRulesToString(AuthorizationRuleCollection rules, ObjectType oType)
        {
            List<string> ruleList = new List<string>();
            switch (oType)
            {
                case ObjectType.File:
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        string tempRights = rule.FileSystemRights == FileSystemRights.FullControl ?
                            Item.FULLCONTROL :
                            (rule.FileSystemRights & (~FileSystemRights.Synchronize)).ToString();
                        ruleList.Add(string.Format(
                            "{0};{1};{2}",
                            rule.IdentityReference.Value,
                            tempRights,
                            rule.AccessControlType));
                    }
                    break;
                case ObjectType.Directory:
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        string tempRights = rule.FileSystemRights == FileSystemRights.FullControl ?
                            Item.FULLCONTROL :
                            (rule.FileSystemRights & (~FileSystemRights.Synchronize)).ToString();
                        ruleList.Add(string.Format(
                            "{0};{1};{2};{3};{4}",
                            rule.IdentityReference.Value,
                            tempRights,
                            rule.InheritanceFlags,
                            rule.PropagationFlags,
                            rule.AccessControlType));
                    }
                    break;
                case ObjectType.Registry:

                    //  Registry用の処理

                    break;
            }
            return string.Join("/", ruleList);
        }

        #endregion
    }
}
