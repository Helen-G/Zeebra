using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class Menu
    {
        private readonly string _currentController, _currentAction;

        public Menu(string currentController, string currentAction)
        {
            _currentController = currentController;
            _currentAction = currentAction;
        }

        public List<MenuItem> Items { get; set; }

        public MenuItem ActiveMenuItem
        {
            get
            {
                return Items.SingleOrDefault(menuItem =>
                    menuItem.Controller == _currentController && menuItem.Action == _currentAction ||
                    menuItem.SubMenuItems != null && menuItem.SubMenuItems.Any(subMenuItem => subMenuItem == ActiveSubMenuItem));
            }
        }

        public MenuItem ActiveSubMenuItem
        {
            get
            {
                return Items.SelectMany(menuItem => menuItem.SubMenuItems ?? new List<MenuItem>()).SingleOrDefault(subMenuItem =>
                    subMenuItem.Controller == _currentController && subMenuItem.Action == _currentAction);
            }
        }

    }

    public class MenuItem
    {
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public List<MenuItem> SubMenuItems { get; set; }

        public MenuItem()
        {
            Controller = "Home";
            Action = "Index";
        }
    }
}