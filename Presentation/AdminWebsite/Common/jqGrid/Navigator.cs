namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    /// <summary>
    /// http://www.trirand.com/jqgridwiki/doku.php?id=wiki:navigator
    /// </summary>
    public class Navigator
    {
        public Navigator()
        {
            this.Buttons = new ButtonOption
            {
                add = false,
                del = false,
                edit = false,
                search = false,
                refresh = false
            };
        }

        public ButtonOption Buttons { get; set; }

        public Navigator EnableAddButton()
        {
            Buttons.add = null;
            return this;
        }

        public Navigator EnableDelButton()
        {
            Buttons.del = null;
            return this;
        }

        public Navigator EnableEditButton()
        {
            Buttons.edit = true;
            return this;
        }
        public Navigator SetEditButtonEnable()
        {
            Buttons.edit = true;
            return this;
        }

        public Navigator DisableSearchButton()
        {
            Buttons.search = false;
            return this;
        }

        public Navigator EnableViewButton()
        {
            Buttons.view = true;
            return this;
        }

        public class ButtonOption
        {
            /// <summary>
            /// Enables or disables the add action in the Navigator.
            /// null means true.
            /// </summary>
            public bool? add { get; set; }
            /// <summary>
            /// Enables or disables the delete action in the Navigator.
            /// null means true.
            /// </summary>
            public bool? del { get; set; }
            /// <summary>
            /// Enables or disables the edit action in the Navigator.
            /// null means true.
            /// </summary>
            public bool? edit { get; set; }
            /// <summary>
            /// Enables or disables the refresh button in the Navigator.
            /// null means true.
            /// </summary>
            public bool? refresh { get; set; }
            /// <summary>
            /// Enables or disables the search button in the Navigator.
            /// null means true.
            /// </summary>
            public bool? search { get; set; }
            /// <summary>
            /// Enables or disables the view button in the Navigator.
            /// null means false.
            /// </summary>
            public bool? view { get; set; }
        }
    }
}
