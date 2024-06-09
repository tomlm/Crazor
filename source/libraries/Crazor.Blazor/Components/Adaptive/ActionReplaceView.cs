using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;

namespace Crazor.Blazor.Components.Adaptive
{
    /// <summary>
    /// ReplaceView(Route) action
    /// </summary>
    public class ActionReplaceView : ActionExecute
    {
        public ActionReplaceView()
        {
            Verb = Constants.REPLACEVIEW_VERB;
        }

        /// <summary>
        /// The route of the view to replace
        /// </summary>
        [Parameter]
        public string? Route { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (Route != null)
            {
                if (this.Item.Data == null)
                    this.Item.Data = new JObject();

                var data = this.Item.Data as JObject;
                if (data != null)
                {
                    data[nameof(Route)] = Route;
                }
            }
        }
    }
}
