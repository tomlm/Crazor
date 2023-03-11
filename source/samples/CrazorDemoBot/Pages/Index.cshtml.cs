// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using System.Net;
using Microsoft.Graph;

namespace CrazorDemoBot.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            HostUri = configuration.GetValue<Uri>("HostUri");
            _graphServiceClient = graphServiceClient;;
        }

        public Uri HostUri { get; set; }

        public async Task OnGet()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();;
            ViewData["GraphApiResult"] = user.DisplayName;;

        }
    }
}