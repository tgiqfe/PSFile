﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Manifest
{
    class PSM1
    {
        public static void Create(string dllFile, string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                //  生成する内容をここに
            }
        }
    }
}
