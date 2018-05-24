using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDapper.ORM.Mapper
{
    /// <summary>
    /// 主键类型
    /// </summary>
    public enum PrimaryKeyType
    {
        /// <summary>
        /// 自动增长(integery)
        /// </summary>
        Identity,

        /// <summary>
        /// 程序指定(可以是guid，可以是其他生成的唯一值)
        /// </summary>
        Assigned
    }
}
