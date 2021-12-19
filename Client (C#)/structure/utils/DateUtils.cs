using app.structure.services;
using app.structure.services.translation;
using System;

namespace app.structure.utils
{
    public class DateUtils
    {
        public static DateTime getDate(long ms)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(ms);
        }

        public static string getDateString(long ms, TranslationService translation)
        {
            DateTime date = getDate(ms);

            return getDateString(date, translation);
        }

        public static string getDateString(DateTime date, TranslationService translation)
        {
            bool isArabic = TranslationService.language == Languages.AR;

            string day = translation.translateNumeric(date.Day);
            string month = translation.translateNumeric(date.Month);
            string year = translation.translateNumeric(date.Year);

            return (isArabic ? year : day) + " / " + month + " / " + (isArabic ? day : year);
        }
    }
}
