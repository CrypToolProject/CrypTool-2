using System;
using System.Globalization;

namespace OnlineDocumentationGenerator.Utils
{
    public static class CultureInfoHelper
    {
        public static CultureInfo GetCultureInfo(string lang)
        {
            try
            {
                return new CultureInfo(lang);
            }
            catch (Exception ex)
            {
                return new CultureInfo("en");
            }
        }
    }
}
