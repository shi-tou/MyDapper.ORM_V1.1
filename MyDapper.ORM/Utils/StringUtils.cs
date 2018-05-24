
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDapper.ORM.Utils
{
    /// <summary>
    /// 字符串处理的相关扩展
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// 使用分隔字符连接字符串组
        /// </summary>
        /// <param name="list">字符列表</param>
        /// <param name="seperator">默认","</param>
        /// <returns></returns>
        public static string AppendStrings(this IEnumerable<string> list, string seperator = ",")
        {
            if (list.Count() == 0)
                return "";
            return list.Aggregate(
                new StringBuilder(),
                (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
                sb => sb.ToString());
        }
    }
}
