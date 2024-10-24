using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        public static String FrenchName(LanguageCode lc)
        {
            switch (lc)
            {
                case LanguageCode.eng:
                    return "Anglais";
                case LanguageCode.fra:
                    return "Français";
                case LanguageCode.zho:
                    return "Chinois";
                default:
                    return "";
            }
        }

        public static String ChineseName(LanguageCode lc)
        {
            switch (lc)
            {
                case LanguageCode.eng:
                    return "英语"; // Yingyu
                case LanguageCode.fra:
                    return "法语"; // Fǎyǔ
                case LanguageCode.zho:
                    return "汉语"; // Hànyǔ
                default:
                    return "";
            }
        }

        public static string LanguageName(LanguageCode naming, LanguageCode named)
        {
            switch (naming)
            {
                case LanguageCode.eng:
                    return EnglishName(named);
                case LanguageCode.fra:
                    return FrenchName(named);
                case LanguageCode.zho:
                    return ChineseName(named);
                default:
                    return "";
            }
        }

        public static List<String> LanguageNames(LanguageCode nativeLanguage)
        {
            List<String> names = new List<String>();
            foreach (LanguageCode lc in Enum.GetValues(typeof(LanguageCode)))
            {
                names.Add(LanguageName(nativeLanguage, lc));
            }
            return names;
        }

        public static List<LanguageCode> LanguageCodes()
        {
            List<LanguageCode> codes = new List<LanguageCode>();
            foreach (LanguageCode lc in Enum.GetValues(typeof(LanguageCode)))
            {
                codes.Add(lc);
            }
            return codes;
        }

        public static decimal Density(LanguageCode lc)
        {
            switch (lc)
            {
                case LanguageCode.eng:
                    return 1.0m;
                case LanguageCode.fra:
                    return 1.0m;
                case LanguageCode.zho:
                    return 0.2m;
                default:
                    return 1.0m;
            }
        }
    }
}
