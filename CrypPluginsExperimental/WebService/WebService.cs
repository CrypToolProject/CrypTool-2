using System;
using System.Collections.Generic;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Xml;
using System.Web.Services.Description;
using System.CodeDom.Compiler;


using Microsoft.CSharp;

using System.Security.Cryptography.Xml;

using System.Security.Cryptography;
using System.Data;
using System.IO;
using System.Xml.Schema;
using System.Collections;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;


namespace WebService
{
    [Author("Tim Podeszwa", "tim.podeszwa@student.uni-siegen.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("WebService.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "WebService/webservice.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class WebService : ICrypComponent
    {
        #region Fields

        private ISettings _settings = new WebServiceSettings();
        public WebServicePresentation presentation;
        private XmlDocument _inputDocument;
        private XmlDocument _outputDocument;
        private XmlDocument _modifiedInputDocument;
        private XmlDocument _wsdlDocument;
        private XmlDocument _soapResponse;
        private Object _service;
        private string[] _wsdlMethod;
        private ServiceDescription _serviceDescription;
        private XmlNode _node;
        private XmlNode _envelope;
        private XmlNode _body;
        private string[] _stringToCompile = new string[5];
        private DataSet _set;
        private string _inputParameter = "";
        private string _inputParameterString = "";
        private string[] _returnParameter = new string[5];
        private string _methodName = "";
        private SignatureValidator _validator;
        private string _publickey;
        private RSACryptoServiceProvider _rsaCryptoServiceProvider;

        #endregion

        #region Properties

        public XmlDocument ModifiedInputDocument
        {
            get
            {
                return this._modifiedInputDocument;
            }
            set
            {
                this._modifiedInputDocument = value;
            }
        }

        public ServiceDescription ServiceDescription
        {
            get
            {
                return this._serviceDescription;
            }
        }

        public RSACryptoServiceProvider RSACryptoServiceProvider
        {
            get
            {
                return this._rsaCryptoServiceProvider;
            }
        }

        public SignatureValidator Validator
        {
            get
            {
                return this._validator;
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

        [PropertyInfo(Direction.OutputData, "WsdlCaption", "WsdlTooltip", false)]
        public XmlDocument Wsdl
        {
            get { return this._wsdlDocument; }
            set
            {
                this._wsdlDocument = value;
                OnPropertyChanged("Wsdl");
            }
        }

        [PropertyInfo(Direction.OutputData, "PublicKeyCaption", "PublicKeyTooltip")]
        public string PublicKey
        {
            get { return this._publickey; }
            set
            {
                this._publickey = value;
                OnPropertyChanged("PublicKey");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public XmlDocument OutputString
        {
            get { return this._outputDocument; }
            set
            {
                this._outputDocument = value;
                OnPropertyChanged("OutputString");
            }
        }

        public ServiceDescription Description
        {
            get { return this._serviceDescription; }
            set { this._serviceDescription = value; }
        }

        public WebServiceSettings WebServiceSettings
        {
            get { return (WebServiceSettings)this._settings; }
            set { this._settings = value; }
        }

        public Object XmlOutputConverter(Object Data)
        {
            XmlDocument doc = (XmlDocument)this._outputDocument;
            StringWriter stringWriter = new StringWriter();
            Object obj = new Object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmlWriter);
                obj = (Object)stringWriter.ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());

            }


            return obj;
        }
        public Object XmlInputConverter(Object Data)
        {
            XmlDocument doc = (XmlDocument)this._inputDocument;
            StringWriter stringWriter = new StringWriter();
            Object obj = new Object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmlWriter);
                obj = (Object)stringWriter.ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());

            }


            return obj;
        }

        public Object WsdlConverter(Object Data)
        {
           
            if (this._wsdlDocument != null)
            {
                XmlDocument doc = (XmlDocument)this._wsdlDocument;
                StringWriter stringWriter = new StringWriter();
                Object obj = new Object();
                try
                {
                    XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                    xmlWriter.Formatting = Formatting.Indented;
                    doc.WriteContentTo(xmlWriter);
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

        #region Constructor

        public WebService()
        {
           
            
            this._wsdlDocument = new XmlDocument();
            this._modifiedInputDocument = new XmlDocument();
            this._wsdlMethod = new string[1];
            this._wsdlMethod[0] = "\n" + @"   
            public ServiceDescription getWsdl(){
            ServiceDescription s1;
            ServiceDescriptionReflector serviceDescriptionReflector = new ServiceDescriptionReflector();
            serviceDescriptionReflector.Reflect(typeof(Service), null);
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            serviceDescriptionReflector.ServiceDescriptions[0].Write(stringWriter);
            s1 = serviceDescriptionReflector.ServiceDescriptions[0];
            XmlSchema schema = s1.Types.Schemas[0];
            string theWsdl = stringWriter.ToString();
            return s1;
            }}";

            this._stringToCompile[0] = @" using System;
            using System.Web;
            using System.Web.Services;
            using System.Web.Services.Protocols;
            using System.Web.Services.Description;
            using System.Xml;
            using System.Xml.Schema;
            using System.IO;";
            this._stringToCompile[1] = @"
            
            public class Service : System.Web.Services.WebService
            {
              public Service()
             {
     
              }";
            this._stringToCompile[2] = @"[WebMethod]";
            this.PropertyChanged += new PropertyChangedEventHandler(WebServicePropertyChangedEventHandler);
            this.presentation = new WebServicePresentation(this);
            this._settings.PropertyChanged += new PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            this.WebServiceSettings.Test = 1;
            this.WebServiceSettings.Integer = 1;
            this.WebServiceSettings.MethodName = "methodName";
            CspParameters parameters = new CspParameters();
            parameters.KeyContainerName = "Container";
            this._rsaCryptoServiceProvider = new RSACryptoServiceProvider(parameters);

        }

        #endregion

        #region EventHandlers

        private void WebServicePropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputString")
            {
                this.CheckSoap();
             
                if (this._inputDocument != null)
                {
                    SoapProtocolImporter t = new SoapProtocolImporter();
                  
                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        
                        this.presentation.NameSpacesTable.Clear();
                        
                        this.presentation.SoapInputItem = null;
                        this.presentation.SoapInputItem = new TreeViewItem();
                        this.presentation._animationTreeView.Items.Clear();
                        presentation.SoapInputItem.IsExpanded = true;
                        StackPanel panel1 = new StackPanel();
                        panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                        TextBlock elem1 = new TextBlock();
                        elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
                        panel1.Children.Insert(0, elem1);
                        this.presentation.SoapInputItem.Header = panel1;
                        this.presentation.CopyXmlToTreeView(this._inputDocument.SelectSingleNode("/*"), this.presentation.SoapInputItem);
                     //   this.presentation.ResetSoapInputItem();
                        this.presentation._animationTreeView.Items.Add(this.presentation.SoapInputItem);
                        this.presentation.FindTreeViewItem(presentation.SoapInputItem, "Envelope", 1).IsExpanded = true;
                        this.presentation.FindTreeViewItem(presentation.SoapInputItem, "Header", 1).IsExpanded = true;
                        this.presentation.FindTreeViewItem(presentation.SoapInputItem, "Security", 1).IsExpanded = true;
                        this.presentation.FindTreeViewItem(presentation.SoapInputItem, "Signature", 1).IsExpanded = true;
                        this.presentation.FindTreeViewItem(presentation.SoapInputItem, "Body", 1).IsExpanded = true;
                        this.presentation._animationTreeView.Items.Refresh();
                       

                    }, null);

                }
            }
        }
        private void SettingsPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            WebServiceSettings webserviceSettings = sender as WebServiceSettings;
            if (e.PropertyName.Equals("createKey"))
            {
                this.CreateRSAKey();

            }
            if (e.PropertyName.Equals("publishKey"))
            {
                this.PublicKey = this.ExportPublicKey();
            }
            if (e.PropertyName.Equals("exportWSDL"))
            {
                this.Wsdl = this._wsdlDocument;
            }
            if (e.PropertyName.Equals("MethodenStub"))
            {
                this.WebServiceSettings.Integer = 1;
                this.WebServiceSettings.String = 0;
                this.WebServiceSettings.Test = 4;
                this.WebServiceSettings.Double = 2;
                this.WebServiceSettings.MethodName = "berechneVerzinsung";

                if (presentation.textBlock1.Inlines != null)
                {
                    presentation.textBlock1.Inlines.Clear();
                }

                this.presentation.textBlock1.Inlines.Add(new Bold(new Run(_stringToCompile[2].ToString() + "\n")));
                this.presentation.VisualMethodName(WebServiceSettings.MethodName);
                this.presentation.VisualReturnParam("double");
                this.presentation.richTextBox1.Document.Blocks.Clear();
                this.presentation.richTextBox1.AppendText("double endKapital;\n" + "int laufzeit=intparam1;\n" + "double zinssatz=doubleparam1;\n" + "double startKapital=doubleparam2;\n" + "endKapital=Math.Round(startKapital*(Math.Pow(1+zinssatz/100,laufzeit)));\n" + "return endKapital;");
            }

            else
            {
                if (e.PropertyName.Equals("Test"))
                {
                    this._returnParameter[0] = "void";
                    this._returnParameter[1] = "int";
                    this._returnParameter[2] = "string";
                    this._returnParameter[3] = "float";
                    this._returnParameter[4] = "double";
                    this.presentation.VisualReturnParam(_returnParameter[webserviceSettings.Test]);
                }

                if (e.PropertyName.Equals("Integer"))
                {
                    if (webserviceSettings.Integer == 1)
                    {
                        this.presentation.VisualParam("int", 1);
                    }
                    if (webserviceSettings.Integer == 2)
                    {
                        this.presentation.VisualParam("int", 2);
                    }
                    if (webserviceSettings.Integer == 0)
                    {
                        this.presentation.VisualParam("int", 0);
                    }

                }
                if (e.PropertyName.Equals("String"))
                {
                    if (webserviceSettings.String == 1)
                    {
                        this.presentation.VisualParam("string", 1);
                    }
                    if (webserviceSettings.String == 2)
                    {

                        this.presentation.VisualParam("string", 2);
                    }

                    if (webserviceSettings.String == 0)
                    {
                        this.presentation.VisualParam("string", 0);
                    }
                }
                if (e.PropertyName.Equals("Double"))
                {
                    if (webserviceSettings.Double == 1)
                    {
                        this.presentation.VisualParam("double", 1);
                    }
                    if (webserviceSettings.Double == 2)
                    {

                        this.presentation.VisualParam("double", 2);
                    }

                    if (webserviceSettings.Double == 0)
                    {
                        this.presentation.VisualParam("double", 0);
                    }
                }


                if (e.PropertyName.Equals("MethodName"))
                {
                    this._methodName = webserviceSettings.MethodName;
                    this.presentation.VisualMethodName(webserviceSettings.MethodName);
                }
                string comma = "";
                if (!this._inputParameter.Equals(""))
                {
                    comma = ",";
                }
                if (this._inputParameterString.Equals(""))
                {
                    comma = "";
                }
                this._stringToCompile[3] = @"public" + " " + this._returnParameter[webserviceSettings.Test] + " " + this._methodName + "(" + "" + this._inputParameter + comma + this._inputParameterString + ")\n{";
                StringBuilder code = new StringBuilder();

                code.Append(_stringToCompile[0]);
                code.Append(_stringToCompile[1]);
                code.Append(_stringToCompile[2]);
                code.Append(_stringToCompile[3]);
            }
        }

        #endregion

        #region Methods

        private void CreateRSAKey()
        {
            CspParameters parameters = new CspParameters();
            parameters.KeyContainerName = "Container";
            this._rsaCryptoServiceProvider = new RSACryptoServiceProvider(parameters);
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Key pair created", this, NotificationLevel.Info));
        }
        private string ExportPublicKey()
        {
            if (this._rsaCryptoServiceProvider != null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Public key exported", this, NotificationLevel.Info));
                return this._rsaCryptoServiceProvider.ToXmlString(false);
            }
            return "";
        }
        public int GetSignatureNumber()
        {
            return this._inputDocument.GetElementsByTagName("ds:Signature").Count;

        }
        public bool CheckSignature()
        {
            XmlNodeList signatureElements = this._inputDocument.GetElementsByTagName("Signature");
            XmlElement signaturElement = (XmlElement)signatureElements.Item(0);
            SignedXml signedXml = new SignedXml(this._inputDocument);
            signedXml.LoadXml(signaturElement);
            return signedXml.CheckSignature(); ;
        }
        private void ReadWebServiceDescription()
        {
            this._set = new DataSet();
            XmlSchema paramsSchema = this._serviceDescription.Types.Schemas[0];
            StringWriter schemaStringWriter = new StringWriter();
            paramsSchema.Write(schemaStringWriter);
            StringReader sreader = new StringReader(schemaStringWriter.ToString());
            XmlTextReader xmlreader = new XmlTextReader(sreader);
            this._set.ReadXmlSchema(xmlreader);

        }
        private bool Compiled()
        {
            if (this._serviceDescription == null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("There is no Web Service Description available", this, NotificationLevel.Error));
                return false;
            }
            return true;
        }
        private void CheckSoap()
        {
            bool signatureValid = true;
            this.Compiled();
            if (this._inputDocument == null)
                return;
            if (this._inputDocument.GetElementsByTagName("ds:Signature") != null)
            {
                this._validator = new SignatureValidator(this);
            }
            signatureValid = this._validator.Valid;
            object invokedObject = new object();
            object[] parameters = null;
            string response;
            if (this._serviceDescription == null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("There is no Web Service Description available", this, NotificationLevel.Error));
            }
            else
            {
                if (!signatureValid)
                {
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Signature validation failed", this, NotificationLevel.Error));
                    goto Abort;
                }

                Types types = this._serviceDescription.Types;
                PortTypeCollection portTypes = this._serviceDescription.PortTypes;
                MessageCollection messages = this._serviceDescription.Messages;
                PortType porttype = portTypes[0];
                Operation operation = porttype.Operations[0];
                OperationOutput output = operation.Messages[0].Operation.Messages.Output;
                OperationInput input = operation.Messages[0].Operation.Messages.Input;
            Message messageOutput = messages[output.Message.Name];
                Message messageInput = messages[input.Message.Name];
                MessagePart messageOutputPart = messageOutput.Parts[0];
                MessagePart messageInputPart = messageInput.Parts[0];
                XmlSchema xmlSchema = types.Schemas[0];
                XmlSchemaElement outputSchema = (XmlSchemaElement)xmlSchema.Elements[messageOutputPart.Element];
                XmlSchemaElement inputSchema = (XmlSchemaElement)xmlSchema.Elements[messageInputPart.Element];
                XmlSchemaComplexType complexTypeOutput = (XmlSchemaComplexType)outputSchema.SchemaType;
                XmlSchemaSequence sequenzTypeOutput = (XmlSchemaSequence)complexTypeOutput.Particle;
                XmlSchemaComplexType complexTypeInput = (XmlSchemaComplexType)inputSchema.SchemaType;
                XmlSchemaSequence sequenzTypeInput = (XmlSchemaSequence)complexTypeInput.Particle;
                Hashtable paramTypesTable = new Hashtable();
                StringWriter xmlStringWriter = new StringWriter();
                xmlSchema.Write(xmlStringWriter);
                this._set = new DataSet();
                StringReader sreader = new StringReader(xmlStringWriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                this._set.ReadXmlSchema(xmlreader);
                if (sequenzTypeInput != null)
                {
                    foreach (XmlSchemaElement inputParam in sequenzTypeInput.Items)
                    {
                        XmlQualifiedName schemaName = inputParam.SchemaTypeName;
                        paramTypesTable.Add(inputParam.QualifiedName.Name, schemaName.Name);
                    }

                    XmlNamespaceManager manager = new XmlNamespaceManager(this._modifiedInputDocument.NameTable);
                    XmlElement body = (XmlElement)this._inputDocument.GetElementsByTagName("s:Body")[0];
                    manager.AddNamespace("s", body.NamespaceURI);
                    manager.AddNamespace("tns", "http://tempuri.org/");
                    XmlNode node = this._modifiedInputDocument.SelectSingleNode("s:Envelope/s:Body/" + "tns:" + this._set.Tables[0].TableName, manager);
                    XmlElement ele = (XmlElement)node;
                    int paramCounter = new Int32();
                    try
                    {
                        paramCounter = ele.ChildNodes.Count;
                    }

                    catch
                    {
                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Some params are missing", this, NotificationLevel.Error));
                    }
                    if (paramCounter != 0)
                    {
                        parameters = new Object[paramCounter];

                        for (int i = 0; i < paramCounter; i++)
                        {
                            string param = ele.ChildNodes[i].InnerText;
                            Object paramType = paramTypesTable[ele.ChildNodes[i].LocalName];
                            if (paramType.ToString().Equals("int"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        parameters[i] = Convert.ToInt32((Object)ele.ChildNodes[i].InnerText);
                                    }
                                    catch (Exception e)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                        goto Abort;
                                    }
                                }
                                else
                                {
                                    if (ele.ChildNodes[i].Name != null)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Parameter " +ele.ChildNodes[i].Name+ " is missing" , this, NotificationLevel.Error));
                                    }
                                        goto Abort;

                                }
                            }
                            if (paramType.ToString().Equals("string"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        parameters[i] = Convert.ToString((Object)ele.ChildNodes[i].InnerText);
                                    }
                                    catch (Exception e)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                        goto Abort;
                                    }
                                }
                                else
                                {
                                    if (ele.ChildNodes[i].Name != null)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Parameter " + ele.ChildNodes[i].Name + " is missing", this, NotificationLevel.Error));
                                    }
                                    goto Abort;

                                }
                            }
                            if (paramType.ToString().Equals("double"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        parameters[i] = Convert.ToDouble((Object)ele.ChildNodes[i].InnerText);
                                    }
                                    catch (Exception e)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                        goto Abort;
                                    }
                                }
                                else
                                {
                                    if (ele.ChildNodes[i].Name != null)
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Parameter " + ele.ChildNodes[i].Name + " is missing", this, NotificationLevel.Error));
                                    }
                                    goto Abort;

                                }
                            }

                        }
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i] == null)
                            {
                                goto Abort;

                            }
                        }
                        try
                        {
                            Type typ = this._service.GetType().GetMethod(operation.Name).ReturnType;
                            string returnType = typ.ToString();
                            if (!returnType.Equals("System.Void"))
                            {
                                invokedObject = this._service.GetType().GetMethod(operation.Name).Invoke(this._service, parameters).ToString();
                            }
                            else { this._service.GetType().GetMethod(operation.Name).Invoke(this._service, parameters).ToString(); }
                        }
                        catch (Exception e)
                        {
                            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                            goto Abort;
                        }
                    }
                    else
                    {
                        if (sequenzTypeOutput != null)
                        {
                            try
                            {
                                invokedObject = this._service.GetType().GetMethod(operation.Name).Invoke(this._service, null).ToString();
                            }
                            catch (Exception e)
                            {

                                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                goto Abort;

                            }
                        }
                        else { this._service.GetType().GetMethod(operation.Name).Invoke(this._service, parameters); }
                    }
                    response = invokedObject.ToString();
                    this.CreateResponse(response);
                }
            }
        Abort: ;
        }
        private void CreateResponse(string response)
        {
            this._soapResponse = new XmlDocument();
            this._node = this._soapResponse.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");
            this._soapResponse.AppendChild(_node);
            this._envelope = this._soapResponse.CreateElement("Envelope", "http://www.w3.org/2001/12/soap-envelope");
            this._soapResponse.AppendChild(_envelope);
            this._body = this._soapResponse.CreateElement("Body", "http://www.w3.org/2001/12/soap-envelope");
            XmlNode input = this._soapResponse.CreateElement(this._set.Tables[1].ToString(), this._set.Tables[1].Namespace);
            DataTable table = this._set.Tables[1];
            foreach (DataColumn tempColumn in table.Columns)
            {
                XmlNode neu = this._soapResponse.CreateElement(tempColumn.ColumnName, this._set.Tables[1].Namespace);
                neu.InnerText = response;
                input.AppendChild(neu);
            }
            this._body.AppendChild(input);
            this._envelope.AppendChild(this._body);
            this.OutputString = this._soapResponse;
        }
        public ArrayList GetSignatureReferences(int i)
        {
            return this._validator.GetSignatureReferences(i);
        }
        public ArrayList GetSignedXmlSignatureReferences()
        {
            return this._validator.GetSignedXmlSignatureReferences();
        }
        public void Compile(string code)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v3.5" } });
            string header = "";
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                header = this.presentation.CopyTextBlockContentToString(this.presentation.textBlock1);

            }, null);
           // codeProvider.CreateGenerator();
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Web.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Web.Services.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Xml.dll");
            CompilerResults compilerResults = null;
            compilerParameters.GenerateExecutable = false;

            try
            {
                string wsdl = this._wsdlMethod[0];
                compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, _stringToCompile[0].ToString() + _stringToCompile[1].ToString() + code + wsdl);
                System.Reflection.Assembly compiledAssembly = compilerResults.CompiledAssembly;
                this._service = compiledAssembly.CreateInstance("Service");
                object invokedObject = this._service.GetType().InvokeMember("getWsdl", System.Reflection.BindingFlags.InvokeMethod, null, this._service, null);
                ServiceDescription description = (ServiceDescription)invokedObject;
                System.IO.StringWriter stringWriter = new System.IO.StringWriter();
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                description.Write(xmlWriter);
                string theWsdl = stringWriter.ToString();
                presentation.ShowWsdl(theWsdl);
                this.Description = description;
                StringReader stringReader = new StringReader(theWsdl);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                this._wsdlDocument.LoadXml(theWsdl);
                System.Windows.Controls.TreeViewItem xmlDecl = null;
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (presentation._wsdlTreeView.HasItems)
                    {
                        xmlDecl = (System.Windows.Controls.TreeViewItem)presentation._wsdlTreeView.Items[0];
                        if (xmlDecl.HasItems)
                        {
                            xmlDecl.Items.Clear();
                        }
                    }

                }, null);
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    presentation.CopyXmlToTreeView(this._wsdlDocument.ChildNodes[1], presentation.WSDLTreeViewItem);
                    TreeView parent = (TreeView)presentation.WSDLTreeViewItem.Parent;

                    if (parent != null)
                    {
                        int pos = parent.Items.IndexOf(presentation.WSDLTreeViewItem);
                        parent.Items.RemoveAt(pos);
                    }

                    presentation._wsdlTreeView.Items.Add(presentation.WSDLTreeViewItem);
                    presentation.WSDLTreeViewItem.IsExpanded = true;
                    for (int i = 0; i < presentation.WSDLTreeViewItem.Items.Count; i++)
                    {
                        TreeViewItem item = (TreeViewItem)presentation.WSDLTreeViewItem.Items[i];
                        item.IsExpanded = true;
                    }
                    presentation.textBox3.Text = "Compiling successfull";
                    this.ReadWebServiceDescription();

                }, null);

            }
            catch (Exception exception)
            {
                CompilerErrorCollection errors = compilerResults.Errors;
                int errorCounter = errors.Count;
                if (errors != null)
                {
                    for (int i = 0; i < errorCounter; i++)
                    {
                        this.presentation.textBox3.Text += "Error: " + errors[i].ErrorText + "\n";
                    }
                }
            }
        }
        public void ShowError(string message)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, NotificationLevel.Error));
        }
        public void ShowWarning(string message)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, NotificationLevel.Warning));
        }
        #endregion

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {


        }

        public void Initialize()
        {
            // if (presentation.textBox1.Text != null)
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.richTextBox1.AppendText(this.WebServiceSettings.UserCode);

            }, null);
            if (this.WebServiceSettings.Compiled == true)
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.Compile(this.presentation.GetStringToCompile());

                }, null);
              
                
            }

        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return this.presentation; }
        }

        public ISettings Settings
        {
            get { return this._settings; }
        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
           
        }
        #endregion
    }



}
