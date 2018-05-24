using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDapper.ORM
{
    public class PageList<T> : List<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// 当前页索引
        /// </summary>
        public int PageIndex { get; set; }
        
        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRecordCount { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPageItems"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalItemCount"></param>
        public PageList(IEnumerable<T> currentPageItems, int pageIndex, int pageSize, int totalItemCount)
        {
            AddRange(currentPageItems);
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecordCount = totalItemCount;
            //取余
            int remainder = totalItemCount % PageSize;
            //取整
            int pageCount = totalItemCount / PageSize;
            TotalPageCount = remainder == 0 ? pageCount : (pageCount + 1);
        }
    }
}
