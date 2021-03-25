using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using System.Web.Services.Description;
using System.Xml;
using System.Collections;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace WebService
{
    /// <summary>
    /// Interaktionslogik für WebServicePresentation.xaml
    /// </summary>
    public partial class WebServicePresentation : System.Windows.Controls.UserControl
    {
        #region Fields
        private bool _isscrolling;
        private bool _referenceValid = true;
        private TreeViewItem _wsdlTreeViewItem;
        private TreeViewItem _soapInputItem;
        private TreeViewItem _foundItem;
        private ArrayList _signatureCollection;
        private ArrayList _tempReferenceCollection;
        private ArrayList _tempTransformCollection;
        private string _webServiceDescription;
        private string _lastURI;
        private Run _methodVisibility;
        private Run _returnParam;
        private Run _methodName;
        private Run _openBrace;
        private Run _closeBrace;
        private Run _comma;
        private Run _openMethodBrace;
        private Bold _webMethod;
        private FlowDocument _doc;
        private Bold _intParams;
        private Bold _stringParams;
        private Bold _doubleParams;
        private System.ComponentModel.SortDescription _sortDescription;
        private DispatcherTimer _dispatcherTimer;
        private DispatcherTimer _decryptionTimer;
        private DispatcherTimer _referenceTimer;
        private DispatcherTimer _transformTimer;
        private DoubleAnimation _textSizeAnimation;
        private DoubleAnimation _textSizeAnimationReverse;
        private DoubleAnimation _textSizeAnimation1;
        private DoubleAnimation _textSizeAnimationReverse1;
        private int _status;
        private int _referenceStatus;
        private int _transformstatus;
        private int _signatureNumber;
        private int _actualSignatureNumber;
        private int _signaturenumber;
        private int _actualReferenceNumber;
        private int _referenceNumber;
        private int _transformNumber;
        private int _transformCount;
        private AnimationController _animationController;
        private Hashtable _namespacesTable;
        private DecryptionAnimation _decryptionAnimation;
        private WebService _webService;
        private Dictionary<string, TreeViewItem> _nodeToTreeViewItemDictionary;
        
        #endregion

        #region Properties

        public WebService WebService
        {
            get
            {
                return this._webService;
            }
        }

        public Hashtable NameSpacesTable
        {
            get
            {
                return this._namespacesTable;
            }
        }


        public DecryptionAnimation DecryptionAnimation
        {
            get
            {
                return this._decryptionAnimation;
            }
        }

        public TreeViewItem WSDLTreeViewItem
        {
            get
            {
                return this._wsdlTreeViewItem;
            }
            set
            {
                this._wsdlTreeViewItem = value;
            }
        }

        public TreeViewItem SoapInputItem
        {
            get
            {
                return this._soapInputItem;
            }
            set
            {
                this._soapInputItem = value;
            }
        }
        public AnimationController AnimationController
        {
            get
            {
                return this._animationController;
            }
        }

        public DispatcherTimer SignatureTimer
        {
            get
            {
                return this._dispatcherTimer;
            }
        }

        #endregion


        #region Constructor
        public WebServicePresentation(WebService webService)
        {
            InitializeComponent();
            this._nodeToTreeViewItemDictionary = new Dictionary<string, TreeViewItem>();
            this._actualSignatureNumber = 1;
            this._decryptionAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            this._animationController = new AnimationController(this);
            this._signatureCollection = new ArrayList();
            this._tempTransformCollection = new ArrayList();
            Paragraph par = (Paragraph)this.richTextBox1.Document.Blocks.FirstBlock;
            par.LineHeight = 5;
            this._status = 1;
            this._dispatcherTimer = new DispatcherTimer();
            this._dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._referenceTimer = new DispatcherTimer();
            this._referenceTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._decryptionTimer = new DispatcherTimer();
            this._decryptionTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._transformTimer = new DispatcherTimer();
            this._transformTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            this._textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            this._dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickEventHandler);
            this._referenceTimer.Tick += new EventHandler(ReferenceTimerTickEventHandler);
            this._transformTimer.Tick += new EventHandler(TransformTimerTickEventHandler);
            this._doc = new FlowDocument();
            this._webMethod = new Bold(new Run("[WebMethod]" + "\n"));
            this._methodVisibility = new Run();
            this._returnParam = new Run();
            this._methodName = new Run();
            this._intParams = new Bold();
            this._stringParams = new Bold();
            this._doubleParams = new Bold();
            this._openBrace = new Run();
            this._closeBrace = new Run();
            this._comma = new Run(",");
            this._openMethodBrace = new Run("\n{");
            textBlock2.Inlines.Add(new Run("}"));
            textBlock2.Visibility = Visibility.Visible;
            this.VisualMethodName("methodName");
            this.richTextBox1.Document = _doc;
            this.textBlock1.Inlines.Add(this._webMethod);
            this.textBlock1.Inlines.Add(this._methodVisibility);
            this.textBlock1.Inlines.Add(this._returnParam);
            this.textBlock1.Inlines.Add(this._methodName);
            this.textBlock1.Inlines.Add(this._openBrace);
            this.textBlock1.Inlines.Add(this._intParams);
            this.textBlock1.Inlines.Add(this._stringParams);
            this.textBlock1.Inlines.Add(this._doubleParams);
            this.textBlock1.Inlines.Add(this._closeBrace);
            textBlock1.Inlines.Add(_openMethodBrace);
            this._wsdlTreeViewItem = new TreeViewItem();
            this._foundItem = new TreeViewItem();
            this._wsdlTreeViewItem.Header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            this._wsdlTreeViewItem.IsExpanded = true;
            this._soapInputItem = new TreeViewItem();
            TextBlock block = new TextBlock();
            block.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            StackPanel panel = new StackPanel();
            panel.Children.Add(block);
            this._soapInputItem.Header = panel;
            this._soapInputItem.IsExpanded = true;
            this._webService = webService;
            this._namespacesTable = new Hashtable();
            _sortDescription = new System.ComponentModel.SortDescription("param", System.ComponentModel.ListSortDirection.Ascending);
            this._tempReferenceCollection = new ArrayList();
            webService.Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            this._animationTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(AnimationTreeViewSelectedItemChangedEventHandler);
            this._scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChangedEventHandler);
        }


        #endregion

        #region Methods

        public void ResetSoapInputItem()
        {
            this._nodeToTreeViewItemDictionary.Clear();
            this._webService.InputString = this._webService.InputString;
            this._dispatcherTimer.Stop();
            this._decryptionTimer.Stop();
            this._referenceTimer.Stop();
            this._animationController.ControllerTimer.Stop();
            this._decryptionAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            this._animationController = new AnimationController(this);
            this._status = 1;
            this._signaturenumber = 0;


        }

        public void CopyXmlToTreeView(XmlNode xmlNode, TreeViewItem treeViewItemParent)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
            if (xmlNode != null)
            {
                TreeViewItem item = new TreeViewItem();
                item.IsExpanded = true;
                StackPanel panel = new StackPanel();
                panel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                TextBlock tbTagOpen = new TextBlock();
                TextBlock tbTagClose = new TextBlock();
                TextBlock tbName = new TextBlock();
                tbTagOpen.Name = "tbTagOpen";
                tbTagClose.Name = "tbTagClose";
                tbName.Name = "tbName";
                tbTagOpen.Text = "<";
                tbTagClose.Text = ">";
                tbName.Text = xmlNode.Name;
                tbTagOpen.Foreground = elemBrush;
                tbTagClose.Foreground = elemBrush;
                tbName.Foreground = elemBrush;
                if (!this._nodeToTreeViewItemDictionary.ContainsKey(xmlNode.OuterXml))
                {
                    xmlNode.Normalize();
                    this._nodeToTreeViewItemDictionary.Add(xmlNode.OuterXml.ToString(), item);
                }
                if (xmlNode.OuterXml.Contains("Body"))
                {

                }
                if (!xmlNode.NodeType.ToString().Equals("Text"))
                {
                    item.Name = "OpenItemXmlNode";
                    panel.Name = "OpenPanelXMLNode";
                    TreeViewItem closeitem = new TreeViewItem();
                    panel.Children.Insert(0, tbTagOpen);
                    panel.Children.Add(tbName);
                    if (!xmlNode.NamespaceURI.Equals(""))
                    {
                        InsertNamespace(ref panel, xmlNode.NamespaceURI, xmlNode.Prefix);
                    }
                    if (xmlNode.Attributes != null)
                    {
                        InsertAttributes(ref panel, xmlNode.Attributes);
                    }

                    panel.Children.Add(tbTagClose);


                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    treeViewItemParent.Items.Add(item);
                    if (xmlNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xmlNode.ChildNodes)
                        {
                            this._lastURI = xmlNode.NamespaceURI; ;
                            CopyXmlToTreeView(child, item);
                        }
                    }
                    StackPanel panel1 = new StackPanel();
                    panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    TextBlock elem1 = new TextBlock();
                    TextBlock tbTagOpen3 = new TextBlock();
                    tbTagOpen3.Name = "tbTagOpen";
                    tbTagOpen3.Text = "<";
                    panel1.Children.Insert(0, tbTagOpen3);
                    elem1.Name = "tbName";
                    elem1.Text = "/" + xmlNode.Name;
                    panel1.Children.Add(elem1);
                    TextBlock tbTagClose3 = new TextBlock();
                    tbTagClose3.Name = "tbTagClose";
                    tbTagClose3.Text = ">";
                    panel1.Children.Add(tbTagClose3);

                    closeitem.Header = panel1;

                    treeViewItemParent.Items.Add(closeitem);
                }
                else
                {
                    TextBlock tbTagOpen2 = new TextBlock();
                    TextBlock tbTagClose2 = new TextBlock();

                    tbTagOpen2.Name = "tbTagOpen";
                    tbTagOpen2.Text = "<";
                    tbTagClose2.Name = "tbTagClose";
                    tbTagClose2.Text = ">";
                    item.Name = "OpenItemTextNode";
                    panel.Name = "OpenPanelTextNode";
                    TextBlock tbText = new TextBlock();
                    tbText.Name = "TextNode";
                    tbText.Text = xmlNode.Value;
                    TextBlock emptyTextBlock = new TextBlock();
                    emptyTextBlock.Text = "";
                    panel.Children.Insert(0, emptyTextBlock);
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    treeViewItemParent.Items.Add(item);
                }
            }

        }
       


        public StackPanel InsertAttributes(ref StackPanel panel, XmlAttributeCollection xmlAttributes)
        {
            foreach (XmlAttribute tempAttribute in xmlAttributes)
            {
                if (!tempAttribute.Name.Contains("xmlns"))
                {
                    TextBlock name = new TextBlock();
                    name.Text = " " + tempAttribute.Name;
                    name.Name = "attributeName";
                    TextBlock value = new TextBlock();
                    value.Name = "attributeValue";
                    value.Text = " =\"" + tempAttribute.Value + "\"";
                    SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                    value.Foreground = valueBrush;
                    panel.Children.Add(name);
                    panel.Children.Add(value);

                }
                else
                {
                    if (!_namespacesTable.ContainsValue(tempAttribute.Value))
                    {
                        this._namespacesTable.Add(tempAttribute.Value, tempAttribute.Value);
                        TextBlock name = new TextBlock();
                        name.Text = " " + tempAttribute.Name;
                        TextBlock value = new TextBlock();
                        value.Text = " =\"" + tempAttribute.Value + "\"";
                        SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                        value.Foreground = valueBrush;
                        panel.Children.Add(name);
                        panel.Children.Add(value);
                    }
                }
            }
            return panel;
        }
        public StackPanel InsertNamespace(ref StackPanel panel, string nameSpace, string nameSpacePrefix)
        {
            if (!_namespacesTable.ContainsValue(nameSpace))
            {
                this._namespacesTable.Add(nameSpace, nameSpace);
                TextBlock xmlns = new TextBlock();
                xmlns.Name = "xmlns";
                xmlns.Text = " xmlns";
                TextBlock prefix = new TextBlock();
                prefix.Name = "xmlnsPrefix";
                if (!nameSpacePrefix.Equals(""))
                { prefix.Text = ":" + nameSpacePrefix; }
                else { prefix.Text = ""; }
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock();
                value.Name = "xmlnsValue";
                value.Text = "=" + "\"" + nameSpace + "\"";
                value.Foreground = valueBrush;
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
        }

        private void DispatcherTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (this._status)
            {
                case 1:

                    this._signatureNumber = this._webService.GetSignatureNumber();
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Check for Signature Element";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._animationTreeView.Items.Refresh();
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "Signature", 1).BringIntoView();
                    this.AnimateFoundElements((TreeViewItem)this._signatureCollection[this._signaturenumber], (TreeViewItem)_signatureCollection[this._signaturenumber]);
                    this._status = 2;
                    slider1.Value++;
                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n Canonicalize SignedInfo";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignedInfo", 1).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/SignedInfo", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignedInfo>", this._actualSignatureNumber));
                    this._status = 3;
                    slider1.Value++;
                    break;
                case 3:
                    this._animationStepsTextBox.Text += "\n -->Find Canonicalization Algorithm";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "CanonicalizationMethod", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/CanonicalizationMethod", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:CanonicalizationMethod>", this._actualSignatureNumber));
                    this._status = 4;
                    slider1.Value++;
                    break;
                case 4:
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindSignatureItems((TreeViewItem)_signatureCollection[this._signaturenumber], "ds:Reference");
                    this.InitializeReferenceAnimation();
                    this._status = 5;
                    this._dispatcherTimer.Stop();
                    slider1.Value++;
                    break;

                case 5:
                    this._animationStepsTextBox.Text += "\n Signature Validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._animationStepsTextBox.Text += "\n -> Find Signature Method";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem(this._soapInputItem, "SignatureMethod", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureMethod", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignatureMethod", this._actualSignatureNumber));
                    this._status = 6;
                    slider1.Value++;


                    break;
                case 6:
                    this._animationStepsTextBox.Text += "\n Get public key for signature validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "KeyInfo", 1).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "KeyInfo", 1), this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "/ds:KeyInfo", 1));
                    this._status = 7;
                    break;
                case 7:
                    this._animationStepsTextBox.Text += "\n -> Validate SignatureValue over SignedInfo";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureValue", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureValue", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignatureValue", this._actualSignatureNumber));
                    this._dispatcherTimer.Stop();
                    this._status = 8;
                    slider1.Value++;
                    this._animationController.ControllerTimer.Start();
                    break;
                case 8:
                    this._tempReferenceCollection.Clear();
                    this._animationController.ControllerTimer.Start();
                    this._signatureNumber = this._webService.GetSignatureNumber();
                    this._actualSignatureNumber++;
                    if (this._signaturenumber + 1 < this._signatureNumber)
                    {
                        this._signaturenumber++;
                        this._status = 1;
                        slider1.Value++;
                    }

                    break;
            }


        }
        private void ReferenceTimerTickEventHandler(object sender, EventArgs e)
        {
            int n = this._webService.Validator.GetReferenceNumber(_signaturenumber);
            switch (_referenceStatus)
            {
                case 1:
                    this._referenceTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Reference Validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._referenceStatus++;
                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n -> Find Reference Element";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    // this.findSignatureItems((TreeViewItem)signatureCollection[this.i], "ds:Reference").BringIntoView() ;
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Reference", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Reference", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:Reference", this._actualSignatureNumber));
                    this._referenceStatus++;
                    break;

                case 3:
                    this._animationStepsTextBox.Text += "\n -> Get referenced Element";
                    XmlNode node = this._webService.Validator.GetSignatureReferenceElement(_signaturenumber) as XmlNode;
                    node.Normalize();
                    TreeViewItem referenceItem = this._nodeToTreeViewItemDictionary[(node.OuterXml).ToString()];
              //    this.FindTreeViewItem(this._soapInputItem, this._webService.Validator.GetSignatureReferenceName(_signaturenumber), this._actualReferenceNumber).BringIntoView();
                    referenceItem.BringIntoView();     
              this.AnimateFoundElements(referenceItem, referenceItem);
                    this._referenceStatus++;
                    break;

                case 4:
                    this._animationStepsTextBox.Text += "\n  -> Apply Transforms";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Transforms", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Transforms", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:Transforms", this._actualSignatureNumber));
                    this._transformCount = this._webService.Validator.GetTransformsCounter(_signaturenumber, this._referenceNumber);
                    this._referenceTimer.Stop();
                    this._referenceStatus++;
                    this.FindSignatureItems((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "ds:Transform");
                    InitializeTransformAnimation();

                    break;
                case 5:
                    this._animationStepsTextBox.Text += "\n  -> Digest References";
                    this._animationStepsTextBox.Text += "\n    -> Find DigestAlgorithm";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestMethod", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestMethod", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:DigestMethod", this._actualSignatureNumber));
                    this._referenceStatus++;
                    break;
                case 6:
                    this._animationStepsTextBox.Text += "\n    -> Calculated DigestValue:" + "\n       " + this._webService.Validator.DigestElement(_signaturenumber, this._referenceNumber);
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._referenceStatus++;
                    break;
                case 7:
                    this._animationStepsTextBox.Text += "\n    -> Compare the DigestValues:";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestValue", this._actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestValue", this._actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:DigestValue", this._actualSignatureNumber));
                    this._referenceStatus++;
                    break;
                case 8:
                    if (this._webService.Validator.CompareDigestValues(_signaturenumber, this._referenceNumber, this._webService.Validator.DigestElement(_signaturenumber, this._referenceNumber)))
                    {

                        this._animationStepsTextBox.Text += "\n Reference Validation succesfull";
                        this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                        this._referenceValid = true;
                        this._referenceStatus++;
                    }
                    else
                    {
                        this._animationStepsTextBox.Text += "\n Reference Validation failed";
                        this._referenceStatus++;
                        this._referenceValid = false;
                    }

                    break;
                case 9:
                    this._referenceTimer.Stop();
                    this._referenceNumber++;
                    // status = 7;
                    this._dispatcherTimer.Start();
                    break;
            }

        }
        private void TransformTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (_transformstatus)
            {
                case 1:
                    this._transformTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Make Transforms";
                    this._transformstatus++;

                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n -> Find Transform";
                    TreeViewItem tempTransform = (TreeViewItem)this._tempTransformCollection[_transformNumber];
                    tempTransform.BringIntoView();
                    this.AnimateFoundElements(tempTransform, tempTransform);
                    this._transformstatus++;
                    break;

                case 3:
                    this._animationStepsTextBox.Text += "\n  ->execute Transform";
                    this._animationStepsTextBox.Text += "\n" + this._webService.Validator.MakeTransforms(_signaturenumber, this._referenceNumber, this._transformNumber);
                    this._transformstatus++;
                    break;

                case 4:
                    if (this._transformNumber + 1 < this._transformCount)
                    {
                        this._transformNumber++;
                        this._transformstatus = 2;
                        slider1.Value++;
                    }
                    else
                    {
                        this._transformTimer.Stop();
                        this._referenceTimer.Start();

                        this._referenceStatus = 5;
                    }


                    break;




            }
        }
        private void InitializeTransformAnimation()
        {
            this._transformstatus = 1;
            this._transformNumber = 0;
            this._transformTimer.Start();


        }

        private void InitializeReferenceAnimation()
        {

            this._referenceStatus = 1;
            this._referenceNumber = 0;
            this._referenceTimer.Start();


        }
        public void InitializeAnimation()
        {
            this._status = 1;
            this._signaturenumber = 0;

        }

        private void AnimateFoundElements(TreeViewItem item, TreeViewItem item2)
        {
            item.IsSelected = true;
            Storyboard storyBoard = new Storyboard();
            this._textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            this._textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            storyBoard.Children.Add(_textSizeAnimation);
            storyBoard.Children.Add(_textSizeAnimationReverse);
            storyBoard.Children[0].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[1].BeginTime = new TimeSpan(0, 0, 4);
            storyBoard.Children.Add(_textSizeAnimation1);
            storyBoard.Children.Add(_textSizeAnimationReverse1);
            storyBoard.Children[2].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[3].BeginTime = new TimeSpan(0, 0, 4);
            Storyboard.SetTarget(_textSizeAnimation, item);
            Storyboard.SetTarget(_textSizeAnimationReverse, item);
            Storyboard.SetTarget(_textSizeAnimation1, item2);
            Storyboard.SetTarget(_textSizeAnimationReverse1, item2);
            Storyboard.SetTargetProperty(_textSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            storyBoard.Begin();
            StackPanel panel = (StackPanel)item.Header;
            TextBlock block = (TextBlock)panel.Children[0];
            storyBoard.Children.Clear();

        }


        public TreeViewItem FindSignatureItems(TreeViewItem treeViewItem, string name)
        {

            StackPanel tempHeader1 = (StackPanel)treeViewItem.Header;

            // string Bezeichner = getNameFromPanel(tempHeader1);
            TextBlock text1 = (TextBlock)tempHeader1.Children[1];
            if (text1.Text.Equals(name))
            {

                this._foundItem = treeViewItem;
                if (name.Equals("ds:Reference"))
                {
                    this._tempReferenceCollection.Add(treeViewItem);
                }
                if (name.Equals("ds:Transform"))
                {
                    this._tempTransformCollection.Add(treeViewItem);

                }
                this._signatureCollection.Add(treeViewItem);
                return treeViewItem;

            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindSignatureItems(childItem, name);

            }
            if (this._foundItem != null)
            {
                return this._foundItem;
            }

            return null;
        }

        public TreeViewItem FindTreeViewItem(TreeViewItem treeViewItem, string name, int n)
        {

            StackPanel tempHeader1 = (StackPanel)treeViewItem.Header;
            string panelName = GetNameFromPanel(tempHeader1);
            if (panelName != null)
            {
                if (panelName.Equals(name))
                {
                    this._foundItem = treeViewItem;
                    StackPanel panel = (StackPanel)treeViewItem.Header;
                    int count = 0;
                    foreach (object obj in panel.Children)
                    {
                        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                        {
                            TextBlock tb = (TextBlock)obj;
                            if (tb.Name.Equals("attributeName"))
                            {
                                if (tb.Text.Trim().Equals("Id"))
                                {
                                    TextBlock block = (TextBlock)panel.Children[count + 1];
                                    this._foundItem = treeViewItem;
                                }
                            }
                        }
                        count++;
                    }
                    return treeViewItem;
                }
            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindTreeViewItem(childItem, name, 4);
            }
            if (this._foundItem != null)
            {
                return this._foundItem;
            }
            return null;
        }
        public TreeViewItem FindTreeViewItemById(TreeViewItem treeViewItem, string name, int n)
        {

            StackPanel tempHeader1 = (StackPanel)treeViewItem.Header;
            string panelName = GetNameFromPanel(tempHeader1);
            if (panelName != null)
            {
                if (panelName.Equals(name))
                {
                    this._foundItem = treeViewItem;
                    StackPanel panel = (StackPanel)treeViewItem.Header;
                    int count = 0;
                    foreach (object obj in panel.Children)
                    {
                        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                        {
                            TextBlock tb = (TextBlock)obj;
                            if (tb.Name.Equals("attributeName"))
                            {
                                if (tb.Text.Trim().Equals("Id"))
                                {
                                    TextBlock block = (TextBlock)panel.Children[count + 1];
                                    this._foundItem = treeViewItem;
                                }
                            }
                        }
                        count++;
                    }
                    return treeViewItem;
                }
            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindTreeViewItemById(childItem, name, 4);
            }
            if (this._foundItem != null)
            {
                return this._foundItem;
            }
            return null;
        }
        private string GetNameFromPanel(StackPanel panel)
        {
            foreach (object obj in panel.Children)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock tb = (TextBlock)obj;
                    if (tb.Name.Equals("tbName"))
                    {
                        string name = tb.Text;
                        if (!name.StartsWith("/"))
                        {

                            string[] splitter = name.Split(new Char[] { ':' });
                            name = splitter[splitter.Length - 1];
                            return name;
                        }
                        else
                        {
                            return name;
                        }
                    }
                }
            }
            return null;
        }


        private void AnimateTextBlock(string animatedBlockName)
        {
            DoubleAnimation widthAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            widthAnimation.AutoReverse = true;
            this.textBlock1.Inlines.Clear();
            this.textBlock1.Inlines.Add(_webMethod);
            this.textBlock1.Inlines.Add(_methodVisibility);
            this.textBlock1.Inlines.Add(_returnParam);
            this.textBlock1.Inlines.Add(_methodName);
            this._openBrace = new Run("(");
            this.textBlock1.Inlines.Add(_openBrace);
            this._comma = new Run(",");
            this.textBlock1.Inlines.Add(_intParams);
            TextRange intParamsText = new TextRange(_intParams.ContentStart, this._intParams.ContentEnd);
            TextRange stringParamsText = new TextRange(_stringParams.ContentStart, this._stringParams.ContentEnd);
            TextRange doubleParamsText = new TextRange(_doubleParams.ContentStart, this._doubleParams.ContentEnd);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(stringParamsText.Text.Equals("")))
                {
                    Run nochnRun = new Run(",");
                    this.textBlock1.Inlines.Add(nochnRun);
                    intParamsText = new TextRange(_intParams.ContentStart, this._intParams.ContentEnd);
                    stringParamsText = new TextRange(_stringParams.ContentStart, this._stringParams.ContentEnd);
                    doubleParamsText = new TextRange(_doubleParams.ContentStart, this._doubleParams.ContentEnd);

                }
                else
                {
                    if (!doubleParamsText.Text.Equals(""))
                    {
                        this.textBlock1.Inlines.Add(_comma);
                    }
                }
            }
            this.textBlock1.Inlines.Add(_stringParams);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(doubleParamsText.Text.Equals("")))
                {
                    this.textBlock1.Inlines.Add(_comma);
                }
            }
            this.textBlock1.Inlines.Add(_doubleParams);
            this._closeBrace = new Run(")");
            this.textBlock1.Inlines.Add(_closeBrace);

            switch (animatedBlockName)
            {
                case "methodName":
                    this._methodName.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "returnParam":
                    this._returnParam.BeginAnimation(Run.FontSizeProperty, widthAnimation);
                    break;
                case "int":
                    this._intParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "string":
                    this._stringParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "float":
                    this._doubleParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
            }
            textBlock1.Inlines.Add(_openMethodBrace);
            textBlock2.Visibility = Visibility.Visible;

        }
        public void VisualReturnParam(string returnType)
        {
            Run returnParam = new Run(" " + returnType);
            returnParam.Foreground = Brushes.Blue;
            this._returnParam = returnParam;
            this.AnimateTextBlock("returnParam");


        }
        public void VisualMethodName(string name)
        {
            Run visibility = new Run("public");
            visibility.Foreground = Brushes.Blue;
            Run methodName = new Run(" " + name);
            this._methodVisibility = visibility;
            this._methodName = methodName;
            this.AnimateTextBlock("methodName");

        }

        public void VisualParam(string parameterType, int parameterCount)
        {
            Bold bold = new Bold();
            string paramName = "";
            for (int i = 1; i <= parameterCount; i++)
            {

                Run typRun = new Run(parameterType);
                typRun.Foreground = Brushes.Blue;

                if (parameterType.Equals("int"))
                {
                    paramName = "intparam";
                }
                if (parameterType.Equals("string"))
                {
                    paramName = "stringparam";
                }
                if (parameterType.Equals("double"))
                {
                    paramName = "doubleparam";
                }
                Run nameRun;
                if (i < parameterCount)
                {
                    nameRun = new Run(" " + paramName + "" + i + ",");
                }
                else { nameRun = new Run(" " + paramName + "" + i); }
                bold.Inlines.Add(typRun);
                bold.Inlines.Add(nameRun);
            }
            switch (parameterType)
            {
                case "int":
                    this._intParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;
                case "string":
                    this._stringParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;
                case "double":
                    this._doubleParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;

            }



        }
        public void Save(string fileName)
        {
            if (this._webService.ServiceDescription != null)
            {
                ServiceDescription test = this._webService.ServiceDescription;
                StreamWriter stream = new StreamWriter(fileName);
                test.Write(stream);
                stream.Close();
            }
        }

        public void ShowWsdl(string wsdl)
        {
            this._webServiceDescription = wsdl;
        }
        public string GetStringToCompile()
        {
            TextRange methodCode = new TextRange(this.richTextBox1.Document.ContentStart, this.richTextBox1.Document.ContentEnd);
            TextRange endBrace = new TextRange(this.textBlock2.Inlines.FirstInline.ContentStart, this.textBlock2.Inlines.FirstInline.ContentEnd);
            string header = CopyTextBlockContentToString(this.textBlock1);

            string codeToCompile = header + methodCode.Text + endBrace.Text;
            return codeToCompile;
        }
        public string CopyTextBlockContentToString(TextBlock block)
        {
            string blockString = "";
            foreach (Inline inline in textBlock1.Inlines)
            {
                TextRange inlineRange = new TextRange(inline.ContentStart, inline.ContentEnd);
                blockString += inlineRange.Text;
            }
            return blockString;
        }

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange code = new TextRange(this.richTextBox1.Document.ContentStart, this.richTextBox1.Document.ContentEnd);
            if (_webService != null)
            {
                this._webService.WebServiceSettings.UserCode = code.Text;
            }



        }


        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (this._webService.CheckSignature() == true)
            {

            }
            else
            {
            }
        }
        private void SettingsPropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TargetFilename")
            {
                string filename = this._webService.WebServiceSettings.TargetFilename;

                {
                    this.Save(filename);
                }
            }
        }



        private void PlayButtonClickEventHandler(object sender, RoutedEventArgs e)
        {

            this.FindSignatureItems((TreeViewItem)this._soapInputItem.Items[0], "ds:Signature");
            if (this._signatureCollection.Count > 0)
            {
                InitializeAnimation();
                this._dispatcherTimer.Start();
            }
            else
            {// calculateBox.Text = "There is no signature in the message"; 
            }
        }

        private void ResetButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
            this._decryptionAnimation.DecryptionTimer.Stop();
            this._decryptionAnimation = new DecryptionAnimation(this);
            this.button1.IsEnabled = true;
            this._namespacesTable.Clear();
            this._signatureCollection.Clear();
            this._tempReferenceCollection.Clear();
            this._animationStepsTextBox.Clear();

            this._dispatcherTimer.Stop();
            this._animationController.ControllerTimer.Stop();
            this._decryptionTimer.Stop();


            this.ResetSoapInputItem();

            this._animationStepsTextBox.Clear();

            //  this.soapInput = new TreeViewItem();


        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._actualSignatureNumber = 1;
            //  status = (int)this.slider1.Value+1;
        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {

            this._animationController.GetTotalSecurityElementsNumber();

            this._decryptionAnimation.initializeAnimation();
            this._decryptionAnimation.fillEncryptedDataTreeviewElements();

        }

        private void Button1Click3EventHandler(object sender, RoutedEventArgs e)
        {
            if (this._soapInputItem != null && this._soapInputItem.Items.Count>0)
            {
                this._animationController.InitializeAnimation();
                this.button1.IsEnabled = false;
            }
            

        }
        #endregion

        private void CompileButtonClickEventHandler(object sender, RoutedEventArgs e)
        {

            this.textBox3.Clear();
            this._webService.Compile(this.GetStringToCompile());
        }
        private void AnimationTreeViewSelectedItemChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this._isscrolling)
            {
                return;
            }
           
             if (e.NewValue != null)
             {
                 this._animationTreeView.InvalidateMeasure();
                 GeneralTransform generalTransForm = (e.NewValue as TreeViewItem).TransformToVisual(this._animationTreeView);

                 Point p = generalTransForm.Transform(new Point(0, 0));
                 Rect p2 = generalTransForm.TransformBounds(new Rect());

                 double offset = this._scrollViewer.VerticalOffset;
         
                 {
                     if ((p.Y - offset) > 0)
                     {
                         this._arrowImage.Margin = new Thickness((e.NewValue as TreeViewItem).Margin.Left, p.Y - offset, (e.NewValue as TreeViewItem).Margin.Right, (e.NewValue as TreeViewItem).Margin.Bottom);
                         //Console.WriteLine(p.Y - offset);
                     }
                     else
                     {
                         this._arrowImage.Margin = new Thickness((e.NewValue as TreeViewItem).Margin.Left, 0, (e.NewValue as TreeViewItem).Margin.Right, (e.NewValue as TreeViewItem).Margin.Bottom);
                     }
                 }

             }

        }

       private void ScrollViewerScrollChangedEventHandler(object sender, ScrollChangedEventArgs e)
        {

            this._isscrolling = true;

            {
                RoutedEventArgs args = new RoutedEventArgs(TreeViewItem.ToolTipOpeningEvent);


                this._animationTreeView.InvalidateMeasure();
                if (this._animationTreeView.SelectedItem != null)
                {
                    GeneralTransform generalTransForm = (this._animationTreeView.SelectedItem as TreeViewItem).TransformToAncestor(this._animationTreeView);

                    Point p = generalTransForm.Transform(new Point(0, 0));
                    Rect p2 = generalTransForm.TransformBounds(new Rect());

                    double offset = this._scrollViewer.VerticalOffset;

                    {
                        if ((p.Y - offset) > 0)
                        {
                            this._arrowImage.Margin = new Thickness((this._animationTreeView.SelectedItem as TreeViewItem).Margin.Left, p.Y - offset, (this._animationTreeView.SelectedItem as TreeViewItem).Margin.Right, (this._animationTreeView.SelectedItem as TreeViewItem).Margin.Bottom);
                            //Console.WriteLine(p.Y - offset);
                        }
                    }
                }
            }
            this._isscrolling = false;
        }


    }


}
