using System;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// This option is used only in advanced single field searching and determines the operation that is applied to the element.
    /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:search_config
    /// </summary>
    [Flags]
    public enum ComparisonOperator
    {
        /// <summary>
        /// the value of the search indicator is false
        /// </summary>
        none = 0,
        /// <summary>
        /// equal
        /// </summary>
        eq = 0x0001,
        /// <summary>
        /// not equal
        /// </summary>
        ne = 0x0002,
        /// <summary>
        /// less than
        /// </summary>
        lt = 0x0004,
        /// <summary>
        /// less or equal
        /// </summary>
        le = 0x0008,
        /// <summary>
        /// greater than
        /// </summary>
        gt = 0x0010,
        /// <summary>
        /// greater or equal
        /// </summary>
        ge = 0x0020,
        /// <summary>
        /// begins with
        /// </summary>
        bw = 0x0040,
        /// <summary>
        /// does not begin with
        /// </summary>
        bn = 0x0080,
        /// <summary>
        /// is in
        /// </summary>
        @in = 0x0100,
        /// <summary>
        /// is not in
        /// </summary>
        ni = 0x0200,
        /// <summary>
        /// ends with
        /// </summary>
        ew = 0x0400,
        /// <summary>
        /// does not end with
        /// </summary>
        en = 0x0800,
        /// <summary>
        /// contains
        /// </summary>
        cn = 0x1000,
        /// <summary>
        /// does not contain
        /// </summary>
        nc = 0x2000
    }
}
