using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Xml;

namespace WebService
{
    public class SignatureValidator
    {
        #region Fields

        private readonly XmlDocument _inputDocument;
        private readonly XmlDocument _tempdocument;
        private readonly ArrayList _wsSecurityHeaderList;
        private readonly ArrayList _referenceList;
        private readonly ArrayList _encryptedDataList;
        private readonly ArrayList _decryptedDataList;
        private readonly ArrayList _encryptedKeyElements;
        private XmlElement _reference;
        private readonly WSSecurityTracer _tracer;
        public string _canonicalizedSignedInfo;
        private readonly WebService _webService;
        private readonly SignedXml _signedXml;
        private readonly ArrayList _signatureReferenceList;
        private XmlElement transformedElement;
        private Canonicalizator _canonicalizator;
        private XmlNode _securityHeader;
        private bool _valid;
        private struct SignatureReference
        {
            public int nr;
            public ArrayList references;
        }

        #endregion

        #region Properties

        public bool Valid => _valid;

        #endregion

        #region Constructor

        public SignatureValidator(WebService webService)
        {
            _valid = true;
            _inputDocument = (XmlDocument)webService.InputString.Clone();
            _canonicalizator = new Canonicalizator(_inputDocument);
            _tempdocument = (XmlDocument)_inputDocument.Clone();
            _wsSecurityHeaderList = new ArrayList();
            _encryptedDataList = new ArrayList();
            _decryptedDataList = new ArrayList();
            _encryptedKeyElements = new ArrayList();
            _referenceList = new ArrayList();
            _webService = webService;
            _signedXml = new SignedXml(_inputDocument);
            _signatureReferenceList = new ArrayList();
            _securityHeader = _inputDocument.GetElementsByTagName("wsse:Security")[0];
            if (_securityHeader != null)
            {
                foreach (XmlElement securityHeader in _securityHeader)
                {
                    if (securityHeader.Name.Equals("xenc:EncryptedData"))
                    {
                        DercryptSingleXmlElement((XmlElement)_wsSecurityHeaderList[0]);
                        FillSecurityHeaderElementsList();
                    }
                    _wsSecurityHeaderList.Add(securityHeader);
                }
            }

            _tracer = new WSSecurityTracer();


            foreach (XmlElement tempElement in _wsSecurityHeaderList)
            {
                if (tempElement.Name.Equals("xenc:EncryptedKey"))
                {
                    try
                    {
                        string decryptedElement = DercryptSingleXmlElement(tempElement);
                    }
                    catch (Exception e)
                    {
                        _webService.ShowError(e.Message);
                        _valid = false;
                    }
                }
                if (tempElement.Name.Equals("ds:Signature"))
                {
                    ValidateSignature(tempElement);
                }

            }
            _webService.presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _webService.presentation.txtTrace.Text += _tracer.signatureTrace;
                _webService.presentation.txtTrace.Text += _tracer.decryptionTrace;
            }, null);
            _webService.ModifiedInputDocument = _inputDocument;
        }

        #endregion

        #region Methods

        private void FillSecurityHeaderElementsList()
        {
            _wsSecurityHeaderList.Clear();
            _securityHeader = _inputDocument.GetElementsByTagName("wsse:Security")[0];
            foreach (XmlElement tempElement in _securityHeader)
            {
                _wsSecurityHeaderList.Add(tempElement);
            }
        }
        private string DercryptSingleXmlElement(XmlElement encryptedKeyElement)
        {
            EncryptedKey encryptdKey = new EncryptedKey();
            encryptdKey.LoadXml(encryptedKeyElement);
            ReferenceList referenceList = encryptdKey.ReferenceList;
            EncryptedReference encryptedReference = referenceList.Item(0);
            string uri = encryptedReference.Uri;
            KeyInfo keyInfo = encryptdKey.KeyInfo;
            _referenceList.Clear();
            ArrayList referenceElementList = FindXmlElementByURI(uri, _inputDocument.ChildNodes[1]);
            XmlElement keyInfoElement = _inputDocument.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            keyInfoElement.AppendChild(encryptedKeyElement);
            if (referenceElementList.Count > 0)
            {
                XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
                RSACryptoServiceProvider provider = _webService.RSACryptoServiceProvider;
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("root");
                root.AppendChild(doc.ImportNode(encryptedKeyElement, true));
                root.AppendChild(doc.ImportNode(encryptedDataElement, true));
                doc.AppendChild(root);
                EncryptedXml encxml2 = new EncryptedXml(doc);
                EncryptedKey encKey2 = new EncryptedKey();
                encKey2.LoadXml((XmlElement)doc.GetElementsByTagName("xenc:EncryptedKey")[0]);
                EncryptedData encData2 = new EncryptedData();
                EncryptedData encDataElement2 = new EncryptedData();
                XmlElement data2 = (XmlElement)doc.GetElementsByTagName("xenc:EncryptedData")[0];
                encDataElement2.LoadXml((XmlElement)doc.GetElementsByTagName("xenc:EncryptedData")[0]);
                encxml2.AddKeyNameMapping("Web Service Public Key", provider);
                SymmetricAlgorithm algo2 = SymmetricAlgorithm.Create();
                algo2.Key = encxml2.DecryptEncryptedKey(encKey2);
                byte[] t2 = encxml2.DecryptData(encDataElement2, algo2);
                encxml2.ReplaceData(data2, t2);
                doc.GetElementsByTagName("root")[0].RemoveChild(doc.GetElementsByTagName("xenc:EncryptedKey")[0]);
                _tracer.appendDecryptedData(uri, doc.FirstChild.InnerXml);
                EncryptedXml encXml = new EncryptedXml(_inputDocument);
                encXml.AddKeyNameMapping("Web Service Public Key", provider);
                EncryptedData data = new EncryptedData();
                data.LoadXml(encryptedDataElement);
                SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
                algo.Key = encXml.DecryptEncryptedKey(encryptdKey);
                byte[] t = encXml.DecryptData(data, algo);
                encXml.ReplaceData(encryptedDataElement, t);
                _encryptedDataList.Add(encryptedDataElement);
                _decryptedDataList.Add(doc.GetElementsByTagName("root")[0]);
                _encryptedKeyElements.Add(encryptedKeyElement);
                string decryptedXmlString;
                return decryptedXmlString = Convert.ToBase64String(t);
            }
            return string.Empty;
        }
        public XmlElement DecryptSingleElementByKeyNumber(int encryptedKeyNumber)
        {
            EncryptedKey encryptedKey = new EncryptedKey();
            encryptedKey.LoadXml((XmlElement)_encryptedKeyElements[encryptedKeyNumber]);
            ReferenceList referenceList = encryptedKey.ReferenceList;
            EncryptedReference encryptedReference = referenceList.Item(0);
            string uri = encryptedReference.Uri;
            KeyInfo keyInfo = encryptedKey.KeyInfo;
            _referenceList.Clear();
            ArrayList referenceElementList = new ArrayList();
            referenceElementList = FindXmlElementByURI(uri, _tempdocument.ChildNodes[1]);
            XmlElement keyInfoElement = _tempdocument.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            keyInfoElement.AppendChild(_tempdocument.ImportNode(encryptedKey.GetXml(), true));
            XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
            RSACryptoServiceProvider provider = _webService.RSACryptoServiceProvider;
            EncryptedXml encXml = new EncryptedXml(_tempdocument);
            encXml.AddKeyNameMapping("Web Service Public Key", provider);
            EncryptedData data = new EncryptedData();
            data.LoadXml(encryptedDataElement);
            SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
            algo.Key = encXml.DecryptEncryptedKey(encryptedKey);
            byte[] t = encXml.DecryptData(data, algo);
            encXml.ReplaceData(encryptedDataElement, t);
            _tempdocument.GetElementsByTagName("wsse:Security")[0].RemoveChild(_tempdocument.GetElementsByTagName("xenc:EncryptedKey")[0]);
            XmlElement root = (XmlElement)_decryptedDataList[encryptedKeyNumber];
            return root;
        }
        public bool ValidateSignature(XmlElement signatureElement)
        {
            bool valid = true;
            Signature signature = new Signature();
            signature.LoadXml(signatureElement);
            XmlNodeList signatureList = _inputDocument.GetElementsByTagName("ds:Signature");
            if (signatureList.Count != 0)
            {
                _signedXml.LoadXml(signatureElement);
                bool validReference = ValidateReferences(_signedXml);
                if (validReference)
                {
                    CanonicalizeSignedInfo(signature.SignedInfo.GetXml());
                    _signedXml.LoadXml(signatureElement);
                }
                else
                {
                    _valid = false;
                }
            }
            return valid;
        }
        public void CanonicalizeSignedInfo(XmlElement SignedInfo)
        {
            Canonicalizator canonicalizator = new Canonicalizator(_inputDocument);
            Stream stream = canonicalizator.CanonicalizeNode(SignedInfo);
            StreamReader canonicalizedStreamReader = new StreamReader(stream);
            string canonicalizedString = canonicalizedStreamReader.ReadToEnd();
            _canonicalizedSignedInfo = canonicalizedString;
            ValidateSignature(_signedXml.Signature, _signedXml.SignatureValue);

        }
        public ArrayList GetSignedXmlSignatureReferences()
        {
            return _signedXml.SignedInfo.References;
        }
        public bool ValidateReferences(SignedXml signedXml)
        {
            byte[] digest;
            ArrayList references = signedXml.SignedInfo.References;
            int singatureReferenceCounter = 1;
            foreach (Reference reference in references)
            {
                string uri = reference.Uri;
                string hashAlgorithm = reference.DigestMethod;
                if (!uri.Equals(""))
                {
                    _referenceList.Clear();
                    SignatureReference sigReference = new SignatureReference
                    {
                        nr = singatureReferenceCounter
                    };
                    singatureReferenceCounter++;
                    sigReference.references = new ArrayList();
                    ArrayList newList = new ArrayList();
                    newList = FindXmlElementByURI(uri, _inputDocument.ChildNodes[0].NextSibling);
                    XmlElement referenceElement = (XmlElement)newList[0];
                    XmlElement clonedReferenceElement = (XmlElement)referenceElement.Clone();
                    newList = (ArrayList)_referenceList.Clone();
                    sigReference.references.Add(clonedReferenceElement);
                    _signatureReferenceList.Add(sigReference);
                }
                if (uri.Equals(""))
                {
                    XmlNode node = null;
                    SignatureReference sigReference = new SignatureReference
                    {
                        nr = singatureReferenceCounter
                    };
                    singatureReferenceCounter++;
                    ArrayList list = new ArrayList();
                    XmlDocument doc = new XmlDocument();
                    Transform trans = reference.TransformChain[0];
                    XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
                    XmlElement xpathElement = xpathTransform.GetXml();
                    string xpath = xpathElement.InnerText;
                    XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(_inputDocument.NameTable);
                    XmlElement bodyElement = (XmlElement)_inputDocument.GetElementsByTagName("s:Body")[0];
                    xmlNamespaceManager.AddNamespace("s", bodyElement.NamespaceURI);
                    xmlNamespaceManager.AddNamespace("tns", "http://tempuri.org/");
                    xmlNamespaceManager.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
                    xmlNamespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");

                    node = _inputDocument.SelectSingleNode(xpath, xmlNamespaceManager);
                    list.Add((XmlElement)node.Clone());
                    sigReference.references = list;
                    _signatureReferenceList.Add(sigReference);
                }
                XmlElement referenceTransformed = ApplyTransform(reference);
                digest = DigestElement(referenceTransformed, hashAlgorithm, "");
                string digestValue = Convert.ToBase64String(digest);
                _tracer.appendReferenceValidation(uri, digestValue);
                string convertedDigest = Convert.ToBase64String(reference.DigestValue);
                if (convertedDigest.Equals(digestValue))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public XmlElement ApplyTransform(Reference reference)
        {
            XmlNode node = null;
            TransformChain transformChain = reference.TransformChain;
            int transCounter = transformChain.Count;
            IEnumerator enumerator = transformChain.GetEnumerator();
            Stream transformstream = new MemoryStream();
            if (reference.Uri.Equals(""))
            {
                _inputDocument.Save(transformstream);
                transformstream.Position = 0;
            }
            else
            {
                XmlNodeReader reader = new XmlNodeReader(_reference);
                XmlWriter writer = new XmlTextWriter(transformstream, Encoding.UTF8);
                writer.WriteNode(reader, false);
                writer.Flush();
                transformstream.Position = 0;
            }
            for (int i = 0; i < transCounter; i++)
            {
                enumerator.MoveNext();
                Transform trans = (Transform)enumerator.Current;
                string typ = trans.ToString();
                switch (typ)
                {
                    case "System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform":
                        if (!reference.Uri.Equals(""))
                        {
                            for (int j = 0; j < _referenceList.Count; j++)
                            {
                                XmlElement temp = (XmlElement)_referenceList[j];
                                string uri = "#" + temp.Attributes["Id"].Value;
                                if (uri.Equals(reference.Uri))
                                {
                                    node = temp;
                                }
                            }

                            XmlNode signatureNode = (node as XmlElement).GetElementsByTagName("ds:Signature") != null ? (node as XmlElement).GetElementsByTagName("ds:Signature")[0] as XmlNode : null;
                            if (signatureNode != null)
                            {
                                node.RemoveChild(signatureNode);
                            }
                        }
                        break;


                    case "System.Security.Cryptography.Xml.XmlDsigExcC14NTransform":

                        if (!reference.Uri.Equals(""))
                        {
                            for (int j = 0; j < _referenceList.Count; j++)
                            {
                                XmlElement temp = (XmlElement)_referenceList[j];
                                string uri = "#" + temp.Attributes["Id"].Value;
                                if (uri.Equals(reference.Uri))
                                {
                                    node = temp;
                                }
                            }

                        }
                        break;

                    case SignedXml.XmlDsigEnvelopedSignatureTransformUrl:
                        {

                        }
                        break;
                    case "System.Security.Cryptography.Xml.XmlDsigXPathTransform":
                        XmlDocument doc = new XmlDocument();
                        XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
                        XmlElement xpathElement = xpathTransform.GetXml();
                        string xpath = xpathElement.InnerText;
                        XmlNamespaceManager xmlNameSpaceManager = new XmlNamespaceManager(_inputDocument.NameTable);
                        XmlElement bodyElement = (XmlElement)_inputDocument.GetElementsByTagName("s:Body")[0];
                        xmlNameSpaceManager.AddNamespace("s", bodyElement.NamespaceURI);
                        xmlNameSpaceManager.AddNamespace("tns", "http://tempuri.org/");
                        xmlNameSpaceManager.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
                        xmlNameSpaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
                        node = _inputDocument.SelectSingleNode(xpath, xmlNameSpaceManager);
                        break;
                }

            }
            return (XmlElement)node;

        }
        public bool ValidateSignature(Signature signature, byte[] bytes)
        {
            bool valid = false;
            KeyInfo keyInfo = signature.KeyInfo;
            CspParameters parameter = new CspParameters();
            RSACryptoServiceProvider rsa;
            DSACryptoServiceProvider dsa;
            XmlElement KeyInfoXml = keyInfo.GetXml();
            Type type = keyInfo.GetType();
            if (KeyInfoXml.FirstChild.FirstChild.Name.Equals("RSAKeyValue"))
            {
                rsa = new RSACryptoServiceProvider(parameter);
                rsa.FromXmlString(keyInfo.GetXml().InnerXml);
                RSAParameters param = rsa.ExportParameters(false);
                byte[] digestSignedInfo = DigestElement(signature.SignedInfo.GetXml(), "", "");
                XmlElement signed = signature.SignedInfo.GetXml();
                string oid = CryptoConfig.MapNameToOID("SHA1");
                valid = rsa.VerifyHash(digestSignedInfo, oid, _signedXml.SignatureValue);

            }
            else
            {
                dsa = new DSACryptoServiceProvider(parameter);
                dsa.FromXmlString(KeyInfoXml.InnerXml);
                byte[] digestSignedInfo = DigestElement(signature.SignedInfo.GetXml(), "", "");
                string oid = CryptoConfig.MapNameToOID("SHA1");
                valid = dsa.VerifyHash(digestSignedInfo, oid, _signedXml.SignatureValue);
            }
            return valid;
        }
        public byte[] DigestElement(XmlElement element, string hashAlgorithm, string canonicalizationAlgorithm)
        {
            Canonicalizator canonicalizator = new Canonicalizator(_inputDocument);
            Stream canonicalStream = canonicalizator.CanonicalizeNode(element);
            canonicalStream.Position = 0;
            StreamReader canonicalStreamReader = new StreamReader(canonicalStream);
            string canonString = canonicalStreamReader.ReadToEnd();
            SHA1CryptoServiceProvider sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
            canonicalStream.Position = 0;
            byte[] hash = sha1CryptoServiceProvider.ComputeHash(canonicalStream);
            string base64ConvertedHashValue = Convert.ToBase64String(hash);
            return hash;
        }
        public ArrayList FindXmlElementByURI(string uri, XmlNode element)
        {
            XmlElement foundElement = null;
            string identifier = uri;
            string[] splittedUri = new string[2];
            char separators = '#';
            splittedUri = identifier.Split(separators);
            string value = splittedUri[1];

            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                XmlNode childNote = element.ChildNodes[i];

                if (childNote.HasChildNodes)
                {
                    FindXmlElementByURI(identifier, childNote);
                }
                if (childNote.Attributes != null)
                {
                    if (childNote.Attributes["Id"] != null)
                    {
                        if (childNote.Attributes["Id"].Value.Equals(value))
                        {
                            foundElement = (XmlElement)childNote;
                            _referenceList.Add(foundElement);
                            _reference = foundElement;
                            return _referenceList;
                        }
                        if (foundElement != null)
                        {
                            break;
                        }
                    }
                }
            }
            return _referenceList;
        }
        public string CanonicalizeSignedInfo(int signatureNumber)
        {
            XmlElement signedInfo = (XmlElement)_inputDocument.GetElementsByTagName("ds:SignedInfo")[signatureNumber];
            _canonicalizator = new Canonicalizator(_inputDocument);
            Stream stream = _canonicalizator.CanonicalizeNode(signedInfo);
            StreamReader sreader = new StreamReader(stream);
            string canonString = sreader.ReadToEnd();
            return canonString;
        }
        public string DigestElement(int signatureNumber, int referenceNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)_inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            string uri = reference.Uri;
            ArrayList references = GetSignatureReferences(signatureNumber);
            byte[] digestedElement = DigestElement((XmlElement)references[referenceNumber], "", "");
            string convertedDigest = Convert.ToBase64String(digestedElement);
            return Convert.ToBase64String(digestedElement);

        }
        public string MakeTransforms(int signatureNumber, int referenceNumber, int transformChainNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)_tempdocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            Transform transform = reference.TransformChain[transformChainNumber];
            string result = "";
            if (transform.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigXPathTransform"))
            {
                XmlNode node = null;
                XmlDocument doc = new XmlDocument();
                XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)transform;
                XmlElement xpathElement = xpathTransform.GetXml();
                string xpath = xpathElement.InnerText;
                XmlNamespaceManager manager = new XmlNamespaceManager(_inputDocument.NameTable);
                XmlElement bodyElement = (XmlElement)_inputDocument.GetElementsByTagName("s:Body")[0];
                manager.AddNamespace("s", bodyElement.NamespaceURI);
                manager.AddNamespace("tns", "http://tempuri.org/");
                node = _tempdocument.SelectSingleNode(xpath, manager);
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                XmlElement element = (XmlElement)node;
                element.Normalize();
                element.WriteTo(xmlTextWriter);
                transformedElement = element;
                result = stringWriter.ToString();

            }

            if (transform.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform"))
            {

            }
            if (transform.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigExcC14NTransform"))
            {
                Stream stream;
                if (transformedElement != null)
                {
                    stream = _canonicalizator.CanonicalizeNode(transformedElement);
                    StreamReader sreader = new StreamReader(stream);
                    string canonString = sreader.ReadToEnd();
                    result = canonString;

                }
                else
                {
                    ArrayList references = GetSignatureReferences(signatureNumber);
                    XmlElement referenceElement = (XmlElement)references[referenceNumber];
                    stream = _canonicalizator.CanonicalizeNode(referenceElement);
                    StreamReader sreader = new StreamReader(stream);
                    string canonString = sreader.ReadToEnd();
                    result = canonString;
                }
            }


            return result;
        }
        public int GetReferenceNumber(int signatureNumber)
        {
            return GetSignatureReferences(signatureNumber).Count;
        }
        public ArrayList GetSignatureReferences(int signatureNumber)
        {
            SignatureReference signatureReference = (SignatureReference)_signatureReferenceList[signatureNumber];
            return signatureReference.references;
        }
        public string GetSignatureReferenceName(int signatureNumber)
        {
            ArrayList referencedElementList = GetSignatureReferences(signatureNumber);
            XmlElement referencedElement = (XmlElement)referencedElementList[0];
            string[] splitter = referencedElement.Name.Split(new char[] { ':' });
            return splitter[1].ToString();

        }
        public XmlElement GetSignatureReferenceElement(int signatureNumber)
        {
            ArrayList referencedElementList = GetSignatureReferences(signatureNumber);
            XmlElement referencedElement = (XmlElement)referencedElementList[0];
            return referencedElement;
        }
        public bool CompareDigestValues(int signatureNumber, int referenceNumber, string digestValue)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)_inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            return Convert.ToBase64String(reference.DigestValue).Equals(digestValue);
        }
        public int GetTransformsCounter(int signatureNumber, int referenceNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)_inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            return reference.TransformChain.Count;
        }
        public int GetEncryptedKeyNumber()
        {
            return _encryptedKeyElements.Count;
        }
        public int GetTotalSecurityElementsNumber()
        {
            return _wsSecurityHeaderList.Count;
        }
        public string GetWSSecurityHeaderElementName(int i)
        {
            string returnString = "";
            try
            {
                XmlElement securityHeader = (XmlElement)_wsSecurityHeaderList[i];
                returnString = securityHeader.Name;
            }
            catch
            {
                returnString = "null";
            }
            return returnString;
        }
        #endregion

    }

}
