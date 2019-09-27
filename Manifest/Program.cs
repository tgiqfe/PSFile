using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Manifest
{
    class Program
    {
        const string PROJECT_NAME = "PSFile";

        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string debugDir = string.Format(@"..\..\..\{0}\bin\Debug", PROJECT_NAME);
            string releaseDir = string.Format(@"..\..\..\{0}\bin\Release", PROJECT_NAME);
            string moduleDir = string.Format(@"..\..\..\{0}\bin\{0}", PROJECT_NAME);

            PSD1.Create(PROJECT_NAME, debugDir);
            PSD1.Create(PROJECT_NAME, releaseDir);
            PSM1.Create(PROJECT_NAME, debugDir);
            PSM1.Create(PROJECT_NAME, releaseDir);

            if (Directory.Exists(releaseDir))
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.Arguments = string.Format(
                        "\"{0}\" \"{1}\" /MIR /E /XJD /XJF", releaseDir, moduleDir);
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
    }
}
