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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    /// <summary>
    /// Class for converting an object to LaTeX representation.
    /// </summary>
    internal class ObjectConverter
    {
        private const string TemplateImagesDir = "TemplateImages";
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
                return ConvertImageSource((BitmapFrame)theObject, docPage.Name, docPage.CurrentLocalization.Name);
            }
            if (theObject is ComponentTemplateList)
            {
                return ConvertTemplateList((ComponentTemplateList)theObject, docPage);
            }
            if (theObject is Reference.ReferenceList)
            {
                return ((Reference.ReferenceList)theObject).ToLaTeX(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
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

                codeBuilder.AppendLine(@"\begin{tabular}{ | p{5cm} | p{7cm} | l | }");
                codeBuilder.AppendLine(@"\hline");
                codeBuilder.AppendLine(string.Format(@" {0} & {1} & {2} \\ \hline\hline",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Name) + "}",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Description) + "}",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateSettingsListCode_Type) + "}"));

                foreach (TaskPaneAttribute setting in settings)
                {
                    // In LaTeX, the first word of a paragraph is not hyphenated.
                    // Thus the \hspace{0} is necessary for the hyphenation rules to take effect on the first word of a table cell (which is also a paragraph).
                    codeBuilder.AppendLine(
                        string.Format(@" {0} & {1} & {2} \\ \hline",
                                      @"\hspace{0pt}" + Helper.EscapeLaTeX(setting.Caption),
                                      @"\hspace{0pt}" + Helper.EscapeLaTeX(setting.ToolTip),
                                      @"\hspace{0pt}" + GetControlTypeString(setting.ControlType)));
                }

                codeBuilder.AppendLine(@"\end{tabular}");

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
                    throw new ArgumentOutOfRangeException(string.Format("ControlType \"{0}\" is unknown. Please add it to the \"GetControlTypeString(ControlType controlType)\"-method in OnlineDocumentationGenerator.Generators.LaTeXGenerator.ObjectConverter.cs", controlType.ToString()));

            }
        }

        private string ConvertConnectorList(PropertyInfoAttribute[] connectors)
        {
            if ((connectors != null) && (connectors.Length > 0))
            {
                StringBuilder codeBuilder = new StringBuilder();

                codeBuilder.AppendLine(@"\begin{tabular}{ | p{3cm} | p{6cm} | l | l | }");
                codeBuilder.AppendLine(@"\hline");
                codeBuilder.AppendLine(string.Format(@" {0} & {1} & {2} & {3} \\ \hline\hline",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Name) + "}",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Description) + "}",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Direction) + "}",
                                                     @"\textbf{" + Helper.EscapeLaTeX(Resources.HtmlGenerator_GenerateConnectorListCode_Type) + "}"));

                foreach (PropertyInfoAttribute connector in connectors)
                {
                    codeBuilder.AppendLine(
                        string.Format(@" {0} & {1} & {2} & {3} \\ \hline",
                                      @"\hspace{0pt}" + Helper.EscapeLaTeX(connector.Caption),
                                      @"\hspace{0pt}" + Helper.EscapeLaTeX(connector.ToolTip),
                                      @"\hspace{0pt}" + GetDirectionString(connector.Direction),
                                      @"\hspace{0pt}" + Helper.EscapeLaTeX(connector.PropertyInfo.PropertyType.Name)));
                }

                codeBuilder.AppendLine(@"\end{tabular}");

                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string GetDirectionString(Direction direction)
        {
            switch (direction)
            {
                case Direction.InputData:
                    return string.Format(@"$\blacktriangleleft$ {0}", Resources.Input_data);
                case Direction.OutputData:
                    return string.Format(@"$\blacktriangleright$ {0}", Resources.Output_data);
                case Direction.ControlSlave:
                    return string.Format(@"$\blacktriangle$ {0}", Resources.Control_slave);
                case Direction.ControlMaster:
                    return string.Format(@"$\blacktriangledown$ {0}", Resources.Control_master);
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown direction \"{0}\" in GetDirectionString method in OnlineDocumentationGenerator.Generators.LaTeXGenerator.ObjectConverter.cs", direction.ToString()));
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
                    link, template.CurrentLocalization.Name, ConvertXElement(template.CurrentLocalization.Description, entityDocumentationPage)));
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
        private string ConvertImageSource(BitmapFrame imageSource, string filename, string caption)
        {
            string imagePath = GetImagePath(imageSource, filename);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"\begin{figure}[!ht]");
            sb.AppendLine(@"\begin{center}");
            //sb.AppendLine("@\includegraphics[width=32pt, height=32pt]{" + imagePath + "}");
            sb.AppendLine(@"\includegraphics[max height=5cm,max width=\textwidth]{" + imagePath + "}");
            sb.AppendLine(@"\end{center}");
            sb.AppendLine(@"\caption{" + Helper.EscapeLaTeX(caption) + "}");
            sb.AppendLine(@"\end{figure}");
            return sb.ToString();
        }

        internal string GetImagePath(BitmapFrame imageSource, string filename)
        {
            filename = filename.Replace(".", "-").Replace(" ", "_");
            filename = filename + ".png";
            if (!_createdImages.Contains(filename))
            {
                string dir = Path.Combine(Path.Combine(_outputDir, LaTeXGenerator.HelpDirectory), TemplateImagesDir);
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
            string imagePath = TemplateImagesDir + "/" + filename;
            _createdImages.Add(filename);
            return imagePath;
        }

        /// <summary>
        /// Converts the given xelement, which is from the xml doc file, into a LaTeX formatted representation.
        /// </summary>
        /// <param name="xelement"></param>
        /// <param name="entityDocumentationPage"></param>
        /// <returns></returns>
        private string ConvertXElement(XElement xelement, EntityDocumentationPage entityDocumentationPage)
        {
            StringBuilder result = new StringBuilder();
            foreach (XNode node in xelement.Nodes())
            {
                if (node is XText)
                {
                    string text = ((XText)node).Value;
                    text = Regex.Replace(text, "[\r\n]+", "\n");
                    text = Regex.Replace(text, "[\t ]+\n", "\n");
                    text = Regex.Replace(text, "[\t ]+\\\\\n", "\\\\\n");
                    result.Append(Helper.EscapeLaTeX(text));
                }
                else if (node is XElement)
                {
                    string nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            Dictionary<string, string> fontDict = new Dictionary<string, string> { { "b", "\\textbf" }, { "i", "\\textit" }, { "u", "\\underline" } };
                            string nodeRep = ConvertXElement((XElement)node, entityDocumentationPage);
                            result.Append(fontDict[nodeName] + "{" + nodeRep + "}");
                            break;
                        case "ref":
                            XAttribute idAtt = ((XElement)node).Attribute("id");
                            if (idAtt != null)
                            {
                                if (entityDocumentationPage is PluginDocumentationPage)
                                {
                                    result.Append("TODO: REF HERE");
                                    //var htmlLinkToRef = entityDocumentationPage.References.GetHTMLinkToRef(idAtt.Value);
                                    //if (htmlLinkToRef != null)
                                    //{
                                    //    result.Append(htmlLinkToRef);
                                    //}
                                }
                            }
                            break;
                        case "img":
                            XAttribute srcAtt = ((XElement)node).Attribute("src");
                            if (srcAtt != null)
                            {
                                int sIndex = srcAtt.Value.IndexOf('/');
                                BitmapFrame image = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                    srcAtt.Value.Substring(0, sIndex), srcAtt.Value.Substring(sIndex + 1))));
                                string filename = string.Format("{0}_{1}", entityDocumentationPage.Name, Path.GetFileNameWithoutExtension(srcAtt.Value));
                                XAttribute captionAtt = ((XElement)node).Attribute("caption");
                                string caption = (captionAtt != null) ? captionAtt.Value : Path.GetFileNameWithoutExtension(srcAtt.Value);
                                result.Append(ConvertImageSource(image, filename, caption));
                            }
                            break;
                        case "newline":
                            result.Append("\\\\\n");
                            break;
                        case "section":
                            XAttribute headline = ((XElement)node).Attribute("headline");
                            if (headline != null)
                            {
                                result.AppendLine("\\subsubsection*{" + headline.Value + "}");
                                result.AppendLine(ConvertXElement((XElement)node, entityDocumentationPage));
                            }
                            break;
                        case "enum":
                        case "list":
                            string t = (nodeName == "enum") ? "enumerate" : "itemize";
                            result.AppendLine("\\begin{" + t + "}");
                            foreach (XElement item in ((XElement)node).Elements("item"))
                            {
                                result.AppendLine(string.Format("\\item {0}", ConvertXElement(item, entityDocumentationPage)));
                            }
                            result.AppendLine("\\end{" + t + "}");
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
                                result.Append(Helper.EscapeLaTeX(linkText));
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
                                    if (docPage != null)
                                    {
                                        linkText = GetEntityName(docPage);
                                    }
                                    else
                                    {
                                        linkText = itemAttribute.Value;
                                    }
                                }
                                result.Append(linkText);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            return result.ToString();
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

        private string GetEntityName(EntityDocumentationPage docPage)
        {
            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (docPage.AvailableLanguages.Contains(lang))
            {
                return docPage.Localizations[lang].Name;
            }
            else
            {
                return docPage.Localizations["en"].Name;
            }
        }
    }
}
