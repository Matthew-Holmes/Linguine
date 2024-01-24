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

        public InternalTextualMedia(string text, string description = "")
        {
            _text = text;
            _description = description;
        }

        public String Text
        {
            get => _text;
        }

        public String Description
        {
            get => _description;
        }
    }
}
