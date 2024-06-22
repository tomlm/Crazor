using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;

namespace Crazor.Blazor.Components.Adaptive
{
    /// <summary>
    /// Cancel the current view (this is just semantic sugar over closing current view and go show caller view)
    /// </summary>
    public class ActionCancel : ActionExecute
    {
        public ActionCancel()
        {
            this.Title = "Cancel";
            this.Verb = Constants.CANCEL_VERB;
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
        }

        [Parameter]
        public string? Message { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (Message != null)
            {
                if (this.Item.Data == null)
                    this.Item.Data = new JObject();

                var data = this.Item.Data as JObject;
                if (data != null)
                {
                    data[nameof(Message)] = Message;
                }
            }
        }

    }
}
