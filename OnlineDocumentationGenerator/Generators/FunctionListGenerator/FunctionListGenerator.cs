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
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Generators.FunctionListGenerator
{
    public class FunctionListGenerator : Generator
    {
        private enum ItemType { Component = 0, Template, Tutorial, Wizard };

        private class ItemDictionary : Dictionary<string, Dictionary<ItemType, HashSet<string>>>
        {
            public void Add(string name, ItemType typ, string path)
            {
                if (!ContainsKey(name))
                {
                    Add(name, new Dictionary<ItemType, HashSet<string>>());
                }
                if (!this[name].ContainsKey(typ))
                {
                    this[name].Add(typ, new HashSet<string>());
                }
                this[name][typ].Add(path);
            }
        }

        private readonly ItemDictionary itemlist = new ItemDictionary();

        public FunctionListGenerator()
        {
        }

        //
        // Generate a list of the functions implemented in CT2 for
        // https://www.CrypTool.org/de/ctp-dokumentation-de/ctp-functions-de
        //
        public override void Generate(TemplateDirectory templatesDir)
        {
            foreach (string lang in AvailableLanguages)
            {
                CultureInfo cultureInfo = CultureInfoHelper.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                // create list of functions
                itemlist.Clear();

                GetComponents(lang);
                GetWizard(lang);
                //GetTemplates(templatesDir, lang, "");

                // create CSV file
                string CSVDesc = GenerateCSVDescription();

                StoreFunctionList(CSVDesc, "FunctionList-" + lang + ".csv");

                // create text file
                string TextDesc = Properties.Resources.FunctionListTemplate
                    .Replace("\r", "")
                    .Replace("$VERSION$", GetVersion())
                    .Replace("$DATE$", DateTime.Now.ToString(CultureInfo.CurrentUICulture.DateTimeFormat))
                    + GenerateDescription();

                StoreFunctionList(TextDesc, "FunctionList-" + lang + ".txt");
            }
        }

        private string GenerateDescription()
        {
            List<string> list = itemlist.Keys.ToList();
            //list.Sort();

            StringBuilder result = new StringBuilder();

            foreach (string key in list)
            {
                List<ItemType> types = itemlist[key].Keys.ToList();
                //types.Sort();
                string occuringTypes = string.Join("/", types.Select(i => ItemType2Char(i)));

                bool firstLine = true;

                foreach (ItemType itemtype in types)
                {
                    foreach (string path in itemlist[key][itemtype])
                    {
                        result.Append(string.Format("{0,-50} {1,-10}", firstLine ? key : "", firstLine ? occuringTypes : ""));
                        result.Append(string.Format(" [{0}] {1}\n", ItemType2Char(itemtype), path));
                        firstLine = false;
                    }
                }

                result.Append("\n");
            }

            return result.ToString();
        }

        private string GenerateCSVDescription()
        {
            List<string> list = itemlist.Keys.ToList();
            //list.Sort();

            StringBuilder result = new StringBuilder();

            foreach (string key in list)
            {
                List<ItemType> types = itemlist[key].Keys.ToList();
                //types.Sort();
                string occuringTypes = string.Join("/", types.Select(i => ItemType2Char(i)));

                result.Append(string.Format("{0};{1};\n", key, occuringTypes));

                foreach (ItemType itemtype in types)
                {
                    foreach (string path in itemlist[key][itemtype])
                    {
                        result.Append(string.Format(";[{0}];{1}\n", ItemType2Char(itemtype), path));
                    }
                }

                result.Append("\n");
            }

            return result.ToString();
        }

        private void GetComponents(string lang)
        {
            IEnumerable<ComponentDocumentationPage> componentDocumentationPages = DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x);

            IOrderedEnumerable<ComponentDocumentationPage> query = from pages in componentDocumentationPages
                                                                   orderby pages.Name
                                                                   select pages;

            try
            {
                foreach (ComponentDocumentationPage page in query)
                {
                    string linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                    LocalizedComponentDocumentationPage pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];
                    PluginInfoAttribute pinfo = pp.PluginType.GetPluginInfoAttribute();
                    FunctionListAttribute[] flas = (FunctionListAttribute[])pp.PluginType.GetCustomAttributes(typeof(FunctionListAttribute), false);

                    switch (page.Category)
                    {
                        case ComponentCategory.Undefined:
                            itemlist.Add(pp.Name, ItemType.Tutorial, Properties.Resources.FL_Tutorial + "\\ " + pp.Name);
                            foreach (FunctionListAttribute fla in flas)
                            {
                                fla.PluginType = pinfo.PluginType;
                                itemlist.Add(fla.Function, ItemType.Tutorial, Properties.Resources.FL_Tutorial + "\\ " + pp.Name + "\\ " + fla.Path);
                            }
                            break;
                        default:
                            string catpath = GetComponentCategory(page.Category) + "\\ " + pp.Name;
                            itemlist.Add(pp.Name, ItemType.Component, catpath);
                            foreach (FunctionListAttribute fla in flas)
                            {
                                fla.PluginType = pinfo.PluginType;
                                string path = (fla.Path != "") ? fla.Path : catpath;
                                itemlist.Add(fla.Function, ItemType.Component, path);
                            }
                            break;
                    }

                    foreach (TemplateDocumentationPage tmpl in pp.Templates.Templates)
                    {
                        string p = tmpl.CurrentLocalization.CategoryPath() + "\\ " + tmpl.CurrentLocalization.Name;
                        itemlist.Add(pp.Name, ItemType.Template, p);
                    }
                }

            }
            catch (Exception)
            {
            }
        }

        private void GetTemplates(TemplateDirectory templatesDir, string lang, string path)
        {
            foreach (TemplateDocumentationPage template in templatesDir.ContainingTemplateDocPages)
            {
                string p = template.CurrentLocalization.CategoryPath() + "\\ " + template.CurrentLocalization.Name;
                itemlist.Add(template.CurrentLocalization.Name, ItemType.Template, p);
            }

            foreach (TemplateDirectory dir in templatesDir.SubDirectories)
            {
                GetTemplates(dir, lang, path + dir.LocalizedInfos[lang].Name + "\\ ");
            }
        }

        private void GetWizard(string lang)
        {
            XElement wizardXML = GetWizardXML();
            if (wizardXML != null)
            {
                GetWizardElements(wizardXML, lang, "");
            }
        }

        // get xml configuration of wizard
        private XElement GetWizardXML()
        {
            try
            {
                Assembly a = Assembly.Load("Wizard");
                Type wizard = a.GetType("Wizard.Wizard");
                MethodInfo init = wizard.GetMethod("Initialize");
                object obj = Activator.CreateInstance(wizard);
                init.Invoke(obj, null);
                MethodInfo getxml = wizard.GetMethod("WizardConfigXML");
                return (XElement)getxml.Invoke(obj, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void GetWizardElements(XElement root, string lang, string path)
        {
            IEnumerable<XElement> inputs = root.Elements("input");
            foreach (XElement elem in inputs)
            {
                XElement name = XMLHelper.GetGlobalizedElementFromXML(elem, "name");
                itemlist.Add(name.Value, ItemType.Wizard, path + name.Value);
            }

            IEnumerable<XElement> categories = root.Elements("category");
            foreach (XElement elem in categories)
            {
                XElement name = XMLHelper.GetGlobalizedElementFromXML(elem, "name");
                GetWizardElements(elem, lang, path + name.Value + "\\ ");
            }
        }

        private string ItemType2Char(ItemType t)
        {
            switch (t)
            {
                case ItemType.Component: return Properties.Resources.FL_Letter_Component;
                case ItemType.Template: return Properties.Resources.FL_Letter_Template;
                case ItemType.Tutorial: return Properties.Resources.FL_Letter_Tutorial;
                case ItemType.Wizard: return Properties.Resources.FL_Letter_Wizard;
                default: return "?";
            }
        }

        private string GetVersion()
        {
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    return "Developer " + AssemblyHelper.Version;
                case Ct2BuildType.Nightly:
                    return "Nightly Build " + AssemblyHelper.Version;
                case Ct2BuildType.Beta:
                    return "Beta " + AssemblyHelper.Version;
                case Ct2BuildType.Stable:
                    return "Stable " + AssemblyHelper.Version;
                default:
                    return AssemblyHelper.Version.ToString();
            }
        }

        private static string GetComponentCategory(ComponentCategory category)
        {
            switch (category)
            {
                case ComponentCategory.CiphersClassic:
                    return Properties.Resources.Category_FL_Classic_Ciphers;
                case ComponentCategory.CiphersModernSymmetric:
                    return Properties.Resources.Category_FL_CiphersModernSymmetric;
                case ComponentCategory.CiphersModernAsymmetric:
                    return Properties.Resources.Category_FL_CiphersModernAsymmetric;
                case ComponentCategory.Steganography:
                    return Properties.Resources.Category_FL_Steganography;
                case ComponentCategory.HashFunctions:
                    return Properties.Resources.Category_FL_HashFunctions;
                case ComponentCategory.CryptanalysisSpecific:
                    return Properties.Resources.Category_FL_CryptanalysisSpecific;
                case ComponentCategory.CryptanalysisGeneric:
                    return Properties.Resources.Category_FL_CryptanalysisGeneric;
                case ComponentCategory.Protocols:
                    return Properties.Resources.Category_FL_Protocols;
                case ComponentCategory.ToolsBoolean:
                    return Properties.Resources.Category_FL_ToolsBoolean;
                case ComponentCategory.ToolsDataflow:
                    return Properties.Resources.Category_FL_ToolsDataflow;
                case ComponentCategory.ToolsDataInputOutput:
                    return Properties.Resources.Category_FL_ToolsDataInputOutput;
                case ComponentCategory.ToolsRandomNumbers:
                    return Properties.Resources.Category_ToolsRandomNumbers;
                case ComponentCategory.ToolsCodes:
                    return Properties.Resources.Category_ToolsCodes;
                case ComponentCategory.ToolsMisc:
                    return Properties.Resources.Category_FL_ToolsMisc;
                default:
                    return Properties.Resources.Category_FL_Unknown;
            }
        }

        private void StoreFunctionList(string content, string filename)
        {
            string filePath = Path.Combine(OutputDir, filename);

            try
            {
                if (!Directory.Exists(OutputDir))
                {
                    Directory.CreateDirectory(OutputDir);
                }

                StreamWriter streamWriter = new StreamWriter(filePath, false, Encoding.GetEncoding("utf-8"));
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while trying to write file: {0}, Message: {1}", filePath, ex.Message);
            }
        }
    }
}