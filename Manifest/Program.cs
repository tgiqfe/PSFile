using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manifest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 4)
            {
                PSD1.Create(args[0], args[1], args[2]);
                PSM1.Create(args[0], args[1], args[3]);
            }
        }
    }
}
