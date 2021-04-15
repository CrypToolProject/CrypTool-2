/*
   Copyright 2018-2021 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.TemplateAndPluginTests;
using WorkspaceManager.Model;

namespace UnitTests
{

    [TestClass]
    public class TemplateLoadingTest
    {
        
        public TemplateLoadingTest()
        {
            TestHelpers.SetAssemblyPaths();
        }

        /// <summary>
        /// This test tests if each template can be loaded and if each type (component) used inside the template can be instantiated
        /// </summary>
        [TestMethod]
        public void TemplateLoadingTestMethod()
        {
            string[] allTemplates = Directory.GetFiles("Templates", "*.cwm", SearchOption.AllDirectories);
            string[] experimentalTemplates = Directory.GetFiles("Templates\\Experimental", "*.cwm", SearchOption.AllDirectories);

            //we only want to test the non-experimental templates
            //thus, we create a list without those from experimental folder
            List<string> templates = new List<string>();            
            foreach (string template in allTemplates)
            {
                if (!experimentalTemplates.Contains(template))
                {
                    templates.Add(template);
                }
            }

            foreach (string template in templates)
            {
                try
                {
                    var modelPersistance = new ModelPersistance();
                    var workspaceModel = modelPersistance.loadModel(template);
                    foreach (PluginModel pluginModel in workspaceModel.GetAllPluginModels())
                    {
                        if (pluginModel.Plugin == null)
                        {
                            Assert.Fail(String.Format("Plugin {0} of template {1} could not be instantiated!",
                                pluginModel.GetName(), template));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail(String.Format("Exception during loading of template {0}: {1}", template, ex.Message));
                }
            }
        }
    }
}