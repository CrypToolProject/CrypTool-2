/*
   Copyright 2008 - 2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase.Attributes;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.Properties;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    internal class TagReplacer
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
            string property;
            StringBuilder htmlBuilder = new StringBuilder(html);

            while ((property = FindDocItemTag(htmlBuilder.ToString(), out int pos, out int len)) != null)
            {
                try
                {
                    System.Reflection.PropertyInfo prop = localizedDocumentationPage.GetType().GetProperty(property);
                    object propVal = prop.GetValue(localizedDocumentationPage, null);
                    string propStr = objectConverter == null ? (propVal == null ? Resources.Null : propVal.ToString()) : objectConverter.Convert(propVal, localizedDocumentationPage.DocumentationPage);

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
            Match match = FindDocItemTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            if (!match.Success || match.Groups.Count < 2)
            {
                return null;
            }

            string property = match.Groups[1].Value;
            return property;
        }

        public static string ReplaceLanguageSwitchs(string html, string lang)
        {
            StringBuilder htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                int pos = match.Index;
                int len = match.Length;

                Match match2 = FindEndingLanguageSwitchTagRegex.Match(htmlBuilder.ToString(), pos + len);
                if (!match2.Success)
                {
                    throw new Exception("Error trying to replace language switch!");
                }

                int pos2 = match2.Index;
                int len2 = match2.Length;

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
            StringBuilder htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningInstallationVersionSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                int pos = match.Index;
                int len = match.Length;

                Match match2 = FindEndingInstallationVersionSwitchTagRegex.Match(htmlBuilder.ToString(), pos + len);
                if (!match2.Success)
                {
                    throw new Exception("Error trying to replace installation version switch!");
                }

                int pos2 = match2.Index;
                int len2 = match2.Length;

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
            StringBuilder htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningSectionSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                int pos = match.Index;
                int len = match.Length;

                Match match2 = FindEndingSectionSwitchTagRegex.Match(htmlBuilder.ToString(), pos + len);
                if (!match2.Success)
                {
                    throw new Exception("Error trying to replace section switch!");
                }

                int pos2 = match2.Index;
                int len2 = match2.Length;

                switch (match.Groups[1].Value)
                {
                    case "introduction":
                        if (page.Introduction == null ||
                            page.Introduction.Value == null ||
                            page.Introduction.Value == string.Empty)
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
                            page.Manual.Value == string.Empty)
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
                            page.Presentation.Value == string.Empty)
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
            Match match = tag.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceTags(string html, Regex tag, string replacement)
        {
            StringBuilder htmlBuilder = new StringBuilder(html);

            while (FindTag(tag, htmlBuilder.ToString(), out int pos, out int len))
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