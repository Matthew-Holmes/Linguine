using DataClasses;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public record ForBestTranslation(String inThisStatementIThink, String isBestTranslatedAs);

    public static class TextFactory
    {
        public static ForBestTranslation BestTranslatedAsString(LanguageCode native)
        {
            switch (native)
            {
                case LanguageCode.eng:
                    return new ForBestTranslation("in this statement I think", "is best translated as");
                case LanguageCode.fra:
                    return new ForBestTranslation("ans cette phrase, je pense que", "se traduit le mieux par");
                case LanguageCode.zho:
                    return new ForBestTranslation("在这句话中，我认为", "最好的翻译是");
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
