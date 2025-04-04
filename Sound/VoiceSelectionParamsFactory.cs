using DataClasses;
using Google.Cloud.TextToSpeech.V1;

namespace Sound
{
    public static class VoiceSelectionParamsFactory
    {
        public static VoiceSelectionParams FromVoice(DataClasses.Voice voice, LanguageCode lc)
        {
            String voiceName          = VoiceDetails.VoiceName(voice, lc);
            String googleLanguageCode = LanguageCodeDetails.GoogleName(lc);

            VoiceSelectionParams ret = new VoiceSelectionParams
            {
                LanguageCode = googleLanguageCode,
                Name         = voiceName,
            };

            return ret;
        }
    }
}
