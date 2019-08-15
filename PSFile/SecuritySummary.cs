using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PSFile
{
    public class SecuritySummary
    {
        enum ObjectType { None, File, Directory, Registry }
        ObjectType _type = ObjectType.None;

        public string Access { get; set; }
        public string Owner { get; set; }
        public bool? Inherited { get; set; }

        public SecuritySummary() { }
        public SecuritySummary(string fileName)
        {
            if (File.Exists(fileName))
            {
                _type = ObjectType.File;
                
            }
            else if (Directory.Exists(fileName))
            {
                _type = ObjectType.Directory;
                GetDirectoryAccess(fileName);
            }
        }
        public SecuritySummary(RegistryKey regKey)
        {
            _type = ObjectType.Registry;
            GetRegistryAccess(regKey);
        }



        /// <summary>
        /// 対象ディレクトリのAccess文字列を取得して返す
        /// </summary>
        /// <param name="directoryName">ディレクトリ名</param>
        /// <returns>Access文字列</returns>
        public string GetDirectoryAccess(string directoryName)
        {
            return GetDirectoryAccess(Directory.GetAccessControl(directoryName));
        }
        public string GetDirectoryAccess(DirectorySecurity security)
        {
            List<string> dirAccessRuleList = new List<string>();
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
            {
                dirAccessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    rule.FileSystemRights,
                    rule.InheritanceFlags,
                    rule.PropagationFlags,
                    rule.AccessControlType));
            }
            return string.Join("/", dirAccessRuleList);
        }


        public string GetOwner(DirectorySecurity security)
        {
            return security.GetOwner(typeof(NTAccount)).Value;
        }

        /// <summary>
        /// 対象レジストリのAccess文字列を取得して返す
        /// </summary>
        /// <param name="regKey">RegistryKeyインスタンス</param>
        /// <returns>Access文字列</returns>
        public string GetRegistryAccess(RegistryKey regKey)
        {
            return GetRegistryAccess(regKey.GetAccessControl());
        }
        public string GetRegistryAccess(RegistrySecurity security)
        {
            List<string> registryAccessRuleList = new List<string>();
            foreach (RegistryAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
            {
                registryAccessRuleList.Add(string.Format(
                    "{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    rule.RegistryRights,
                    rule.InheritanceFlags,
                    rule.PropagationFlags,
                    rule.AccessControlType));
            }
            return string.Join("/", registryAccessRuleList);
        }
    }
}
