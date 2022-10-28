using AdaptiveCards;
using Crazor.Exceptions;
using Crazor.Interfaces;
using Crazor.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Bot.Schema;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Diag = System.Diagnostics;

namespace Crazor
{
    public class CardView<AppT> : RazorPage, ICardView
        where AppT : CardApp
    {
        private static XmlSerializer _cardSerializer = new XmlSerializer(typeof(AdaptiveCard));
        private static XmlWriterSettings _settings = new XmlWriterSettings()
        {
            Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
            Indent = true,
        };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CardView()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        [JsonIgnore]
        public IUrlHelper UrlHelper { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public AppT App { get; set; }

        [JsonIgnore]
        public AdaptiveCardInvokeAction Action { get; set; }

        [JsonIgnore]
        public IView RazorView { get; set; }

        [JsonIgnore]
        public Dictionary<string, HashSet<string>> ValidationErrors { get; set; } = new Dictionary<string, HashSet<string>>();

        [JsonIgnore]
        public bool IsModelValid { get; set; } = true;

        [JsonIgnore]
        CardApp ICardView.App { get => this.App; set => this.App = (AppT)value; }

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

            if (verb == Constants.LOADROUTE_VERB)
            {
                // LoadRoute verb should invoke this method FIRST before validation, as this method should load the model.
                verbMethod = GetMethod($"On{verb}") ?? GetMethod(verb);
                if (verbMethod != null)
                {
                    var data = JObject.FromObject(this.Action.Data);
                    var routeAttribute = this.GetType().GetCustomAttribute<RouteAttribute>();
                    if (routeAttribute != null)
                    {
                        var loadRoute = data.ToObject<LoadRouteModel>();
                        data = loadRoute!.GetDataForRoute(routeAttribute);
                    }

                    try
                    {
                        await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, data));
                    }
                    catch (CardRouteNotFoundException notFound)
                    {
                        AddBannerMessage(notFound.Message, AdaptiveContainerStyle.Attention);
                        CancelCard();
                    }
                    catch (Exception err)
                    {
                        if (err.InnerException is CardRouteNotFoundException notFound)
                        {
                            CancelCard(notFound.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                // I *think* the correct behavior is not NOT validate on LoadROUTE
                // ValidateModel();
                verb = Constants.REFRESH_VERB;
            }
            else
            {
                // otherwise, validate Model first so verb can check Model.IsValid property to decide what to do.
                ValidateModel();
            }

            verbMethod = GetMethod($"On{verb}") ?? GetMethod(verb);
            if (verbMethod != null)
            {
                await InvokeMethodAsync(verbMethod, GetMethodArgs(verbMethod, (JObject?)this.Action?.Data));
                return true;
            }

            // Default implementation of CLOSE and CANCEL calls CloseView() or CancelView() appropriately.
            switch (verb)
            {
                case Constants.OK_VERB:
                    if (this.IsModelValid)
                    {
                        this.CloseCard(this.GetModel());
                    }
                    return true;

                case Constants.CANCEL_VERB:
                    this.CancelCard();
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
                        (prop.Name == "Model" || prop.Name == "App" || prop.GetCustomAttribute<BindPropertyAttribute>() != null))
                    {
                        object obj = this;
                        foreach (var part in parts.Take(parts.Length - 1))
                        {
                            obj = obj.GetPropertyValue(part);
                        }
                        var targetProperty = obj.GetType().GetProperty(parts.Last());
                        if (targetProperty != null)
                        {
                            if (property.Value != null)
                            {
                                var val = (object)property.Value;
                                var targetType = targetProperty.PropertyType;
                                if (targetType.Name == "Nullable`1")
                                {
                                    targetType = targetType.GenericTypeArguments[0];
                                }

                                switch (targetType.Name)
                                {
                                    case "Byte":
                                        val = Convert.ToByte(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "Int16":
                                        val = Convert.ToInt16(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "Int32":
                                        val = Convert.ToInt32(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "Int64":
                                        val = Convert.ToInt64(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "UInt16":
                                        val = Convert.ToUInt16(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "UInt32":
                                        val = Convert.ToUInt32(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "UInt64":
                                        val = Convert.ToUInt64(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "Single":
                                        val = Convert.ToSingle(Convert.ToDouble(val.ToString()));
                                        break;
                                    case "Double":
                                        val = Convert.ToDouble(val.ToString());
                                        break;
                                    case "Boolean":
                                        val = Convert.ToBoolean(val.ToString());
                                        break;
                                    case "DateTime":
                                        val = Convert.ToDateTime(val.ToString());
                                        break;
                                    default:
                                        if (targetType.IsEnum)
                                        {
                                            val = Enum.Parse(targetType, val.ToString()!);
                                        }
                                        else
                                        {
                                            val = Convert.ChangeType(val.ToString(), targetType);
                                        }
                                        break;
                                }
                                targetProperty.SetValue(obj, val);
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
            var validator = new DataAnnotationsValidator();

            // do shallow validation for root level properties
            var validationResults = new List<ValidationResult>();
            this.IsModelValid = validator.TryValidateObject(this, validationResults);
            AddValidationResults(String.Empty, validationResults);

            var model = this.GetModel();
            if (model != null)
            {
                validationResults = new List<ValidationResult>();

                if (!validator.TryValidateObjectRecursive(model, validationResults))
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
            return String.Empty;
        }

        /// <summary>
        /// Navigate to card by name
        /// </summary>
        /// <param name="cardName">name of card </param>
        /// <param name="model">model to pass</param>
        public void ShowCard(string cardName, object? model = null)
        {
            this.App!.ShowCard(cardName, model);
        }

        /// <summary>
        /// Replace this card with another one 
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="model">model to pass</param>
        public void ReplaceCard(string cardName, object? model = null)
        {
            this.App!.ReplaceCard(cardName, model);
        }

        /// <summary>
        /// Add a banner message to be displayed to the viewer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Default)
        {
            this.App?.AddBannerMessage(text, style);
        }

        /// <summary>
        /// Close the current card, optionalling returning the result
        /// </summary>
        /// <param name="result">the result to return to the current caller</param>
        public void CloseCard(object? result = null)
        {
            this.App?.CloseCard(new CardResult()
            {
                Name = this.Name,
                Result = result,
                Success = true
            });
        }

        /// <summary>
        /// Change the taskmodule status
        /// </summary>
        /// <param name="status"></param>
        public void CloseTaskModule(TaskModuleStatus status)
        {
            App.CloseTaskModule(status);
        }

        /// <summary>
        /// Cancel the current card, returning a message
        /// </summary>
        /// <param name="message">optional message to return.</param>
        public void CancelCard(string? message = null)
        {
            this.App?.CloseCard(new CardResult()
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
                        if (Action!.Id != null)
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
                if (task != null)
                {

                    await (Task)task;
                    var result = task!.GetType().GetProperty("Result", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)!.GetValue(task);
                    return result;
                }
                throw new ArgumentNullException(verbMethod.Name);
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

                if (Debugger.IsAttached)
                {
                    var cardPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "card.json");
                    Diag.Debug.WriteLine($"AdaptiveCard JSON available at:\n{cardPath}");
                    File.WriteAllText(cardPath, JsonConvert.SerializeObject(card, Newtonsoft.Json.Formatting.Indented));
                }
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

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                return (AdaptiveChoice[])await InvokeMethodAsync(searchMethod, GetMethodArgs(searchMethod, data));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
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
                if (ViewData!.Model != null)
                {
                    return ViewData.Model;
                }

#pragma warning disable CS8603 // Possible null reference return.
                return default(ModelT);
#pragma warning restore CS8603 // Possible null reference return.
            }

            set
            {
                this.ViewData!.Model = value;
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
