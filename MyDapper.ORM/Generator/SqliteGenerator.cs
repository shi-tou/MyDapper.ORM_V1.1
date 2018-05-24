using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDapper.ORM.Generator
{
    public class SqliteGenerator : SqlGenerator, ISqlGenerator
    {
        /// <summary>
        /// 分页语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public override string GetPageListSql<T>(int pageIndex, int pageSize, string orderBy)
        {
            return base.GetPageListSql<T>(pageIndex, pageSize, orderBy);
        }

        /// <summary>
        /// 分页语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W">查询对象</typeparam>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public override string GetPageListSql<T, W>(W where, int pageIndex, int pageSize, string orderBy)
        {
            return base.GetPageListSql<T, W>(where, pageIndex, pageSize, orderBy);
        }
    }
}
