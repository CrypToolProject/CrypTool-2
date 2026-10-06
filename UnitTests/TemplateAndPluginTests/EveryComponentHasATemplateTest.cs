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
        /// Loads plugin assemblies so their concrete components can be compared with templates.
        /// </summary>
        public EveryComponentHasATemplateTest()
        {
            TestHelpers.LoadPluginAssemblies();
        }

        /// <summary>
        /// Reports concrete components that do not appear in a template.
        /// </summary>
        [TestMethod]
        public void EveryConcreteComponentHasATemplate()
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

            Assert.IsTrue(iCrypComponentTypesWithoutTemplate.Count == 0,
                String.Format("{0} concrete components have no template: {1}",
                    iCrypComponentTypesWithoutTemplate.Count,
                    String.Join(", ", iCrypComponentTypesWithoutTemplate.Select(type => type.FullName))));
        }

        /// <summary>
        /// Gets the plugin types referenced by templates.
        /// </summary>
        /// <param name="templates">Paths to the template files.</param>
        /// <returns>Template paths grouped by component type.</returns>
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
        /// Gets concrete plugin component types from loaded assemblies.
        /// </summary>
        /// <returns>Concrete component types.</returns>
        public List<Type> GetAllICrypComponentTypes()
        {
            List<Type> crypComponents = new List<Type>();
            Type iCrypComponentType = typeof(ICrypComponent);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (iCrypComponentType.IsAssignableFrom(type) && !type.IsAbstract && type != iCrypComponentType)
                    {
                        crypComponents.Add(type);
                    }
                }
            }
            return crypComponents;
        }

        /// <summary>
        /// Gets all workspace template files.
        /// </summary>
        /// <returns>Paths to workspace templates.</returns>
        public string[] GetAllTemplates()
        {
            string[] allTemplates = Directory.GetFiles("Templates", "*.cwm", SearchOption.AllDirectories);
            return allTemplates;

        }
    }
}
