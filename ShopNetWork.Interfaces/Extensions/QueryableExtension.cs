using Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNetWork.Interfaces.Extensions
{
    public static class QueryableExtension
    {
        /// <summary>
        /// 读取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PagedInfo<T>> ToPageAsync<T>(this ISugarQueryable<T> source, PageParm parm)
        {
            var page = new PagedInfo<T>();
            var total = await source.CountAsync();
            page.TotalCount = total;
            page.TotalPages = total / parm.PageSize;

            if (total % parm.PageSize > 0)
                page.TotalPages++;

            page.PageSize = parm.PageSize;
            page.PageIndex = parm.PageIndex;

            page.DataSource = await source.OrderByIF(!string.IsNullOrEmpty(parm.Sort), $"{parm.OrderBy} {(parm.Sort == "descending" ? "desc" : "asc")}").ToPageListAsync(parm.PageIndex, parm.PageSize);
            return page;
        }

        /// <summary>
        /// 读取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PagedInfo<T> ToPage<T>(this ISugarQueryable<T> source, PageParm parm)
        {
            var page = new PagedInfo<T>();
            var total = source.Count();
            page.TotalCount = total;
            page.TotalPages = total / parm.PageSize;

            if (total % parm.PageSize > 0)
                page.TotalPages++;

            page.PageSize = parm.PageSize;
            page.PageIndex = parm.PageIndex;

            page.DataSource = source.OrderByIF(!string.IsNullOrEmpty(parm.Sort), $"{parm.OrderBy} {(parm.Sort == "descending" ? "desc" : "asc")}").ToPageList(parm.PageIndex, parm.PageSize);
            return page;
        }

    }
}
