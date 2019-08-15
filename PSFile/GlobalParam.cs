using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PSFile
{
    class GlobalParam
    {
        public readonly static Regex reg_Delimitor = new Regex(@",\s*");
    }
}
