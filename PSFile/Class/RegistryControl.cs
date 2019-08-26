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

        //  レジストリ値の種類の変換
        public static RegistryValueKind StringToValueKind(string valueKind)
        {
            switch (valueKind.ToLower())
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
        public static string RegistryValueToString(RegistryKey regKey, string name)
        {
            return RegistryValueToString(regKey, name, regKey.GetValueKind(name), true);
        }
        public static string RegistryValueToString(RegistryKey regKey, string name, RegistryValueKind valueKind)
        {
            return RegistryValueToString(regKey, name, valueKind, true);
        }
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

        //  レジストリAccessと文字列の変換
        public static RegistryAccessRule StringToAccessRule(string ruleString)
        {
            if (ruleString.Contains(";"))
            {
                string[] ruleArray = ruleString.Split(';');
                return new RegistryAccessRule(
                    new NTAccount(ruleArray[0]),
                    Enum.TryParse(ruleArray[1], out RegistryRights tempRights) ? tempRights : RegistryRights.ReadKey,
                    Enum.TryParse(ruleArray[2], out InheritanceFlags tempInheritance) ? tempInheritance : InheritanceFlags.ContainerInherit,
                    Enum.TryParse(ruleArray[3], out PropagationFlags tempPropagation) ? tempPropagation : PropagationFlags.None,
                    Enum.TryParse(ruleArray[4], out AccessControlType tempAccessControlType) ? tempAccessControlType : AccessControlType.Allow);
            }
            return null;
        }


        public static string AccessRulesToString(AuthorizationRuleCollection rules)
        {
            List<string> accessArrayList = new List<string>();
            foreach (RegistryAccessRule rule in rules)
            {
                accessArrayList.Add(AccessRulesToString(rule));
            }
            return string.Join("/", accessArrayList);
        }
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
    }
}
