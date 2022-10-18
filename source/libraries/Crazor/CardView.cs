using AdaptiveCards;
using DataAnnotationsValidator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Diag = System.Diagnostics;

namespace Crazor
{
    public class CardView : RazorPage
    {
        private static XmlSerializer _cardSerializer = new XmlSerializer(typeof(AdaptiveCard));
        private static XmlWriterSettings _settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };

        public CardView()
        {
        }

        [JsonIgnore]
        public IUrlHelper UrlHelper { get; set; }

        [JsonIgnore]
        public string Name { get; set; } = String.Empty;

        [JsonIgnore]
        public CardApp App { get; set; }

        [JsonIgnore]
        public AdaptiveCardInvokeAction? Action { get; set; }

        [JsonIgnore]
        public IView? RazorView { get; set; }

        [JsonIgnore]
        public Dictionary<string, HashSet<string>> ValidationErrors { get; set; } = new Dictionary<string, HashSet<string>>();

        [JsonIgnore]
        public bool IsModelValid { get; set; } = true;

        /// <summary>
        /// ExecuteAsync is disabled because the default writes the output directly to 
        /// the response, and we need to process it directly.
        /// </summary>
        /// <returns></returns>
        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void OnLoadCardContext(ViewContext viewContext)
        {
            var method = GetMethod("OnInitialize");
            if (method != null)
            {
                method.Invoke(this, null);
            }
        }

        public async virtual Task<bool> OnVerbAsync(AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(this.App);
            this.Action = action;

            // Process BindProperty tags
            BindProperties();

            var verb = action.Verb;
            MethodInfo? verbMethod = null;

            if (verb == Constants.LOADPAGE_VERB)
            {
                // LoadPage verb should invoke this method FIRST before validation, as this method should load the model.
                verbMethod = GetMethod($"On{verb}") ?? GetMethod(verb);
                if (verbMethod != null)
                {
                    await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, (JObject?)this.Action?.Data));
                }

                ValidateModel();
                return true;
            }

            // otherwise, validate Model first so verb can check Model.IsValid property to decide what to do.
            ValidateModel();

            verbMethod = GetMethod($"On{verb}") ?? GetMethod(verb);
            if (verbMethod != null)
            {
                await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, (JObject?)this.Action?.Data));
                return true;
            }

            // Default implementation of CLOSE and CANCEL calls CloseView() or CancelView() appropriately.
            switch (verb)
            {
                case Constants.CLOSE_VERB:
                    this.CloseView(this.GetModel());
                    return true;

                case Constants.CANCEL_VERB:
                    this.CancelView();
                    return true;
            }

            // Otherwise, if a verb matches a view just navigate to it.
            if (App.HasView(verb))
            {
                ShowCard(verb);
                return true;
            }

            return false;
        }

        private void BindProperties()
        {
            JObject? data = (JObject?)this.Action?.Data;
            if (data != null)
            {
                foreach (var property in data.Properties())
                {
                    var parts = property.Name.Split('.');

                    // if root is [BindProperty]
                    var prop = this.GetType().GetProperty(parts[0]);
                    if (prop != null &&
                        (prop.Name == "Model" || prop.GetCustomAttribute<BindPropertyAttribute>() != null))
                    {
                        object obj = this;
                        foreach (var part in parts.Take(parts.Count() - 1))
                        {
                            obj = obj.GetPropertyValue(part);
                        }
                        var targetProperty = obj.GetType().GetProperty(parts.Last());
                        if (targetProperty != null)
                        {
                            if (property.Value != null)
                            {
                                targetProperty.SetValue(obj, property.Value.ToObject(targetProperty.PropertyType));
                            }
                            else
                            {
                                targetProperty.SetValue(obj, (targetProperty.PropertyType.IsValueType) ? Activator.CreateInstance(targetProperty.PropertyType) : null);
                            }
                        }
                    }
                }
            }
        }

        private void ValidateModel()
        {
            // validate root object model
            var validator = new DataAnnotationsValidator.DataAnnotationsValidator();

            // do shallow validation for root level properties
            var validationResults = new List<ValidationResult>();
            this.IsModelValid = validator.TryValidateObject(this, validationResults);
            AddValidationResults(String.Empty, validationResults);

            var model = this.GetModel();
            if (model != null)
            {
                validationResults = new List<ValidationResult>();

                if (!validator.TryValidateObjectRecursive(this.GetModel(), validationResults))
                {
                    this.IsModelValid = false;
                    AddValidationResults($"Model.", validationResults);
                }
            }

            // for complex types do a recursive deep validation. We can't
            // do this at the root because CardView is too complicated for a deep compare.
            foreach (var property in this.GetType().GetProperties().Where(p => p.GetCustomAttribute<BindPropertyAttribute>() != null &&
                                                                              !p.PropertyType.IsValueType &&
                                                                              p.PropertyType != typeof(string)))
            {
                validationResults = new List<ValidationResult>();
                var value = property.GetValue(this);
                if (!validator.TryValidateObjectRecursive(value, validationResults))
                {
                    this.IsModelValid = false;
                    AddValidationResults($"{property.Name}.", validationResults);
                }
            }
        }

        private void AddValidationResults(string prefix, List<ValidationResult> validationResults)
        {
            foreach (var result in validationResults)
            {
                foreach (var member in result.MemberNames)
                {
                    var path = $"{prefix}{member}";
                    if (!ValidationErrors.TryGetValue(path, out var list))
                    {
                        list = new HashSet<string>();
                        ValidationErrors[path] = list;
                    }

                    if (result.ErrorMessage != null)
                    {
                        list.Add(result.ErrorMessage);
                    }
                }
            }
        }

        public virtual object? GetModel()
        {
            return this.ViewBag.Model;
        }

        public virtual string GetRoute()
        {
            //var routeAttribute = this.GetType().GetCustomAttribute<RouteAttribute>();
            //if (routeAttribute != null)
            //{
            //    var template = routeAttribute.Template.TrimStart('/', '~');
            //    var parts = template.Split('/');
            //    StringBuilder sb = new StringBuilder();
            //    sb.Append(App.GetRoute());
            //    foreach(var part in parts)
            //    {
            //        if (part.StartsWith('{') && part.EndsWith('}'))
            //        {
            //            var path = part.TrimStart('{').TrimEnd('}');
            //            var value = ObjectPath.GetPathValue<object>(this, path.TrimEnd('?'));
            //            if (value != null)
            //            {
            //                sb.Append($"/{value}");
            //            }
            //            else if (!path.EndsWith('?'))
            //            {
            //                throw new ArgumentNullException($"{part} not found in {template}");
            //            }
            //        }
            //        else
            //        {
            //            sb.Append($"/{part}");
            //        }
            //    }

            //    return sb.ToString();
            //}
            // else 
            if (this.Name == "Default")
            {
                return App.GetRoute();
            }
            else
            {
                return $"{App.GetRoute()}/{this.Name}";
            }
        }

        /// <summary>
        /// Navigate to screen name.
        /// </summary>
        /// <param name="screenName"></param>
        public void ShowCard(string cardName, object? model = null)
        {
            this.App?.ShowCard(cardName, model);
        }

        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Default)
        {
            this.App?.AddBannerMessage(text, style);
        }

        public void CloseView(object? result = null)
        {
            // pop screen
            // grab current view
            // restore screen with screenstate
            // set result
            this.App?.Close(new CardResult()
            {
                Name = this.Name,
                Result = result,
                Success = true
            });
        }

        public void CancelView(string? message = null)
        {
            this.App?.Close(new CardResult()
            {
                Name = this.Name,
                Message = message,
                Success = false
            });
        }

        private List<object?>? GetMethodArgs(MethodInfo? method, JObject? data)
        {
            ArgumentNullException.ThrowIfNull(method);

            List<object?> args = new List<object?>();
            if (data != null)
            {
                foreach (var parm in method.GetParameters())
                {
                    if (parm.Name?.ToLower() == "id")
                    {
                        if (Action.Id != null)
                        {
                            args.Add(Action.Id);
                        }
                        else if (data.TryGetValue(Constants.IDDATA_KEY, out var id))
                        {
                            args.Add(id.ToString());
                        }
                    }
                    else
                    {
                        var prop = data.Properties().Where(p => p.Name.ToLower() == parm?.Name?.ToLower()).SingleOrDefault();
                        if (prop != null)
                        {
                            var arg = prop.Value.ToObject(parm.ParameterType);
                            args.Add(arg);
                        }
                        else
                        {
                            args.Add(parm.ParameterType.IsValueType ? Activator.CreateInstance(parm.ParameterType) : null);
                        }
                    }
                }
            }
            return args;
        }

        public async Task<object?> InvokeMethodAsync(MethodInfo? verbMethod, List<object?>? args = null)
        {
            ArgumentNullException.ThrowIfNull(verbMethod);

            if (verbMethod.ReturnType.Name == "Task")
            {
                await ((Task?)verbMethod.Invoke(this, args?.ToArray()) ?? throw new Exception("Task not returned from async verb!"));
                return null;
            }
            else if (verbMethod.ReturnType.Name == "Task`1")
            {
                var task = verbMethod.Invoke(this, args?.ToArray());
                await (Task)task;
                var result = task.GetType().GetProperty("Result", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).GetValue(task);
                return result;
            }
            else
            {
                return verbMethod.Invoke(this, args?.ToArray());
            }
        }

        public virtual async Task OnCardResumeAsync(CardResult screenResult, CancellationToken ct)
        {
            if (screenResult.Success)
            {
                MethodInfo? verbMethod = GetMethod($"On{screenResult.Name}Completed") ?? GetMethod($"{screenResult.Name}Completed");
                if (verbMethod != null)
                {
                    await InvokeMethodAsync(verbMethod, new List<object?>() { screenResult.Result });
                }
            }
            else
            {
                MethodInfo? verbMethod = GetMethod($"On{screenResult.Name}Canceled") ?? GetMethod($"{screenResult.Name}Canceled");
                if (verbMethod != null)
                {
                    await InvokeMethodAsync(verbMethod, new List<object?>() { screenResult.Message });
                }
                else
                {
                    verbMethod = GetMethod($"OnCanceled") ?? GetMethod($"Canceled");
                    if (verbMethod != null)
                    {
                        await InvokeMethodAsync(verbMethod, new List<object?>() { screenResult.Message });
                    }
                }
            }
        }

        /// <summary>
        /// Bind View
        /// </summary>
        /// <param name="viewName">view name</param>
        /// <returns>bound card</returns>
        public async Task<AdaptiveCard?> BindView(IServiceProvider services)
        {
            string xml = String.Empty;
            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    this.ViewContext.Writer = writer;
                    await this.RazorView!.RenderAsync(this.ViewContext);
                    xml = writer.ToString().Trim();
                }

                if (!xml.StartsWith("<?xml"))
                {
                    xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n{xml}";
                }
                Diag.Debug.WriteLine(xml);

                var reader = XmlReader.Create(new StringReader(xml));
                var card = (AdaptiveCard?)_cardSerializer.Deserialize(reader);
                return card;
            }
            catch (Exception err)
            {
                var xerr = err as XmlException ?? err.InnerException as XmlException;
                if (xerr != null)
                {
                    var line = String.Join("\n", xml.Trim() + "\n\n".Split("\n").Skip(xerr.LineNumber - 2).Take(2));
                    throw new XmlException($"{xerr.Message}\n{line}", xerr.InnerException, xerr.LineNumber, xerr.LinePosition);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<AdaptiveChoice[]> OnSearchChoicesAsync(SearchInvoke search, IServiceProvider services)
        {
            var searchMethod = GetMethod($"Get{search.Dataset}Choices");
            if (searchMethod != null)
            {
                JObject data = JObject.FromObject(new
                {
                    queryText = search.QueryText,
                    top = search.QueryOptions.Top,
                    skip = search.QueryOptions.Skip
                });

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                return (AdaptiveChoice[])await InvokeMethodAsync(searchMethod, GetMethodArgs(searchMethod, data));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            throw new Exception($"Get{search.Dataset}Choices or Get{search.Dataset}ChoicesAsync not found!");
        }

        public MethodInfo? GetMethod(string methodPrefix)
        {
            var method = this.GetType().GetMethod($"{methodPrefix}Async");
            if (method == null)
            {
                method = this.GetType().GetMethod($"{methodPrefix}");
            }
            return method;
        }

        protected virtual JObject GetScopedMemory<MemoryAttributeT>()
                where MemoryAttributeT : Attribute
        {
            lock (this)
            {

                dynamic memory = new JObject();
                foreach (var property in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(MemoryAttributeT))))
                {
                    var val = property.GetValue(this);
                    memory[property.Name] = val != null ? JToken.FromObject(val) : null;
                }

                return memory;
            }
        }

    }

    public class CardView<AppT> : CardView
        where AppT : CardApp
    {
        // Summary:
        //     Gets the Model property of the Microsoft.AspNetCore.Mvc.Razor.RazorPage`1.ViewData
        //     property.
        public AppT App
        {
            get => (AppT)base.App;
        }
    }

    public class CardView<AppT, ModelT> : CardView<AppT>
        where AppT : CardApp
        where ModelT : class
    {
        // Summary:
        //     Gets the Model property of the Microsoft.AspNetCore.Mvc.Razor.RazorPage`1.ViewData
        //     property.
        public ModelT Model
        {
            get
            {
                if (ViewData.Model != null)
                {
                    return ViewData.Model;
                }

#pragma warning disable CS8603 // Possible null reference return.
                return default(ModelT);
#pragma warning restore CS8603 // Possible null reference return.
            }

            set
            {
                this.ViewData.Model = value;
                this.ViewContext.ViewData.Model = value;
            }
        }

        //
        // Summary:
        //     Gets or sets the dictionary for view data.
        [RazorInject]
        public ViewDataDictionary<ModelT>? ViewData { get; set; }

        public override void OnLoadCardContext(ViewContext viewContext)
        {
            ModelT? model = viewContext.ViewData.Model as ModelT;
            if (model == null)
            {
                if (viewContext.ViewData.Model is JToken jt)
                {
                    viewContext.ViewData.Model = (ModelT?)jt.ToObject(typeof(ModelT));
                }
                else
                {
                    viewContext.ViewData.Model = Activator.CreateInstance<ModelT>();
                }
            }
            this.ViewData = new ViewDataDictionary<ModelT>(viewContext.ViewData);
            base.OnLoadCardContext(viewContext);
        }

        public override object GetModel()
        {
            return this.Model;
        }
    }
}
