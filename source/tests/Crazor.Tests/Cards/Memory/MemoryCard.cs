using AdaptiveCards;
using Crazor.Attributes;

namespace Crazor.Tests.Cards.Memory
{
    [CardRoute("test")]
    public class Default : CustomCardView
    {
        public override async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return new AdaptiveCard("1.5")
            {
            };
        }
    }
}
