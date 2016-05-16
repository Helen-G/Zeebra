using System.Web;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public class ListHelper
    {
        public static int HandlePageSize(HttpContextBase httpContext, int pageSize, int defaultPageSize)
        {
            var isSessionAvailable = httpContext != null && httpContext.Session != null;

            if (pageSize <= 0)
            {
                var pageSizeFromSession = (isSessionAvailable) ? httpContext.Session["pageSize"] : null;
                pageSize = (pageSizeFromSession != null) ? (int)pageSizeFromSession : defaultPageSize;
            }

            if (isSessionAvailable)
            {
                httpContext.Session["pageSize"] = pageSize;
            }
            return pageSize;
        }

        public static bool IsAsc(string sortSord)
        {
            return sortSord == null || !sortSord.ToLower().Equals("desc");
        }
    }
}