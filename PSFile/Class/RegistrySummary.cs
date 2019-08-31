using Microsoft.Win32;
using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PSFile
{
    class RegistrySummary
    {
        //  クラスパラメータ
        public string Name { get; set; }
        public string Path { get; set; }
        public string Owner { get; set; }
        public string Access { get; set; }
        public bool? Inherited { get; set; }
        public SerializableDictionary<string, string> Values { get; set; }

        //  コンストラクタ
        public RegistrySummary() { }
        public RegistrySummary(RegistryKey regKey)
        {
            this.Path = regKey.Name;
            this.Name = System.IO.Path.GetFileName(this.Path);
        }
        public RegistrySummary(RegistryKey regKey, bool isLoad)
        {
            this.Path = regKey.Name;
            this.Name = System.IO.Path.GetFileName(this.Path);
            if (isLoad)
            {
                LoadSecurity(regKey);
                LoadValues(regKey);
            }
        }
        public RegistrySummary(RegistryKey regKey, bool IgnoreSecurity, bool IgnoreValues)
        {
            this.Path = regKey.Name;
            this.Name = System.IO.Path.GetFileName(this.Path);
            if (!IgnoreSecurity) { LoadSecurity(regKey); }
            if (!IgnoreValues) { LoadValues(regKey); }
        }
        public RegistrySummary(RegistryKey regKey, int rootKeyLength, bool IgnoreSecurity, bool IgnoreValues)
        {
            this.Path = regKey.Name.Substring(rootKeyLength);
            this.Name = System.IO.Path.GetFileName(regKey.Name);
            if (!IgnoreSecurity) { LoadSecurity(regKey); }
            if (!IgnoreValues) { LoadValues(regKey); }
        }

        /// <summary>
        /// セキュリティ情報を取得
        /// </summary>
        public void LoadSecurity()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                LoadSecurity(regKey);
            }
        }
        private void LoadSecurity(RegistryKey regKey)
        {
            RegistrySecurity security = regKey.GetAccessControl();
            this.Owner = security.GetOwner(typeof(NTAccount)).Value;
            this.Access = RegistryControl.AccessRulesToString(security.GetAccessRules(true, false, typeof(NTAccount)));
            this.Inherited = !security.AreAccessRulesProtected;
        }

        /// <summary>
        /// 値を取得
        /// </summary>
        public void LoadValues()
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(Path, false, false))
            {
                LoadValues(regKey);
            }
        }
        private void LoadValues(RegistryKey regKey)
        {
            Values = new SerializableDictionary<string, string>();

            string[] valueNames = regKey.GetValueNames();
            Array.Sort(valueNames, StringComparer.OrdinalIgnoreCase);
            foreach (string name in valueNames)
            {
                RegistryValueKind valueKind = regKey.GetValueKind(name);

                Values[name] = RegistryControl.ValueKindToString(valueKind) + ":" +
                    RegistryControl.RegistryValueToString(regKey, name, valueKind, true);
            }
        }

        /// <summary>
        /// PowerShell用のレジストリキーパスを取得
        /// </summary>
        /// <returns></returns>
        public string GetPSPath()
        {
            string keyName = Path.Substring(Path.IndexOf("\\") + 1);
            switch (RegistryControl.GetRootkey(Path).ToString())
            {
                case Item.HKEY_CLASSES_ROOT: return System.IO.Path.Combine(Item.HKCR_, keyName);
                case Item.HKEY_CURRENT_USER: return System.IO.Path.Combine(Item.HKCU_, keyName);
                case Item.HKEY_LOCAL_MACHINE: return System.IO.Path.Combine(Item.HKLM_, keyName);
                case Item.HKEY_USERS: return System.IO.Path.Combine(Item.HKU_, keyName);
                case Item.HKEY_CURRENT_CONFIG: return System.IO.Path.Combine(Item.HKCC_, keyName);
            }
            return string.Empty;
        }
    }
}
