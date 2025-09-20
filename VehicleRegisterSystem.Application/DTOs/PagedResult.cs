using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs
{
    /// <summary>
    /// كائن نقل البيانات لنتائج البحث المقسمة على صفحات
    /// Data Transfer Object for paginated search results
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>عناصر الصفحة الحالية - Current page items</summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>إجمالي عدد العناصر - Total count of items</summary>
        public int TotalCount { get; set; }

        /// <summary>رقم الصفحة الحالية - Current page number</summary>
        public int PageNumber { get; set; }

        /// <summary>حجم الصفحة - Page size</summary>
        public int PageSize { get; set; }

        /// <summary>إجمالي عدد الصفحات - Total number of pages</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>وجود صفحة سابقة - Has previous page</summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>وجود صفحة تالية - Has next page</summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
