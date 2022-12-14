/*
   Copyright 2022 Nils Kopal <Nils.Kopal<AT>CrypTool.org>

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WorkspaceManager.Model;

namespace UnitTests
{

    [TestClass]
    public class EveryComponentHasATemplateTest
    {
        /// <summary>
        /// This test tests, which ICrypComponent have their own template file. It generates a report in standard out
        /// </summary>
        public EveryComponentHasATemplateTest()
        {
            TestHelpers.SetAssemblyPaths();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CrypPlugins";

            foreach (string assemblyFile in Directory.GetFiles(path, "*.dll",SearchOption.TopDirectoryOnly))
            {
                try
                {
                    string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                    Assembly.Load(assemblyName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't load assembly {0}: {1}.", assemblyFile, ex);
                }
            }
        }

        /// <summary>
        /// This test tests if each component is used in at least one template
        /// </summary>
        [TestMethod]
        public void EveryComponentHasATemplateTestMethod()
        {
            string[] allTemplates = GetAllTemplates();
            Dictionary<Type, List<string>> pluginTypesUsedInTemplatesDictionary = GetPluginTypesUsedInTemplatesDictionary(allTemplates);

            List<Type> allICrypComponentTypes = GetAllICrypComponentTypes();
            List<Type> iCrypComponentTypesWithoutTemplate = new List<Type>();

            Console.WriteLine("##########################");
            Console.WriteLine("Components with templates:");
            Console.WriteLine("##########################");
            foreach (Type type in allICrypComponentTypes)
            {
                if (pluginTypesUsedInTemplatesDictionary.Keys.Contains(type))
                {
                    Console.WriteLine("Component {0} is used in {1} templates", type.FullName, pluginTypesUsedInTemplatesDictionary[type].Count);
                }
                else
                {
                    iCrypComponentTypesWithoutTemplate.Add(type);                  
                }
            }

            if (iCrypComponentTypesWithoutTemplate.Count > 0)
            {
                Console.WriteLine("#############################");
                Console.WriteLine("Components without templates:");
                Console.WriteLine("#############################");
                foreach (Type type in iCrypComponentTypesWithoutTemplate)
                {
                    Console.WriteLine("Component {0} is not used in any template", type.FullName);
                }
            }

            Assert.IsTrue(iCrypComponentTypesWithoutTemplate.Count == 0, String.Format("{0} components have no template. See standard out for details!", iCrypComponentTypesWithoutTemplate.Count));
        }

        /// <summary>
        /// Get all plugin types used in templates
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        private Dictionary<Type, List<string>> GetPluginTypesUsedInTemplatesDictionary(string[] templates)
        {
            Dictionary<Type, List<string>> pluginTypesUsedInTemplatesDictionary = new Dictionary<Type, List<string>>();
            ModelPersistance persistance = new ModelPersistance();

            foreach (string template in templates) 
            {
                WorkspaceModel workspaceModel = persistance.loadModel(template);

                foreach(PluginModel pluginModel in workspaceModel.GetAllPluginModels()) 
                {
                    Type type = pluginModel.PluginType;
                    if (!pluginTypesUsedInTemplatesDictionary.ContainsKey(type))
                    {
                        pluginTypesUsedInTemplatesDictionary[type] = new List<string>
                        {
                            template
                        };
                        continue;
                    }
                    if (!pluginTypesUsedInTemplatesDictionary[type].Contains(template))
                    {
                        pluginTypesUsedInTemplatesDictionary[type].Add(template);
                    }                                
                }            
            }
            return pluginTypesUsedInTemplatesDictionary;
        }

        /// <summary>
        /// Get all ICrypComponent Types
        /// </summary>
        /// <returns></returns>
        public List<Type> GetAllICrypComponentTypes()
        {
            List<Type> crypComponents = new List<Type>();
            Type iCrypComponentType = typeof(ICrypComponent);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (iCrypComponentType.IsAssignableFrom(type) && type != iCrypComponentType)
                    {
                        crypComponents.Add(type);
                    }
                }
            }
            return crypComponents;
        }

        /// <summary>
        /// Returns a list of all template files
        /// </summary>
        /// <returns></returns>
        public string[] GetAllTemplates()
        {
            string[] allTemplates = Directory.GetFiles("Templates", "*.cwm", SearchOption.AllDirectories);
            return allTemplates;

        }
    }
}