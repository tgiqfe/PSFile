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

        public const string CONTAINERINHERIT = "ContainerInherit";      //  略称：CI
        public const string OBJECTINHERIT = "ObjectInherit";            //  略称：OI
        public const string INHERITONLY = "InheritOnly ";               //  略称：IO
        public const string NOPROPAGATEINHERIT = "NoPropagateInherit";

        //  属性
        public const string ARCHIVE = "Archive";                        //  略称：A
        public const string COMPRESSED = "Compressed";
        public const string DEVICE = "Device";
        public const string DIRECTORY = "Directory";
        public const string ENCRYPTED = "Encrypted";
        public const string HIDDEN = "Hidden";                          //  略称：H
        public const string INTEGRITYSTREAM = "IntegrityStream";        //  略称：V
        public const string NORMAL = "Normal";
        public const string NOSCRUBDATA = "NoScrubData";                //  略称：X
        public const string NOTCONTENTINDEXED = "NotContentIndexed";    //  略称：I
        public const string OFFLINE = "Offline";                        //  略称：O
        public const string READONLY = "ReadOnly";                      //  略称：R
        public const string REPARSEPOINT = "ReparsePoint";
        public const string SPARSEFILE = "SparseFile";
        public const string SYSTEM = "System";                          //  略称：S
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
        public const string PATH = "Path";
        public const string NAME = "Name";
        public const string VALUE = "Value";
        public const string TYPE = "Type";
        public const string OWNER = "Owner";
        public const string ACCESS = "Access";
        public const string ACCOUNT = "Account";
        public const string INHERITED = "Inherited";
        public const string HASH = "Hash";
        public const string ATTRIBUTES = "Attributes";
        public const string SIZE = "Size";
        public const string SECURITYBLOCK = "SecurityBlock";
        public const string CREATIONTIME = "CreationTime";
        public const string LASTWRITETIME = "LastWriteTime";
        public const string LASTACCESSTIME = "LastAccessTime";

        //  テストモード
        public const string CONTAIN = "Contain";
        public const string MATCH = "Match";

        //  アプリケーション名
        public const string APPLICATION_NAME = "PSFile";

        #region CheckCase
        private static readonly string[] fields =
            typeof(Item).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(null) as string).ToArray();
        private static readonly Dictionary<string, string> simpleFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CI",  CONTAINERINHERIT },
            { "OI",  OBJECTINHERIT },
            { "IO", INHERITONLY },
            { "A", ARCHIVE },
            { "H", HIDDEN },
            { "V", INTEGRITYSTREAM },
            { "X", NOSCRUBDATA },
            { "I", NOTCONTENTINDEXED },
            { "O", OFFLINE },
            { "R", READONLY },
            { "S", SYSTEM }
        };

        /// <summary>
        /// 大文字/小文字解決
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string CheckCase(string val)
        {
            if (val == null) { return null; }
            List<string> valueList = new List<string>();
            foreach (string valuu in Functions.SplitComma(val))
            {
                string matchVal = fields.FirstOrDefault(x => x.Equals(valuu, StringComparison.OrdinalIgnoreCase));
                if(matchVal != null)
                {
                    valueList.Add(matchVal);
                }
                else if (simpleFields.ContainsKey(valuu))
                {
                    valueList.Add(simpleFields[valuu]);
                }
            }
            return string.Join(", ", valueList);
        }
        public static string CheckCase(string[] valu)
        {
            if (valu == null) { return null; }
            List<string> valueList = new List<string>();
            foreach (string valuu in valu)
            {
                valueList.Add(Item.CheckCase(valuu));
            }
            return string.Join(", ", valueList);
        }
        #endregion
    }
}
