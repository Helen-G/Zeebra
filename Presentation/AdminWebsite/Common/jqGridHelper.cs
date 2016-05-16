using System.Collections.Generic;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public class jqGridHelper 
    {
      

        /// <summary>
        /// Creates search package for Advanced Search function
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static SearchPackage GetExportSearchPackage(Dictionary<string, string> dict, string sortColumn, string sortOrder)
        {
            SearchPackage searchPackage = new SearchPackage();
            searchPackage.SortSord = sortOrder;
            searchPackage.SortColumn = sortColumn;
            searchPackage.PageIndex = 1;
            searchPackage.RowCount = 100000000;

            AdvancedFilter advancedFilter = new AdvancedFilter();
            List<SingleFilter> lstfilters = new List<SingleFilter>();
            SingleFilter singleFilter;

            foreach (KeyValuePair<string, string> item in dict)
            {
                singleFilter = new SingleFilter(item.Key, ComparisonOperator.eq, item.Value);
                lstfilters.Add(singleFilter);
            }

            advancedFilter.Rules = lstfilters.ToArray();
            searchPackage.AdvancedFilter = advancedFilter;
            return searchPackage;
        }
    }
}