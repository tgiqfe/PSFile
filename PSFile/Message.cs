using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.IO;

namespace PSFile
{
    class Message
    {
        /// <summary>
        /// 管理者実行しているかどうかの確認
        /// </summary>
        /// <returns></returns>
        public static bool CheckAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                Console.Error.WriteLine("管理者として実行されていません。所有者変更は失敗する可能性があります。");
            }
            return isAdmin;
        }

        /// <summary>
        /// パスが子/子孫関係にあるかどうかの確認
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public static bool CheckChildItem(string sourcePath, string destinationPath)
        {
            bool isChild = destinationPath.StartsWith(sourcePath.TrimEnd('\\') + "\\");
            if (isChild)
            {
                Console.Error.WriteLine("DestinationがSourceの子or子孫オブジェクトです。");
            }
            return isChild;
        }
    }
}
