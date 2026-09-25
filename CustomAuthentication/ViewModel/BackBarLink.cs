using System.Collections.Generic;

namespace CustomAuthentication.ViewModel
{
    // A single pill link rendered by Views/Shared/_BackBar.cshtml.
    public class BackBarLink
    {
        public string Text { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public object RouteValues { get; set; }

        public BackBarLink() { }

        public BackBarLink(string text, string controller, string action, object routeValues = null)
        {
            Text = text;
            Controller = controller;
            Action = action;
            RouteValues = routeValues;
        }
    }
}
