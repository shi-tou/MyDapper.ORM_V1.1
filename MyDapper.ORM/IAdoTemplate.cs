using MyDapper.ORM.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDapper.ORM
{
    /// <summary>
    /// 通用数据库操作接口
    /// </summary>
    public interface IAdoTemplate : IDisposable
    {
        #region ---property---
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// 当前事务对象
        /// </summary>
        IDbTransaction DbTransaction { get; }

        /// <summary>
        /// sql语句构造器
        /// </summary>
        ISqlGenerator SqlGenerator { get; set; }
        #endregion

        #region ---Transaction---
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// 提交事件
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 委托方式使用事务
        /// </summary>
        /// <param name="action"></param>
        void RunInTransaction(Action action);

        /// <summary>
        /// 委托方式使用事务(有返回值)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        T RunInTransaction<T>(Func<T> func);
        #endregion

        #region ---Insert---
        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        int Insert<T>(T t);

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        int Insert<T>(List<T> listT);
        #endregion

        #region ---Update---

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        int Update<T>(T t);
        #endregion

        #region ---Delete---
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        int Delete<T>();

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="vlaue"></param>
        /// <returns></returns>
        int Delete<T>(string columnName, object vlaue);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        int Delete<T>(Hashtable hs);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        int Delete<T, W>(W where);
        #endregion

        #region ---GetModel---
        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        T GetModel<T>(string columnName, object value);

        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        T GetModel<T>(Hashtable hs);

        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        T GetModel<T, W>(W where);
        #endregion

        #region ---GetList---
        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> GetList<T>();

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="vlaue"></param>
        /// <returns></returns>
        List<T> GetList<T>(string columnName, object vlaue);

        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        List<T> GetList<T>(Hashtable hs);

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        List<T> GetList<T, W>(W where);

        /// <summary>
        /// 查询(多记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="sql"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        List<T> GetList<T, W>(string sql, W where);
        #endregion

        #region ---GetPageList---
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        PageList<T> GetPageList<T>(int pageIndex, int pageSize, string orderBy);

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
        PageList<T> GetPageList<T, W>(W where, int pageIndex, int pageSize, string orderBy);

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
        PageList<T> GetPageList<T, W>(string sql, W where, int pageIndex, int pageSize, string orderBy);
        #endregion

        #region ---GetCount---
        /// <summary>
        /// 获取计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        int GetCount<T>();

        /// <summary>
        /// 获取计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        int GetCount<T, W>(W where);
        #endregion
    }
}
