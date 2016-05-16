using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    public class SearchResult<TResult>
    {
        public int PageIndex { get; private set; }
        public int TotalRows { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public string SearchType { get; private set; }
        public IEnumerable<TResult> Results { get; private set; }

        public SearchResult(int totalRows, int pageIndex, int pageSize, IEnumerable<TResult> result)
        {
            this.TotalRows = totalRows;
            this.PageIndex = pageIndex;
            this.TotalPages = (int)Math.Ceiling(((float)this.TotalRows) / pageSize);
            this.Results = result;
        }
        public SearchResult(int totalRows, int pageIndex, int pageSize, string searchType, IEnumerable<TResult> result)
        {
            this.TotalRows = totalRows;
            this.PageIndex = pageIndex;
            this.TotalPages = (int)Math.Ceiling(((float)this.TotalRows) / pageSize);
            this.PageSize = pageSize;
            this.SearchType = searchType;
            this.Results = result;
        }

        public void CleanResult()
        {
            this.Results = null;
        }

        public object ToGridData(object rows)
        {
            var jsonData = new
            {
                page = PageIndex,
                total = TotalPages,
                records = TotalRows,
                rows
            };
            rows = null;
            return jsonData;
        }

        public object ToGridDataWithFooterRow(object rows, object footerData)
        {
            var jsonData = new
            {
                page = PageIndex,
                total = TotalPages,
                records = TotalRows,
                rows,
                userdata = footerData
            };

            return jsonData;
        }
    }
}
