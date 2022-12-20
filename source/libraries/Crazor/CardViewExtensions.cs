// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Crazor.Attributes;
using Crazor.Interfaces;
using Crazor.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

        public static void Validate(this ICardView cardView)
        {
            // validate root object model
            var validator = new DataAnnotationsValidator();

            // do shallow validation for root level properties
            var validationResults = new List<ValidationResult>();
            cardView.IsModelValid = validator.TryValidateObject(cardView, validationResults);
            cardView.AddValidationResults(String.Empty, validationResults);

            // for complex types do a recursive deep validation. We can't
            // do this at the root because CardView is too complicated for a deep compare.
            foreach (var property in cardView.GetBindableProperties()
                                        .Where(p => !p.PropertyType.IsValueType && p.PropertyType != typeof(string)))
            {
                validationResults = new List<ValidationResult>();
                var value = property.GetValue(cardView);
                if (!validator.TryValidateObjectRecursive(value, validationResults))
                {
                    cardView.IsModelValid = false;
                    cardView.AddValidationResults($"{property.Name}.", validationResults);
                }
            }
        }

        public static void AddValidationResults(this ICardView cardView, string prefix, List<ValidationResult> validationResults)
        {
            foreach (var result in validationResults)
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
        }
    }
}
