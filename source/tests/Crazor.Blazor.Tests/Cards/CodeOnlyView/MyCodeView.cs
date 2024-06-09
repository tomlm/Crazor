using Crazor.Attributes;

namespace Crazor.Blazor.Tests.Cards.CodeOnlyView
{
    /// <summary>
    /// You don't have to use Razor for your view.  Simply derive from CardView and override BindCard() to return the AdaptiveCard
    /// </summary>
    [CardRoute("MyCode")]
    public class MyCodeView : CustomCardView
    {

        [SessionMemory]
        public int Counter { get; set; }

        public override async Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var card = new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock($"CodeOnly"),
                    new AdaptiveTextBlock($"Counter: {this.Counter}")
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveExecuteAction(){ Verb = nameof(OnIncrement), Title = "Increment"}
                }
            };
            System.Diagnostics.Debug.WriteLine(card.ToJson());
            return card;
        }

        public void OnIncrement()
            => this.Counter++;

    }
}
