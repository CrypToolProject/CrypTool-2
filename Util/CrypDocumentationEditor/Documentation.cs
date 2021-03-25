/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Documents;
using System.Windows;

namespace CrypDocumentationEditor
{
    public class Documentation
    {
        private XmlDocument xml = null;
        public const string DEFAULT_LANGUAGE = "en";

        public Documentation()
        {
            try
            {
                Language = DEFAULT_LANGUAGE;
                xml = new XmlDocument();

                XmlDeclaration declaration = (XmlDeclaration)xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", null);
                declaration.Encoding = "utf-8";
                XmlNode documentationNode = xml.CreateNode(XmlNodeType.Element, "documentation", null);
                XmlNode introductionNode = xml.CreateNode(XmlNodeType.Element, "introduction", null);
                XmlNode usageNode = xml.CreateNode(XmlNodeType.Element, "usage", null);
                XmlNode presentationNode = xml.CreateNode(XmlNodeType.Element, "presentation", null);
                XmlNode languageNode = xml.CreateNode(XmlNodeType.Element, "language", null);

                XmlAttribute languageNodeAttribute = xml.CreateAttribute("culture");
                languageNodeAttribute.Value = Language;                

                XmlAttribute introductionlangAttribute = xml.CreateAttribute("lang");
                introductionlangAttribute.Value = Language;
                XmlAttribute usagelangAttribute = xml.CreateAttribute("lang");
                usagelangAttribute.Value = Language;
                XmlAttribute presentationlangAttribute = xml.CreateAttribute("lang");
                presentationlangAttribute.Value = Language;

                xml.AppendChild(declaration);
                xml.AppendChild(documentationNode);
                documentationNode.AppendChild(languageNode);
                languageNode.Attributes.Append(languageNodeAttribute);
                documentationNode.AppendChild(introductionNode);
                introductionNode.Attributes.Append(introductionlangAttribute);
                documentationNode.AppendChild(usageNode);
                usageNode.Attributes.Append(usagelangAttribute);
                documentationNode.AppendChild(presentationNode);
                presentationNode.Attributes.Append(presentationlangAttribute);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string Language
        {
            get;
            set;
        }

        public void Load(string filename)
        {            
            xml.Load(filename);                    
        }

        public void Save(string filename)
        {
            xml.Save(filename);
        }

        public FlowDocument Introduction
        {
            get
            {
                XmlNode introduction = xml.SelectSingleNode("/documentation/introduction[@lang='"+ Language +"']");
                if (introduction == null)
                {
                    XmlNode documentationNode = xml.SelectSingleNode("/documentation");
                    introduction = xml.CreateNode(XmlNodeType.Element, "introduction", null);
                    XmlAttribute introductionlangAttribute = xml.CreateAttribute("lang");
                    introductionlangAttribute.Value = Language;
                    introduction.Attributes.Append(introductionlangAttribute);
                    documentationNode.AppendChild(introduction);
                }
                string text = introduction.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode introduction = xml.SelectSingleNode("/documentation/introduction[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                introduction.InnerXml = builder.ToString();
            }
        }

        public FlowDocument Usage
        {
            get
            {
                XmlNode usage = xml.SelectSingleNode("/documentation/usage[@lang='" + Language + "']");
                if (usage == null)
                {
                    XmlNode documentationNode = xml.SelectSingleNode("/documentation");
                    usage = xml.CreateNode(XmlNodeType.Element, "usage", null);
                    XmlAttribute usagelangAttribute = xml.CreateAttribute("lang");
                    usagelangAttribute.Value = Language;
                    usage.Attributes.Append(usagelangAttribute);
                    documentationNode.AppendChild(usage);
                }
                string text = usage.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode usage = xml.SelectSingleNode("/documentation/usage[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                usage.InnerXml = builder.ToString();
            }
        }

        public FlowDocument Presentation
        {
            get
            {
                XmlNode presentation = xml.SelectSingleNode("/documentation/presentation[@lang='" + Language + "']");
                if (presentation == null)
                {
                    XmlNode documentationNode = xml.SelectSingleNode("/documentation");
                    presentation = xml.CreateNode(XmlNodeType.Element, "presentation", null);
                    XmlAttribute presentationlangAttribute = xml.CreateAttribute("lang");
                    presentationlangAttribute.Value = Language;
                    presentation.Attributes.Append(presentationlangAttribute);
                    documentationNode.AppendChild(presentation);
                }
                string text = presentation.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode presentation = xml.SelectSingleNode("/documentation/presentation[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                presentation.InnerXml = builder.ToString();
            }
        }

        public string[] GetLanguages()
        {
            XmlNodeList nodes = xml.SelectNodes("/documentation/language");
            if (nodes.Count == 0)
            {
                return null;
            }
            string[] languages = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                languages[i] = nodes[i].Attributes["culture"].Value;
            }
            return languages;
        }

        public void AddLanguage(string language)
        {
            XmlNode languageNode = xml.CreateNode(XmlNodeType.Element, "language", null);
            XmlAttribute languageNodeAttribute = xml.CreateAttribute("culture");
            languageNodeAttribute.Value = language;
            languageNode.Attributes.Append(languageNodeAttribute);

            XmlNode documentationNode = xml.SelectSingleNode("/documentation");

            XmlNode introductionNode = xml.CreateNode(XmlNodeType.Element, "introduction", null);
            XmlNode usageNode = xml.CreateNode(XmlNodeType.Element, "usage", null);
            XmlNode presentationNode = xml.CreateNode(XmlNodeType.Element, "presentation", null);                        

            XmlAttribute introductionlangAttribute = xml.CreateAttribute("lang");
            introductionlangAttribute.Value = language;
            XmlAttribute usagelangAttribute = xml.CreateAttribute("lang");
            usagelangAttribute.Value = language;
            XmlAttribute presentationlangAttribute = xml.CreateAttribute("lang");
            presentationlangAttribute.Value = language;

            documentationNode.AppendChild(languageNode);            
            languageNode.Attributes.Append(languageNodeAttribute);
            documentationNode.AppendChild(introductionNode);
            introductionNode.Attributes.Append(introductionlangAttribute);
            documentationNode.AppendChild(usageNode);
            usageNode.Attributes.Append(usagelangAttribute);
            documentationNode.AppendChild(presentationNode);
            presentationNode.Attributes.Append(presentationlangAttribute);            
        }

        public List<Reference> GetReferences()
        {
            List<Reference> references = new List<Reference>();
            XmlNodeList linkReferenceNodes = xml.SelectNodes("/documentation/references/linkReference");
            foreach (XmlNode node in linkReferenceNodes)
            {
                references.Add(new LinkReference() { Link = node["link"].Attributes["url"].Value, Caption = node["caption"].InnerText });                
            }
            XmlNodeList bookReferenceNodes = xml.SelectNodes("/documentation/references/bookReference");
            foreach (XmlNode node in bookReferenceNodes)
            {
                references.Add(new BookReference() { Author = node["author"].InnerText, Publisher = node["publisher"].InnerText, Name = node["name"].InnerText });
            }
            return references;
        }

        public void AddReferences(List<Reference> references)
        {
            XmlNode referencesNode = xml.SelectSingleNode("/documentation/references");
            if (referencesNode == null)
            {
                referencesNode = xml.CreateNode(XmlNodeType.Element, "references", null);
                XmlNode documentationNode = xml.SelectSingleNode("/documentation");
                documentationNode.AppendChild(referencesNode);
            }
            else
            {
                referencesNode.RemoveAll();
            }

            foreach (Reference reference in references)
            {
                if (reference is LinkReference)
                {
                    LinkReference linkReference = (LinkReference)reference;
                    XmlNode linkReferenceNode = xml.CreateNode(XmlNodeType.Element, "linkReference", null);
                    XmlNode linkNode = xml.CreateNode(XmlNodeType.Element, "link", null);
                    XmlAttribute urlAttribute = xml.CreateAttribute("url", null);
                    XmlAttribute linkNodeAttribute = xml.CreateAttribute("lang");
                    XmlNode captionNode = xml.CreateNode(XmlNodeType.Element, "caption", null);
                    XmlAttribute captionNodeAttribute = xml.CreateAttribute("lang");
                    urlAttribute.Value = ((LinkReference)reference).Link;
                    linkNode.Attributes.Append(urlAttribute);
                    linkNodeAttribute.Value = "en";
                    linkNode.Attributes.Append(linkNodeAttribute);
                    captionNode.InnerText = linkReference.Caption;
                    captionNodeAttribute.Value = "en";
                    captionNode.Attributes.Append(captionNodeAttribute);
                    linkReferenceNode.AppendChild(linkNode);
                    linkReferenceNode.AppendChild(captionNode);
                    referencesNode.AppendChild(linkReferenceNode);
                }
                else if (reference is BookReference)
                {
                    BookReference bookReference = (BookReference)reference;
                    XmlNode bookReferenceNode = xml.CreateNode(XmlNodeType.Element, "bookReference", null);
                    XmlNode authorNode = xml.CreateNode(XmlNodeType.Element, "author", null);
                    XmlAttribute authorNodeAttribute = xml.CreateAttribute("lang");
                    XmlNode publisherNode = xml.CreateNode(XmlNodeType.Element, "publisher", null);
                    XmlAttribute publisherNodeAttribute = xml.CreateAttribute("lang");
                    XmlNode nameNode = xml.CreateNode(XmlNodeType.Element, "name", null);
                    XmlAttribute nameNodeAttribute = xml.CreateAttribute("lang");
                    authorNode.InnerText = bookReference.Author;
                    authorNodeAttribute.Value = "en";
                    authorNode.Attributes.Append(authorNodeAttribute);
                    publisherNode.InnerText = bookReference.Publisher;
                    publisherNodeAttribute.Value = "en";
                    publisherNode.Attributes.Append(publisherNodeAttribute);
                    nameNode.InnerText = bookReference.Name;
                    nameNodeAttribute.Value = "en";
                    nameNode.Attributes.Append(nameNodeAttribute);
                    bookReferenceNode.AppendChild(authorNode);
                    bookReferenceNode.AppendChild(publisherNode);
                    bookReferenceNode.AppendChild(nameNode);
                    referencesNode.AppendChild(bookReferenceNode);
                }

            }

        }

        public void RemoveReference(Reference reference)
        {
            if (reference is BookReference)
            {
                BookReference bookReference = (BookReference)reference;
                XmlNode referencesNode = xml.SelectSingleNode("/documentation/references");
                foreach (XmlNode node in referencesNode.ChildNodes)
                {
                    if (node.Name.Equals("bookReference") && node["name"].InnerText.Equals(bookReference.Name))
                    {
                        referencesNode.RemoveChild(node);
                    }
                }
            }
            else if (reference is LinkReference)
            {
                LinkReference linkReference = (LinkReference)reference;
                XmlNode referencesNode = xml.SelectSingleNode("/documentation/references");
                foreach (XmlNode node in referencesNode.ChildNodes)
                {
                    if (node.Name.Equals("linkReference") && node["caption"].InnerText.Equals(linkReference.Caption))
                    {
                        referencesNode.RemoveChild(node);
                    }
                }
            }
        }
    }

    public class Reference
    {
        
    }

    public class BookReference : Reference
    {
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Name { get; set; }
    }

    public class LinkReference : Reference
    {
        public string Link { get; set; }
        public string Caption { get; set; }
    }
}
