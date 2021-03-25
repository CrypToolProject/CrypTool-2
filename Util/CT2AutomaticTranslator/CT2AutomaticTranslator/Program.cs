/*
   Copyright 2018 Nils Kopal, nils.kopal@uni-kassel.de

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
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace CT2AutomaticTranslator
{
    /// <summary>
    /// This software is intented for automatic translation of the CT2 application 
    /// using the google cloud translation service
    /// It is able to automatically translate:
    /// - 1) all resource files of CT2 components (create new ones; add to svn)
    /// ---> you need to have svn installed and be executable, i.e. be in your global path
    /// - 2) all templates
    /// - 3) all help files
    /// - 4) the Wizard config
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This program uses Google Cloud translation to create an automatic translation of CrypTool 2.
        /// Before start, create a Google Cloud user and get an API key.
        /// Also make sure to clean up your CT2 SVN (delete all files not belonging to SVN with cleanup)
        /// </summary>
        static string targetDirectory = @"C:\Users\nilsk\Desktop\CrypTool2";
        static string templateDirectory = targetDirectory + @"\Templates\";
        static string wizardDirectory = targetDirectory + @"\CrypPlugins\Wizard\";

        static string targetLanguage = "zh-CN";
        static string key = "AIzaSyA3ho9Fwu_0OMcZ-CnswSELKSZVjrpko6g"; // enter here the google auth key

        private static Dictionary<string, Project> projectCache = new Dictionary<string, Project>();

        static void Main(string[] args)
        {
            //Create a Google API instance with the given key
            GoogleTranslator.Init(key);

            //Set console to utf-8
            Console.OutputEncoding = Encoding.UTF8;

            var startTime = DateTime.Now;

            //Translate every resource(s) file found in tagretDirectory and all sub directories
            Console.WriteLine("Start translating resource files of CT2");
            WalkDirectory_CrypTool2Components(targetDirectory);
            Console.WriteLine("Finished translating resource files of CT2");
            Console.WriteLine("######################################################################");
            Thread.Sleep(3000);
            
            //Translate every template file found in templateDirectory and all sub directories
            Console.WriteLine("Start translating templates of CT2");
            WalkDirectory_CrypTool2Templates(templateDirectory);
            Console.WriteLine("Finished translating templates of CT2");
            Console.WriteLine("######################################################################");
            Thread.Sleep(3000);
            
            //Translate online help; search for every xml file containing a starting <documentation> tag
            Console.WriteLine("Start translating online help of CT2");
            WalkDirectory_CrypTool2OnlineHelp(targetDirectory);
            Console.WriteLine("Finished translating online help of CT2");
            Console.WriteLine("######################################################################");
            Thread.Sleep(3000);
            
            //Translate Wizard; search for every xml file containing a starting <category> tag
            Console.WriteLine("Start translating Wizard of CT2");
            WalkDirectory_Wizard(wizardDirectory);
            Console.WriteLine("Finished translating Wizard of CT2");
            Console.WriteLine("######################################################################");

            Console.WriteLine("Complete translation took {0}", DateTime.Now - startTime);
            Console.WriteLine("CT2AutomaticTranslator terminated...");

            Console.ReadLine();
        }

        /// <summary>
        /// Walks through a directory structure
        /// Recursevely calls itself
        /// Translates all resource(s) files
        /// </summary>
        /// <param name="targetDirectory"></param>
        private static void WalkDirectory_CrypTool2Components(string targetDirectory)
        {
            try
            {
                if (targetDirectory.ToLower().Contains("properties") || targetDirectory.ToLower().Contains("resources"))
                {
                    Console.WriteLine("Working now on directory: {0}", targetDirectory);
                    string resourcesFile = FindResourcesFile(targetDirectory);
                    if (resourcesFile != null)
                    {
                        Console.WriteLine("Resources file: {0}", resourcesFile);
                        var result = CopyResourceFile(resourcesFile);
                        TranslateResourceFile(result);
                    }
                    else
                    {
                        Console.WriteLine("Resources file not found!!");
                    }

                    string resourceFile = FindResourceFile(targetDirectory);
                    if (resourceFile != null)
                    {
                        Console.WriteLine("Resources file: {0}", resourceFile);
                        var result = CopyResourceFile(resourceFile);
                        TranslateResourceFile(result);
                    }
                    else
                    {
                        Console.WriteLine("Resource file not found!!");
                    }
                    Thread.Sleep(1000);
                }
                //recursevely call itself with all subdirectories
                foreach (string subdirectory in Directory.GetDirectories(targetDirectory.ToString()))
                {
                    WalkDirectory_CrypTool2Components(subdirectory.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception during walking of " + targetDirectory + " : " + ex.Message);
                Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// Walks through a directory structure
        /// Recursevely calls itself
        /// Translates all template files
        /// </summary>
        /// <param name="targetDirectory"></param>
        private static void WalkDirectory_CrypTool2Templates(string targetDirectory)
        {
            TranslateTemplateDirectoryXML(targetDirectory);
            Thread.Sleep(1000);

            foreach (string file in Directory.GetFiles(targetDirectory, "*.xml"))
            {
                TranslateTemplate(file);
                Thread.Sleep(1000);
            }
            //recursevely call itself with all subdirectories
            foreach (string subdirectory in Directory.GetDirectories(targetDirectory.ToString()))
            {                
                WalkDirectory_CrypTool2Templates(subdirectory.ToString());
            }
        }

        /// <summary>
        /// Walks through a directory structure
        /// Recursevely calls itself
        /// Translates all online help files
        /// </summary>
        /// <param name="targetDirectory"></param>
        private static void WalkDirectory_CrypTool2OnlineHelp(string targetDirectory)
        {
            foreach (string file in Directory.GetFiles(targetDirectory, "*.xml"))
            {
                if (TranslateOnlineHelp(file))
                {
                    //only when we found a file to translate, we sleep after translation to 
                    //not come over the limit of google cloud translation service
                    Thread.Sleep(1000);
                }
            }

            //recursevely call itself with all subdirectories
            foreach (string subdirectory in Directory.GetDirectories(targetDirectory.ToString()))
            {
                WalkDirectory_CrypTool2OnlineHelp(subdirectory.ToString());
            }
        }

        /// <summary>
        /// Walks through a directory structure
        /// Recursevely calls itself
        /// Translates all wizard files
        /// </summary>
        /// <param name="wizardDirectory"></param>
        private static void WalkDirectory_Wizard(string wizardDirectory)
        {
            foreach (string file in Directory.GetFiles(wizardDirectory, "*.xml"))
            {
                if (TranslateWizardFile(file))
                {
                    //only when we found a file to translate, we sleep after translation to 
                    //not come over the limit of google cloud translation service
                    Thread.Sleep(1000);
                }
            }

            //recursevely call itself with all subdirectories
            foreach (string subdirectory in Directory.GetDirectories(wizardDirectory.ToString()))
            {
                WalkDirectory_Wizard(subdirectory.ToString());
            }
        }

        /// <summary>
        /// Translate a given wizard file to target language
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool TranslateWizardFile(string file)
        {
            try
            {
                Console.WriteLine("Translate wizard config xml file {0}", file);
                var doc = XDocument.Load(file);
                var category_element = doc.Element("category");
                if (category_element == null)
                {
                    Console.WriteLine("The xml file {0} is not a valid wizard config file. Aborting now...", file);
                    return false;
                }

                //search for every tag having a lang="en" attribute and translate it to target language
                //we have to translate:
                // - tag content
                // - content attribute

                //1. collect things to translate
                var elements_to_translate = new List<XElement>();
                var sourceStrings = new List<string>();
                foreach (XElement element in category_element.Descendants())
                {
                    //look, if it has a lang="en" attribute
                    bool hasLangAttribute = false;
                    foreach (var attribute in element.Attributes("lang"))
                    {
                        if (attribute.Value != null && attribute.Value.Equals("en"))
                        {
                            hasLangAttribute = true;
                            break;
                        }
                    }
                    //if we have no lang="en"; we just go on with next element
                    if (hasLangAttribute == false)
                    {
                        continue;
                    }

                    //collect here strings and xelements to translate
                    bool hasData = false;
                    if (element.Value != null && !element.Value.Equals(string.Empty) && !element.Value.Equals(""))
                    {
                        hasData = true;
                        sourceStrings.Add(element.Value);
                    }
                    if (element.Attribute("content") != null && element.Attribute("content").Value != null &&
                        !element.Attribute("content").Value.Equals(string.Empty) && !element.Attribute("content").Value.Equals(""))
                    {
                        hasData = true;
                        sourceStrings.Add((element.Attribute("content").Value));
                    }
                    if (hasData)
                    {
                        elements_to_translate.Add(element);
                    }
                }

                //2. translate everything
                var destinationStrings = new List<string>();
                int i = 0;
                while (i < sourceStrings.Count)
                {
                    List<string> sendStrings = new List<string>();
                    for (int j = 0; i < sourceStrings.Count && j < 100; j++)
                    {
                        sendStrings.Add(sourceStrings[i]);
                        i++;
                    }
                    destinationStrings.AddRange(GoogleTranslator.Translate(sendStrings, targetLanguage));
                }

                //3. add to config file
                foreach (var element in elements_to_translate)
                {
                    XElement translatedElement = new XElement(element.Name);
                    translatedElement.Add(new XAttribute("lang", targetLanguage));

                    if (element.Value != null && !element.Value.Equals(string.Empty) && !element.Value.Equals(""))
                    {
                        translatedElement.Value = destinationStrings[0];
                        destinationStrings.RemoveAt(0);
                    }
                    if (element.Attribute("content") != null && element.Attribute("content").Value != null &&
                        !element.Attribute("content").Value.Equals(string.Empty) && !element.Attribute("content").Value.Equals(""))
                    {
                        var attribute = new XAttribute("content", targetLanguage);
                        attribute.Value = destinationStrings[0];
                        translatedElement.Add(attribute);
                        destinationStrings.RemoveAt(0);
                    }
                    element.AddAfterSelf(translatedElement);
                }
                doc.Save(file);
                Console.WriteLine("Successfully translated wizard config xml file {0}", file);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception occured during translation: {0}", ex.Message);
                Console.Error.WriteLine(ex);
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a given file is a help file. If yes, it translates it to target language
        /// </summary>
        /// <param name="file"></param>
        private static bool TranslateOnlineHelp(string file)
        {
            try
            {
                Console.WriteLine("Translate online documentation xml file {0}", file);
                var doc = XDocument.Load(file);
                var documenation_element = doc.Element("documentation");
                if (documenation_element == null)
                {
                    Console.WriteLine("The xml file {0} is not an online documentation xml. Aborting now...", file);
                    return false;
                }
                var language_element = documenation_element.Element("language");
                var introduction_element = documenation_element.Element("introduction");
                var usage_element = documenation_element.Element("usage");
                var presentation_element = documenation_element.Element("presentation");
                if (language_element == null)
                {
                    Console.WriteLine("Did not find any language element. Aborting now...");
                    return false;
                }

                List<string> sourceStrings = new List<string>();
                List<string> destinationStrings;

                if (introduction_element != null && introduction_element.Value != null && !introduction_element.Value.Equals(String.Empty) && !introduction_element.Value.Equals(""))
                {
                    sourceStrings.Add(introduction_element.Value);
                }
                if (usage_element != null && usage_element.Value != null && !usage_element.Value.Equals(String.Empty) && !usage_element.Value.Equals(""))
                {
                    sourceStrings.Add(usage_element.Value);
                }
                if (presentation_element != null && presentation_element.Value != null && !presentation_element.Value.Equals(String.Empty) && !presentation_element.Value.Equals(""))
                {
                    sourceStrings.Add(presentation_element.Value);
                }

                destinationStrings = GoogleTranslator.Translate(sourceStrings, targetLanguage);

                XElement new_language_element = new XElement("language");
                new_language_element.Add(new XAttribute("culture", targetLanguage));
                language_element.AddAfterSelf(new_language_element);

                if (introduction_element != null && introduction_element.Value != null && !introduction_element.Value.Equals(String.Empty) && !introduction_element.Value.Equals(""))
                {                    
                    XElement translated_introduction = new XElement("introduction");
                    translated_introduction.Value = destinationStrings[0];
                    translated_introduction.Add(new XAttribute("lang", targetLanguage));
                    destinationStrings.RemoveAt(0);
                    introduction_element.AddAfterSelf(translated_introduction);
                }
                if (usage_element != null && usage_element.Value != null && !usage_element.Value.Equals(String.Empty) && !usage_element.Value.Equals(""))
                {
                    XElement translated_usage = new XElement("usage");
                    translated_usage.Value = destinationStrings[0];
                    translated_usage.Add(new XAttribute("lang", targetLanguage));
                    destinationStrings.RemoveAt(0);
                    usage_element.AddAfterSelf(translated_usage);
                }
                if (presentation_element != null && presentation_element.Value != null && !presentation_element.Value.Equals(String.Empty) && !presentation_element.Value.Equals(""))
                {
                    XElement translated_presentation = new XElement("presentation");
                    translated_presentation.Value = destinationStrings[0];
                    translated_presentation.Add(new XAttribute("lang", targetLanguage));
                    destinationStrings.RemoveAt(0);
                    presentation_element.AddAfterSelf(translated_presentation);
                }

                doc.Save(file);
                Console.WriteLine("Successfully translated online documentation xml file {0}", file);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception occured during translation: {0}", ex.Message);
                Console.Error.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Method to find a "resources.resx" file in a given directory
        /// Returns null if not found
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static string FindResourcesFile(string dir)
        {
            foreach (string filename in Directory.GetFiles(dir, "resources.resx"))
            {
                return filename;
            }
            return null;
        }

        /// <summary>
        /// Method to find a "resources.resx" file in a given directory
        /// Returns null if not found
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static string FindResourceFile(string dir)
        {
            foreach (string filename in Directory.GetFiles(dir, "resource.resx"))
            {
                return filename;
            }
            return null;
        }

        /// <summary>
        /// Creates a copy of a taret "resources.resx" file and appends
        /// language code.
        /// Example: Resources.resx ---> Resources.fr.resx
        /// </summary>
        /// <param name="resourceFile"></param>
        private static string CopyResourceFile(string resourceFile)
        {
            string newFilename = resourceFile.Substring(0, resourceFile.Length - 5) + "." + targetLanguage + ".resx";

            Console.WriteLine("Copy resource file from {0} to {1}", resourceFile, newFilename);
            try
            {                
                if (File.Exists(newFilename))
                {
                    Console.WriteLine("Copy alread exists... delete it now!");
                    File.Delete(newFilename);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not delete: {0}", ex.Message);
                Console.Error.WriteLine(ex);
            }

            try
            {                
                File.Copy(resourceFile, newFilename);
                Console.WriteLine("Copy created");
                SVNAdd(newFilename);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not copy: {0}", ex.Message);
                Console.Error.WriteLine(ex);
                return null;
            }
            return newFilename;
        }

        /// <summary>
        /// Add the given file to SVN using the installed svn tool
        /// Tested and works with TortoiseSVN
        /// </summary>
        /// <param name="file"></param>
        private static void SVNAdd(string file)
        {
            try
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("svn.exe", "add \"" + file + "\"");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                Console.Write("Add file {0} to svn", file);
                while (!p.HasExited)
                {
                    Thread.Sleep(500);
                    Console.Write(".");
                }
                if (p.ExitCode == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Successfully added to svn...");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error: SVN add exitit with exitcode {0}", p.ExitCode);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine();
                Console.Error.WriteLine("Exception occured during SVN add: {0} ", ex.Message);
                Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// Translates a given resource files
        /// --> creates new files (.resx and .cs)
        /// --> adds new files to svn
        /// --> adds created files to .csproj
        /// </summary>
        /// <param name="resourceFile"></param>
        private static void TranslateResourceFile(string resourceFile){

            Console.WriteLine("Translating now resource file {0}", resourceFile);
            var doc = XDocument.Load(resourceFile);

            List<XElement> elements = new List<XElement>();
            List<string> sourceStrings = new List<string>();
            
            //Collect elements for translation
            foreach(var element in doc.Root.DescendantNodes().OfType<XElement>())
            {
                if (element.Name.ToString().Equals("data"))
                {
                    bool foundPreserve = false;
                    foreach (var attribute in element.Attributes())
                    {
                        if(attribute.Value.ToString().Equals("preserve")){
                            foundPreserve = true;
                            break;
                        }
                    }
                    if (foundPreserve && !element.Value.Equals(""))
                    {                        
                        elements.Add(element);
                        sourceStrings.Add(element.Descendants().First().Value.ToString());
                    }
                }
            }
            
            //Send to google and translate
            List<string> destinationStrings = new List<string>();
            try
            {
                List<int> empty_string_indexes = new List<int>();
                int i = 0;
                while (i < sourceStrings.Count)
                {
                    List<string> sendStrings = new List<string>();
                    for (int j = 0; i < sourceStrings.Count && j < 100; j++)
                    {
                        if (sourceStrings[i] == null || sourceStrings[i].Equals(string.Empty) || sourceStrings[i].Equals(""))
                        {
                            empty_string_indexes.Add(i);
                            sourceStrings[i] = "nothing to translate";
                        }
                        sendStrings.Add(sourceStrings[i]);
                        i++;
                    }
                    destinationStrings.AddRange(GoogleTranslator.Translate(sendStrings, targetLanguage));
                }
                foreach (int k in empty_string_indexes)
                {
                    destinationStrings[k] = "";
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception during translation: {0}", ex);
                Console.Error.WriteLine(ex);
                return;
            }
            if (destinationStrings == null || destinationStrings.Count == 0)
            {
                return;
            }
            for (int i = 0; i < destinationStrings.Count; i++)
            {
                //Console.WriteLine(sourceStrings[i] + " ==> " + destinationStrings[i]);
                elements[i].Descendants().First().Value = destinationStrings[i];
            }
            doc.Save(resourceFile);

            //add to cs project
            
            //1: get parent path
            var dir = Directory.GetParent(resourceFile);
            dir = dir.Parent;
            
            //2: open cs project file
            var files = Directory.GetFiles(dir.FullName, "*.csproj");
            if (files.Length == 0)
            {
                return;
            }
            Project project;
            if (!projectCache.ContainsKey(files[0]))
            {
                project = new Project(files[0]);
                projectCache.Add(files[0], project);
            }
            else
            {
                project = projectCache[files[0]];
            }

            var metadata = new List<KeyValuePair<string, string>>();
            metadata.Add(new KeyValuePair<string, string>("Generator", "ResXFileCodeGenerator"));

            if (resourceFile.ToLower().Contains("properties\\resources") && resourceFile.ToLower().Contains("properties"))
            {           
                project.AddItemFast("EmbeddedResource", "Properties\\Resources." + targetLanguage + ".resx", metadata);
                File.Create(Directory.GetParent(resourceFile).FullName + "\\Resources." + targetLanguage + ".Designer.cs").Close();
                SVNAdd(Directory.GetParent(resourceFile).FullName + "\\Resources." + targetLanguage + ".Designer.cs");
            }
            else if (resourceFile.ToLower().Contains("properties\\resource") && resourceFile.ToLower().Contains("properties"))
            {             
                project.AddItemFast("EmbeddedResource", "Properties\\Resource." + targetLanguage + ".resx", metadata);
                File.Create(Directory.GetParent(resourceFile).FullName + "\\Resource." + targetLanguage + ".Designer.cs").Close();
                SVNAdd(Directory.GetParent(resourceFile).FullName + "\\Resource." + targetLanguage + ".Designer.cs");
            }
            else if (resourceFile.ToLower().Contains("resources\\resources\\"))
            {               
                project.AddItemFast("EmbeddedResource", "Resources\\Resources." + targetLanguage + ".resx", metadata);
                File.Create(Directory.GetParent(resourceFile).FullName + "\\Resources." + targetLanguage + ".Designer.cs").Close();
                SVNAdd(Directory.GetParent(resourceFile).FullName + "\\Resources." + targetLanguage + ".Designer.cs");
            }
            else if (resourceFile.ToLower().Contains("resources\\resource\\"))
            {             
                project.AddItemFast("EmbeddedResource", "Resources\\Resource." + targetLanguage + ".resx", metadata);
                File.Create(Directory.GetParent(resourceFile).FullName + "\\Resource." + targetLanguage + ".Designer.cs").Close();
                SVNAdd(Directory.GetParent(resourceFile).FullName + "\\Resource." + targetLanguage + ".Designer.cs");
            }
            project.Save();
            Console.WriteLine("Successfully translated resource file {0}", resourceFile);
        }

        /// <summary>
        /// Translates a template xml to target language
        /// </summary>
        /// <param name="file"></param>
        public static void TranslateTemplate(string file)
        {
            Console.WriteLine("Translating template: {0}", file);
            try
            {

                var doc = XDocument.Load(file);
                var sampleElement = doc.Element("sample");
                if (sampleElement == null)
                {
                    Console.WriteLine("No valid template file: sample element missing. Aborting...");
                    return;
                }

                //Select Title
                var title_element = sampleElement.Element("title");
                if (title_element == null)
                {
                    Console.WriteLine("No title found. Aborting...");
                    return;
                }
                var title_text = title_element.Value;

                //Select Summary
                var summary_element = sampleElement.Element("summary");
                var summary_text = summary_element.Value;

                //Select Description
                var description_element = sampleElement.Element("description");
                var description_text = description_element.Value;

                //Select Keywords
                var keywords_element = sampleElement.Element("keywords");
                var keywords_text = keywords_element.Value;

                //Select Replacements
                var replacements_element = sampleElement.Element("replacements");
                List<string> replacements_texts = new List<string>();
                if (replacements_element != null)
                {
                    foreach (XElement replacement_element in replacements_element.Elements("replacement"))
                    {
                        replacements_texts.Add(replacement_element.Attribute("value").Value);
                    }
                }

                //translate everything in one step
                List<string> sourceTexts = new List<string>();
                sourceTexts.Add(title_text);
                sourceTexts.Add(summary_text);
                sourceTexts.Add(description_text);
                sourceTexts.Add(keywords_text);
                sourceTexts.AddRange(replacements_texts);

                var translatedTexts = GoogleTranslator.Translate(sourceTexts, targetLanguage);
                //generate everything new

                var translated_title_text = translatedTexts[0];
                translatedTexts.RemoveAt(0);
                XElement translated_title = new XElement("title");
                translated_title.Value = translated_title_text;
                translated_title.Add(new XAttribute("lang", targetLanguage));

                var translated_summary_text = translatedTexts[0];
                translatedTexts.RemoveAt(0);
                XElement translated_summary_element = new XElement("summary");
                translated_summary_element.Value = translated_summary_text;
                translated_summary_element.Add(new XAttribute("lang", targetLanguage));

                var translated_description_text = translatedTexts[0];
                translatedTexts.RemoveAt(0);
                XElement translated_description_element = new XElement("description");
                translated_description_element.Value = translated_description_text;
                translated_description_element.Add(new XAttribute("lang", targetLanguage));

                var translated_keywords_text = translatedTexts[0];
                translatedTexts.RemoveAt(0);
                XElement translated_keywords_element = new XElement("keywords");
                translated_keywords_element.Value = translated_keywords_text;
                translated_keywords_element.Add(new XAttribute("lang", targetLanguage));

                //at this point, only the replacements are left in the translated texts

                title_element.AddAfterSelf(translated_title);
                summary_element.AddAfterSelf(translated_summary_element);
                description_element.AddAfterSelf(translated_description_element);
                keywords_element.AddAfterSelf(translated_keywords_element);

                //finally, generate replacements
                if (replacements_element != null)
                {
                    var translated_replacements_element = new XElement("replacements");
                    translated_replacements_element.Add(new XAttribute("lang", targetLanguage));
                    replacements_element.AddAfterSelf(translated_replacements_element);

                    int i = 0;
                    foreach (XElement replacement_element in replacements_element.Elements("replacement"))
                    {
                        XElement translated_replacement_element = new XElement("replacement");
                        translated_replacement_element.Add(new XAttribute("key", replacement_element.Attribute("key").Value));
                        // added .Replace(@"\ ",@"\")) to fix a problem with formatting of memo fields... 
                        // here, the \b for example was replaced by \ b with a space. thus we have to fix it
                        translated_replacement_element.Add(new XAttribute("value", translatedTexts[i].Replace(@"\ ",@"\")));
                        translated_replacements_element.Add(translated_replacement_element);
                        i++;
                    }
                }
                doc.Save(file);
                Console.WriteLine("Successfully translated template: {0}", file);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception during translation of template: {0} : {1}", file, ex.Message);
                Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// Translates a dir.xml of targetDirectory to target language
        /// </summary>
        /// <param name="targetDirectory"></param>
        private static void TranslateTemplateDirectoryXML(string targetDirectory)
        {
            Console.WriteLine("Translating template directory file: {0}", targetDirectory + @"\dir.xml");
            if (!File.Exists(targetDirectory + @"\dir.xml"))
            {
                Console.WriteLine("dir.xml does not exist. Aborting...");
                return;
            }

            try
            {
                var doc = XDocument.Load(targetDirectory + @"\dir.xml");

                var directory_element = doc.Element("directory");
                if (directory_element == null)
                {
                    Console.WriteLine("No valid directory xml file! Aborting...");
                    return;
                }

                var name_element = directory_element.Element("name");
                var summary_element = directory_element.Element("summary");

                var sourceTexts = new List<string>();
                sourceTexts.Add(name_element.Value);
                sourceTexts.Add(summary_element.Value);

                var translated_texts = GoogleTranslator.Translate(sourceTexts, targetLanguage);

                XElement translated_name_element = new XElement("name");
                translated_name_element.Value = translated_texts[0];
                translated_name_element.Add(new XAttribute("lang", targetLanguage));

                XElement translated_summary_element = new XElement("summary");
                translated_summary_element.Value = translated_texts[1];
                translated_summary_element.Add(new XAttribute("lang", targetLanguage));

                name_element.AddAfterSelf(translated_name_element);
                summary_element.AddAfterSelf(translated_summary_element);

                doc.Save(targetDirectory + @"\dir.xml");

                Console.WriteLine("Successfully translated template directory file: {0}", targetDirectory + @"\dir.xml");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception during translation of template directory file: {0} : {1}", targetDirectory + @"\dir.xml", ex.Message);
                Console.Error.WriteLine(ex);
            }
        }
    }
}
