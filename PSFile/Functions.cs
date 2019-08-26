using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PSFile
{
    class Functions
    {
        //public readonly static Regex reg_Delimitor = new Regex(@",\s*");

        public readonly static Regex reg_Comma = new Regex(@",\s*");

        public static string[] SplitComma(string sourceText)
        {
            return reg_Comma.Split(sourceText);
        }
    }
}
