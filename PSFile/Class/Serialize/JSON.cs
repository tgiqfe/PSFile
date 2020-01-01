using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace PSFile.Serialize
{
    internal class JSON
    {
        /// <summary>
        /// JsonConvert用デフォルト設定
        /// </summary>
        private static Func<JsonSerializerSettings> JsonConvert_DefaultSettings = null;

        /// <summary>
        /// TextReaderからオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tr"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T Deserialize<T>(TextReader tr)
        {
            return JsonConvert.DeserializeObject<T>(tr.ReadToEnd());
        }

        /// <summary>
        /// オブジェクトをシライライズしてTextWriterの書き込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="tw"></param>
        /// <param name="extension"></param>
        public static void Serialize<T>(object obj, TextWriter tw)
        {
            if (JsonConvert_DefaultSettings == null)
            {
                JsonConvert_DefaultSettings = () =>
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                    };

                    //  enum型を名前で表示
                    settings.Converters.Add(new StringEnumConverter());

                    //  enum型の名前をキャメルケースで表示
                    //settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));

                    return settings;
                };
                JsonConvert.DefaultSettings = JsonConvert_DefaultSettings;
            }
            tw.WriteLine(JsonConvert.SerializeObject(obj));
        }
    }
}
