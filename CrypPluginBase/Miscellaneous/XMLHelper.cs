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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Xml.Linq;

namespace CrypTool.PluginBase.Miscellaneous
{
    public static class XMLHelper
    {
        public static XElement GetGlobalizedElementFromXML(XElement xml, string element)
        {
            CultureInfo currentLang = CultureInfo.CurrentUICulture;

            IEnumerable<XElement> allElements = xml.Elements(element);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                    {
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == "en" select descln;
                    }
                }
            }

            if (foundElements == null || !foundElements.Any())
            {
                if (xml.Element(element) != null)
                {
                    return xml.Element(element);
                }
                else
                {
                    return null;
                }
            }

            return foundElements.First();
        }

        public static Inline ConvertFormattedXElement(XElement xelement, bool isNewLine = true)
        {
            Span span = new Span();

            foreach (XNode node in xelement.Nodes())
            {
                if (node is XText)
                {
                    string line = ((XText)node).Value;
                    line = new Regex(@"\s*[\r\n]+\s*").Replace(line, " ");
                    if (isNewLine)
                    {
                        line = line.TrimStart();
                    }

                    isNewLine = false;
                    span.Inlines.Add(new Run(line));
                }
                else if (node is XElement)
                {
                    isNewLine = false;
                    string nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                            Inline nodeRep = ConvertFormattedXElement((XElement)node, isNewLine);
                            span.Inlines.Add(new Bold(nodeRep));
                            break;
                        case "i":
                            nodeRep = ConvertFormattedXElement((XElement)node, isNewLine);
                            span.Inlines.Add(new Italic(nodeRep));
                            break;
                        case "u":
                            nodeRep = ConvertFormattedXElement((XElement)node, isNewLine);
                            span.Inlines.Add(new Underline(nodeRep));
                            break;
                        case "newline":
                            span.Inlines.Add(new LineBreak());
                            isNewLine = true;
                            break;
                        case "external":
                            XAttribute reference = ((XElement)node).Attribute("ref");
                            if (reference != null)
                            {
                                Inline linkText = ConvertFormattedXElement((XElement)node, isNewLine);
                                if (linkText == null)
                                {
                                    linkText = new Run(reference.Value);
                                }
                                Hyperlink link = new Hyperlink(linkText)
                                {
                                    NavigateUri = new Uri(reference.Value)
                                };
                                span.Inlines.Add(link);
                            }
                            break;
                        case "pluginRef":
                            XAttribute plugin = ((XElement)node).Attribute("plugin");
                            if (plugin != null)
                            {
                                Inline linkText = ConvertFormattedXElement((XElement)node, isNewLine);
                                if (linkText == null)
                                {
                                    linkText = new Run(plugin.Value);
                                }
                                span.Inlines.Add(linkText);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            if (span.Inlines.Count == 0)
            {
                return null;
            }

            if (span.Inlines.Count == 1)
            {
                return span.Inlines.First();
            }

            return span;
        }

        public static string ConvertFormattedXElementToString(XElement xelement, bool isNewLine = true)
        {
            string result = "";

            foreach (XNode node in xelement.Nodes())
            {
                if (node is XText)
                {
                    string line = ((XText)node).Value;
                    line = new Regex(@"\s*[\r\n]+\s*").Replace(line, " ");
                    if (isNewLine)
                    {
                        line = line.TrimStart();
                    }

                    isNewLine = false;
                    result += line;
                }
                else if (node is XElement)
                {
                    isNewLine = false;
                    string nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            string nodeRep = ConvertFormattedXElementToString((XElement)node, isNewLine);
                            result += nodeRep;
                            break;
                        case "newline":
                            result += Environment.NewLine;
                            isNewLine = true;
                            break;
                        case "external":
                            XAttribute reference = ((XElement)node).Attribute("ref");
                            if (reference != null)
                            {
                                string linkText = ConvertFormattedXElementToString((XElement)node, isNewLine);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference.Value;
                                }

                                result += linkText;
                            }
                            break;
                        case "pluginRef":
                            XAttribute plugin = ((XElement)node).Attribute("plugin");
                            if (plugin != null)
                            {
                                string linkText = ConvertFormattedXElementToString((XElement)node, isNewLine);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = plugin.Value;
                                }

                                result += linkText;
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            return result;
        }
    }
}
