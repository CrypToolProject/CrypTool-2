using System;
using System.Text;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services.Description;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Threading;
using System.Threading;


namespace Soap
{
    [Author("Tim Podeszwa", "tim.podeszwa@student.uni-siegen.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Soap.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Soap/soap.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Soap : ICrypComponent
    {

        #region Fields

        private ISettings _settings = new SoapSettings();
        private SoapPresentation presentation;
        private XmlNode _node;
        private XmlNode _envelope;
        private XmlNode _body;
        private XmlDocument _wsdl;
        private XmlDocument _soap;
        private XmlDocument _inputDocument;
        private XmlDocument _securedSOAP;
        private string[] _signedElements;
        private Hashtable _idTable;
        private bool _bodySigned;
        private bool _methodNameSigned;
        private bool _bodyEncrypted;
        private bool _methodNameEncrypted;
        private bool _secHeaderEnc;
        private bool _secHeaderSigned;
        private bool _loaded;
        private bool send;
        private bool _gotKey;
        private bool _wsdlLoaded;
        private bool _isExecuting;
        private int _contentCounter;
        private bool _hadHeader;
        private RSACryptoServiceProvider _wsRSACryptoProv;
        private RSACryptoServiceProvider _rsaCryptoProv;
        private DSACryptoServiceProvider _dsaCryptoProv;
        private CspParameters _cspParams;
        private RSACryptoServiceProvider _rsaKey;
        private string _wsPublicKey;
        private string _lastSessionKey;
        private EncryptionSettings _encryptionSettings;
        private SignatureSettings _signatureSettings;

        #endregion

        #region Properties

        public string LastSessionKey
        {
            get
            {
                return this._lastSessionKey;
            }
        }
        public bool HadHeader
        {
            get
            {
                return this._hadHeader;
            }
        }

        public bool WSDLLoaded
        {
            get
            {
                return this._wsdlLoaded;
            }
        }
        public bool GotKey
        {
            get
            {
                return this._gotKey;
            }
        }
        public XmlDocument SecuredSoap
        {
            get
            {
                return this._securedSOAP;
            }
        }

        public RSACryptoServiceProvider WsRSACryptoProv
        {
            get { return this._wsRSACryptoProv; }
            set { this._wsRSACryptoProv = value; }
        }


        public string WsPublicKey
        {
            get { return this._wsPublicKey; }
            set { this._wsPublicKey = value; }
        }

        [PropertyInfo(Direction.InputData, "WsdlCaption", "WsdlTooltip", false)]
        public XmlDocument Wsdl
        {
            set
            {
                if (value != null)
                {
                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (value != null)
                        {
                            this._wsdl = value;
                            string wsdlString = CopyXmlToString(value);
                            this.LoadWSDL(wsdlString);
                            this._wsdlLoaded = true;
                            this.OnPropertyChanged("wsdl");
                            this.CreateInfoMessage("Received WSDL File");
                            this.CreateInfoMessage("Created SOAP Message");
                        }
                    }, null);
                }
            }
            get
            {
                return null;
            }
        }

        [PropertyInfo(Direction.InputData, "PublicKeyCaption", "PublicKeyTooltip",false)]
        public string PublicKey
        {
            get
            {
                return this._wsPublicKey;
            }
            set
            {
                if (value != null)
                {
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        {
                            this._wsPublicKey = value;
                            WsRSACryptoProv.FromXmlString(this._wsPublicKey);
                            this._gotKey = true;
                            mySettings.gotkey = true;
                            mySettings.wsRSAcryptoProv = WsRSACryptoProv.ToXmlString(false);
                            OnPropertyChanged("publicKey");
                            CreateInfoMessage("Public Key Received");
                        }
                    }, null);
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip",true)]
        public XmlDocument OutputString
        {
            get { return this._securedSOAP; }
            set
            {

                this._securedSOAP = value;
                OnPropertyChanged("OutputString");
                send = true;

            }
        }
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", false)]
        public XmlDocument InputString
        {
            get { return this._inputDocument; }
            set
            {
                this._inputDocument = value;

                OnPropertyChanged("InputString");
            }
        }

        #endregion

        #region Structs

        private struct EncryptionSettings
        {
            public bool content;
            public bool showsteps;
        }
        private struct SignatureSettings
        {
            public string sigAlg;
            public bool Xpath;
            public bool showsteps;
        }

        #endregion

        #region Constructor

        public Soap()
        {
            this._soap = new XmlDocument();
            this._gotKey = false;
            this.presentation = new SoapPresentation(this);
            this._settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            this._wsdlLoaded = false;
            this._idTable = new Hashtable();
            this._soap = new XmlDocument();
            this._encryptionSettings = new EncryptionSettings();
            this._signatureSettings = new SignatureSettings();
            this._cspParams = new CspParameters();
            this._cspParams.KeyContainerName = "XML_ENC_RSA_KEY";
            this._rsaKey = new RSACryptoServiceProvider(_cspParams);
            this._rsaCryptoProv = new RSACryptoServiceProvider();
            this._dsaCryptoProv = new DSACryptoServiceProvider();
            this._wsRSACryptoProv = new RSACryptoServiceProvider();
            this._securedSOAP = new XmlDocument();
            this._soap = new XmlDocument();
            mySettings.idtable = _idTable;
            this._rsaCryptoProv.ToXmlString(false);
            mySettings.rsacryptoProv = this._rsaCryptoProv.ToXmlString(true);
            mySettings.dsacryptoProv = this._dsaCryptoProv.ToXmlString(true);
            mySettings.wsRSAcryptoProv = this._wsRSACryptoProv.ToXmlString(false);
            this._contentCounter = 0;
            mySettings.securedsoap = CopyXmlToString(this._securedSOAP);
            this.InputString = new XmlDocument();
            this._loaded = false;
            this._signatureSettings.sigAlg = "1";
        }

        #endregion

        #region Methods

        public bool GetShowSteps()
        {
            return this._signatureSettings.showsteps;
        }

        private void SetSignedElements(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable table = ds.Tables[0];
                this._signedElements = new string[table.Columns.Count];
            }
        }

        public void AddIdToElement(string element)
        {
            if (element != null)
            {
                if (!this._idTable.ContainsKey(element))
                {
                    System.Random randomizer = new Random();
                    int randomNumber = randomizer.Next(100000000);
                    if (!this._idTable.ContainsValue(randomNumber))
                    {
                        System.Threading.Thread.Sleep(500);
                        randomNumber = randomizer.Next(100000000);
                    }
                    this._idTable.Add(element, randomNumber);
                    mySettings.idtable = this._idTable;
                }
            }
        }

        private XmlNode GetElementById(string id)
        {
            XmlNodeList securityHeader = this._securedSOAP.GetElementsByTagName("wsse:Security");
            if (securityHeader != null)
            {
                foreach (XmlNode node in securityHeader)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                        {
                            return node;
                        }
                    }
                }
            }
            XmlNode body = this._securedSOAP.GetElementsByTagName("s:Body")[0];
            if (body != null)
            {
                foreach (XmlAttribute att in body.Attributes)
                {
                    if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                    {
                        return body;
                    }
                }
            }

            foreach (XmlNode node in body.ChildNodes)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                    {
                        return node;
                    }
                }
                foreach (XmlNode child in node.ChildNodes)
                {
                    foreach (XmlAttribute att in child.Attributes)
                    {
                        if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                        {
                            return child;
                        }
                    }
                }
            }
            return null;
        }

        public XmlNode[] GetSignedElements()
        {
            ArrayList list = new ArrayList();
            if (this._bodySigned)
            {
                list.Add(this._securedSOAP.GetElementsByTagName("s:Body")[0]);
            }
            if (this._methodNameSigned)
            {
                list.Add(this._securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild);
            }
            foreach (XmlNode node in this._securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
            {
                if (this.GetIsSigned(node))
                {
                    list.Add(node);
                }
            }
            if (this._secHeaderSigned)
            {
                if (this.GetIsSigned(this._securedSOAP.GetElementsByTagName("wsse:Security")[0]))
                {
                    list.Add(this._securedSOAP.GetElementsByTagName("wsse:Security")[0]);
                }
            }


            XmlNode[] signedElements = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                signedElements[i] = (XmlNode)list[i];
            }
            return signedElements;
        }

        public XmlNode[] GetElementsToSign()
        {
            if (this._secHeaderEnc && this._secHeaderSigned)
            {
                XmlNode[] elementToSign = new XmlNode[0];
                return elementToSign;
            }
            if (this._secHeaderEnc)
            {
                XmlNode[] elementToSign = new XmlNode[1];
                elementToSign[0] = this._securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return elementToSign;
            }


            ArrayList list = new ArrayList();
            if (!this._secHeaderSigned && (this._securedSOAP.GetElementsByTagName("wsse:Security").Count > 0))
            {
                list.Add(this._securedSOAP.GetElementsByTagName("wsse:Security")[0]);
            }
            XmlNode body = _securedSOAP.GetElementsByTagName("s:Body")[0];
            XmlNode bodysChildNode = body.ChildNodes[0];
            if (!this._bodySigned)
            {
                list.Add(body);
                if (!this._bodyEncrypted)

                    if (!this._methodNameSigned)
                    {
                        list.Add(bodysChildNode);
                        if (!this._methodNameEncrypted)
                        {
                            foreach (XmlNode childNode in bodysChildNode.ChildNodes)
                            {
                                bool signed = false;
                                XmlNode[] signedElements = this.GetSignedElements();
                                foreach (XmlNode signedElement in signedElements)
                                {
                                    if (childNode.Name.Equals(signedElement.Name))
                                    {
                                        signed = true;
                                    }
                                }
                                if (!signed)
                                {
                                    list.Add(childNode);
                                }
                            }
                        }
                    }
            }


            XmlNode[] elementsToSign = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                elementsToSign[i] = (XmlNode)list[i];
            }
            return elementsToSign;
        }

        private bool GetHasEncryptedContent(XmlNode node)
        {
            bool value = false;
            if (node != null && node.HasChildNodes)
            {
                if (node.ChildNodes[0].Name.Equals("xenc:EncryptedData"))
                {
                    foreach (XmlAttribute att in node.ChildNodes[0].Attributes)
                    {
                        if (att.Value.Equals(EncryptedXml.XmlEncElementContentUrl))
                        {
                            value = true;
                        }
                    }
                }
            }
            return value;
        }

        public XmlNode[] GetEncryptedElements()
        {
            ArrayList list = new ArrayList();
            XmlNode header = this._securedSOAP.GetElementsByTagName("s:Header")[0];
            if (header != null)
            {
                foreach (XmlNode node in this._securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
                {
                    if (node.Name.Equals("wsse:Security"))
                    {
                        if (GetHasEncryptedContent(node))
                        {
                            list.Add(node);
                        }
                    }
                }
            }

            XmlElement body = (XmlElement)_securedSOAP.GetElementsByTagName("s:Body")[0];
            if (GetHasEncryptedContent(body))
            {
                list.Add(body);
            }
            else
            {
                foreach (XmlNode node in body.ChildNodes)
                {
                    if (node.Name.Equals("xenc:EncryptedData"))
                    {
                        list.Add(node);
                    }
                    else
                    {
                        if (GetHasEncryptedContent(node))
                        {
                            list.Add(node);
                        }
                        foreach (XmlNode nod in node.ChildNodes)
                        {
                            if (nod.Name.Equals("xenc:EncryptedData"))
                            {
                                list.Add(nod);
                            }
                            else
                            {
                                if (GetHasEncryptedContent(nod))
                                {
                                    list.Add(nod);
                                }
                            }
                        }
                    }
                }
            }


            XmlNode[] encryptedElements = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                encryptedElements[i] = (XmlNode)list[i];
            }
            return encryptedElements;
        }

        public XmlNode[] GetElementsToEncrypt()
        {
            if (this._secHeaderEnc && this._secHeaderSigned)
            {
                XmlNode[] elementToEncrypt = new XmlNode[0];
                return elementToEncrypt;
            }
            if (this._secHeaderSigned)
            {
                XmlNode[] elementToEncrypt = new XmlNode[1];
                elementToEncrypt[0] = _securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return elementToEncrypt;
            }
            else
            {

                ArrayList list = new ArrayList();
                XmlNode header = this._securedSOAP.GetElementsByTagName("s:Header")[0];
                if (header != null)
                {
                    foreach (XmlNode node in this._securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
                    {
                        if (node.Name.Equals("wsse:Security") && (!GetHasEncryptedContent(node)))
                        {
                            list.Add(node);
                        }
                    }
                }
                XmlElement body = (XmlElement)_securedSOAP.GetElementsByTagName("s:Body")[0];
                if (!GetHasEncryptedContent(body))
                {
                    list.Add(body);
                    if (!this._bodySigned)
                    {
                        foreach (XmlNode node in body.ChildNodes)
                        {
                            if (!GetHasEncryptedContent(node) && (!node.Name.Equals("xenc:EncryptedData")))
                            {
                                list.Add(node);
                                if (!this._methodNameSigned)
                                {
                                    foreach (XmlNode nod in node.ChildNodes)
                                    {
                                        if (!GetHasEncryptedContent(nod) && (!nod.Name.Equals("xenc:EncryptedData")))
                                        {
                                            list.Add(nod);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                XmlNode[] elementsToEncrypt = new XmlNode[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    elementsToEncrypt[i] = (XmlNode)list[i];
                }
                return elementsToEncrypt;
            }

        }

        private bool GetIsSigned(XmlNode node)
        {
            bool signed = false;
            if (node != null)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Id"))
                    {
                        foreach (XmlNode refElem in this._securedSOAP.GetElementsByTagName("ds:Reference"))
                        {
                            foreach (XmlAttribute refAtt in refElem.Attributes)
                            {
                                if (refAtt.Name.Equals("URI"))
                                {
                                    if (refAtt.Value.Equals("#" + att.Value))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (XmlNode xPath in _securedSOAP.GetElementsByTagName("ds:XPath"))
                {
                    string[] splitter = xPath.InnerText.Split(new char[] { '/' });
                    if (splitter[splitter.Length - 1].Equals(node.Name))
                    {
                        return true;
                    }
                }
            }
            return signed;
        }

        public XmlNode[] GetParameterToEdit()
        {
            ArrayList list = new ArrayList();
            if (this._bodyEncrypted || this._bodySigned || this._methodNameEncrypted || this._methodNameSigned)
            {
                XmlNode[] emptySet = new XmlNode[0];
                return emptySet;
            }
            if (this._secHeaderEnc || this._secHeaderSigned)
            {
                XmlNode[] emptySet = new XmlNode[0];
                return emptySet;
            }

            if (!this._secHeaderEnc)
            {
                foreach (XmlNode parameter in this._securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
                {
                    if (!GetIsSigned(parameter))
                    {
                        if (!GetHasEncryptedContent(parameter))
                        {
                            if (!parameter.Name.Equals("xenc:EncryptedData"))
                                list.Add(parameter);
                        }
                    }
                }
            }
            XmlNode[] parametersToEdit = new XmlNode[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                parametersToEdit[i] = (XmlNode)list[i];
            }
            return parametersToEdit;
        }

        public XmlNode[] GetParameter()
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode node in this._securedSOAP.GetElementsByTagName("s:Body")[0].ChildNodes[0].ChildNodes)
            {
                XmlNode[] signedNodes = GetSignedElements();
                bool isSigned = false;
                foreach (XmlNode signedElement in signedNodes)
                {
                    if (signedElement.Name.Equals(node.Name))
                    {
                        isSigned = true;
                    }

                }
                XmlNode[] encryptedNodes = GetEncryptedElements();
                bool isEncrypted = false;
                foreach (XmlNode encryptedNode in encryptedNodes)
                {
                    if (encryptedNode.Equals(node))
                    {
                        isEncrypted = true;
                    }
                }
                if (!isSigned && !isEncrypted)
                {
                    list.Add(node);
                }
            }
            XmlNode[] parameters = new XmlNode[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                parameters[i] = (XmlNode)list[i];
            }
            return parameters;
        }

        public string GetSignatureAlgorithm()
        {
            return this._signatureSettings.sigAlg;
        }

        public void SaveSoap()
        {
            mySettings.securedsoap = CopyXmlToString(this._securedSOAP);
        }

        public void LoadWSDL(string wsdlString)
        {
            if (wsdlString != null && !wsdlString.Equals(""))
            {
                StringReader stringReader = new StringReader(wsdlString);
                XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
                ServiceDescription serviceDescriptionsRead = ServiceDescription.Read(xmlTextReader);
                ServiceDescription serviceDescription = serviceDescriptionsRead.Services[0].ServiceDescription;
                Types types = serviceDescription.Types;
                //PortTypeCollection portTypes = serviceDescription.PortTypes;
                //MessageCollection messages = serviceDescription.Messages;
                //XmlSchema schema = types.Schemas[0];
                //PortType porttype = portTypes[0];
                //Operation operation = porttype.Operations[0];
                //OperationInput input = operation.Messages[0].Operation.Messages.Input;
                //Message message = messages[input.Message.Name];
                //MessagePart messagePart = message.Parts[0];
                XmlSchema xmlSchema = types.Schemas[0];
                StringWriter stringWriter = new StringWriter();
                xmlSchema.Write(stringWriter);
                DataSet set = new DataSet();
                StringReader sreader = new StringReader(stringWriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                set.ReadXmlSchema(xmlreader);
                this.SetSignedElements(set);
                this._soap = new XmlDocument();
                this._node = this._soap.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");
                this._soap.AppendChild(_node);
                this._envelope = this._soap.CreateElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
                this._soap.AppendChild(_envelope);
                this._body = this._soap.CreateElement("s", "Body", "http://www.w3.org/2003/05/soap-envelope");
                XmlNode inputNode = this._soap.CreateElement("tns", set.Tables[0].ToString(), set.Tables[0].Namespace);
                DataTable table = set.Tables[0];
                foreach (DataColumn tempColumn in table.Columns)
                {
                    XmlNode newElement = this._soap.CreateElement("tns", tempColumn.ColumnName, set.Tables[0].Namespace);
                    inputNode.AppendChild(newElement);
                }
                this._body.AppendChild(inputNode);
                this._envelope.AppendChild(this._body);
                stringWriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                this._soap.WriteContentTo(xmlTextWriter);
                XmlNode rootElement = this._soap.SelectSingleNode("/*");
                presentation.OriginalSoapItem = new System.Windows.Controls.TreeViewItem();
                presentation.OriginalSoapItem.IsExpanded = true;
                StackPanel panel1 = new StackPanel();
                StackPanel origSoapPanel = new StackPanel();
                StackPanel origSoapPanel2 = new StackPanel();
                panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                origSoapPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                origSoapPanel2.Orientation = System.Windows.Controls.Orientation.Horizontal;
                TextBlock elem1 = new TextBlock();
                elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
                TextBlock origSoapElem = new TextBlock();
                origSoapElem.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
                TextBlock origSoapElem2 = new TextBlock();
                origSoapElem2.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
                panel1.Children.Insert(0, elem1);
                origSoapPanel.Children.Insert(0, origSoapElem);
                origSoapPanel2.Children.Insert(0, origSoapElem2);
                presentation.OriginalSoapItem.Header = panel1;
                this._loaded = false;
                this._securedSOAP = (XmlDocument)this._soap.Clone();
                mySettings.soapelement = CopyXmlToString(this._soap);
                mySettings.securedsoap = CopyXmlToString(this._securedSOAP);
                this.presentation.CopyXmlToTreeView(rootElement, presentation.OriginalSoapItem);
                this.presentation.treeView.Items.Add(presentation.OriginalSoapItem);
                presentation.treeView.Items.Refresh();
                ShowSecuredSoap();
                this._loaded = true;
                this.InputString = this._soap;
                this._wsdlLoaded = true;
                mySettings.wsdlloaded = true;
                OnPropertyChanged("OutputString");
            }
        }

        public bool GetIsShowEncryptionsSteps()
        {
            return this._encryptionSettings.showsteps;
        }

        public bool GetIsEncryptContent()
        {
            return this._encryptionSettings.content;
        }

        public string CopyXmlToString(XmlDocument doc)
        {
            if (doc != null)
            {
                StringWriter stringWriter = new StringWriter();
                doc.Normalize();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmlTextWriter);
                return stringWriter.ToString();
            }
            else
            {
                return "";
            }
        }

        public XmlDocument CopyStringToXml(string s)
        {
            XmlDocument doc = new XmlDocument();
            if (!s.Equals(""))
            {
                StringReader stringReader = new StringReader(s);
                XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
                doc.Load(xmlTextReader);
            }
            return doc;
        }

        public void AddSignedElement(string newElement)
        {
            bool isSigned = false;
            foreach (string signedElement in this._signedElements)
            {
                if (signedElement != null)
                {
                    if (signedElement.Equals(newElement))
                    {
                        isSigned = true;
                    }
                }
            }
            if (!isSigned)
            {
                int count = -1;
                foreach (string signedElement in this._signedElements)
                {
                    count++;
                    if (signedElement == null)
                    {
                        break;
                    }

                }
                this._signedElements[count] = newElement;
            }
        }

        public bool GetIsSigned(string Element)
        {
            bool issigned = false;
            foreach (string s in _signedElements)
            {
                if (s != null)
                {
                    if (s.Equals(Element))
                    {
                        issigned = true;
                    }
                }
            }
            return issigned;
        }

        public void RemoveSignature(string Id)
        {
            XmlNodeList signatureElements = this._securedSOAP.GetElementsByTagName("ds:Signature");
            ArrayList list = new ArrayList();
            XmlNode nodeToDelete = null;
            foreach (XmlNode node in signatureElements)
            {

                foreach (XmlNode child in node.FirstChild.ChildNodes)
                {
                    if (child.Name.Equals("ds:Reference"))
                    {
                        foreach (XmlAttribute att in child.Attributes)
                        {
                            if (att.Name.Equals("URI"))
                            {
                                if (att.Value.Equals("#" + Id))
                                {
                                    nodeToDelete = node;
                                }
                            }
                        }
                    }
                }
            }
            if (nodeToDelete != null)
            {
                foreach (XmlNode node in nodeToDelete.ChildNodes)
                {
                    if (node.Name.Equals("ds:Reference"))
                    {
                        foreach (XmlAttribute atttribute in node.Attributes)
                        {
                            if (atttribute.Name.Equals("URI"))
                            {
                                if (!atttribute.Value.Equals("#" + Id))
                                {
                                    string[] id = atttribute.Value.Split(new char[] { '#' });
                                    XmlNode element = this.GetElementById(id[0]);
                                    list.Add(element);
                                }
                            }
                        }
                    }
                }
            }
            XmlElement[] signArray = new XmlElement[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                signArray[i] = (XmlElement)list[i];
            }

            if (nodeToDelete != null)
            {
                this._securedSOAP.GetElementsByTagName("wsse:Security")[0].RemoveChild(nodeToDelete);
            }

            if (signArray.Length > 0)
            {
                this.SignElements(signArray);
            }
            this.ShowSecuredSoap();
        }

        public void EncryptElements(XmlElement[] elements)
        {
            if (this._gotKey)
            {
                bool content = this._encryptionSettings.content;
                XmlNode securityHeader = this._securedSOAP.GetElementsByTagName("wsse:Security")[0];
                if (securityHeader == null)
                {
                    this._hadHeader = false;
                    XmlNode header = this._securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2003/05/soap-envelope");
                    string wsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";
                    securityHeader = this._securedSOAP.CreateElement("wsse", "Security", wsseNamespace);
                    header.AppendChild(securityHeader);
                    XmlNode envelope = this._securedSOAP.GetElementsByTagName("s:Envelope")[0];
                    XmlNode soapBody = this._securedSOAP.GetElementsByTagName("s:Body")[0];
                    envelope.InsertBefore(header, soapBody);
                }
                else
                {
                    this._hadHeader = true;
                }
                RijndaelManaged sessionKey = new RijndaelManaged();
                sessionKey.KeySize = 256;
                EncryptedXml encryptedXML = new EncryptedXml();
                EncryptedKey encryptedKey = new EncryptedKey();
                byte[] encryptedEncryptionKey = EncryptedXml.EncryptKey(sessionKey.Key, this._wsRSACryptoProv, false);
                encryptedKey.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
                encryptedKey.CipherData = new CipherData(encryptedEncryptionKey);
                KeyInfoName keyName = new KeyInfoName();
                keyName.Value = "Web Service Public Key";
                encryptedKey.KeyInfo.AddClause(keyName);
                foreach (XmlElement elem in elements)
                {
                    if (elem != null)
                    {
                        //Check if Security Header or Body. Only content encryption is allowed by WS-Security
                        if (elem.Name.Equals("s:Body") || elem.Name.Equals("wsse:Security"))
                        {
                            if (content == false)
                            {
                                CreateErrorMessage("Only the content of the  " + elem.Name + " element can be encrypted");
                            }
                            content = true;
                        }
                        this._lastSessionKey = Convert.ToBase64String(sessionKey.Key);
                        byte[] encryptedElement = encryptedXML.EncryptData(elem, sessionKey, content);
                        EncryptedData encElement = new EncryptedData();
                        DataReference encryptedDataReference = new DataReference();
                        if (!content)
                        {
                            encElement.Type = EncryptedXml.XmlEncElementUrl;
                            encElement.Id = this._idTable[elem.Name].ToString();
                            encryptedDataReference.Uri = "#" + this._idTable[elem.Name].ToString();
                        }
                        else
                        {
                            encElement.Type = EncryptedXml.XmlEncElementContentUrl;
                            AddIdToElement(this._contentCounter + elem.Name);
                            encElement.Id = _idTable[this._contentCounter + elem.Name].ToString();
                            encryptedDataReference.Uri = "#" + this._idTable[_contentCounter + elem.Name].ToString();
                            this._contentCounter++;
                            mySettings.contentcounter = this._contentCounter;
                        }

                        encElement.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);
                        encElement.CipherData.CipherValue = encryptedElement;
                        encryptedKey.AddReference(encryptedDataReference);
                        string s = this._securedSOAP.GetElementsByTagName(elem.Name)[0].ParentNode.Name;

                        if (!content)
                        {
                            this._securedSOAP.GetElementsByTagName(s)[0].ReplaceChild(this._securedSOAP.ImportNode(encElement.GetXml(), true), this._securedSOAP.GetElementsByTagName(elem.Name)[0]);
                        }
                        else
                        {
                            this._securedSOAP.GetElementsByTagName(elem.Name)[0].RemoveAll();
                            this._securedSOAP.GetElementsByTagName(elem.Name)[0].AppendChild(this._securedSOAP.ImportNode(encElement.GetXml(), true));
                        }
                        if (elem.Name.Equals("s:Body"))
                        {
                            this._bodyEncrypted = true;
                            mySettings.bodyencrypted = true;
                        }
                        if (elem.Name.Equals(_soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                        {
                            this._methodNameEncrypted = true;
                            mySettings.methodnameencrypted = this._methodNameEncrypted;
                        }
                        if (elem.Name.Equals("wsse:Security"))
                        {
                            this._secHeaderEnc = true;
                            mySettings.secheaderEnc = true;
                        }
                    }

                }
                securityHeader.InsertBefore(this._securedSOAP.ImportNode(encryptedKey.GetXml(), true), securityHeader.ChildNodes[0]);
                this.PrefixesToEncryptedElement();
                mySettings.securedsoap = CopyXmlToString(_securedSOAP);
            }
            else
            {
                CreateErrorMessage("No key for encryption available");
            }
        }

        private void PrefixesToEncryptedElement()
        {
            XmlNodeList encryptedKeys = _securedSOAP.GetElementsByTagName("EncryptedKey");
            foreach (XmlNode encryptedKey in encryptedKeys)
            {
                AddPrefixesToNodeAndChildNode("xenc", encryptedKey);
            }
            XmlNodeList encryptedDataElements = this._securedSOAP.GetElementsByTagName("EncryptedData");
            foreach (XmlNode encryptedDataElement in encryptedDataElements)
            {
                AddPrefixesToNodeAndChildNode("xenc", encryptedDataElement);
            }
        }

        private void AddPrefixesToNodeAndChildNode(string prefix, XmlNode node)
        {
            if (node.Name.Equals("KeyInfo"))
            {
                node.Prefix = "ds";
                prefix = "ds";
            }
            else
            {
                node.Prefix = prefix;
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                this.AddPrefixesToNodeAndChildNode(prefix, child);
            }
        }

        public void DecryptDocument()
        {
            XmlElement securityHeader = (XmlElement)this._securedSOAP.GetElementsByTagName("Security")[0];
            XmlElement encryptedKey = (XmlElement)this._securedSOAP.GetElementsByTagName("EncryptedKey")[0];
            XmlElement encryptedData = (XmlElement)this._securedSOAP.GetElementsByTagName("EncryptedData")[0];
            XmlElement keyInfo = this._securedSOAP.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            securityHeader.RemoveChild(encryptedKey);
            keyInfo.AppendChild(encryptedKey);
            encryptedData.InsertAfter(keyInfo, encryptedData.GetElementsByTagName("EncryptionMethod")[0]);
            this.ShowSecuredSoap();
            EncryptedXml encXml = new EncryptedXml(this._securedSOAP);
            encXml.AddKeyNameMapping("RSA-Key", this._rsaKey);
            encXml.DecryptDocument();
        }

        public void SignElements(XmlElement[] elements)
        {
            String sigAlgo = this._signatureSettings.sigAlg;
            XmlNode securityHeader = this._securedSOAP.GetElementsByTagName("Security")[0];
            if (securityHeader == null)
            {
                XmlNode header = this._securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2003/05/soap-envelope");
                string wsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";
                securityHeader = this._securedSOAP.CreateElement("Security", wsseNamespace);
                header.AppendChild(securityHeader);
                XmlNode envelope = this._securedSOAP.GetElementsByTagName("Envelope")[0];
                XmlNode soapBody = this._securedSOAP.GetElementsByTagName("Body")[0];
                envelope.InsertBefore(header, soapBody);
            }
            SignedXml signedXML = new SignedXml(this._securedSOAP);
            foreach (XmlElement elementToSign in elements)
            {
                XmlAttribute idAttribute = this._securedSOAP.CreateAttribute("Id");
                idAttribute.Value = this._idTable[elementToSign.Name].ToString();
                elementToSign.Attributes.Append(idAttribute);
                XmlAttributeCollection attributes = elementToSign.Attributes;
                XmlAttribute id = attributes["Id"];
                Reference reference = new Reference("#" + id.Value);
                //   Reference reference = new Reference("");
                XmlElement xpathElement = this._securedSOAP.CreateElement("XPath");
                string Xpath = "ancestor-or-self::Body";
                XmlElement root = this._securedSOAP.DocumentElement;
                XmlElement body = (XmlElement)this._securedSOAP.GetElementsByTagName("Body")[0];
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(this._securedSOAP.NameTable);
                namespaceManager.AddNamespace("s", body.NamespaceURI);
                xpathElement.InnerText = Xpath;
                XmlDsigXPathTransform xpathTrans = new XmlDsigXPathTransform();
                xpathTrans.LoadInnerXml(xpathElement.SelectNodes("."));
                XmlDsigExcC14NTransform transform = new XmlDsigExcC14NTransform();
                reference.AddTransform(transform);
                signedXML.AddReference(reference);
                if (elementToSign.Name.Equals("s:Body"))
                {
                    this._bodySigned = true;
                    mySettings.bodysigned = true;
                }
                if (elementToSign.Name.Equals(this._soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                {
                    this._methodNameSigned = true;
                    mySettings.methodnameSigned = true;
                }

            }
            if (sigAlgo.Equals("1"))
            {
                CspParameters parameter = new CspParameters();
                parameter.KeyContainerName = "Container";
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider(parameter);
                signedXML.SigningKey = provider;
                signedXML.ComputeSignature();
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new RSAKeyValue(provider));
                signedXML.KeyInfo = keyInfo;
                Reference reference = (Reference)signedXML.SignedInfo.References[0];
                IEnumerator enumerator = reference.TransformChain.GetEnumerator();
                enumerator.MoveNext();
                XmlElement envelope = (XmlElement)this._securedSOAP.GetElementsByTagName("Envelope")[0];
                Transform tran = (Transform)enumerator.Current;
                XmlNodeList list2 = envelope.SelectNodes("//. | //@* | //namespace::*");

            }
            if (sigAlgo.Equals("0"))
            {
                DSA dsa = DSA.Create();
                dsa.ToXmlString(false);
                signedXML.SigningKey = dsa;
                signedXML.ComputeSignature();
            }

            XmlElement signaturElement = signedXML.GetXml();
            securityHeader.InsertBefore(this._securedSOAP.ImportNode(signaturElement, true), securityHeader.ChildNodes[0]);

        }

        public bool CheckSecurityHeader()
        {
            bool securityheader = false;
            XmlNodeList securityElements = this._securedSOAP.GetElementsByTagName("wsse:Security");
            if (!(securityElements.Count == 0))
            {
                securityheader = true;
            }
            return securityheader;
        }

        public void CreateSecurityHeaderAndSoapHeader()
        {
            if (!CheckSecurityHeader())
            {
                XmlElement envelope = (XmlElement)this._securedSOAP.GetElementsByTagName("s:Envelope")[0];
                XmlElement header = this._securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2001/12/soap-envelope");
                XmlElement securityHeader = this._securedSOAP.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
                envelope.InsertBefore(header, envelope.FirstChild);
                header.AppendChild(securityHeader);
                mySettings.securedsoap = CopyXmlToString(_securedSOAP);
            }
        }

        private string GetXPathValue(XmlElement element)
        {
            string xPathValue = "/s:Envelope";
            if (element.Name.Equals("wsse:Security"))
            {
                xPathValue = xPathValue + "/s:Header"+"/wsse:Security";
                return xPathValue;
            }
            xPathValue = xPathValue + "/s:Body";
            if (element.Name.Equals("s:Body"))
            {
                return xPathValue;
            }
            xPathValue = xPathValue + "/" + this._securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name;
            if (element.Name.Equals(this._securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name))
            {
                return xPathValue;
            }
            xPathValue = xPathValue + "/" + element.Name;
            return xPathValue;
        }

        public void CreateErrorMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Error));
        }

        public void CreateInfoMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Info));
        }

        public void SignElementsManual(XmlElement[] elementsToSign)
        {

            string dsNamespace = "http://www.w3.org/2000/09/xmldsig#";
            XmlElement signature = this._securedSOAP.CreateElement("ds", "Signature", dsNamespace);
            XmlElement signedInfo = this._securedSOAP.CreateElement("ds", "SignedInfo", dsNamespace);
            XmlElement canonicalizationMethod = _securedSOAP.CreateElement("ds", "CanonicalizationMethod", dsNamespace);
            XmlAttribute canMeth = this._securedSOAP.CreateAttribute("Algorithm");
            canMeth.Value = SignedXml.XmlDsigExcC14NTransformUrl;
            canonicalizationMethod.Attributes.Append(canMeth);
            XmlElement signatureMethod = this._securedSOAP.CreateElement("ds", "SignatureMethod", dsNamespace);
            XmlAttribute sigMeth = this._securedSOAP.CreateAttribute("Algorithm");

            if (this._signatureSettings.sigAlg.Equals("0"))
            {
                sigMeth.Value = SignedXml.XmlDsigDSAUrl;
            }
            if (this._signatureSettings.sigAlg.Equals("1"))
            {
                sigMeth.Value = SignedXml.XmlDsigRSASHA1Url;
            }

            signatureMethod.Attributes.Append(sigMeth);
            XmlNode securityHeader = this._securedSOAP.GetElementsByTagName("wsse:Security")[0];
            
            signature.AppendChild(signedInfo);
            signedInfo.AppendChild(canonicalizationMethod);
            signedInfo.AppendChild(signatureMethod);

            XmlAttribute foundIdAttribute = null;
            foreach (XmlElement elementToSign in elementsToSign)
            {
                this.AddIdToElement(elementToSign.Name);

                foreach (XmlAttribute xmlAttribute in elementToSign.Attributes)
                {
                    
                        if (xmlAttribute.Name == "Id")
                        {
                            foundIdAttribute = xmlAttribute;
                            break;
                        }
                    
                }


            
                XmlAttribute idAttribute = this._securedSOAP.CreateAttribute("Id");
                if (foundIdAttribute == null)
                {
                    idAttribute.Value = this._idTable[elementToSign.Name].ToString();
                    elementToSign.Attributes.Append(idAttribute);
                }
                    
                XmlElement referenceElement = this._securedSOAP.CreateElement("ds", "Reference", dsNamespace);
                XmlAttribute uri = this._securedSOAP.CreateAttribute("URI");
                XmlElement transforms = _securedSOAP.CreateElement("ds", "Transforms", dsNamespace);
                referenceElement.AppendChild(transforms);

                if (this._signatureSettings.Xpath)
                {
                    uri.Value = "";
                    XmlElement xPathTransform = this._securedSOAP.CreateElement("ds", "Transform", dsNamespace);
                    XmlAttribute xPathTransAtt = this._securedSOAP.CreateAttribute("Algorithm");
                    xPathTransAtt.Value = SignedXml.XmlDsigXPathTransformUrl;
                    xPathTransform.Attributes.Append(xPathTransAtt);
                    XmlElement xPathValue = this._securedSOAP.CreateElement("ds", "XPath", dsNamespace);
                    xPathValue.InnerXml = this.GetXPathValue(elementToSign);
                    xPathTransform.AppendChild(xPathValue);
                    transforms.AppendChild(xPathTransform);
                }
                else
                {
                    if (foundIdAttribute == null)
                    {
                        uri.Value = "#" + this._idTable[elementToSign.Name].ToString();
                    }
                    else
                    {
                        uri.Value = "#" + foundIdAttribute.Value;
                    }
                }
                referenceElement.Attributes.Append(uri);
                XmlElement c14nTransform = this._securedSOAP.CreateElement("ds", "Transform", dsNamespace);
                XmlAttribute c14Url = this._securedSOAP.CreateAttribute("Algorithm");
                c14Url.Value = SignedXml.XmlDsigEnvelopedSignatureTransformUrl;
                c14nTransform.Attributes.Append(c14Url);
                transforms.AppendChild(c14nTransform);
                XmlElement digestMethod = this._securedSOAP.CreateElement("ds", "DigestMethod", dsNamespace);
                XmlAttribute digMethodAttribute = this._securedSOAP.CreateAttribute("Algorithm");
                digMethodAttribute.Value = SignedXml.XmlDsigSHA1Url;
                digestMethod.Attributes.Append(digMethodAttribute);
                referenceElement.AppendChild(digestMethod);
                XmlElement digestValue = this._securedSOAP.CreateElement("ds", "DigestValue", dsNamespace);
                digestValue.InnerText = Convert.ToBase64String(this.GetDigestValueForElementWithSha1(elementToSign));
                referenceElement.AppendChild(digestValue);
                signedInfo.AppendChild(referenceElement);
                if (elementToSign.Name.Equals("s:Body"))
                {
                    this._bodySigned = true;
                    mySettings.bodysigned = true;
                }
                if (elementToSign.Name.Equals(this._soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                {
                    this._methodNameSigned = true;
                     mySettings.methodnameSigned = true;
                }
                if (elementToSign.Name.Equals("wsse:Security"))
                {
                    this._secHeaderSigned = true;
                    mySettings.secheaderSigned = true;
                }

            }
            XmlElement signatureValue = this._securedSOAP.CreateElement("ds", "SignatureValue", dsNamespace);
            KeyInfo keyInfo = new KeyInfo();
            if (this._signatureSettings.sigAlg.Equals("1"))
            {
                signatureValue.InnerXml = Convert.ToBase64String(this._rsaCryptoProv.SignHash(this.GetDigestValueForElementWithSha1(signedInfo), CryptoConfig.MapNameToOID("SHA1")));
                keyInfo.AddClause(new RSAKeyValue(this._rsaCryptoProv));

            }
            if (this._signatureSettings.sigAlg.Equals("0"))
            {
                signatureValue.InnerXml = Convert.ToBase64String(this._dsaCryptoProv.SignHash(this.GetDigestValueForElementWithSha1(signedInfo), CryptoConfig.MapNameToOID("SHA1")));
                keyInfo.AddClause(new DSAKeyValue(this._dsaCryptoProv));
            }
            signature.AppendChild(signatureValue);
            XmlElement xmlKeyInfo = keyInfo.GetXml();
            xmlKeyInfo.Prefix = "ds";
            foreach (XmlNode childNode in xmlKeyInfo.ChildNodes)
            {
                childNode.Prefix = "ds";
            }
            signature.AppendChild(this._securedSOAP.ImportNode(xmlKeyInfo, true));
            securityHeader.InsertBefore(signature, securityHeader.FirstChild);
         //   XmlElement secHead = (XmlElement)this._securedSOAP.GetElementsByTagName("wsse:Security")[0].Insert(signature);
            mySettings.securedsoap = this.CopyXmlToString(this._securedSOAP);
        }

        public string GetIdToElement(string elementName)
        {
            if (elementName != null && this._idTable.ContainsKey(elementName))
            {
                string id = this._idTable[elementName].ToString();
                return id;
            }
            return null;
        }

        public bool GetXPathTransForm()
        {
            return this._signatureSettings.Xpath;
        }

        public byte[] GetDigestValueForElementWithSha1(XmlElement element)
        {
            //XmlNode node = element.GetElementsByTagName("ds:Signature")[0];
            //element.RemoveChild(node);
            Stream canonicalized = this.CanonicalizeNodeWithExcC14n(element);
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            StreamReader reader = new StreamReader(canonicalized);

            string canonicalizedString = reader.ReadToEnd();
           canonicalized.Position = 0;

            byte[] byteValue = sha1.ComputeHash(canonicalized);
            return byteValue;
        }

        public Stream CanonicalizeNodeWithExcC14n(XmlElement nodeToCanon)
        {
            XmlNode node = (XmlNode)nodeToCanon;
            XmlNodeReader reader = new XmlNodeReader(node);
            Stream inputStream = new MemoryStream();
            XmlWriter writer = new XmlTextWriter(inputStream, Encoding.UTF8);
            writer.WriteNode(reader, false);
            writer.Flush();
            inputStream.Position = 0;
            XmlDsigExcC14NTransform transformation = new XmlDsigExcC14NTransform();
            transformation.LoadInput(inputStream);
            Stream outputStream = (Stream)transformation.GetOutput();
            return outputStream;
        }

        public void ShowSecuredSoap()
        {
            presentation.treeView.Items.Clear();
            presentation._namespacesTable.Clear();
            this.presentation.SecuredSoapItem = null;
            this.presentation.SecuredSoapItem = new System.Windows.Controls.TreeViewItem();
            presentation.SecuredSoapItem.IsExpanded = true;
            StackPanel panel1 = new StackPanel();
            panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
            TextBlock elem1 = new TextBlock();
            elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            panel1.Children.Insert(0, elem1);
            presentation.SecuredSoapItem.Header = panel1;
            XmlNode rootElement = _securedSOAP.SelectSingleNode("/*");
            this.presentation.CopyXmlToTreeView(rootElement,  presentation.SecuredSoapItem);
            this.presentation.treeView.Items.Add(presentation.SecuredSoapItem);
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public Object XmlOutputConverter(Object Data)
        {

           if(this._securedSOAP!=null)
            {
                
                XmlDocument doc = (XmlDocument)this._securedSOAP;
                StringWriter stringWriter = new StringWriter();
                Object obj = new Object();
                try
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                    xmlTextWriter.Formatting = Formatting.Indented;
                    doc.WriteContentTo(xmlTextWriter);
                    obj = (Object)stringWriter.ToString();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;

        }
        public Object WsdlConverter(Object Data)
        {

           if(this._wsdl!=null)
            {
                
                XmlDocument doc = (XmlDocument)this._wsdl;
                StringWriter stringWriter = new StringWriter();
                Object obj = new Object();
                try
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                    xmlTextWriter.Formatting = Formatting.Indented;
                    doc.WriteContentTo(xmlTextWriter);
                    obj = (Object)stringWriter.ToString();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;

        }
       

        public Object XMLInputConverter(Object Data)
        {
            if(this._inputDocument!=null)
            {
                Object obj = new Object();
                try
                {
                    XmlDocument doc = (XmlDocument)this._inputDocument;
                    StringWriter stringWriter = new StringWriter();
                    
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                    xmlTextWriter.Formatting = Formatting.Indented;
                    doc.WriteContentTo(xmlTextWriter);
                    obj = (Object)stringWriter.ToString();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());

                }
                
                return obj;
            }
            return null;
        }

        #endregion

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {
            if (this._isExecuting)
            {
                this.OutputString = this._securedSOAP;
                this._isExecuting = false;
            }
        }

        public void Initialize()
        {

        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void PostExecution()
        {
            this._isExecuting = false;
            Dispose();
        }

        public void PreExecution()
        {
            this._isExecuting = true;
            Dispose();
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return this.presentation; }
        }

        public ISettings Settings
        {
            get { return (SoapSettings)this._settings; }
        }

        public SoapSettings mySettings
        {
            get { return (SoapSettings)this._settings; }
        }

        public void Stop()
        {
            send = false;
        }

        #endregion

        #region EventHandlers

        private void SettingsPropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SoapSettings settings = sender as SoapSettings;

            switch (e.PropertyName)
            {
                case "SignatureAlg":
                    this._signatureSettings.sigAlg = settings.SignatureAlg;
                    break;

                case "SigXPathRef":
                    this._signatureSettings.Xpath = settings.SigXPathRef;
                    break;

                case "SigShowSteps":
                    this._signatureSettings.showsteps = settings.SigShowSteps;
                    break;
                case "EncContentRadio":
                    if (settings.EncContentRadio == 0)
                    {
                        this._encryptionSettings.content = false;
                    }
                    if (settings.EncContentRadio == 1)
                    {
                        this._encryptionSettings.content = true;
                    }
                    break;

                case "EncShowSteps":
                    this._encryptionSettings.showsteps = settings.EncShowSteps;
                    break;
                case "gotkey":
                    this._gotKey = settings.gotkey;
                    break;
                case "wspublicKey":
                    this._wsPublicKey = settings.wspublicKey;
                    break;
                case "dsacryptoProv":
                    this._dsaCryptoProv.FromXmlString(settings.dsacryptoProv);
                    break;
                case "rsacryptoProv":
                    this._rsaCryptoProv.FromXmlString(settings.rsacryptoProv);
                    break;
                case "wsRSAcryptoProv":
                    this._wsRSACryptoProv.FromXmlString(settings.wsRSAcryptoProv);
                    break;
                case "contentcounter":
                    this._contentCounter = settings.contentcounter;
                    break;
                case "secheaderSigned":
                    this._secHeaderSigned = settings.secheaderSigned;
                    break;
                case "secheaderEnc":
                    this._secHeaderEnc = settings.secheaderEnc;
                    break;
                case "methodnameencrypted":
                    this._methodNameEncrypted = settings.methodnameencrypted;
                    break;
                case "bodyencrypted":
                    this._bodyEncrypted = settings.bodyencrypted;
                    break;
                case "methodnameSigned":
                    this._methodNameSigned = settings.methodnameSigned;
                    break;
                case "bodysigned":
                    this._bodySigned = settings.bodysigned;
                    break;
                case "idtable":
                    this._idTable = settings.idtable;
                    break;
                case "securedsoap":
                    if (settings.securedsoap != null)
                    {
                        if (!this._loaded)
                        {
                            this._securedSOAP = (CopyStringToXml(settings.securedsoap));
                            ShowSecuredSoap();
                            this._loaded = true;
                        }
                        else
                        {
                            this._loaded = true;
                        }
                    }
                    break;
                case "soapelement":
                    if (settings.soapelement != null)
                    {
                        this._soap = CopyStringToXml(settings.soapelement);
                    }
                    break;
                case "wsdlloaded":
                    this._wsdlLoaded = settings.wsdlloaded;
                    break;
                case "sendSoap":
                    if (!send)
                    {
                        OnPropertyChanged("OutputString");
                        send = true;
                    }
                    break;
                case "ResetSoap":
                    if (this._soap != null)
                    {
                        mySettings.bodysigned = false;
                        mySettings.secheaderSigned = false;
                        mySettings.secheaderEnc = false;
                        mySettings.methodnameSigned = false;
                        mySettings.methodnameencrypted = false;
                        mySettings.bodyencrypted = false;
                        this._securedSOAP = (XmlDocument)this._soap.Clone();
                        mySettings.securedsoap = CopyXmlToString(_securedSOAP);
                        ShowSecuredSoap();
                    }
                    break;
                case "AnimationSpeed":
                    presentation.SetAnimationSpeed(settings.AnimationSpeed);
                    break;
                case "playPause":
                    presentation.StartStopAnimation();
                    break;
                case "endAnimation":
                    presentation.EndAnimation();
                    break;

            }
        }

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #endregion
    }

}
