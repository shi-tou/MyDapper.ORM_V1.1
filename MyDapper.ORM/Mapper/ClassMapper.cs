using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyDapper.ORM.Mapper
{
    public class ClassMapper
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 主键
        /// </summary>
        public string PrimaryKey { get; set; }
        /// <summary>
        /// 主键类型
        /// </summary>
        public PrimaryKeyType PrimaryKeyType { get; set; }
        /// <summary>
        /// 属性
        /// </summary>
        public List<PropertyInfo> Properties { get; set; }
        
    }
}
