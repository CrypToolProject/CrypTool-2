using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace Soap
{
    internal class EncryptionAnimator
    {
        private readonly TreeView treeView;
        private readonly Soap soap;
        private readonly SoapPresentation presentation;
        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private TreeViewItem item1;
        private readonly SolidColorBrush elemBrush;
        private readonly DoubleAnimation TextSizeAnimation, TextSizeAnimationReverse, opacityAnimation, TextSizeAnimation1, TextSizeAnimationReverse1, opacityAnimation1;
        private int status, encryptedElements;
        private XmlNode[] elementsToEncrypt;
        private XmlNode actElementToEncrypt, actEncryptedDataElement, actcipherdata;
        private TreeViewItem itemToEncrypt, actciphervalueitem, actcipherdataitem;
        private string actId = "";
        private string cipherValueOfElement;
        private TreeViewItem header, actEncDataItem, rootItem, actEncKey, actRefList;
        private int index;
        private bool secHeader;




        public EncryptionAnimator(ref TreeView tv, ref Soap securedSOAP)
        {
            treeView = tv;
            soap = securedSOAP;
            presentation = (SoapPresentation)soap.Presentation;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);


            elemBrush = new SolidColorBrush(Colors.MediumVioletRed);

            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1))
            {
                AutoReverse = false
            };
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1))
            {
                AutoReverse = false
            };
            opacityAnimation = new DoubleAnimation(0.1, 1, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1))
            {
                AutoReverse = false
            };
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1))
            {
                AutoReverse = false
            };
            opacityAnimation1 = new DoubleAnimation(0.1, 1, TimeSpan.FromSeconds(1));

        }

        public void startAnimation(XmlNode[] elementsToEncrypt)
        {
            status = 0;
            encryptedElements = 0;
            dispatcherTimer.Start();
            this.elementsToEncrypt = elementsToEncrypt;
            presentation._namespacesTable.Clear();

        }


        public void endAnimation()
        {
            dispatcherTimer.Stop();
            soap.ShowSecuredSoap();
            presentation.animationRunning = false;
            soap.CreateInfoMessage("Animation end");
        }

        public void playpause()
        {
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
                soap.CreateInfoMessage("Stop encryption animation");

            }
            else
            {
                dispatcherTimer.Start();
                soap.CreateInfoMessage("Restart encryption animation");
            }
        }

        public void setAnimationSpeed(int speedValue)
        {
            switch (speedValue)
            {
                case 1:
                    changeAnimationSpeed(1);
                    break;
                case 2:
                    changeAnimationSpeed(4);
                    break;
                case 3:
                    changeAnimationSpeed(5);
                    break;
                case 4:
                    changeAnimationSpeed(6);
                    break;
                case 5:
                    changeAnimationSpeed(7);
                    break;
            }
        }

        private void changeAnimationSpeed(int seconds)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            switch (status)
            {
                case 0:
                    if (encryptedElements <= elementsToEncrypt.Length)
                    {
                        actElementToEncrypt = elementsToEncrypt[encryptedElements];
                        presentation.AddTextToInformationBox("Actual Element to encrypt is:" + actElementToEncrypt.Name);
                        itemToEncrypt = animateElement(actElementToEncrypt.Name);
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                        status = 1;
                    }
                    break;
                case 1:
                    presentation.AddTextToInformationBox("Generate 256 AES Session Key to encrypt the element");
                    status = 2;
                    break;
                case 2:
                    presentation.AddTextToInformationBox("Session Key is: " + soap.LastSessionKey);

                    status = 3;
                    break;
                case 3:

                    if (!soap.GetIsEncryptContent())
                    {
                        string id = checkForID(itemToEncrypt);
                        if (id != null)
                        {
                            presentation.AddTextToInformationBox("The " + actElementToEncrypt.Name + " has already an id: " + id);
                            actId = id;
                        }
                        else
                        {
                            actId = soap.GetIdToElement(actElementToEncrypt.Name);

                            presentation.AddTextToInformationBox("The " + actElementToEncrypt.Name + " gets the id: " + actId);
                            createAttributeForElement(itemToEncrypt, "URI", "#" + actId);

                        }
                    }
                    else
                    {
                        XmlNode encryptedElement = soap.SecuredSoap.GetElementsByTagName(actElementToEncrypt.Name)[0];
                        foreach (XmlAttribute att in encryptedElement.FirstChild.Attributes)
                        {
                            if (att.Name.Equals("Id"))
                            {
                                actId = att.Value;
                            }
                        }
                        presentation.AddTextToInformationBox("The Id for the EncryptedData Element will be: " + actId);
                    }
                    status = 4;
                    break;
                case 4:
                    presentation.AddTextToInformationBox("Calculate cipher value of the element");
                    status = 5;
                    break;
                case 5:
                    XmlNodeList encryptedDataElements = soap.SecuredSoap.GetElementsByTagName("xenc:EncryptedData");

                    foreach (XmlNode node in encryptedDataElements)
                    {
                        foreach (XmlAttribute att in node.Attributes)
                        {
                            if (att.Name.Equals("Id") && att.Value.Equals(actId))
                            {
                                actEncryptedDataElement = node;
                                foreach (XmlNode child in node.ChildNodes)
                                {
                                    if (child.Name.Equals("xenc:CipherData"))
                                    {
                                        cipherValueOfElement = child.FirstChild.InnerText;
                                    }
                                }
                            }
                        }
                    }
                    presentation.AddTextToInformationBox("CipherValue is:" + cipherValueOfElement);
                    status = 6;
                    break;
                case 6:
                    presentation.AddTextToInformationBox("Create EncryptedData Element");
                    rootItem = (TreeViewItem)presentation.treeView.Items[0];
                    presentation.treeView.Items.Clear();
                    header = new TreeViewItem();
                    StackPanel panel = new StackPanel();
                    TextBlock block = new TextBlock
                    {
                        FontSize = 16,
                        Text = "Encrypted Data Element"
                    };
                    panel.Children.Add(block);
                    header.Header = panel;
                    presentation.treeView.Items.Add(header);
                    actEncDataItem = addChildElement(header, actEncryptedDataElement, true);
                    status = 7;
                    break;

                case 7:
                    presentation.AddTextToInformationBox("Create the EncryptionMethod Element ");
                    addChildElement(actEncDataItem, actEncryptedDataElement.FirstChild, false);
                    status = 8;
                    break;

                case 8:
                    presentation.AddTextToInformationBox("Create the CipherData Element ");
                    foreach (XmlNode node in actEncryptedDataElement.ChildNodes)
                    {
                        if (node.Name.Equals("xenc:CipherData"))
                        {
                            actcipherdata = node;
                        }
                    }
                    actcipherdataitem = addChildElement(actEncDataItem, actcipherdata, false);
                    status = 9;
                    break;

                case 9:
                    presentation.AddTextToInformationBox("Create the CipherValue Element");
                    actciphervalueitem = addChildElement(actcipherdataitem, actcipherdata.FirstChild, false);
                    status = 10;
                    break;

                case 10:
                    presentation.AddTextToInformationBox("Insert the cipher value");
                    createValue(actciphervalueitem, actcipherdata.FirstChild.InnerText);
                    status = 11;
                    break;
                case 11:
                    presentation.treeView.Items.Clear();
                    presentation.treeView.Items.Add(rootItem);
                    status = 12;
                    break;

                case 12:
                    if (soap.GetIsEncryptContent())
                    {
                        presentation.AddTextToInformationBox("Replase the content of the " + actElementToEncrypt.Name);
                        presentation.AddTextToInformationBox("Element with the created EncryptedData Element");
                        itemToEncrypt.Items.Clear();
                        TreeViewItem parent1 = (TreeViewItem)actEncDataItem.Parent;
                        parent1.Items.Remove(actEncDataItem);
                        itemToEncrypt.Items.Add(actEncDataItem);


                    }
                    else
                    {
                        TreeViewItem parent = (TreeViewItem)itemToEncrypt.Parent;
                        index = parent.Items.IndexOf(itemToEncrypt);
                        TreeViewItem parent1 = (TreeViewItem)actEncDataItem.Parent;
                        parent1.Items.Remove(actEncDataItem);

                        parent.Items.RemoveAt(index);
                        parent.Items.RemoveAt(index);
                        presentation.AddTextToInformationBox("Replase the the " + actElementToEncrypt.Name + " Element");
                        parent.Items.Insert(index, actEncDataItem);
                        TextBlock tb = new TextBlock();
                        TextBlock tb1 = new TextBlock();
                        TextBlock tb2 = new TextBlock();

                        tb.Name = "tbNameClose";
                        tb.Text = "/xenc:EncryptedData";
                        tb1.Text = "<";
                        tb2.Text = ">";


                        StackPanel panel1 = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal
                        };
                        panel1.Children.Add(tb1);
                        panel1.Children.Add(tb);
                        panel1.Children.Add(tb2);
                        tb.Foreground = elemBrush;
                        tb1.Foreground = elemBrush;
                        tb2.Foreground = elemBrush;

                        TreeViewItem closeItem = new TreeViewItem
                        {
                            Header = panel1
                        };

                        parent.Items.Insert(index + 1, closeItem);

                    }
                    status = 13;
                    break;

                case 13:
                    presentation.AddTextToInformationBox("Check for Header");

                    if (!soap.HadHeader)
                    {
                        secHeader = false;
                        status = 14;
                    }
                    else { secHeader = true; status = 15; }

                    break;
                case 14:
                    presentation.AddTextToInformationBox("No Header found. Create SOAP and Security Header");
                    addChildElement("s:Envelope", soap.SecuredSoap.GetElementsByTagName("s:Header")[0], true);
                    addChildElement("s:Header", soap.SecuredSoap.GetElementsByTagName("wsse:Security")[0], true);
                    status = 15;
                    break;
                case 15:
                    presentation.AddTextToInformationBox("Create Encrypted Key");
                    actEncKey = addChildElement("wsse:Security", soap.SecuredSoap.GetElementsByTagName("xenc:EncryptedKey")[0], true);
                    status = 16;
                    break;

                case 16:
                    presentation.AddTextToInformationBox("Create Encryption Method Element");
                    addChildElement(actEncKey, soap.SecuredSoap.GetElementsByTagName("xenc:EncryptionMethod")[0], true);
                    status = 17;
                    break;
                case 17:
                    presentation.AddTextToInformationBox("Add Key Info Element");
                    TreeViewItem keyInfo = addChildElement(actEncKey, soap.SecuredSoap.GetElementsByTagName("ds:KeyInfo")[0], true);
                    keyInfo.IsExpanded = true;
                    presentation.CopyXmlToTreeView(soap.SecuredSoap.GetElementsByTagName("ds:KeyInfo")[0].FirstChild, keyInfo);
                    status = 18;
                    break;

                case 18:
                    presentation.AddTextToInformationBox("Create CipherData Element");
                    actcipherdataitem = addChildElement(actEncKey, soap.SecuredSoap.GetElementsByTagName("xenc:CipherData")[0], false);
                    status = 19;
                    break;
                case 19:
                    presentation.AddTextToInformationBox("Create CipherValue Element");
                    actciphervalueitem = addChildElement(actcipherdataitem, soap.SecuredSoap.GetElementsByTagName("xenc:CipherValue")[0], false);
                    status = 20;
                    break;
                case 20:
                    presentation.AddTextToInformationBox("Create CipherValue Element by encrypting the session key with the public key of the web service");
                    status = 21;
                    break;
                case 21:
                    presentation.AddTextToInformationBox("Cipher Value is: " + soap.SecuredSoap.GetElementsByTagName("xenc:CipherValue")[0].InnerText);
                    status = 22;
                    break;
                case 22:
                    presentation.AddTextToInformationBox("Insert Cipher Value");
                    createValue(actciphervalueitem, soap.SecuredSoap.GetElementsByTagName("xenc:CipherValue")[0].InnerText);
                    status = 23;
                    break;
                case 23:
                    presentation.AddTextToInformationBox("Create Reference List");
                    actRefList = addChildElement(actEncKey, soap.SecuredSoap.GetElementsByTagName("xenc:ReferenceList")[0], false);
                    status = 24;
                    break;
                case 24:
                    status = 25;
                    presentation.AddTextToInformationBox("Insert Data Reference");
                    addChildElement(actRefList, soap.SecuredSoap.GetElementsByTagName("xenc:ReferenceList")[0], false);
                    status = 25;
                    break;
                case 25:
                    dispatcherTimer.Stop();
                    presentation._namespacesTable.Clear();
                    presentation.animationRunning = false;
                    status = 0;
                    soap.ShowSecuredSoap();
                    break;

            }
        }

        #region Helper



        /// <summary>
        /// Returns the Name of the Element without the prefix
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        private string getNameFromPanel(StackPanel panel, bool prefix)
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

        private TreeViewItem findItem(TreeViewItem item, string bezeichner)
        {
            if (item != null)
            {
                StackPanel tempHeader1 = (StackPanel)item.Header;
                string Bezeichner = getNameFromPanel(tempHeader1, true);
                if (Bezeichner != null)
                {
                    if (Bezeichner.Equals(bezeichner))
                    {
                        item1 = item;
                        return item;
                    }
                }
                foreach (TreeViewItem childItem in item.Items)
                {
                    findItem(childItem, bezeichner);
                }
                if (item1 != null)
                {
                    return item1;
                }
            }
            return null;
        }

        private StackPanel insertNamespace(ref StackPanel panel, string nspace, string Prefix)
        {
            if (!presentation._namespacesTable.ContainsValue(nspace))
            {
                presentation._namespacesTable.Add(nspace, nspace);
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

        private StackPanel insertAttributes(ref StackPanel panel, XmlAttributeCollection attributes)
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
                    if (!presentation._namespacesTable.ContainsValue(tempAttribute.Value))
                    {
                        presentation._namespacesTable.Add(tempAttribute.Value, tempAttribute.Value);
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

        private string checkForID(TreeViewItem item)
        {
            StackPanel panel = (StackPanel)item.Header;
            int count = 0;
            foreach (object obj in panel.Children)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock tb = (TextBlock)obj;
                    if (tb.Name.Equals("attributeName"))
                    {
                        if (tb.Text.Equals("Id"))
                        {
                            TextBlock block = (TextBlock)panel.Children[count + 1];
                            return block.Text;
                        }
                    }
                }
                count++;
            }
            return null;
        }

        #endregion



        #region AnimationMethods

        private void createAttributeForElement(string Element, string attName, string attValue)
        {
            TreeViewItem item = findItem(presentation.SecuredSoapItem, Element);
            StackPanel panel = (StackPanel)item.Header;

            TextBlock attributeName = new TextBlock();
            TextBlock attributeValue = new TextBlock();
            attributeName.Text = attName;
            attributeName.Name = "attributeName";
            attributeValue.Text = "=" + attValue;
            attributeValue.Name = "attributeValue";

            attributeName.Opacity = 0.1;
            attributeValue.Opacity = 0.1;



            panel.Children.Insert(3, attributeName);
            panel.Children.Insert(4, attributeValue);

            attributeName.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation);
            attributeValue.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation1);

            TextSizeAnimation.AutoReverse = true;
            TextSizeAnimation1.AutoReverse = true;

            attributeName.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            attributeValue.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation1);

            TextSizeAnimation.AutoReverse = false;
            TextSizeAnimation1.AutoReverse = false;

        }

        private void createAttributeForElement(TreeViewItem item, string attName, string attValue)
        {
            StackPanel panel = (StackPanel)item.Header;

            TextBlock attributeName = new TextBlock();
            TextBlock attributeValue = new TextBlock();
            attributeName.Text = attName;
            attributeName.Name = "attributeName";
            attributeValue.Text = "=" + attValue;
            attributeValue.Name = "attributeValue";

            attributeName.Opacity = 0.1;
            attributeValue.Opacity = 0.1;



            panel.Children.Insert(3, attributeName);
            panel.Children.Insert(4, attributeValue);

            attributeName.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation);
            attributeValue.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation1);

            TextSizeAnimation.AutoReverse = true;
            TextSizeAnimation1.AutoReverse = true;

            attributeName.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            attributeValue.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation1);

            TextSizeAnimation.AutoReverse = false;
            TextSizeAnimation1.AutoReverse = false;

        }

        private void animateItemInclChilds(string parentElem)
        {
            TreeViewItem parentElement = findItem(presentation.SecuredSoapItem, parentElem);
            TextSizeAnimation.AutoReverse = true;
            parentElement.BeginAnimation(TreeViewItem.FontSizeProperty, TextSizeAnimation);
            TextSizeAnimation.AutoReverse = false;
        }

        private TreeViewItem addChildElement(string parentElem, XmlNode newElem, bool first)
        {
            TreeViewItem parentElement;

            parentElement = findItem(presentation.SecuredSoapItem, parentElem);



            TreeViewItem newElement = new TreeViewItem();


            TreeViewItem newCloseElement = new TreeViewItem();

            StackPanel newPanel = new StackPanel
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
            tbName.Text = newElem.Name;

            tbTagOpen.Foreground = elemBrush;
            tbTagClose.Foreground = elemBrush;
            tbName.Foreground = elemBrush;

            newPanel.Children.Add(tbTagOpen);
            newPanel.Children.Add(tbName);

            if (!newElem.NamespaceURI.Equals(""))
            {
                insertNamespace(ref newPanel, newElem.NamespaceURI, newElem.Prefix);
            }
            if (newElem.Attributes != null)
            {
                insertAttributes(ref newPanel, newElem.Attributes);
            }

            newPanel.Children.Add(tbTagClose);
            StackPanel newClosePanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            TextBlock elem1Open = new TextBlock
            {
                Text = "<"
            };
            newClosePanel.Children.Insert(0, elem1Open);
            TextBlock elem1Close = new TextBlock
            {
                Text = ">"
            };
            TextBlock elem1Name = new TextBlock
            {
                Text = "/" + newElem.Name
            };

            newClosePanel.Children.Add(elem1Name);
            newClosePanel.Children.Add(elem1Close);

            elem1Open.Foreground = elemBrush;
            elem1Name.Foreground = elemBrush;
            elem1Close.Foreground = elemBrush;


            newElement.Header = newPanel;
            newCloseElement.Header = newClosePanel;
            if (!first)
            {
                parentElement.Items.Add(newElement);
                parentElement.Items.Add(newCloseElement);
            }
            else
            {
                parentElement.Items.Insert(0, newElement);
                parentElement.Items.Insert(1, newCloseElement);
            }
            parentElement.IsExpanded = true;
            newPanel.Opacity = 0.1;
            newClosePanel.Opacity = 0.1;
            animationAddElements(newPanel, newClosePanel);
            return newElement;
        }



        private TreeViewItem addChildElement(TreeViewItem parentElement, XmlNode newElem, int index)
        {
            TreeViewItem newElement = new TreeViewItem();


            TreeViewItem newCloseElement = new TreeViewItem();

            StackPanel newPanel = new StackPanel
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
            tbName.Text = newElem.Name;

            tbTagOpen.Foreground = elemBrush;
            tbTagClose.Foreground = elemBrush;
            tbName.Foreground = elemBrush;

            newPanel.Children.Add(tbTagOpen);
            newPanel.Children.Add(tbName);

            if (!newElem.NamespaceURI.Equals(""))
            {
                insertNamespace(ref newPanel, newElem.NamespaceURI, newElem.Prefix);
            }
            if (newElem.Attributes != null)
            {
                insertAttributes(ref newPanel, newElem.Attributes);
            }

            newPanel.Children.Add(tbTagClose);

            StackPanel newClosePanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            TextBlock elem1Open = new TextBlock
            {
                Text = "<"
            };
            newClosePanel.Children.Insert(0, elem1Open);
            TextBlock elem1Close = new TextBlock
            {
                Text = ">"
            };
            TextBlock elem1Name = new TextBlock
            {
                Text = "/" + newElem.Name
            };

            newClosePanel.Children.Add(elem1Name);
            newClosePanel.Children.Add(elem1Close);

            elem1Open.Foreground = elemBrush;
            elem1Name.Foreground = elemBrush;
            elem1Close.Foreground = elemBrush;


            newElement.Header = newPanel;
            newCloseElement.Header = newClosePanel;

            parentElement.Items.Insert(index, newElement);
            parentElement.Items.Insert(index + 1, newCloseElement);

            parentElement.IsExpanded = true;
            newPanel.Opacity = 0.1;
            newClosePanel.Opacity = 0.1;
            animationAddElements(newPanel, newClosePanel);
            return newElement;
        }




        private TreeViewItem addChildElement(TreeViewItem parentElement, XmlNode newElem, bool first)
        {
            TreeViewItem newElement = new TreeViewItem();


            TreeViewItem newCloseElement = new TreeViewItem();

            StackPanel newPanel = new StackPanel
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
            tbName.Text = newElem.Name;

            tbTagOpen.Foreground = elemBrush;
            tbTagClose.Foreground = elemBrush;
            tbName.Foreground = elemBrush;

            newPanel.Children.Add(tbTagOpen);
            newPanel.Children.Add(tbName);

            if (!newElem.NamespaceURI.Equals(""))
            {
                insertNamespace(ref newPanel, newElem.NamespaceURI, newElem.Prefix);
            }
            if (newElem.Attributes != null)
            {
                insertAttributes(ref newPanel, newElem.Attributes);
            }

            newPanel.Children.Add(tbTagClose);

            StackPanel newClosePanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            TextBlock elem1Open = new TextBlock
            {
                Text = "<"
            };
            newClosePanel.Children.Insert(0, elem1Open);
            TextBlock elem1Close = new TextBlock
            {
                Text = ">"
            };
            TextBlock elem1Name = new TextBlock
            {
                Text = "/" + newElem.Name
            };

            newClosePanel.Children.Add(elem1Name);
            newClosePanel.Children.Add(elem1Close);

            elem1Open.Foreground = elemBrush;
            elem1Name.Foreground = elemBrush;
            elem1Close.Foreground = elemBrush;


            newElement.Header = newPanel;
            newCloseElement.Header = newClosePanel;
            if (!first)
            {
                parentElement.Items.Add(newElement);
                parentElement.Items.Add(newCloseElement);
            }
            else
            {
                parentElement.Items.Insert(0, newElement);
                parentElement.Items.Insert(1, newCloseElement);
            }
            parentElement.IsExpanded = true;
            newPanel.Opacity = 0.1;
            newClosePanel.Opacity = 0.1;
            animationAddElements(newPanel, newClosePanel);
            return newElement;
        }


        private void animationAddElements(StackPanel t1, StackPanel t2)
        {
            Storyboard sb = new Storyboard();

            TextBlock tb1 = (TextBlock)t1.Children[1];
            TextBlock tb2 = (TextBlock)t2.Children[2];

            sb.Children.Add(opacityAnimation);
            sb.Children.Add(opacityAnimation1);

            sb.Children.Add(TextSizeAnimation);
            sb.Children.Add(TextSizeAnimation1);

            sb.Children.Add(TextSizeAnimationReverse);
            sb.Children.Add(TextSizeAnimationReverse1);

            sb.Children[2].BeginTime = new TimeSpan(0, 0, 2);
            sb.Children[3].BeginTime = new TimeSpan(0, 0, 2);
            sb.Children[4].BeginTime = new TimeSpan(0, 0, 4);
            sb.Children[5].BeginTime = new TimeSpan(0, 0, 4);

            Storyboard.SetTarget(opacityAnimation, t1);
            Storyboard.SetTarget(TextSizeAnimation, tb1);

            Storyboard.SetTarget(opacityAnimation1, t2);
            Storyboard.SetTarget(TextSizeAnimation1, tb2);

            Storyboard.SetTarget(TextSizeAnimationReverse, tb1);
            Storyboard.SetTarget(TextSizeAnimationReverse1, tb2);

            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(TextBlock.OpacityProperty));
            Storyboard.SetTargetProperty(TextSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));

            Storyboard.SetTargetProperty(opacityAnimation1, new PropertyPath(TextBlock.OpacityProperty));
            Storyboard.SetTargetProperty(TextSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));

            Storyboard.SetTargetProperty(TextSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            sb.Begin();
        }

        private void createValue(TreeViewItem parentItem, string value)
        {
            parentItem.IsExpanded = true;

            TreeViewItem valueItem = new TreeViewItem();
            StackPanel panel = new StackPanel();

            TextBlock block1 = new TextBlock();
            TextBlock block2 = new TextBlock();
            TextBlock block3 = new TextBlock();

            block1.Name = "value1";
            block2.Name = "value2";
            block3.Name = "value3";

            block1.Opacity = 0.1;
            block1.Text = value;

            panel.Children.Insert(0, block1);

            valueItem.Header = panel;
            parentItem.Items.Add(valueItem);

            block1.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation);
            TextSizeAnimation.AutoReverse = true;

            block1.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            TextSizeAnimation.AutoReverse = false;
        }



        private TreeViewItem animateElement(string itemName)
        {
            TreeViewItem item = findItem(presentation.SecuredSoapItem, itemName);
            TreeViewItem closeItem = findItem(presentation.SecuredSoapItem, "/" + itemName);

            StackPanel panel = (StackPanel)item.Header;
            StackPanel panel2 = (StackPanel)closeItem.Header;
            TextBlock textblock1 = new TextBlock();
            TextBlock textblock2 = new TextBlock();
            TextBlock textblock3 = new TextBlock();
            TextBlock textblock4 = new TextBlock();


            TextBlock closetextblock1 = new TextBlock();
            TextBlock closetextblock2 = new TextBlock();
            TextBlock closetextblock3 = new TextBlock();
            TextBlock closetextblock4 = new TextBlock();


            TextSizeAnimation.AutoReverse = true;

            if (panel.Children.Count > 0)
            {
                textblock1 = (TextBlock)panel.Children[0];
                textblock1.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel.Children.Count > 1)
            {
                textblock2 = (TextBlock)panel.Children[1];
                textblock2.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel.Children.Count > 2)
            {
                textblock3 = (TextBlock)panel.Children[2];
                textblock3.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            //if (panel.Children.Count > 3)
            //{
            //    textblock4 = (TextBlock)panel.Children[3];
            //    textblock4.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            //}


            if (panel2.Children.Count > 0)
            {
                closetextblock1 = (TextBlock)panel2.Children[0];
                closetextblock1.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel2.Children.Count > 1)
            {
                closetextblock2 = (TextBlock)panel2.Children[1];
                closetextblock2.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel2.Children.Count > 2)
            {
                closetextblock3 = (TextBlock)panel2.Children[2];
                closetextblock3.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            TextSizeAnimation.AutoReverse = false;
            return item;
        }

        private void animateElement(TreeViewItem item)
        {


            StackPanel panel = (StackPanel)item.Header;

            TextBlock textblock1 = new TextBlock();
            TextBlock textblock2 = new TextBlock();
            TextBlock textblock3 = new TextBlock();
            TextBlock textblock4 = new TextBlock();




            TextSizeAnimation.AutoReverse = true;

            if (panel.Children.Count > 0)
            {
                textblock1 = (TextBlock)panel.Children[0];
                textblock1.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel.Children.Count > 1)
            {
                textblock2 = (TextBlock)panel.Children[1];
                textblock2.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            if (panel.Children.Count > 2)
            {
                textblock3 = (TextBlock)panel.Children[2];
                textblock3.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimation);
            }
            TextSizeAnimation.AutoReverse = false;

        }


        #endregion


    }
}
