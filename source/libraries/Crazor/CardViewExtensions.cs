using Crazor.Attributes;
using Crazor.Interfaces;
using Crazor.Validation;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Crazor
{
    public static class CardViewExtensions
    {
        public static async Task<bool> InvokeVerbAsync(this ICardView cardView, AdaptiveCardInvokeAction action, CancellationToken cancellationToken)
        {
            var verbMethod = cardView.GetMethod(action.Verb);
            if (verbMethod != null)
            {
                await cardView.InvokeMethodAsync(verbMethod, cardView.GetMethodArgs(verbMethod, (JObject?)action?.Data, cancellationToken));
                return true;
            }
            return false;
        }

        public static List<object?>? GetMethodArgs(this ICardView cardView, MethodInfo? method, JObject? data, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(method);

            List<object?> args = new List<object?>();
            if (data != null)
            {
                foreach (var parm in method.GetParameters())
                {
                    if (parm.ParameterType == typeof(CancellationToken))
                    {
                        args.Add(cancellationToken);
                    }
                    //else if (parm.Name?.ToLower() == "id")
                    //{
                    //    if (Action!.Id != null)
                    //    {
                    //        args.Add(Action.Id);
                    //    }
                    //    else if (data.TryGetValue(Constants.IDDATA_KEY, out var id))
                    //    {
                    //        args.Add(id.ToString());
                    //    }
                    //}
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

        public static async Task<object?> InvokeMethodAsync(this ICardView cardView, MethodInfo? verbMethod, List<object?>? args = null)
        {
            ArgumentNullException.ThrowIfNull(verbMethod);

            if (verbMethod.ReturnType.Name == "Task")
            {
                await ((Task?)verbMethod.Invoke(cardView, args?.ToArray()) ?? throw new Exception("Task not returned from async verb!"));
                return null;
            }
            else if (verbMethod.ReturnType.Name == "Task`1")
            {
                var task = verbMethod.Invoke(cardView, args?.ToArray());
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
                return verbMethod.Invoke(cardView, args?.ToArray());
            }

        }
        public static MethodInfo? GetMethod(this ICardView cardView, string methodName)
        {
            return cardView.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? cardView.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Utility method which validates a cardview using reflection and Data annotations
        /// </summary>
        /// <param name="cardView"></param>
        public static void ValidateModelWithAnnotations(this ICardView cardView)
        {
            // validate root object model
            var validator = new DataAnnotationsValidator();

            // do shallow validation for root level properties
            var validationResults = new List<ValidationResult>();
            cardView.IsModelValid = validator.TryValidateObject(cardView, validationResults);
            foreach (var validationResult in validationResults)
                cardView.AddValidationResult(String.Empty, validationResult);

            // for complex types do a recursive deep validation. We can't
            // do this at the root because CardView is too complicated for a deep compare.
            foreach (var property in cardView.GetBindableProperties()
                                        .Where(p => !p.PropertyType.IsValueType && p.PropertyType != typeof(string)))
            {
                validationResults = new List<ValidationResult>();
                var value = property.GetValue(cardView);
                if (value == null)
                {
                    // if this is required and null then we have an issue.
                    if (property.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        cardView.IsModelValid = false;
                        foreach (var validationResult in validationResults)
                            cardView.AddValidationResult($"{property.Name}.", validationResult);
                    }
                }
                else
                {
                    if (!validator.TryValidateObjectRecursive(value, validationResults))
                    {
                        cardView.IsModelValid = false;
                        foreach (var validationResult in validationResults)
                            cardView.AddValidationResult($"{property.Name}.", validationResult);
                    }
                }
            }
        }

        /// <summary>
        /// Utility method for CardView implementations to add a validation
        /// </summary>
        /// <param name="cardView"></param>
        /// <param name="prefix">like property name</param>
        /// <param name="result">validation result to add</param>
        public static void AddValidationResult(this ICardView cardView, string prefix, ValidationResult result)
        {
            foreach (var member in result.MemberNames)
            {
                var path = $"{prefix}{member}";
                if (!cardView.ValidationErrors.TryGetValue(path, out var list))
                {
                    list = new HashSet<string>();
                    cardView.ValidationErrors[path] = list;
                }

                if (result.ErrorMessage != null)
                {
                    list.Add(result.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// GetRouteHelper() - returns custom subpath for the ICardView
        /// </summary>
        /// <remarks>
        /// The default is to use reflection and [Route] to calculate the route
        /// </remarks>
        /// <returns>relative path to the card for deep linking</returns>
        public static string GetRoute(ICardView cardView)
        {
            StringBuilder sb = new StringBuilder();
            var routeAttr = cardView.GetType().GetCustomAttribute<CardRouteAttribute>();
            if (routeAttr != null)
            {
                var parts = routeAttr.Template.Split('/');
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    if (part.StartsWith('{') && part.EndsWith('}'))
                    {
                        parts[i] = ObjectPath.GetPathValue<string?>(cardView, part.Trim('{', '}', '?'), null)!;
                    }
                }
                sb.Append(String.Join('/', parts));
            }
            else
            {
                var name = (cardView.Name != Constants.DEFAULT_VIEW) ? cardView.Name : String.Empty;
                sb.Append($"/Cards/{cardView.App.Name}/{name}");
            }

            var fromCardQueryProperties = cardView.GetType().GetProperties().Where(p => p.GetCustomAttribute<FromCardQueryAttribute>() != null);
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            if (fromCardQueryProperties.Any())
            {
                foreach (var targetProperty in fromCardQueryProperties)
                {
                    var queryParameterName = targetProperty.GetCustomAttribute<FromCardQueryAttribute>()?.Name ?? targetProperty.Name;
                    if (queryParameterName != null)
                    {
                        var value = targetProperty.GetValue(cardView);
                        if (value != null)
                        {
                            queryParameters.Add(queryParameterName, value.ToString()!);
                        }
                    }
                }
            }
            if (queryParameters.Any())
            {
                sb.Append($"?{String.Join('&', queryParameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}");
            }

            return sb.ToString().TrimEnd('?');
        }
    }
}
