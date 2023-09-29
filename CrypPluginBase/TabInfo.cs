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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace CrypTool.PluginBase
{
    public class ImageHelper
    {
        public static ImageSource LoadImage(Uri file)
        {
            Image i = new Image
            {
                Source = new BitmapImage(file)
            };
            return i.Source;
        }
    }

    [Serializable()]
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.PluginBase.Properties.Resources")]
    public class TabInfo : IDeserializationCallback
    {
        [NonSerialized]
        private Span tooltip;
        public Span Tooltip { get => tooltip; set => tooltip = value; }

        [NonSerialized]
        private ImageSource icon;
        public ImageSource Icon { get => icon; set => icon = value; }

        private string title;
        public string Title { get => title; set => title = value; }

        private string copyText;
        public string CopyText { get => copyText; set => copyText = value; }

        private FileInfo filename;
        public FileInfo Filename
        {
            get => filename;
            set
            {
                filename = value;
                TabInfo info = GenerateTabInfo(value);
                Title = info.Title;
                Icon = info.Icon;
                Tooltip = info.Tooltip;
            }
        }

        public TabInfo GenerateTabInfo(FileInfo file)
        {
            bool component = (file.Extension.ToLower() == ".component");
            string title = null;
            Span summary = new Span();
            string iconFile = null;
            string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
            if (File.Exists(xmlFile))
            {
                try
                {
                    XElement xml = XElement.Load(xmlFile);

                    XElement titleElement = XMLHelper.GetGlobalizedElementFromXML(xml, "title");
                    XElement summaryElement = XMLHelper.GetGlobalizedElementFromXML(xml, "summary");
                    XElement descriptionElement = XMLHelper.GetGlobalizedElementFromXML(xml, "description");

                    if (titleElement != null)
                    {
                        title = titleElement.Value;
                        if (title != null && title.Trim().Length > 0)
                        {
                            summary.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(titleElement)));
                        }
                    }

                    if (summaryElement != null)
                    {
                        if (summary.Inlines.Count > 0)
                        {
                            summary.Inlines.Add(new LineBreak());
                            summary.Inlines.Add(new LineBreak());
                        }
                        summary.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));
                    }

                    if (descriptionElement != null && descriptionElement.Value.Length > 1)
                    {
                        if (summary.Inlines.Count > 0)
                        {
                            summary.Inlines.Add(new LineBreak());
                            summary.Inlines.Add(new LineBreak());
                        }
                        summary.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                    }

                    if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                    {
                        iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                    }

                    CopyText = (titleElement != null ? (titleElement.Value + Environment.NewLine + Environment.NewLine) : "") +
                        (summaryElement != null ? (summaryElement.Value + Environment.NewLine + Environment.NewLine) : "") +
                        (descriptionElement != null ? XMLHelper.ConvertFormattedXElementToString(descriptionElement) : "");
                }
                catch (Exception)
                {
                    //we do nothing if the loading of an description xml fails => this is not a hard error
                }
            }

            if ((title == null) || (title.Trim() == ""))
            {
                title = component ? file.Name : Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
            }

            if (summary.Inlines.Count == 0)
            {
                string desc = component ? Properties.Resources.This_is_a_standalone_component_ : Properties.Resources.This_is_a_WorkspaceManager_file_;
                summary.Inlines.Add(new Run(desc));
            }

            if (iconFile == null || !File.Exists(iconFile))
            {
                iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
            }

            ImageSource image = null;
            if (File.Exists(iconFile))
            {
                try
                {
                    image = ImageHelper.LoadImage(new Uri(iconFile));
                }
                catch (Exception)
                {
                    image = null;
                }
            }
            else
            {
                string ext = file.Extension.Remove(0, 1);
                if (!component && ComponentInformations.EditorExtension.ContainsKey(ext))
                {
                    Type editorType = ComponentInformations.EditorExtension[ext];
                    image = editorType.GetImage(0).Source;
                }
            }

            return new TabInfo() { Tooltip = summary, Title = title, Icon = image, filename = file };
        }

        public void OnDeserialization(object sender)
        {
            if (filename == null)
            {
                return;
            }

            TabInfo info = GenerateTabInfo(filename);
            Icon = info.Icon;
            Title = info.Title;
            Tooltip = info.Tooltip;
        }
    }
}
