// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
