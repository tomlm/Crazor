using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microsoft.Bot.Cards
{
    /// <summary>
    /// Implements a tag helper as a Razor view as the template
    /// </summary>
    /// <remarks>
    ///     uses convention that /TagHelpers/ has razor template based views for tags
    ///     For a folder /TagHelpers/Foo
    ///     * FooTagHelper.cs -> Defines the properties with HtmlAttribute on it (derived from ViewTagHelper)
    ///     * default.cshtml -> Defines the template with Model=>FooTagHelper
    /// </remarks>
    public class RazorTagHelper : TagHelper
    {
        private string _viewPath;

        public RazorTagHelper()
        {
            _viewPath = $"~/TagHelpers/{GetType().Namespace!.Split('.').Last()}/Default.cshtml";
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public TagHelperContent? ChildContent { get; set; }

        [HtmlAttributeName]
        public string? Id { get; set; } 

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            throw new Exception("You must use async method ProcessAsync()");
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext is null)
            {
                throw new ArgumentNullException(nameof(ViewContext));
            }

            // get child content and capture it in our model so we can insert it in our output
            ChildContent = await output.GetChildContentAsync();

            IHtmlHelper? htmlHelper = ViewContext.HttpContext.RequestServices.GetService<IHtmlHelper>();
            ArgumentNullException.ThrowIfNull(htmlHelper);

            (htmlHelper as IViewContextAware)!.Contextualize(ViewContext);
            var content = await htmlHelper.PartialAsync(_viewPath, this);

            output.TagName = null;
            output.Content.SetHtmlContent(content);
        }
    }
}
