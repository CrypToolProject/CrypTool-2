using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WebService
{
    public class DecryptionAnimation
    {
        private TreeViewItem foundItem, actualEncryptedData, actualEncryptedDataEndTag, actualEncryptedDataParent, actualEncryptedKeyItem, actualEncryptedKeyEndTagItem;
        private int status, actualEncryptedKeyNumber, totalKeyNumber;
        private readonly DispatcherTimer _decryptionTimer;
        private readonly WebServicePresentation presentation;
        private readonly ArrayList encryptedKeyTreeviewElements;
        private readonly DoubleAnimation opacityAnimation;
        private readonly DoubleAnimation opacityAnimation1;
        private DoubleAnimation TextSizeAnimation;
        private DoubleAnimation TextSizeAnimationReverse;
        private DoubleAnimation TextSizeAnimation1;
        private DoubleAnimation TextSizeAnimationReverse1;
        public bool allowExecute;

        public DispatcherTimer DecryptionTimer => _decryptionTimer;

        public DecryptionAnimation(WebServicePresentation presentation)
        {
            this.presentation = presentation;
            allowExecute = false;
            opacityAnimation = new DoubleAnimation(1, 0.0, TimeSpan.FromSeconds(1));
            opacityAnimation1 = new DoubleAnimation(1, 0.0, TimeSpan.FromSeconds(1));
            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            encryptedKeyTreeviewElements = new ArrayList();
            _decryptionTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 0)
            };
            _decryptionTimer.Tick += new EventHandler(decryptionTimer_Tick);

        }
        public void initializeAnimation()
        {
            totalKeyNumber = presentation.WebService.Validator.GetEncryptedKeyNumber();
            actualEncryptedKeyNumber = 0;
            status = 1;


        }

        private void decryptionTimer_Tick(object sender, EventArgs e)
        {
            switch (status)
            {
                case 1:
                    presentation._animationStepsTextBox.Text += "\n Found EnrcyptedKey Element";
                    presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    _decryptionTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    TreeViewItem item = (TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber];
                    item.IsSelected = true;
                    actualEncryptedKeyItem = item;
                    if (item.Parent != null)
                    {
                        TreeViewItem actualEncryptedKeyParentItem = (TreeViewItem)item.Parent;
                        int t = actualEncryptedKeyParentItem.Items.IndexOf(item) + 1;
                        actualEncryptedKeyEndTagItem = (TreeViewItem)actualEncryptedKeyParentItem.Items[t];
                    }
                    item.BringIntoView();

                    animateFoundElements(item, item);
                    status++;

                    break;
                case 2:
                    presentation._animationStepsTextBox.Text += "\n Encrypt Key";
                    presentation._animationStepsTextBox.Text += "\n -> find EncryptionMethod";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "EncryptionMethod", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "EncryptionMethod", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/xenc:EncryptionMethod", 1));
                    status++;
                    break;
                case 3:
                    presentation._animationStepsTextBox.Text += "\n -> Get information about the key to decrypt the encrypted key";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "KeyInfo", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "KeyInfo", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/ds:KeyInfo", 1));
                    presentation._animationStepsTextBox.Text += "\n The information shows that the key was encrypted mit the Web Service public key";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    status++;
                    break;
                case 4:
                    presentation._animationStepsTextBox.Text += "\n ->  Get the cipher data";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "CipherData", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "CipherData", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/xenc:CipherData", 1));
                    status++;
                    break;
                case 5:
                    presentation._animationStepsTextBox.Text += "\n ->  Get the cipher value inside cipher data";
                    presentation._animationStepsTextBox.Text += "\n ->  which represents the encrypted key";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);

                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "CipherValue", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "CipherValue", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/xenc:CipherValue", 1));
                    status++;
                    break;
                case 6:
                    presentation._animationStepsTextBox.Text += "\n -> Decrypt the encrypted key";
                    presentation._animationStepsTextBox.Text += "\n -> Get the reference list which shows";
                    presentation._animationStepsTextBox.Text += "\n -> which data was encrypted with the encrypted key";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "ReferenceList", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "ReferenceList", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/xenc:ReferenceList", 1));
                    status++;
                    break;
                case 7:
                    presentation._animationStepsTextBox.Text += "\n -> Get the reference";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "DataReference", 1).BringIntoView();
                    animateFoundElements(findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "DataReference", 1), findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "/xenc:DataReference", 1));

                    status++;
                    break;

                case 8:
                    presentation._animationStepsTextBox.Text += "\n -> Get the referenced data";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    }
                    TreeViewItem dataReference = findItem((TreeViewItem)encryptedKeyTreeviewElements[actualEncryptedKeyNumber], "DataReference", 1);
                    dataReference.BringIntoView();
                    StackPanel tempHeader1 = (StackPanel)dataReference.Header;
                    string uri = "";
                    foreach (object obj in tempHeader1.Children)
                    {
                        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                        {

                            TextBlock block = (TextBlock)obj;
                            if (block.Name.Equals("attributeValue"))
                            {
                                string wert = block.Text;
                                string[] splitter = wert.Split(new char[] { '#' });
                                uri = splitter[1].ToString();
                                string[] splitter2 = uri.Split(new char[] { '"' });
                                uri = splitter2[0].ToString();
                                uri.Trim();

                            }



                        }
                    }

                    TreeViewItem test = findReferencedData((TreeViewItem)presentation.SoapInputItem.Items[0], "EncryptedData", uri);

                    actualEncryptedData = test;
                    TreeViewItem parent = (TreeViewItem)actualEncryptedData.Parent;
                    actualEncryptedDataParent = parent;
                    int n = actualEncryptedDataParent.Items.IndexOf(actualEncryptedData) + 1;
                    actualEncryptedDataEndTag = (TreeViewItem)actualEncryptedDataParent.Items[n];
                    animateFoundElements(test, (TreeViewItem)actualEncryptedDataParent.Items[n]);






                    status++;
                    break;
                case 9:
                    presentation._animationStepsTextBox.Text += "\n -> Decrypt the data and replace ";
                    presentation._animationStepsTextBox.Text += "\n -> the EncryptedData Element with the decrypted data";
                    if (presentation._animationStepsTextBox.LineCount > 0)
                    {
                        presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                        // n = this.actualEncryptedDataParent.Items.IndexOf(actualEncryptedData) + 1;
                    }
                    animationAddElements(actualEncryptedData, actualEncryptedDataEndTag);
                    status++;


                    break;

                case 10:
                    bool isContentEncrypted = false;
                    parent = (TreeViewItem)actualEncryptedData.Parent;
                    actualEncryptedDataParent = parent;
                    StackPanel tempHeader2 = (StackPanel)actualEncryptedData.Header;
                    foreach (object obj in tempHeader2.Children)
                    {
                        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                        {

                            TextBlock block = (TextBlock)obj;
                            if (block.Text.Contains("http://www.w3.org/2001/04/xmlenc#Content"))
                            {
                                isContentEncrypted = true;

                            }



                        }
                    }
                    int pos = parent.Items.IndexOf(actualEncryptedData);
                    animateFoundElements((TreeViewItem)parent.Items[pos], (TreeViewItem)parent.Items[pos]);

                    TreeViewItem tempItem = new TreeViewItem();

                    presentation.CopyXmlToTreeView(presentation.WebService.Validator.DecryptSingleElementByKeyNumber(actualEncryptedKeyNumber), tempItem);

                    TreeViewItem decryptedDataItem = (TreeViewItem)tempItem.Items[0];
                    tempItem.Items.Remove(decryptedDataItem);
                    //   tempItem.Items.Remove(decryptedDataEndTagItem);
                    TreeViewItem tempDecryptedDataItem = decryptedDataItem;
                    ArrayList decryptedDataItems = new ArrayList();

                    int decryptedDataItemCounter = decryptedDataItem.Items.Count;
                    int index = 1;

                    ItemCollection collection = decryptedDataItem.Items;

                    foreach (TreeViewItem actualItem in collection)
                    {
                        decryptedDataItems.Add(actualItem);
                    }
                    TreeViewItem tempItem2 = null;
                    for (int i = 0; i < decryptedDataItems.Count; i++)
                    {
                        tempItem2 = (TreeViewItem)decryptedDataItems[i];
                        StackPanel header = (StackPanel)tempItem2.Header;
                        int h = header.Children.Count;
                        //for (int i = 0; i < h; i++)
                        //{
                        //   TextBlock block =(TextBlock) header.Children[i];
                        //   string text = block.Text;
                        //}


                        decryptedDataItem.Items.Remove(tempItem2);
                        if (isContentEncrypted)
                        {
                            for (int j = 0; j < h; j++)
                            {
                                TextBlock block = (TextBlock)header.Children[j];
                                string text = block.Text;
                            }
                            parent.Items.Add(tempItem2);
                        }
                        else
                        {
                            int inde = parent.Items.IndexOf(actualEncryptedData);
                            int rtrt = pos;

                            parent.Items.Insert(pos, tempItem2);
                            pos++;
                        }
                        parent.Items.Remove(actualEncryptedData);
                        parent.Items.Remove(actualEncryptedDataEndTag);


                    }

                    for (int z = 1; z <= decryptedDataItemCounter; z++)
                    {
                        //TreeViewItem tempItem2;
                        //int helpcounter;
                        //helpcounter = z;


                        //    tempItem2 = (TreeViewItem)decryptedDataItem.Items[index-1];
                        //    StackPanel header = (StackPanel)tempItem2.Header;
                        //    int h=header.Children.Count;
                        //    //for (int i = 0; i < h; i++)
                        //    //{
                        //    //   TextBlock block =(TextBlock) header.Children[i];
                        //    //   string text = block.Text;
                        //    //}


                        //decryptedDataItem.Items.Remove(tempItem2);
                        //if (isContentEncrypted)
                        //{   for (int i = 0; i < h; i++)
                        //    {
                        //        TextBlock block = (TextBlock)header.Children[i];
                        //        string text = block.Text;
                        //    }
                        //    parent.Items.Add(tempItem2);
                        //}
                        //else
                        //{
                        //    parent.Items.Insert(pos, tempItem2);
                        //}
                        //   decryptedDataItemCounter--;
                        index--;

                    }


                    int intemCounter = decryptedDataItem.Items.Count;
                    //     TreeViewItem decryptedDataEndTagItem = (TreeViewItem)tempItem.Items[1];



                    //parent.Items.Insert(pos, decryptedDataItem);
                    //parent.Items.Insert(pos+1, decryptedDataEndTagItem);
                    //      this.animateFoundElements((TreeViewItem)parent.Items[pos], (TreeViewItem)parent.Items[pos+1]);

                    status++;
                    break;
                case 11:
                    presentation._animationStepsTextBox.Text += "\n -> Remove EncryptedKey Element from security header";
                    presentation._animationStepsTextBox.ScrollToLine(presentation._animationStepsTextBox.LineCount - 1);
                    animationAddElements(actualEncryptedKeyItem, actualEncryptedKeyEndTagItem);


                    status++;
                    break;
                case 12:

                    actualEncryptedKeyItem.BringIntoView();
                    TreeViewItem keyParent = (TreeViewItem)actualEncryptedKeyItem.Parent;
                    keyParent.Items.Remove(actualEncryptedKeyItem);
                    keyParent.Items.Remove(actualEncryptedKeyEndTagItem);
                    status++;
                    _decryptionTimer.Stop();
                    presentation.AnimationController.ControllerTimer.Start();
                    break;
                case 13:
                    if (actualEncryptedKeyNumber + 1 < totalKeyNumber)
                    {

                        actualEncryptedKeyNumber++;
                        status = 1;
                        allowExecute = false;

                    }

                    break;






            }
        }

        public TreeViewItem findReferencedData(TreeViewItem item, string bezeichner, string reference)
        {

            StackPanel tempHeader1 = (StackPanel)item.Header;
            string Bezeichner = getNameFromPanel(tempHeader1);
            if (Bezeichner != null)
            {
                if (Bezeichner.Equals(bezeichner))
                {
                    foreach (object obj in tempHeader1.Children)
                    {
                        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                        {

                            TextBlock block = (TextBlock)obj;
                            if (block.Name.Equals("attributeValue"))
                            {
                                string wert = block.Text;
                                string[] splitter = wert.Split(new char[] { '"' });
                                if (splitter[1].ToString().Equals(reference))
                                {
                                    foundItem = item;

                                    return item;
                                }

                            }



                        }
                    }
                }

                {

                    foreach (TreeViewItem childItem in item.Items)
                    {
                        findReferencedData(childItem, bezeichner, reference);
                    }
                    if (foundItem != null)
                    {
                        return foundItem;
                    }
                }
            }

            return null;
        }

        public TreeViewItem findItem(TreeViewItem item, string bezeichner, int n)
        {

            StackPanel tempHeader1 = (StackPanel)item.Header;
            string Bezeichner = getNameFromPanel(tempHeader1);
            if (Bezeichner != null)
            {
                if (Bezeichner.Equals(bezeichner))
                {
                    foundItem = item;

                    return item;
                }
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                findItem(childItem, bezeichner, 4);
            }
            if (foundItem != null)
            {
                return foundItem;
            }
            return null;
        }
        private void animationAddElements(TreeViewItem t1, TreeViewItem t2)
        {
            Storyboard sb = new Storyboard();

            sb.Children.Add(opacityAnimation);
            sb.Children.Add(opacityAnimation1);



            sb.Children[0].BeginTime = new TimeSpan(0, 0, 2);
            sb.Children[1].BeginTime = new TimeSpan(0, 0, 2);


            Storyboard.SetTarget(opacityAnimation, t1);


            Storyboard.SetTarget(opacityAnimation1, t2);



            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(TextBlock.OpacityProperty));


            Storyboard.SetTargetProperty(opacityAnimation1, new PropertyPath(TextBlock.OpacityProperty));



            sb.Begin();
        }
        public void animateFoundElements(TreeViewItem item, TreeViewItem item2)
        {
            Storyboard storyBoard = new Storyboard();
            item.IsSelected = true;
            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            storyBoard.Children.Add(TextSizeAnimation);
            storyBoard.Children.Add(TextSizeAnimationReverse);
            storyBoard.Children[0].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[1].BeginTime = new TimeSpan(0, 0, 4);
            storyBoard.Children.Add(TextSizeAnimation1);
            storyBoard.Children.Add(TextSizeAnimationReverse1);
            storyBoard.Children[2].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[3].BeginTime = new TimeSpan(0, 0, 4);
            Storyboard.SetTarget(TextSizeAnimation, item);
            Storyboard.SetTarget(TextSizeAnimationReverse, item);
            Storyboard.SetTarget(TextSizeAnimation1, item2);
            Storyboard.SetTarget(TextSizeAnimationReverse1, item2);
            Storyboard.SetTargetProperty(TextSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            storyBoard.Begin();
            StackPanel panel = (StackPanel)item.Header;
            TextBlock block = (TextBlock)panel.Children[0];

            storyBoard.Children.Clear();
        }
        //private string getNameFromPanel(StackPanel panel)
        //{
        //    foreach (object obj in panel.Children)
        //    {
        //        if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
        //        {
        //            TextBlock tb = (TextBlock)obj;
        //            if (tb.Name.Equals("tbName"))
        //            {
        //                string name = tb.Text;
        //                string[] splitter = name.Split(new Char[] { ':' });
        //                name = splitter[splitter.Length - 1];
        //                return name;
        //            }
        //        }
        //    }
        //    return null;
        //}
        private string getNameFromPanel(StackPanel panel)
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
        public void fillEncryptedDataTreeviewElements()
        {
            if (presentation.SoapInputItem != null && presentation.SoapInputItem.Items.Count > 0)
            {
                findItems((TreeViewItem)presentation.SoapInputItem.Items[0], "xenc:EncryptedKey");

                initializeAnimation();
            }
        }
        public TreeViewItem findItems(TreeViewItem item, string bezeichner)
        {

            StackPanel tempHeader1 = (StackPanel)item.Header;

            // string Bezeichner = getNameFromPanel(tempHeader1);
            TextBlock text1 = (TextBlock)tempHeader1.Children[1];
            if (text1.Text.Equals(bezeichner))
            {

                foundItem = item;
                if (bezeichner.Equals("xenc:EncryptedKey"))
                {
                    encryptedKeyTreeviewElements.Add(item);
                }


                return item;

            }
            foreach (TreeViewItem childItem in item.Items)
            {
                findItems(childItem, bezeichner);

            }
            if (foundItem != null)
            {
                return foundItem;
            }

            return null;
        }
        public DispatcherTimer getDecryptiontimer()
        {
            return _decryptionTimer;
        }



    }
}
