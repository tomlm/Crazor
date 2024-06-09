namespace Crazor.Blazor.Components.Adaptive
{
    public class ActionLogin : ActionExecute
    {
        public ActionLogin()
        {
            this.Title = "Login";
            this.Verb = Constants.LOGIN_VERB;
            this.AssociatedInputs = AdaptiveAssociatedInputs.None;
        }
    }
}
