using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Crazor.Server
{
    public partial class CardActivityHandler
    {

        /// <summary>
        /// Invoked when a signIn invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnSignInInvokeAsync(turnContext, cancellationToken);
        }

        // NOTE: OnTeamsSigninVerifyStateAsync is only called by default for OnSignInInvokeAsync(). There is no reason to define that I can determine, since we are
        // defining OnSigninInvokeAsync().
        // protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)


    }
}