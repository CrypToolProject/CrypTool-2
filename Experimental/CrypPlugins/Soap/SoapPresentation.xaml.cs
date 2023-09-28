using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Soap
{
    /// <summary>
    /// Interaktionslogik für SoapPresentation.xaml
    /// </summary>
    public partial class SoapPresentation : UserControl
    {
        #region Fields

        private string _lastURI;
        private Soap _soap;
        private TreeViewItem _originalSoapItem;
        private TreeViewItem _securedSoapItem;
        private readonly TreeViewItem _foundItem = null;
        public Hashtable _namespacesTable;
        private SignatureAnimator _signatureAnimator;
        private EncryptionAnimator _encryptionAnimator;

        #endregion


        #region Properties

        public bool animationRunning;

        public TreeViewItem OriginalSoapItem
        {
            get => _originalSoapItem;
            set => _originalSoapItem = value;
        }

        public TreeViewItem SecuredSoapItem
        {
            get => _securedSoapItem;
            set => _securedSoapItem = value;
        }

        #endregion

        #region Constructor

        public SoapPresentation(Soap soap)
        {
            InitializeComponent();
            _soap = soap;
            animationRunning = false;
            _namespacesTable = new Hashtable();
        }

        #endregion

        #region Methods

        public void StartStopAnimation()
        {
            if (animationRunning)
            {
                if (_signatureAnimator != null)
                {
                    _signatureAnimator.startstopAnimation();
                }
                if (_encryptionAnimator != null)
                {
                    _encryptionAnimator.playpause();

                }
            }
            else
            {
                _soap.CreateInfoMessage("No animation running");
            }
        }

        public void CopyXmlToTreeView(XmlNode xmlNode, TreeViewItem treeViewItemParent)
        {
            _namespacesTable.Clear();
            if (_securedSoapItem != null)
            {
                ClearBoxes(_securedSoapItem);
            }
            CopyXmlToTreeViewReal(xmlNode, treeViewItemParent);
        }

        private void CopyXmlToTreeViewReal(XmlNode xmlNode, TreeViewItem treeViewItemParent)
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

                if (!xmlNode.NodeType.ToString().Equals("Text"))
                {
                    item.Name = "OpenItemXmlNode";
                    panel.Name = "OpenPanelXMLNode";
                    TreeViewItem closeitem = new TreeViewItem();
                    panel.Children.Insert(0, tbTagOpen);
                    panel.Children.Add(tbName);
                    if (!xmlNode.NamespaceURI.Equals(""))
                    {
                        InsertNamespace(panel, xmlNode.NamespaceURI, xmlNode.Prefix);
                    }
                    if (xmlNode.Attributes != null)
                    {
                        InsertAttributes(panel, xmlNode.Attributes);
                    }
                    panel.Children.Add(tbTagClose);
                    if (!animationRunning)
                    {
                        XmlNode[] ElementsToEnc = _soap.GetElementsToEncrypt();

                        foreach (XmlNode node in ElementsToEnc)
                        {
                            if (node.Name.Equals(xmlNode.Name))
                            {
                                AddOpenLockToPanel(panel, xmlNode.Name);
                            }
                        }
                        XmlNode[] EncryptedElements = _soap.GetEncryptedElements();
                        foreach (XmlNode node in EncryptedElements)
                        {
                            if (node.Name.Equals(xmlNode.Name))
                            {
                                AddClosedLockToPanel(panel);
                            }
                        }
                        XmlNode[] signedElements = _soap.GetSignedElements();
                        foreach (XmlNode node in signedElements)
                        {
                            if (node.Name.Equals(xmlNode.Name))
                            {
                                string id = "";
                                foreach (XmlAttribute att in node.Attributes)
                                {
                                    if (att.Name.Equals("Id"))
                                    {
                                        id = att.Value;
                                    }
                                }
                                AddSignedIconToPanel(panel, xmlNode.Name, id);
                            }
                        }
                        XmlNode[] elementsToSign = _soap.GetElementsToSign();
                        foreach (XmlNode node in elementsToSign)
                        {
                            if (node.Name.Equals(xmlNode.Name))
                            {
                                AddToSignIconToPanel(panel, xmlNode.Name);
                            }
                        }
                        XmlNode[] parameters = _soap.GetParameterToEdit();
                        foreach (XmlNode node in parameters)
                        {
                            if (node.Name.Equals(xmlNode.Name))
                            {
                                AddEditImageToPanel(panel, xmlNode.Name);
                            }
                        }
                    }
                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    treeViewItemParent.Items.Add(item);
                    if (xmlNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xmlNode.ChildNodes)
                        {
                            _lastURI = xmlNode.NamespaceURI; ;
                            CopyXmlToTreeViewReal(child, item);
                        }
                    }
                    StackPanel panel1 = new StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal
                    };
                    TextBlock elem1Open = new TextBlock
                    {
                        Text = "<"
                    };
                    panel1.Children.Insert(0, elem1Open);
                    TextBlock elem1Close = new TextBlock
                    {
                        Text = ">"
                    };
                    TextBlock elem1Name = new TextBlock
                    {
                        Text = "/" + xmlNode.Name
                    };
                    panel1.Children.Add(elem1Name);
                    panel1.Children.Add(elem1Close);

                    closeitem.Header = panel1;

                    treeViewItemParent.Items.Add(closeitem);
                }
                else
                {
                    item.Name = "OpenItemTextNode";
                    panel.Name = "OpenPanelTextNode";
                    TextBlock tbText = new TextBlock
                    {
                        Name = "TextNode",
                        Text = xmlNode.Value
                    };
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    treeViewItemParent.Items.Add(item);
                }
            }
        }

        private void AddOpenLockToPanel(StackPanel panel, string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.OpenLock;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image myImage2 = new Image
            {
                Source = bi,
                Name = name
            };
            int i = panel.Children.Count;
            myImage2.MouseLeftButtonDown += new MouseButtonEventHandler(IamgeMouseLeftButtonDownEventHandler);
            myImage2.ToolTip = "Click this picture to encrypt the <" + name + "> element";
            myImage2.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHandler);
            myImage2.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);
            panel.Children.Add(myImage2);
        }

        private void AddClosedLockToPanel(StackPanel panel)
        {

            System.Drawing.Bitmap bitmap = Resource1.ClosedLock;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image closedLockImage = new Image
            {
                Source = bi,
                Name = "EncryptedData"
            };
            int i = panel.Children.Count;
            closedLockImage.ToolTip = "This Element is encrypted";
            panel.Children.Add(closedLockImage);
            closedLockImage.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHandler);
            closedLockImage.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);
        }

        private void AddSignedIconToPanel(StackPanel panel, string name, string id)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.ClosedCert;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image signedImage = new Image
            {
                Source = bi,
                Name = name + "_" + id
            };
            int i = panel.Children.Count;
            signedImage.ToolTip = "The Element: " + name + " " + id + "is signed";
            panel.Children.Add(signedImage);
            signedImage.MouseDown += new MouseButtonEventHandler(SignedImageMouseDownEventHandler);
            signedImage.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHandler);
            signedImage.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);
        }

        private void SignedImageMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!animationRunning)
            {
                ClearBoxes(_securedSoapItem);
                _namespacesTable.Clear();
                Image signIcon = (Image)sender;
                string[] name = signIcon.Name.Split(new char[] { '_' });
                string id = name[2];
                _soap.RemoveSignature(id);
            }
        }

        private void AddToSignIconToPanel(StackPanel panel, string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.OpenCert;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image signImage = new Image
            {
                Source = bi,
                Name = name
            };
            int i = panel.Children.Count;
            signImage.ToolTip = "Click here to sign the: " + name + " Element";
            panel.Children.Add(signImage);
            signImage.MouseDown += new MouseButtonEventHandler(ImageMouseDownEventHandler);
            signImage.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHandler);
            signImage.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);
        }

        private void AddEditImageToPanel(StackPanel panel, string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.EditIcon;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image editImage = new Image
            {
                Source = bi,
                Name = name
            };
            int i = panel.Children.Count;
            editImage.ToolTip = "Click here or on the element name to edit the: " + name + " Element";
            panel.Children.Add(editImage);
            editImage.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHandler);
            editImage.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);
        }

        private void ImageMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!animationRunning)
            {

                if (_soap.GetSignatureAlgorithm() == null)
                {
                    _soap.CreateErrorMessage("You have to select a signature algorithm before you can sign parts of the message");
                }
                else
                {
                    _namespacesTable.Clear();
                    Image signIcon = (Image)sender;
                    string[] test = signIcon.Name.Split(new char[] { '_' });
                    string name = test[0] + ":" + test[1];
                    XmlElement[] array = new XmlElement[1];
                    array[0] = (XmlElement)_soap.SecuredSoap.GetElementsByTagName(name)[0];
                    if (!_soap.GetXPathTransForm())
                    {
                        _soap.AddIdToElement(name);
                    }
                    if (!_soap.GetShowSteps())
                    {
                        if (!_soap.CheckSecurityHeader())
                        {
                            _soap.CreateSecurityHeaderAndSoapHeader();
                        }
                        _soap.SignElementsManual(array);
                        _soap.ShowSecuredSoap();
                    }
                    else
                    {
                        animationRunning = true;

                        _signatureAnimator = new SignatureAnimator(ref treeView, ref _soap);
                        _signatureAnimator.startAnimation(array);
                        _soap.CreateInfoMessage("Signature animation started");
                    }
                }
            }
        }

        public void EndAnimation()
        {
            if (animationRunning)
            {
                if (_signatureAnimator != null)
                {
                    _signatureAnimator.endAnimation();
                }
                if (_encryptionAnimator != null)
                {
                    _encryptionAnimator.endAnimation();
                }
            }
            else
            {
                _soap.CreateInfoMessage("No animation running");
            }
        }

        public void SetAnimationSpeed(int s)
        {
            if (_signatureAnimator != null)
            {
                _signatureAnimator.setAnimationSpeed(s);
            }
            if (_encryptionAnimator != null)
            {
                _encryptionAnimator.setAnimationSpeed(s);
            }
        }

        public StackPanel InsertNamespace(StackPanel panel, string nspace, string Prefix)
        {
            if (!_namespacesTable.ContainsValue(nspace))
            {
                _namespacesTable.Add(nspace, nspace);
                TextBlock xmlns = new TextBlock
                {
                    Name = "xmlns",
                    Text = " xmlns"
                };
                TextBlock prefix = new TextBlock
                {
                    Name = "xmlnsPrefix"
                };
                if (!Prefix.Equals(""))
                { prefix.Text = ":" + Prefix; }
                else { prefix.Text = ""; }
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock
                {
                    Name = "xmlnsValue",
                    Text = "=" + "\"" + nspace + "\"",
                    Foreground = valueBrush
                };
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
        }

        public StackPanel InsertAttributes(StackPanel panel, XmlAttributeCollection attributes)
        {
            foreach (XmlAttribute tempAttribute in attributes)
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

        private void DeleteItem(string text, TreeViewItem itemToDelete)
        {
            if (itemToDelete.HasItems)
            {
                int count = 0;
                foreach (TreeViewItem child in itemToDelete.Items)
                {
                    StackPanel tempHeader1 = (StackPanel)child.Header;
                    TextBlock text1 = (TextBlock)tempHeader1.Children[0];
                    if (text1.Text.Equals(text))
                    {
                        itemToDelete.Items.RemoveAt(count);
                        break;
                    }
                    else
                    {
                        if (child.HasItems)
                        {
                            DeleteItem(text, child);
                        }
                    }
                    count++;
                }
            }
        }

        private void FormatOrigTV(TreeViewItem treeViewItem)
        {
            StackPanel tempHeader = (StackPanel)treeViewItem.Header;
            TextBlock elem = new TextBlock();
            DoubleAnimation widthAnimation =
            new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1))
            {
                AutoReverse = false
            };
            elem = (TextBlock)tempHeader.Children[0];
            if (elem.FontSize == 16.0)
            {
                treeViewItem.BeginAnimation(TreeViewItem.FontSizeProperty, widthAnimation);
            }
            treeViewItem.FontStyle = FontStyles.Normal;
            string s = treeViewItem.Header.ToString();
            if (treeViewItem.HasItems)
            {
                foreach (TreeViewItem child in treeViewItem.Items)
                {
                    FormatOrigTV(child);
                }
            }
        }

        private void SetLbEnd()
        {
            _signSteps.SelectedIndex = 0;
        }

        private string GetNameFromPanel(StackPanel panel, bool prefix)
        {
            foreach (object obj in panel.Children)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock tb = (TextBlock)obj;
                    if (tb.Name.Equals("tbName"))
                    {

                        string name = tb.Text;
                        if (!prefix)
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

        private void ClearBoxes(TreeViewItem item)
        {
            if (item.HasItems)
            {
                bool childIsTextBox = false;
                string text = "";
                foreach (TreeViewItem childItem in item.Items)
                {
                    if (childItem.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                    {
                        StackPanel panel = (StackPanel)childItem.Header;

                        foreach (object obj in panel.Children)
                        {
                            if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBox"))
                            {
                                TextBox box = (TextBox)obj;
                                if (item.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                                {
                                    StackPanel parentPanel = (StackPanel)item.Header;
                                    if (parentPanel.Children.Count > 2)
                                    {
                                        TextBlock block = (TextBlock)parentPanel.Children[1];
                                        _soap.SecuredSoap.GetElementsByTagName(block.Text)[0].InnerText = box.Text;
                                        _soap.SaveSoap();
                                        text = box.Text;
                                        childIsTextBox = true;
                                    }
                                }

                            }
                        }
                    }
                }
                if (childIsTextBox)
                {
                    item.Items.RemoveAt(0);
                    if (!text.Equals(""))
                    {
                        TreeViewItem newItem = new TreeViewItem();
                        StackPanel newPanel = new StackPanel();
                        TextBlock block = new TextBlock
                        {
                            Text = text
                        };
                        newPanel.Children.Add(block);
                        newItem.Header = newPanel;
                        item.Items.Add(newItem);
                    }
                }
            }


            foreach (TreeViewItem childItem in item.Items)
            {
                ClearBoxes(childItem);
            }

        }

        public void AddTextToInformationBox(string information)
        {
            _signSteps.Items.Add(information);
            _signSteps.ScrollIntoView(information);
        }

        #endregion

        #region EventHandlers

        private void IamgeMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            ClearBoxes(_securedSoapItem);
            if (!animationRunning)
            {
                _namespacesTable.Clear();
                Image img = (Image)sender;
                string name = img.Name;
                string[] splitter = name.Split(new char[] { '_' });
                if (splitter.Length > 1)
                {
                    name = splitter[0] + ":" + splitter[1];
                }
                else
                {
                    name = splitter[0];
                }
                XmlElement[] array = new XmlElement[1];
                array[0] = (XmlElement)_soap.SecuredSoap.GetElementsByTagName(name)[0];
                _soap.AddIdToElement(name);

                if (_soap.GotKey)
                {
                    _soap.EncryptElements(array);
                    if (!_soap.GetIsShowEncryptionsSteps())
                    {
                        _soap.ShowSecuredSoap();
                    }
                    else
                    {
                        _encryptionAnimator = new EncryptionAnimator(ref treeView, ref _soap);
                        _encryptionAnimator.startAnimation(array);
                        animationRunning = true;
                    }
                }
                else
                {
                    _soap.CreateErrorMessage("No key for encryption available. Create one in a Web Service Plugin");
                }
            }
        }
        private void TreeViewSelectedItemChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (_soap.WSDLLoaded)
            {
                ClearBoxes(_securedSoapItem);
                TreeView tv = (TreeView)sender;
                if (tv.SelectedItem != null)
                {
                    TreeViewItem item = (TreeViewItem)tv.SelectedItem;
                    StackPanel tempPanel = (StackPanel)item.Header;
                    object temp = tempPanel.Children[0];
                    string type = temp.GetType().ToString();

                    if (type.Equals("System.Windows.Controls.TextBlock"))
                    {
                        XmlNode[] parameter = _soap.GetParameterToEdit();

                        string name = GetNameFromPanel(tempPanel, true);

                        foreach (XmlNode node in parameter)
                        {
                            if (node.Name.Equals(name))
                            {
                                string text = "";
                                if (item.HasItems)
                                {
                                    TreeViewItem childItem = (TreeViewItem)item.Items[0];
                                    StackPanel childPanel = (StackPanel)childItem.Header;
                                    text = GetNameFromPanel(childPanel, false);
                                    item.Items.RemoveAt(0);
                                }
                                item.IsExpanded = true;
                                TreeViewItem newItem = new TreeViewItem();
                                item.Items.Add(newItem);
                                newItem.IsExpanded = true;
                                StackPanel panel = new StackPanel();
                                TextBox box = new TextBox
                                {
                                    Height = 23,
                                    Width = 80,
                                    Text = _soap.SecuredSoap.GetElementsByTagName(name)[0].InnerXml.ToString()
                                };
                                ;
                                box.IsEnabled = true;

                                panel.Children.Add(box);
                                newItem.Header = panel;

                                box.KeyDown += new KeyEventHandler(BoxKeyDownEventHandler);
                                StackPanel parentPanel = (StackPanel)item.Header;
                                TextBlock parentBlock = (TextBlock)parentPanel.Children[0];
                                name = GetNameFromPanel(tempPanel, false);
                                box.Name = name;
                            }
                        }
                    }
                }
            }
        }

        private void ImageMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            DoubleAnimation widhtAnimation = new DoubleAnimation(23, 18, TimeSpan.FromSeconds(0.2))
            {
                AutoReverse = false
            };
            DoubleAnimation heightAnimation = new DoubleAnimation(23, 18, TimeSpan.FromSeconds(0.2))
            {
                AutoReverse = false
            };
            img.BeginAnimation(Image.WidthProperty, widhtAnimation);
            img.BeginAnimation(Image.HeightProperty, heightAnimation);
        }

        private void ImageMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            DoubleAnimation widhtAnimation = new DoubleAnimation(18, 23, TimeSpan.FromSeconds(0.2))
            {
                AutoReverse = false
            };
            DoubleAnimation heightAnimation = new DoubleAnimation(18, 23, TimeSpan.FromSeconds(0.2))
            {
                AutoReverse = false
            };
            img.BeginAnimation(Image.WidthProperty, widhtAnimation);
            img.BeginAnimation(Image.HeightProperty, heightAnimation);
        }

        private void BoxKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                InputEndEventHandler(sender, true);
            }
        }

        private void InputEndEventHandler(object sender, bool enter)
        {
            ClearBoxes(_securedSoapItem);
            _soap.ShowSecuredSoap();
        }

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }

        #endregion


    }
}
