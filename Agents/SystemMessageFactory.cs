using DataClasses;

namespace Agents
{
    internal static class SystemMessageFactory
    {
        // TODO - parametrisable system messages!
        internal static String SystemMessageFor(AgentTask task, LanguageCode language)
        {
            switch (language)
            {
                case LanguageCode.eng:
                    return EnglishSystemMessages.GetValueOrDefault(task) ?? throw new NotImplementedException();
                case LanguageCode.fra:
                    return FrenchSystemMessages.GetValueOrDefault(task)  ?? throw new NotImplementedException();
                case LanguageCode.zho:
                    return ChineseSystemMessages.GetValueOrDefault(task) ?? throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private static Dictionary<AgentTask, String> EnglishSystemMessages = new Dictionary<AgentTask, String>
        {
            {AgentTask.ContextChangeIdentification, "Given the following numbered statements and context, identify when the context changes, if it does. Return each context change on a new line, with a brief description of why the context is changing. Do not include any preamble besides the inline description. return format (x,y, etc line numbers): x: reason\ny: reason\n.... The line number MUST correspond to the line number where the context changes."},

            {AgentTask.ContextUpdating, "Consider the following statements, and context, please update the context if it has changed. Such as a change of location, time or focus. If you do think the context has changed, preserve context elements that are still applicable. Remove lines that no longer apply. Respond with just the new context, with each item on a new line as before. DO NOT SUMMARISE - provide concise contextual information only. ONLY RESPOND WITH THE NEW CONTEXT, no other information"},

            {AgentTask.MultiDefinitionResolution, "Given the following word, context and definition options, determine the most appropriate definition, respond with only a single integer value and nothing else using the indexing of the definitions given, if no definitions match, respond -1" },

            {AgentTask.SingleDefinitionResolution, "Given the following word, context and definitions, do you think the definition is correct, ignoring inflection, reply either yes or no, and nothing else" },

            {AgentTask.DecompositionToStatements, "break the following text into the statements composing it, usually sentences for conventional text, they should be long enough that with context provided a user can understand the meaning of the statement; the statements must be verbatim from the text DO NOT SUMMARISE, DO NOT NUMBER THE STATEMENTS, the statements should map injectively into the text. Return each statement on a new line" },

            {AgentTask.DecompositionToUnits, "Decompose the following text into units of meaning, normally single words. Do not split compound words such as \nhot dog\n or \nfind out\n. Do not split names such as \nJoe Bloggs\n or \nUnited Kingdom\n. Only respond using direct contents of the text, with each unit of meaning on a newline, ignore punctuation and brackets that don't affect words' meaning, similarly don't add punctuation to the units unless essential, the result must map injectively into the original text, add no whitespace unless it already existed and conveyed meaning. Do not complete truncated words, all units should map into the original verbatim, DO NOT skip words, you can skip punctuation. format \nunit1\nunit2 etc" },

            {AgentTask.UnitRooting, "process the following lines to remove inflection, converting verbs to the infinitive and nouns to the singular for example. Use standard English capitalisation, proper nouns, names, surnames, locations, place names and acronyms MUST obey capitalisation rules, with the initial letter capitalised, otherwise favour lowercase. Remove unnecessary punctuation and possessive apostrophes. You MUST NOT omit lines. DO NOT combine lines. DO NOT split lines. The number of output and input lines MUST be the same. Format \nword1\nword2 etc." },

            {AgentTask.DefinitionParsing, "produce a short educational translation of this definition for an adult English speaker, your response should be in English and take into account their proficiency level in the definition's language. Keep your definition succinct. Include a translation of the word into English. Strictly required response format: English translation of word \n short translation of provided definition" },

            {AgentTask.DefinitionIPAPronouncing, "given the following word (definition also provided) provide the word's pronunciation using the International Phonetic Alphabet IPA in British English, only reply with the pronunciation, nothing else" },

            {AgentTask.DefinitionRomanisedPronouncing, "given the following word (definition also provided) provide a simple pronunciation, using only alphabet letters, no IPA or diacritics, only reply with the word's pronunciation, nothing else" },

            {AgentTask.DefinitionRewriting, "You are an expert lexicographer, given the following word and definition, please write a new definition, the definition should avoid politicalisation and dated language, if the definition provided points to another, do not do the same, write one for this word. Reply with only the new definition" },

            {AgentTask.DetailedDefinitionWriting, "Given this foreign word and definition, please write a detailed definition, in English, of the word, specific to the word sense described in the existing definition. Only describe the meaning of the word, NOT synonyms, NOT register, NOR giving examples. Reply only with the definition, DO NOT write \"definition:\" at the start." },

            {AgentTask.DefinitionSynonyms, "Given this foreign word and definition, please provide a list of synonymns, in English, of the word. Seperate them with commas, and only reply with the list, nothing else. DO NOT start your response with \"synonyms: \"" },

            {AgentTask.WordSenseDistinguishingExplanation, "Given this foreign word and definition, write short summary of how this sense of the word differs from other senses. If you can't think of any other sense then say so. Be precise and lexicographical with your response. DO NOT format your response with headings or titles, just provide a few sentences explaining how this word sense is different from other possibilities." },

            {AgentTask.RegisterAnalysis, "Given this foreign word and definition, tell me about the register of the word, types of text that may include it used in this sense, and topics/fields where I might find it. DO NOT add titles or headings to your response, just provide a few sentences helping me understand this word more." },

            {AgentTask.SimpleExampleOfWord, "Given this word and definition, provide a simple example sentence using the word in the sense the definition describes, reply only with the example sentence and nothing else." },

            {AgentTask.MediumExampleOfWord, "Given this word and definition, provide a moderately complex example sentence using the word in the sense the definition describes, reply only with the example sentence and nothing else." },

            {AgentTask.HardExampleOfWord, "Given this word and definition, provide a very complex example sentence using the word in the sense the definition describes, reply only with the example sentence and nothing else." },

            {AgentTask.GeneralPurpose, "you are a helpful assistant for language learners" },
        };
        private static Dictionary<AgentTask, String> FrenchSystemMessages = new Dictionary<AgentTask, String>
        {
            { AgentTask.ContextChangeIdentification, "Compte tenu des déclarations numérotées et du contexte suivants, identifiez quand le contexte change, si c'est le cas. Renvoie chaque changement de contexte sur une nouvelle ligne, avec une brève description de la raison pour laquelle le contexte change. N'incluez aucun préambule autre que la description en ligne. format de retour (numéros de ligne x, y, etc) : x : raison\ny : raison\n.... Le numéro de ligne DOIT correspondre au numéro de ligne où le contexte change."},

            {AgentTask.ContextUpdating, "Tenez compte des déclarations suivantes et du contexte, veuillez mettre à jour le contexte pour refléter les changements qui se produisent entre les lignes demandées, conservez les éléments de contexte qui sont toujours applicables, mais supprimez et ajoutez des lignes de contexte afin que les déclarations suivantes soient compréhensibles en utilisant ce contexte. Répondez avec uniquement le nouveau contexte, avec chaque élément sur une nouvelle ligne comme auparavant. NE PAS RÉSUMER – fournissez uniquement des informations contextuelles concises. RÉPONDEZ UNIQUEMENT AVEC LE NOUVEAU CONTEXTE, aucune autre information"},

            {AgentTask.MultiDefinitionResolution, "Étant donné les options de mot, de contexte et de définition suivantes, déterminez la définition la plus appropriée, répondez avec une seule valeur entière et rien d'autre en utilisant l'indexation des définitions données, si aucune définition ne correspond, répondez -1" },

            {AgentTask.SingleDefinitionResolution, "Étant donné le mot suivant, le contexte et les définitions, pensez-vous que la définition est correcte, en ignorant l'inflexion, répondez oui ou non, et rien d'autre" },

            {AgentTask.DecompositionToStatements, "diviser le texte suivant en déclarations qui le composent, généralement des phrases pour un texte conventionnel, elles doivent être suffisamment longues pour qu'avec le contexte fourni, un utilisateur puisse comprendre le sens de la déclaration ; les déclarations doivent être textuellement tirées du texte. NE PAS RÉSUMER, NE NUMÉROTER PAS LES DÉCLARATIONS, les déclarations doivent être mappées de manière injective dans le texte. Renvoie chaque instruction sur une nouvelle ligne" },

            {AgentTask.DecompositionToUnits, "Décomposez le texte suivant en unités de sens, normalement des mots simples. Ne divisez pas les mots composés tels que \npomme de terre\n ou \nposte de travail\n. Ne divisez pas les noms tels que \nean Dupont\n ou \nNouvelle-Zélande\n. Répondez uniquement en utilisant le contenu direct du texte, avec chaque unité de sens sur une nouvelle ligne, ignorez la ponctuation et les crochets qui n'affectent pas le sens des mots, de même, n'ajoutez pas de ponctuation aux unités sauf si cela est essentiel, le résultat doit être mappé de manière injective dans le texte original, n’ajoutez aucun espace à moins qu’il n’existe déjà et ne transmette un sens. Ne complétez pas les mots tronqués, toutes les unités doivent correspondre au texte original, NE sautez PAS de mots, vous pouvez sauter la ponctuation. formater \nunité1\nunité2 etc." },

            {AgentTask.UnitRooting, "Traitez les lignes suivantes pour supprimer l'inflexion, en convertissant les verbes à l'infinitif et les noms au singulier par exemple. Utilisez des majuscules anglaises standard : les noms propres, les prénoms, les lieux, les noms de lieux et les acronymes DOIVENT obéir aux règles de majuscule, avec la lettre initiale en majuscule, sinon privilégiez les minuscules. Supprimez la ponctuation inutile et les apostrophes possessives. Vous NE DEVEZ PAS omettre de lignes. NE combinez PAS les lignes. NE divisez PAS les lignes. Le nombre de lignes de sortie et d'entrée DOIT être le même. Formater \nmot1\nmot2 etc." },

            {AgentTask.DefinitionParsing, "analysez cette définition pour un adulte francophone, votre réponse doit être en français et prendre en compte son niveau de compétence dans la langue de la définition. Gardez votre définition succincte." },

            {AgentTask.DefinitionIPAPronouncing, "étant donné le mot suivant (définition également fournie), fournissez la prononciation du mot en utilisant l'alphabet phonétique international (API), répondez uniquement avec la prononciation du mot, rien d'autre" },

            {AgentTask.DefinitionRomanisedPronouncing, "étant donné le mot suivant (définition également fournie), fournissez une prononciation simple du mot, n'utilisez pas l'API, répondez uniquement avec la prononciation simple du mot, rien d'autre" },

            {AgentTask.DefinitionRewriting, "Vous êtes un lexicographe expert. Étant donné le mot et sa définition, veuillez rédiger une nouvelle définition. Cette définition doit éviter toute politisation et tout langage désuet. Si la définition fournie renvoie à une autre définition, n'en faites pas autant. Répondez en utilisant uniquement la nouvelle définition." },

            {AgentTask.DetailedDefinitionWriting, "Étant donné ce mot étranger et sa définition, veuillez rédiger une définition détaillée, en français, du mot, spécifique au sens décrit dans la définition existante. Décrivez uniquement le sens du mot, SANS synonymes, SANS registre, NI exemples. Répondez uniquement avec la définition, N'écrivez PAS « définition : » au début." },

            {AgentTask.DefinitionSynonyms, "Étant donné ce mot étranger et sa définition, veuillez fournir une liste de synonymes, en français, du mot. Séparez-les par des virgules et répondez uniquement avec la liste, sans plus. NE COMMENCEZ PAS votre réponse par « synonymes : »." },

            {AgentTask.WordSenseDistinguishingExplanation, "Étant donné ce mot étranger et sa définition, rédigez un bref résumé expliquant en quoi ce sens diffère des autres. Si vous ne trouvez aucun autre sens, indiquez-le. Soyez précis et lexicographique dans votre réponse. N'utilisez pas de titres ou d'en-têtes dans votre réponse ; fournissez simplement quelques phrases expliquant en quoi ce sens diffère des autres possibilités." },

            {AgentTask.RegisterAnalysis, "Étant donné ce mot étranger et sa définition, veuillez m'indiquer son registre, les types de textes qui pourraient l'inclure dans ce sens, ainsi que les sujets/domaines où je pourrais le trouver. N'ajoutez pas de titre ni d'intertitre à votre réponse ; fournissez simplement quelques phrases m'aidant à mieux comprendre ce mot." },

            {AgentTask.SimpleExampleOfWord, "Étant donné ce mot et cette définition, fournissez un exemple de phrase simple en utilisant le mot dans le sens décrit par la définition, répondez uniquement avec l'exemple de phrase et rien d'autre." },

            {AgentTask.MediumExampleOfWord, "Étant donné ce mot et cette définition, fournissez une phrase d'exemple modérément complexe en utilisant le mot dans le sens décrit par la définition, répondez uniquement avec la phrase d'exemple et rien d'autre." },

            {AgentTask.HardExampleOfWord, "Étant donné ce mot et cette définition, fournissez une phrase d'exemple très complexe en utilisant le mot dans le sens décrit par la définition, répondez uniquement avec la phrase d'exemple et rien d'autre." },

            {AgentTask.GeneralPurpose, "vous êtes un assistant utile pour les apprenants de langues" },

        };

        private static Dictionary<AgentTask, String> ChineseSystemMessages = new Dictionary<AgentTask, String>
        {
            { AgentTask.ContextChangeIdentification, "给定以下编号的语句和上下文，确定上下文何时发生变化（如果发生变化）。在新行上返回每个上下文变化，并简要说明上下文变化的原因。除了内联描述外，不要包含任何前言。返回格式（x、y 等行号）：x：原因 \ny：原因 \n.... 行号必须与上下文发生变化的行号相对应。"},

            {AgentTask.ContextUpdating, "考虑以下陈述和上下文，请更新上下文以反映所要求行之间发生的变化，保留仍然适用的上下文元素，但删除和添加上下文行，以便后续陈述可以使用该上下文来理解。仅使用新的上下文进行回复，每一项都像以前一样在新行上。不要总结 - 仅提供简明的上下文信息。仅使用新的上下文进行回复，不要提供其他信息"},

            {AgentTask.MultiDefinitionResolution, "给定以下单词、上下文和定义选项，确定最合适的定义，使用给定定义的索引仅用一个整数值来响应，而不使用其他任何值，如果没有匹配的定义，则响应 -1" },

            {AgentTask.SingleDefinitionResolution, "给出以下单词、上下文和定义，你认为定义是否正确，忽略词形变化，回答“是”或“否”，仅此而已" },

            {AgentTask.DecompositionToStatements, "将以下文本分解为组成它的语句，通常是句子，对于常规文本，它们应该足够长，以便用户在提供上下文的情况下能够理解语句的含义；语句必须逐字逐句地来自文本不要总结，不要对语句进行编号，语句应该注入到文本中。在新行上返回每个语句" },

            {AgentTask.DecompositionToUnits, "将以下文本分解为意义单元，通常是单个单词。不要拆分复合词，例如 \n日光灯\n 或 \n洗衣机\n。不要拆分名称，例如 \n张伟\n 或 \n中华人民共和国\n。仅使用文本的直接内容进行响应，每个意义单元位于换行符上，忽略不影响单词含义的标点符号和括号，同样，除非必要，否则不要向单元添加标点符号，结果必须直接映射到原始文本中，除非空格已经存在并传达含义，否则不要添加空格。不要完成截断的单词，所有单元都应逐字映射到原始文本中，不要跳过单词，你可以跳过标点符号。格式 \n单元1\n单元2 等" },

            {AgentTask.UnitRooting, "处理以下行以删除词形变化，例如将动词转换为不定式，将名词转换为单数。删除不必要的标点符号。您不得省略行。不要合并行。不要拆分行。输出和输入行的数量必须相同。你必须用中文回答，不要翻译任何东西。格式 \n单词1\n单词2 等。" },

            {AgentTask.DefinitionParsing, "为成年中文使用者解析这个定义，您的回答应该是中文，并考虑他们对定义语言的熟练程度。保持你的定义简洁。" },

            {AgentTask.DefinitionIPAPronouncing, "给出以下单词（也为您提供了一个定义），使用国际音标 (IPA) 提供该单词的发音。仅回复发音，无需其他" },

            {AgentTask.DefinitionRomanisedPronouncing, "给出以下单词（也为你提供了一个定义）提供该单词的拼音，只用该单词的拼音回复，不用其他内容" },

            {AgentTask.DefinitionRewriting, "您是一位词典编纂专家，请根据以下单词及其定义，写出一个新的定义。定义应避免政治化和过时的语言。如果现有定义指向其他定义，请勿照搬，请为该单词另写一个定义。请仅使用新的定义进行回复。用标准拼音写在定义的开头，单词之间不加点。" },

            {AgentTask.DetailedDefinitionWriting, "给出该外来词及其定义，请用中文详细解释该词，具体到现有定义中所描述的词义。仅描述该词的含义，不要描述同义词、语域或举例。回复时仅提供定义，请勿在开头写“定义：”。" },

            {AgentTask.DefinitionSynonyms, "鉴于此外来词及其定义，请提供该词的中文同义词列表。请用逗号分隔，并仅回复列表，请勿添加其他内容。请勿以“同义词”开头。" },

            {AgentTask.WordSenseDistinguishingExplanation, "给出这个外来词及其定义，简要概括该词义与其他词义的区别。如果想不出其他词义，请直接写出。回答应准确，并遵循词典顺序。请勿使用标题或标题，只需用几句话解释该词义与其他词义的区别即可。" },

            {AgentTask.RegisterAnalysis, "给出这个外来词及其定义，请告诉我这个词的语域、可能包含该词的文本类型，以及我可能在哪些主题/领域找到它。请勿在您的回复中添加标题或小标题，只需提供几句话帮助我更好地理解这个词即可。" },

            {AgentTask.SimpleExampleOfWord, "给出这个词和定义，用定义所描述的含义提供一个简单的例句，只用例句回复，不用其他任何东西。" },

            {AgentTask.MediumExampleOfWord, "给出这个词和定义，用定义所描述的含义使用该词提供一个中等复杂的例句，只用例句回复，不用其他任何东西。" },

            {AgentTask.HardExampleOfWord, "给出这个词和定义，用定义所描述的含义使用该词提供一个非常复杂的例句，只用例句回复，不用其他任何东西。" },

            {AgentTask.GeneralPurpose, "你是语言学习者的得力助手" },
        };
    }

}
