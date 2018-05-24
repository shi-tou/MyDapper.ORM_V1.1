using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MyDapper.ORM.Mapper;
using MyDapper.ORM.Utils;

namespace MyDapper.ORM.Generator
{
    /// <summary>
    /// sql语句构造器
    /// </summary>
    public class SqlGenerator : ISqlGenerator
    {
        /// <summary>
        /// 参数前缀
        /// </summary>
        public virtual char ParameterPrefix
        {
            get { return '@'; }
        }

        /// <summary>
        /// 空的表达式
        /// </summary>
        public virtual string EmptyExpression
        {
            get { return "1=1"; }
        }

        /// <summary>
        /// 获取类映射
        /// </summary>
        /// <param name="t">类型</param>
        /// <returns></returns>
        public virtual ClassMapper GetMapper(Type t)
        {
            List<PropertyInfo> propertys = null;
            TableAttribute attribute = null;
            propertys = ReflectionUtils.TypePropertiesCache(t);
            attribute = ReflectionUtils.CustomAttributesCache(t);
            ClassMapper map = new ClassMapper
            {
                TableName = attribute == null ? "" : attribute.TableName,
                PrimaryKey = attribute == null ? "" : attribute.PrimaryKey,
                PrimaryKeyType = attribute == null ? PrimaryKeyType.Assigned : attribute.PrimaryKeyType,
                Properties = propertys
            };
            return map;
        }

        /// <summary>
        /// Insert语句
        /// </summary>
        /// <returns></returns>
        public virtual string GetInsertSql<T>()
        {
            Type t = typeof(T);
            ClassMapper map = GetMapper(t);
            string columnNames = string.Empty;
            string parameters = string.Empty;
            string selectIdentity = string.Empty;
            if (map.PrimaryKeyType == PrimaryKeyType.Identity)
            {
                columnNames = map.Properties.Where(p => p.Name.ToLower() != map.PrimaryKey.ToLower())
                    .Select(p => p.Name).AppendStrings(",");
                parameters = map.Properties.Where(p => p.Name.ToLower() != map.PrimaryKey.ToLower())
                    .Select(p => ParameterPrefix + p.Name).AppendStrings(",");
                selectIdentity = "SELECT LAST_INSERT_ID();";
            }
            else
            {
                columnNames = map.Properties.Select(p => p.Name).AppendStrings(",");
                parameters = map.Properties.Select(p => ParameterPrefix + p.Name).AppendStrings(",");
            }
            return string.Format("INSERT INTO {0} ({1}) values ({2});{3}", map.TableName, columnNames, parameters,selectIdentity);
        }

        /// <summary>
        /// Select语句
        /// </summary>
        /// <returns></returns>
        public virtual string GetSelectSql<T>()
        {
            ClassMapper mapT = GetMapper(typeof(T));
            return string.Format("SELECT * FROM {0} ", mapT.TableName);
        }

        /// <summary>
        /// Select语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="_params"></param>
        /// <returns></returns>
        public virtual string GetSelectSql<T, W>(W where)
        {
            ClassMapper mapT = GetMapper(typeof(T));
            ClassMapper mapW = GetMapper(where.GetType());
            string strWhere = mapW.Properties.Select(p => string.Format("{0}={1}{0}", p.Name, ParameterPrefix)).AppendStrings(" and ");
            return string.Format("SELECT * FROM {0} WHERE {1}", mapT.TableName, string.IsNullOrEmpty(strWhere) ? EmptyExpression : strWhere);
        }

        /// <summary>
        /// Update语句(默认更新条件为主键)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetUpdateSql<T>(T t)
        {
            ClassMapper mapT = GetMapper(typeof(T));
            string set = mapT.Properties.Where(p => p.Name.ToLower() != mapT.PrimaryKey.ToLower() && p.GetValue(t, null) != null)
                .Select(p => string.Format("{0}={1}{0}", p.Name, ParameterPrefix)).AppendStrings(",");
            string where = string.Format("{0}={1}{0}", mapT.PrimaryKey, ParameterPrefix);
            return string.Format("UPDATE {0} SET {1} WHERE {2}", mapT.TableName, set, where);
        }

        /// <summary>
        /// Delete语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual string GetDeleteSql<T>()
        {
            ClassMapper mapT = GetMapper(typeof(T));
            return string.Format("DELETE FROM {0}", mapT.TableName);
        }

        /// <summary>
        /// Delete语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W">查询对象</typeparam>
        /// <returns></returns>
        public virtual string GetDeleteSql<T, W>(W where)
        {
            ClassMapper mapT = GetMapper(typeof(T));
            ClassMapper mapW = GetMapper(where.GetType());
            string strWhere = mapW.Properties.Select(p => string.Format("{0}={1}{0}", p.Name, ParameterPrefix)).AppendStrings(" and ");
            return string.Format("DELETE FROM {0} WHERE {1}", mapT.TableName, string.IsNullOrEmpty(strWhere) ? EmptyExpression : strWhere);
        }

        /// <summary>
        /// Count语句
        /// </summary>
        /// <returns></returns>
        public virtual string GetCountSql<T>()
        {
            ClassMapper mapT = GetMapper(typeof(T));
            return string.Format("SELECT COUNT(1) FROM {0}", mapT.TableName);
        }

        /// <summary>
        /// Count语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W">查询对象</typeparam>
        /// <returns></returns>
        public virtual string GetCountSql<T, W>(W where)
        {
            ClassMapper mapT = GetMapper(typeof(T));
            ClassMapper mapW = GetMapper(where.GetType());
            string strWhere = mapW.Properties.Select(p => string.Format("{0}={1}{0}", p.Name, ParameterPrefix)).AppendStrings(" and ");
            return string.Format("SELECT COUNT(1) FROM {0} WHERE {1} ", mapT.TableName, string.IsNullOrEmpty(strWhere) ? EmptyExpression : strWhere);
        }

        /// <summary>
        /// 分页语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual string GetPageListSql<T>(int pageIndex, int pageSize, string orderBy)
        {
            throw new NotImplementedException();
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
        public virtual string GetPageListSql<T, W>(W where, int pageIndex, int pageSize, string orderBy)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 分页语句(联表查询)
        /// </summary>
        /// <param name="sql">传入的联表查询语句</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual string GetPageListSql(string sql, int pageIndex, int pageSize, string orderBy)
        {
            throw new NotImplementedException();
        }
    }
}
