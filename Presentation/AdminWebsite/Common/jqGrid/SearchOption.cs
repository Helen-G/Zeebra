using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:singe_searching#options
    /// </summary>
    public class SearchOption
    {
        private List<KeyValuePair<string, AdvancedFilter>> _filterTemplates;

        /// <summary>
        /// This event onInitializeSearch occurs only once when the modal is created.
        /// Subscribe this event to set the first user defined template as the initial template.
        /// </summary>
        public JSFunction onInitializeSearch
        {
            get
            {
                return _filterTemplates.Count == 0 ? null : new JSFunction { Definition = @"function(){jQuery('select.ui-template').val('0').change();}" };
            }
        }

        public SearchOption()
        {
            _filterTemplates = new List<KeyValuePair<string, AdvancedFilter>>();
        }

        /// <summary>
        /// Indicates whether to allow advanced searching.
        /// null means false.
        /// </summary>
        public bool? multipleSearch { get; set; }

        /// <summary>
        /// Indicates whether the complex condfitions is enabled in advanced searching.
        /// null means false.
        /// </summary>
        public bool? multipleGroup { get; set; }

        /// <summary>
        /// Indicates whether the query button is visible in advanced searching.
        /// null means false.
        /// </summary>
        public bool? showQuery { get; set; }

        /// <summary>
        /// This option is valid only in navigator options. If set to true the dialog appear automatically when the grid is constructed for first time
        /// </summary>
        public bool? showOnLoad { get; set; }

        /// <summary>
        /// Defines the name of the templates used for easy user input.
        /// </summary>
        public string[] tmplNames
        {
            get
            {
                if (_filterTemplates.Count == 0)
                {
                    return null;
                }
                else
                {
                    return Array.ConvertAll<KeyValuePair<string, AdvancedFilter>, string>(_filterTemplates.ToArray(), x => x.Key);
                }
            }
        }

        /// <summary>
        /// If defined this should correspond to the tmplNames
        /// </summary>
        public AdvancedFilter[] tmplFilters
        {
            get
            {
                if (_filterTemplates.Count == 0)
                {
                    return null;
                }
                else
                {
                    return Array.ConvertAll<KeyValuePair<string, AdvancedFilter>, AdvancedFilter>(_filterTemplates.ToArray(), x => x.Value);
                }
            }
        }

        public SearchOption AddFilterTemplate(string name, AdvancedFilter filter)
        {
            _filterTemplates.Add(new KeyValuePair<string, AdvancedFilter>(name, filter));
            return this;
        }

        public SearchOption DisableMultipleSearch()
        {
            this.multipleSearch = null;
            return this;
        }

        public SearchOption ShowOnLoad()
        {
            this.showOnLoad = true;
            return this;
        }
    }
}
