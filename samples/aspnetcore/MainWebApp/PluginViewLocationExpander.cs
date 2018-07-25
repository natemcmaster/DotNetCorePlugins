using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace MainWebApp
{
    public class PluginViewLocationExpander : IViewLocationExpander
    {
        private const string PLUGIN_KEY = "plugin";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            context.Values.TryGetValue(PLUGIN_KEY, out var pluginName);

            if (!string.IsNullOrWhiteSpace(pluginName))
            {
                var moduleViewLocations = new string[]
                {
                    $"/plugins/{pluginName}/Views/{{1}}/{{0}}.cshtml",
                    $"/plugins/{pluginName}/Views/Shared/{{0}}.cshtml"
                };

                viewLocations = moduleViewLocations.Concat(viewLocations);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var controllerName = context.ActionContext.ActionDescriptor.DisplayName;
            if (controllerName == null) // in case of render view to string
            {
                return;
            }

            // Get assembly name
            var pluginName = controllerName.Split('(', ')')[1];
            context.Values[PLUGIN_KEY] = pluginName;
        }
    }
}
