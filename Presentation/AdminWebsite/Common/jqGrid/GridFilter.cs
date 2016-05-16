using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:advanced_searching#options
    /// </summary>
    public class AdvancedFilter
    {
        public LogicalOperator groupOp { get; set; }
        public SingleFilter[] Rules
        {
            get
            {
                return _rules.Count == 0 ? null : _rules.ToArray();
            }
            set
            {
                _rules.AddRange(value);
            }
        }

        public AdvancedFilter[] Groups
        {
            get
            {
                return _groups.Count == 0 ? null : _groups.ToArray();
            }
            set
            {
                _groups.AddRange(value);
            }
        }

        private List<SingleFilter> _rules;
        private List<AdvancedFilter> _groups;

        public AdvancedFilter()
        {
            _rules = new List<SingleFilter>();
            _groups = new List<AdvancedFilter>();
        }

        public AdvancedFilter Add(SingleFilter rule)
        {
            _rules.Add(rule);
            return this;
        }

        public AdvancedFilter Add(AdvancedFilter group)
        {
            _groups.Add(group);
            return this;
        }
    }

    public enum LogicalOperator
    {
        AND,
        OR
    }

    public class SingleFilter
    {
        public SingleFilter(string field, ComparisonOperator op, string data)
        {
            Field = field;
            Comparison = op;
            Data = data;
        }

        public string Field { get; private set; }
        public ComparisonOperator Comparison { get; private set; }
        public string Data { get; private set; }
    }
}
