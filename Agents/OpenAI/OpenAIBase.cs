﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Agents.OpenAI
{
    public class OpenAIBase : AgentBase
    {
        private readonly String _apiKey;
        private readonly HttpClient _httpClient;
        private readonly String _preamble;
        private readonly int _promptDepth;
        private readonly int _maxTokens;

        public OpenAIBase(String apiKey, String preamble, int promptDepth, int maxTokens)
        {
            _apiKey = apiKey;
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            this._preamble = preamble;
            this._promptDepth = promptDepth;
            this._maxTokens = maxTokens;
        }

        protected override string GetResponseCore(string prompt)
        {
            return Task.Run(async () => await SendMessage(prompt)).Result;
        }

        private async Task<string> SendMessage(string message)
        {
            var messages = new List<object>();

            // Assuming _preamble is a system message about the assistant's role
            messages.Add(new { role = "system", content = _preamble });

            // Add chat history to messages
            int start = Math.Max(0, PromptResponseHistory.Count - _promptDepth);
            for (int i = start; i < PromptResponseHistory.Count; i++)
            {
                var entry = PromptResponseHistory[i];
                // Assuming entry.Item1 is the user message and entry.Item2 is the AI response
                if (!string.IsNullOrEmpty(entry.Item1))
                {
                    messages.Add(new { role = "user", content = entry.Item1 });
                }
                if (!string.IsNullOrEmpty(entry.Item2))
                {
                    messages.Add(new { role = "assistant", content = entry.Item2 });
                }
            }

            // Add the current user message
            messages.Add(new { role = "user", content = message });

            var data = new
            {
                model = "gpt-3.5-turbo",
                messages = messages.ToArray(),
                temperature = 0.5,
                max_tokens = _maxTokens,
            };

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                string modelResponse = jsonResponse.choices[0].message.content.ToString().Trim();

                return modelResponse;
            }
            else
            {
                throw new Exception($"API Error: {responseString}");
            }
        }

    }
}