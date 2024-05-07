using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using Newtonsoft.Json;

namespace Agents.OpenAI
{
    public class OpenAIBase : ParametrisedAgentBase
    {
        private readonly String _apiKey;
        private readonly HttpClient _httpClient;

        public OpenAIBase(String apiKey)
        {
            _apiKey             = apiKey;
            this._httpClient    = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            DiscreteParameters.Add(new Parameter<int>("PromptDepth", 10, int.MaxValue, 0, 100,   0));
            DiscreteParameters.Add(new Parameter<int>("MaxTokens", 1000, int.MaxValue, 0, 16000, 0));

            ContinousParameters.Add(new Parameter<double>("Temperature",      1.0, 2.0, 0.0));
            ContinousParameters.Add(new Parameter<double>("TopP",             1.0, 1.0, 0.0));
            ContinousParameters.Add(new Parameter<double>("FrequencyPenalty", 0.0, 1.0, 0.0));
            ContinousParameters.Add(new Parameter<double>("PresencePenalty",  0.0, 1.0, 0.0));

            StringParameters.Add("system", "You are a helpful assistant");
            StringParameters.Add("model", "gpt-3.5-turbo-0125");
        }

        protected override async Task<String> GetResponseCore(string prompt)
        {
            var messages = new List<object>();

            // assuming _preamble is a system message about the assistant's role
            messages.Add(new { role = "system", content = StringParameters["system"]});

            // add chat history to messages
            int start = Math.Max(0, PromptResponseHistory.Count - DiscreteParameter("PromptDepth").Value);
            for (int i = start; i < PromptResponseHistory.Count; i++)
            {
                var entry = PromptResponseHistory[i];

                if (!string.IsNullOrEmpty(entry.Item1))
                {
                    messages.Add(new { role = "user", content = entry.Item1 });
                }
                if (!string.IsNullOrEmpty(entry.Item2))
                {
                    messages.Add(new { role = "assistant", content = entry.Item2 });
                }
            }

            // add the current user prompt
            messages.Add(new { role = "user", content = prompt });

            var data = new
            {
                model = StringParameters["model"],
                messages = messages.ToArray(),
                temperature = ContinousParameter("Temperature").Value,
                max_tokens = DiscreteParameter("MaxTokens").Value,
                top_p = ContinousParameter("TopP").Value,
                frequency_penalty = ContinousParameter("FrequencyPenalty").Value,
                presence_penalty = ContinousParameter("PresencePenalty").Value
            };

            var content         = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var response        = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString  = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                string modelResponse = jsonResponse.choices[0].message.content.ToString().Trim();

                return modelResponse;
            }
            else
            {
                throw new ApiException($"API Error: {responseString}", response.StatusCode);
            }
        }
    }

}
