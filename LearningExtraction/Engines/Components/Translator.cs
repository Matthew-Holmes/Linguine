using DataClasses;
using Google.Cloud.Translation.V2;
using Infrastructure.AntiCorruptionLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class Translator
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);

        public static async Task<String> Translate(String text, LanguageCode from , LanguageCode to)
        {
            await _semaphore.WaitAsync();
            try
            {
                var client = TranslationClient.Create();

                String target = LanguageCodeDetails.GoogleName(to);
                String source = LanguageCodeDetails.GoogleName(from);

                var response = await client.TranslateTextAsync(text, target, source);

                return response.TranslatedText;
            } 
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
