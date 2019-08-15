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
        public static bool CheckAdmin()
        {
            //  管理者実行確認
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                Console.Error.WriteLine("管理者として実行されていません。所有者変更は失敗する可能性があります。");
            }
            return isAdmin;
        }
    }
}
