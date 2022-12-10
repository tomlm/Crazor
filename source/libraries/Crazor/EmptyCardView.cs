// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Interfaces;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Crazor
{
    public class EmptyCardView : ICardView
    {
        public string Name { get; set; }

        public CardApp App { get; set; }

        public AdaptiveCardInvokeAction Action { get; set; }

        public Dictionary<string, HashSet<string>> ValidationErrors { get; set; } = new Dictionary<string, HashSet<string>>();

        public bool IsModelValid { get; set; }

        public void BindProperties(JObject data)
        {
            
        }

        public string GetRoute()
        {
            return "/Cards/Empty";
        }

        public void LoadState(CardViewState cardState)
        {
            
        }
        public void SaveState(CardViewState cardState)
        {
        }

        public Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnResumeView(CardResult screenResult, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<AdaptiveChoice[]> OnSearchChoices(SearchInvoke search, CancellationToken cancellationToken)
        {
            return Task.FromResult(Array.Empty<AdaptiveChoice>());
        }

        public Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdaptiveCard("1.0"))!;
        }

    }
}
