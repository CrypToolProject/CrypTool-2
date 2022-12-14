using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Web.Services.Description;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;


namespace Soap
{
    [Author("Tim Podeszwa", "tim.podeszwa@student.uni-siegen.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Soap.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Soap/soap.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Soap : ICrypComponent
    {

        #region Fields

        private readonly ISettings _settings = new SoapSettings();
        private readonly SoapPresentation presentation;
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
        private readonly RSACryptoServiceProvider _rsaCryptoProv;
        private readonly DSACryptoServiceProvider _dsaCryptoProv;
        private readonly CspParameters _cspParams;
        private readonly RSACryptoServiceProvider _rsaKey;
        private string _wsPublicKey;
        private string _lastSessionKey;
        private EncryptionSettings _encryptionSettings;
        private SignatureSettings _signatureSettings;

        #endregion

        #region Properties

        public string LastSessionKey => _lastSessionKey;
        public bool HadHeader => _hadHeader;

        public bool WSDLLoaded => _wsdlLoaded;
        public bool GotKey => _gotKey;
        public XmlDocument SecuredSoap => _securedSOAP;

        public RSACryptoServiceProvider WsRSACryptoProv
        {
            get => _wsRSACryptoProv;
            set => _wsRSACryptoProv = value;
        }


        public string WsPublicKey
        {
            get => _wsPublicKey;
            set => _wsPublicKey = value;
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
                            _wsdl = value;
                            string wsdlString = CopyXmlToString(value);
                            LoadWSDL(wsdlString);
                            _wsdlLoaded = true;
                            OnPropertyChanged("wsdl");
                            CreateInfoMessage("Received WSDL File");
                            CreateInfoMessage("Created SOAP Message");
                        }
                    }, null);
                }
            }
            get => null;
        }

        [PropertyInfo(Direction.InputData, "PublicKeyCaption", "PublicKeyTooltip", false)]
        public string PublicKey
        {
            get => _wsPublicKey;
            set
            {
                if (value != null)
                {
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        {
                            _wsPublicKey = value;
                            WsRSACryptoProv.FromXmlString(_wsPublicKey);
                            _gotKey = true;
                            mySettings.gotkey = true;
                            mySettings.wsRSAcryptoProv = WsRSACryptoProv.ToXmlString(false);
                            OnPropertyChanged("publicKey");
                            CreateInfoMessage("Public Key Received");
                        }
                    }, null);
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public XmlDocument OutputString
        {
            get => _securedSOAP;
            set
            {

                _securedSOAP = value;
                OnPropertyChanged("OutputString");
                send = true;

            }
        }
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", false)]
        public XmlDocument InputString
        {
            get => _inputDocument;
            set
            {
                _inputDocument = value;

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
            _soap = new XmlDocument();
            _gotKey = false;
            presentation = new SoapPresentation(this);
            _settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            _wsdlLoaded = false;
            _idTable = new Hashtable();
            _soap = new XmlDocument();
            _encryptionSettings = new EncryptionSettings();
            _signatureSettings = new SignatureSettings();
            _cspParams = new CspParameters
            {
                KeyContainerName = "XML_ENC_RSA_KEY"
            };
            _rsaKey = new RSACryptoServiceProvider(_cspParams);
            _rsaCryptoProv = new RSACryptoServiceProvider();
            _dsaCryptoProv = new DSACryptoServiceProvider();
            _wsRSACryptoProv = new RSACryptoServiceProvider();
            _securedSOAP = new XmlDocument();
            _soap = new XmlDocument();
            mySettings.idtable = _idTable;
            _rsaCryptoProv.ToXmlString(false);
            mySettings.rsacryptoProv = _rsaCryptoProv.ToXmlString(true);
            mySettings.dsacryptoProv = _dsaCryptoProv.ToXmlString(true);
            mySettings.wsRSAcryptoProv = _wsRSACryptoProv.ToXmlString(false);
            _contentCounter = 0;
            mySettings.securedsoap = CopyXmlToString(_securedSOAP);
            InputString = new XmlDocument();
            _loaded = false;
            _signatureSettings.sigAlg = "1";
        }

        #endregion

        #region Methods

        public bool GetShowSteps()
        {
            return _signatureSettings.showsteps;
        }

        private void SetSignedElements(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable table = ds.Tables[0];
                _signedElements = new string[table.Columns.Count];
            }
        }

        public void AddIdToElement(string element)
        {
            if (element != null)
            {
                if (!_idTable.ContainsKey(element))
                {
                    System.Random randomizer = new Random();
                    int randomNumber = randomizer.Next(100000000);
                    if (!_idTable.ContainsValue(randomNumber))
                    {
                        System.Threading.Thread.Sleep(500);
                        randomNumber = randomizer.Next(100000000);
                    }
                    _idTable.Add(element, randomNumber);
                    mySettings.idtable = _idTable;
                }
            }
        }

        private XmlNode GetElementById(string id)
        {
            XmlNodeList securityHeader = _securedSOAP.GetElementsByTagName("wsse:Security");
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
            XmlNode body = _securedSOAP.GetElementsByTagName("s:Body")[0];
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
            if (_bodySigned)
            {
                list.Add(_securedSOAP.GetElementsByTagName("s:Body")[0]);
            }
            if (_methodNameSigned)
            {
                list.Add(_securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild);
            }
            foreach (XmlNode node in _securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
            {
                if (GetIsSigned(node))
                {
                    list.Add(node);
                }
            }
            if (_secHeaderSigned)
            {
                if (GetIsSigned(_securedSOAP.GetElementsByTagName("wsse:Security")[0]))
                {
                    list.Add(_securedSOAP.GetElementsByTagName("wsse:Security")[0]);
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
            if (_secHeaderEnc && _secHeaderSigned)
            {
                XmlNode[] elementToSign = new XmlNode[0];
                return elementToSign;
            }
            if (_secHeaderEnc)
            {
                XmlNode[] elementToSign = new XmlNode[1];
                elementToSign[0] = _securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return elementToSign;
            }


            ArrayList list = new ArrayList();
            if (!_secHeaderSigned && (_securedSOAP.GetElementsByTagName("wsse:Security").Count > 0))
            {
                list.Add(_securedSOAP.GetElementsByTagName("wsse:Security")[0]);
            }
            XmlNode body = _securedSOAP.GetElementsByTagName("s:Body")[0];
            XmlNode bodysChildNode = body.ChildNodes[0];
            if (!_bodySigned)
            {
                list.Add(body);
                if (!_bodyEncrypted)
                {
                    if (!_methodNameSigned)
                    {
                        list.Add(bodysChildNode);
                        if (!_methodNameEncrypted)
                        {
                            foreach (XmlNode childNode in bodysChildNode.ChildNodes)
                            {
                                bool signed = false;
                                XmlNode[] signedElements = GetSignedElements();
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
            XmlNode header = _securedSOAP.GetElementsByTagName("s:Header")[0];
            if (header != null)
            {
                foreach (XmlNode node in _securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
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
            if (_secHeaderEnc && _secHeaderSigned)
            {
                XmlNode[] elementToEncrypt = new XmlNode[0];
                return elementToEncrypt;
            }
            if (_secHeaderSigned)
            {
                XmlNode[] elementToEncrypt = new XmlNode[1];
                elementToEncrypt[0] = _securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return elementToEncrypt;
            }
            else
            {

                ArrayList list = new ArrayList();
                XmlNode header = _securedSOAP.GetElementsByTagName("s:Header")[0];
                if (header != null)
                {
                    foreach (XmlNode node in _securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
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
                    if (!_bodySigned)
                    {
                        foreach (XmlNode node in body.ChildNodes)
                        {
                            if (!GetHasEncryptedContent(node) && (!node.Name.Equals("xenc:EncryptedData")))
                            {
                                list.Add(node);
                                if (!_methodNameSigned)
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
                        foreach (XmlNode refElem in _securedSOAP.GetElementsByTagName("ds:Reference"))
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
            if (_bodyEncrypted || _bodySigned || _methodNameEncrypted || _methodNameSigned)
            {
                XmlNode[] emptySet = new XmlNode[0];
                return emptySet;
            }
            if (_secHeaderEnc || _secHeaderSigned)
            {
                XmlNode[] emptySet = new XmlNode[0];
                return emptySet;
            }

            if (!_secHeaderEnc)
            {
                foreach (XmlNode parameter in _securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
                {
                    if (!GetIsSigned(parameter))
                    {
                        if (!GetHasEncryptedContent(parameter))
                        {
                            if (!parameter.Name.Equals("xenc:EncryptedData"))
                            {
                                list.Add(parameter);
                            }
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
            foreach (XmlNode node in _securedSOAP.GetElementsByTagName("s:Body")[0].ChildNodes[0].ChildNodes)
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
            return _signatureSettings.sigAlg;
        }

        public void SaveSoap()
        {
            mySettings.securedsoap = CopyXmlToString(_securedSOAP);
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
                SetSignedElements(set);
                _soap = new XmlDocument();
                _node = _soap.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");
                _soap.AppendChild(_node);
                _envelope = _soap.CreateElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
                _soap.AppendChild(_envelope);
                _body = _soap.CreateElement("s", "Body", "http://www.w3.org/2003/05/soap-envelope");
                XmlNode inputNode = _soap.CreateElement("tns", set.Tables[0].ToString(), set.Tables[0].Namespace);
                DataTable table = set.Tables[0];
                foreach (DataColumn tempColumn in table.Columns)
                {
                    XmlNode newElement = _soap.CreateElement("tns", tempColumn.ColumnName, set.Tables[0].Namespace);
                    inputNode.AppendChild(newElement);
                }
                _body.AppendChild(inputNode);
                _envelope.AppendChild(_body);
                stringWriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                _soap.WriteContentTo(xmlTextWriter);
                XmlNode rootElement = _soap.SelectSingleNode("/*");
                presentation.OriginalSoapItem = new System.Windows.Controls.TreeViewItem
                {
                    IsExpanded = true
                };
                StackPanel panel1 = new StackPanel();
                StackPanel origSoapPanel = new StackPanel();
                StackPanel origSoapPanel2 = new StackPanel();
                panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                origSoapPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                origSoapPanel2.Orientation = System.Windows.Controls.Orientation.Horizontal;
                TextBlock elem1 = new TextBlock
                {
                    Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                };
                TextBlock origSoapElem = new TextBlock
                {
                    Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                };
                TextBlock origSoapElem2 = new TextBlock
                {
                    Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                };
                panel1.Children.Insert(0, elem1);
                origSoapPanel.Children.Insert(0, origSoapElem);
                origSoapPanel2.Children.Insert(0, origSoapElem2);
                presentation.OriginalSoapItem.Header = panel1;
                _loaded = false;
                _securedSOAP = (XmlDocument)_soap.Clone();
                mySettings.soapelement = CopyXmlToString(_soap);
                mySettings.securedsoap = CopyXmlToString(_securedSOAP);
                presentation.CopyXmlToTreeView(rootElement, presentation.OriginalSoapItem);
                presentation.treeView.Items.Add(presentation.OriginalSoapItem);
                presentation.treeView.Items.Refresh();
                ShowSecuredSoap();
                _loaded = true;
                InputString = _soap;
                _wsdlLoaded = true;
                mySettings.wsdlloaded = true;
                OnPropertyChanged("OutputString");
            }
        }

        public bool GetIsShowEncryptionsSteps()
        {
            return _encryptionSettings.showsteps;
        }

        public bool GetIsEncryptContent()
        {
            return _encryptionSettings.content;
        }

        public string CopyXmlToString(XmlDocument doc)
        {
            if (doc != null)
            {
                StringWriter stringWriter = new StringWriter();
                doc.Normalize();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
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
            foreach (string signedElement in _signedElements)
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
                foreach (string signedElement in _signedElements)
                {
                    count++;
                    if (signedElement == null)
                    {
                        break;
                    }

                }
                _signedElements[count] = newElement;
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
            XmlNodeList signatureElements = _securedSOAP.GetElementsByTagName("ds:Signature");
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
                                    XmlNode element = GetElementById(id[0]);
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
                _securedSOAP.GetElementsByTagName("wsse:Security")[0].RemoveChild(nodeToDelete);
            }

            if (signArray.Length > 0)
            {
                SignElements(signArray);
            }
            ShowSecuredSoap();
        }

        public void EncryptElements(XmlElement[] elements)
        {
            if (_gotKey)
            {
                bool content = _encryptionSettings.content;
                XmlNode securityHeader = _securedSOAP.GetElementsByTagName("wsse:Security")[0];
                if (securityHeader == null)
                {
                    _hadHeader = false;
                    XmlNode header = _securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2003/05/soap-envelope");
                    string wsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";
                    securityHeader = _securedSOAP.CreateElement("wsse", "Security", wsseNamespace);
                    header.AppendChild(securityHeader);
                    XmlNode envelope = _securedSOAP.GetElementsByTagName("s:Envelope")[0];
                    XmlNode soapBody = _securedSOAP.GetElementsByTagName("s:Body")[0];
                    envelope.InsertBefore(header, soapBody);
                }
                else
                {
                    _hadHeader = true;
                }
                RijndaelManaged sessionKey = new RijndaelManaged
                {
                    KeySize = 256
                };
                EncryptedXml encryptedXML = new EncryptedXml();
                EncryptedKey encryptedKey = new EncryptedKey();
                byte[] encryptedEncryptionKey = EncryptedXml.EncryptKey(sessionKey.Key, _wsRSACryptoProv, false);
                encryptedKey.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
                encryptedKey.CipherData = new CipherData(encryptedEncryptionKey);
                KeyInfoName keyName = new KeyInfoName
                {
                    Value = "Web Service Public Key"
                };
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
                        _lastSessionKey = Convert.ToBase64String(sessionKey.Key);
                        byte[] encryptedElement = encryptedXML.EncryptData(elem, sessionKey, content);
                        EncryptedData encElement = new EncryptedData();
                        DataReference encryptedDataReference = new DataReference();
                        if (!content)
                        {
                            encElement.Type = EncryptedXml.XmlEncElementUrl;
                            encElement.Id = _idTable[elem.Name].ToString();
                            encryptedDataReference.Uri = "#" + _idTable[elem.Name].ToString();
                        }
                        else
                        {
                            encElement.Type = EncryptedXml.XmlEncElementContentUrl;
                            AddIdToElement(_contentCounter + elem.Name);
                            encElement.Id = _idTable[_contentCounter + elem.Name].ToString();
                            encryptedDataReference.Uri = "#" + _idTable[_contentCounter + elem.Name].ToString();
                            _contentCounter++;
                            mySettings.contentcounter = _contentCounter;
                        }

                        encElement.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);
                        encElement.CipherData.CipherValue = encryptedElement;
                        encryptedKey.AddReference(encryptedDataReference);
                        string s = _securedSOAP.GetElementsByTagName(elem.Name)[0].ParentNode.Name;

                        if (!content)
                        {
                            _securedSOAP.GetElementsByTagName(s)[0].ReplaceChild(_securedSOAP.ImportNode(encElement.GetXml(), true), _securedSOAP.GetElementsByTagName(elem.Name)[0]);
                        }
                        else
                        {
                            _securedSOAP.GetElementsByTagName(elem.Name)[0].RemoveAll();
                            _securedSOAP.GetElementsByTagName(elem.Name)[0].AppendChild(_securedSOAP.ImportNode(encElement.GetXml(), true));
                        }
                        if (elem.Name.Equals("s:Body"))
                        {
                            _bodyEncrypted = true;
                            mySettings.bodyencrypted = true;
                        }
                        if (elem.Name.Equals(_soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                        {
                            _methodNameEncrypted = true;
                            mySettings.methodnameencrypted = _methodNameEncrypted;
                        }
                        if (elem.Name.Equals("wsse:Security"))
                        {
                            _secHeaderEnc = true;
                            mySettings.secheaderEnc = true;
                        }
                    }

                }
                securityHeader.InsertBefore(_securedSOAP.ImportNode(encryptedKey.GetXml(), true), securityHeader.ChildNodes[0]);
                PrefixesToEncryptedElement();
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
            XmlNodeList encryptedDataElements = _securedSOAP.GetElementsByTagName("EncryptedData");
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
                AddPrefixesToNodeAndChildNode(prefix, child);
            }
        }

        public void DecryptDocument()
        {
            XmlElement securityHeader = (XmlElement)_securedSOAP.GetElementsByTagName("Security")[0];
            XmlElement encryptedKey = (XmlElement)_securedSOAP.GetElementsByTagName("EncryptedKey")[0];
            XmlElement encryptedData = (XmlElement)_securedSOAP.GetElementsByTagName("EncryptedData")[0];
            XmlElement keyInfo = _securedSOAP.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            securityHeader.RemoveChild(encryptedKey);
            keyInfo.AppendChild(encryptedKey);
            encryptedData.InsertAfter(keyInfo, encryptedData.GetElementsByTagName("EncryptionMethod")[0]);
            ShowSecuredSoap();
            EncryptedXml encXml = new EncryptedXml(_securedSOAP);
            encXml.AddKeyNameMapping("RSA-Key", _rsaKey);
            encXml.DecryptDocument();
        }

        public void SignElements(XmlElement[] elements)
        {
            string sigAlgo = _signatureSettings.sigAlg;
            XmlNode securityHeader = _securedSOAP.GetElementsByTagName("Security")[0];
            if (securityHeader == null)
            {
                XmlNode header = _securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2003/05/soap-envelope");
                string wsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";
                securityHeader = _securedSOAP.CreateElement("Security", wsseNamespace);
                header.AppendChild(securityHeader);
                XmlNode envelope = _securedSOAP.GetElementsByTagName("Envelope")[0];
                XmlNode soapBody = _securedSOAP.GetElementsByTagName("Body")[0];
                envelope.InsertBefore(header, soapBody);
            }
            SignedXml signedXML = new SignedXml(_securedSOAP);
            foreach (XmlElement elementToSign in elements)
            {
                XmlAttribute idAttribute = _securedSOAP.CreateAttribute("Id");
                idAttribute.Value = _idTable[elementToSign.Name].ToString();
                elementToSign.Attributes.Append(idAttribute);
                XmlAttributeCollection attributes = elementToSign.Attributes;
                XmlAttribute id = attributes["Id"];
                Reference reference = new Reference("#" + id.Value);
                //   Reference reference = new Reference("");
                XmlElement xpathElement = _securedSOAP.CreateElement("XPath");
                string Xpath = "ancestor-or-self::Body";
                XmlElement root = _securedSOAP.DocumentElement;
                XmlElement body = (XmlElement)_securedSOAP.GetElementsByTagName("Body")[0];
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(_securedSOAP.NameTable);
                namespaceManager.AddNamespace("s", body.NamespaceURI);
                xpathElement.InnerText = Xpath;
                XmlDsigXPathTransform xpathTrans = new XmlDsigXPathTransform();
                xpathTrans.LoadInnerXml(xpathElement.SelectNodes("."));
                XmlDsigExcC14NTransform transform = new XmlDsigExcC14NTransform();
                reference.AddTransform(transform);
                signedXML.AddReference(reference);
                if (elementToSign.Name.Equals("s:Body"))
                {
                    _bodySigned = true;
                    mySettings.bodysigned = true;
                }
                if (elementToSign.Name.Equals(_soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                {
                    _methodNameSigned = true;
                    mySettings.methodnameSigned = true;
                }

            }
            if (sigAlgo.Equals("1"))
            {
                CspParameters parameter = new CspParameters
                {
                    KeyContainerName = "Container"
                };
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider(parameter);
                signedXML.SigningKey = provider;
                signedXML.ComputeSignature();
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new RSAKeyValue(provider));
                signedXML.KeyInfo = keyInfo;
                Reference reference = (Reference)signedXML.SignedInfo.References[0];
                IEnumerator enumerator = reference.TransformChain.GetEnumerator();
                enumerator.MoveNext();
                XmlElement envelope = (XmlElement)_securedSOAP.GetElementsByTagName("Envelope")[0];
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
            securityHeader.InsertBefore(_securedSOAP.ImportNode(signaturElement, true), securityHeader.ChildNodes[0]);

        }

        public bool CheckSecurityHeader()
        {
            bool securityheader = false;
            XmlNodeList securityElements = _securedSOAP.GetElementsByTagName("wsse:Security");
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
                XmlElement envelope = (XmlElement)_securedSOAP.GetElementsByTagName("s:Envelope")[0];
                XmlElement header = _securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2001/12/soap-envelope");
                XmlElement securityHeader = _securedSOAP.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
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
                xPathValue = xPathValue + "/s:Header" + "/wsse:Security";
                return xPathValue;
            }
            xPathValue = xPathValue + "/s:Body";
            if (element.Name.Equals("s:Body"))
            {
                return xPathValue;
            }
            xPathValue = xPathValue + "/" + _securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name;
            if (element.Name.Equals(_securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name))
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
            XmlElement signature = _securedSOAP.CreateElement("ds", "Signature", dsNamespace);
            XmlElement signedInfo = _securedSOAP.CreateElement("ds", "SignedInfo", dsNamespace);
            XmlElement canonicalizationMethod = _securedSOAP.CreateElement("ds", "CanonicalizationMethod", dsNamespace);
            XmlAttribute canMeth = _securedSOAP.CreateAttribute("Algorithm");
            canMeth.Value = SignedXml.XmlDsigExcC14NTransformUrl;
            canonicalizationMethod.Attributes.Append(canMeth);
            XmlElement signatureMethod = _securedSOAP.CreateElement("ds", "SignatureMethod", dsNamespace);
            XmlAttribute sigMeth = _securedSOAP.CreateAttribute("Algorithm");

            if (_signatureSettings.sigAlg.Equals("0"))
            {
                sigMeth.Value = SignedXml.XmlDsigDSAUrl;
            }
            if (_signatureSettings.sigAlg.Equals("1"))
            {
                sigMeth.Value = SignedXml.XmlDsigRSASHA1Url;
            }

            signatureMethod.Attributes.Append(sigMeth);
            XmlNode securityHeader = _securedSOAP.GetElementsByTagName("wsse:Security")[0];

            signature.AppendChild(signedInfo);
            signedInfo.AppendChild(canonicalizationMethod);
            signedInfo.AppendChild(signatureMethod);

            XmlAttribute foundIdAttribute = null;
            foreach (XmlElement elementToSign in elementsToSign)
            {
                AddIdToElement(elementToSign.Name);

                foreach (XmlAttribute xmlAttribute in elementToSign.Attributes)
                {

                    if (xmlAttribute.Name == "Id")
                    {
                        foundIdAttribute = xmlAttribute;
                        break;
                    }

                }



                XmlAttribute idAttribute = _securedSOAP.CreateAttribute("Id");
                if (foundIdAttribute == null)
                {
                    idAttribute.Value = _idTable[elementToSign.Name].ToString();
                    elementToSign.Attributes.Append(idAttribute);
                }

                XmlElement referenceElement = _securedSOAP.CreateElement("ds", "Reference", dsNamespace);
                XmlAttribute uri = _securedSOAP.CreateAttribute("URI");
                XmlElement transforms = _securedSOAP.CreateElement("ds", "Transforms", dsNamespace);
                referenceElement.AppendChild(transforms);

                if (_signatureSettings.Xpath)
                {
                    uri.Value = "";
                    XmlElement xPathTransform = _securedSOAP.CreateElement("ds", "Transform", dsNamespace);
                    XmlAttribute xPathTransAtt = _securedSOAP.CreateAttribute("Algorithm");
                    xPathTransAtt.Value = SignedXml.XmlDsigXPathTransformUrl;
                    xPathTransform.Attributes.Append(xPathTransAtt);
                    XmlElement xPathValue = _securedSOAP.CreateElement("ds", "XPath", dsNamespace);
                    xPathValue.InnerXml = GetXPathValue(elementToSign);
                    xPathTransform.AppendChild(xPathValue);
                    transforms.AppendChild(xPathTransform);
                }
                else
                {
                    if (foundIdAttribute == null)
                    {
                        uri.Value = "#" + _idTable[elementToSign.Name].ToString();
                    }
                    else
                    {
                        uri.Value = "#" + foundIdAttribute.Value;
                    }
                }
                referenceElement.Attributes.Append(uri);
                XmlElement c14nTransform = _securedSOAP.CreateElement("ds", "Transform", dsNamespace);
                XmlAttribute c14Url = _securedSOAP.CreateAttribute("Algorithm");
                c14Url.Value = SignedXml.XmlDsigEnvelopedSignatureTransformUrl;
                c14nTransform.Attributes.Append(c14Url);
                transforms.AppendChild(c14nTransform);
                XmlElement digestMethod = _securedSOAP.CreateElement("ds", "DigestMethod", dsNamespace);
                XmlAttribute digMethodAttribute = _securedSOAP.CreateAttribute("Algorithm");
                digMethodAttribute.Value = SignedXml.XmlDsigSHA1Url;
                digestMethod.Attributes.Append(digMethodAttribute);
                referenceElement.AppendChild(digestMethod);
                XmlElement digestValue = _securedSOAP.CreateElement("ds", "DigestValue", dsNamespace);
                digestValue.InnerText = Convert.ToBase64String(GetDigestValueForElementWithSha1(elementToSign));
                referenceElement.AppendChild(digestValue);
                signedInfo.AppendChild(referenceElement);
                if (elementToSign.Name.Equals("s:Body"))
                {
                    _bodySigned = true;
                    mySettings.bodysigned = true;
                }
                if (elementToSign.Name.Equals(_soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                {
                    _methodNameSigned = true;
                    mySettings.methodnameSigned = true;
                }
                if (elementToSign.Name.Equals("wsse:Security"))
                {
                    _secHeaderSigned = true;
                    mySettings.secheaderSigned = true;
                }

            }
            XmlElement signatureValue = _securedSOAP.CreateElement("ds", "SignatureValue", dsNamespace);
            KeyInfo keyInfo = new KeyInfo();
            if (_signatureSettings.sigAlg.Equals("1"))
            {
                signatureValue.InnerXml = Convert.ToBase64String(_rsaCryptoProv.SignHash(GetDigestValueForElementWithSha1(signedInfo), CryptoConfig.MapNameToOID("SHA1")));
                keyInfo.AddClause(new RSAKeyValue(_rsaCryptoProv));

            }
            if (_signatureSettings.sigAlg.Equals("0"))
            {
                signatureValue.InnerXml = Convert.ToBase64String(_dsaCryptoProv.SignHash(GetDigestValueForElementWithSha1(signedInfo), CryptoConfig.MapNameToOID("SHA1")));
                keyInfo.AddClause(new DSAKeyValue(_dsaCryptoProv));
            }
            signature.AppendChild(signatureValue);
            XmlElement xmlKeyInfo = keyInfo.GetXml();
            xmlKeyInfo.Prefix = "ds";
            foreach (XmlNode childNode in xmlKeyInfo.ChildNodes)
            {
                childNode.Prefix = "ds";
            }
            signature.AppendChild(_securedSOAP.ImportNode(xmlKeyInfo, true));
            securityHeader.InsertBefore(signature, securityHeader.FirstChild);
            //   XmlElement secHead = (XmlElement)this._securedSOAP.GetElementsByTagName("wsse:Security")[0].Insert(signature);
            mySettings.securedsoap = CopyXmlToString(_securedSOAP);
        }

        public string GetIdToElement(string elementName)
        {
            if (elementName != null && _idTable.ContainsKey(elementName))
            {
                string id = _idTable[elementName].ToString();
                return id;
            }
            return null;
        }

        public bool GetXPathTransForm()
        {
            return _signatureSettings.Xpath;
        }

        public byte[] GetDigestValueForElementWithSha1(XmlElement element)
        {
            //XmlNode node = element.GetElementsByTagName("ds:Signature")[0];
            //element.RemoveChild(node);
            Stream canonicalized = CanonicalizeNodeWithExcC14n(element);
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            StreamReader reader = new StreamReader(canonicalized);

            string canonicalizedString = reader.ReadToEnd();
            canonicalized.Position = 0;

            byte[] byteValue = sha1.ComputeHash(canonicalized);
            return byteValue;
        }

        public Stream CanonicalizeNodeWithExcC14n(XmlElement nodeToCanon)
        {
            XmlNode node = nodeToCanon;
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
            presentation.SecuredSoapItem = null;
            presentation.SecuredSoapItem = new System.Windows.Controls.TreeViewItem
            {
                IsExpanded = true
            };
            StackPanel panel1 = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            TextBlock elem1 = new TextBlock
            {
                Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            };
            panel1.Children.Insert(0, elem1);
            presentation.SecuredSoapItem.Header = panel1;
            XmlNode rootElement = _securedSOAP.SelectSingleNode("/*");
            presentation.CopyXmlToTreeView(rootElement, presentation.SecuredSoapItem);
            presentation.treeView.Items.Add(presentation.SecuredSoapItem);
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public object XmlOutputConverter(object Data)
        {

            if (_securedSOAP != null)
            {

                XmlDocument doc = _securedSOAP;
                StringWriter stringWriter = new StringWriter();
                object obj = new object();
                try
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                    {
                        Formatting = Formatting.Indented
                    };
                    doc.WriteContentTo(xmlTextWriter);
                    obj = stringWriter.ToString();
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;

        }
        public object WsdlConverter(object Data)
        {

            if (_wsdl != null)
            {

                XmlDocument doc = _wsdl;
                StringWriter stringWriter = new StringWriter();
                object obj = new object();
                try
                {
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                    {
                        Formatting = Formatting.Indented
                    };
                    doc.WriteContentTo(xmlTextWriter);
                    obj = stringWriter.ToString();
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;

        }


        public object XMLInputConverter(object Data)
        {
            if (_inputDocument != null)
            {
                object obj = new object();
                try
                {
                    XmlDocument doc = _inputDocument;
                    StringWriter stringWriter = new StringWriter();

                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                    {
                        Formatting = Formatting.Indented
                    };
                    doc.WriteContentTo(xmlTextWriter);
                    obj = stringWriter.ToString();
                }
                catch (Exception)
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
            if (_isExecuting)
            {
                OutputString = _securedSOAP;
                _isExecuting = false;
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
            _isExecuting = false;
            Dispose();
        }

        public void PreExecution()
        {
            _isExecuting = true;
            Dispose();
        }

        public System.Windows.Controls.UserControl Presentation => presentation;

        public ISettings Settings => (SoapSettings)_settings;

        public SoapSettings mySettings => (SoapSettings)_settings;

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
                    _signatureSettings.sigAlg = settings.SignatureAlg;
                    break;

                case "SigXPathRef":
                    _signatureSettings.Xpath = settings.SigXPathRef;
                    break;

                case "SigShowSteps":
                    _signatureSettings.showsteps = settings.SigShowSteps;
                    break;
                case "EncContentRadio":
                    if (settings.EncContentRadio == 0)
                    {
                        _encryptionSettings.content = false;
                    }
                    if (settings.EncContentRadio == 1)
                    {
                        _encryptionSettings.content = true;
                    }
                    break;

                case "EncShowSteps":
                    _encryptionSettings.showsteps = settings.EncShowSteps;
                    break;
                case "gotkey":
                    _gotKey = settings.gotkey;
                    break;
                case "wspublicKey":
                    _wsPublicKey = settings.wspublicKey;
                    break;
                case "dsacryptoProv":
                    _dsaCryptoProv.FromXmlString(settings.dsacryptoProv);
                    break;
                case "rsacryptoProv":
                    _rsaCryptoProv.FromXmlString(settings.rsacryptoProv);
                    break;
                case "wsRSAcryptoProv":
                    _wsRSACryptoProv.FromXmlString(settings.wsRSAcryptoProv);
                    break;
                case "contentcounter":
                    _contentCounter = settings.contentcounter;
                    break;
                case "secheaderSigned":
                    _secHeaderSigned = settings.secheaderSigned;
                    break;
                case "secheaderEnc":
                    _secHeaderEnc = settings.secheaderEnc;
                    break;
                case "methodnameencrypted":
                    _methodNameEncrypted = settings.methodnameencrypted;
                    break;
                case "bodyencrypted":
                    _bodyEncrypted = settings.bodyencrypted;
                    break;
                case "methodnameSigned":
                    _methodNameSigned = settings.methodnameSigned;
                    break;
                case "bodysigned":
                    _bodySigned = settings.bodysigned;
                    break;
                case "idtable":
                    _idTable = settings.idtable;
                    break;
                case "securedsoap":
                    if (settings.securedsoap != null)
                    {
                        if (!_loaded)
                        {
                            _securedSOAP = (CopyStringToXml(settings.securedsoap));
                            ShowSecuredSoap();
                            _loaded = true;
                        }
                        else
                        {
                            _loaded = true;
                        }
                    }
                    break;
                case "soapelement":
                    if (settings.soapelement != null)
                    {
                        _soap = CopyStringToXml(settings.soapelement);
                    }
                    break;
                case "wsdlloaded":
                    _wsdlLoaded = settings.wsdlloaded;
                    break;
                case "sendSoap":
                    if (!send)
                    {
                        OnPropertyChanged("OutputString");
                        send = true;
                    }
                    break;
                case "ResetSoap":
                    if (_soap != null)
                    {
                        mySettings.bodysigned = false;
                        mySettings.secheaderSigned = false;
                        mySettings.secheaderEnc = false;
                        mySettings.methodnameSigned = false;
                        mySettings.methodnameencrypted = false;
                        mySettings.bodyencrypted = false;
                        _securedSOAP = (XmlDocument)_soap.Clone();
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
