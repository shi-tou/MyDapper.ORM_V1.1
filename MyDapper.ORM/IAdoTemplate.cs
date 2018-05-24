using MyDapper.ORM.Generator;
using System;
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

        #region 使用事务
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

        #region 插入/删除/更新/查询
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

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        int Update<T>(T t);

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
        /// <param name="hs"></param>
        /// <returns></returns>
        int Delete<T, W>(W where);

        /// <summary>
        /// 查询(单记录)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hs"></param>
        /// <returns></returns>
        T GetModel<T, R>(R parms);

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
        /// <param name="hs"></param>
        /// <returns></returns>
        List<T> GetList<T, W>(W where);

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

        
        #endregion
    }
}
