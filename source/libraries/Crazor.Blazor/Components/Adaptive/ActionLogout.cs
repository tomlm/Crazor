namespace Crazor.Blazor.Components.Adaptive
{
    public class ActionLogout : ActionExecute
    {
        public ActionLogout()
        {
            this.Title = "Logout";
            this.Verb = Constants.LOGOUT_VERB;
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
        }
    }
}
