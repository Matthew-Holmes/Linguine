using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMedia
{
    public class InternalTextualMedia
    {
        String _text;
        String _description;
        LanguageCode _languageCode;

        public InternalTextualMedia(string text, LanguageCode lc, string description = "")
        {
            _text = text;
            _description = description;
            _languageCode = lc;
        }

        public String Text
        {
            get => _text;
        }

        public String Description
        {
            get => _description;
        }

        public LanguageCode LanguageCode
        {
            get => _languageCode;
        }
    }
}
