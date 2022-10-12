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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        public string Name { get; set; } = String.Empty;

        [JsonIgnore]
        public CardApp? App { get; set; }

        [JsonIgnore]
        public AdaptiveCardInvokeAction? Action { get; set; }

        [JsonIgnore]
        public IView? RazorView { get; set; }

        [JsonIgnore]
        public Dictionary<string, HashSet<string>> ValidationErrors { get; set; } = new Dictionary<string, HashSet<string>>();

        [JsonIgnore]
        public bool IsModelValid { get; set; } = true;

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

            var validator = new DataAnnotationsValidator.DataAnnotationsValidator();
            var validationResults = new List<ValidationResult>();

            // Bind properties
            JObject? data = (JObject?)this.Action?.Data;
            if (data != null)
            {
                foreach (var property in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(BindPropertyAttribute))))
                {
                    var prop = data.Properties().Where(p => p.Name.ToLower() == property.Name.ToLower()).SingleOrDefault();
                    if (prop != null)
                    {
                        property.SetValue(this, prop.Value.ToObject(property.PropertyType));
                    }
                    else
                    {
                        property.SetValue(this, (property.PropertyType.IsValueType) ? Activator.CreateInstance(property.PropertyType) : null);
                    }
                }
            }

            // validate object model
            this.IsModelValid = validator.TryValidateObject(this, validationResults);
            foreach (var result in validationResults)
            {
                foreach (var member in result.MemberNames)
                {
                    if (!ValidationErrors.TryGetValue(member, out var list))
                    {

                        list = new HashSet<string>();
                        ValidationErrors[member] = list;
                    }

                    if (result.ErrorMessage != null)
                    {
                        list.Add(result.ErrorMessage);
                    }
                }
            }

            var verb = action.Verb;
            if (!String.IsNullOrEmpty(verb))
            {
                MethodInfo? verbMethod = GetMethod($"On{verb}") ?? GetMethod(verb);
                if (verbMethod != null)
                {
                    return await InvokeMethodAsync(verbMethod, GetVerbArgs(verbMethod));
                }

                if (App.HasView(verb))
                {
                    ShowCard(verb);
                    return true;
                }

                switch (verb.ToLower())
                {
                    case "close":
                        this.Close(this.GetModel());
                        break;
                    case "cancel":
                        this.Cancel();
                        break;
                }
            }
            return false;
        }

        public virtual object? GetModel()
        {
            return this.ViewBag.Model;
        }

        /// <summary>
        /// Navigate to screen name.
        /// </summary>
        /// <param name="screenName"></param>
        public void ShowCard(string cardName, object? model = null)
        {
            this.App?.ShowCard(cardName, model ?? this.App);
        }

        public void AddBannerMessage(string text, AdaptiveContainerStyle style = AdaptiveContainerStyle.Default)
        {
            this.App?.AddBannerMessage(text, style);
        }

        public void Close(object? result = null)
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

        public void Cancel(string? message = null)
        {
            this.App?.Close(new CardResult()
            {
                Name = this.Name,
                Message = message,
                Success = false
            });
        }

        private List<object?>? GetVerbArgs(MethodInfo? verbMethod)
        {
            ArgumentNullException.ThrowIfNull(verbMethod);

            List<object?> args = new List<object?>();
            JObject? data = (JObject?)this.Action?.Data;
            if (data != null)
            {
                foreach (var parm in verbMethod.GetParameters())
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

        public async Task<bool> InvokeMethodAsync(MethodInfo? verbMethod, List<object?>? args = null)
        {
            ArgumentNullException.ThrowIfNull(verbMethod);

            if (verbMethod.ReturnType == typeof(Task))
            {
                await ((Task?)verbMethod.Invoke(this, args?.ToArray()) ?? throw new Exception("Task not returned from async verb!"));
                return true;
            }
            else
            {
                verbMethod.Invoke(this, args?.ToArray());
                return true;
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

                //if (!xml.StartsWith("<?xml"))
                //{
                //    xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n{xml}";
                //}
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
                    var line = String.Join("\n", xml.Trim()+"\n\n".Split("\n").Skip(xerr.LineNumber - 2).Take(2));
                    throw new XmlException($"{xerr.Message}\n{line}", xerr.InnerException, xerr.LineNumber, xerr.LinePosition);
                }
                else
                {
                    throw;
                }
            }
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

    public class CardView<ModelT> : CardView
        where ModelT : class
    {
        public CardView()
        {
        }

        // Summary:
        //     Gets the Model property of the Microsoft.AspNetCore.Mvc.Razor.RazorPage`1.ViewData
        //     property.
        public ModelT Model
        {
            get
            {
                if (ViewData != null)
                {
                    return ViewData.Model;
                }

#pragma warning disable CS8603 // Possible null reference return.
                return default(ModelT);
#pragma warning restore CS8603 // Possible null reference return.
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
