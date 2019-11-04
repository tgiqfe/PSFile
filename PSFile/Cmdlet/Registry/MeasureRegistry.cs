using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PSFile.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Measure, "Registry")]
    public class MeasureRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }


    }
}
