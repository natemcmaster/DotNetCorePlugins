using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plugin.Abstractions;

namespace MainWebApp
{
    public class IndexModel : PageModel
    {
        public IndexModel(IEnumerable<IPluginLink> pluginLinks)
        {
            Links = pluginLinks.Select(p => p.GetHref()).ToArray();
        }

        public string[] Links { get; }

        public void OnGet()
        {
        }
    }
}
