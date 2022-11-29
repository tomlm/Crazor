using AdaptiveCards;
using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace CrazorDemoBot.Cards.CodeOnlyView
{
    /// <summary>
    /// You don't have to use Razor for your view.  Simply derive from CardView and override BindCard() to return the AdaptiveCard
    /// </summary>
    [Route("MyCode")]
    public class MyCodeView : CardView
    {
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
                    new AdaptiveExecuteAction(){ Verb = nameof(OnIncrement), Title = "Increment"}
                }
            };
        }

        public void OnIncrement()
            => this.Counter++;
    }
}
