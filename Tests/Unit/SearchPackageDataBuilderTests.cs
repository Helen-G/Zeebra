using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit
{
    class SearchPackageDataBuilderTests : AdminWebsiteUnitTestsBase
    {
        private SearchPackage _searchPackage;
        private List<SearchPackageTestItem> _items;
 
        public override void BeforeEach()
        {
            base.BeforeEach();

            _searchPackage = new SearchPackage
            {
                PageIndex = 0,
                RowCount = 10,
                SortASC = true,
                SortColumn = "Property1",
                SortSord = "",
                SingleFilter = null,
                AdvancedFilter = null
            };

            _items = new List<SearchPackageTestItem>()
            {
                new SearchPackageTestItem
                {
                    Property1 = "abcdef",
                    Property2 = "ghijkl",
                    Property3 = "mnopqr"
                },
                new SearchPackageTestItem
                {
                    Property1 = "123456",
                    Property2 = null,
                    Property3 = "some test string"
                },
                new SearchPackageTestItem
                {
                    Property1 = "aaabbbccc",
                    Property2 = "not null",
                    Property3 = "another test string"
                },
                new SearchPackageTestItem
                {
                    Property1 = "12345678910",
                    Property2 = "not null",
                    Property3 = "another test string"
                }
            };
        }

        [Test]
        public void Can_serve_data()
        {
            var dataBuilder = new SearchPackageDataBuilder<SearchPackageTestItem>(_searchPackage, _items.AsQueryable());

            dataBuilder.Map(x => x.Property1,
                x => new[]
                {
                    x.Property2,
                    x.Property3 + "!"
                });

            var result = dataBuilder.GetPageData(x => x.Property2);

            Assert.AreEqual(1, result.total);// totalpages
            Assert.AreEqual(_items.Count(), result.records);
            var cells = result.rows[0].cell as string[];
            Assert.AreEqual("mnopqr!", cells[1]);
        }

        [Test]
        public void Can_filter()
        {
            _searchPackage.AdvancedFilter = new AdvancedFilter
            {
                Rules = new[]
                {
                    new SingleFilter("Property1", ComparisonOperator.cn, "abcdef")
                }
            };
   
            var dataBuilder = new SearchPackageDataBuilder<SearchPackageTestItem>(_searchPackage, _items.AsQueryable());

            dataBuilder.Map(x => x.Property1,
                x => new[]
                {
                    x.Property2,
                    x.Property3
                });

            var result = dataBuilder.GetPageData(x => x.Property2);

            Assert.AreEqual(1, result.records);
        }

        [Test]
        public void Can_filter_out_everthing()
        {
            _searchPackage.AdvancedFilter = new AdvancedFilter
            {
                Rules = new[]
                {
                    new SingleFilter("Property1", ComparisonOperator.cn, "xyz")
                }
            };

            var dataBuilder = new SearchPackageDataBuilder<SearchPackageTestItem>(_searchPackage, _items.AsQueryable());

            dataBuilder.Map(x => x.Property1,
                x => new[]
                {
                    x.Property2,
                    x.Property3
                });

            var result = dataBuilder.GetPageData(x => x.Property2);

            Assert.AreEqual(0, result.records);
        }

        [Test]
        public void Can_apply_custom_filter()
        {
            _searchPackage.AdvancedFilter = new AdvancedFilter
            {
                Rules = new[]
                {
                    new SingleFilter("Property1", ComparisonOperator.cn, "123456"),
                    new SingleFilter("Property3", ComparisonOperator.cn, "test"),
                }
            };

            var dataBuilder = new SearchPackageDataBuilder<SearchPackageTestItem>(_searchPackage, _items.AsQueryable());

            dataBuilder.SetFilterRule(x => x.Property1, (value) => p => p.Property1 == value);
            dataBuilder.SetFilterRule(x => x.Property3, (value) => p => p.Property3.Contains(value));

            dataBuilder.Map(x => x.Property1,
                x => new[]
                {
                    x.Property1,
                    x.Property2,
                    x.Property3
                });

            var result = dataBuilder.GetPageData(x => x.Property2);

            Assert.AreEqual(1, result.records);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSortProperty()
        {
            _searchPackage.SortColumn = "invalidpropertyname";
            var dataBuilder = new SearchPackageDataBuilder<SearchPackageTestItem>(_searchPackage, _items.AsQueryable());

            dataBuilder.Map(x => x.Property1,
                x => new[]
                {
                    x.Property2,
                    x.Property3 + "!"
                });

            dataBuilder.GetPageData(x => x.Property2);
        }
    }

    class SearchPackageTestItem
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
    }
}
