using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;


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
        private readonly ArrayList _signatureCollection;
        private readonly ArrayList _tempReferenceCollection;
        private readonly ArrayList _tempTransformCollection;
        private string _webServiceDescription;
        private string _lastURI;
        private Run _methodVisibility;
        private Run _returnParam;
        private Run _methodName;
        private Run _openBrace;
        private Run _closeBrace;
        private Run _comma;
        private readonly Run _openMethodBrace;
        private readonly Bold _webMethod;
        private readonly FlowDocument _doc;
        private Bold _intParams;
        private Bold _stringParams;
        private Bold _doubleParams;
        private System.ComponentModel.SortDescription _sortDescription;
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly DispatcherTimer _decryptionTimer;
        private readonly DispatcherTimer _referenceTimer;
        private readonly DispatcherTimer _transformTimer;
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
        private readonly int _actualReferenceNumber;
        private int _referenceNumber;
        private int _transformNumber;
        private int _transformCount;
        private AnimationController _animationController;
        private readonly Hashtable _namespacesTable;
        private DecryptionAnimation _decryptionAnimation;
        private readonly WebService _webService;
        private readonly Dictionary<string, TreeViewItem> _nodeToTreeViewItemDictionary;

        #endregion

        #region Properties

        public WebService WebService => _webService;

        public Hashtable NameSpacesTable => _namespacesTable;


        public DecryptionAnimation DecryptionAnimation => _decryptionAnimation;

        public TreeViewItem WSDLTreeViewItem
        {
            get => _wsdlTreeViewItem;
            set => _wsdlTreeViewItem = value;
        }

        public TreeViewItem SoapInputItem
        {
            get => _soapInputItem;
            set => _soapInputItem = value;
        }
        public AnimationController AnimationController => _animationController;

        public DispatcherTimer SignatureTimer => _dispatcherTimer;

        #endregion


        #region Constructor
        public WebServicePresentation(WebService webService)
        {
            InitializeComponent();
            _nodeToTreeViewItemDictionary = new Dictionary<string, TreeViewItem>();
            _actualSignatureNumber = 1;
            _decryptionAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            _animationController = new AnimationController(this);
            _signatureCollection = new ArrayList();
            _tempTransformCollection = new ArrayList();
            Paragraph par = (Paragraph)richTextBox1.Document.Blocks.FirstBlock;
            par.LineHeight = 5;
            _status = 1;
            _dispatcherTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _referenceTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _decryptionTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _transformTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            _textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickEventHandler);
            _referenceTimer.Tick += new EventHandler(ReferenceTimerTickEventHandler);
            _transformTimer.Tick += new EventHandler(TransformTimerTickEventHandler);
            _doc = new FlowDocument();
            _webMethod = new Bold(new Run("[WebMethod]" + "\n"));
            _methodVisibility = new Run();
            _returnParam = new Run();
            _methodName = new Run();
            _intParams = new Bold();
            _stringParams = new Bold();
            _doubleParams = new Bold();
            _openBrace = new Run();
            _closeBrace = new Run();
            _comma = new Run(",");
            _openMethodBrace = new Run("\n{");
            textBlock2.Inlines.Add(new Run("}"));
            textBlock2.Visibility = Visibility.Visible;
            VisualMethodName("methodName");
            richTextBox1.Document = _doc;
            textBlock1.Inlines.Add(_webMethod);
            textBlock1.Inlines.Add(_methodVisibility);
            textBlock1.Inlines.Add(_returnParam);
            textBlock1.Inlines.Add(_methodName);
            textBlock1.Inlines.Add(_openBrace);
            textBlock1.Inlines.Add(_intParams);
            textBlock1.Inlines.Add(_stringParams);
            textBlock1.Inlines.Add(_doubleParams);
            textBlock1.Inlines.Add(_closeBrace);
            textBlock1.Inlines.Add(_openMethodBrace);
            _wsdlTreeViewItem = new TreeViewItem();
            _foundItem = new TreeViewItem();
            _wsdlTreeViewItem.Header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            _wsdlTreeViewItem.IsExpanded = true;
            _soapInputItem = new TreeViewItem();
            TextBlock block = new TextBlock
            {
                Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            };
            StackPanel panel = new StackPanel();
            panel.Children.Add(block);
            _soapInputItem.Header = panel;
            _soapInputItem.IsExpanded = true;
            _webService = webService;
            _namespacesTable = new Hashtable();
            _sortDescription = new System.ComponentModel.SortDescription("param", System.ComponentModel.ListSortDirection.Ascending);
            _tempReferenceCollection = new ArrayList();
            webService.Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            _animationTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(AnimationTreeViewSelectedItemChangedEventHandler);
            _scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChangedEventHandler);
        }


        #endregion

        #region Methods

        public void ResetSoapInputItem()
        {
            _nodeToTreeViewItemDictionary.Clear();
            _webService.InputString = _webService.InputString;
            _dispatcherTimer.Stop();
            _decryptionTimer.Stop();
            _referenceTimer.Stop();
            _animationController.ControllerTimer.Stop();
            _decryptionAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            _animationController = new AnimationController(this);
            _status = 1;
            _signaturenumber = 0;


        }

        public void CopyXmlToTreeView(XmlNode xmlNode, TreeViewItem treeViewItemParent)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
            if (xmlNode != null)
            {
                TreeViewItem item = new TreeViewItem
                {
                    IsExpanded = true
                };
                StackPanel panel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal
                };
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
                if (!_nodeToTreeViewItemDictionary.ContainsKey(xmlNode.OuterXml))
                {
                    xmlNode.Normalize();
                    _nodeToTreeViewItemDictionary.Add(xmlNode.OuterXml.ToString(), item);
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
                            _lastURI = xmlNode.NamespaceURI; ;
                            CopyXmlToTreeView(child, item);
                        }
                    }
                    StackPanel panel1 = new StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal
                    };
                    TextBlock elem1 = new TextBlock();
                    TextBlock tbTagOpen3 = new TextBlock
                    {
                        Name = "tbTagOpen",
                        Text = "<"
                    };
                    panel1.Children.Insert(0, tbTagOpen3);
                    elem1.Name = "tbName";
                    elem1.Text = "/" + xmlNode.Name;
                    panel1.Children.Add(elem1);
                    TextBlock tbTagClose3 = new TextBlock
                    {
                        Name = "tbTagClose",
                        Text = ">"
                    };
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
                    TextBlock tbText = new TextBlock
                    {
                        Name = "TextNode",
                        Text = xmlNode.Value
                    };
                    TextBlock emptyTextBlock = new TextBlock
                    {
                        Text = ""
                    };
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
                    TextBlock name = new TextBlock
                    {
                        Text = " " + tempAttribute.Name,
                        Name = "attributeName"
                    };
                    TextBlock value = new TextBlock
                    {
                        Name = "attributeValue",
                        Text = " =\"" + tempAttribute.Value + "\""
                    };
                    SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                    value.Foreground = valueBrush;
                    panel.Children.Add(name);
                    panel.Children.Add(value);

                }
                else
                {
                    if (!_namespacesTable.ContainsValue(tempAttribute.Value))
                    {
                        _namespacesTable.Add(tempAttribute.Value, tempAttribute.Value);
                        TextBlock name = new TextBlock
                        {
                            Text = " " + tempAttribute.Name
                        };
                        TextBlock value = new TextBlock
                        {
                            Text = " =\"" + tempAttribute.Value + "\""
                        };
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
                _namespacesTable.Add(nameSpace, nameSpace);
                TextBlock xmlns = new TextBlock
                {
                    Name = "xmlns",
                    Text = " xmlns"
                };
                TextBlock prefix = new TextBlock
                {
                    Name = "xmlnsPrefix"
                };
                if (!nameSpacePrefix.Equals(""))
                { prefix.Text = ":" + nameSpacePrefix; }
                else { prefix.Text = ""; }
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock
                {
                    Name = "xmlnsValue",
                    Text = "=" + "\"" + nameSpace + "\"",
                    Foreground = valueBrush
                };
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
        }

        private void DispatcherTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (_status)
            {
                case 1:

                    _signatureNumber = _webService.GetSignatureNumber();
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    _animationStepsTextBox.Text += "\n Check for Signature Element";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    _animationTreeView.Items.Refresh();
                    FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "Signature", 1).BringIntoView();
                    AnimateFoundElements((TreeViewItem)_signatureCollection[_signaturenumber], (TreeViewItem)_signatureCollection[_signaturenumber]);
                    _status = 2;
                    slider1.Value++;
                    break;
                case 2:
                    _animationStepsTextBox.Text += "\n Canonicalize SignedInfo";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "SignedInfo", 1).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/SignedInfo", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/ds:SignedInfo>", _actualSignatureNumber));
                    _status = 3;
                    slider1.Value++;
                    break;
                case 3:
                    _animationStepsTextBox.Text += "\n -->Find Canonicalization Algorithm";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "CanonicalizationMethod", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/CanonicalizationMethod", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/ds:CanonicalizationMethod>", _actualSignatureNumber));
                    _status = 4;
                    slider1.Value++;
                    break;
                case 4:
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindSignatureItems((TreeViewItem)_signatureCollection[_signaturenumber], "ds:Reference");
                    InitializeReferenceAnimation();
                    _status = 5;
                    _dispatcherTimer.Stop();
                    slider1.Value++;
                    break;

                case 5:
                    _animationStepsTextBox.Text += "\n Signature Validation";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    _animationStepsTextBox.Text += "\n -> Find Signature Method";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem(_soapInputItem, "SignatureMethod", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "SignatureMethod", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/ds:SignatureMethod", _actualSignatureNumber));
                    _status = 6;
                    slider1.Value++;


                    break;
                case 6:
                    _animationStepsTextBox.Text += "\n Get public key for signature validation";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "KeyInfo", 1).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "KeyInfo", 1), FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/ds:KeyInfo", 1));
                    _status = 7;
                    break;
                case 7:
                    _animationStepsTextBox.Text += "\n -> Validate SignatureValue over SignedInfo";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "SignatureValue", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "SignatureValue", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_signatureCollection[_signaturenumber], "/ds:SignatureValue", _actualSignatureNumber));
                    _dispatcherTimer.Stop();
                    _status = 8;
                    slider1.Value++;
                    _animationController.ControllerTimer.Start();
                    break;
                case 8:
                    _tempReferenceCollection.Clear();
                    _animationController.ControllerTimer.Start();
                    _signatureNumber = _webService.GetSignatureNumber();
                    _actualSignatureNumber++;
                    if (_signaturenumber + 1 < _signatureNumber)
                    {
                        _signaturenumber++;
                        _status = 1;
                        slider1.Value++;
                    }

                    break;
            }


        }
        private void ReferenceTimerTickEventHandler(object sender, EventArgs e)
        {
            int n = _webService.Validator.GetReferenceNumber(_signaturenumber);
            switch (_referenceStatus)
            {
                case 1:
                    _referenceTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    _animationStepsTextBox.Text += "\n Reference Validation";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    _referenceStatus++;
                    break;
                case 2:
                    _animationStepsTextBox.Text += "\n -> Find Reference Element";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    // this.findSignatureItems((TreeViewItem)signatureCollection[this.i], "ds:Reference").BringIntoView() ;
                    FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "Reference", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "Reference", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "/ds:Reference", _actualSignatureNumber));
                    _referenceStatus++;
                    break;

                case 3:
                    _animationStepsTextBox.Text += "\n -> Get referenced Element";
                    XmlNode node = _webService.Validator.GetSignatureReferenceElement(_signaturenumber) as XmlNode;
                    node.Normalize();
                    TreeViewItem referenceItem = _nodeToTreeViewItemDictionary[(node.OuterXml).ToString()];
                    //    this.FindTreeViewItem(this._soapInputItem, this._webService.Validator.GetSignatureReferenceName(_signaturenumber), this._actualReferenceNumber).BringIntoView();
                    referenceItem.BringIntoView();
                    AnimateFoundElements(referenceItem, referenceItem);
                    _referenceStatus++;
                    break;

                case 4:
                    _animationStepsTextBox.Text += "\n  -> Apply Transforms";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "Transforms", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "Transforms", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "/ds:Transforms", _actualSignatureNumber));
                    _transformCount = _webService.Validator.GetTransformsCounter(_signaturenumber, _referenceNumber);
                    _referenceTimer.Stop();
                    _referenceStatus++;
                    FindSignatureItems((TreeViewItem)_tempReferenceCollection[_referenceNumber], "ds:Transform");
                    InitializeTransformAnimation();

                    break;
                case 5:
                    _animationStepsTextBox.Text += "\n  -> Digest References";
                    _animationStepsTextBox.Text += "\n    -> Find DigestAlgorithm";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "DigestMethod", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "DigestMethod", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "/ds:DigestMethod", _actualSignatureNumber));
                    _referenceStatus++;
                    break;
                case 6:
                    _animationStepsTextBox.Text += "\n    -> Calculated DigestValue:" + "\n       " + _webService.Validator.DigestElement(_signaturenumber, _referenceNumber);
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    _referenceStatus++;
                    break;
                case 7:
                    _animationStepsTextBox.Text += "\n    -> Compare the DigestValues:";
                    _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                    FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "DigestValue", _actualSignatureNumber).BringIntoView();
                    AnimateFoundElements(FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "DigestValue", _actualSignatureNumber), FindTreeViewItem((TreeViewItem)_tempReferenceCollection[_referenceNumber], "/ds:DigestValue", _actualSignatureNumber));
                    _referenceStatus++;
                    break;
                case 8:
                    if (_webService.Validator.CompareDigestValues(_signaturenumber, _referenceNumber, _webService.Validator.DigestElement(_signaturenumber, _referenceNumber)))
                    {

                        _animationStepsTextBox.Text += "\n Reference Validation succesfull";
                        _animationStepsTextBox.ScrollToLine(_animationStepsTextBox.LineCount - 1);
                        _referenceValid = true;
                        _referenceStatus++;
                    }
                    else
                    {
                        _animationStepsTextBox.Text += "\n Reference Validation failed";
                        _referenceStatus++;
                        _referenceValid = false;
                    }

                    break;
                case 9:
                    _referenceTimer.Stop();
                    _referenceNumber++;
                    // status = 7;
                    _dispatcherTimer.Start();
                    break;
            }

        }
        private void TransformTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (_transformstatus)
            {
                case 1:
                    _transformTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    _animationStepsTextBox.Text += "\n Make Transforms";
                    _transformstatus++;

                    break;
                case 2:
                    _animationStepsTextBox.Text += "\n -> Find Transform";
                    TreeViewItem tempTransform = (TreeViewItem)_tempTransformCollection[_transformNumber];
                    tempTransform.BringIntoView();
                    AnimateFoundElements(tempTransform, tempTransform);
                    _transformstatus++;
                    break;

                case 3:
                    _animationStepsTextBox.Text += "\n  ->execute Transform";
                    _animationStepsTextBox.Text += "\n" + _webService.Validator.MakeTransforms(_signaturenumber, _referenceNumber, _transformNumber);
                    _transformstatus++;
                    break;

                case 4:
                    if (_transformNumber + 1 < _transformCount)
                    {
                        _transformNumber++;
                        _transformstatus = 2;
                        slider1.Value++;
                    }
                    else
                    {
                        _transformTimer.Stop();
                        _referenceTimer.Start();

                        _referenceStatus = 5;
                    }


                    break;




            }
        }
        private void InitializeTransformAnimation()
        {
            _transformstatus = 1;
            _transformNumber = 0;
            _transformTimer.Start();


        }

        private void InitializeReferenceAnimation()
        {

            _referenceStatus = 1;
            _referenceNumber = 0;
            _referenceTimer.Start();


        }
        public void InitializeAnimation()
        {
            _status = 1;
            _signaturenumber = 0;

        }

        private void AnimateFoundElements(TreeViewItem item, TreeViewItem item2)
        {
            item.IsSelected = true;
            Storyboard storyBoard = new Storyboard();
            _textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            _textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
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

                _foundItem = treeViewItem;
                if (name.Equals("ds:Reference"))
                {
                    _tempReferenceCollection.Add(treeViewItem);
                }
                if (name.Equals("ds:Transform"))
                {
                    _tempTransformCollection.Add(treeViewItem);

                }
                _signatureCollection.Add(treeViewItem);
                return treeViewItem;

            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindSignatureItems(childItem, name);

            }
            if (_foundItem != null)
            {
                return _foundItem;
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
                    _foundItem = treeViewItem;
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
                                    _foundItem = treeViewItem;
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
            if (_foundItem != null)
            {
                return _foundItem;
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
                    _foundItem = treeViewItem;
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
                                    _foundItem = treeViewItem;
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
            if (_foundItem != null)
            {
                return _foundItem;
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

                            string[] splitter = name.Split(new char[] { ':' });
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
            DoubleAnimation widthAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1))
            {
                AutoReverse = true
            };
            textBlock1.Inlines.Clear();
            textBlock1.Inlines.Add(_webMethod);
            textBlock1.Inlines.Add(_methodVisibility);
            textBlock1.Inlines.Add(_returnParam);
            textBlock1.Inlines.Add(_methodName);
            _openBrace = new Run("(");
            textBlock1.Inlines.Add(_openBrace);
            _comma = new Run(",");
            textBlock1.Inlines.Add(_intParams);
            TextRange intParamsText = new TextRange(_intParams.ContentStart, _intParams.ContentEnd);
            TextRange stringParamsText = new TextRange(_stringParams.ContentStart, _stringParams.ContentEnd);
            TextRange doubleParamsText = new TextRange(_doubleParams.ContentStart, _doubleParams.ContentEnd);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(stringParamsText.Text.Equals("")))
                {
                    Run nochnRun = new Run(",");
                    textBlock1.Inlines.Add(nochnRun);
                    intParamsText = new TextRange(_intParams.ContentStart, _intParams.ContentEnd);
                    stringParamsText = new TextRange(_stringParams.ContentStart, _stringParams.ContentEnd);
                    doubleParamsText = new TextRange(_doubleParams.ContentStart, _doubleParams.ContentEnd);

                }
                else
                {
                    if (!doubleParamsText.Text.Equals(""))
                    {
                        textBlock1.Inlines.Add(_comma);
                    }
                }
            }
            textBlock1.Inlines.Add(_stringParams);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(doubleParamsText.Text.Equals("")))
                {
                    textBlock1.Inlines.Add(_comma);
                }
            }
            textBlock1.Inlines.Add(_doubleParams);
            _closeBrace = new Run(")");
            textBlock1.Inlines.Add(_closeBrace);

            switch (animatedBlockName)
            {
                case "methodName":
                    _methodName.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "returnParam":
                    _returnParam.BeginAnimation(Run.FontSizeProperty, widthAnimation);
                    break;
                case "int":
                    _intParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "string":
                    _stringParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "float":
                    _doubleParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
            }
            textBlock1.Inlines.Add(_openMethodBrace);
            textBlock2.Visibility = Visibility.Visible;

        }
        public void VisualReturnParam(string returnType)
        {
            Run returnParam = new Run(" " + returnType)
            {
                Foreground = Brushes.Blue
            };
            _returnParam = returnParam;
            AnimateTextBlock("returnParam");


        }
        public void VisualMethodName(string name)
        {
            Run visibility = new Run("public")
            {
                Foreground = Brushes.Blue
            };
            Run methodName = new Run(" " + name);
            _methodVisibility = visibility;
            _methodName = methodName;
            AnimateTextBlock("methodName");

        }

        public void VisualParam(string parameterType, int parameterCount)
        {
            Bold bold = new Bold();
            string paramName = "";
            for (int i = 1; i <= parameterCount; i++)
            {

                Run typRun = new Run(parameterType)
                {
                    Foreground = Brushes.Blue
                };

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
                    _intParams = bold;
                    AnimateTextBlock(parameterType);
                    break;
                case "string":
                    _stringParams = bold;
                    AnimateTextBlock(parameterType);
                    break;
                case "double":
                    _doubleParams = bold;
                    AnimateTextBlock(parameterType);
                    break;

            }



        }
        public void Save(string fileName)
        {
            if (_webService.ServiceDescription != null)
            {
                ServiceDescription test = _webService.ServiceDescription;
                StreamWriter stream = new StreamWriter(fileName);
                test.Write(stream);
                stream.Close();
            }
        }

        public void ShowWsdl(string wsdl)
        {
            _webServiceDescription = wsdl;
        }
        public string GetStringToCompile()
        {
            TextRange methodCode = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
            TextRange endBrace = new TextRange(textBlock2.Inlines.FirstInline.ContentStart, textBlock2.Inlines.FirstInline.ContentEnd);
            string header = CopyTextBlockContentToString(textBlock1);

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
            TextRange code = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
            if (_webService != null)
            {
                _webService.WebServiceSettings.UserCode = code.Text;
            }



        }


        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (_webService.CheckSignature() == true)
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
                string filename = _webService.WebServiceSettings.TargetFilename;

                {
                    Save(filename);
                }
            }
        }



        private void PlayButtonClickEventHandler(object sender, RoutedEventArgs e)
        {

            FindSignatureItems((TreeViewItem)_soapInputItem.Items[0], "ds:Signature");
            if (_signatureCollection.Count > 0)
            {
                InitializeAnimation();
                _dispatcherTimer.Start();
            }
            else
            {// calculateBox.Text = "There is no signature in the message"; 
            }
        }

        private void ResetButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
            _decryptionAnimation.DecryptionTimer.Stop();
            _decryptionAnimation = new DecryptionAnimation(this);
            button1.IsEnabled = true;
            _namespacesTable.Clear();
            _signatureCollection.Clear();
            _tempReferenceCollection.Clear();
            _animationStepsTextBox.Clear();

            _dispatcherTimer.Stop();
            _animationController.ControllerTimer.Stop();
            _decryptionTimer.Stop();


            ResetSoapInputItem();

            _animationStepsTextBox.Clear();

            //  this.soapInput = new TreeViewItem();


        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _actualSignatureNumber = 1;
            //  status = (int)this.slider1.Value+1;
        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {

            _animationController.GetTotalSecurityElementsNumber();

            _decryptionAnimation.initializeAnimation();
            _decryptionAnimation.fillEncryptedDataTreeviewElements();

        }

        private void Button1Click3EventHandler(object sender, RoutedEventArgs e)
        {
            if (_soapInputItem != null && _soapInputItem.Items.Count > 0)
            {
                _animationController.InitializeAnimation();
                button1.IsEnabled = false;
            }


        }
        #endregion

        private void CompileButtonClickEventHandler(object sender, RoutedEventArgs e)
        {

            textBox3.Clear();
            _webService.Compile(GetStringToCompile());
        }
        private void AnimationTreeViewSelectedItemChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isscrolling)
            {
                return;
            }

            if (e.NewValue != null)
            {
                _animationTreeView.InvalidateMeasure();
                GeneralTransform generalTransForm = (e.NewValue as TreeViewItem).TransformToVisual(_animationTreeView);

                Point p = generalTransForm.Transform(new Point(0, 0));
                Rect p2 = generalTransForm.TransformBounds(new Rect());

                double offset = _scrollViewer.VerticalOffset;

                {
                    if ((p.Y - offset) > 0)
                    {
                        _arrowImage.Margin = new Thickness((e.NewValue as TreeViewItem).Margin.Left, p.Y - offset, (e.NewValue as TreeViewItem).Margin.Right, (e.NewValue as TreeViewItem).Margin.Bottom);
                        //Console.WriteLine(p.Y - offset);
                    }
                    else
                    {
                        _arrowImage.Margin = new Thickness((e.NewValue as TreeViewItem).Margin.Left, 0, (e.NewValue as TreeViewItem).Margin.Right, (e.NewValue as TreeViewItem).Margin.Bottom);
                    }
                }

            }

        }

        private void ScrollViewerScrollChangedEventHandler(object sender, ScrollChangedEventArgs e)
        {

            _isscrolling = true;

            {
                RoutedEventArgs args = new RoutedEventArgs(TreeViewItem.ToolTipOpeningEvent);


                _animationTreeView.InvalidateMeasure();
                if (_animationTreeView.SelectedItem != null)
                {
                    GeneralTransform generalTransForm = (_animationTreeView.SelectedItem as TreeViewItem).TransformToAncestor(_animationTreeView);

                    Point p = generalTransForm.Transform(new Point(0, 0));
                    Rect p2 = generalTransForm.TransformBounds(new Rect());

                    double offset = _scrollViewer.VerticalOffset;

                    {
                        if ((p.Y - offset) > 0)
                        {
                            _arrowImage.Margin = new Thickness((_animationTreeView.SelectedItem as TreeViewItem).Margin.Left, p.Y - offset, (_animationTreeView.SelectedItem as TreeViewItem).Margin.Right, (_animationTreeView.SelectedItem as TreeViewItem).Margin.Bottom);
                            //Console.WriteLine(p.Y - offset);
                        }
                    }
                }
            }
            _isscrolling = false;
        }


    }


}
