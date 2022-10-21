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
using CrypTool.PluginBase.Editor;
using System;
using System.IO;
using System.Linq;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class OnlineHelp
    {
        public static readonly string HelpDirectory = "OnlineDocumentation";
        public static readonly string RelativeComponentDocDirectory = "Components";
        public static readonly string RelativeEditorDocDirectory = "Editors";
        public static readonly string RelativeTemplateDocDirectory = "Templates";
        public static readonly string RelativeCommonDocDirectory = "Common";

        public struct TemplateType
        {
            public string RelativeTemplateFilePath;
            public TemplateType(string relativeTemplateFilePath)
            {
                RelativeTemplateFilePath = relativeTemplateFilePath;
            }
        }

        public delegate void ShowDocPageHandler(object docEntity);
        public static event ShowDocPageHandler ShowDocPage;

        public static void InvokeShowDocPage(object docEntity)
        {
            if (ShowDocPage != null)
            {
                ShowDocPage(docEntity);
            }
        }

        public static string GetPluginDocFilename(Type plugin, string lang)
        {
            string filename = string.Format("{0}_{1}.html", plugin.FullName, lang);
            if (plugin.GetInterfaces().Contains(typeof(IEditor)))
            {
                return Path.Combine(RelativeEditorDocDirectory, filename);
            }
            else
            {
                return Path.Combine(RelativeComponentDocDirectory, filename);
            }
        }

        public static string GetTemplateDocFilename(string relativTemplateFilePath, string lang)
        {
            string flattenedPath = Path.GetDirectoryName(relativTemplateFilePath).Replace(Path.DirectorySeparatorChar, '.');
            string filename = string.Format("{0}.{1}_{2}.html", flattenedPath, Path.GetFileNameWithoutExtension(relativTemplateFilePath), lang);
            return Path.Combine(RelativeTemplateDocDirectory, filename);
        }

        public static string GetCommonDocFilename(string name, string lang)
        {
            string file = string.Format("{0}_{1}.html", name, lang);
            return Path.Combine(RelativeCommonDocDirectory, file);
        }

        public static string GetComponentIndexFilename(string lang)
        {
            return string.Format("index_{0}.html", lang);
        }

        public static string GetEditorIndexFilename(string lang)
        {
            return string.Format("editors_{0}.html", lang);
        }

        public static string GetTemplatesIndexFilename(string lang)
        {
            return string.Format("templates_{0}.html", lang);
        }

        public static string GetCommonIndexFilename(string lang)
        {
            return string.Format("common_{0}.html", lang);
        }
    }
}
