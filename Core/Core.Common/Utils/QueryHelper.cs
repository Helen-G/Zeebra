using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AFT.RegoV2.Core.Common.Utils
{
    public static class QueryHelper
    {
        public static IQueryable<T> Order<T>(IQueryable<T> queryable, string propertyPath, bool isAsc)
        {
            if (!string.IsNullOrWhiteSpace(propertyPath))
            {
                var elementType = queryable.ElementType;
                var propTokens = propertyPath.Split('.');

                // Building the expression p => p.[prop].[prop]...
                var paramExpr = Expression.Parameter(elementType, "p");
                Expression expr = paramExpr;
                PropertyInfo propInfo = null;
                var type = elementType;
                foreach (var propToken in propTokens)
                {
                    propInfo = type.GetProperty(propToken);
                    expr = Expression.Property(expr, propInfo);
                    type = propInfo.PropertyType;
                }
                if (propInfo == null)
                {
                    throw new ArgumentException("Bad propertyPath: " + propertyPath);
                }
                var orderByExpr = Expression.Lambda(expr, new[] { paramExpr });

                var orderByCallExpr = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending",
                    new[] { queryable.ElementType, propInfo.PropertyType }, queryable.Expression, orderByExpr);

                return queryable.Provider.CreateQuery<T>(orderByCallExpr);
            }
            return queryable;
        }
    }
}