using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDapper.ORM.Mapper
{
    /// <summary>
    /// 实体与数据表的映射
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 注入属性值
        /// </summary>
        /// <param name="tableName">映射到数据库的表名</param>
        /// <param name="primaryKey">主键(默认为"ID")</param>
        /// /// <param name="keyType">主键类型(默认为自动增长)</param>
        public TableAttribute(string tableName, string primaryKey = "ID", PrimaryKeyType primaryKeyType = PrimaryKeyType.Identity)
        {
            this.tableName = tableName;
            this.primaryKey = primaryKey;
            this.primaryKeyType = primaryKeyType;
        }
        //表名
        public string tableName;
        public string TableName
        {
            set { tableName = value; }
            get { return tableName; }
        }
        //主键
        public string primaryKey;
        public string PrimaryKey
        {
            set { primaryKey = value; }
            get { return primaryKey; }
        }
        //主键
        public PrimaryKeyType primaryKeyType;
        public PrimaryKeyType PrimaryKeyType
        {
            set { primaryKeyType = value; }
            get { return primaryKeyType; }
        }
    }
}
