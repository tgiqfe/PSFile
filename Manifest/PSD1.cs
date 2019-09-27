using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;

namespace Manifest
{
    class PSD1
    {
        const string EXTENSION = ".psd1";

        public static void Create(string projectName, string outputDir)
        {
            string dllFile = Path.Combine(outputDir, projectName + ".dll");
            string outputFile = Path.Combine(outputDir, projectName + EXTENSION);
            if (!File.Exists(dllFile)) { return; }

            List<string> CmdletsToExport = new List<string>();
            string cmdletDir = @"..\..\..\" + projectName + @"\Cmdlet";
            foreach (string csFile in Directory.GetFiles(cmdletDir, "*.cs", SearchOption.AllDirectories))
            {
                using (StreamReader sr = new StreamReader(csFile, Encoding.UTF8))
                {
                    string readLine = "";
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(readLine, @"^\s*\[Cmdlet\(Verbs"))
                        {
                            string cmdPre = readLine.Substring(
                                readLine.IndexOf(".") + 1, readLine.IndexOf(",") - readLine.IndexOf(".") - 1);
                            string cmdSuf = readLine.Substring(
                                readLine.IndexOf("\"") + 1, readLine.LastIndexOf("\"") - readLine.IndexOf("\"") - 1);
                            CmdletsToExport.Add(cmdPre + "-" + cmdSuf);
                        }
                    }
                }
            }

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(dllFile);

            string RootModule = Path.GetFileName(dllFile);
            string ModuleVersion = fvi.FileVersion;
            string Guid = "663118e5-b580-4f06-afd7-e9ec4e4020af";
            string Author = "q";
            string CompanyName = "q";
            string Copyright = fvi.LegalCopyright;
            string Description = "File/Directory/Registry IO by PowerShell";

            string manifestString = string.Format(@"@{{
RootModule = ""{0}""
ModuleVersion = ""{1}""
GUID = ""{2}""
Author = ""{3}""
CompanyName = ""{4}""
Copyright = ""{5}""
Description = ""{6}""
CmdletsToExport = @(
""{7}""
)
}}",
RootModule, ModuleVersion, Guid, Author, CompanyName, Copyright, Description,
string.Join("\", \"", CmdletsToExport)
);
            using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                sw.WriteLine(manifestString);
            }
        }
    }
}
