using AdaptiveCards;
using Crazor;
using Crazor.Attributes;

namespace CrazorDemoBot.Cards.CodeOnlyView
{
    /// <summary>
    /// You don't have to use Razor for your view.  Simply derive from CardView and override BindCard() to return the AdaptiveCard
    /// </summary>
    public class MyCodeView : CardView<CodeOnlyViewApp>
    {

        [SessionMemory]
        public int Counter { get; set; }

        /// <summary>
        /// Override BindCard() to use code to create the Adaptive Card.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>AdaptiveCard for this view</returns>
        public override async Task<AdaptiveCard?> BindCard(CancellationToken cancellationToken)
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
                    new AdaptiveExecuteAction() { Verb=nameof(OnIncrement), Title = "Increment"}
                }
            };
        }

        public void OnIncrement()
        {
            this.Counter++;
        }
    }
}
