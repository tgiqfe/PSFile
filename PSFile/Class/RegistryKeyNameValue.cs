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

        public RegistryKeyNameValue() { }

        /// <summary>
        /// レジストリキーパスを格納
        /// </summary>
        /// <param name="registryKey"></param>
        public void AddKey(string registryKey)
        {
            this.RegistryKey = registryKey;
        }

        /// <summary>
        /// レジストリキーパスを格納。RegistyKeyクラスのままでもOK
        /// </summary>
        /// <param name="regKey"></param>
        public void AddKey(RegistryKey regKey)
        {
            this.RegistryKey = regKey.ToString();
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
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"[{RegistryKey}]\r\n");
            foreach (KeyValuePair<string, string> pair in RegistryValue)
            {
                sb.AppendLine(string.Format("{0}=\"{1}\"",
                    string.IsNullOrEmpty(pair.Key) ? "(既定)" : $"\"{pair.Key}\"",
                    pair.Value));
            }
            return sb.ToString();
        }
    }
}
