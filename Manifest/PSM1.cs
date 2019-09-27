using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Manifest
{
    class PSM1
    {
        const string EXTENSION = ".psm1";
        public static void Create(string projectName, string outputDir)
        {
            string dllFile = Path.Combine(outputDir, projectName + ".dll");
            string outputFile = Path.Combine(outputDir, projectName + EXTENSION);
            if (!File.Exists(dllFile)) { return; }
            using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                sw.WriteLine();
            }
        }
    }
}
