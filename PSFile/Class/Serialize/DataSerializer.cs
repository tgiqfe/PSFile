using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PSFile.Serialize
{
    class DataSerializer
    {
        #region Deserialize

        /// <summary>
        /// ファイルからオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string fileName) where T : class
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    return Deserialize<T>(sr, Enum.TryParse(
                        Path.GetExtension(fileName).TrimStart('.'), true, out DataType extension) ?
                        extension : DataType.None);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 文字列からオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceText"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string sourceText, DataType extension) where T : class
        {
            if (!string.IsNullOrEmpty(sourceText))
            {
                using (StringReader sr = new StringReader(sourceText))
                {
                    return Deserialize<T>(sr, extension);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// TextStreamからオブジェクトにデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tr"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T Deserialize<T>(TextReader tr, DataType extension) where T : class
        {
            try
            {
                switch (extension)
                {
                    case DataType.Json:
                        return JSON.Deserialize<T>(tr);
                    case DataType.Xml:
                        return XML.Deserialize<T>(tr);
                    case DataType.Yml:
                        return YML.Deserialize<T>(tr);
                }
            }
            catch
            {
                //return new T();
                //  ↑のようにする場合は、where T : new() が必要。
                //  但し、この場合は配列を T として指定できなくなるので注意
                return null;
            }
            return null;
        }

        #endregion

        #region Serialize

        /// <summary>
        /// オブジェクトをシリアライズしてファイルに出力
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        public static void Serialize<T>(object obj, string fileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                Serialize<T>(obj, sw, Enum.TryParse(
                    Path.GetExtension(fileName).TrimStart('.'), true, out DataType extension) ?
                    extension : DataType.None);
            }
        }

        /// <summary>
        /// オブジェクトをシリアライズして文字列として返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string Serialize<T>(object obj, DataType extension)
        {
            using (StringWriter sw = new StringWriter())
            {
                Serialize<T>(obj, sw, extension);
                return sw.ToString();
            }
        }

        /// <summary>
        /// オブジェクトをシリアライズしてTextStreamに出力
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="tw"></param>
        /// <param name="extension"></param>
        public static void Serialize<T>(object obj, TextWriter tw, DataType extension)
        {
            if (obj != null)
            {
                switch (extension)
                {
                    case DataType.Json:
                        JSON.Serialize<T>(obj, tw);
                        break;
                    case DataType.Xml:
                        XML.Serialize<T>(obj, tw);
                        break;
                    case DataType.Yml:
                        YML.Serialize<T>(obj, tw);
                        break;
                }
            }
        }

        #endregion
    }
}
