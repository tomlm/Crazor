// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrazorDemoBot.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            HostUri = configuration.GetValue<Uri>("HostUri");
        }

        public Uri HostUri { get; set; }

        public void OnGet()
        {

        }
    }
}