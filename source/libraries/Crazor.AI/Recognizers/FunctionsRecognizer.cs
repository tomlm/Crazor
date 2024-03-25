using Azure.AI.OpenAI;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.Bot.Builder;

namespace Crazor.AI.Recognizers
{
    /// <summary>
    /// Recognizer which recognizes multiple commands as a single "Functions" intent.
    /// </summary>
    public class FunctionsRecognizer
    {
        private const string IM_START = "<|im_start|>";
        private const string IM_END = "<|im_end|>";
        private const string STOP = "<|STOP";

        // public const string LASTPROMPT = "this._lastprompt";
        public const string FUNCTIONS_INTENT = "Functions";
        public const string NONE_INTENT = "None";

        private readonly OpenAIClient _openAIClient;

        public FunctionsRecognizer(OpenAIClient openAIClient)
        {
            _openAIClient = openAIClient;
        }

        /// <summary>
        /// Functions to recognize
        /// </summary>
        public List<FunctionDefinition> Functions = new List<FunctionDefinition>();

        public async virtual Task<RecognizerResult> RecognizeAsync(string model, string text, string instructions, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine("===== RECOGNIZE PROMPT");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<|im_start|>system\r\nI am a bot which excels at examining users text and identifying functions in it from the user text.\r\n");
            sb.AppendLine($"Today is: {DateTime.Now}");
            sb.AppendLine();
            if (!String.IsNullOrEmpty(instructions))
            {
                sb.AppendLine(instructions);
                sb.AppendLine();
            }
            sb.AppendLine("FUNCTIONLIST:");
            foreach (var intent in Functions)
            {
                sb.AppendLine($"* {intent.Signature} - {intent.Description}. Examples:");
                foreach (var example in intent.Examples)
                {
                    sb.AppendLine($"  - {example}");
                }
            }
            sb.AppendLine();
            sb.AppendLine(@"Transform the user text only (not the bot text) into a list of identified functions (from FUNCTIONLIST) using schema property name and not label.");
            //sb.AppendLine("Bot said:");
            //sb.AppendLine(lastPrompt);
            sb.AppendLine("<|im_end|>");
            sb.AppendLine("<|im_start|>user");
            sb.AppendLine(text);
            sb.AppendLine("<|im_end|>");
            sb.AppendLine("<|im_start|>assistant");
            sb.AppendLine("The comma delimited list of commands found is ");

            Debug.WriteLine(sb.ToString());

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var response = await GetCompletionsAsync(model, sb.ToString(), 0.0f, _openAIClient, maxTokens: 1500, cancellationToken: cancellationToken);
            sw.Stop();

            Debug.WriteLine($"===== RECOGNIZE RESPONSE {sw.Elapsed}");
            Debug.WriteLine(response);

            var functions = new List<Function>();
            if (response != null)
            {
                //var yaml = GetResultBlock(response);
                //commands = YamlConvert.DeserializeObject<List<Command>>(yaml);
                functions = ParseFunctions(response.Trim());
            }

            var result = new RecognizerResult() { Text = text };
            if (functions.Any())
            {
                result.Intents.Add(FUNCTIONS_INTENT, new IntentScore() { Score = 1.0 });
                result.Entities.Add(FUNCTIONS_INTENT, JArray.FromObject(functions));
            }
            else
            {
                result.Intents.Add(NONE_INTENT, new IntentScore() { Score = 1.0 });
            }

            return result;
        }


        private async Task<string> GetCompletionsAsync(string model, string prompt, float temp, OpenAIClient openAIClient, string[] stopWords = null, int maxTokens = 1500, float topP = 0.0f, float presencePenalty = 0.0f, float frequencyPenalty = 0.0f, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < 3; i++)
            {

                try
                {

                    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(timeout.HasValue ? timeout.Value.Milliseconds : 5 * 1000);

                    if (model.Contains("chat"))
                    {
                        var options = new ChatCompletionsOptions()
                        {
                            DeploymentName = "Production",
                            Temperature = temp,
                            MaxTokens = maxTokens,
                            NucleusSamplingFactor = (float)0.0,
                            FrequencyPenalty = frequencyPenalty,
                            PresencePenalty = presencePenalty,
                            User = Guid.NewGuid().ToString("n")
                        };
                        if (stopWords != null)
                        {
                            foreach (var stopWord in stopWords)
                            {
                                options.StopSequences.Add(stopWord);
                            }
                        }
                        else
                        {
                            options.StopSequences.Add(STOP);
                        }
                        Debug.WriteLine($"---> STOPWORDS: [{string.Join(",", options.StopSequences)}]");

                        ChatRole chatRole = ChatRole.User;
                        var chatTexts = prompt.Split(IM_START, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (var chatText in chatTexts)
                        {
                            ChatRequestMessage message = null;
                            if (chatText.StartsWith("user"))
                            {
                                int iEnd = chatText.IndexOf(IM_END);
                                message = new ChatRequestUserMessage(chatText.Substring("user".Length, iEnd));
                            }
                            else if (chatText.StartsWith("assistant"))
                            {
                                int iEnd = chatText.IndexOf(IM_END);
                                if (iEnd > 0)
                                    message = new ChatRequestAssistantMessage(chatText.Substring("assistant".Length, iEnd));
                                else
                                    message = new ChatRequestAssistantMessage(chatText.Substring("assistant".Length));
                            }
                            else if (chatText.StartsWith("system"))
                            {
                                int iEnd = chatText.IndexOf(IM_END);
                                message = new ChatRequestSystemMessage(chatText.Substring("system".Length, iEnd));
                            }

                            options.Messages.Add(message);
                        }

                        var response = await openAIClient.GetChatCompletionsAsync(/*model, */options, cts.Token);
                        return response.Value.Choices.FirstOrDefault()?.Message.Content;
                    }
                    else
                    {
                        var options = new CompletionsOptions()
                        {
                            DeploymentName = "Production",
                            Temperature = temp,
                            MaxTokens = maxTokens,
                            FrequencyPenalty = frequencyPenalty,
                            PresencePenalty = presencePenalty,
                            User = Guid.NewGuid().ToString("n")
                        };
                        if (stopWords != null)
                        {
                            foreach (var stopWord in stopWords)
                            {
                                options.StopSequences.Add(stopWord);
                            }
                        }
                        else
                        {
                            options.StopSequences.Add(STOP);
                        }
                        Debug.WriteLine($"---> STOPWORDS: [{string.Join(",", options.StopSequences)}]");
                        options.Prompts.Add(prompt);
                        var responseWithoutStream = await openAIClient.GetCompletionsAsync(options, cts.Token);
                        return responseWithoutStream.Value.Choices.FirstOrDefault()?.Text;
                    }
                }
                catch (Exception err)
                {

                }
            }
            return "Failed";
        }

        // write method to properly tokenize and parse comma delimited list of functions like:
        // F("arg1", "Arg,2"), F(arg1), ... into a list of Command objects        
        // F('arg1', ['arg2','arg3']), F(arg1), ... into a list of Command objects

        public static List<Function> ParseFunctions(string text)
        {
            if (text.StartsWith("FUNCTIONLIST:"))
                text = text.Substring("FUNCTIONLIST:".Length);
            text = text.Trim().TrimStart('[').TrimEnd(']');

            var functions = new List<Function>();
            var function = new Function();
            var arg = "";
            var inArg = false;
            char quoteChar = ' ';
            var inFunctionName = false;
            bool inArray = false;
            List<string> arrayArgs = new List<string>();

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inArg)
                {
                    if (quoteChar == ' ' && c == '"')
                    {
                        inArg = true;
                        quoteChar = c;
                        continue;
                    }
                    else if (quoteChar != ' ')
                    {
                        if (c == quoteChar)
                        {
                            quoteChar = ' ';
                            arg = arg.Trim().Trim('\'', '\"');
                            if (!String.IsNullOrWhiteSpace(arg))
                            {
                                if (inArray)
                                    arrayArgs.Add(arg.Trim());
                                else
                                    function.Args.Add(arg.Trim());
                            }
                            arg = "";
                            continue;
                        }
                        else
                        {
                            arg += c;
                            continue;
                        }
                    }
                    else if (c == ')')
                    {
                        // Completed string
                        inArg = false;
                        inFunctionName = false;
                        arg = arg.Trim().Trim('\'', '\"');
                        if (!String.IsNullOrWhiteSpace(arg))
                        {
                            if (inArray)
                                arrayArgs.Add(arg.Trim());
                            else
                                function.Args.Add(arg.Trim());
                        }
                        functions.Add(function);
                        function = null;
                        arg = "";
                        continue;
                    }
                    else if (c == ',')
                    {
                        arg = arg.Trim().Trim('\'', '\"');
                        if (!String.IsNullOrWhiteSpace(arg))
                        {
                            if (inArray)
                                arrayArgs.Add(arg.Trim());
                            else
                                function.Args.Add(arg.Trim());
                        }
                        arg = "";
                    }
                    else if (!inArray && c == '[')
                    {
                        inArray = true;
                        continue;
                    }
                    else if (inArray && c == ']')
                    {
                        arg = arg.Trim().Trim('\'', '\"');
                        if (!String.IsNullOrWhiteSpace(arg))
                        {
                            arrayArgs.Add(arg.Trim());
                        }
                        function.Args.Add(arrayArgs);
                        inArray = false;
                        arg = "";
                        continue;
                    }
                    else
                    {
                        arg += c;
                        continue;
                    }
                }
                else if (!inFunctionName)
                {
                    if (Char.IsLetter(c))
                    {
                        inFunctionName = true;
                        arg += c;
                    }
                    // else skip char;
                    continue;
                }
                else if (inFunctionName)
                {
                    if (c == '(')
                    {
                        inFunctionName = false;
                        inArg = true;
                        function = new Function();
                        function.Name = arg.Trim();
                        arg = "";
                    }
                    else
                    {
                        arg += c;
                        continue;
                    }
                }
            }
            return functions;
        }


    }
}

