using Dapper;
using MyDapper.ORM.Generator;
using MyDapper.ORM.Mapper;
using MyDapper.ORM.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyDapper.ORM
{
    public class AdoTemplate : IAdoTemplate
    {
        public AdoTemplate()
        {
        }
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public virtual IDbConnection DbConnection { get; set; }

        /// <summary>
        /// 当前事务对象
        /// </summary>
        public virtual IDbTransaction DbTransaction { get; set; }

        /// <summary>
        /// sql语句构造器
        /// </summary>
        public virtual ISqlGenerator SqlGenerator { get; set; }

        #region 使用事务
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DbTransaction = DbConnection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// 提交事件
        /// </summary>
        public void Commit()
        {
            DbTransaction.Commit();
            DbTransaction = null;
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            DbTransaction.Rollback();
            DbTransaction = null;
        }

        /// <summary>
        /// 委托方式使用事务
        /// </summary>
        /// <param name="action"></param>
        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                Commit();
            }
            catch (Exception ex)
            {
                if (DbTransaction != null)
                {
                    Rollback();
                }
                throw ex;
            }
        }

        /// <summary>
        /// 委托方式使用事务(有返回值)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                T result = func();
                Commit();
                return result;
            }
            catch (Exception ex)
            {
                if (DbTransaction != null)
                {
                    Rollback();
                }
                throw ex;
            }
        }
        #endregion

        #region 插入/删除/更新/查询
        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual int Insert<T>(T t)
        {
            try
            {
                string sql = SqlGenerator.GetInsertSql<T>();
                return DbConnection.Execute(sql, t);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual int Insert<T>(List<T> listT)
        {
            try
            {
                string sql = SqlGenerator.GetInsertSql<T>();
                return DbConnection.Execute(sql, listT);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual int Update<T>(T t)
        {
            try
            {
                string sql = SqlGenerator.GetUpdateSql<T>(t);
                return DbConnection.Execute(sql, t);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }        
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual int Delete<T>()
        {
            try
            {
                string sql = SqlGenerator.GetDeleteSql<T>();
                return DbConnection.Execute(sql);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        public virtual int Delete<T, W>(W where)
        {
            try
            {
                string sql = SqlGenerator.GetDeleteSql<T,W>(where);
                return DbConnection.Execute(sql, where);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        public virtual T GetModel<T, W>(W where)
        {
            try
            {
                string sql = SqlGenerator.GetSelectSql<T,W>(where);
                return DbConnection.QuerySingle<T>(sql, where);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(T);
            }
        }

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> GetList<T>()
        {
            try
            {
                string sql = SqlGenerator.GetSelectSql<T>();
                return DbConnection.Query<T>(sql).ToList();
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(List<T>);
            }
        }

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual List<T> GetList<T, W>(W where) 
        {
            try
            {
                string sql = SqlGenerator.GetSelectSql<T, W>(where);
                return DbConnection.Query<T>(sql, where).ToList();
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(List<T>);
            }
        }

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="sql"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual List<T> GetList<T, W>(string sql, W where)
        {
            try
            {
                return DbConnection.Query<T>(sql, where).ToList();
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(List<T>);
            }
        }        
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual PageList<T> GetPageList<T>(int pageIndex, int pageSize, string orderBy)
        {
            try
            {
                string sql = SqlGenerator.GetPageListSql<T>(pageIndex, pageSize, orderBy);
                int totalCount = GetCount<T>();
                List<T> list = DbConnection.Query<T>(sql).ToList();
                return new PageList<T>(list, pageIndex, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(PageList<T>);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">sql查询语句</param>
        /// <param name="param">sql参数</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual PageList<T> GetPageList<T, W>(W where, int pageIndex, int pageSize, string orderBy)
        {
            try
            {
                string sql = SqlGenerator.GetPageListSql<T, W>(where, pageIndex, pageSize, orderBy);
                int totalCount = GetCount<T, W>(where);
                List<T> list = DbConnection.Query<T>(sql, where).ToList();
                return new PageList<T>(list, pageIndex, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(PageList<T>);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">sql查询语句</param>
        /// <param name="param">sql参数</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        public virtual PageList<T> GetPageList<T,W>(string sql, W where, int pageIndex, int pageSize, string orderBy)
        {
            try
            {
                string getCountSql = string.Format("select count(1) from ({0}) as A",sql);
                int totalCount = Convert.ToInt32(DbConnection.ExecuteScalar(getCountSql, where));
                sql = SqlGenerator.GetPageListSql(sql, pageIndex, pageSize, orderBy);
                List<T> list = DbConnection.Query<T>(sql, where).ToList();
                return new PageList<T>(list, pageIndex, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return default(PageList<T>);
            }
        }

        /// <summary>
        /// 获取计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual int GetCount<T>()
        {
            try
            {
                string sql = SqlGenerator.GetCountSql<T>();
                return Convert.ToInt32(DbConnection.ExecuteScalar(sql));
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 获取计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual int GetCount<T, W>(W where)
        {
            try
            {
                string sql = SqlGenerator.GetCountSql<T, W>(where);
                return Convert.ToInt32(DbConnection.ExecuteScalar(sql,where));
            }
            catch (Exception ex)
            {
                string exMessage = ex.Message;
                return 0;
            }
        }
        #endregion

        #region 执行Sql语句方法
        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, null);
        }
        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(string commandText, params DbParameter[] parms)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, parms);
        }
        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, object parms)
        {
            return DbConnection.Execute(commandText, parms, null, null, commandType);
        }
        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText, null);
        }
        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string commandText, object parms)
        {
            return ExecuteScalar(CommandType.Text, commandText, parms);
        }
        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, object parms)
        {
            return DbConnection.ExecuteScalar(commandText, parms, null, null, commandType);
        }
        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行</returns>
        public DataRow ExecuteDataRow(CommandType commandType, string commandText, object parms)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public DataTable ExecuteDataTable(CommandType commandType, string commandText, object parms)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        public DataSet ExecuteDataSet(CommandType commandType, string commandText, object parms)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// 释放资源
        /// 1-如果事务异常，则回滚事务
        /// 2-释放数据库连接
        /// </summary>
        public void Dispose()
        {
            if (DbConnection.State != ConnectionState.Closed)
            {
                if (DbTransaction != null)
                {
                    DbTransaction.Rollback();
                }
                DbConnection.Close();
            }
        }
    }
}
