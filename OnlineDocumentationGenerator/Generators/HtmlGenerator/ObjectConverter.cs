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
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WorkspaceManager.Model;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    /// <summary>
    /// Class for converting an object to an html representation.
    /// Also takes care of referenced image resources while converting.
    /// </summary>
    internal class ObjectConverter
    {
        private readonly List<EntityDocumentationPage> _docPages;
        private readonly string _outputDir;
        private readonly HashSet<string> _createdImages = new HashSet<string>();

        public ObjectConverter(List<EntityDocumentationPage> docPages, string outputDir)
        {
            _docPages = docPages;
            _outputDir = outputDir;
        }

        public string Convert(object theObject, EntityDocumentationPage docPage)
        {
            if (theObject == null)
            {
                return Resources.Not_available;
            }

            if (theObject is XElement)
            {
                string elementString = ConvertXElement((XElement)theObject, docPage);
                if (string.IsNullOrWhiteSpace(elementString))
                {
                    return Convert(null, docPage);
                }
                return elementString;
            }
            if (theObject is BitmapFrame)
            {
                return string.Format("<img src=\"{0}\" />", ConvertImageSource((BitmapFrame)theObject, docPage.Name, docPage));
            }
            if (theObject is ComponentTemplateList)
            {
                return ConvertTemplateList((ComponentTemplateList)theObject, docPage);
            }
            if (theObject is Reference.ReferenceList)
            {
                return ((Reference.ReferenceList)theObject).ToHTML(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
            if (theObject is PropertyInfoAttribute[])
            {
                return ConvertConnectorList((PropertyInfoAttribute[])theObject);
            }
            if (theObject is TaskPaneAttribute[])
            {
                return ConvertSettingsList((TaskPaneAttribute[])theObject);
            }

            return theObject.ToString();
        }

        private string ConvertSettingsList(TaskPaneAttribute[] settings)
        {
            if ((settings != null) && (settings.Length > 0))
            {
                StringBuilder codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateSettingsListCode_Type));

                foreach (TaskPaneAttribute setting in settings)
                {
                    codeBuilder.AppendLine(string.Format("<tr> <td>{0}</td> <td>{1}</td> <td>{2}</td> </tr>",
                                                         HttpUtility.HtmlEncode(setting.Caption), HttpUtility.HtmlEncode(setting.ToolTip),
                                                         GetControlTypeString(setting.ControlType)));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string GetControlTypeString(ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.TextBox:
                    return Resources.Text_box;
                case ControlType.ComboBox:
                    return Resources.Combo_box;
                case ControlType.RadioButton:
                    return Resources.Radio_button;
                case ControlType.CheckBox:
                    return Resources.Check_box;
                case ControlType.OpenFileDialog:
                    return Resources.Open_file_dialog;
                case ControlType.SaveFileDialog:
                    return Resources.Save_file_dialog;
                case ControlType.NumericUpDown:
                    return Resources.Numeric_up_down;
                case ControlType.Button:
                    return Resources.Button;
                case ControlType.Slider:
                    return Resources.Slider;
                case ControlType.TextBoxReadOnly:
                    return Resources.Text_box__read_only_;
                case ControlType.DynamicComboBox:
                    return Resources.Dynamic_combo_box;
                case ControlType.TextBoxHidden:
                    return Resources.Text_box__hidden_;
                case ControlType.KeyTextBox:
                    return Resources.Key_text_box;
                case ControlType.LanguageSelector:
                    return Resources.LanguageSelector;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("ControlType \"{0}\" is unknown. Please add it to the \"GetControlTypeString(ControlType controlType)\"-method in OnlineDocumentationGenerator.Generators.HtmlGenerator.ObjectConverter.cs", controlType.ToString()));
            }
        }

        private string ConvertConnectorList(PropertyInfoAttribute[] connectors)
        {
            if ((connectors != null) && (connectors.Length > 0))
            {
                StringBuilder codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> <th>{3}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Direction,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Type));

                foreach (PropertyInfoAttribute connector in connectors)
                {
                    string type = connector.PropertyInfo.PropertyType.Name;
                    System.Windows.Media.Color color = ColorHelper.GetLineColor(connector.PropertyInfo.PropertyType);
                    codeBuilder.AppendLine(
                        string.Format("<tr> <td bgcolor=\"#{0}{1}{2}\">{3}</td> <td bgcolor=\"#{0}{1}{2}\">{4}</td> <td bgcolor=\"#{0}{1}{2}\" nowrap>{5}</td> <td bgcolor=\"#{0}{1}{2}\">{6}</td> </tr>", color.R.ToString("x"), color.G.ToString("x"), color.B.ToString("x"),
                                      HttpUtility.HtmlEncode(connector.Caption),
                                      HttpUtility.HtmlEncode(connector.ToolTip),
                                      GetDirectionString(connector.Direction),
                                      type));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string GetDirectionString(Direction direction)
        {
            switch (direction)
            {
                case Direction.InputData:
                    return string.Format("◄ {0}", Resources.Input_data);
                case Direction.OutputData:
                    return string.Format("► {0}", Resources.Output_data);
                case Direction.ControlSlave:
                    return string.Format("▲ {0}", Resources.Control_slave);
                case Direction.ControlMaster:
                    return string.Format("▼ {0}", Resources.Control_master);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown direction \"{0}\" in GetDirectionString method in OnlineDocumentationGenerator.Generators.HtmlGenerator.ObjectConverter.cs", direction.ToString()));
            }
        }

        private string ConvertTemplateList(ComponentTemplateList componentTemplateList, EntityDocumentationPage entityDocumentationPage)
        {
            if (componentTemplateList.Templates.Count == 0)
            {
                return Resources.NoContent;
            }

            StringBuilder codeBuilder = new StringBuilder();
            codeBuilder.AppendLine(string.Format("<p>{0}</p>", Resources.Templates_description));
            codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>",
                Resources.File, Resources.Description));

            foreach (TemplateDocumentationPage template in componentTemplateList.Templates)
            {
                //var link = Path.Combine(Path.Combine("..\\..", DocGenerator.TemplateDirectory), template.TemplateFile);
                string link = template.CurrentLocalization.FilePath;
                codeBuilder.AppendLine(string.Format("<tr> <td><a href=\"..\\{0}\">{1}</a></td> <td>{2}</td> </tr>",
                    link, template.CurrentLocalization.Name, ConvertXElement(template.CurrentLocalization.Summary, entityDocumentationPage)));
            }

            codeBuilder.AppendLine("</table>");

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Converts the given imageSource parameter to a file in the doc directory and returns an html string referencing this.
        /// </summary>
        /// <param name="imageSource">The ImageSource containing the image to convert.</param>
        /// <param name="filename">The wished filename (withouth the extension)</param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private string ConvertImageSource(BitmapFrame imageSource, string filename, EntityDocumentationPage entityDocumentationPage)
        {
            filename += ".png";
            if (!_createdImages.Contains(filename))
            {
                string dir = Path.Combine(Path.Combine(_outputDir, OnlineHelp.HelpDirectory), entityDocumentationPage.DocDirPath);
                //create image file:
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string file = Path.Combine(dir, filename);
                using (FileStream fileStream = new FileStream(file, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(imageSource);
                    encoder.Save(fileStream);
                }
            }
            _createdImages.Add(filename);
            return filename;
        }

        /// <summary>
        /// Converts the given xelement, which is from the xml doc file, into an html formated representation.
        /// </summary>
        /// <param name="xelement"></param>
        /// <param name="entityDocumentationPage"></param>
        /// <returns></returns>
        private string ConvertXElement(XElement xelement, EntityDocumentationPage entityDocumentationPage)
        {
            StringBuilder result = new StringBuilder();
            int sectionCounter = 0;
            int subsectionCounter = 0;
            int subsubsectionCounter = 0;

            foreach (XNode node in xelement.Nodes())
            {
                if (node is XText)
                {
                    result.Append(HttpUtility.HtmlEncode(((XText)node).Value));
                }
                else if (node is XElement)
                {
                    string nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            {
                                string nodeRep = ConvertXElement((XElement)node, entityDocumentationPage);
                                result.Append(string.Format("<{0}>{1}</{0}>", nodeName, nodeRep));
                            }
                            break;
                        case "font":
                            {
                                string nodeRep = ConvertXElement((XElement)node, entityDocumentationPage);
                                XAttribute colorAtt = ((XElement)node).Attribute("color");
                                XAttribute backgroundAtt = ((XElement)node).Attribute("background");
                                result.Append(string.Format("<span style=\"color:{0};background-color:{1}\">{2}</span>", (colorAtt == null ? "black" : colorAtt.Value), (backgroundAtt == null ? "white" : backgroundAtt.Value), nodeRep));
                            }
                            break;
                        case "ref":
                            XAttribute idAtt = ((XElement)node).Attribute("id");
                            if (idAtt != null)
                            {
                                if (entityDocumentationPage is PluginDocumentationPage || entityDocumentationPage is CommonDocumentationPage)
                                {
                                    string htmlLinkToRef = entityDocumentationPage.References.GetHTMLinkToRef(idAtt.Value);
                                    if (htmlLinkToRef != null)
                                    {
                                        result.Append(htmlLinkToRef);
                                    }
                                }
                            }
                            break;
                        case "img":
                            XAttribute srcAtt = ((XElement)node).Attribute("src");
                            if (srcAtt != null)
                            {
                                string srcname = srcAtt.Value.Replace('\\', '/');
                                int sIndex = srcname.IndexOf('/');
                                BitmapFrame image = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}", srcname.Substring(0, sIndex), srcname.Substring(sIndex + 1))));
                                string filename = string.Format("{0}_{1}", entityDocumentationPage.Name, Path.GetFileNameWithoutExtension(srcname));
                                filename = ConvertImageSource(image, filename, entityDocumentationPage);
                                ((XElement)node).Attribute("src").SetValue(filename);
                                string img = node.ToString();
                                XAttribute caption = ((XElement)node).Attribute("caption");
                                if (caption != null)
                                {
                                    img = string.Format("<table><caption align=\"bottom\">{0}</caption><tr><td>{1}</td></tr></table>", caption.Value, img);
                                }

                                result.Append(img);
                            }
                            break;
                        case "newline":
                            result.Append("<br/>");
                            break;
                        case "enum":
                        case "list":
                            string t = (nodeName == "enum") ? "ol" : "ul";
                            result.AppendLine(string.Format("<{0}>", t));
                            foreach (XElement item in ((XElement)node).Elements("item"))
                            {
                                result.AppendLine(string.Format("<li>{0}</li>", ConvertXElement(item, entityDocumentationPage)));
                            }
                            result.AppendLine(string.Format("</{0}>", t));
                            break;
                        case "table":
                            XAttribute borderAtt = ((XElement)node).Attribute("border");
                            if (borderAtt != null)
                            {
                                if (int.TryParse(borderAtt.Value, out int border))
                                {
                                    result.Append(ConvertTable((XElement)node, border));
                                    continue;
                                }
                            }
                            result.Append(ConvertTable((XElement)node, null));
                            break;
                        case "external":
                            XAttribute reference = ((XElement)node).Attribute("ref");
                            if (reference != null)
                            {
                                string linkText = ConvertXElement((XElement)node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference.Value;
                                }
                                result.Append(string.Format("<a href=\"{0}?external\"><img src=\"../external_link.png\" border=\"0\">{1}</a>", reference.Value, linkText));
                            }
                            break;
                        case "internal":
                            XAttribute reference_internal = ((XElement)node).Attribute("ref");
                            if (reference_internal != null)
                            {
                                string linkText = ConvertXElement((XElement)node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference_internal.Value;
                                }
                                result.Append(string.Format("<a href=\"{0}\">{1}</a>", reference_internal.Value, linkText));
                            }
                            break;
                        case "docRef":
                            XAttribute itemAttribute = ((XElement)node).Attribute("item");
                            if (itemAttribute != null)
                            {
                                string linkText = ConvertXElement((XElement)node, entityDocumentationPage);
                                EntityDocumentationPage docPage = GetEntityDocPage(itemAttribute.Value);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = GetEntityName(docPage);
                                }

                                int dirLevel = entityDocumentationPage.DocDirPath.Split(Path.PathSeparator).Length;
                                string d = "";
                                for (int i = 0; i < dirLevel; i++)
                                {
                                    d += Path.Combine(d, "..");
                                }
                                string entityLink = GetEntityLink(docPage);
                                if (entityLink != null)
                                {
                                    result.Append(string.Format("<a href=\"{0}\">{1}</a>", Path.Combine(d, entityLink), linkText));
                                }
                                else
                                {
                                    result.Append(string.Format("<b><font color=\"red\">{0}</font></b>", linkText));
                                }
                            }
                            break;

                        case "section":
                            {
                                XAttribute headline = ((XElement)node).Attribute("headline");
                                if (headline != null)
                                {
                                    sectionCounter++;
                                    subsectionCounter = 0;
                                    subsubsectionCounter = 0;
                                    result.AppendLine(string.Format("<h2>{0}. {1}</h2>", sectionCounter, headline.Value));
                                    result.AppendLine(ConvertXElement((XElement)node, entityDocumentationPage));
                                }
                            }
                            break;

                        case "subsection":
                            {
                                XAttribute headline = ((XElement)node).Attribute("headline");
                                if (headline != null)
                                {
                                    subsectionCounter++;
                                    subsubsectionCounter = 0;
                                    result.AppendLine(string.Format("<h2>{0}.{1}. {2}</h2>", sectionCounter, subsectionCounter, headline.Value));
                                    result.AppendLine(ConvertXElement((XElement)node, entityDocumentationPage));
                                }
                            }
                            break;

                        case "subsubsection":
                            {
                                XAttribute headline = ((XElement)node).Attribute("headline");
                                if (headline != null)
                                {
                                    subsubsectionCounter++;
                                    subsubsectionCounter = 0;
                                    result.AppendLine(string.Format("<h2>{0}.{1}.{2}. {3}</h2>", sectionCounter, subsectionCounter, headline.Value));
                                    result.AppendLine(ConvertXElement((XElement)node, entityDocumentationPage));
                                }
                            }
                            break;

                        default:
                            continue;
                    }
                }
            }

            return result.ToString();
        }

        private string ConvertTable(XElement node, int? border)
        {
            StringBuilder sb = new StringBuilder("<table");
            if (border.HasValue)
            {
                sb.Append(string.Format(" border=\"{0}\"", border.Value));
            }
            sb.AppendLine(">");

            foreach (XElement row in node.Elements("tr"))
            {
                sb.AppendLine("<tr>");
                foreach (XElement data in row.Elements("td"))
                {
                    sb.AppendLine(string.Format("<td>{0}</td>", data.Value));
                }
                foreach (XElement header in row.Elements("th"))
                {
                    sb.AppendLine(string.Format("<th>{0}</th>", header.Value));
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private EntityDocumentationPage GetEntityDocPage(string entity)
        {
            foreach (EntityDocumentationPage docPage in _docPages)
            {
                if (docPage.Name == entity)
                {
                    return docPage;
                }
            }
            return null;
        }

        private string GetEntityLink(EntityDocumentationPage docPage)
        {
            if (docPage == null)
            {
                return null;
            }
            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (docPage.AvailableLanguages.Contains(lang))
            {
                return docPage.Localizations[lang].FilePath;
            }
            else if (docPage.AvailableLanguages.Contains("en"))
            {
                return docPage.Localizations["en"].FilePath;
            }
            return null;
        }

        private string GetEntityName(EntityDocumentationPage docPage)
        {
            if (docPage == null)
            {
                return "unknown entity";
            }
            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (docPage.AvailableLanguages.Contains(lang))
            {
                return docPage.Localizations[lang].Name;
            }
            else if (docPage.AvailableLanguages.Contains("en"))
            {
                return docPage.Localizations["en"].Name;
            }
            return "unknown entity";
        }
    }
}
