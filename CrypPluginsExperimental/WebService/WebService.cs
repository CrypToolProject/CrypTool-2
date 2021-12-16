using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Web.Services.Description;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;


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
        private object _service;
        private readonly string[] _wsdlMethod;
        private ServiceDescription _serviceDescription;
        private XmlNode _node;
        private XmlNode _envelope;
        private XmlNode _body;
        private readonly string[] _stringToCompile = new string[5];
        private DataSet _set;
        private readonly string _inputParameter = "";
        private readonly string _inputParameterString = "";
        private readonly string[] _returnParameter = new string[5];
        private string _methodName = "";
        private SignatureValidator _validator;
        private string _publickey;
        private RSACryptoServiceProvider _rsaCryptoServiceProvider;

        #endregion

        #region Properties

        public XmlDocument ModifiedInputDocument
        {
            get => _modifiedInputDocument;
            set => _modifiedInputDocument = value;
        }

        public ServiceDescription ServiceDescription => _serviceDescription;

        public RSACryptoServiceProvider RSACryptoServiceProvider => _rsaCryptoServiceProvider;

        public SignatureValidator Validator => _validator;

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

        [PropertyInfo(Direction.OutputData, "WsdlCaption", "WsdlTooltip", false)]
        public XmlDocument Wsdl
        {
            get => _wsdlDocument;
            set
            {
                _wsdlDocument = value;
                OnPropertyChanged("Wsdl");
            }
        }

        [PropertyInfo(Direction.OutputData, "PublicKeyCaption", "PublicKeyTooltip")]
        public string PublicKey
        {
            get => _publickey;
            set
            {
                _publickey = value;
                OnPropertyChanged("PublicKey");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public XmlDocument OutputString
        {
            get => _outputDocument;
            set
            {
                _outputDocument = value;
                OnPropertyChanged("OutputString");
            }
        }

        public ServiceDescription Description
        {
            get => _serviceDescription;
            set => _serviceDescription = value;
        }

        public WebServiceSettings WebServiceSettings
        {
            get => (WebServiceSettings)_settings;
            set => _settings = value;
        }

        public object XmlOutputConverter(object Data)
        {
            XmlDocument doc = _outputDocument;
            StringWriter stringWriter = new StringWriter();
            object obj = new object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                doc.WriteContentTo(xmlWriter);
                obj = stringWriter.ToString();
            }
            catch (Exception)
            {
                //Console.WriteLine(e.ToString());

            }


            return obj;
        }
        public object XmlInputConverter(object Data)
        {
            XmlDocument doc = _inputDocument;
            StringWriter stringWriter = new StringWriter();
            object obj = new object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                doc.WriteContentTo(xmlWriter);
                obj = stringWriter.ToString();
            }
            catch (Exception)
            {
                //Console.WriteLine(e.ToString());

            }


            return obj;
        }

        public object WsdlConverter(object Data)
        {

            if (_wsdlDocument != null)
            {
                XmlDocument doc = _wsdlDocument;
                StringWriter stringWriter = new StringWriter();
                object obj = new object();
                try
                {
                    XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter)
                    {
                        Formatting = Formatting.Indented
                    };
                    doc.WriteContentTo(xmlWriter);
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

        #region Constructor

        public WebService()
        {


            _wsdlDocument = new XmlDocument();
            _modifiedInputDocument = new XmlDocument();
            _wsdlMethod = new string[1];
            _wsdlMethod[0] = "\n" + @"   
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

            _stringToCompile[0] = @" using System;
            using System.Web;
            using System.Web.Services;
            using System.Web.Services.Protocols;
            using System.Web.Services.Description;
            using System.Xml;
            using System.Xml.Schema;
            using System.IO;";
            _stringToCompile[1] = @"
            
            public class Service : System.Web.Services.WebService
            {
              public Service()
             {
     
              }";
            _stringToCompile[2] = @"[WebMethod]";
            PropertyChanged += new PropertyChangedEventHandler(WebServicePropertyChangedEventHandler);
            presentation = new WebServicePresentation(this);
            _settings.PropertyChanged += new PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            WebServiceSettings.Test = 1;
            WebServiceSettings.Integer = 1;
            WebServiceSettings.MethodName = "methodName";
            CspParameters parameters = new CspParameters
            {
                KeyContainerName = "Container"
            };
            _rsaCryptoServiceProvider = new RSACryptoServiceProvider(parameters);

        }

        #endregion

        #region EventHandlers

        private void WebServicePropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputString")
            {
                CheckSoap();

                if (_inputDocument != null)
                {
                    SoapProtocolImporter t = new SoapProtocolImporter();

                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        presentation.NameSpacesTable.Clear();

                        presentation.SoapInputItem = null;
                        presentation.SoapInputItem = new TreeViewItem();
                        presentation._animationTreeView.Items.Clear();
                        presentation.SoapInputItem.IsExpanded = true;
                        StackPanel panel1 = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal
                        };
                        TextBlock elem1 = new TextBlock
                        {
                            Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                        };
                        panel1.Children.Insert(0, elem1);
                        presentation.SoapInputItem.Header = panel1;
                        presentation.CopyXmlToTreeView(_inputDocument.SelectSingleNode("/*"), presentation.SoapInputItem);
                        //   this.presentation.ResetSoapInputItem();
                        presentation._animationTreeView.Items.Add(presentation.SoapInputItem);
                        presentation.FindTreeViewItem(presentation.SoapInputItem, "Envelope", 1).IsExpanded = true;
                        presentation.FindTreeViewItem(presentation.SoapInputItem, "Header", 1).IsExpanded = true;
                        presentation.FindTreeViewItem(presentation.SoapInputItem, "Security", 1).IsExpanded = true;
                        presentation.FindTreeViewItem(presentation.SoapInputItem, "Signature", 1).IsExpanded = true;
                        presentation.FindTreeViewItem(presentation.SoapInputItem, "Body", 1).IsExpanded = true;
                        presentation._animationTreeView.Items.Refresh();


                    }, null);

                }
            }
        }
        private void SettingsPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            WebServiceSettings webserviceSettings = sender as WebServiceSettings;
            if (e.PropertyName.Equals("createKey"))
            {
                CreateRSAKey();

            }
            if (e.PropertyName.Equals("publishKey"))
            {
                PublicKey = ExportPublicKey();
            }
            if (e.PropertyName.Equals("exportWSDL"))
            {
                Wsdl = _wsdlDocument;
            }
            if (e.PropertyName.Equals("MethodenStub"))
            {
                WebServiceSettings.Integer = 1;
                WebServiceSettings.String = 0;
                WebServiceSettings.Test = 4;
                WebServiceSettings.Double = 2;
                WebServiceSettings.MethodName = "berechneVerzinsung";

                if (presentation.textBlock1.Inlines != null)
                {
                    presentation.textBlock1.Inlines.Clear();
                }

                presentation.textBlock1.Inlines.Add(new Bold(new Run(_stringToCompile[2].ToString() + "\n")));
                presentation.VisualMethodName(WebServiceSettings.MethodName);
                presentation.VisualReturnParam("double");
                presentation.richTextBox1.Document.Blocks.Clear();
                presentation.richTextBox1.AppendText("double endKapital;\n" + "int laufzeit=intparam1;\n" + "double zinssatz=doubleparam1;\n" + "double startKapital=doubleparam2;\n" + "endKapital=Math.Round(startKapital*(Math.Pow(1+zinssatz/100,laufzeit)));\n" + "return endKapital;");
            }

            else
            {
                if (e.PropertyName.Equals("Test"))
                {
                    _returnParameter[0] = "void";
                    _returnParameter[1] = "int";
                    _returnParameter[2] = "string";
                    _returnParameter[3] = "float";
                    _returnParameter[4] = "double";
                    presentation.VisualReturnParam(_returnParameter[webserviceSettings.Test]);
                }

                if (e.PropertyName.Equals("Integer"))
                {
                    if (webserviceSettings.Integer == 1)
                    {
                        presentation.VisualParam("int", 1);
                    }
                    if (webserviceSettings.Integer == 2)
                    {
                        presentation.VisualParam("int", 2);
                    }
                    if (webserviceSettings.Integer == 0)
                    {
                        presentation.VisualParam("int", 0);
                    }

                }
                if (e.PropertyName.Equals("String"))
                {
                    if (webserviceSettings.String == 1)
                    {
                        presentation.VisualParam("string", 1);
                    }
                    if (webserviceSettings.String == 2)
                    {

                        presentation.VisualParam("string", 2);
                    }

                    if (webserviceSettings.String == 0)
                    {
                        presentation.VisualParam("string", 0);
                    }
                }
                if (e.PropertyName.Equals("Double"))
                {
                    if (webserviceSettings.Double == 1)
                    {
                        presentation.VisualParam("double", 1);
                    }
                    if (webserviceSettings.Double == 2)
                    {

                        presentation.VisualParam("double", 2);
                    }

                    if (webserviceSettings.Double == 0)
                    {
                        presentation.VisualParam("double", 0);
                    }
                }


                if (e.PropertyName.Equals("MethodName"))
                {
                    _methodName = webserviceSettings.MethodName;
                    presentation.VisualMethodName(webserviceSettings.MethodName);
                }
                string comma = "";
                if (!_inputParameter.Equals(""))
                {
                    comma = ",";
                }
                if (_inputParameterString.Equals(""))
                {
                    comma = "";
                }
                _stringToCompile[3] = @"public" + " " + _returnParameter[webserviceSettings.Test] + " " + _methodName + "(" + "" + _inputParameter + comma + _inputParameterString + ")\n{";
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
            CspParameters parameters = new CspParameters
            {
                KeyContainerName = "Container"
            };
            _rsaCryptoServiceProvider = new RSACryptoServiceProvider(parameters);
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Key pair created", this, NotificationLevel.Info));
        }
        private string ExportPublicKey()
        {
            if (_rsaCryptoServiceProvider != null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Public key exported", this, NotificationLevel.Info));
                return _rsaCryptoServiceProvider.ToXmlString(false);
            }
            return "";
        }
        public int GetSignatureNumber()
        {
            return _inputDocument.GetElementsByTagName("ds:Signature").Count;

        }
        public bool CheckSignature()
        {
            XmlNodeList signatureElements = _inputDocument.GetElementsByTagName("Signature");
            XmlElement signaturElement = (XmlElement)signatureElements.Item(0);
            SignedXml signedXml = new SignedXml(_inputDocument);
            signedXml.LoadXml(signaturElement);
            return signedXml.CheckSignature(); ;
        }
        private void ReadWebServiceDescription()
        {
            _set = new DataSet();
            XmlSchema paramsSchema = _serviceDescription.Types.Schemas[0];
            StringWriter schemaStringWriter = new StringWriter();
            paramsSchema.Write(schemaStringWriter);
            StringReader sreader = new StringReader(schemaStringWriter.ToString());
            XmlTextReader xmlreader = new XmlTextReader(sreader);
            _set.ReadXmlSchema(xmlreader);

        }
        private bool Compiled()
        {
            if (_serviceDescription == null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("There is no Web Service Description available", this, NotificationLevel.Error));
                return false;
            }
            return true;
        }
        private void CheckSoap()
        {
            bool signatureValid = true;
            Compiled();
            if (_inputDocument == null)
            {
                return;
            }

            if (_inputDocument.GetElementsByTagName("ds:Signature") != null)
            {
                _validator = new SignatureValidator(this);
            }
            signatureValid = _validator.Valid;
            object invokedObject = new object();
            object[] parameters = null;
            string response;
            if (_serviceDescription == null)
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

                Types types = _serviceDescription.Types;
                PortTypeCollection portTypes = _serviceDescription.PortTypes;
                MessageCollection messages = _serviceDescription.Messages;
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
                _set = new DataSet();
                StringReader sreader = new StringReader(xmlStringWriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                _set.ReadXmlSchema(xmlreader);
                if (sequenzTypeInput != null)
                {
                    foreach (XmlSchemaElement inputParam in sequenzTypeInput.Items)
                    {
                        XmlQualifiedName schemaName = inputParam.SchemaTypeName;
                        paramTypesTable.Add(inputParam.QualifiedName.Name, schemaName.Name);
                    }

                    XmlNamespaceManager manager = new XmlNamespaceManager(_modifiedInputDocument.NameTable);
                    XmlElement body = (XmlElement)_inputDocument.GetElementsByTagName("s:Body")[0];
                    manager.AddNamespace("s", body.NamespaceURI);
                    manager.AddNamespace("tns", "http://tempuri.org/");
                    XmlNode node = _modifiedInputDocument.SelectSingleNode("s:Envelope/s:Body/" + "tns:" + _set.Tables[0].TableName, manager);
                    XmlElement ele = (XmlElement)node;
                    int paramCounter = new int();
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
                        parameters = new object[paramCounter];

                        for (int i = 0; i < paramCounter; i++)
                        {
                            string param = ele.ChildNodes[i].InnerText;
                            object paramType = paramTypesTable[ele.ChildNodes[i].LocalName];
                            if (paramType.ToString().Equals("int"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        parameters[i] = Convert.ToInt32((object)ele.ChildNodes[i].InnerText);
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
                            if (paramType.ToString().Equals("string"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        parameters[i] = Convert.ToString((object)ele.ChildNodes[i].InnerText);
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
                                        parameters[i] = Convert.ToDouble((object)ele.ChildNodes[i].InnerText);
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
                            Type typ = _service.GetType().GetMethod(operation.Name).ReturnType;
                            string returnType = typ.ToString();
                            if (!returnType.Equals("System.Void"))
                            {
                                invokedObject = _service.GetType().GetMethod(operation.Name).Invoke(_service, parameters).ToString();
                            }
                            else { _service.GetType().GetMethod(operation.Name).Invoke(_service, parameters).ToString(); }
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
                                invokedObject = _service.GetType().GetMethod(operation.Name).Invoke(_service, null).ToString();
                            }
                            catch (Exception e)
                            {

                                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                goto Abort;

                            }
                        }
                        else { _service.GetType().GetMethod(operation.Name).Invoke(_service, parameters); }
                    }
                    response = invokedObject.ToString();
                    CreateResponse(response);
                }
            }
        Abort:;
        }
        private void CreateResponse(string response)
        {
            _soapResponse = new XmlDocument();
            _node = _soapResponse.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");
            _soapResponse.AppendChild(_node);
            _envelope = _soapResponse.CreateElement("Envelope", "http://www.w3.org/2001/12/soap-envelope");
            _soapResponse.AppendChild(_envelope);
            _body = _soapResponse.CreateElement("Body", "http://www.w3.org/2001/12/soap-envelope");
            XmlNode input = _soapResponse.CreateElement(_set.Tables[1].ToString(), _set.Tables[1].Namespace);
            DataTable table = _set.Tables[1];
            foreach (DataColumn tempColumn in table.Columns)
            {
                XmlNode neu = _soapResponse.CreateElement(tempColumn.ColumnName, _set.Tables[1].Namespace);
                neu.InnerText = response;
                input.AppendChild(neu);
            }
            _body.AppendChild(input);
            _envelope.AppendChild(_body);
            OutputString = _soapResponse;
        }
        public ArrayList GetSignatureReferences(int i)
        {
            return _validator.GetSignatureReferences(i);
        }
        public ArrayList GetSignedXmlSignatureReferences()
        {
            return _validator.GetSignedXmlSignatureReferences();
        }
        public void Compile(string code)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
            string header = "";
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                header = presentation.CopyTextBlockContentToString(presentation.textBlock1);

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
                string wsdl = _wsdlMethod[0];
                compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, _stringToCompile[0].ToString() + _stringToCompile[1].ToString() + code + wsdl);
                System.Reflection.Assembly compiledAssembly = compilerResults.CompiledAssembly;
                _service = compiledAssembly.CreateInstance("Service");
                object invokedObject = _service.GetType().InvokeMember("getWsdl", System.Reflection.BindingFlags.InvokeMethod, null, _service, null);
                ServiceDescription description = (ServiceDescription)invokedObject;
                System.IO.StringWriter stringWriter = new System.IO.StringWriter();
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                description.Write(xmlWriter);
                string theWsdl = stringWriter.ToString();
                presentation.ShowWsdl(theWsdl);
                Description = description;
                StringReader stringReader = new StringReader(theWsdl);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                _wsdlDocument.LoadXml(theWsdl);
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
                    presentation.CopyXmlToTreeView(_wsdlDocument.ChildNodes[1], presentation.WSDLTreeViewItem);
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
                    ReadWebServiceDescription();

                }, null);

            }
            catch (Exception)
            {
                CompilerErrorCollection errors = compilerResults.Errors;
                int errorCounter = errors.Count;
                if (errors != null)
                {
                    for (int i = 0; i < errorCounter; i++)
                    {
                        presentation.textBox3.Text += "Error: " + errors[i].ErrorText + "\n";
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
                presentation.richTextBox1.AppendText(WebServiceSettings.UserCode);

            }, null);
            if (WebServiceSettings.Compiled == true)
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Compile(presentation.GetStringToCompile());

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

        public System.Windows.Controls.UserControl Presentation => presentation;

        public ISettings Settings => _settings;

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
