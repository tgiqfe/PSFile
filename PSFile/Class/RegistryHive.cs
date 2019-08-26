using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PSFile
{
    class RegistryHive
    {
        //  フィールドパラメータ
        private static int _myToken;
        private static LUID _restoreLuid;
        private static LUID _backupLuid;

        //  定数
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const int TOKEN_QUERY = 0x00000008;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_PRIV_COUNT = 1;
        private const string SE_RESTORE_NAME = "SeRestorePrivilege";
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";
        private const uint HKEY_USERS = 0x80000003;

        //  関数
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIV
        {
            public LUID Luid;
            public int Attr;
            public int Count;
            public TOKEN_PRIV(LUID luid)
            {
                this.Luid = luid;
                this.Attr = SE_PRIVILEGE_ENABLED;
                this.Count = TOKEN_PRIV_COUNT;
            }
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool OpenProcessToken(int processHandle, int desiredAccess, ref int tokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetCurrentProcess();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool AdjustTokenPrivileges(
            int tokenHandle,
            int disablePrivs,
            [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIV newState,
            int buffereLength,
            int previousState,
            int returnlength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        //  ハイブファイルを指定してロード
        public static void Load(string keyName, string hiveFile)
        {
            if (AdjustToken())
            {
                RegLoadKey(HKEY_USERS, keyName, hiveFile);
            }
        }

        //  指定したキーをアンロード
        public static void UnLoad(string keyName)
        {
            if (AdjustToken())
            {
                RegUnLoadKey(HKEY_USERS, keyName);
            }
        }

        //  プロセストークンに権限を追加
        private static bool AdjustToken()
        {
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref _myToken))
            { Console.WriteLine("OpenProcess：失敗"); return false; }

            if (!LookupPrivilegeValue(null, SE_RESTORE_NAME, ref _restoreLuid))
            { Console.WriteLine("LookupPrivilege：失敗"); return false; }

            if (!LookupPrivilegeValue(null, SE_BACKUP_NAME, ref _backupLuid))
            { Console.WriteLine("LookupPrivilege：失敗"); return false; }

            TOKEN_PRIV tokenPriv1 = new TOKEN_PRIV(_restoreLuid);
            TOKEN_PRIV tokenPriv2 = new TOKEN_PRIV(_backupLuid);

            if (!AdjustTokenPrivileges(_myToken, 0, ref tokenPriv1, 1024, 0, 0))
            { Console.WriteLine("AdjustToken：失敗"); return false; }

            if (!AdjustTokenPrivileges(_myToken, 0, ref tokenPriv2, 1024, 0, 0))
            { Console.WriteLine("AdjustToken：失敗"); return false; }

            return true;
        }
    }
}
