using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using Newtonsoft.Json;
using Linq = System.Linq.Expressions;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// Holds the parameter in the url
    /// refer to the prmNames in the web page: http://www.trirand.com/jqgridwiki/doku.php?id=wiki:options
    /// </summary>
    public class SearchPackage : ISearchPackage
    {
        /// <summary>
        /// the the requested page
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// the number of rows requested
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Top Records
        /// </summary>
        public int TopRecords { get; set; }

        /// <summary>
        /// The numbe of the total records return from DB.
        /// </summary>
        ////public int Total { get; set; }

        /// <summary>
        /// the sorting column
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// the sort order
        /// </summary>
        public string SortSord { get; set; }

        /// <summary>
        /// the sort order
        /// </summary>
        public bool SortASC { get; set; }

        public SingleFilter SingleFilter { get; set; }

        public AdvancedFilter AdvancedFilter { get; set; }
    }

    public class SearchPackageResultRow
    {
        public object id { get; set; }
        public object cell { get; set; }
    }

    public class SearchPackageResult
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }
        public List<SearchPackageResultRow> rows { get; set; }
    }

    public class SearchPackageDataBuilder<T>
    {
        public SearchPackage SearchPackage { get; set; }

        private IQueryable<T> _queryable;

        private Func<T, object> _idMap;
        private Func<T, object> _cellMap;

        public SearchPackageDataBuilder(SearchPackage searchPackage, IQueryable<T> rawData)
        {
            SearchPackage = searchPackage;
            _queryable = rawData;
            
            _filterActions = new Dictionary<string, Func<string, Linq.Expression<Func<T, bool>>>>();

        }

        private Dictionary<string, Func<string, Linq.Expression<Func<T, bool>>>> _filterActions;

        public SearchPackageDataBuilder<T> SetFilterRule(Linq.Expression<Func<T, object>> filterProperty,
            Func<string, Linq.Expression<Func<T, bool>>> filterAction)
        {
            var filterPropertyName = filterProperty.Body.ToString();
            filterPropertyName = filterPropertyName.Substring(filterPropertyName.IndexOf('.') + 1).TrimEnd(new [] {')'});
            _filterActions.Add(filterPropertyName.ToLower(), filterAction);

            return this;
        }

        public SearchPackageDataBuilder<T> SetFilterRule(string filterPropertyName,
            Func<string, Linq.Expression<Func<T, bool>>> filterAction)
        {
            _filterActions.Add(filterPropertyName.ToLower(), filterAction);

            return this;
        }


        public SearchPackageDataBuilder<T> Map(Linq.Expression<Func<T, object>> idExpression, Linq.Expression<Func<T, object>> cellExpression)
        {
            _idMap = idExpression.Compile();
            _cellMap = cellExpression.Compile();
            return this;
        }

        public SearchPackageResult GetPageData(Linq.Expression<Func<T, object>> defaultSortProperty)
        {
            var recordsRequested = SearchPackage.RowCount;
            var pageIndex = Math.Max(SearchPackage.PageIndex - 1, 0);

            var recordsTotalCount = DoFilterData();

            if (SearchPackage.TopRecords != 0) 
            {
                recordsTotalCount = Math.Min(SearchPackage.TopRecords, recordsTotalCount);
            }

            var totalPages = 0;
            if (recordsRequested > 0)
            {
                totalPages = (int)Math.Ceiling(((float)recordsTotalCount) / recordsRequested);
            }

            DoOrder(defaultSortProperty);


            if (SearchPackage.TopRecords == 0)
            {
                _queryable = _queryable
                    .Skip(recordsRequested * pageIndex)
                    .Take(recordsRequested);
            }
            else
            {
                if (pageIndex == totalPages - 1)
                {
                    var numRecords = recordsTotalCount - recordsRequested * pageIndex;
                    _queryable = _queryable
                             .Skip(recordsRequested * pageIndex)
                             .Take(numRecords);
                }
                else
                {
                  _queryable = _queryable
                      .Skip(recordsRequested * pageIndex)
                      .Take(recordsRequested);
                }
            }

            var gridRowData = new List<SearchPackageResultRow>();
// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in _queryable)
            {
                gridRowData.Add(new SearchPackageResultRow()
                {
                    id = _idMap(item),
                    cell = _cellMap(item)
                });
            }

            return new SearchPackageResult()
            {
                page = pageIndex + 1,
                total = totalPages,
                records = recordsTotalCount,
                rows = gridRowData
            };
        }

        public List<T> GetExportResult(Linq.Expression<Func<T, object>> defaultSortProperty)
        {
            var recordsTotalCount = DoFilterData();
            DoOrder(defaultSortProperty);

            var gridResult = new List<T>();
            foreach (var item in _queryable)
            {
                gridResult.Add(item);
            }
            return gridResult;
        }

        private  object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            if (conversionType == typeof(Double)) value = ((string)value).Replace(".", ",");
            if (conversionType.IsEnum) return Enum.Parse(conversionType, (string)value);
            if (conversionType == typeof(Guid)) return Guid.Parse((string)value);
            if (conversionType == typeof (DateTimeOffset)) return DateTimeOffset.Parse((string)value);
            return Convert.ChangeType(value, conversionType);
        }
        private int DoFilterData()
        {
            var filterData = GetFilterData();

            foreach (var singleFilter in filterData)
            {
                var dataKey = singleFilter.Field;
                var dataOp = singleFilter.Comparison;
                var dataValue = singleFilter.Data;

                Linq.Expression<Func<T, bool>> wherePredicate;

                if (_filterActions.ContainsKey(dataKey.ToLower()))
                {
                    wherePredicate = _filterActions[dataKey.ToLower()].Invoke(dataValue);
                }
                else if ( dataKey.Contains("-date-range-"))
                {
                   var param = Linq.Expression.Parameter(typeof(T), "x");
                   var propName = dataKey.Substring(0, dataKey.IndexOf("-date-range-", 0));
                   var property = BuildPropertyExpression(param, propName);

                   Linq.ConstantExpression filter = Linq.Expression.Constant
                             (
                                 ChangeType(dataValue, property.Type), property.Type
                             );
                   Linq.BinaryExpression condition; 
                   if(dataKey.Contains("-date-range-start"))
                   {
                       condition = Linq.Expression.GreaterThanOrEqual(property, filter);
                   }
                   else
                   {
                       if (filter.Type == typeof(DateTimeOffset))
                        filter = Linq.Expression.Constant(((DateTimeOffset) filter.Value).AddDays(1), property.Type);
                       else
                        filter = Linq.Expression.Constant(((DateTime)filter.Value).AddDays(1), property.Type);

                       condition = Linq.Expression.LessThan(property, filter);
                   }
                   wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                else if (dataOp == ComparisonOperator.@in)
                {
                    var param = Linq.Expression.Parameter(typeof(T), "x");
                    var property = BuildPropertyExpression(param, dataKey);
                    var collectionType = typeof(ICollection<>).MakeGenericType(property.Type);
                    var collectionData = JsonConvert.DeserializeObject(dataValue, collectionType);
                    var method = collectionType.GetMethod("Contains", new[] { property.Type });
                    var value = Linq.Expression.Constant(collectionData, collectionType);
                    var condition = Linq.Expression.Call(value, method, property);

                    wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                else
                {
                    var param = Linq.Expression.Parameter(typeof (T), "x");
                    var property = BuildPropertyExpression(param, dataKey);
                    var value = Linq.Expression.Constant(ChangeType(dataValue, property.Type), property.Type);
                    if (dataOp == ComparisonOperator.cn && property.Type != typeof (string))
                    {
                        dataOp = ComparisonOperator.eq;
                    }

                    var contains = typeof(string).GetMethod("Contains", new Type[] { typeof (string) });
                    var startsWith = typeof(string).GetMethod("StartsWith", new Type[] { typeof (string) });
                    var endsWith = typeof(string).GetMethod("EndsWith", new Type[] { typeof (string) });
                    
                    Linq.Expression condition;
                    switch (dataOp)
                    {
                        case ComparisonOperator.eq:
                            condition = Linq.Expression.Equal(property, value);
                            break;
                        case ComparisonOperator.ne:
                            condition = Linq.Expression.NotEqual(property, value);
                            break;
                        case ComparisonOperator.lt:
                            condition = Linq.Expression.LessThan(property, value);
                            break;
                        case ComparisonOperator.le:
                            condition = Linq.Expression.LessThanOrEqual(property, value);
                            break;
                        case ComparisonOperator.gt:
                            condition = Linq.Expression.GreaterThan(property, value);
                            break;
                        case ComparisonOperator.ge:
                            condition = Linq.Expression.GreaterThanOrEqual(property, value);
                            break;
                        case ComparisonOperator.bw:
                            condition = Linq.Expression.Call(property, startsWith, value);
                            break;
                        case ComparisonOperator.bn:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, startsWith, value));
                            break;
                        case ComparisonOperator.ni:
                            condition = Linq.Expression.Not(Linq.Expression.Call(value, contains, property));
                            break;
                        case ComparisonOperator.ew:
                            condition = Linq.Expression.Call(property, endsWith, value);
                            break;
                        case ComparisonOperator.en:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, endsWith, value));
                            break;
                        case ComparisonOperator.cn:
                            condition = Linq.Expression.Call(property, contains, value);
                            break;
                        case ComparisonOperator.nc:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, contains, value));
                            break;
                        default:
                            condition = Linq.Expression.Equal(property, value);
                            break;
                    }
                    wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                _queryable = _queryable.Where(wherePredicate);
            }
            return _queryable.Count();
        }


        private List<SingleFilter> GetFilterData()
        {
            if (SearchPackage.AdvancedFilter == null || SearchPackage.AdvancedFilter.Rules == null)
            {
                return new List<SingleFilter>();
            }

            return SearchPackage.AdvancedFilter.Rules
                .Where(singleFilter => !string.IsNullOrEmpty(singleFilter.Data)).ToList();
        }

        public void DoOrder(Linq.Expression<Func<T, object>> defaultSortProperty)
        {
            var sortPropertyPath = SearchPackage.SortColumn;
            if (sortPropertyPath == null)
            {
                if (defaultSortProperty.Body is Linq.MemberExpression)
                {
                    sortPropertyPath = ((Linq.MemberExpression)defaultSortProperty.Body).Member.Name;
                }
                else
                {
                    var op = ((Linq.UnaryExpression) defaultSortProperty.Body).Operand;
                    sortPropertyPath = ((Linq.MemberExpression)op).Member.Name;
                }
            }
            if (string.IsNullOrWhiteSpace(sortPropertyPath)) return;

            var isAscending = SearchPackage.SortSord == null ||
                  SearchPackage.SortSord.ToLowerInvariant().Contains("asc");

            var paramExpr = Linq.Expression.Parameter(_queryable.ElementType, "p");
            var expr = BuildPropertyExpression(paramExpr, sortPropertyPath);
            var orderByExpr = Linq.Expression.Lambda(expr, new[] { paramExpr });
            var orderByCallExpr = Linq.Expression.Call(typeof(Queryable), isAscending ? "OrderBy" : "OrderByDescending",
                new[] { _queryable.ElementType, expr.Type }, _queryable.Expression, orderByExpr);

            _queryable = _queryable.Provider.CreateQuery<T>(orderByCallExpr);
        }

        // Building the expression x => x.[prop].[prop]...
        private static Linq.Expression BuildPropertyExpression(Linq.Expression paramExpr, string propertyPath)
        {
            var propTokens = propertyPath.Split('.');
            return propTokens.Aggregate(paramExpr, Linq.Expression.Property);
        }
    }
}
