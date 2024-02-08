using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public  class TextDecompositionAndRootingAgent : OpenAIBase
    {

        private static String preamble = "Decompose the following text into units of meaning and their uninflected roots, either single words or phrases, do not split full names, nor split words when their composite is very common or cannot be naively inferred from their components, only respond using direct contents of the text, ignore punctuation unless it is essential to the meaning of the word, similarly don't add punctuation to the units unless essential, add no whitespace unless it already existed and conveyed meaning. Each OriginalUnit MUST directly and injectively map into the original text. Then append the root form of the unit, removing inflection i.e using infinitives, singular form etc, and using canonical lower vs uppercase. Return format OriginalUnit1\troot1\nOriginalUnit2\troot2 etc The set of OriginalUnit values must map injectively into the text.";

        
        protected override async Task<String> GetResponseCore(string prompt)
        {
            return "There\tthere\nnow\tnow\nis\tbe\nyour\tyour\ninsular\tinsular\ncity\tcity\nof\tof\nthe\tthe\nManhattoes\tManhattoes\n,\t,\nbelted\tbelt\nround\tround\nby\tby\nwharves\twharf\nas\tas\nIndian\tIndian\nisles\tisle\nby\tby\ncoral\tcoral\nreefs\treef\n—\t—\ncommerce\tcommerce\nsurrounds\tsurround\nit\tit\nwith\twith\nher\ther\nsurf\tsurf\n.\t.\nRight\tRight\nand\tand\nleft\tleft\n,\t,\nthe\tthe\nstreets\tstreet\ntake\ttake\nyou\tyou\nwaterward\twaterward\n.\t.\nIts\tIts\nextreme\textreme\ndowntown\tdowntown\nis\tbe\nthe\tthe\nbattery\tbattery\n,\t,\nwhere\twhere\nthat\tthat\nnoble\tnoble\nmole\tmole\nis\tbe\nwashed\twash\nby\tby\nwaves\twave\n,\t,\nand\tand\ncooled\tcool\nby\tby\nbreezes\tbreeze\n,\t,\nwhich\twhich\na\ta\nfew\tfew\nhours\thour\nprevious\tprevious\nwere\tbe\nout\tout\nof\tof\nsight\tsight\nof\tof\nland\tland\n.\t.\nLook\tLook\nat\tat\nthe\tthe\ncrowds\tcrowd\nof\tof\nwater-gazers\twater-gazer\nthere\tthere\n.\t.";
        }
        

        public TextDecompositionAndRootingAgent(string apiKey) : base(apiKey, preamble, 0, 4096, "gpt-4-0125-preview") { }

    }
}
