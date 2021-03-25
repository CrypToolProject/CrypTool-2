using System;
using System.Text;
using System.Text.RegularExpressions;
using CrypTool.PluginBase.Attributes;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    class TagReplacer
    {
        private static readonly Regex FindDocItemTagRegex = new Regex("<docItem.*?property=\"(.*?)\".*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindLanguageSelectionTagRegex = new Regex("<languageSelection.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindBeginningLanguageSwitchTagRegex = new Regex("<languageSwitch.*?lang=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEndingLanguageSwitchTagRegex = new Regex("</.*?languageSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindBeginningInstallationVersionSwitchTagRegex = new Regex("<installationVersionSwitch.*?version=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEndingInstallationVersionSwitchTagRegex = new Regex("</.*?installationVersionSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindComponentListTagRegex = new Regex("<componentList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindComponentTreeTagRegex = new Regex("<componentTree.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEditorListTagRegex = new Regex("<editorList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindTemplateListTagRegex = new Regex("<templatesList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindTemplateTreeTagRegex = new Regex("<templatesTree.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindCommonListTagRegex = new Regex("<commonList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindBeginningSectionSwitchTagRegex = new Regex("<sectionSwitch.*?section=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEndingSectionSwitchTagRegex = new Regex("</.*?sectionSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ReplaceDocItemTags(string html, LocalizedEntityDocumentationPage localizedDocumentationPage, ObjectConverter objectConverter)
        {
            int pos;
            int len;
            string property;
            var htmlBuilder = new StringBuilder(html);

            while ((property = FindDocItemTag(htmlBuilder.ToString(), out pos, out len)) != null)
            {
                try
                {
                    var prop = localizedDocumentationPage.GetType().GetProperty(property);
                    var propVal = prop.GetValue(localizedDocumentationPage, null);
                    var propStr = objectConverter == null ? (propVal == null ? Resources.Null : propVal.ToString()) : objectConverter.Convert(propVal, localizedDocumentationPage.DocumentationPage);

                    htmlBuilder.Remove(pos, len);
                    htmlBuilder.Insert(pos, propStr);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error trying to replace DocItem tag with property {0}! Message: {1}", property, ex.Message));
                }
            }

            return htmlBuilder.ToString();
        }

        internal static string FindDocItemTag(string html, out int pos, out int len)
        {
            var match = FindDocItemTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            if (!match.Success || match.Groups.Count < 2)
                return null;
            var property = match.Groups[1].Value;
            return property;
        }

        public static string ReplaceLanguageSwitchs(string html, string lang)
        {
            var htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                var pos = match.Index;
                var len = match.Length;

                var match2 = FindEndingLanguageSwitchTagRegex.Match(htmlBuilder.ToString(), pos+len);
                if (!match2.Success)
                    throw new Exception("Error trying to replace language switch!");
                var pos2 = match2.Index;
                var len2 = match2.Length;
                
                if (match.Groups[1].Value == lang)
                {
                    htmlBuilder.Remove(pos2, len2);
                    htmlBuilder.Remove(pos, len);
                }
                else
                {
                    htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                }

                match = FindBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            }

            return htmlBuilder.ToString();
        }

        public static string ReplaceInstallVersionSwitchs(string html, Ct2InstallationType installationType)
        {
            var htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningInstallationVersionSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                var pos = match.Index;
                var len = match.Length;

                var match2 = FindEndingInstallationVersionSwitchTagRegex.Match(htmlBuilder.ToString(), pos + len);
                if (!match2.Success)
                    throw new Exception("Error trying to replace installation version switch!");
                var pos2 = match2.Index;
                var len2 = match2.Length;

                if (MatchesInstallationType(match.Groups[1].Value, installationType))
                {
                    htmlBuilder.Remove(pos2, len2);
                    htmlBuilder.Remove(pos, len);
                }
                else
                {
                    htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                }

                match = FindBeginningInstallationVersionSwitchTagRegex.Match(htmlBuilder.ToString());
            }

            return htmlBuilder.ToString();
        }       

        private static bool MatchesInstallationType(string typeText, Ct2InstallationType installationType)
        {
            bool neg = false;
            if (typeText[0] == '~')
            {
                neg = true;
                typeText = typeText.Substring(1);
            }

            switch (typeText)
            {
                case "Developer":
                    return (neg ^ (installationType == Ct2InstallationType.Developer));
                case "ZIP":
                    return (neg ^ (installationType == Ct2InstallationType.ZIP));
                case "MSI":
                    return (neg ^ (installationType == Ct2InstallationType.MSI));
                default:
                    return false;
            }
        }

        public static string ReplaceSectionSwitchs(string html, LocalizedComponentDocumentationPage page)
        {
            var htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningSectionSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                var pos = match.Index;
                var len = match.Length;

                var match2 = FindEndingSectionSwitchTagRegex.Match(htmlBuilder.ToString(), pos + len);
                if (!match2.Success)
                    throw new Exception("Error trying to replace section switch!");
                var pos2 = match2.Index;
                var len2 = match2.Length;

                switch (match.Groups[1].Value)
                {
                    case "introduction":
                        if (page.Introduction == null ||
                            page.Introduction.Value == null ||
                            page.Introduction.Value == String.Empty)
                        {
                            htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                        }
                        else
                        {
                            htmlBuilder.Remove(pos2, len2);
                            htmlBuilder.Remove(pos, len);
                        }
                        break;
                    case "usage":
                        if (page.Manual == null ||
                            page.Manual.Value == null ||
                            page.Manual.Value == String.Empty)
                        {
                            htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                        }
                        else
                        {
                            htmlBuilder.Remove(pos2, len2);
                            htmlBuilder.Remove(pos, len);
                        }
                        break;
                    case "presentation":
                        if (page.Presentation == null ||
                            page.Presentation.Value == null ||
                            page.Presentation.Value == String.Empty)
                        {
                            htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                        }
                        else
                        {
                            htmlBuilder.Remove(pos2, len2);
                            htmlBuilder.Remove(pos, len);
                        }
                        break;
                }
                
                match = FindBeginningSectionSwitchTagRegex.Match(htmlBuilder.ToString());
            }

            return htmlBuilder.ToString();
        }

        internal static bool FindTag(Regex tag, string html, out int pos, out int len)
        {
            var match = tag.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceTags(string html, Regex tag, string replacement)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindTag(tag, htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, replacement);
            }

            return htmlBuilder.ToString();
        }

        public static string ReplaceLanguageSelectionTag(string html, string languageSelectionCode)
        {
            return ReplaceTags(html, FindLanguageSelectionTagRegex, languageSelectionCode);
        }

        public static string ReplaceComponentList(string html, string componentListCode)
        {
            return ReplaceTags(html, FindComponentListTagRegex, componentListCode);
        }

        public static string ReplaceComponentTree(string html, string componentTreeCode)
        {
            return ReplaceTags(html, FindComponentTreeTagRegex, componentTreeCode);
        }

        public static string ReplaceCommonList(string html, string commonListCode)
        {
            return ReplaceTags(html, FindCommonListTagRegex, commonListCode);
        }

        public static string ReplaceTemplatesList(string html, string templatesListCode)
        {
            return ReplaceTags(html, FindTemplateListTagRegex, templatesListCode);
        }

        public static string ReplaceTemplatesTree(string html, string templatesTreeCode)
        {
            return ReplaceTags(html, FindTemplateTreeTagRegex, templatesTreeCode);
        }

        public static string ReplaceEditorList(string html, string editorListCode)
        {
            return ReplaceTags(html, FindEditorListTagRegex, editorListCode);
        }
    }
}