using Newtonsoft.Json.Linq;
using YamlConverter;
using Azure.AI.OpenAI;
using Microsoft.Bot.Builder;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Text;

namespace Crazor.AI.Recognizers
{
    public class FormRecognizer : FunctionsRecognizer
    {

        public FormRecognizer(OpenAIClient openAIClient) :
            base(openAIClient)
        {
            Functions.Add(new FunctionDefinition($"{FormFunctions.ASSIGN}('property', 'value')", "assign to a property or property array a single value.", "My name is joe => ASSIGN('name', 'joe')", "My like frogs and dogs => ASSIGN('likes', ['frogs','dogs'])"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.CLEAR}('property')", "clear the value for a property (setting it to null).", "Forget my name => CLEAR('name')"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.REMOVE}('property', 'value')", "remove a value from a property or collection.", "Remove apple from cart => REMOVE('cart', 'apple')"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.CHANGE}('property')", "a request to change the property but with no value provided.", "I want to change my name => CHANGE('name')"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.CANCEL}()", "cancels the current form dialog.", "I don't want to do this => CANCEL()"));
            //Functions.Add(new FunctionDefinition($"{FormFunctions.APPROVAL}(value)", "a confirmation to continue or do something, the value is true or false.", "Yes, please => APPROVAL(true)", "No, thanks => APPROVAL(false)", "make it so => APPROVAL(true)", "not now => APPROVAL(false)"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.RESET}()", "resets all the properties values to null to start the form over from scratch.", "Start over => RESET()"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.QUESTION}('sentence?')", "When the user asks a question this captures the sentence of the question.", "What is your name ? => QUESTION('What is your name?')"));
            //Functions.Add(new FunctionDefinition($"{FormFunctions.SHOW}()", "Show the current values for the form.", "Show me what you have => SHOW()", "What data do you have => SHOW()"));
            //Functions.Add(new FunctionDefinition($"{FormFunctions.OPTIONS}('property')", "The user has requested help for a property.", "what are my options for <property> => HELP(<property> )"));
            //Functions.Add(new FunctionDefinition($"{FormFunctions.VALUE}('value')", "A value in answer to previous question.", "Bob White=> VALUE('Bob White')"));
            Functions.Add(new FunctionDefinition($"{FormFunctions.SHOWPROPERTY}('property')", "Request to describe the value of a property.", "What is my name => SHOWPROPERTY('name')"));

        }

        public async virtual Task<RecognizerResult> RecognizeAsync(object model, string text, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var property in model.GetType().GetProperties())
            {
                sb.AppendLine($"  * {property.PropertyType.Name} {property.Name} - {property.GetPropertyLabel()}");
            }

            var instructions = $"The properties for the form are:\n{sb.ToString()}";

            // call OpenAI recognizer
            var result = await base.RecognizeAsync("gpt-3.5-turbo", text, instructions, cancellationToken);

            if (result.Intents.ContainsKey(FormRecognizer.FUNCTIONS_INTENT))
            {
                // var lastPrompt = dc.State.GetStringValue(cardView.LASTPROMPT) ?? string.Empty;
                var commands2 = JToken.FromObject(result.Entities[FormRecognizer.FUNCTIONS_INTENT]!).ToObject<List<Function>>()!;

                // cull out hallucination of ASSIGN(prop, null)
                commands2 = commands2.Where(r =>
                {
                    var value = r.Args.Skip(1).FirstOrDefault();
                    if (r.Name == FormFunctions.ASSIGN && (value == null || string.Compare(value?.ToString()?.Trim(), "null", ignoreCase: true) == 0))
                        return false;
                    return true;
                }).ToList();

                result.Entities[FormRecognizer.FUNCTIONS_INTENT] = JArray.FromObject(commands2);
            }
            return result;
        }

    }
}

