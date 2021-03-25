using System;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Windows.Threading;
using System.Threading;

namespace WebService
{
    public class SignatureValidator
    {
        #region Fields

        private XmlDocument _inputDocument;
        private XmlDocument _tempdocument;
        private ArrayList _wsSecurityHeaderList;
        private ArrayList _referenceList;
        private ArrayList _encryptedDataList;
        private ArrayList _decryptedDataList;
        private ArrayList _encryptedKeyElements;
        private XmlElement _reference;
        private WSSecurityTracer _tracer;
        public string _canonicalizedSignedInfo;
        private WebService _webService;
        private SignedXml _signedXml;
        private ArrayList _signatureReferenceList;
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

        public bool Valid
        {
            get
            {
                return this._valid;
            }
        }

        #endregion

        #region Constructor

        public SignatureValidator(WebService webService)
        {
            this._valid = true;
            this._inputDocument = (XmlDocument)webService.InputString.Clone();
            this._canonicalizator = new Canonicalizator(this._inputDocument);
            this._tempdocument = (XmlDocument)this._inputDocument.Clone();
            this._wsSecurityHeaderList = new ArrayList();
            this._encryptedDataList = new ArrayList();
            this._decryptedDataList = new ArrayList();
            this._encryptedKeyElements = new ArrayList();
            this._referenceList = new ArrayList();
            this._webService = webService;
            this._signedXml = new SignedXml(this._inputDocument);
            this._signatureReferenceList = new ArrayList();
            this._securityHeader = this._inputDocument.GetElementsByTagName("wsse:Security")[0];
            if (this._securityHeader != null)
            {
                foreach (XmlElement securityHeader in this._securityHeader)
                {
                    if (securityHeader.Name.Equals("xenc:EncryptedData"))
                    {
                        this.DercryptSingleXmlElement((XmlElement)this._wsSecurityHeaderList[0]);
                        this.FillSecurityHeaderElementsList();
                    }
                    this._wsSecurityHeaderList.Add(securityHeader);
                }
            }

            this._tracer = new WSSecurityTracer();


            foreach (XmlElement tempElement in this._wsSecurityHeaderList)
            {
                if (tempElement.Name.Equals("xenc:EncryptedKey"))
                {
                    try
                    {
                       string decryptedElement = this.DercryptSingleXmlElement(tempElement);
                    }
                    catch (Exception e)
                    {
                        this._webService.ShowError(e.Message);
                        this._valid = false;
                    }
                }
                if (tempElement.Name.Equals("ds:Signature"))
                {
                    this.ValidateSignature(tempElement);
                }

            }
            this._webService.presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this._webService.presentation.txtTrace.Text += this._tracer.signatureTrace;
                this._webService.presentation.txtTrace.Text += this._tracer.decryptionTrace;
            }, null);
            this._webService.ModifiedInputDocument = this._inputDocument;
        }

        #endregion

        #region Methods

        private void FillSecurityHeaderElementsList()
        {
            this._wsSecurityHeaderList.Clear();
            this._securityHeader = this._inputDocument.GetElementsByTagName("wsse:Security")[0];
            foreach (XmlElement tempElement in _securityHeader)
            {
                this._wsSecurityHeaderList.Add(tempElement);
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
            this._referenceList.Clear();
            ArrayList referenceElementList = this.FindXmlElementByURI(uri, this._inputDocument.ChildNodes[1]);
            XmlElement keyInfoElement = this._inputDocument.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            keyInfoElement.AppendChild(encryptedKeyElement);
            if (referenceElementList.Count > 0)
            {
                XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
                RSACryptoServiceProvider provider = this._webService.RSACryptoServiceProvider;
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("root");
                root.AppendChild(doc.ImportNode((XmlNode)encryptedKeyElement, true));
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
                this._tracer.appendDecryptedData(uri, doc.FirstChild.InnerXml);
                EncryptedXml encXml = new EncryptedXml(this._inputDocument);
                encXml.AddKeyNameMapping("Web Service Public Key", provider);
                EncryptedData data = new EncryptedData();
                data.LoadXml((XmlElement)encryptedDataElement);
                SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
                algo.Key = encXml.DecryptEncryptedKey(encryptdKey);
                byte[] t = encXml.DecryptData(data, algo);
                encXml.ReplaceData(encryptedDataElement, t);
                this._encryptedDataList.Add(encryptedDataElement);
                this._decryptedDataList.Add(doc.GetElementsByTagName("root")[0]);
                this._encryptedKeyElements.Add(encryptedKeyElement);
                string decryptedXmlString;
                return decryptedXmlString = Convert.ToBase64String(t);
            }
            return string.Empty;
        }
        public XmlElement DecryptSingleElementByKeyNumber(int encryptedKeyNumber)
        {
            EncryptedKey encryptedKey = new EncryptedKey();
            encryptedKey.LoadXml((XmlElement)this._encryptedKeyElements[encryptedKeyNumber]);
            ReferenceList referenceList = encryptedKey.ReferenceList;
            EncryptedReference encryptedReference = referenceList.Item(0);
            string uri = encryptedReference.Uri;
            KeyInfo keyInfo = encryptedKey.KeyInfo;
            this._referenceList.Clear();
            ArrayList referenceElementList = new ArrayList();
            referenceElementList = this.FindXmlElementByURI(uri, this._tempdocument.ChildNodes[1]);
            XmlElement keyInfoElement = this._tempdocument.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
            keyInfoElement.AppendChild(_tempdocument.ImportNode((XmlNode)encryptedKey.GetXml(), true));
            XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
            RSACryptoServiceProvider provider = this._webService.RSACryptoServiceProvider;
            EncryptedXml encXml = new EncryptedXml(this._tempdocument);
            encXml.AddKeyNameMapping("Web Service Public Key", provider);
            EncryptedData data = new EncryptedData();
            data.LoadXml((XmlElement)encryptedDataElement);
            SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
            algo.Key = encXml.DecryptEncryptedKey(encryptedKey);
            byte[] t = encXml.DecryptData(data, algo);
            encXml.ReplaceData(encryptedDataElement, t);
            this._tempdocument.GetElementsByTagName("wsse:Security")[0].RemoveChild(_tempdocument.GetElementsByTagName("xenc:EncryptedKey")[0]);
            XmlElement root = (XmlElement)this._decryptedDataList[encryptedKeyNumber];
            return (XmlElement)root;
        }
        public bool ValidateSignature(XmlElement signatureElement)
        {
            bool valid = true;
            Signature signature = new Signature();
            signature.LoadXml(signatureElement);
            XmlNodeList signatureList = this._inputDocument.GetElementsByTagName("ds:Signature");
            if (signatureList.Count != 0)
            {
                this._signedXml.LoadXml((XmlElement)signatureElement);
                bool validReference = ValidateReferences(this._signedXml);
                if (validReference)
                {
                    this.CanonicalizeSignedInfo(signature.SignedInfo.GetXml());
                    this._signedXml.LoadXml((XmlElement)signatureElement);
                }
                else
                {
                    this._valid = false;
                }
            }
            return valid;
        }
        public void CanonicalizeSignedInfo(XmlElement SignedInfo)
        {
            Canonicalizator canonicalizator = new Canonicalizator(this._inputDocument);
            Stream stream = canonicalizator.CanonicalizeNode(SignedInfo);
            StreamReader canonicalizedStreamReader = new StreamReader(stream);
            string canonicalizedString = canonicalizedStreamReader.ReadToEnd();
            this._canonicalizedSignedInfo = canonicalizedString;
            this.ValidateSignature(this._signedXml.Signature, this._signedXml.SignatureValue);

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
                    this._referenceList.Clear();
                    SignatureReference sigReference = new SignatureReference();
                    sigReference.nr = singatureReferenceCounter;
                    singatureReferenceCounter++;
                    sigReference.references = new ArrayList();
                    ArrayList newList = new ArrayList();
                    newList = this.FindXmlElementByURI(uri, this._inputDocument.ChildNodes[0].NextSibling);
                    XmlElement referenceElement = (XmlElement)newList[0];
                    XmlElement clonedReferenceElement = (XmlElement)referenceElement.Clone();
                    newList = (ArrayList)this._referenceList.Clone();
                    sigReference.references.Add(clonedReferenceElement);
                    this._signatureReferenceList.Add(sigReference);
                }
                if (uri.Equals(""))
                {
                    XmlNode node = null;
                    SignatureReference sigReference = new SignatureReference();
                    sigReference.nr = singatureReferenceCounter;
                    singatureReferenceCounter++;
                    ArrayList list = new ArrayList();
                    XmlDocument doc = new XmlDocument();
                    Transform trans = reference.TransformChain[0];
                    XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
                    XmlElement xpathElement = xpathTransform.GetXml();
                    string xpath = xpathElement.InnerText;
                    XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(this._inputDocument.NameTable);
                    XmlElement bodyElement = (XmlElement)this._inputDocument.GetElementsByTagName("s:Body")[0];
                    xmlNamespaceManager.AddNamespace("s", bodyElement.NamespaceURI);
                    xmlNamespaceManager.AddNamespace("tns", "http://tempuri.org/");
                    xmlNamespaceManager.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
                    xmlNamespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
              
                    node = this._inputDocument.SelectSingleNode(xpath, xmlNamespaceManager);
                    list.Add((XmlElement)node.Clone());
                    sigReference.references = list;
                    this._signatureReferenceList.Add(sigReference);
                }
                XmlElement referenceTransformed = this.ApplyTransform(reference);
                digest = this.DigestElement(referenceTransformed, hashAlgorithm, "");
                string digestValue = Convert.ToBase64String(digest);
                this._tracer.appendReferenceValidation(uri, digestValue);
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
                this._inputDocument.Save(transformstream);
                transformstream.Position = 0;
            }
            else
            {
                XmlNodeReader reader = new XmlNodeReader((XmlNode)this._reference);
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
                                XmlElement temp = (XmlElement)this._referenceList[j];
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
                                XmlElement temp = (XmlElement)this._referenceList[j];
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
                        XmlNamespaceManager xmlNameSpaceManager = new XmlNamespaceManager(this._inputDocument.NameTable);
                        XmlElement bodyElement = (XmlElement)this._inputDocument.GetElementsByTagName("s:Body")[0];
                        xmlNameSpaceManager.AddNamespace("s", bodyElement.NamespaceURI);
                        xmlNameSpaceManager.AddNamespace("tns", "http://tempuri.org/");
                        xmlNameSpaceManager.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
                        xmlNameSpaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
                        node = this._inputDocument.SelectSingleNode(xpath, xmlNameSpaceManager);
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
                byte[] digestSignedInfo = this.DigestElement(signature.SignedInfo.GetXml(), "", "");
                XmlElement signed = signature.SignedInfo.GetXml();
                string oid = CryptoConfig.MapNameToOID("SHA1");
                valid = rsa.VerifyHash(digestSignedInfo, oid, this._signedXml.SignatureValue);
                
            }
            else
            {
                dsa = new DSACryptoServiceProvider(parameter);
                dsa.FromXmlString(KeyInfoXml.InnerXml);
                byte[] digestSignedInfo = this.DigestElement(signature.SignedInfo.GetXml(), "", "");
                string oid = CryptoConfig.MapNameToOID("SHA1");
                valid = dsa.VerifyHash(digestSignedInfo, oid, this._signedXml.SignatureValue);
            }
            return valid;
        }
        public byte[] DigestElement(XmlElement element, string hashAlgorithm, string canonicalizationAlgorithm)
        {
            Canonicalizator canonicalizator = new Canonicalizator(this._inputDocument);
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
            String identifier = uri;
            string[] splittedUri = new string[2];
            char separators = '#';
            splittedUri = identifier.Split(separators);
            string value = splittedUri[1];

            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                XmlNode childNote = (XmlNode)element.ChildNodes[i];

                if (childNote.HasChildNodes)
                {
                    this.FindXmlElementByURI(identifier, childNote);
                }
                if (childNote.Attributes != null)
                {
                    if (childNote.Attributes["Id"] != null)
                    {
                        if (childNote.Attributes["Id"].Value.Equals(value))
                        {
                            foundElement = (XmlElement)childNote;
                            this._referenceList.Add(foundElement);
                            this._reference = foundElement;
                            return this._referenceList;
                        }
                        if (foundElement != null)
                        {
                            break;
                        }
                    }
                }
            }
            return this._referenceList;
        }
        public string CanonicalizeSignedInfo(int signatureNumber)
        {
            XmlElement signedInfo = (XmlElement)this._inputDocument.GetElementsByTagName("ds:SignedInfo")[signatureNumber];
            this._canonicalizator = new Canonicalizator(this._inputDocument);
            Stream stream = this._canonicalizator.CanonicalizeNode(signedInfo);
            StreamReader sreader = new StreamReader(stream);
            string canonString = sreader.ReadToEnd();
            return canonString;
        }
        public string DigestElement(int signatureNumber, int referenceNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)this._inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            string uri = reference.Uri;
            ArrayList references = this.GetSignatureReferences(signatureNumber);
            byte[] digestedElement = this.DigestElement((XmlElement)references[referenceNumber], "", "");
            string convertedDigest = Convert.ToBase64String(digestedElement);
            return Convert.ToBase64String(digestedElement);

        }
        public string MakeTransforms(int signatureNumber, int referenceNumber, int transformChainNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)this._tempdocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
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
                XmlNamespaceManager manager = new XmlNamespaceManager(this._inputDocument.NameTable);
                XmlElement bodyElement = (XmlElement)this._inputDocument.GetElementsByTagName("s:Body")[0];
                manager.AddNamespace("s", bodyElement.NamespaceURI);
                manager.AddNamespace("tns", "http://tempuri.org/");
                node = this._tempdocument.SelectSingleNode(xpath, manager);
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                XmlElement element = (XmlElement)node;
                element.Normalize();
                element.WriteTo(xmlTextWriter);
                this.transformedElement = element;
                result = stringWriter.ToString();

            }

            if(transform.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform"))
            {

            }
            if (transform.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigExcC14NTransform"))
            {
                Stream stream;
                if (transformedElement != null)
                {
                    stream = this._canonicalizator.CanonicalizeNode(transformedElement);
                    StreamReader sreader = new StreamReader(stream);
                    string canonString = sreader.ReadToEnd();
                    result = canonString;

                }
                else
                {
                    ArrayList references = this.GetSignatureReferences(signatureNumber);
                    XmlElement referenceElement = (XmlElement)references[referenceNumber];
                    stream = this._canonicalizator.CanonicalizeNode(referenceElement);
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
            SignatureReference signatureReference = (SignatureReference)this._signatureReferenceList[signatureNumber];
            return signatureReference.references;
        }
        public string GetSignatureReferenceName(int signatureNumber)
        {
            ArrayList referencedElementList = this.GetSignatureReferences(signatureNumber);
            XmlElement referencedElement = (XmlElement)referencedElementList[0];
            string[] splitter = referencedElement.Name.Split(new Char[] { ':' });
            return splitter[1].ToString();

        }
        public XmlElement GetSignatureReferenceElement(int signatureNumber)
        {
            ArrayList referencedElementList = this.GetSignatureReferences(signatureNumber);
            XmlElement referencedElement = (XmlElement)referencedElementList[0];
            return referencedElement;
        }
        public bool CompareDigestValues(int signatureNumber, int referenceNumber, string digestValue)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)this._inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            return Convert.ToBase64String(reference.DigestValue).Equals(digestValue);
        }
        public int GetTransformsCounter(int signatureNumber, int referenceNumber)
        {
            SignedXml signedXml = new SignedXml();
            signedXml.LoadXml((XmlElement)this._inputDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
            Signature signature = signedXml.Signature;
            Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
            return reference.TransformChain.Count;
        }
        public int GetEncryptedKeyNumber()
        {
            return this._encryptedKeyElements.Count;
        }
        public int GetTotalSecurityElementsNumber()
        {
            return this._wsSecurityHeaderList.Count;
        }
        public string GetWSSecurityHeaderElementName(int i)
        {
            string returnString = "";
            try
            {
                XmlElement securityHeader = (XmlElement)this._wsSecurityHeaderList[i];
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
