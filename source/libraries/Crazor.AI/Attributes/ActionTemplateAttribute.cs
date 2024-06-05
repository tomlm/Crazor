// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveExpressions;

namespace Crazor.AI.Attributes
{

    public class ActionTemplateAttribute : Attribute, IResponseTemplate
    {
        private string template;
        private Expression expression;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ActionTemplateAttribute(string? template)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ArgumentNullException.ThrowIfNull(template);
            Template = template;
        }

        public string Template
        {
            get => template;
            set
            {
                template = value;
                expression = AdaptiveExpressions.Expression.Parse($"`{template}`");
            }
        }

        public virtual string BindTemplate(IDictionary<string, object> data)
        {
            var (result, error) = expression.TryEvaluate(data);
            return result.ToString()!;
        }
    }
}
