using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class TextUnit
    {
        public String Text { get; private set; }

        public TextUnit(String text)
        {
            Text = text;
        }
    }
}
