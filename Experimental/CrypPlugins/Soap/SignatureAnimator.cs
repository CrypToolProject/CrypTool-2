using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace Soap
{
    internal class SignatureAnimator
    {
        private readonly TreeView tv;
        private readonly Soap soap;
        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private readonly System.Windows.Threading.DispatcherTimer referencesTimer;
        private readonly System.Windows.Threading.DispatcherTimer referenceElementsTimer;
        private System.Windows.Threading.DispatcherTimer actTimer;
        private readonly System.Windows.Threading.DispatcherTimer TransformsTimer;
        private int status, referencesCounter, referencesSteps, transformsCounter, transformSteps, xPathArrayCounter;
        private readonly SoapPresentation presentation;
        private TreeViewItem item1, rootItem, actSignatureItem;
        private readonly SolidColorBrush elemBrush;
        private readonly DoubleAnimation TextSizeAnimation, TextSizeAnimationReverse, opacityAnimation, TextSizeAnimation1, TextSizeAnimationReverse1, opacityAnimation1;
        private XmlElement[] elementsToSign;
        private XmlElement actElementToReference;
        private readonly XmlElement actTransformsElement;
        private XmlElement actReferenceElement;
        private XmlElement xPathHelper;
        private TreeViewItem actElementToReferenceTVI, actTransformsElementTVI, actReferenceElementTVI, actDigestValue, actSignatureValue;
        private bool useactSigItem;
        private string xPath;
        private readonly string outPut;
        private string[] xPathSteps;
        private bool animationRunning;
        private readonly bool header;
        private bool signed;

        public SignatureAnimator(ref TreeView tv, ref Soap securedSOAP)
        {
            useactSigItem = false;
            this.tv = tv;
            soap = securedSOAP;
            presentation = (SoapPresentation)securedSOAP.Presentation;
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            referencesTimer = new System.Windows.Threading.DispatcherTimer();
            TransformsTimer = new System.Windows.Threading.DispatcherTimer();

            referenceElementsTimer = new System.Windows.Threading.DispatcherTimer();

            referenceElementsTimer.Tick += new EventHandler(referenceElementsTimer_Tick);
            referencesTimer.Tick += new EventHandler(referenceTimer_Tick);
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            TransformsTimer.Tick += new EventHandler(TransformsTimer_Tick);

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
            actTimer = dispatcherTimer;
            status = 0;
            animationRunning = false;

            signed = false;
        }



        public void startAnimation(XmlElement[] elementsToSign)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            referenceElementsTimer.Interval = new TimeSpan(0, 0, 0, 3, 0);
            TransformsTimer.Interval = new TimeSpan(0, 0, 0, 3, 0);
            referencesTimer.Interval = new TimeSpan(0, 0, 0, 3, 0);
            rootItem = (TreeViewItem)tv.Items[0];
            dispatcherTimer.Start();
            actTimer = dispatcherTimer;
            status = 0;
            this.elementsToSign = elementsToSign;
            animationRunning = true;
            signed = false;
        }

        public void startstopAnimation()
        {
            if (animationRunning)
            {

                actTimer.Stop();
                soap.CreateInfoMessage("Stop signature animation");
                animationRunning = false;

            }
            else
            {
                actTimer.Start();
                animationRunning = true;
                soap.CreateInfoMessage("Restart signature animation");

            }
        }

        public void endAnimation()
        {
            dispatcherTimer.Stop();
            referenceElementsTimer.Stop();
            TransformsTimer.Stop();
            referencesTimer.Stop();
            referencesCounter = 0;
            status = 0;
            referencesSteps = 0;
            xPathArrayCounter = 0;
            transformSteps = 0;
            transformsCounter = 0;
            if (!soap.CheckSecurityHeader())
            {
                soap.CreateSecurityHeaderAndSoapHeader();
            }
            if (!signed)
            {
                soap.SignElementsManual(elementsToSign);
            }

            presentation.animationRunning = false;
            animationRunning = false;
            soap.CreateInfoMessage("Animation end");
            soap.ShowSecuredSoap();
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
            referenceElementsTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
            TransformsTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
            referencesTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
        }





        #region Methods

        #endregion

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
                        if (tb.Text.Trim().Equals("Id"))
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
            if (!useactSigItem)
            {
                parentElement = findItem(presentation.SecuredSoapItem, parentElem);
            }
            else
            {
                parentElement = findItem(actSignatureItem, parentElem);
            }

            TreeViewItem newElement = new TreeViewItem();

            if (newElem.Name.Equals("ds:Signature"))
            {
                actSignatureItem = newElement;
            }
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

        private TreeViewItem addChildElement(TreeViewItem parentElement, XmlNode newElem, bool first)
        {
            TreeViewItem newElement = new TreeViewItem();

            if (newElem.Name.Equals("ds:Signature"))
            {
                actSignatureItem = newElement;
            }
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



        private void animateElement(string itemName)
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



        #region Timer
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            switch (status)
            {
                case 0:
                    //Start and HeaderCheck
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 6, 0);
                    presentation.AddTextToInformationBox("Check Envelope for SOAP-Header");
                    bool secHead = soap.CheckSecurityHeader();
                    if (!secHead)
                    {
                        presentation.AddTextToInformationBox("No SOAP-Header found");
                        status = 1;
                    }
                    else
                    {
                        presentation.AddTextToInformationBox("SOAP-Header already exists");
                        animateElement("s:Header");
                        status = 2;
                    }
                    break;

                case 1:
                    //Create Soap Header
                    presentation.AddTextToInformationBox("Add Soap-Header to Envelope");
                    soap.CreateSecurityHeaderAndSoapHeader();
                    addChildElement("s:Envelope", soap.SecuredSoap.GetElementsByTagName("s:Header")[0], true);
                    status = 3;
                    //  slider1.Value++;
                    break;

                case 2:
                    //Check Security Header -> Found -> Animate
                    presentation.AddTextToInformationBox("Check for Security-Header");
                    presentation.AddTextToInformationBox("Security-Header found");
                    animateElement("wsse:Security");
                    status = 4;
                    break;

                case 3:
                    //Create Security Header
                    presentation.AddTextToInformationBox("Create Security-Header as child of SOAP-Header");
                    addChildElement("s:Header", soap.SecuredSoap.GetElementsByTagName("wsse:Security")[0], true);
                    status = 4;
                    break;

                case 4:
                    //Create Signature Element
                    presentation.AddTextToInformationBox("Create a ds:Signature-Element as first child of the Security-Header");
                    soap.SignElementsManual(elementsToSign);
                    signed = true;
                    addChildElement("wsse:Security", soap.SecuredSoap.GetElementsByTagName("ds:Signature")[0], true);
                    //Signature TreeViewItem is saved in actSignatureItem var
                    status = 5;
                    break;

                case 5:
                    //Create SignedInfo Element
                    presentation.AddTextToInformationBox("Create a ds:SignedInfo Element");
                    useactSigItem = true;
                    addChildElement("ds:Signature", soap.SecuredSoap.GetElementsByTagName("ds:SignedInfo")[0], false);
                    status = 6;
                    break;

                case 6:
                    //Textoutput for Canonic. Method
                    presentation.AddTextToInformationBox("Create Canonicalization Method Element");
                    presentation.AddTextToInformationBox("This Element specifies the canonicalization method for the Signed-Info Element");
                    status = 7;

                    break;

                case 7:
                    //Create Canon
                    useactSigItem = true;
                    addChildElement("ds:SignedInfo", soap.SecuredSoap.GetElementsByTagName("ds:CanonicalizationMethod")[0], false);
                    status = 8;
                    break;

                case 8:
                    //Textoutput Signature Method
                    presentation.AddTextToInformationBox("Create Signature Method Element with the choosen signature algorithm");
                    string s = "";
                    if (soap.GetSignatureAlgorithm().Equals("1"))
                    {
                        s = "RSA-SHA1";
                    }
                    else
                    {
                        s = "DSA-SHA1";
                    }
                    presentation.AddTextToInformationBox("The signature algorithm is: " + s);
                    status = 9;
                    break;
                case 9:
                    //Create Signature Method 
                    useactSigItem = true;
                    addChildElement("ds:SignedInfo", soap.SecuredSoap.GetElementsByTagName("ds:SignatureMethod")[0], false);
                    status = 10;
                    break;
                case 10:
                    //Start Reference Elements 
                    presentation.AddTextToInformationBox("Create Reference-Elements for the elements to sign");
                    referencesCounter = 0;
                    referencesTimer.Start();
                    actTimer = referencesTimer;
                    dispatcherTimer.Stop();

                    status = 11;
                    break;
                case 11:
                    presentation.AddTextToInformationBox("Create Signature Value Element");
                    actSignatureValue = addChildElement(actSignatureItem, soap.SecuredSoap.GetElementsByTagName("ds:SignatureValue")[0], false);
                    status = 12;
                    break;
                case 12:
                    presentation.AddTextToInformationBox("Canonicalize the SignedInfo Element");
                    rootItem = (TreeViewItem)presentation.treeView.Items[0];
                    TreeViewItem canon = new TreeViewItem();
                    StreamReader sreader = new StreamReader(soap.CanonicalizeNodeWithExcC14n((XmlElement)soap.SecuredSoap.GetElementsByTagName("ds:SignedInfo")[0]));
                    string canonString = sreader.ReadToEnd();
                    XmlDocument doc = new XmlDocument();
                    XmlElement elem = doc.CreateElement("temp");
                    elem.InnerXml = canonString;
                    TextBlock tb = new TextBlock
                    {
                        FontSize = 14,
                        Text = "Canonicalized Element: "
                    };
                    StackPanel panel = new StackPanel();
                    panel.Children.Add(tb);
                    canon.Header = panel;
                    XmlNode node = elem.ChildNodes[0];
                    presentation.CopyXmlToTreeView(node, canon);
                    presentation.treeView.Items.Clear();
                    canon.IsExpanded = true;
                    presentation.treeView.Items.Add(canon);
                    status = 13;
                    break;
                case 13:
                    presentation.AddTextToInformationBox("Calculate SHA-1 Hash Value of this Element");
                    status = 14;
                    break;
                case 14:
                    XmlElement signedInfo = (XmlElement)soap.SecuredSoap.GetElementsByTagName("ds:SignedInfo")[0];
                    byte[] temp = soap.GetDigestValueForElementWithSha1(signedInfo);
                    string value = Convert.ToBase64String(temp);
                    presentation.AddTextToInformationBox("SHA-1 Hash Value of the ds:SignedInfo Element is: " + value);
                    status = 15;
                    break;
                case 15:
                    presentation.AddTextToInformationBox("Encrypt the calculated SHA-1 hash value to get die signature value");
                    status = 16;
                    break;
                case 16:
                    XmlNode signatureValue = soap.SecuredSoap.GetElementsByTagName("ds:SignatureValue")[0];
                    presentation.AddTextToInformationBox("The Signature Value is: " + signatureValue.InnerText);
                    presentation.treeView.Items.Clear();
                    presentation.treeView.Items.Add(rootItem);
                    createValue(actSignatureValue, signatureValue.InnerText);
                    status = 17;
                    break;
                case 17:
                    animateElement(actSignatureValue);
                    presentation.AddTextToInformationBox("Add KeyInfo Element");

                    status = 18;
                    break;
                case 18:
                    TreeViewItem keyInfo = new TreeViewItem();
                    XmlNode keyInfoXML = soap.SecuredSoap.GetElementsByTagName("ds:KeyInfo")[0];
                    StackPanel panel1 = new StackPanel
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
                    tbName.Text = "ds:KeyInfo";

                    panel1.Children.Add(tbTagOpen);
                    panel1.Children.Add(tbName);
                    panel1.Children.Add(tbTagClose);

                    tbName.Foreground = elemBrush;
                    tbTagClose.Foreground = elemBrush;
                    tbTagOpen.Foreground = elemBrush;

                    presentation.InsertNamespace(panel1, keyInfoXML.NamespaceURI, "ds");
                    keyInfo.Header = panel1;
                    presentation.CopyXmlToTreeView(keyInfoXML, keyInfo);
                    actSignatureItem.Items.Add(keyInfo);
                    animateElement(keyInfo);
                    status = 19;
                    break;
                case 19:
                    status = 0;
                    dispatcherTimer.Stop();
                    presentation.animationRunning = false;
                    soap.ShowSecuredSoap();
                    animationRunning = false;
                    break;

            }

        }

        private void referenceElementsTimer_Tick(object sender, EventArgs e)
        {
            switch (referencesSteps)
            {
                case 0:
                    TreeViewItem element = findItem(presentation.SecuredSoapItem, actElementToReference.Name);
                    element.BringIntoView();
                    actElementToReferenceTVI = element;
                    if (!soap.GetXPathTransForm())
                    {
                        actElementToReference = elementsToSign[referencesCounter];
                        presentation.AddTextToInformationBox("Check if the " + actElementToReference.Name + "-Element has an ID");

                        string id = checkForID(element);
                        if (id != null)
                        {
                            presentation.AddTextToInformationBox("The Element already has an ID: " + id);
                            animateElement(actElementToReference.Name);
                            referencesSteps = 2;
                        }
                        else
                        {
                            presentation.AddTextToInformationBox("The Element has no ID");
                            referencesSteps = 1;
                        }
                    }
                    else
                    {
                        presentation.AddTextToInformationBox("The " + actElementToReference.Name + " Element is referenced by a xPath transform");
                        referencesSteps = 3;
                    }
                    break;

                case 1:
                    //Generate ID for Act Element
                    presentation.AddTextToInformationBox("Create Id for the Element");
                    TreeViewItem element1 = findItem(presentation.SecuredSoapItem, actElementToReference.Name);
                    createAttributeForElement(actElementToReference.Name, "Id", soap.GetIdToElement(actElementToReference.Name));
                    referencesSteps = 2;
                    break;


                case 2:
                    presentation.AddTextToInformationBox("Create Reference Element for " + actElementToReference.Name + " Element");
                    actReferenceElementTVI = addChildElement("ds:SignedInfo", soap.SecuredSoap.GetElementsByTagName("ds:Reference")[referencesCounter], false);
                    actReferenceElement = (XmlElement)soap.SecuredSoap.GetElementsByTagName("ds:Reference")[referencesCounter];

                    referencesSteps = 5;

                    break;

                case 3:
                    //XPAth Reference Element
                    presentation.AddTextToInformationBox("Create Reference Element for " + actElementToReference.Name + " Element");
                    actReferenceElementTVI = addChildElement("ds:SignedInfo", soap.SecuredSoap.GetElementsByTagName("ds:Reference")[referencesCounter], false);
                    actReferenceElement = (XmlElement)soap.SecuredSoap.GetElementsByTagName("ds:Reference")[referencesCounter];
                    referencesSteps = 4;
                    break;
                case 4:
                    presentation.AddTextToInformationBox("The Reference Element needs no URI cause of the XPath Transform");
                    referencesSteps = 5;
                    break;
                case 5:
                    presentation.AddTextToInformationBox("Create a Transforms Element");
                    useactSigItem = true;
                    actTransformsElementTVI = addChildElement("ds:Reference", soap.SecuredSoap.GetElementsByTagName("ds:Transforms")[referencesCounter], false);
                    referencesSteps = 6;
                    break;
                case 6:
                    transformsCounter = actReferenceElement.FirstChild.ChildNodes.Count;
                    actTransformsElementTVI.BringIntoView();
                    referencesSteps = 7;
                    TransformsTimer.Start();
                    actTimer = TransformsTimer;
                    referenceElementsTimer.Stop();
                    break;
                case 7:
                    presentation.AddTextToInformationBox("Create the DigestMethod-Element");
                    addChildElement("ds:Reference", soap.SecuredSoap.GetElementsByTagName("ds:DigestMethod")[referencesCounter], false);
                    referencesSteps = 8;
                    break;

                case 8:
                    presentation.AddTextToInformationBox("Calculate the DigestValue for the Referenced Element");
                    referencesSteps = 9;
                    break;
                case 9:
                    presentation.AddTextToInformationBox("Find the referenced element");
                    referencesSteps = 10;
                    break;
                case 10:
                    if (soap.GetXPathTransForm())
                    {
                        presentation.AddTextToInformationBox("URI is empty, so the complete document is referenced");
                        TreeViewItem temp = (TreeViewItem)presentation.treeView.Items[0];
                        temp.BringIntoView();
                        animateElement(temp);
                        referencesSteps = 11;
                    }
                    else
                    {
                        presentation.AddTextToInformationBox("The referenced element is: " + actElementToReference.Name);
                        actElementToReferenceTVI.BringIntoView();
                        animateElement(actElementToReferenceTVI);
                        referencesSteps = 13;
                    }
                    break;
                case 11:
                    presentation.AddTextToInformationBox("Do the XPath Transform on the referenced element");
                    xPathSteps = xPath.Split(new char[] { '/' });
                    xPathArrayCounter = 1;
                    referencesSteps = 12;
                    break;
                case 12:
                    if (xPathArrayCounter <= xPathSteps.Length - 1)
                    {
                        presentation.AddTextToInformationBox("Actual element: " + xPathSteps[xPathArrayCounter]);
                        animateItemInclChilds(xPathSteps[xPathArrayCounter]);
                        xPathArrayCounter++;
                    }
                    else
                    {
                        referencesSteps = 13;
                    }
                    break;
                case 13:
                    presentation.AddTextToInformationBox("Canonicalize the actual Element");
                    referencesSteps = 14;
                    break;

                case 14:
                    presentation.AddTextToInformationBox("The canonicalized element is:");
                    rootItem = (TreeViewItem)presentation.treeView.Items[0];
                    TreeViewItem canon = new TreeViewItem();
                    StreamReader sreader = new StreamReader(soap.CanonicalizeNodeWithExcC14n(actElementToReference));
                    string canonString = sreader.ReadToEnd();
                    XmlDocument doc = new XmlDocument();
                    XmlElement elem = doc.CreateElement("temp");
                    elem.InnerXml = canonString;
                    TextBlock tb = new TextBlock
                    {
                        FontSize = 14,
                        Text = "Canonicalized Element"
                    };
                    XmlNode node = elem.ChildNodes[0];
                    presentation.CopyXmlToTreeView(node, canon);
                    presentation.treeView.Items.Clear();
                    canon.IsExpanded = true;
                    presentation.treeView.Items.Add(canon);
                    referencesSteps = 15;
                    break;
                case 15:
                    referencesSteps = 16;
                    break;
                case 16:
                    presentation.AddTextToInformationBox("Calculate SHA-1 Hash of this Element:");
                    referencesSteps = 17;
                    break;
                case 17:
                    XmlNode digest = soap.SecuredSoap.GetElementsByTagName("ds:DigestValue")[referencesCounter];
                    presentation.AddTextToInformationBox("SHA-1 Value:" + digest.InnerText);
                    referencesSteps = 18;

                    break;
                case 18:
                    presentation.treeView.Items.Clear();
                    presentation.treeView.Items.Add(rootItem);

                    presentation.AddTextToInformationBox("Create Digest Value Element");
                    actDigestValue = addChildElement(actReferenceElementTVI, soap.SecuredSoap.GetElementsByTagName("ds:DigestValue")[referencesCounter], false);
                    referencesSteps = 19;
                    break;

                case 19:
                    presentation.AddTextToInformationBox("Add Digest Value to Digest Value Element");
                    createValue(actDigestValue, soap.SecuredSoap.GetElementsByTagName("ds:DigestValue")[referencesCounter].InnerText);
                    referencesSteps = 20;
                    break;
                case 20:
                    referencesSteps = 0;
                    referencesCounter++;
                    referenceElementsTimer.Stop();
                    referencesTimer.Start();
                    actTimer = referencesTimer;
                    break;
            }
        }



        private void referenceTimer_Tick(object sender, EventArgs e)
        {
            // Anzahl der zu signierenden Elemente (Noch 1)
            if (referencesCounter < elementsToSign.Length)
            {
                referencesSteps = 0;
                referenceElementsTimer.Start();
                actTimer = referenceElementsTimer;
                actElementToReference = elementsToSign[referencesCounter];
                referencesTimer.Stop();

            }
            else
            {
                dispatcherTimer.Start();
                actTimer = dispatcherTimer;
                referencesTimer.Stop();
            }

        }

        private void TransformsTimer_Tick(object sender, EventArgs e)
        {
            switch (transformSteps)
            {
                case 0:
                    if (transformsCounter > 1)
                    {
                        presentation.AddTextToInformationBox("Create a XPath Transform Element");
                        //TreeViewItem test = addChildElement("ds:Transforms",actReferenceElement.FirstChild.ChildNodes[0],true);
                        addChildElement(actTransformsElementTVI, actReferenceElement.FirstChild.ChildNodes[0], true);
                        xPathHelper = null;
                        transformSteps = 1;
                    }
                    else
                    {
                        transformSteps = 4;
                    }
                    break;
                case 1:
                    presentation.AddTextToInformationBox("Create XPath String");
                    xPath = "/" + actElementToReference.Name;
                    xPathHelper = actElementToReference;
                    transformSteps = 2;
                    presentation.AddTextToInformationBox("Actual XPath: " + xPath);
                    break;
                case 2:

                    xPathHelper = (XmlElement)xPathHelper.ParentNode;
                    xPath = "/" + xPathHelper.Name + xPath;

                    animateElement(xPathHelper.Name);
                    presentation.AddTextToInformationBox("Actual XPath: " + xPath);
                    if (xPathHelper.Name.Equals("s:Envelope"))
                    {
                        transformSteps = 3;
                    }
                    break;
                case 3:
                    presentation.AddTextToInformationBox("Add XPath Value to Transform Element");
                    createValue((TreeViewItem)actTransformsElementTVI.Items[0], xPath);
                    transformSteps = 4;
                    break;
                case 4:
                    //Create C14N
                    presentation.AddTextToInformationBox("Create Canonicalization Transform");
                    if (soap.GetXPathTransForm())
                    {
                        addChildElement(actTransformsElementTVI, actReferenceElement.FirstChild.ChildNodes[1], false);
                    }
                    else
                    {
                        addChildElement(actTransformsElementTVI, actReferenceElement.FirstChild.ChildNodes[0], false);
                    }
                    TransformsTimer.Stop();
                    referenceElementsTimer.Start();
                    actTimer = referencesTimer;
                    transformSteps = 0;
                    break;


            }
        }


        #endregion 
    }
}
