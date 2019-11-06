/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;
using YamlDotNet;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.IO;

namespace PSFile
{
    class DataSerializer
    {
        //  静的パラメータ
        private static XmlSerializerNamespaces XmlSerializer_Namespaces = null;
        private static Func<JsonSerializerSettings> JsonConvert_DefaultSettings = null;

        //  デシリアライズ
        public static T Deserialize<T>(string fileName) where T : class, new()
        {
            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    return Deserialize<T>(sr, Path.GetExtension(fileName));
                }
            }
            catch { }
            //catch (Exception e) { Console.WriteLine(e.ToString()); }
            //catch (Exception e) { Console.WriteLine(e.Message); }
            return null;
        }
        public static T Deserialize<T>(TextReader tr, string extension) where T : class, new()
        {
            switch (extension)
            {
                case ".xml":
                    return new XmlSerializer(typeof(T)).Deserialize(tr) as T;
                case ".json":
                    return JsonConvert.DeserializeObject<T>(tr.ReadToEnd());
                case ".yml":
                    return new Deserializer().Deserialize<T>(tr) as T;
            }
            return null;
        }

        //  シリアライズ
        public static void Serialize<T>(object obj, string fileName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    Serialize<T>(obj, sw, Path.GetExtension(fileName));
                }
            }
            catch { }
            //catch (Exception e) { Console.WriteLine(e.ToString()); }
            //catch (Exception e) { Console.WriteLine(e.Message); }
        }
        public static void Serialize<T>(object obj, TextWriter tw, string extension)
        {
            if (!extension.StartsWith(".")) { extension = "." + extension; }
            switch (extension.ToLower())
            {
                case ".xml":
                    if (XmlSerializer_Namespaces == null)
                    {
                        XmlSerializer_Namespaces = new XmlSerializerNamespaces(
                            new XmlQualifiedName[1] { XmlQualifiedName.Empty });
                    }
                    new XmlSerializer(typeof(T)).Serialize(tw, obj, XmlSerializer_Namespaces);
                    break;
                case ".json":
                    if (JsonConvert_DefaultSettings == null)
                    {
                        JsonConvert_DefaultSettings = () =>
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings()
                            {
                                Formatting = Newtonsoft.Json.Formatting.Indented,
                                NullValueHandling = NullValueHandling.Ignore,
                            };
                            //settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                            settings.Converters.Add(new StringEnumConverter());
                            return settings;
                        };
                        JsonConvert.DefaultSettings = JsonConvert_DefaultSettings;
                    }
                    tw.WriteLine(JsonConvert.SerializeObject(obj));
                    break;
                case ".yml":
                    new Serializer().Serialize(tw, obj);
                    break;
            }
        }
    }
}
*/