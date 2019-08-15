using System;
using System.IO;
using System.Reflection;

namespace PSFile
{
    class EmbeddedResource
    {
        public static void Expand(string outputDir)
        {
            //  現バージョン以外で展開済みの場合、フォルダーごと削除
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            string versionFile = Path.Combine(outputDir, string.Format("{0}_{1}_{2}_{3}.txt",
                    ver.Major, ver.Minor, ver.Build, ver.Revision));
            if (!File.Exists(versionFile))
            {
                if (Directory.Exists(outputDir)) { Directory.Delete(outputDir, true); }
                Directory.CreateDirectory(outputDir);

                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                int excludeLength = (executingAssembly.GetName().Name + ".Embedded.").Length;
                foreach (string resourcePath in executingAssembly.GetManifestResourceNames())
                {
                    string outputFile = Path.Combine(outputDir, resourcePath.Substring(excludeLength));
                    using (Stream stream = executingAssembly.GetManifestResourceStream(resourcePath))
                    using (BinaryReader reader = new BinaryReader(stream))
                    using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outputFile)))
                    {
                        writer.Write(reader.ReadBytes((int)stream.Length), 0, (int)stream.Length);
                    }
                }
                File.Create(versionFile).Close();
            }
        }
    }
}
