using System.Globalization;

namespace OnlineDocumentationGenerator.Utils
{
    public static class CultureInfoHelper
    {
        public static CultureInfo GetCultureInfo(string lang)
        {
            CultureInfo cultureInfo = new CultureInfo("en");
            if (lang.Equals("zh"))
            {
                cultureInfo = new CultureInfo("zh-CN");
            }
            else
            {
                cultureInfo = new CultureInfo(lang);
            }
            return cultureInfo;
        }
    }
}
