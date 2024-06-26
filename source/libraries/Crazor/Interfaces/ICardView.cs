using Microsoft.Bot.Schema;
using System.Reflection;

namespace Crazor.Interfaces
{
    public interface ICardView
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        string Name { get; }

        /// <summary>
        /// App reference
        /// </summary>
        CardApp App { get; set; }

        /// <summary>
        /// Validation errors for the current view.
        /// </summary>
        Dictionary<string, HashSet<string>> ValidationErrors { get; }

        /// <summary>
        /// Is the current view valid?
        /// </summary>
        bool IsModelValid { get; set; }

        /// <summary>
        /// Get any custom route data for the cardview
        /// </summary>
        /// <returns></returns>
        string GetRoute();

        /// <summary>
        /// Get Model (specificallly @model style model)
        /// </summary>
        /// <returns></returns>
        object? GetModel();

        /// <summary>
        /// Set the Model (if returned by GetModel)
        /// </summary>
        /// <param name="model"></param>
        void SetModel(object model);

        /// <summary>
        /// Enumerate properties on the view which are persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetPersistentProperties();

        /// <summary>
        /// Enumerate properties on the view which are bindable and persistent
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> GetBindableProperties();

        /// <summary>
        /// Render the card 
        /// </summary>
        /// <param name="isPreview">IsPreview is signal that anonymous preview card should be returned.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task AdaptiveCard</returns>
        Task<AdaptiveCard?> RenderCardAsync(bool isPreview, CancellationToken cancellationToken);

        /// <summary>
        /// OnInitializedAsync() - Initalize members
        /// </summary>
        /// <remarks>
        /// This will be called only once to initialize the instance data of the cardview.
        /// </remarks>
        /// <returns>Task</returns>
        Task OnInitializedAsync(CancellationToken cancellationToken);

        /// <summary>
        /// OnValidateModelAsync() - Called to validate model, 
        /// </summary>
        /// <remarks>
        /// sets IsModelValid and fill ValidationErrors collection
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task OnValidateModelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// OnActionAsync() - Called to process an incoming verb action.
        /// </summary>
        /// <remarks>
        /// The default implementation uses reflection to find the name of the method and invoke it.
        /// </remarks>
        /// <param name="action">the action to process</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task OnActionAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken);

        /// <summary>
        /// OnResumeView() - Called when a CardResult has returned back to this view
        /// </summary>
        /// <remarks>
        /// Override this to handle the result that is returned to the card from a child view.
        /// When a view is resumed because a child view has completed this method will
        /// be called giving you an opportunity to do something with the result of the child view.
        /// </remarks>
        /// <param name="cardResult">the card result</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>task</returns>
        Task OnResumeViewAsync(CardResult cardResult, CancellationToken cancellationToken);

        /// <summary>
        /// Called to search for choices.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="services"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, CancellationToken cancellationToken);
    }
}
