using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public static class PromptFactory
    {
        // TODO - a similar method but for when in the user's native language
        public static String PromptForForeignLanguageDefinitionResolution(
            LanguageCode native, String targetLanguageWord, String targetLanguageStatement, String nativeLanguageStatement)
        {
            switch (native)
            {
                case LanguageCode.eng:
                    return $"For the following statement and translation to English: \n{targetLanguageStatement}\n{nativeLanguageStatement}\n how would you best translate {targetLanguageWord}? Only reply with the translation for {targetLanguageWord}, nothing else";
                case LanguageCode.fra:
                    return $"Pour l'énoncé suivant et sa traduction en français : \n{targetLanguageStatement}\n{nativeLanguageStatement}\n comment traduiriez-vous au mieux {targetLanguageWord} ? Répondez uniquement avec la traduction de {targetLanguageWord}, rien d'autre.";
                case LanguageCode.zho:
                    return $"对于以下语句及其中文翻译：\n{targetLanguageStatement}\n{nativeLanguageStatement}\n你认为将 {targetLanguageWord} 最佳翻译成中文是什么？只回复 {targetLanguageWord} 的中文翻译，别回复其他内容。";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
