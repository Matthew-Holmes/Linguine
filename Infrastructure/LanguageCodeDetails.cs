using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class LanguageCodeDetails
    {
        public static String EnglishName(LanguageCode lc)
        {
            switch (lc)
            {
                case LanguageCode.eng:
                    return "English";
                case LanguageCode.fra:
                    return "French";
                case LanguageCode.zho:
                    return "Chinese";
                default:
                    return "";
            }
        }
    }
}
