using DataAnnotationsValidator;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using Crazor.Attributes;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Crazor.TagHelpers
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

        public CardView View { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            throw new Exception("You must use async method ProcessAsync()");
        }

        public override void Init(TagHelperContext context)
        {
            // find the View property, it's handy
            var viewContext = ViewContext;
            while (this.View == null)
            {
                // ((RazorView)page.ViewContext.View).RazorPage;
                if (viewContext.View is RazorView rv)
                {
                    if (rv.RazorPage is CardView cv)
                    {
                        this.View = cv;
                        break;
                    }
                    else
                    {
                        ViewContext = rv.RazorPage.ViewContext;
                    }
                }
                break;
            }

            base.Init(context);
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // get child content and capture it in our model so we can insert it in our .cshtml template
            ChildContent = await output.GetChildContentAsync();

            IHtmlHelper? htmlHelper = ViewContext.HttpContext.RequestServices.GetService<IHtmlHelper>();
            ArgumentNullException.ThrowIfNull(htmlHelper);

            // bind our view content
            (htmlHelper as IViewContextAware)!.Contextualize(ViewContext);
            var content = await htmlHelper.PartialAsync(_viewPath, this);

            output.TagName = null;
            output.Content.SetHtmlContent(content);
        }
    }
}
