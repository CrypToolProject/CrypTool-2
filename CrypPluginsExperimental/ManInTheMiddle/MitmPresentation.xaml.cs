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

namespace ManInTheMiddle
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class MitmPresentation : UserControl
    {
        #region Fields

        private TreeViewItem _foundItem;
        private TreeViewItem _soapItem;
        private string _lastURI;
        private readonly Hashtable _namespacesTable;
        private readonly ManInTHeMiddle _manInTheMiddle;

        #endregion


        public TreeViewItem SoapItem
        {
            get => _soapItem;
            set => _soapItem = value;
        }
        #region Constructor

        public MitmPresentation(ManInTHeMiddle mitm)
        {
            InitializeComponent();
            _foundItem = new TreeViewItem();
            _lastURI = "";
            _namespacesTable = new Hashtable();
            _manInTheMiddle = mitm;
        }

        #endregion

        #region Methods

        private string GetNameFromPanel(StackPanel panel, bool prefix)
        {
            foreach (object childPanel in panel.Children)
            {
                if (childPanel.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock tb = (TextBlock)childPanel;
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
        public void CopyXmlToTreeView(XmlNode xmlNode, TreeViewItem treeViewItemParent, XmlNode[] inputParameter)
        {
            _namespacesTable.Clear();

            CopyXmlToTreeViewReal(xmlNode, ref treeViewItemParent, inputParameter);

        }
        private void CopyXmlToTreeViewReal(XmlNode xNode, ref TreeViewItem tviParent, XmlNode[] parameter)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
            if (xNode != null)
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
                tbName.Text = xNode.Name;

                tbTagOpen.Foreground = elemBrush;
                tbTagClose.Foreground = elemBrush;
                tbName.Foreground = elemBrush;
                if (!xNode.NodeType.ToString().Equals("Text"))
                {
                    item.Name = "OpenItemXmlNode";
                    panel.Name = "OpenPanelXMLNode";
                    TreeViewItem closeitem = new TreeViewItem();
                    panel.Children.Insert(0, tbTagOpen);
                    panel.Children.Add(tbName);
                    if (!xNode.NamespaceURI.Equals(""))
                    {
                        InsertNamespace(panel, xNode.NamespaceURI, xNode.Prefix);
                    }
                    if (xNode.Attributes != null)
                    {
                        InsertAttributes(panel, xNode.Attributes);
                    }


                    panel.Children.Add(tbTagClose);
                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    tviParent.Items.Add(item);
                    foreach (XmlNode node in parameter)
                    {
                        if (node.Name.Equals(xNode.Name))
                        {
                            AddEditImageToPanel(panel, xNode.Name);
                        }
                    }
                    if (xNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xNode.ChildNodes)
                        {
                            _lastURI = xNode.NamespaceURI; ;
                            CopyXmlToTreeViewReal(child, ref item, parameter);
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
                        Text = "/" + xNode.Name
                    };
                    panel1.Children.Add(elem1Name);
                    panel1.Children.Add(elem1Close);

                    closeitem.Header = panel1;

                    tviParent.Items.Add(closeitem);
                }
                else
                {
                    item.Name = "OpenItemTextNode";
                    panel.Name = "OpenPanelTextNode";
                    TextBlock tbText = new TextBlock
                    {
                        Name = "TextNode",
                        Text = xNode.Value
                    };
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    tviParent.Items.Add(item);
                }
            }
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
            editImage.MouseEnter += new MouseEventHandler(ImageMouseEnterEventHAndler);
            editImage.MouseLeave += new MouseEventHandler(ImageMouseLeaveEventHandler);


        }
        private StackPanel InsertNamespace(StackPanel panel, string nspace, string Prefix)
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
        private StackPanel InsertAttributes(StackPanel panel, XmlAttributeCollection attributes)
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
        private TreeViewItem FindTreeViewItemByName(TreeViewItem item, string name)
        {
            StackPanel panelHeader = (StackPanel)item.Header;
            string nameFromPanel = GetNameFromPanel(panelHeader, false);
            if (nameFromPanel != null)

            {
                if (nameFromPanel.Equals(name))
                {
                    _foundItem = item;
                    return item;
                }
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                FindTreeViewItemByName(childItem, name);
            }
            if (_foundItem != null)
            {
                return _foundItem;
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
                                //  soap.securedSOAP.GetElementsByTagName("")[0];

                                if (item.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                                {
                                    StackPanel parentPanel = (StackPanel)item.Header;
                                    if (parentPanel.Children.Count > 2)
                                    {
                                        TextBlock block = (TextBlock)parentPanel.Children[1];
                                        _manInTheMiddle.soap.GetElementsByTagName(block.Text)[0].InnerText = box.Text;
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

        #endregion

        #region EventHandlers

        private void TreeViewSelectedItemChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ClearBoxes(_soapItem);
            TreeView treeViewItem = (TreeView)sender;
            if (treeViewItem.SelectedItem != null)
            {
                TreeViewItem item = (TreeViewItem)treeViewItem.SelectedItem;
                StackPanel tempPanel = (StackPanel)item.Header;
                object temp = tempPanel.Children[0];
                string type = temp.GetType().ToString();

                if (type.Equals("System.Windows.Controls.TextBlock"))
                {
                    XmlNode[] parameter = _manInTheMiddle.getParameter();

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
                                Text = _manInTheMiddle.soap.GetElementsByTagName(name)[0].InnerXml.ToString()
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
        private void BoxKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ClearBoxes(_soapItem);
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
        private void ImageMouseEnterEventHAndler(object sender, MouseEventArgs e)
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

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
