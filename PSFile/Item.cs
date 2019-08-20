using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PSFile
{
    class Item
    {
        //  レジストリ値の種類
        public const string REG_SZ = "REG_SZ";
        public const string REG_BINARY = "REG_BINARY";
        public const string REG_DWORD = "REG_DWORD";
        public const string REG_QWORD = "REG_QWORD";
        public const string REG_MULTI_SZ = "REG_MULTI_SZ";
        public const string REG_EXPAND_SZ = "REG_EXPAND_SZ";
        public const string REG_NONE = "REG_NONE";

        //  レジストリルートキー名
        public const string HKEY_CLASSES_ROOT = "HKEY_CLASSES_ROOT";
        public const string HKEY_CURRENT_USER = "HKEY_CURRENT_USER";
        public const string HKEY_LOCAL_MACHINE = "HKEY_LOCAL_MACHINE";
        public const string HKEY_USERS = "HKEY_USERS";
        public const string HKEY_CURRENT_CONFIG = "HKEY_CURRENT_CONFIG";
        public const string HKCR = "HKCR";
        public const string HKCU = "HKCU";
        public const string HKLM = "HKLM";
        public const string HKU = "HKU";
        public const string HKCC = "HKCC";
        public const string HKCR_ = "HKCR:";
        public const string HKCU_ = "HKCU:";
        public const string HKLM_ = "HKLM:";
        public const string HKU_ = "HKU:";
        public const string HKCC_ = "HKCC:";

        //  継承設定の変更方法
        public const string NONE = "None";
        public const string ENABLE = "Enable";
        public const string DISABLE = "Disable";
        public const string REMOVE = "Remove";

        public const string CONTAINERINHERIT = "ContainerInherit";
        public const string OBJECTINHERIT = "ObjectInherit";
        public const string INHERITONLY = "InheritOnly ";
        public const string NOPROPAGATEINHERIT = "NoPropagateInherit";

        //  属性
        public const string ARCHIVE = "Archive";
        public const string COMPRESSED = "Compressed";
        public const string DEVICE = "Device";
        public const string DIRECTORY = "Directory";
        public const string ENCRYPTED = "Encrypted";
        public const string HIDDEN = "Hidden";
        public const string INTEGRITYSTREAM = "IntegrityStream";
        public const string NORMAL = "Normal";
        public const string NOSCRUBDATA = "NoScrubData";
        public const string NOTCONTENTINDEXED = "NotContentIndexed";
        public const string OFFLINE = "Offline";
        public const string READONLY = "ReadOnly";
        public const string REPARSEPOINT = "ReparsePoint";
        public const string SPARSEFILE = "SparseFile";
        public const string SYSTEM = "System";
        public const string TEMPORARY = "Temporary";

        //  アクセス権種別
        public const string APPENDDATA = "AppendData";
        public const string CHANGEPERMISSIONS = "ChangePermissions";
        public const string CREATEDIRECTORIES = "CreateDirectories";
        public const string CREATEFILES = "CreateFiles";
        public const string CREATELINK = "CreateLink";
        public const string CREATESUBKEY = "CreateSubKey";
        public const string DELETE = "Delete";
        public const string DELETESUBDIRECTORIESANDFILES = "DeleteSubdirectoriesAndFiles";
        public const string ENUMERATESUBKEYS = "EnumerateSubKeys";
        public const string EXECUTEFILE = "ExecuteFile";
        public const string EXECUTEKEY = "ExecuteKey";
        public const string FULLCONTROL = "FullControl";
        public const string LISTDIRECTORY = "ListDirectory";
        public const string MODIFY = "Modify";
        public const string NOTIFY = "Notify";
        public const string QUERYVALUES = "QueryValues";
        public const string READ = "Read";
        public const string READANDEXECUTE = "ReadAndExecute";
        public const string READATTRIBUTES = "ReadAttributes";
        public const string READDATA = "ReadData";
        public const string READEXTENDEDATTRIBUTES = "ReadExtendedAttributes";
        public const string READKEY = "ReadKey";
        public const string READPERMISSIONS = "ReadPermissions";
        public const string SETVALUE = "SetValue";
        public const string SYNCHRONIZE = "Synchronize";
        public const string TAKEOWNERSHIP = "TakeOwnership";
        public const string TRAVERSE = "Traverse";
        public const string WRITE = "Write";
        public const string WRITEATTRIBUTES = "WriteAttributes";
        public const string WRITEDATA = "WriteData";
        public const string WRITEEXTENDEDATTRIBUTES = "WriteExtendedAttributes";
        public const string WRITEKEY = "WriteKey";

        //  アクセスコントロールタイプ
        public const string ALLOW = "Allow";
        public const string DENY = "Deny";

        //  出力データタイプ
        public const string REG = "Reg";
        public const string DAT = "Dat";
        public const string XML = "Xml";
        public const string JSON = "Json";
        public const string YML = "Yml";

        //  テスト対象
        //public const string KEY = "Key";
        public const string PATH = "Path";
        public const string NAME = "Name";
        public const string VALUE = "Value";
        public const string TYPE = "Type";
        public const string OWNER = "Owner";
        public const string ACCESS = "Access";
        public const string INHERIT = "Inherit";
        public const string HASH = "Hash";
        public const string ATTRIBUTE = "Attribute";
        public const string SIZE = "Size";
        public const string SECURITYBLOCK = "SecurityBlock";

        //  テストモード
        public const string CONTAIN = "Contain";
        public const string MATCH = "Match";

        #region CheckCase
        private static readonly string[] fields = 
            typeof(Item).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(null) as string).ToArray();

        /// <summary>
        /// 大文字/小文字解決
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string CheckCase(string val)
        {
            if (val == null) { return null; }
            string[] valu = GlobalParam.reg_Delimitor.Split(val);
            for (int i = 0; i < valu.Length; i++)
            {
                string matchVal = fields.FirstOrDefault(x => x.Equals(valu[i], StringComparison.OrdinalIgnoreCase));
                if (matchVal != null)
                {
                    valu[i] = matchVal;
                }
            }
            return string.Join(", ", valu);
        }
        public static string CheckCase(string[] valu)
        {
            if (valu == null) { return null; }
            for (int i = 0; i < valu.Length; i++)
            {
                valu[i] = Item.CheckCase(valu[i]);
            }
            return string.Join(", ", valu);
        }
        #endregion
    }
}
