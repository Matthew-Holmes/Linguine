using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public record ForBestTranslation(String inThisStatementIThink, String isBestTranslatedAs);
    public record ForDefinitionResolution(String word, String surroundingContext, String definitionOptions);

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

        public static ForDefinitionResolution DefinitionResolutionString(LanguageCode target)
        {
            switch (target)
            {
                case LanguageCode.eng:
                    return new ForDefinitionResolution("Word: ", "Surrounding Context: ", "Definitions: ");
                case LanguageCode.fra:
                    return new ForDefinitionResolution("Mot : ", "Contexte : ", "Définitions : ");
                case LanguageCode.zho:
                    return new ForDefinitionResolution("词语：", "上下文：", "释义：");
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
