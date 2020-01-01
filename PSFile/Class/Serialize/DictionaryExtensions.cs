using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSFile.Serialize
{
    /// <summary>
    /// Dictionary用拡張クラス
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// キーが無い場合にデフォルト値を返す
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue)
        {
            if (source == null) { source = new Dictionary<TKey, TValue>(); }
            return source.ContainsKey(key) ? source[key] : defaultValue;
        }

        /// <summary>
        /// キーが無い場合にnullを返す
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrNull<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key) where TValue : class
        {
            if (source == null) { source = new Dictionary<TKey, TValue>(); }
            return source.ContainsKey(key) ? source[key] : null;
        }

        /// <summary>
        /// Valueが真偽値で、且つtrueになる値の場合はtrue。それ以外はfalse。キー無しの場合もfalse
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBool<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
        {
            if (source == null) { source = new Dictionary<TKey, TValue>(); }
            if (source.ContainsKey(key))
            {
                object val = source[key];
                if (val == null) { return false; }
                switch (val)
                {
                    case bool b:
                        return b;
                    case int i:
                        return i != 0;
                    case long l:
                        return l != 0;
                    case double d:
                        return d != 0;
                    case string s:
                        return !new string[] { "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無" }.Any(
                            x => x.Equals(s, System.StringComparison.OrdinalIgnoreCase));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Dictionaryに対象のキーが含まれている場合に削除
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        public static void DeleteIfContains<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
        {
            if (source == null) { source = new Dictionary<TKey, TValue>(); }
            if (source.ContainsKey(key))
            {
                source.Remove(key);
            }
        }
    }
}
