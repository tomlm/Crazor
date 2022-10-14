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

        [Binding(BindingType.PropertyName)]
        [HtmlAttributeName]
        public string? Id { get; set; }

        [HtmlAttributeName]
        public string Binding { get; set; }

        public object BindingValue { get; set; }

        public CardView View { get; set; }

        [HtmlAttributeName(nameof(ValidationErrors))]
        public HashSet<string> ValidationErrors { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            throw new Exception("You must use async method ProcessAsync()");
        }

        public override void Init(TagHelperContext context)
        {
            CardView cardView = null;
            var viewContext = ViewContext;
            while (cardView == null)
            {
                // ((RazorView)page.ViewContext.View).RazorPage;
                if (viewContext.View is RazorView rv)
                {
                    if (rv.RazorPage is CardView cv)
                    {
                        cardView = cv;
                        break;
                    }
                    else
                    {
                        ViewContext = rv.RazorPage.ViewContext;
                    }
                }
                break;
            }
            if (cardView != null)
            {
                var id = Id ?? Binding;
                if (!string.IsNullOrEmpty(id))
                {
                    if (cardView.ValidationErrors.TryGetValue(id, out var errors))
                    {
                        ValidationErrors = errors;
                    }
                }

                if (!string.IsNullOrEmpty(Binding))
                {
                    BindingValue = cardView.GetPropertyValue(Binding);
                }
            }

            base.Init(context);
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

        public string EmitAttributes(string[] ignore = null)
        {
            ignore = ignore ?? Array.Empty<string>();
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !ignore.Contains(p.Name) && p.GetCustomAttribute<HtmlAttributeNameAttribute>(true) != null);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                string attributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? property.Name;
                var bindValueAttribute = property.GetCustomAttribute<BindingAttribute>();
                var value = property?.GetValue(this);
                if (value == null && Binding != null && bindValueAttribute != null)
                {
                    switch(bindValueAttribute.Policy)
                    {
                        case BindingType.DisplayName:
                            var dna = property.GetCustomAttribute<DisplayNameAttribute>();
                            if (dna != null)
                            {
                                value = dna.DisplayName ?? Binding;
                            }
                            else
                            {
                                value = Binding;
                            }
                            break;
                        case BindingType.PropertyName:
                            value = Binding;
                            break;
                        case BindingType.Value:
                            value = BindingValue;
                            break;
                    }
                }

                if (value != null)
                {
                    if (property.PropertyType.IsEnum)
                    {
                        sb.AppendLine($"{attributeName}=\"{value}\" ");
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        sb.AppendLine($"{attributeName}=\"{value.ToString().ToLower()}\" ");
                    }
                    else
                    {
                        sb.AppendLine($"{attributeName}=\"{value.ToString()}\" ");
                    }
                }
            }

            return sb.ToString();
        }

        public string EmitDisplayName(string attributeName)
        {
            var property = GetType().GetProperty(attributeName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return string.Empty;
            }
            string displayName = Binding ?? Id ?? attributeName;
            var dna = property.GetCustomAttribute<DisplayNameAttribute>();
            if (dna != null)
            {
                displayName = dna.DisplayName;
            }
            var value = property.GetValue(this);
            return $"{attributeName}=\"{displayName}\"";
        }
    }
}
