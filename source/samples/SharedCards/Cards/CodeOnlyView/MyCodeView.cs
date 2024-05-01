


using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace SharedCards.Cards.CodeOnlyView
{
    /// <summary>
    /// You don't have to use Razor for your view.  Simply derive from CardView and override RenderCardAsync() to return the AdaptiveCard
    /// </summary>
    /// <remarks>
    /// NOTE: The default implementation of OnAction is to use reflection to map incoming values to the the properties on the view. 
    /// Then RenderCardAsync() is called to bind the view model to the AdaptiveCard.
    /// </remarks>
    [CardRoute("MyCard")]
    public class MyCodeView : CustomCardView
    {
        [Inject]
        public IConfiguration Configuration { get; set; }

        [SessionMemory]
        public int Counter { get; set; }

        public override async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock($"Counter is {this.Counter}")
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveExecuteAction()
                    {
                        Verb = nameof(OnIncrement),
                        Title = "Increment"
                    }
                }
            };
        }

        public void OnIncrement()
            => this.Counter++;

    }
}
