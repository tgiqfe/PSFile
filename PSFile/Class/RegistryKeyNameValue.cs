using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSFile.Serialize;
using Microsoft.Win32;

namespace PSFile
{
    public class RegistryKeyNameValue
    {
        public string RegistryKey { get; set; }
        public SerializableDictionary<string, string> RegistryValue { get; set; }
        public bool Enabled { get; set; }

        public RegistryKeyNameValue() { }

        /// <summary>
        /// レジストリキーパスを格納
        /// </summary>
        /// <param name="registryKey"></param>
        public void AddKey(string registryKey)
        {
            this.RegistryKey = registryKey;
            Enabled = true;
        }

        /// <summary>
        /// レジストリキーパスを格納。RegistyKeyクラスのままでもOK
        /// </summary>
        /// <param name="regKey"></param>
        public void AddKey(RegistryKey regKey)
        {
            this.RegistryKey = regKey.ToString();
            Enabled = true;
        }

        /// <summary>
        /// レジストリ値を格納。
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="registryName"></param>
        /// <param name="registryValue"></param>
        public void AddValue(string registryKey, string registryName, string registryValue)
        {
            this.RegistryKey = registryKey;
            if (RegistryValue == null)
            {
                RegistryValue = new SerializableDictionary<string, string>();
            }
            RegistryValue[registryName] = registryValue;
            Enabled = true;
        }

        /// <summary>
        /// レジストリ値を格納。RegistyKeyクラスのままでもOK
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="registryName"></param>
        /// <param name="registryValue"></param>
        public void AddValue(RegistryKey regKey, string registryName, string registryValue)
        {
            this.RegistryKey = regKey.ToString();
            if (RegistryValue == null)
            {
                RegistryValue = new SerializableDictionary<string, string>();
            }
            RegistryValue[registryName] = registryValue;
            Enabled = true;
        }
    }
}
