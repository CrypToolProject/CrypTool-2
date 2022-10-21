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
using OnlineDocumentationGenerator.DocInformations.Localization;
using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class EditorDocumentationPage : PluginDocumentationPage
    {
        public EditorDocumentationPage(Type pluginType, XElement xml) : base(pluginType, xml)
        {
        }

        protected override LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type editorType, XElement xml, string lang, BitmapFrame editorImage)
        {
            if (pluginDocumentationPage is EditorDocumentationPage)
            {
                return new LocalizedEditorDocumentationPage((EditorDocumentationPage)pluginDocumentationPage,
                                                               editorType, xml, lang, editorImage);
            }
            return null;
        }
    }
}
