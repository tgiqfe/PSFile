using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;

namespace PSFile.Serialize
{
    internal class XML
    {
        /// <summary>
        /// XMLシリアライズ用の、空の名前空間
        /// </summary>
        private static XmlSerializerNamespaces XmlSerializer_Namespaces = null;

        /// <summary>
        /// TextReaderからオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tr"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T Deserialize<T>(TextReader tr) where T : class
        {
            return new XmlSerializer(typeof(T)).Deserialize(tr) as T;
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
            if (XmlSerializer_Namespaces == null)
            {
                XmlSerializer_Namespaces = new XmlSerializerNamespaces(
                    new XmlQualifiedName[1] { XmlQualifiedName.Empty });
            }
            new XmlSerializer(typeof(T)).Serialize(tw, obj, XmlSerializer_Namespaces);
        }
    }
}
