using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public record ForBestTranslation(String inThisStatementIThink, String isBestTranslatedAs);
    public record ForDefinitionResolution(String word, String surroundingContext, String definitionOptions, String singleDefinition);

    // TODO - reconcile this with prompt factory in Agents

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
                    return new ForDefinitionResolution("Word: ", "Surrounding Context: ", "Definitions: ", "Definition: ");
                case LanguageCode.fra:
                    return new ForDefinitionResolution("Mot : ", "Contexte : ", "Définitions : ", "Définition : ");
                case LanguageCode.zho:
                    return new ForDefinitionResolution("词语：", "上下文：", "释义：", "释义：");
                default:
                    throw new NotImplementedException();
            }
        }

        public static List<string> Affirmatives(LanguageCode lang)
        {
            switch (lang)
            {
                case LanguageCode.eng:
                    return new List<string>
            {
                "Yes", "yes", "y", "Y",
                "Yep", "yep", "Ye", "ye", "Ya", "ya",
                "Yah", "yah", "Sure", "sure",
                "Absolutely", "absolutely", "Of course", "of course",
                "Indeed", "indeed", "Affirmative", "affirmative",
                "Right", "right", "Correct", "correct",
                "Definitely", "definitely", "Certainly", "certainly",
                "Sure thing", "sure thing", "Totally", "totally",
                "For sure", "for sure", "You bet", "you bet",
                "Aye", "aye"
            };

                case LanguageCode.fra:
                    return new List<string>
            {
                "Oui", "oui", "Ouais", "ouais", "Ouaip", "ouaip",
                "Si", "si", // used to contradict a negative in French
                "Bien sûr", "bien sûr", "Absolument", "absolument",
                "Tout à fait", "tout à fait", "Certainement", "certainement",
                "Exactement", "exactement", "C'est ça", "c'est ça",
                "D'accord", "d'accord", "Carrément", "carrément",
                "Effectivement", "effectivement", "Parfait", "parfait",
                "Juste", "juste"
            };

                case LanguageCode.zho:
                    return new List<string>
            {
                "是", "是的", "对", "对的",
                "好的", "好", "嗯", "嗯嗯", "当然", "可以",
                "没错", "确实", "行", "行啊",
                "好啊", "对啊", "对呀", "对喔",
                "可以的", "没问题", "成", "好吧",
                "必须的", "完全正确", "正是如此", "说得对"
            };

                default:
                    throw new NotImplementedException();
            }
        }


        public static List<string> Negatives(LanguageCode lang)
        {
            switch (lang)
            {
                case LanguageCode.eng:
                    return new List<string>
            {
                "No", "no", "Nope", "nope", "N", "n",
                "Nah", "nah", "Na", "na",
                "Negative", "negative", "Not at all", "not at all",
                "Absolutely not", "absolutely not", "No way", "no way",
                "Never", "never", "Not really", "not really",
                "Certainly not", "certainly not", "I don't think so", "i don't think so",
                "Incorrect", "incorrect", "Wrong", "wrong"
            };

                case LanguageCode.fra:
                    return new List<string>
            {
                "Non", "non", "Nan", "nan", "Nope", "nope",
                "Pas du tout", "pas du tout", "Absolument pas", "absolument pas",
                "Jamais", "jamais", "Certainement pas", "certainement pas",
                "Je ne pense pas", "je ne pense pas", "Faux", "faux",
                "Incorrect", "incorrect", "C’est faux", "c’est faux"
            };

                case LanguageCode.zho:
                    return new List<string>
            {
                "不", "不是", "没有", "没", "不行", "不能", "不可以",
                "绝不", "绝对不是", "从来没有", "从来不", "不对", "错了",
                "不是的", "不可能", "没门", "不行啊",
                "才不是", "一点也不", "并不是", "说错了"
            };

                default:
                    throw new NotImplementedException();
            }
        }

    }
}
