using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClasses
{
    public static class VoiceDetails
    {
        public static Voice RandomVoice(Random rng)
        {
            var values = Enum.GetValues(typeof(Voice));
            int index = rng.Next(values.Length);
            return (Voice)values.GetValue(index)!;
        }

        public static String VoiceName(Voice voice, LanguageCode lc)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(LanguageCodeDetails.GoogleName(lc));
            sb.Append("-Chirp3-HD-");
            sb.Append(VoiceToString(voice));

            return sb.ToString();
        }

        private static String VoiceToString(Voice voice)
        {
            switch (voice)
            {
                case Voice.Aoede:
                    return "Aoede";
                case Voice.Charon:
                    return "Charon";
                case Voice.Fenrir:
                    return "Fenrir";
                case Voice.Kore:
                    return "Kore";
                case Voice.Leda:
                    return "Leda";
                case Voice.Orus:
                    return "Orus";
                case Voice.Puck:
                    return "Puck";
                case Voice.Zephyr:
                    return "Zephyr";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
