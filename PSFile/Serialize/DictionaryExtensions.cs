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
            if (source == null)
            {
                source = new Dictionary<TKey, TValue>();
            }
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
            if (source == null)
            {
                source = new Dictionary<TKey, TValue>();
            }
            return source.ContainsKey(key) ? source[key] : null;
        }
    }
}
