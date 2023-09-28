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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using Ionic.Zip;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class HtmlGenerator : Generator
    {
        private readonly Type _typeToGenerate = null;
        private readonly int _commonDocId = -1;

        private static readonly Dictionary<string, string> _languagePresentationString = new Dictionary<string, string>() { { "en", "English" }, { "de", "Deutsch" } };
        private static readonly Dictionary<string, string> _languagePresentationIcon = new Dictionary<string, string>() { { "en", "en.png" }, { "de", "de.png" } };

        private ObjectConverter _objectConverter;
        private TemplateDirectory _templatesDir;

        public HtmlGenerator()
        {

        }

        /// <summary>
        /// When constructed with this constructor, only the help of the component with the defined type is generated
        /// </summary>
        /// <param name="typeToGenerate"></param>
        public HtmlGenerator(Type typeToGenerate)
        {
            _typeToGenerate = typeToGenerate;
        }

        /// <summary>
        /// When constructed with this constructor, only the common page with the defined id is generated
        /// </summary>
        /// <param name="commonDocId"></param>
        public HtmlGenerator(int commonDocId)
        {
            _commonDocId = commonDocId;
        }

        public override void Generate(TemplateDirectory templatesDir)
        {
            _templatesDir = templatesDir;
            _objectConverter = new ObjectConverter(DocPages, OutputDir);

            GenerateDocPages();

            if (_typeToGenerate == null && _commonDocId == -1)
            {
                GenerateComponentIndexPages();
                GenerateTemplateIndexPages();
                GenerateEditorIndexPages();
                GenerateCommonIndexPages();
            }

            CopyAdditionalResources();
        }

        private void GenerateComponentIndexPages()
        {
            foreach (string lang in AvailableLanguages)
            {
                CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                string indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateComponentsIndex, lang);
                indexHtml = TagReplacer.ReplaceInstallVersionSwitchs(indexHtml, AssemblyHelper.InstallationType);
                string languageSelectionCode = GenerateIndexLanguageSelectionCode(AvailableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);

                string componentListCode = GenerateComponentListCode(DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceComponentList(indexHtml, componentListCode);
                string componentTreeCode = GenerateComponentTreeCode(DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceComponentTree(indexHtml, componentTreeCode);

                string filename = OnlineHelp.GetComponentIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private void GenerateTemplateIndexPages()
        {
            foreach (string lang in AvailableLanguages)
            {
                CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                string templatesHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateTemplatesIndex, lang);
                string languageSelectionCode = GenerateTemplatesPageLanguageSelectionCode(AvailableLanguages, lang);
                templatesHtml = TagReplacer.ReplaceLanguageSelectionTag(templatesHtml, languageSelectionCode);

                string templatesTreeCode = GenerateTemplatesTreeCode(lang);
                templatesHtml = TagReplacer.ReplaceTemplatesTree(templatesHtml, templatesTreeCode);
                string templatesListCode = GenerateTemplatesListCode(lang);
                templatesHtml = TagReplacer.ReplaceTemplatesList(templatesHtml, templatesListCode);

                string filename = OnlineHelp.GetTemplatesIndexFilename(lang);
                StoreIndexPage(templatesHtml, filename);
            }
        }

        private void GenerateEditorIndexPages()
        {
            foreach (string lang in AvailableLanguages)
            {
                CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                string indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateEditorIndex, lang);
                indexHtml = TagReplacer.ReplaceInstallVersionSwitchs(indexHtml, AssemblyHelper.InstallationType);
                string languageSelectionCode = GenerateEditorLanguageSelectionCode(AvailableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);
                string editorListCode = GenerateEditorListCode(DocPages.FindAll(x => x is EditorDocumentationPage).Select(x => (EditorDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceEditorList(indexHtml, editorListCode);

                string filename = OnlineHelp.GetEditorIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private void GenerateCommonIndexPages()
        {
            foreach (string lang in AvailableLanguages)
            {
                CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                string commonHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateCommonIndex, lang);
                string languageSelectionCode = GenerateCommonLanguageSelectionCode(AvailableLanguages, lang);
                commonHtml = TagReplacer.ReplaceLanguageSelectionTag(commonHtml, languageSelectionCode);
                string commonListCode = GenerateCommonListCode(DocPages.FindAll(x => x is CommonDocumentationPage).Select(x => (CommonDocumentationPage)x), lang);
                commonHtml = TagReplacer.ReplaceCommonList(commonHtml, commonListCode);

                string filename = OnlineHelp.GetCommonIndexFilename(lang);
                StoreIndexPage(commonHtml, filename);
            }
        }

        private readonly List<TemplateDocumentationPage> AllTemplatesList = new List<TemplateDocumentationPage>();

        private string GenerateTemplatesListCode(string lang)
        {
            AllTemplatesList.Clear();
            WalkTemplateDirectory2(_templatesDir);
            IEnumerable<IGrouping<char, TemplateDocumentationPage>> groups = AllTemplatesList
                .Where(t => !string.IsNullOrWhiteSpace(t.CurrentLocalization.Name))
                .OrderBy(t => t.CurrentLocalization.Name)
                .GroupBy(t => t.CurrentLocalization.Name[0]);

            string anchor = "<p>" + string.Concat(groups.Select(g => string.Format("<a href=\"#{0}\"><b>{0}</b></a>&nbsp;\n", g.Key))) + "</p>";

            StringBuilder stringBuilder = new StringBuilder(anchor);
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\" class=\"filterable\">");

            foreach (IGrouping<char, TemplateDocumentationPage> group in groups)
            {
                stringBuilder.AppendLine(string.Format("<tr class=\"letter\"><td><h2 id=\"{0}\">{0}</h2></td><td></td></tr>", group.Key));
                foreach (TemplateDocumentationPage templateDocumentationPage in group)
                {
                    LocalizedTemplateDocumentationPage locTemplate = templateDocumentationPage.CurrentLocalization;
                    string description = _objectConverter.Convert(locTemplate.SummaryOrDescription, templateDocumentationPage);
                    description = description.Replace("../", ""); //correct relative paths in images
                    string iconPath = Path.Combine(templateDocumentationPage.DocDirPath, $"{templateDocumentationPage.Name}.png");
                    stringBuilder.AppendLine(string.Format("<tr class=\"filterable\"><td>&nbsp;</td><td><div class=\"boximage\"><img src=\"{0}\"></div></td><td><a href=\"{1}\">{2}</a></td><td>{3}</td></tr>",
                        iconPath, locTemplate.FilePath, locTemplate.Name, description));
                }
            }

            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine(string.Format("<script type=\"text/javascript\"> lang = \"{0}\"; </script>", lang));
            stringBuilder.AppendLine("<script type=\"text/javascript\" src=\"filterTable.js\"></script>");

            return stringBuilder.ToString();
        }

        private void WalkTemplateDirectory2(TemplateDirectory templatesDir)
        {
            AllTemplatesList.AddRange(templatesDir.ContainingTemplateDocPages);
            foreach (TemplateDirectory dir in templatesDir.SubDirectories)
            {
                WalkTemplateDirectory2(dir);
            }
        }

        private string GenerateTemplatesTreeCode(string lang)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder anchorBuilder = new StringBuilder();

            anchorBuilder.Append("<p>");
            anchorBuilder.Append("<ul>");

            foreach (TemplateDirectory dir in _templatesDir.SubDirectories)
            {
                WalkTemplateDirectory(dir, anchorBuilder, stringBuilder, new List<string>(), lang);
            }

            anchorBuilder.Append("</ul>");
            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);

            return anchorBuilder.ToString();
        }

        /// <summary>
        /// Unique id for div of template tables
        /// </summary>
        private int _uid;

        private void WalkTemplateDirectory(TemplateDirectory templatesDir, StringBuilder anchorBuilder, StringBuilder stringBuilder, List<string> categories, string lang)
        {
            string id = "ID_" + (++_uid);

            categories.Add(templatesDir.GetName());

            if (templatesDir.ContainingTemplateDocPages.Count > 0)
            {
                stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");
                stringBuilder.AppendLine(string.Format("<tr><td colspan=4><h2 id=\"{0}\">{1}</h2></td></tr>", id, string.Join(" / ", categories)));

                foreach (TemplateDocumentationPage templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
                {
                    LocalizedTemplateDocumentationPage locTemplate = templateDocumentationPage.CurrentLocalization;
                    string description = _objectConverter.Convert(locTemplate.SummaryOrDescription, templateDocumentationPage);
                    string spaces = string.Join("", Enumerable.Range(0, categories.Count - 1).Select(x => "&nbsp;&nbsp;"));
                    description = description.Replace("../", ""); //correct relative paths in images
                    string iconPath = Path.Combine(templateDocumentationPage.DocDirPath, $"{templateDocumentationPage.Name}.png");
                    stringBuilder.AppendLine(
                        string.Format("<tr><td>{0}</td><td><div class=\"boximage\"><img src=\"{1}\"></div></td><td><a href=\"{2}\">{3}</a></td><td>{4}</td></tr>",
                        spaces, iconPath, locTemplate.FilePath, locTemplate.Name, description));
                }

                stringBuilder.AppendLine("</table><p>");
            }

            anchorBuilder.AppendLine(string.Format("<li><a href=\"#{0}\"><b>{1}</b></a></li>", id, templatesDir.GetName()));

            if (templatesDir.SubDirectories.Count > 0)
            {
                anchorBuilder.Append("<ul>");
                foreach (TemplateDirectory dir in templatesDir.SubDirectories)
                {
                    WalkTemplateDirectory(dir, anchorBuilder, stringBuilder, categories, lang);
                }

                anchorBuilder.Append("</ul>");
            }

            categories.RemoveAt(categories.Count - 1);
        }

        private static string GenerateComponentListCode(IEnumerable<ComponentDocumentationPage> componentDocumentationPages, string lang)
        {
            IEnumerable<IGrouping<char, ComponentDocumentationPage>> query = from pages in componentDocumentationPages
                                                                             orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                                                                             group pages by pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name[0];

            string anchor = "<p>" + string.Concat(query.Select(g => string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;\n", g.Key))) + "</p>";

            StringBuilder stringBuilder = new StringBuilder(anchor);
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\" class=\"filterable\">");

            foreach (IGrouping<char, ComponentDocumentationPage> pagegroup in query)
            {
                stringBuilder.AppendLine(string.Format("<tr class=\"letter\"><td colspan=2><h2 id=\"{0}\">{0}</h2></td></tr>", pagegroup.Key));
                foreach (ComponentDocumentationPage page in pagegroup)
                {
                    string linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                    LocalizedComponentDocumentationPage pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];
                    stringBuilder.AppendLine(string.Format("<tr class=\"filterable\"><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                        OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
                }
            }

            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine(string.Format("<script type=\"text/javascript\"> lang = \"{0}\"; </script>", lang));
            stringBuilder.AppendLine("<script type=\"text/javascript\" src=\"filterTable.js\"></script>");

            return stringBuilder.ToString();
        }

        private static string GenerateComponentTreeCode(IEnumerable<PluginDocumentationPage> componentDocumentationPages, string lang)
        {
            var query = from pages in componentDocumentationPages
                        orderby pages.Category
                        group pages by pages.Category into g
                        select new { Key = GetCategoryName(g.Key), Elements = g };

            string anchor = "<p><ul>" + string.Join("\n", query.Select(g => string.Format("<li><a href=\"#{0}\"><b>{0}</b><a></li>", g.Key))) + "</ul></p>";

            StringBuilder stringBuilder = new StringBuilder(anchor);
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            foreach (var pagegroup in query)
            {
                stringBuilder.AppendLine(string.Format("<tr><td colspan=2><h2 id=\"{0}\">{0}</h2></td></tr>", pagegroup.Key));
                foreach (PluginDocumentationPage page in pagegroup.Elements)
                {
                    string linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                    LocalizedComponentDocumentationPage pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];
                    stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                        OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
                }
            }

            stringBuilder.AppendLine("</table>");

            return stringBuilder.ToString();
        }

        private static string GenerateEditorListCode(IEnumerable<PluginDocumentationPage> editorDocumentationPages, string lang)
        {
            StringBuilder stringBuilderListCode = new StringBuilder();
            stringBuilderListCode.AppendLine("<table width=\"100%\"  border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            IOrderedEnumerable<PluginDocumentationPage> query = from pages in editorDocumentationPages
                                                                orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                                                                select pages;

            foreach (PluginDocumentationPage page in query)
            {
                string linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                LocalizedEditorDocumentationPage pp = (LocalizedEditorDocumentationPage)page.Localizations[linkedLang];
                stringBuilderListCode.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }

            stringBuilderListCode.AppendLine("</table>");

            return stringBuilderListCode.ToString();
        }

        private static string GenerateCommonListCode(IEnumerable<CommonDocumentationPage> commonDocumentationPages, string lang)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            IOrderedEnumerable<CommonDocumentationPage> query = from pages in commonDocumentationPages
                                                                orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                                                                select pages;

            foreach (CommonDocumentationPage page in query)
            {
                string linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                LocalizedCommonDocumentationPage pp = (LocalizedCommonDocumentationPage)page.Localizations[linkedLang];
                stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td></tr>",
                    OnlineHelp.GetCommonDocFilename(page.Name, linkedLang), pp.Name));
            }

            stringBuilder.AppendLine("</table>");

            return stringBuilder.ToString();
        }

        private static string GetCategoryName(ComponentCategory category)
        {
            switch (category)
            {
                case ComponentCategory.CiphersClassic:
                    return Properties.Resources.Category_Classic_Ciphers;
                case ComponentCategory.CiphersModernSymmetric:
                    return Properties.Resources.Category_CiphersModernSymmetric;
                case ComponentCategory.CiphersModernAsymmetric:
                    return Properties.Resources.Category_CiphersModernAsymmetric;
                case ComponentCategory.Steganography:
                    return Properties.Resources.Category_Steganography;
                case ComponentCategory.HashFunctions:
                    return Properties.Resources.Category_HashFunctions;
                case ComponentCategory.CryptanalysisSpecific:
                    return Properties.Resources.Category_CryptanalysisSpecific;
                case ComponentCategory.CryptanalysisGeneric:
                    return Properties.Resources.Category_CryptanalysisGeneric;
                case ComponentCategory.Protocols:
                    return Properties.Resources.Category_Protocols;
                case ComponentCategory.ToolsBoolean:
                    return Properties.Resources.Category_ToolsBoolean;
                case ComponentCategory.ToolsDataflow:
                    return Properties.Resources.Category_ToolsDataflow;
                case ComponentCategory.ToolsDataInputOutput:
                    return Properties.Resources.Category_ToolsDataInputOutput;
                case ComponentCategory.ToolsRandomNumbers:
                    return Properties.Resources.Category_ToolsRandomNumbers;
                case ComponentCategory.ToolsCodes:
                    return Properties.Resources.Category_ToolsCodes;
                case ComponentCategory.ToolsMisc:
                    return Properties.Resources.Category_ToolsMisc;
                default:
                    return Properties.Resources.Category_Unknown;
            }
        }

        private static string GetDocumentationTemplate(EntityDocumentationPage entityDocPage)
        {
            if (entityDocPage is EditorDocumentationPage)
            {
                return Properties.Resources.TemplateEditorDocumentationPage;
            }

            if (entityDocPage is ComponentDocumentationPage)
            {
                return Properties.Resources.TemplateComponentDocumentationPage;
            }

            if (entityDocPage is TemplateDocumentationPage)
            {
                return Properties.Resources.TemplateTemplateDocumentationPage;
            }

            if (entityDocPage is CommonDocumentationPage)
            {
                return Properties.Resources.TemplateCommonDocumentationPage;
            }

            throw new Exception(string.Format("Unknown documentation page type {0}!", entityDocPage.GetType()));
        }

        private void GenerateDocPages()
        {
            foreach (EntityDocumentationPage documentationPage in DocPages)
            {
                //this here allows the generation of a single documentation page
                //to do so, the type of the component has to be given in constructor of the
                //HtmlGenerator
                if (_typeToGenerate != null || _commonDocId > 0)
                {
                    if (documentationPage is PluginDocumentationPage)
                    {
                        PluginDocumentationPage pluginDocumentationPage = (PluginDocumentationPage)documentationPage;
                        if (!pluginDocumentationPage.PluginType.Equals(_typeToGenerate))
                        {
                            continue;
                        }
                    }
                    else if (documentationPage is CommonDocumentationPage)
                    {
                        CommonDocumentationPage commonDocumentationPage = (CommonDocumentationPage)documentationPage;
                        if (commonDocumentationPage.Id != _commonDocId)
                        {
                            continue;
                        }

                    }
                    else
                    {
                        continue;
                    }

                }

                foreach (string lang in documentationPage.AvailableLanguages)
                {
                    try
                    {
                        LocalizedEntityDocumentationPage localizedEntityDocumentationPage = documentationPage.Localizations[lang];

                        CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;

                        string html = TagReplacer.ReplaceLanguageSwitchs(GetDocumentationTemplate(documentationPage), lang);
                        html = TagReplacer.ReplaceDocItemTags(html, localizedEntityDocumentationPage, _objectConverter);
                        string languageSelectionCode = GenerateLanguageSelectionCode(documentationPage, documentationPage.AvailableLanguages, lang);
                        html = TagReplacer.ReplaceLanguageSelectionTag(html, languageSelectionCode);
                        LocalizedComponentDocumentationPage localizedComponentDocumentationPage = localizedEntityDocumentationPage as LocalizedComponentDocumentationPage;
                        if (localizedComponentDocumentationPage != null)
                        {
                            html = TagReplacer.ReplaceSectionSwitchs(html, localizedComponentDocumentationPage);
                        }
                        string filename = documentationPage.Localizations[lang].FilePath;
                        StoreDocPage(html, filename);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                    }
                }
            }
        }

        private void StoreDocPage(string html, string filename)
        {
            string filePath = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, filename));
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                StreamWriter streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }

        private void StoreIndexPage(string html, string filename)
        {
            string filePath = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, filename));
            try
            {
                if (!Directory.Exists(Path.Combine(OutputDir, OnlineHelp.HelpDirectory)))
                {
                    Directory.CreateDirectory(Path.Combine(OutputDir, OnlineHelp.HelpDirectory));
                }

                StreamWriter streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }

        private static string GenerateLanguageSelectionCodeBase(IEnumerable<string> availableLanguages, Dictionary<string, string[]> langdata, string lang)
        {
            List<string> languagesList = availableLanguages.ToList();
            languagesList.Sort();

            IEnumerable<string> langs = languagesList.Select(new Func<string, string>(l =>
            {
                string s = string.Format("<img src=\"{0}\" border=\"0\"/>&nbsp;{1}", langdata[l][0], _languagePresentationString[l]);
                if (l != lang)
                {
                    s = string.Format("<a href=\"{0}\">{1}</a>", langdata[l][1], s);
                }

                return s;
            }));

            return string.Join("&nbsp; | &nbsp;", langs) + "&nbsp;";
        }

        private static string GenerateLanguageSelectionCode(EntityDocumentationPage entityDocumentationPage, IEnumerable<string> availableLanguages, string lang)
        {
            string iconPath = Path.Combine(entityDocumentationPage.DocDirPath.Split(Path.PathSeparator).Select(p => "..").ToArray());

            Dictionary<string, string[]> langdata = availableLanguages.ToDictionary(l => l, l => new string[] { Path.Combine(iconPath, _languagePresentationIcon[l]), Path.GetFileName(entityDocumentationPage.Localizations[l].FilePath) });
            return GenerateLanguageSelectionCodeBase(availableLanguages, langdata, lang);
        }

        private static string GenerateIndexLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            Dictionary<string, string[]> langdata = availableLanguages.ToDictionary(l => l, l => new string[] { _languagePresentationIcon[l], OnlineHelp.GetComponentIndexFilename(l) });
            return GenerateLanguageSelectionCodeBase(availableLanguages, langdata, lang);
        }

        private static string GenerateTemplatesPageLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            Dictionary<string, string[]> langdata = availableLanguages.ToDictionary(l => l, l => new string[] { _languagePresentationIcon[l], OnlineHelp.GetTemplatesIndexFilename(l) });
            return GenerateLanguageSelectionCodeBase(availableLanguages, langdata, lang);
        }

        private static string GenerateEditorLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            Dictionary<string, string[]> langdata = availableLanguages.ToDictionary(l => l, l => new string[] { _languagePresentationIcon[l], OnlineHelp.GetEditorIndexFilename(l) });
            return GenerateLanguageSelectionCodeBase(availableLanguages, langdata, lang);
        }

        private static string GenerateCommonLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            Dictionary<string, string[]> langdata = availableLanguages.ToDictionary(l => l, l => new string[] { _languagePresentationIcon[l], OnlineHelp.GetCommonIndexFilename(l) });
            return GenerateLanguageSelectionCodeBase(availableLanguages, langdata, lang);
        }

        private void CopyAdditionalResources()
        {
            bool developer = AssemblyHelper.InstallationType == Ct2InstallationType.Developer;

            XElement additionalResources = XElement.Parse(Properties.Resources.AdditionalResources);
            foreach (XElement r in additionalResources.Elements("file"))
            {
                try
                {
                    string path = r.Attribute("path").Value;
                    int sIndex = path.IndexOf('/');
                    Uri resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                       path.Substring(0, sIndex), path.Substring(sIndex + 1)));
                    string fileName = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, Path.GetFileName(path)));

                    using (Stream resStream = Application.GetResourceStream(resUri).Stream)
                    using (StreamWriter streamWriter = new System.IO.StreamWriter(fileName, false))
                    {
                        resStream.CopyTo(streamWriter.BaseStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error trying to copy additional resource: {0}", ex.Message));
                }
            }

            foreach (XElement r in additionalResources.Elements("archive"))
            {
                XAttribute excl = r.Attribute("excludeDeveloper");
                if (!developer || excl == null || excl.Value.ToLower() == "false")
                {
                    try
                    {
                        string path = r.Attribute("path").Value;
                        int sIndex = path.IndexOf('/');
                        Uri resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                           path.Substring(0, sIndex), path.Substring(sIndex + 1)));

                        //Extract archive:
                        using (Stream resStream = Application.GetResourceStream(resUri).Stream)
                        using (ZipFile zipPackage = ZipFile.Read(resStream))
                        {
                            zipPackage.ExtractAll(OnlineHelp.HelpDirectory, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error trying to copy additional resource archive: {0}", ex.Message));
                    }
                }
            }
        }
    }
}