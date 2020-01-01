using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YamlDotNet;
using YamlDotNet.Serialization;

namespace PSFile.Serialize
{
    internal class YML
    {
        /// <summary>
        /// TextReaderからオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tr"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T Deserialize<T>(TextReader tr) where T: class
        {
            return new Deserializer().Deserialize<T>(tr) as T;
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
            new Serializer().Serialize(tw, obj);
        }
    }
}
