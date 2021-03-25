using System;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services.Description;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Threading;
using System.Threading;

namespace ManInTheMiddle
{
    [Author("Jan Bernhardt", "jan_bernhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("ManInTheMiddle", "Represents a Man in the middle", "", new[] { "ManInTheMiddle/ManInTheMiddleIcon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ManInTHeMiddle : ICrypComponent
    {

      private ISettings settings = new ManInTheMiddleSettings();
        private XmlDocument inputSoap;
        private XmlDocument outputSoap;
        public XmlDocument inputAnswer,wsdl,soap;
        private MitmPresentation presentation;
        private bool wrapper,send;
        

        public ManInTHeMiddle()
        {
            presentation = new MitmPresentation(this);
            outputSoap = null;
            this.PropertyChanged += new PropertyChangedEventHandler(ManInTHeMiddle_PropertyChanged);
            settings.PropertyChanged +=new PropertyChangedEventHandler(settings_PropertyChanged);
            wsdl = null;
            send = false;
            mySettings.Soap = "";
         
        }

        void ManInTHeMiddle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ManInTHeMiddle s = (ManInTHeMiddle)sender;
            switch (e.PropertyName)
            {
                case "InputString":
                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (inputSoap != null)
                        {
                            TreeViewItem item = new TreeViewItem();
                            XmlNode node = (XmlNode)inputSoap.DocumentElement;
                            TextBlock elem1 = new TextBlock();
                            elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
                            StackPanel panel = new StackPanel();
                            panel.Children.Add(elem1);
                            item.Header = panel;

                            presentation.CopyXmlToTreeView(node, item, this.getParameter());
                            presentation.treeView.Items.Clear();
                            item.IsExpanded = true;
                            presentation.treeView.Items.Add(item);
                        }
                    }, null);
                   break;
            }
        }

        public bool checkSecurityHeader()
        {
            bool securityheader = false;
            XmlNodeList list =  outputSoap.GetElementsByTagName("wsse:Security");
            if (!(list.Count == 0))
            {
                securityheader = true;
            }
            return securityheader;
        }

        public void wrapperAttackOnBody()
        {
            if (!checkSecurityHeader())
            {
                outputSoap.GetElementsByTagName("s:Envelope")[0].RemoveChild(outputSoap.GetElementsByTagName("s:Body")[0]);
                outputSoap.GetElementsByTagName("s:Envelope")[0].AppendChild(outputSoap.ImportNode(soap.GetElementsByTagName("s:Body")[0],true));
            }
            else 
            {
                XmlNode wrapperElement = outputSoap.CreateElement("TheWrapper");
                outputSoap.GetElementsByTagName("s:Header")[0].AppendChild(wrapperElement);
                wrapperElement.AppendChild(outputSoap.GetElementsByTagName("s:Body")[0]);
                outputSoap.GetElementsByTagName("s:Envelope")[0].AppendChild(outputSoap.ImportNode(soap.GetElementsByTagName("s:Body")[0],true));
            }
        }

        public void createErrorMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Error));
        }

        public void createInfoMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Info));
        }

        [PropertyInfo(Direction.OutputData, "SOAP Output", "Output a modified SOAP message to be processed by the Web Service", true)]
         public XmlDocument OutputString
        {
            get {
                outputSoap=(XmlDocument) inputSoap.Clone();
                if (!this.wrapper)
                {
                    send = true;
                    return this.outputSoap;
                }
                else
                {
                    send = true;
                    wrapperAttackOnBody();
                    return outputSoap;
                }

            }
            set
            { 
                
                this.outputSoap = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "SOAP Input", "Input from a Web-Service Client", false)]
        public XmlDocument InputString
        {
            get { return this.inputSoap; }
            set
            {
                
                    this.inputSoap = value;
               
                OnPropertyChanged("InputString");
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "SOAP Input", "Soap Message response from a Web-Service", false)]
        public XmlDocument InputAnswer
        {
            get { return this.inputAnswer; }
            set
            {
                this.inputAnswer = value;
                OnPropertyChanged("InputAnswer");
              
            }
        }

        [PropertyInfo(Direction.InputData, "WSDL Input", "WSDL to create the soap message")]
        public XmlDocument wsdlInput
        {
            get { return this.wsdl; }
            set
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    string s = xmlToString(value);
                    loadWSDL(s);
                    OnPropertyChanged("wsdlInput");
                    createInfoMessage("Received WSDL File");
                }, null);
            }
        }

        public void loadWSDL(string wsdlString)
        {
            if (!wsdlString.Equals(""))
            {
                StringReader sr = new StringReader(wsdlString);
                XmlTextReader tx = new XmlTextReader(sr);

                ServiceDescription t = ServiceDescription.Read(tx);
                // Initialize a service description importer.

                ServiceDescription serviceDescription = t.Services[0].ServiceDescription;
                Types types = serviceDescription.Types;
                PortTypeCollection portTypes = serviceDescription.PortTypes;
                MessageCollection messages = serviceDescription.Messages;
                XmlSchema schema = types.Schemas[0];
                PortType porttype = portTypes[0];
                Operation operation = porttype.Operations[0];
                OperationInput input = operation.Messages[0].Operation.Messages.Input;
                Message message = messages[input.Message.Name];

                MessagePart messagePart = message.Parts[0];
                //        XmlSchemaObject fsdf = types.Schemas[0].Elements[messagePart.Element];
                XmlSchema fsdf = types.Schemas[0];
                if (fsdf == null)
                {
                    //Console.WriteLine("Test");
                }
                StringWriter twriter = new StringWriter();
                //  TextWriter writer= new TextWriter(twriter);
                fsdf.Write(twriter);
                DataSet set = new DataSet();
                StringReader sreader = new StringReader(twriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                set.ReadXmlSchema(xmlreader);
  
                soap = new XmlDocument();
                XmlNode node,envelope,header, body, securityHeader;
                node = soap.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");

                soap.AppendChild(node);
                envelope = soap.CreateElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");


                soap.AppendChild(envelope);

                body = soap.CreateElement("s", "Body", "http://www.w3.org/2003/05/soap-envelope");
                XmlNode eingabe = soap.CreateElement("tns", set.Tables[0].ToString(), set.Tables[0].Namespace);
                DataTable table = set.Tables[0];
                foreach (DataColumn tempColumn in table.Columns)
                {
                    XmlNode neu = soap.CreateElement("tns", tempColumn.ColumnName, set.Tables[0].Namespace);
                    eingabe.AppendChild(neu);
                }
                body.AppendChild(eingabe);
                envelope.AppendChild(body);
                mySettings.Soap = xmlToString(soap);
           
            }
        }

        public void showsoapBody()
        {
            XmlNode rootElement = soap.SelectSingleNode("/*");
            presentation.SoapItem = new System.Windows.Controls.TreeViewItem();


            presentation.SoapItem.IsExpanded = true;

            StackPanel panel1 = new StackPanel();

            panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;


            TextBlock elem1 = new TextBlock();
            elem1.Text = "Insert your values:";



            panel1.Children.Insert(0, elem1);

            presentation.SoapItem.Header = panel1;

            presentation.CopyXmlToTreeView(soap.GetElementsByTagName("s:Body")[0], presentation.SoapItem, this.getParameter());
            presentation.treeView.Items.Clear();
            presentation.treeView.Items.Add(presentation.SoapItem);
            this.outputSoap = this.soap;
            OnPropertyChanged("OutputString");
        }


        public XmlNode[] getParameter()
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode node in soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].ChildNodes)
            {

                list.Add(node);
            }
            XmlNode[] nodeset = new XmlNode[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                nodeset[i] = (XmlNode)list[i];
            }
            return nodeset;
        }





        public Object XmlOutputConverter(Object Data)
        {
            string test = Data.ToString();

            XmlDocument doc = (XmlDocument)this.outputSoap;
            StringWriter t= new StringWriter();
            Object obj = new Object();
            try
            {
                XmlTextWriter j = new XmlTextWriter(t);
                j.Formatting = Formatting.Indented;
                doc.WriteContentTo(j);
                obj = (Object)t.ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());

            }
            return obj;
        }

        private ManInTheMiddleSettings mySettings
        {
            get { return (ManInTheMiddleSettings)this.settings; }
        }

        void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           ManInTheMiddleSettings s = (ManInTheMiddleSettings)sender;
            switch (e.PropertyName)
            {
                case "insertBody":
                    wrapper = s.insertBody;
                    if ((wrapper) && (s.Soap!=""))
                    {
                        this.showsoapBody();
                    }
                    else
                    {
                        presentation.treeView.Items.Clear();
                    }
                    break;
                case "Soap":
                    if (s.Soap != "")
                    {
                        soap = stringToXml(s.Soap);
                        if (wrapper)
                        {
                            this.showsoapBody();
                        }
                    }
                    break;

            }
        }

        public string xmlToString(XmlDocument doc)
        {

            if (doc != null)
            {
                StringWriter sw = new StringWriter();
                doc.Normalize();
                XmlTextWriter tx = new XmlTextWriter(sw);
                tx.Formatting = Formatting.Indented;
                doc.WriteContentTo(tx);
                return sw.ToString();
            }
            else
            {
                return "";
            }
        }

        public XmlDocument stringToXml(string s)
        {
            XmlDocument doc = new XmlDocument();
            if (!s.Equals(""))
            {
                StringReader sr = new StringReader(s);
                XmlTextReader tx = new XmlTextReader(sr);
                doc.Load(tx);
            }
            return doc;
        }

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {
            if (!send)
            {
                OnPropertyChanged("OutputString");
            }
            else { send = false; }
      
        }

        public void Initialize()
        {

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
            get { return this.settings; }
        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));



        }

        public void Stop()
        {
            send = false;
        }

        #endregion

       

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        


        #endregion
    }
}
