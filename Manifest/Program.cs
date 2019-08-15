using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manifest
{
    class Program
    {
        /// <summary>
        /// マニフェストファイル自動生成用
        /// </summary>
        /// <param name="args">
        /// 引数1 ⇒ モジュールDLLファイルへのパス
        /// 引数2 ⇒ .psd1ファイルへのパス
        /// 引数3 ⇒ .psm1ファイルへのパス
        /// </param>
        static void Main(string[] args)
        {
            if (args.Length >= 3)
            {
                PSD1.Create(args[0], args[1]);
                PSM1.Create(args[0], args[2]);
            }
        }
    }
}
