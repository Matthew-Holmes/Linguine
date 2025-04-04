using Google.Cloud.TextToSpeech.V1;
using DataClasses;

namespace Sound
{
    public static class Talker
    {
        // fix the speed at 1.0 since the google API hasn't got variable speeds for the 
        // voices I'm interested in yet

        public static byte[] TextToSpeech(String text, DataClasses.Voice voice, LanguageCode lc)
        {
            return _TextToSpeech(text, voice, lc, 1.0m);
        }
        private static byte[] _TextToSpeech(String text, DataClasses.Voice voice, LanguageCode lc, decimal speed)
        {
            TextToSpeechClient client = TextToSpeechClient.Create();

            SynthesisInput input = new SynthesisInput
            {
                Text = text
            };

            VoiceSelectionParams voiceSelection = VoiceSelectionParamsFactory.FromVoice(voice, lc);

            AudioConfig audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Linear16, /* highest quality */
                SpeakingRate  = (double)speed,
            };

            SynthesizeSpeechResponse response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            Google.Protobuf.ByteString audioContent = response.AudioContent;

            return audioContent.ToByteArray();
        }
    }
}
