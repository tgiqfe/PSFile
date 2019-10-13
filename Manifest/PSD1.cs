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

            List<string> CmdletsToExportList = new List<string>();
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
                            CmdletsToExportList.Add(cmdPre + "-" + cmdSuf);
                        }
                    }
                }
            }
            string CmdletsToExport = "\"" + string.Join("\", \"", CmdletsToExportList) + "\"";
            int cursor = 0;
            int commaCount = 0;
            while ((cursor = CmdletsToExport.IndexOf(",", cursor)) >= 0)
            {
                cursor += 2;
                commaCount++;
                if ((commaCount % 4) == 0)
                {
                    CmdletsToExport = CmdletsToExport.Insert(cursor, "\r\n");
                }
            }

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(dllFile);

            string RootModule = Path.GetFileName(dllFile);
            string ModuleVersion = fvi.FileVersion;
            string Guid = "75e60d76-7594-4f1b-af01-a2629646e1ec";
            string Author = "q";
            string CompanyName = "q";
            string Copyright = fvi.LegalCopyright;
            string Description = "Run enumerated script";

            string manifestString = string.Format(@"@{{
RootModule = ""{0}""
ModuleVersion = ""{1}""
GUID = ""{2}""
Author = ""{3}""
CompanyName = ""{4}""
Copyright = ""{5}""
Description = ""{6}""
CmdletsToExport = @({7})
}}",
RootModule, ModuleVersion, Guid, Author, CompanyName, Copyright, Description,
CmdletsToExport
);
            using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                sw.WriteLine(manifestString);
            }
        }
    }
}
