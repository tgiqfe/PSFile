using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile
{
    class RegistryControl
    {
        public static RegistryKey GetRootkey(string rootPath)
        {
            if (rootPath.Contains("\\"))
            {
                rootPath = rootPath.Substring(0, rootPath.IndexOf("\\"));
            }
            switch (rootPath)
            {
                case Item.HKCR:
                case Item.HKCR_:
                case Item.HKEY_CLASSES_ROOT:
                    return Registry.ClassesRoot;
                case Item.HKCU:
                case Item.HKCU_:
                case Item.HKEY_CURRENT_USER:
                    return Registry.CurrentUser;
                case Item.HKLM:
                case Item.HKLM_:
                case Item.HKEY_LOCAL_MACHINE:
                    return Registry.LocalMachine;
                case Item.HKU:
                case Item.HKU_:
                case Item.HKEY_USERS:
                    return Registry.Users;
                case Item.HKCC:
                case Item.HKCC_:
                case Item.HKEY_CURRENT_CONFIG:
                    return Registry.CurrentConfig;
            }
            return null;
        }
        public static RegistryKey GetRegistryKey(string path, bool isCreate, bool writable)
        {
            string keyName = path.Substring(path.IndexOf("\\") + 1);
            return isCreate ?
                GetRootkey(path).CreateSubKey(keyName, writable) :
                GetRootkey(path).OpenSubKey(keyName, writable);
        }

        /// <summary>
        /// レジストリ値の種類の変換
        /// </summary>
        /// <param name="valueKindString">レジストリ種類の文字列</param>
        /// <returns>RegistryValueKind</returns>
        public static RegistryValueKind StringToValueKind(string valueKindString)
        {
            switch (valueKindString.ToLower())
            {
                case "reg_sz": return RegistryValueKind.String;
                case "reg_binary": return RegistryValueKind.Binary;
                case "reg_dword": return RegistryValueKind.DWord;
                case "reg_qword": return RegistryValueKind.QWord;
                case "reg_multi_sz": return RegistryValueKind.MultiString;
                case "reg_expand_sz": return RegistryValueKind.ExpandString;
                case "reg_none": return RegistryValueKind.None;
            }
            return RegistryValueKind.String;
        }

        /// <summary>
        /// レジストリ値の種類の変換
        /// </summary>
        /// <param name="valueKind">RegistryValueKind</param>
        /// <returns>レジストリ種類の文字列</returns>
        public static string ValueKindToString(RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String: return Item.REG_SZ;
                case RegistryValueKind.Binary: return Item.REG_BINARY;
                case RegistryValueKind.DWord: return Item.REG_DWORD;
                case RegistryValueKind.QWord: return Item.REG_QWORD;
                case RegistryValueKind.MultiString: return Item.REG_MULTI_SZ;
                case RegistryValueKind.ExpandString: return Item.REG_EXPAND_SZ;
                case RegistryValueKind.None: return Item.REG_NONE;
            }
            return Item.REG_SZ;
        }

        /// <summary>
        /// レジストリ値を文字列に変換
        /// </summary>
        /*
        public static string RegistryValueToString(RegistryKey regKey, string name)
        {
            return RegistryValueToString(regKey, name, regKey.GetValueKind(name), true);
        }
        public static string RegistryValueToString(RegistryKey regKey, string name, RegistryValueKind valueKind)
        {
            return RegistryValueToString(regKey, name, valueKind, true);
        }
        */
        public static string RegistryValueToString(RegistryKey regKey, string name, RegistryValueKind valueKind, bool noResolv)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                    return regKey.GetValue(name) as string;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return regKey.GetValue(name).ToString();
                case RegistryValueKind.ExpandString:
                    return noResolv ?
                        regKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames) as string :
                        regKey.GetValue(name) as string;
                case RegistryValueKind.Binary:
                    return BitConverter.ToString(regKey.GetValue(name) as byte[]).Replace("-", "").ToUpper();
                case RegistryValueKind.MultiString:
                    return string.Join("\\0", regKey.GetValue(name) as string[]);
                case RegistryValueKind.None:
                default:
                    return null;
            }
        }

        //  REG_BINARYの値の変換
        public static byte[] StringToRegBinary(string val)
        {
            if (Regex.IsMatch(val, @"^[0-9a-fA-F]+$"))
            {
                List<byte> tempBytes = new List<byte>();
                for (int i = 0; i < val.Length / 2; i++)
                {
                    tempBytes.Add(Convert.ToByte(val.Substring(i * 2, 2), 16));
                }
                return tempBytes.ToArray();
            }
            return null;
        }

        /// <summary>
        /// 文字列からFileSystemAccessのListを取得
        /// </summary>
        /// <param name="ruleString">Access文字列</param>
        /// <returns></returns>
        public static List<RegistryAccessRule> StringToAccessRules(string ruleString)
        {
            List<RegistryAccessRule> ruleList = new List<RegistryAccessRule>();
            foreach (string ruleStr in ruleString.Split('/'))
            {
                string[] fields = ruleStr.Split(';');
                ruleList.Add(new RegistryAccessRule(
                    new NTAccount(fields[0]),
                    Enum.TryParse(fields[1], out RegistryRights tempRights) ? tempRights : RegistryRights.ReadKey,
                    Enum.TryParse(fields[2], out InheritanceFlags tempInheritance) ? tempInheritance : InheritanceFlags.ContainerInherit,
                    Enum.TryParse(fields[3], out PropagationFlags tempPropagation) ? tempPropagation : PropagationFlags.None,
                    Enum.TryParse(fields[4], out AccessControlType tempAccessControlType) ? tempAccessControlType : AccessControlType.Allow));
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
            foreach (RegistryAccessRule rule in rules)
            {
                accessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    rule.RegistryRights.ToString(),
                    rule.InheritanceFlags.ToString(),
                    rule.PropagationFlags.ToString(),
                    rule.AccessControlType.ToString()));

                //accessRuleList.Add(AccessRulesToString(rule));
            }
            return string.Join("/", accessRuleList);
        }
        /*
        public static string AccessRulesToString(RegistryAccessRule rule)
        {
            string[] accessArray = new string[5];
            accessArray[0] = rule.IdentityReference.Value;          //  Account
            accessArray[1] = rule.RegistryRights.ToString();        //  Rights
            accessArray[2] = rule.InheritanceFlags.ToString();      //  InheritanceFlags
            accessArray[3] = rule.PropagationFlags.ToString();      //  PropagationFlags
            accessArray[4] = rule.AccessControlType.ToString();     //  AccessControlType
            return string.Join(";", accessArray);
        }
        */
    }
}
