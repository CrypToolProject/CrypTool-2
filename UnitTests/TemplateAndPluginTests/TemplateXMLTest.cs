/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTests
{

    [TestClass]
    public class TemplateXMLTest
    {
        public TemplateXMLTest()
        {
        }

        /// <summary>
        /// This test tests if each template file has a meta-xml file
        /// Experimental folder is excluded
        /// </summary>
        [TestMethod]
        public void TemplateXMLTestMethod()
        {
            string[] allFiles = Directory.GetFiles("Templates", "*.*", SearchOption.AllDirectories);

            HashSet<string> templates = new HashSet<string>();
            HashSet<string> xmls = new HashSet<string>();
            foreach (string file in allFiles)
            {                
                if (file.ToLower().EndsWith("dir.xml"))
                {
                    //we don't need dir.xml files
                    continue;
                }
                if (file.ToLower().EndsWith("cwm"))
                {
                    templates.Add(file);
                }
                if (file.ToLower().EndsWith("xml"))
                {
                    xmls.Add(file);
                }                
            }

            foreach (string template in templates)
            {
                string xmlname = template.Substring(0, template.Length - 3) + "xml";
                if (!xmls.Contains(xmlname))
                {
                    Assert.Fail(string.Format("Template {0} has no xml file!", template));
                }
            }
        }
    }
}