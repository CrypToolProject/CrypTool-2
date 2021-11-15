using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Generators;
using CrypTool.PluginBase.Attributes;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Utils;

namespace OnlineDocumentationGenerator
{
    public class DocGenerator
    {
        public static string TemplateDirectory = "Templates";
        public static string CommonDirectory = "Common";
        public static Dictionary<string, List<TemplateDocumentationPage>> RelevantComponentToTemplatesMap = new Dictionary<string, List<TemplateDocumentationPage>>();
        public static Dictionary<string, Type> PluginInformations;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public static XMLReplacement XMLReplacement
        {
            get; 
            set;
        }

        public void Generate(string baseDir, Generator generator)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            generator.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;

            try
            {
                generator.OutputDir = baseDir;

                PluginInformations = ReadPluginInformations();

                var templatesDir = ReadTemplates(baseDir, "", null, generator);
                ReadPlugins(generator);
                ReadCommonDocPages(generator);

                // check if the plugins listed in the relevantPlugins tag actually exist
                foreach (var rc in RelevantComponentToTemplatesMap)
                {
                    if (!PluginInformations.Any(x => x.Value.Name == rc.Key))
                    {
                        // if plugin doesn't exist, try to find similar plugin names for correction suggestions
                        string k = rc.Key.ToLower();
                        
                        var alternatives = PluginInformations
                            .Select(x => x.Value.Name)
                            .Where(x => (x = Regex.Replace(x, @"\s", "").ToLower()) == k ||
                                        (x.Length >= 4 && k.StartsWith(x.Substring(0, 4))) ||
                                        (k.Length >= 4 && x.StartsWith(k.Substring(0, 4))) ||
                                        (x.Length >= 3 && k.Contains(x)) ||
                                        (k.Length >= 3 && x.Contains(k))
                                        )
                            .Union(FuzzySearch.Search(rc.Key, PluginInformations.Select(x => x.Value.Name).ToList(), 0.7));
                        string msg = $"Relevant plugin \"{rc.Key}\" not found. Referenced in: " + String.Join(", ", rc.Value.Select(e => $"\"{e.Name}\"")) + ".";
                        
                        if (alternatives.Any())
                            msg += "\nDid you mean: " + String.Join(", ", alternatives.Select(x => $"\"{x}\"")) + " ?";

                        GuiLogMessage(msg, NotificationLevel.Error);
                        Console.WriteLine(msg);
                    }
                }

                try
                {
                    generator.Generate(templatesDir);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while trying to generate documentation: {0}", ex.Message), NotificationLevel.Error);
                    Console.WriteLine("Error while trying to generate documentation: {0}", ex.Message);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        public static PluginDocumentationPage CreatePluginDocumentationPage(Type type)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                XElement xml = null;
                if(XMLReplacement != null && XMLReplacement.Type != null && XMLReplacement.Type.Equals(type))
                {
                    xml = XMLReplacement.XElement;
                }
                if (type.GetInterfaces().Contains(typeof(IEditor)))
                {                    
                    return new EditorDocumentationPage(type, xml);
                }
                else
                {
                    return new ComponentDocumentationPage(type, xml);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        private static int CompareTemplateDirectories(TemplateDirectory x, TemplateDirectory y)
        {
            if (x.Order < y.Order) return -1;
            if (x.Order > y.Order) return 1;
            return String.Compare(x.GetName(), y.GetName());
        }

        private static int CompareTemplateDocPages(TemplateDocumentationPage x, TemplateDocumentationPage y)
        {
            return String.Compare(x.CurrentLocalization.Name, y.CurrentLocalization.Name);
        }

        private TemplateDirectory ReadTemplates(string baseDir, string subdir, TemplateDirectory parent, Generator generator)
        {
            var directory = new DirectoryInfo(Path.Combine(baseDir, Path.Combine(TemplateDirectory, subdir)));
            var templateDir = new TemplateDirectory(directory, parent);

            //recursively analyze subdirs:
            foreach(var childdir in directory.GetDirectories())
            {
                var subDir = ReadTemplates(baseDir, Path.Combine(subdir, childdir.Name), templateDir, generator);
                templateDir.SubDirectories.Add(subDir);
            }
            templateDir.SubDirectories.Sort(CompareTemplateDirectories);

            var templatePath = new DirectoryInfo(Path.Combine(generator.OutputDir, OnlineHelp.HelpDirectory, OnlineHelp.RelativeTemplateDocDirectory));
            foreach (var file in directory.GetFiles().Where(x => (x.Extension.ToLower() == ".cwm")))
            {
                try
                {
                    var relTemplateFilePath = RelativePaths.GetRelativePath(file, templatePath);
                    var templatePage = new TemplateDocumentationPage(file.FullName, relTemplateFilePath, subdir, templateDir);
                    if (templatePage.RelevantPlugins != null)
                    {
                        foreach (var relevantPlugin in templatePage.RelevantPlugins)
                        {
                            if (RelevantComponentToTemplatesMap.ContainsKey(relevantPlugin))
                            {
                                RelevantComponentToTemplatesMap[relevantPlugin].Add(templatePage);
                            }
                            else
                            {
                                RelevantComponentToTemplatesMap.Add(relevantPlugin, new List<TemplateDocumentationPage>() { templatePage });
                            }
                        }
                    }
                    generator.AddDocumentationPage(templatePage);
                    templateDir.ContainingTemplateDocPages.Add(templatePage);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while trying to read templates for Online Help generation: {0} ({1})", ex.Message, file.FullName), NotificationLevel.Warning);
                }
                templateDir.ContainingTemplateDocPages.Sort(CompareTemplateDocPages);
            }
            return templateDir;
        }
        public static Dictionary<string, Type> ReadPluginInformations()
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins
            string CrypToolStoreSubFolder = "";

            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    CrypToolStoreSubFolder = "Developer";
                    break;
                case Ct2BuildType.Nightly:
                    CrypToolStoreSubFolder = "Nightly";
                    break;
                case Ct2BuildType.Beta:
                    CrypToolStoreSubFolder = "Beta";
                    break;
                case Ct2BuildType.Stable:
                    CrypToolStoreSubFolder = "Release";
                    break;
                default: //if no known version is given, we assume developer
                    CrypToolStoreSubFolder = "Developer";
                    break;
            }

            var pluginManager = new PluginManager(null, CrypToolStoreSubFolder);
            return pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies);
        }
        private void ReadPlugins(Generator generator)
        {
            foreach (Type type in PluginInformations.Values)
            {
                if (type.GetPluginInfoAttribute() != null)
                {
                    try
                    {
                        var p = CreatePluginDocumentationPage(type);
                        if (p != null)
                        {
                            generator.AddDocumentationPage(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("{0} error: {1}", type.GetPluginInfoAttribute().Caption, ex.Message),
                                      NotificationLevel.Error);
                    }
                }
            }
        }

        private void ReadCommonDocPages(Generator generator)
        {
            try
            {
                XElement xml0 = XElement.Parse(Properties.Resources.HomomorphicChiffres);
                XElement xml1 = XElement.Parse(Properties.Resources.CrypToolBook);
                XElement xml2 = XElement.Parse(Properties.Resources.PseudoRandomFunction_based_KeyDerivationFunctions);

                if(XMLReplacement != null && XMLReplacement.CommonDocId == 0)
                {
                    xml0 = XMLReplacement.XElement;
                }
                if (XMLReplacement != null && XMLReplacement.CommonDocId == 1)
                {
                    xml1 = XMLReplacement.XElement;
                }
                if (XMLReplacement != null && XMLReplacement.CommonDocId == 2)
                {
                    xml2 = XMLReplacement.XElement;
                }

                generator.AddDocumentationPage(new CommonDocumentationPage(0, xml0));
                generator.AddDocumentationPage(new CommonDocumentationPage(1, xml1));
                generator.AddDocumentationPage(new CommonDocumentationPage(2, xml2));

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error while trying to read common doc page: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
        }
    }

    public class XMLReplacement
    {
        public XMLReplacement()
        {
            CommonDocId = -1;
        }

        public Type Type
        {
            get; set;

        }

        public int CommonDocId
        {
            get;
            set;
        }

        public XElement XElement
        {
            get; set;
        }
    }
}
