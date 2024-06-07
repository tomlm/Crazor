using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Crazor.Blazor.Components.Adaptive
{
    /// <summary>
    /// ShowView(Route) action
    /// </summary>
    public class ActionShowView : ActionExecute
    {
        public ActionShowView()
        {
            Verb = Constants.SHOWVIEW_VERB;
        }

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
