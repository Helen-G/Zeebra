using System;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:options
    /// </summary>
    public class GridSetting
    {
        private List<Column> _colModel;

        public GridSetting()
        {
            _colModel = new List<Column>();
        }

        public string caption { get; set; }
        public string url { get; set; }
        public string editurl { get; set; }
        public DataType datatype { get; set; }
        public JSFunction loadComplete { get; set; }
        public JSFunction gridComplete { get; set; }
        public JSFunction onSelectRow { get; set; }
        public JSFunction loadError { get; set; }
        public JSFunction afterInsertRow { get; set; }

        /// <summary>
        /// When autowidth is set to true the grid fits to the width of the parent container.
        /// This option does not resize the grid when the width of the parent container changes. 
        /// </summary>
        public bool? autowidth { get; set; }

        public bool? shrinkToFit { get; set; }
        /// <summary>
        /// If this option is not set, the width of the grid is a sum of the widths of the columns defined in the colModel (in pixels).
        /// If this option is set, the initial width of each column is set according to the value of shrinkToFit option.
        /// </summary>
        public int? width { get; set; }

        /// <summary>
        /// The height of the grid.
        /// Can be set as number (in this case we mean pixels) or as percentage (only 100% is acceped) or value of auto is acceptable.
        /// </summary>
        public string height { get; set; }

        public bool ignoreCase { get; set; }
        public bool sortable { get; set; }
        public bool columnReordering { get; set; }
        public bool footerrow { get; set; }
        public bool userDataOnFooter { get; set; }

        public Column[] colModel
        {
            get
            {
                return _colModel.Count == 0 ? null : _colModel.ToArray();
            }
        }

        public void AddColumn(Column column)
        {
            _colModel.Add(column);
        }

        public int rowNum { get; set; }
        public int[] rowList { get; set; }
        public string pager { get; set; }
        public string sortname { get; set; }
        public SortOrder sortorder { get; set; }
        /// <summary>
        /// If true, jqGrid displays the beginning and ending record number in the grid, out of the total number of records in the query. This information is shown in the pager bar (bottom right by default)
        /// null means false.
        /// </summary>
        public bool? viewrecords { get; set; }
        public bool? rownumbers { get; set; }
        public bool? gridview { get; set; }
        public bool? multiselect { get; set; }

        public enum DataType
        {
            json,
            local
        }

        public enum SortOrder
        {
            asc,
            desc
        }

        /// <summary>
        /// The colModel property defines the individual grid columns as an array of properties. This is the most important part of the jqGrid
        /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:colmodel_options
        /// </summary>
        public class Column
        {
            private static string[] ReservedNames = { "subgrid", "cb", "rn" };

            /// <summary>
            /// Set the unique name in the grid for the column. This property is required
            /// </summary>
            public string name { get; set; }
            public string label { get; set; }

            /// <summary>
            /// The initial width of the column, in pixels. This value currently can not be set as percentage
            /// </summary>
            public int? width { get; set; }
            /// <summary>
            /// Defines if this column is hidden at initialization.
            /// </summary>
            public bool? hidden { get; set; }
            public bool? @fixed { get; set; }
            /// <summary>
            /// Defines the alignment of the cell in the Body layer, not in header cell
            /// </summary>
            public Align? align { get; set; }

            /// <summary>
            /// When used in search modules, disables or enables searching on that column.
            /// </summary>
            public bool? search { get; set; }
            /// <summary>
            /// Show/Hide title for jqGrid column
            /// </summary>
            public bool? title { get; set; }

            /// <summary>
            /// Whether this column can be sorted. The default value is true.
            /// </summary>
            public bool? sortable { get; set; }

            public string sorttype { get; set; }

            /// <summary>
            /// Freeze the column from scrolling in horizontal scrollbar
            /// </summary>
            public bool? frozen { get; set; }

            public ColumnSearchOption searchoptions { get; set; }
            /// <summary>
            /// Make field can be edited.
            /// </summary>
            /// <param name="name"></param>
            public bool editable { get; set; }

            /// <summary>
            /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:common_rules#editrules
            /// </summary>
            /// <param name="name"></param>
            /// 
            public ColumnEditrule editrules { get; set; }

            /// <summary>
            /// </summary>
            ////public ColumnEditorType? edittype { get; set; }
            ////public ColumnEditOption editoptions { get; set; }
            public ColumnFormOption formoptions { get; set; }

            public Column SetEditrule(ColumnEditrule editrules)
            {
                this.editrules = editrules;
                return this;
            }
            public Column(string name)
            {
                // Make sure columnname is not left blank
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("No columnname specified");
                }

                if (ReservedNames.Contains(name))
                {
                    throw new ArgumentException("Columnname '" + name + "' is reserved");
                }
                this.name = name;
                this.search = false;
            }

            public Column(string name, string label)
                : this(name)
            {
                this.label = label;
                this.title = true; // Default is Show Column Tooltip
            }

            public Column(string name, string label, bool showColumnTooltip, bool freezeColumn)
                : this(name)
            {
                this.label = label;
                this.title = showColumnTooltip;
                this.frozen = freezeColumn;
            }

            public Column Hide()
            {
                this.hidden = true;
                return this;
            }

            public Column Fix()
            {
                this.@fixed = true;
                return this;
            }

            public Column SetAlign(Align align)
            {
                this.align = align;
                return this;
            }

            public Column SetSortType(string sortType)
            {
                this.sorttype = sortType;
                return this;
            }

            public Column SetWidth(int width)
            {
                this.width = width;
                return this;
            }

            public Column Allow(ComparisonOperator options)
            {
                this.searchoptions = new ColumnSearchOption
                {
                    ComparisonOperator = options
                };
                this.search = true;
                return this;
            }
            public Column Editable()
            {
                this.editable = true;
                return this;
            }

            public Column EditableAndHidden()
            {
                this.SecurityEditrule.edithidden = true;
                this.hidden = true;
                this.editable = true;
                return this;
            }

            public Column Required()
            {
                this.SecurityEditrule.required = true;
                this.SecurityFormOption.elmsuffix = "(<span style='color:red;'>*</span>)";
                return this;
            }

            public Column AsKey()
            {
                this.SecurityEditrule.edithidden = true;
                this.Hide().Editable();
                return this;
            }

            public Column AsNumber()
            {
                this.SecurityEditrule.number = true;
                return this;
            }

            public Column AsInteger()
            {
                this.SecurityEditrule.integer = true;
                return this;
            }

            private void CheckboxFormatter()
            {
                this.formatter = GridSetting.Formatter.checkbox;
            }

            public Column Suffix(string suffix)
            {
                this.formoptions = new GridSetting.ColumnFormOption()
                {
                    elmsuffix = "(<span style='font-weight: bold;'>" + suffix + "</span>)"
                };

                return this;
            }

            #region edittype
            public ColumnEditorType? edittype { get; set; }
            public ColumnEditOption editoptions { get; set; }

            public Column TextEditor(int? size, int? maxLength, string defaultValue = null)
            {
                this.edittype = GridSetting.ColumnEditorType.text;
                this.editoptions = new GridSetting.ColumnEditOption
                {
                    size = size,
                    maxlength = maxLength,
                    defaultValue = defaultValue
                };
                return this;
            }

            public Column PasswordEditor(int? size, int? maxLength, string defaultValue = null)
            {
                this.edittype = GridSetting.ColumnEditorType.password;
                this.editoptions = new GridSetting.ColumnEditOption
                {
                    size = size,
                    maxlength = maxLength,
                    defaultValue = defaultValue
                };
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="column"></param>
            /// <param name="numberOfRows"></param>
            /// <param name="width">in characters</param>
            public Column TextAreaEditor(int? numberOfRows, int? width, string defaultValue = null)
            {
                this.edittype = GridSetting.ColumnEditorType.textarea;
                this.editoptions = new GridSetting.ColumnEditOption()
                {
                    rows = numberOfRows.HasValue ? numberOfRows.Value.ToString() : null,
                    cols = width.HasValue ? width.Value.ToString() : null,
                    defaultValue = defaultValue
                };
                return this;
            }

            /// <summary>
            /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:predefined_formatter#formatter_type_select
            /// </summary>
            /// <param name="column"></param>
            /// <param name="url"></param>
            /// <returns></returns>
            public Column SelectEditor(string url, string defaultValue = null)
            {
                ////this.formatter = GridSetting.Formatter.select;
                this.edittype = GridSetting.ColumnEditorType.select;
                this.editoptions = new GridSetting.ColumnEditOption()
                {
                    dataUrl = url,
                    defaultValue = defaultValue
                };
                return this;
            }

            #endregion

            #region Format
            public Formatter? formatter { get; set; }
            public ColumnFormatoptions formatoptions { get; set; }

            public Column Formatter(Formatter formatter)
            {
                switch (formatter)
                {
                    case GridSetting.Formatter.checkbox:
                        this.CheckboxFormatter();
                        break;
                    case GridSetting.Formatter.currency:
                        this.CurrencyFormatter();
                        break;
                    case GridSetting.Formatter.integer:
                        this.IntegerFormatter();
                        break;
                    case GridSetting.Formatter.number:
                        this.NumberFormatter();
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return this;
            }

            /// <summary>
            /// Formatter
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            private void CurrencyFormatter()
            {
                this.formatter = GridSetting.Formatter.currency;
                this.formatoptions = new GridSetting.ColumnFormatoptions
                {
                    decimalSeparator = ".",
                    thousandsSeparator = ",",
                    suffix = "$",
                    decimalPlaces = 2,
                    defaulValue = "0.00"
                };
            }

            private void NumberFormatter()
            {
                this.formatter = GridSetting.Formatter.number;
                this.formatoptions = new GridSetting.ColumnFormatoptions
                {
                    decimalSeparator = ".",
                    thousandsSeparator = ",",
                    decimalPlaces = 2,
                    defaulValue = "0.00"
                };
            }

            private void IntegerFormatter()
            {
                this.formatter = GridSetting.Formatter.integer;
                this.formatoptions = new GridSetting.ColumnFormatoptions
                {
                    thousandsSeparator = ",",
                    defaulValue = "0"
                };
            }
            #endregion

            public Column CheckBoxEditor(string val, string defaultValue = null)
            {
                this.formatter = GridSetting.Formatter.checkbox;
                ColumnFormatoptions colFormatOpt = new ColumnFormatoptions();
                colFormatOpt.disabled = false;
                this.formatoptions = colFormatOpt;
                this.edittype = GridSetting.ColumnEditorType.checkbox;
                this.editoptions = new GridSetting.ColumnEditOption
                {
                    value = val,
                    defaultValue = defaultValue
                };
                return this;
            }

            public Column Min(int min)
            {
                this.SecurityEditrule.minValue = min;
                return this;
            }

            public Column Max(int max)
            {
                this.SecurityEditrule.maxValue = max;
                return this;
            }

            public Column CustomValidation(string func)
            {
                this.SecurityEditrule.custom = true;
                this.SecurityEditrule.custom_func = new JSFunction { Definition = func };
                return this;
            }

            public Column DefaultValue(string value)
            {
                this.SecurityEditOption.defaultValue = value;
                return this;
            }

            public Column Disabled()
            {
                this.editable = true;
                ////this.SecurityEditOption.dataInit = new JSFunction { Defination = "function(element){ jQuery(element).attr('disabled','disabled'),jQuery(element).attr('style','background:#e1e0e0;border:1px solid')}" };
                this.SecurityEditOption.dataInit = new JSFunction { Definition = "function(element){ jQuery(element).attr('contenteditable','false'),jQuery(element).attr('style','background:#e1e0e0;border:1px solid')}" };
                return this;
            }

            public Column DisabledAndHidden()
            {
                this.SecurityEditrule.edithidden = true;
                this.hidden = true;
                return this.Disabled();
            }

            public Column DisableSort()
            {
                this.sortable = false;
                return this;
            }

            private ColumnEditrule SecurityEditrule
            {
                get
                {
                    if (this.editrules == null)
                    {
                        this.editrules = new ColumnEditrule();
                    }

                    return this.editrules;
                }
            }

            private ColumnFormOption SecurityFormOption
            {
                get
                {
                    if (this.formoptions == null)
                    {
                        this.formoptions = new ColumnFormOption();
                    }

                    return this.formoptions;
                }
            }

            private ColumnEditOption SecurityEditOption
            {
                get
                {
                    if (this.editoptions == null)
                    {
                        this.editoptions = new ColumnEditOption();
                    }

                    return this.editoptions;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Align
        {
            left,
            center,
            right
        }

        public enum Formatter
        {
            text,
            select,
            checkbox,
            currency,
            number,
            integer,
            date,
            email
        }

        /// <summary>
        /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:search_config
        /// </summary>
        public class ColumnSearchOption
        {
            private ComparisonOperator _comparisonOperator;

            /// <summary>
            /// Determines the comparison operators that are applied to the this column.
            /// This option is used only in advanced/single field searching.
            /// If not set all the available options will be used.
            /// <remarks>
            /// The algorithm to convert Bit Flags to string[] is inspired by System.Enum.InternalFlagsFormat(RuntimeType eT, object value)
            /// </remarks>
            /// </summary>
            public string[] sopt
            {
                get
                {
                    ulong num = (ulong)_comparisonOperator;
                    string[] names = Enum.GetNames(typeof(ComparisonOperator));
                    ulong[] values = Array.ConvertAll<ComparisonOperator, ulong>((ComparisonOperator[])Enum.GetValues(typeof(ComparisonOperator)), x => (ulong)x);
                    int index = values.Length - 1;

                    List<string> list = new List<string>();
                    ulong temp = num;
                    while (index >= 0)
                    {
                        if ((index == 0) && (values[index] == 0))
                        {
                            break;
                        }
                        if ((num & values[index]) == values[index])
                        {
                            num -= values[index];
                            list.Insert(0, names[index]);
                        }
                        index--;
                    }

                    return list.Count == 0 ? null : list.ToArray();
                }
            }

            public ComparisonOperator ComparisonOperator
            {
                set
                {
                    _comparisonOperator = value;
                }
            }
        }

        public class ColumnEditrule
        {
            public bool? edithidden { get; set; }
            public bool? required { get; set; }
            public bool? custom { get; set; }
            public JSFunction custom_func { get; set; }
            public bool? number { get; set; }
            public bool? integer { get; set; }
            public int? minValue { get; set; }
            public int? maxValue { get; set; }
        }
        public static ColumnEditrule GetColumnEditrule()
        {
            return new ColumnEditrule();
        }

        /// <summary>
        /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:common_rules#edittype
        /// </summary>
        public class ColumnEditOption
        {
            #region edittype select
            public string dataUrl { get; set; }
            #endregion

            #region edittype text
            public int? size { get; set; }
            public int? maxlength { get; set; }
            #endregion

            #region edittype text
            public string value { get; set; }
            #endregion

            #region edittype textarea
            public string rows { get; set; }
            public string cols { get; set; }
            #endregion

            #region edittype defaultValue
            public string defaultValue { get; set; }
            #endregion

            #region  dataInit
            public JSFunction dataInit { get; set; }
            #endregion
        }

        public enum ColumnEditorType
        {
            text,
            textarea,
            select,
            checkbox,
            password
        }
        public class ColumnFormOption
        {
            public string elmprefix;
            public string elmsuffix;
        }

        public class ColumnFormatoptions
        {
            #region  integer, number,currency
            public string thousandsSeparator { get; set; }
            public string defaulValue { get; set; }
            #endregion

            #region  number,currency
            public string decimalSeparator { get; set; }
            public int decimalPlaces { get; set; }
            #endregion

            #region  currency
            public string suffix { get; set; }
            #endregion

            #region
            public bool disabled { get; set; }
            #endregion
        }
    }
}
