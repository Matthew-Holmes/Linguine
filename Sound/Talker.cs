using Google.Cloud.TextToSpeech.V1;
using DataClasses;

namespace Sound
{
    public static class Talker
    {
        // fix the speed at 1.0 since the google API hasn't got variable speeds for the 
        // voices I'm interested in yet

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);

        public static async Task<byte[]> TextToSpeech(String text, DataClasses.Voice voice, LanguageCode lc)
        {
            return await _TextToSpeech(text, voice, lc, 1.0m);
        }

        private static async Task<byte[]> _TextToSpeech(String text, DataClasses.Voice voice, LanguageCode lc, decimal speed)
        {
            await _semaphore.WaitAsync();

            try
            {
                TextToSpeechClient client = TextToSpeechClient.Create();

                var input = new SynthesisInput { Text = text };

                var voiceSelection = VoiceSelectionParamsFactory.FromVoice(voice, lc);

                var audioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Linear16,
                    SpeakingRate = (double)speed,
                };

                var response = await client.SynthesizeSpeechAsync(input, voiceSelection, audioConfig);

                return response.AudioContent.ToByteArray();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
