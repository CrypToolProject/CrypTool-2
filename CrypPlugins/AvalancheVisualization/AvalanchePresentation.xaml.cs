
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Linq;
using CrypTool.PluginBase.IO;


namespace AvalancheVisualization
{
    /// <summary>
    /// Interaction logic for AvaAESPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("AvalancheVisualization.Properties.Resources")]
    public partial class AvalanchePresentation : UserControl
    {


        #region variables

        public CStreamWriter debugStream = new CStreamWriter();
        public int roundNumber = 1;
        public int action = 1;
        public AutoResetEvent buttonNextClickedEvent;
        // public AutoResetEvent end;
        
        public byte[][] sBox = new byte[16][];
        public byte[][] states = new byte[40][];
        public byte[][] statesB = new byte[40][];
        public byte[][] keyList = new byte[11][];
        public string[,] lrData = new string[17, 2];
        public string[,] lrDataB = new string[17, 2];

        public byte[] tempState;
        public byte[] textA;
        public byte[] textB;
        public byte[] keyA;
        public byte[] key;

        public byte[] unchangedCipher;
        public byte[] changedCipher;
        public string[] differentBits;
        public int sequencePosition;
        public int flippedSeqPosition;
        public double avgNrDiffBit;
        double avalanche;
        static Random rnd = new Random();

        public byte[][] roundConstant = new byte[12][];
        public int keysize;
        public double progress;
        public int shift = 0;
        public int mode;

        public bool lostFocus = false;
        public DES desDiffusion;
        public AES aesDiffusion;
        public int roundDES;
        public int slide;
        public int contrast;

        public bool canModify = false;
        public bool canModifyDES = false;
        public bool canStop = false;
        public bool canModifyOthers = false;
        private bool updateColor = false;
        public bool newChanges;

        public string[] leftHalf = new string[32];
        public string[] rightHalf = new string[32];
        public string[] leftHalfB = new string[32];
        public string[] rightHalfB = new string[32];

        public byte[] seqA;
        public byte[] seqB;
        public byte[] newText;
        public byte[] newKey;
        public byte[] currentDES;

        private CrypTool.Plugins.AvalancheVisualization.AvalancheVisualization avalancheVisualization;
        #endregion

        #region constructor

        public AvalanchePresentation(CrypTool.Plugins.AvalancheVisualization.AvalancheVisualization av)
        {
            InitializeComponent();

            avalancheVisualization = av;
            inputInBits.IsVisibleChanged += onVisibleChanged;
            OrigInitialStateGrid.IsVisibleChanged += onVisibleChanged;
            modifiedInitialStateGrid.IsVisibleChanged += onVisibleChanged;
            initStateTitle.IsVisibleChanged += onTitleChanged;
            modificationGridDES.IsVisibleChanged += onVisibleChanged;
            afterRoundsGrid.IsVisibleChanged += onVisibleChanged;
            buttonsPanel.IsVisibleChanged += onTitleChanged;
            inputGridDES.IsVisibleChanged += modify;
            othersGrid.IsVisibleChanged += modifyOthers;
            modificationGridDES.IsVisibleChanged += modify;
            //ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
        }

        #endregion

        #region methods

        //loads initial unchanged text
        public void loadInitialState(byte[] initState, byte[] initKey)
        {
            int i = 1;
            int j = 17;
            int k = 1;

            string binSequence = binaryAsString(initState).Replace(" ", "");
            string keyBinSequence = binaryAsString(initKey).Replace(" ", "");


            if (mode == 0)
            {
                while (i <= 16)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = initState[i - 1].ToString("X2");
                    i++;
                }

                if (keysize == 0)
                {
                    int index128 = 0;

                    while (j <= 32 && index128 < 16)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = initKey[index128].ToString("X2");
                        j++;
                        index128++;
                    }


                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit{0}", a))).Text =
                            keyBinSequence[a - 1].ToString();
                }
                else if (keysize == 1)
                {


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;

                    }


                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit192_{0}", a))).Text =
                            keyBinSequence[a - 1].ToString();
                }
                else
                {
                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;

                    }

                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();

                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit256_{0}", a))).Text =
                            keyBinSequence[a - 1].ToString();
                }

            }
            else
            {
                for (int a = 1; a <= binSequence.Length; a++)
                    ((TextBlock)this.FindName(string.Format("desBit{0}", a))).Text = binSequence[a - 1].ToString();
                for (int a = 1; a <= keyBinSequence.Length; a++)
                    ((TextBlock)this.FindName(string.Format("desKeyBit{0}", a))).Text =
                        keyBinSequence[a - 1].ToString();

                string firstHalf = binSequence.Substring(0, 32);
                string secondHalf = binSequence.Substring(32, 32);
                string firstKeyHalf = keyBinSequence.Substring(0, 32);
                string secondKeyHalf = keyBinSequence.Substring(32, 32);

                origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
                origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            }
        }

        public void loadChangedMsg(byte[] msg, bool textChanged)
        {
            int k = 33;
            int i = 1;


            if (mode == 0)
            {



                if (radioDecimal.IsChecked == true)
                {
                    while (k <= 48)
                    {

                        ((TextBlock)this.FindName("initStateTxtBlock" + k)).Text = msg[i - 1].ToString();

                        i++;
                        k++;

                    }
                }

                if (radioHexa.IsChecked == true)
                {
                    while (k <= 48)
                    {

                        ((TextBlock)this.FindName("initStateTxtBlock" + k)).Text = msg[i - 1].ToString("X2");

                        i++;
                        k++;

                    }
                }
            }
            else
            {

                if (radioBinaryDes.IsChecked == true)
                {

                    string binSequence = binaryAsString(msg).Replace(" ", "");
                    string firstHalf = binSequence.Substring(0, 32);
                    string secondHalf = binSequence.Substring(32, 32);

                    modTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);

                }

                if (radioDecimalDes.IsChecked == true)
                {

                    string strB = decimalAsString(textB);
                    modTextDES.Text = strB;


                }

                if (radioHexaDes.IsChecked == true)
                {
                    string strB = hexaAsString(textB);
                    modTextDES.Text = strB;

                    string keyStrB = hexaAsString(key);
                    modKeyDES.Text = keyStrB;
                }




            }
        }

        public void loadChangedKey(byte[] newKey)
        {

            if (mode == 0)
            {

                if (radioDecimal.IsChecked == true)
                {

                    if (keysize == 1)
                    {

                        int i = 1;

                        while (i <= 24)
                        {
                            ((TextBlock)this.FindName("modKey192_" + i)).Text = newKey[i - 1].ToString();
                            i++;

                        }

                    }

                    else if (keysize == 2)
                    {
                        int i = 1;

                        while (i <= 32)
                        {
                            ((TextBlock)this.FindName("modKey256_" + i)).Text = newKey[i - 1].ToString();
                            i++;

                        }
                    }

                    else

                    {

                        int l = 49;
                        int i = 1;

                        while (l <= 64)
                        {
                            ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = newKey[i - 1].ToString();
                            l++;
                            i++;
                        }
                    }
                }


                if (radioHexa.IsChecked == true)
                {


                    if (keysize == 1)
                    {

                        int i = 1;

                        while (i <= 24)
                        {
                            ((TextBlock)this.FindName("modKey192_" + i)).Text = newKey[i - 1].ToString("X2");
                            i++;

                        }

                    }

                    else if (keysize == 2)
                    {
                        int i = 1;

                        while (i <= 32)
                        {
                            ((TextBlock)this.FindName("modKey256_" + i)).Text = newKey[i - 1].ToString("X2");
                            i++;

                        }
                    }

                    else

                    {

                        int l = 49;
                        int i = 1;

                        while (l <= 64)
                        {
                            ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = newKey[i - 1].ToString("X2");
                            l++;
                            i++;
                        }
                    }
                }

            }

            else

            {

                if (radioBinaryDes.IsChecked == true)
                {

                    string keyBinSequence = binaryAsString(newKey).Replace(" ", "");

                    string firstKeyHalf = keyBinSequence.Substring(0, 32);
                    string secondKeyHalf = keyBinSequence.Substring(32, 32);

                    modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
                }

                if (radioDecimalDes.IsChecked == true)
                {
                    string keyStrB = decimalAsString(key);
                    modKeyDES.Text = keyStrB;
                }

                if (radioHexaDes.IsChecked == true)
                {
                    string keyStrB = hexaAsString(key);
                    modKeyDES.Text = keyStrB;
                }

            }
        }

        public void coloringKey()
        {
            clearKeyEffect();

            switch (mode)
            {

                case 0:

                    clearKeyColors();

                    IEnumerable<Border> gridChildren;
                    IEnumerable<Border> gridChildren2;

                    if (keysize == 0)
                    {
                        gridChildren = originalKeyGrid.Children.OfType<Border>();
                        gridChildren2 = modifiedKeyGrid.Children.OfType<Border>();
                    }
                    else if (keysize == 1)
                    {
                        gridChildren = originalKeyGrid192.Children.OfType<Border>();
                        gridChildren2 = modifiedKeyGrid192.Children.OfType<Border>();
                    }
                    else
                    {
                        gridChildren = originalKeyGrid256.Children.OfType<Border>();
                        gridChildren2 = modifiedKeyGrid256.Children.OfType<Border>();
                    }

                    int a = 0;
                    int b = 0;

                    foreach (Border bor in gridChildren)
                    {
                        if (keyA[a] != key[a])
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");

                        a++;
                    }

                    foreach (Border bor in gridChildren2)
                    {
                        if (keyA[b] != key[b])
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");

                        b++;
                    }

                    break;

                case 1:

                    if (radioDecimalDes.IsChecked == true || radioHexaDes.IsChecked == true)
                    {
                        for (byte i = 0; i < keyA.Length; i++)
                        {
                            if (keyA[i] != key[i])
                            {
                                List<int> positions = arrangePos();
                                TextEffect ti = new TextEffect();

                                ti.PositionStart = positions[i];
                                ti.Foreground = Brushes.Red;

                                if (radioDecimalDes.IsChecked == true)
                                    ti.PositionCount = 4;
                                if (radioHexaDes.IsChecked == true)
                                    ti.PositionCount = 2;

                                origKeyDES.TextEffects.Add(ti);
                                modKeyDES.TextEffects.Add(ti);
                            }
                        }
                    }
                    else
                    {
                        for (byte i = 0; i < origKeyDES.Text.Length; i++)
                        {
                            if (origKeyDES.Text[i] != modKeyDES.Text[i])
                            {
                                TextEffect ti2 = new TextEffect();
                                ti2.PositionStart = i;
                                ti2.PositionCount = 1;
                                ti2.Foreground = Brushes.Red;
                                origKeyDES.TextEffects.Add(ti2);
                                modKeyDES.TextEffects.Add(ti2);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        public void coloringText()
        {
            clearTextEffect();

            switch (mode)
            {

                case 0:

                    clearColors();

                    IEnumerable<Border> gridChildren = Grid1.Children.OfType<Border>();
                    IEnumerable<Border> gridChildren2 = Grid2.Children.OfType<Border>();

                    int a = 0;
                    int b = 0;

                    foreach (Border bor in gridChildren)
                    {
                        if (textA[a] != textB[a])
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");

                        a++;
                    }

                    foreach (Border bor in gridChildren2)
                    {
                        if (textA[b] != textB[b])
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");

                        b++;
                    }

                    break;

                case 1:

                    if (radioDecimalDes.IsChecked == true || radioHexaDes.IsChecked == true)
                    {

                        for (byte i = 0; i < textA.Length; i++)
                        {
                            if (textA[i] != textB[i])
                            {
                                List<int> positions = arrangePos();
                                TextEffect ti = new TextEffect();
                                ti.PositionStart = positions[i];
                                ti.Foreground = Brushes.Red;

                                if (radioDecimalDes.IsChecked == true)
                                    ti.PositionCount = 4;
                                if (radioHexaDes.IsChecked == true)
                                    ti.PositionCount = 3;

                                origTextDES.TextEffects.Add(ti);
                                modTextDES.TextEffects.Add(ti);
                            }
                        }
                    }
                    else
                    {

                        for (byte i = 0; i < origTextDES.Text.Length; i++)
                        {
                            if (origTextDES.Text[i] != modTextDES.Text[i])
                            {
                                TextEffect ti = new TextEffect();
                                ti.PositionStart = i;
                                ti.Foreground = Brushes.Red;
                                ti.PositionCount = 1;
                                origTextDES.TextEffects.Add(ti);
                                modTextDES.TextEffects.Add(ti);
                            }
                        }

                    }
                    break;

                default:
                    break;

            }
        }



        public void setAndLoadButtons()
        {
            slide = 2;
            backButton.Visibility = Visibility.Hidden;
            ChangesMadeButton.Visibility = Visibility.Hidden;
            contButton.Visibility = Visibility.Visible;

            if (mode == 0)
            {
                initMsg.Text = string.Empty;
                initKey.Text = string.Empty;
                initMsg.Text = Properties.Resources.Initial_Message;
                initKey.Text = Properties.Resources.Initial_Key;

                // radioHexa.IsChecked = true;
                aesCheckBox.Visibility = Visibility.Hidden;
                inputInBits.Visibility = Visibility.Hidden;
                changeMsgAes.Visibility = Visibility.Hidden;
                initStateTitle.Visibility = Visibility.Hidden;

                modifiedInitialStateGrid.Visibility = Visibility.Visible;
                radioButtons.Visibility = Visibility.Visible;
                setButtonsScrollViewer();

                showChangeTitle.Visibility = Visibility.Visible;
                buttonsPanel.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;
                inputDataButton.Visibility = Visibility.Visible;

                if (keysize == 1)
                    modifiedKeyGrid192.Visibility = Visibility.Visible;
                else if (keysize == 2)
                    modifiedKeyGrid256.Visibility = Visibility.Visible;
                else
                    modifiedKeyGrid.Visibility = Visibility.Visible;




                showChangeTitle.Text = showChangeTitle.Text.TrimEnd('1', '2', '5', '6', '8', '9');
                if (keysize == 1)
                    showChangeTitle.Inlines.Add(new Run("192") { Foreground = Brushes.White });
                else if (keysize == 2)
                    showChangeTitle.Inlines.Add(new Run("256") { Foreground = Brushes.White });
                else
                    showChangeTitle.Inlines.Add(new Run("128") { Foreground = Brushes.White });

            }
            else
            {


                inputGridDES.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Visible;
                buttonsPanel.Visibility = Visibility.Visible;
                inputDataButton.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;

                setButtonsScrollViewer();
            }
        }

        public bool stop = false;





        private List<int> arrangePos()
        {

            List<int> intList = new List<int>();

            int[] decPos = new int[] { 0, 4, 7, 11, 15, 19, 23, 27 };
            int[] hexPos = new int[] { 0, 3, 6, 9, 12, 15, 18, 21 };

            if (mode == 1)
            {
                if (radioDecimalDes.IsChecked == true)
                    intList.AddRange(decPos);
                if (radioHexaDes.IsChecked == true)
                    intList.AddRange(hexPos);
            }
            else
            {
                intList.AddRange(hexPos);
            }


            return intList;
        }

        public void clearColors()
        {
            IEnumerable<Border> gridChildren = Grid1.Children.OfType<Border>();
            IEnumerable<Border> gridChildren2 = Grid2.Children.OfType<Border>();


            foreach (Border bor in gridChildren)
                bor.Background = Brushes.Transparent;

            foreach (Border bor in gridChildren2)
                bor.Background = Brushes.Transparent;


        }

        public void clearKeyColors()
        {
            IEnumerable<Border> gridChildren;
            IEnumerable<Border> gridChildren2;

            if (keysize == 0)
            {
                gridChildren = originalKeyGrid.Children.OfType<Border>();
                gridChildren2 = modifiedKeyGrid.Children.OfType<Border>();
            }

            else if (keysize == 1)
            {
                gridChildren = originalKeyGrid192.Children.OfType<Border>();
                gridChildren2 = modifiedKeyGrid192.Children.OfType<Border>();
            }

            else
            {
                gridChildren = originalKeyGrid256.Children.OfType<Border>();
                gridChildren2 = modifiedKeyGrid256.Children.OfType<Border>();
            }

            foreach (Border bor in gridChildren)
                bor.Background = Brushes.Transparent;

            foreach (Border bor in gridChildren2)
                bor.Background = Brushes.Transparent;
        }

        //Loads byte information into the respective columns
        public void loadBytePropagationData()
        {
            int a = 0;

            List<TextBox> tmp = createTxtBoxList();
            byte[] state = arrangeColumn(states[0]);


            foreach (TextBox tb in tmp)
            {
                tb.Text = tempState[a].ToString("X2");
                a++;
            }

            int i = 1;

            byte[] subBytes1 = arrangeColumn(states[1]);
            byte[] shiftRows1 = arrangeColumn(states[2]);
            byte[] mixColumns1 = arrangeColumn(states[3]);
            byte[] addKey1 = arrangeColumn(states[4]);
            byte[] subBytes2 = arrangeColumn(states[5]);
            byte[] shiftRows2 = arrangeColumn(states[6]);
            byte[] mixColumns2 = arrangeColumn(states[7]);
            byte[] addKey2 = arrangeColumn(states[8]);
            byte[] subBytes3 = arrangeColumn(states[9]);
            byte[] shiftRows3 = arrangeColumn(states[10]);
            byte[] mixColumns3 = arrangeColumn(states[11]);
            byte[] addKey3 = arrangeColumn(states[12]);

            while (i <= 16)
            {

                ((TextBlock)this.FindName("roundZero" + i)).Text = state[i - 1].ToString("X2");
                ((TextBlock)this.FindName("sBoxRound1_" + i)).Text = subBytes1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound1_" + i)).Text = shiftRows1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns1_" + i)).Text = mixColumns1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey1_" + i)).Text = addKey1[i - 1].ToString("X2");

                ((TextBlock)this.FindName("sBoxRound2_" + i)).Text = subBytes2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound2_" + i)).Text = shiftRows2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns2_" + i)).Text = mixColumns2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey2_" + i)).Text = addKey2[i - 1].ToString("X2");


                ((TextBlock)this.FindName("sBoxRound3_" + i)).Text = subBytes3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound3_" + i)).Text = shiftRows3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns3_" + i)).Text = mixColumns3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey3_" + i)).Text = addKey3[i - 1].ToString("X2");
                i++;
            }
        }

        //applies a PKCS7 padding 
        public void padding()
        {
            if (textB.Length != 16)
            {
                byte[] temp = new byte[16];
                int x = 0;
                int padding = 16 - textB.Length;
                if (textB.Length < 16)
                {
                    foreach (byte b in textB)
                    {
                        temp[x] = b;
                        x++;
                    }
                    while (x < 16)
                    {
                        // temp[x] = (byte)padding;
                        temp[x] = 0;
                        x++;
                    }
                }
                else
                {
                    while (x < 16)
                    {
                        temp[x] = textB[x];
                        x++;
                    }
                }
                textB = temp;
            }
        }

        //shows different intermediate states of the AES encryption process
        public void printIntermediateStates(byte[][] states, byte[][] statesB)
        {
            List<TextBlock> tmp = createTxtBlockList(3);
            List<TextBlock> tmpB = createTxtBlockList(4);


            byte[] state = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = arrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

            int i = 0;
            int j = 0;

            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }
        }


        public string toString(byte[] result)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append(b.ToString() + " ");
            }
            return sb.ToString();
        }

        private byte[] rearrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[1] = input[4];
            result[2] = input[8];
            result[3] = input[12];
            result[4] = input[1];
            result[5] = input[5];
            result[6] = input[9];
            result[7] = input[13];
            result[8] = input[2];
            result[9] = input[6];
            result[10] = input[10];
            result[11] = input[14];
            result[12] = input[3];
            result[13] = input[7];
            result[14] = input[11];
            result[15] = input[15];
            return result;
        }

        //shows initial state
        public void setUpSubByte(byte[][] states, byte[][] statesB)
        {


            List<TextBlock> tmp = createTxtBlockList(1);
            List<TextBlock> tmpB = createTxtBlockList(2);


            byte[] state = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = arrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

            int i = 0;
            int j = 0;

            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }


            //encryptionProgress();

        }

        // Value of avalanche effect
        public double calcAvalancheEffect(int flippedBits, Tuple<string, string> strTuple)
        {

            double avalancheEffect = ((double)flippedBits / strTuple.Item1.Length) * 100;
            double roundUp = Math.Round(avalancheEffect, 1, MidpointRounding.AwayFromZero);

            return roundUp;
        }

        //average number of flipped bits per byte
        public double avgNrperByte(int flippedBits)
        {
            if (mode == 0)
                avgNrDiffBit = ((double)flippedBits / 16);
            else
                avgNrDiffBit = ((double)flippedBits / 8);

            avgNrDiffBit = Math.Round(avgNrDiffBit, 1, MidpointRounding.AwayFromZero);

            return avgNrDiffBit;

        }

        //calculates longest identical bit sequence
        public int longestIdenticalSequence(string[] str)
        {
            int lastCount = 0;
            int longestCount = 0;
            int i = 0;

            int offset = 0;

            while (i < str.Length)
            {
                if (str[i] == " ")
                {
                    lastCount++;

                    if (lastCount > longestCount)
                    {
                        longestCount = lastCount;
                        offset = i - lastCount + 1;
                    }
                }
                else
                {
                    lastCount = 0;
                }

                i++;
            }
            sequencePosition = offset;
            return longestCount;
        }

        public int longestFlippedSequence(string[] str)
        {
            int lastCount = 0;
            int longestCount = 0;
            int i = 0;

            int offset = 0;

            while (i < str.Length)
            {
                if (str[i] == "X")
                {
                    lastCount++;

                    if (lastCount > longestCount)
                    {
                        longestCount = lastCount;
                        offset = i - lastCount + 1;
                    }
                }
                else
                {
                    lastCount = 0;
                }

                i++;
            }
            flippedSeqPosition = offset;


            return longestCount;


        }

        public int countOccurrence(string[] str)
        {
            int occurence = new int();

            if (mode == 1)
            {
                int count = 0;
                int count2 = 0;
                int i = 0;
                int j = 32;

                while (i <= 31)
                {

                    if (str[i].Equals("X"))
                        count++;

                    i++;
                }

                while (j <= 63)
                {
                    if (str[j].Equals("X"))
                        count2++;

                    j++;
                }

                //only left half
                if (!count.Equals(0) && count2.Equals(0))
                    occurence = 0;
                //only right half
                else if (!count2.Equals(0) && count.Equals(0))
                    occurence = 1;
                //no changes
                else if (count.Equals(0) && count2.Equals(0))
                    occurence = 2;
                else
                    occurence = 3;
            }

            else
            {
                int count = 0;

                foreach (string st in str)
                {
                    if (st.Equals("X"))
                        count++;
                }

                if (count == 0)
                    occurence = 2;

            }


            return occurence;

        }

        public void showOccurence(int occurrence)
        {
            extraordinaryOccur.Text = string.Empty;

            if (occurrence != 3)
            {
                if (mode == 1)
                    extraordinaryOccur.Visibility = Visibility.Visible;

                if (occurrence == 0)
                    extraordinaryOccur.Text = Properties.Resources.OnlyLeft;
                else if (occurrence == 1)
                    extraordinaryOccur.Text = Properties.Resources.OnlyRight;
                else
                {
                    if (mode == 1)
                        extraordinaryOccur.Text = Properties.Resources.NoChanges;

                    if (mode == 0)
                    {
                        extraordinaryOccurAes.Visibility = Visibility.Visible;
                        extraordinaryOccurAes.Text = Properties.Resources.NoChanges;
                    }
                }
            }

            else
            {
                extraordinaryOccur.Visibility = Visibility.Hidden;
                extraordinaryOccurAes.Visibility = Visibility.Hidden;
            }

        }

        //set colors of pie chart
        public void setColors()
        {
            SolidColorBrush brushA = new SolidColorBrush();
            SolidColorBrush brushB = new SolidColorBrush();

            if (contrast == 1)
            {
                brushA.Color = Color.FromRgb(0, 0, 0);
                brushB.Color = Color.FromRgb(255, 255, 255);

                Cb1.Background = Brushes.White;
                Cb2.Background = Brushes.Black;

                Cbclass1.Background = Brushes.White;
                Cbclass2.Background = Brushes.Black;
            }
            else
            {
                brushA.Color = Color.FromRgb(255, 40, 0);
                brushB.Color = Color.FromRgb(76, 187, 23);

                Cb1.Background = (Brush)new BrushConverter().ConvertFromString("#4cbb17");
                Cb2.Background = (Brush)new BrushConverter().ConvertFromString("#ff2800");

                Cbclass1.Background = (Brush)new BrushConverter().ConvertFromString("#4cbb17");
                Cbclass2.Background = (Brush)new BrushConverter().ConvertFromString("#ff2800");
            }

            flippedBitsPiece.Fill = brushA;
            unflippedBitsPiece.Fill = brushB;
        }

        //set position of angles of pie chart
        public void setAngles(double angle_A, double angle_B)
        {
            flippedBitsPiece.angle = angle_A;
            unflippedBitsPiece.angle = angle_B;
            flippedBitsPiece.pieceRotation = 0;
            unflippedBitsPiece.pieceRotation = angle_A;

            if (avalanche == 0)
                  unflippedBitsPiece.angle = 359.9;
                 
        
        }

        //prints out current statistical values of cipher
        public void showStatistics(int bitsFlipped, int longestLength, int longestflipped,
            Tuple<string, string> strTuple)
        {
            stats1.Inlines.Add(new Run(" " + bitsFlipped.ToString())
            {
                Foreground = Brushes.Red,
                FontWeight = FontWeights.DemiBold
            });

            if (bitsFlipped > 1 || bitsFlipped == 0)
                stats1.Inlines.Add(
                    new Run(string.Format(Properties.Resources.StatsBullet1_Plural, strTuple.Item1.Length, avalanche)));
            else
                stats1.Inlines.Add(
                    new Run(string.Format(Properties.Resources.StatsBullet1, strTuple.Item1.Length, avalanche)));

            stats2.Inlines.Add(
                new Run(string.Format(Properties.Resources.StatsBullet2, longestLength.ToString(), sequencePosition)));
            stats3.Inlines.Add(
                new Run(string.Format(Properties.Resources.StatsBullet3, longestflipped.ToString(), flippedSeqPosition)));
            if (mode != 2)
            {
                stats4.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet4, avgNrDiffBit)));
            }
        }

        //Signalizes flipped bits and highlight the differences 
        public void showBitSequence(Tuple<string, string> strTuple)
        {

            string[] diffBits = new string[strTuple.Item1.Length];


            for (int i = 0; i < strTuple.Item1.Length; i++)
            {
                if (strTuple.Item1[i] != strTuple.Item2[i])
                {
                    diffBits[i] = "X";
                }
                else
                    diffBits[i] = " ";

            }

            differentBits = diffBits;



            if (mode == 0)
            {
                int a = 0;
                int b = 256;

                while (a < 128 && b < 384)
                {
                    ((TextBlock)this.FindName("txt" + b)).Text = diffBits[a].ToString();
                    a++;
                    b++;
                }

                int j = 0;
                int k = 128;

                while (j < 128 && k < 256)
                {
                    if (diffBits[j] == "X")
                    {
                        ((TextBlock)this.FindName("txt" + j)).Background =
                            (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)this.FindName("txt" + k)).Background =
                            (Brush)new BrushConverter().ConvertFromString("#fe6f5e");

                    }
                    j++;
                    k++;
                }
            }
            else if (mode == 1)
            {
                int a = 0;
                int b = 129;

                while (a < 64 && b < 193)
                {
                    ((TextBlock)this.FindName("desTxt" + b)).Text = diffBits[a].ToString();
                    ((TextBlock)this.FindName("desTxt" + b)).Foreground = Brushes.Red;
                    a++;
                    b++;
                }

                int j = 1;
                int k = 65;

                while (j < 64 && k < 128)
                {
                    if (diffBits[j - 1] == "X")
                    {
                        ((TextBlock)this.FindName("desTxt" + j)).Background =
                            (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)this.FindName("desTxt" + k)).Background =
                            (Brush)new BrushConverter().ConvertFromString("#fe6f5e");

                    }
                    j++;
                    k++;
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (string str in diffBits)
                {
                    sb.Append(str);
                }
                string differentBits2 = Regex.Replace(sb.ToString(), ".{8}", "$0 ");
                TB3.Text = differentBits2;
            }

        }

        //Shows binary values of each cipher byte
        public void displayBinaryValues(byte[] cipherStateA, byte[] cipherStateB)
        {

            byte[] textToArrange = arrangeText(cipherStateA);
            byte[] textToArrangeB = arrangeText(cipherStateB);


            string encryptionStateA = binaryAsString(textToArrange).Replace(" ", "");
            string encryptionStateB = binaryAsString(textToArrangeB).Replace(" ", "");


            int a = 0;
            int b = 128;
            while (a < 128 && b < 256)
            {
                ((TextBlock)this.FindName("txt" + a)).Text = encryptionStateA[a].ToString();
                ((TextBlock)this.FindName("txt" + b)).Text = encryptionStateB[a].ToString();
                a++;
                b++;
            }

        }

        public void toStringArray(int round)
        {

            string A = lrData[round, 0];
            string B = lrData[round, 1];
            string C = lrDataB[round, 0];
            string D = lrDataB[round, 1];


            for (int i = 0; i < 32; i++)
            {
                leftHalf[i] = A.Substring(i, 1);
                rightHalf[i] = B.Substring(i, 1);
                leftHalfB[i] = C.Substring(i, 1);
                rightHalfB[i] = D.Substring(i, 1);

            }

            string bitSeqA = string.Concat(A, B);
            string bitseqB = string.Concat(C, D);

            seqA = getByteArray(bitSeqA);
            seqB = getByteArray(bitseqB);
        }



        public byte[] getByteArray(string str)
        {
            byte[] byteArray = new byte[8];

            for (int i = 0; i < 8; i++)
                byteArray[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);

            return byteArray;
        }

        public void displayBinaryValuesDES()
        {
            /*int a = 0;
            int b = 64;
            while (a < 64 && b < 128)
            {
                ((TextBlock)this.FindName("txt" + a)).Text = msgA[a];
                ((TextBlock)this.FindName("txt" + b)).Text = msgB[a];
                a++;
                b++;
            }*/

            bitGridDES.Visibility = Visibility.Visible;

            int a = 0;
            int b = 32;
            int c = 64;
            int d = 96;
            while (a < 32 && b < 64 && c < 96 && d < 128)
            {

                ((TextBlock)this.FindName("txt" + a)).Text = leftHalf[a];
                ((TextBlock)this.FindName("txt" + b)).Text = rightHalf[a];
                ((TextBlock)this.FindName("txt" + c)).Text = leftHalfB[a];
                ((TextBlock)this.FindName("txt" + d)).Text = rightHalfB[a];

                a++;
                b++;
                c++;
                d++;
            }


            int i = 1;
            int j = 33;
            int k = 65;
            int l = 97;
            while (i < 33 && j < 65 && k < 97 && l < 129)
            {
                ((TextBlock)this.FindName("desTxt" + i)).Text = leftHalf[i - 1];
                ((TextBlock)this.FindName("desTxt" + j)).Text = rightHalf[i - 1];
                ((TextBlock)this.FindName("desTxt" + k)).Text = leftHalfB[i - 1];
                ((TextBlock)this.FindName("desTxt" + l)).Text = rightHalfB[i - 1];

                i++;
                j++;
                k++;
                l++;
            }




        }

        //transforms to string of binary values
        public string binaryAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();

            sb = new StringBuilder();

            var encoding = Encoding.GetEncoding(437);

            for (int i = 0; i < byteSequence.Length; i++)
            {
                if (byteSequence[i] <= 127)
                {
                    sb.Append(Convert.ToString(byteSequence[i], 2).PadLeft(8, '0') + " ");
                }
                else
                {
                    sb.Append(Convert.ToString(byteSequence[i], 2) + " ");
                }
            }

            return sb.ToString();
        }

        //string of decimal values
        public string decimalAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();

            foreach (byte b in byteSequence)
                sb.AppendFormat("{0:D3}{1}", b, " ");

            return sb.ToString();
            ;

        }


        //string of hexadecimal values
        public string hexaAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var v in byteSequence)
                sb.AppendFormat("{0:X2}{1}", v, " ");

            return sb.ToString();
        }

        //counts how many bits are flipped after comparison
        public int nrOfBitsFlipped(byte[] nr1, byte[] nr2)
        {
            int shift = 7;
            int count = 0;

            byte[] comparison = this.exclusiveOr(nr1, nr2);

            for (int j = 0; j < comparison.Length; j++)
            {

                for (int i = 0; i <= shift; i++)
                {

                    if ((comparison[j] & (1 << i)) != 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        // XOR operation
        public byte[] exclusiveOr(byte[] input1, byte[] input2)
        {
            byte[] result = new byte[input1.Length];



            for (int i = 0; i < input1.Length; i++)
            {
                result[i] = (byte)(input1[i] ^ input2[i]);
            }

            return result;
        }

        //returns a tuple of strings
        public Tuple<string, string> binaryStrings(byte[] cipherStateA, byte[] cipherStateB)
        {
            byte[] textToArrange;
            byte[] textToArrangeB;
            string encryptionStateA = "";
            string encryptionStateB = "";

            if (mode == 0)
            {
                textToArrange = arrangeText(cipherStateA);
                textToArrangeB = arrangeText(cipherStateB);
                encryptionStateA = binaryAsString(textToArrange).Replace(" ", "");
                encryptionStateB = binaryAsString(textToArrangeB).Replace(" ", "");

            }
            else if (mode == 2 || mode == 3 || mode == 4)
            {
                encryptionStateA = binaryAsString(cipherStateA).Replace(" ", "");
                encryptionStateB = binaryAsString(cipherStateB).Replace(" ", "");
            }
            else
            {

                encryptionStateA = lrData[roundDES, 0] + lrData[roundDES, 1];
                encryptionStateB = lrDataB[roundDES, 0] + lrDataB[roundDES, 1];

            }

            var tuple = new Tuple<string, string>(encryptionStateA, encryptionStateB);

            return tuple;

        }

        //Set content of toolTips
        public void setToolTips()
        {
            tp1.Content = avalanche + " %";
            tp2.Content = (100 - avalanche) + " %";
        }

        private void removeBackground()
        {
            IEnumerable<Button> StackPanelChildren = buttonsPanel.Children.OfType<Button>();



            foreach (Button button in StackPanelChildren)
                button.ClearValue(BackgroundProperty);


        }

        //clear background colors
        private void removeColors()
        {

            if (mode == 0)
            {
                int a = 0;
                int b = 128;
                while (a < 128 && b < 256)
                {
                    ((TextBlock)this.FindName("txt" + a)).Background = Brushes.Transparent;
                    ((TextBlock)this.FindName("txt" + b)).Background = Brushes.Transparent;
                    a++;
                    b++;
                }
            }
            else
            {
                int j = 1;
                int k = 65;
                while (j < 65 && k < 129)
                {
                    ((TextBlock)this.FindName("desTxt" + j)).Background = Brushes.Transparent;
                    ((TextBlock)this.FindName("desTxt" + k)).Background = Brushes.Transparent;
                    j++;
                    k++;
                }
                modificationGridDES.Visibility = Visibility.Hidden;

            }

        }

        //updates roundnr displayed on GUI
        public void changeRoundNr(int number)
        {

            afterRoundsTitle.Text = afterRoundsTitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            afterRoundsSubtitle.Text = afterRoundsSubtitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (mode == 0)
            {
                if (keysize == 1)
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES192));
                else if (keysize == 2)
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES256));
                else
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES128));
            }
            else if (mode == 1)
                afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_DES));


            afterRoundsSubtitle.Inlines.Add(new Run(string.Format("{0}", number)));

        }

        public void changeTitle()
        {
            if (mode == 0)
            {
                initStateTitle.Text = initStateTitle.Text.TrimEnd('1', '2', '5', '6', '8', '9');
                if (keysize == 1)
                    initStateTitle.Inlines.Add(new Run("192") { Foreground = Brushes.White });
                else if (keysize == 2)
                    initStateTitle.Inlines.Add(new Run("256") { Foreground = Brushes.White });
                else
                    initStateTitle.Inlines.Add(new Run("128") { Foreground = Brushes.White });
            }
            else
            {
                if (mode == 2)
                {
                    othersOrigTitle.Text = Properties.Resources.HashFunctionInit;
                    othersModTitle.Text = Properties.Resources.HashFunctionMod;
                    lbl.Text = Properties.Resources.HashFunctionInitBin;
                    lbl2.Text = Properties.Resources.HashFunctionModBin;
                    othersSubtitle.Text = Properties.Resources.HashFunctionSubtitle;
                }
                if (mode == 4)
                    othersSubtitle.Text = Properties.Resources.ModernCipherSubtitle;
            }
        }

        //new position after shiftrows operation
        public TextBlock afterShifting(int position, int round)
        {

            int newPosition = 0;

            if (position == 1 || position == 5 || position == 9 || position == 13)
            {
                newPosition = position % 16;
            }
            if (position == 2 || position == 6 || position == 10 || position == 14)
            {
                newPosition = ((position - 4) % 16 + 16) % 16;
            }
            if (position == 3 || position == 7 || position == 11 || position == 15)
            {
                newPosition = ((position - 8) % 16 + 16) % 16;
            }
            if (position == 4 || position == 8 || position == 12 || position == 16)
            {
                newPosition = ((position - 12) % 16 + 16) % 16;

                if (position == 12)
                    newPosition = 16;
            }


            TextBlock txtBlock = ((TextBlock)this.FindName("shiftRowRound" + round + "_" + newPosition));

            return txtBlock;
        }



        public void clearElements()
        {
            toGeneral.Visibility = Visibility.Hidden;
            contButton.Visibility = Visibility.Hidden;

            if (mode == 0)
            {
                OrigInitialStateGrid.Visibility = Visibility.Hidden;
                modifiedInitialStateGrid.Visibility = Visibility.Hidden;
                initStateTitle.Visibility = Visibility.Hidden;
                showChangeTitle.Visibility = Visibility.Hidden;
                radioButtons.Visibility = Visibility.Hidden;
                generalViewAES.Visibility = Visibility.Hidden;


                bitRepresentationSV.ScrollToHorizontalOffset(0.0);
            }
            else
            {
                modificationGridDES.Visibility = Visibility.Hidden;
                generalViewDES.Visibility = Visibility.Hidden;
            }
            afterRoundsTitle.Text = string.Empty;


        }

        public void showElements()
        {

            contButton.Visibility = Visibility.Visible;

            if (mode == 0)
            {
                afterRoundsGrid.Visibility = Visibility.Visible;
                bitRepresentationGrid.Visibility = Visibility.Visible;
                curvedLinesCanvas.Visibility = Visibility.Visible;
            }

            if (mode == 1)
                adjustStats();


            bitsData.Visibility = Visibility.Visible;
            Cb1.Visibility = Visibility.Visible;
            Cb2.Visibility = Visibility.Visible;
            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;
            afterRoundsTitle.Visibility = Visibility.Visible;
            afterRoundsSubtitle.Visibility = Visibility.Visible;

        }


        public List<TextBox> createTxtBoxList()
        {
            List<TextBox> txtBoxList = new List<TextBox>();

            for (int i = 1; i <= 16; i++)
                txtBoxList.Add((TextBox)this.FindName("txtBox" + i));

            return txtBoxList;
        }

        public List<Border> createBorderList(int borderListNr)
        {
            List<Border> borderList = new List<Border>();

            switch (borderListNr)
            {
                case 1:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 2:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 3:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 4:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 5:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 6:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 7:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 8:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 9:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 10:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 11:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 12:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 13:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 14:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 15:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 16:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 17:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 18:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 19:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 20:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 21:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 22:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 23:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 24:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                default:
                    break;

            }

            return borderList;

        }

        public List<TextBlock> createTxtBlockList(int txtBlockType)
        {
            List<TextBlock> txtBlockList = new List<TextBlock>();

            switch (txtBlockType)
            {
                case 0:

                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("initStateTxtBlock" + i));
                    }
                    break;
                case 1:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterAddKey" + i));
                    }
                    break;
                case 2:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterAddKey" + i));
                    }
                    break;
                case 3:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterRoundTxt" + i));
                    }
                    break;
                case 4:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterRoundTxt" + i));
                    }
                    break;
                case 5:
                    for (int i = 256; i < 384; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("txt" + i));
                    }
                    break;
                case 6:
                    for (int i = 1; i < 129; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("bit" + i));
                        txtBlockList.Add((TextBlock)this.FindName("keyBit" + i));

                    }
                    break;
                case 7:
                    for (int i = 1; i < 65; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("desBit" + i));
                        txtBlockList.Add((TextBlock)this.FindName("desKeyBit" + i));
                    }
                    break;

                default:
                    break;

            }
            return txtBlockList;
        }


        public void setButtonsScrollViewer()
        {
            if (mode == 0)
            {
                if (keysize == 1)
                {
                    afterRound11Button.Visibility = Visibility.Visible;
                    afterRound12Button.Visibility = Visibility.Visible;

                }
                else if (keysize == 2)
                {
                    afterRound11Button.Visibility = Visibility.Visible;
                    afterRound12Button.Visibility = Visibility.Visible;
                    afterRound13Button.Visibility = Visibility.Visible;
                    afterRound14Button.Visibility = Visibility.Visible;

                }
            }
            else
            {
                afterRound11Button.Visibility = Visibility.Visible;
                afterRound12Button.Visibility = Visibility.Visible;
                afterRound13Button.Visibility = Visibility.Visible;
                afterRound14Button.Visibility = Visibility.Visible;
                afterRound15Button.Visibility = Visibility.Visible;
                afterRound16Button.Visibility = Visibility.Visible;
            }
        }


        private byte[] arrangeColumn(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[1] = input[1];
            result[2] = input[2];
            result[3] = input[3];
            result[4] = input[4];
            result[5] = input[5];
            result[6] = input[6];
            result[7] = input[7];
            result[8] = input[8];
            result[9] = input[9];
            result[10] = input[10];
            result[11] = input[11];
            result[12] = input[12];
            result[13] = input[13];
            result[14] = input[14];
            result[15] = input[15];
            return result;
        }

        private byte[] arrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[4] = input[1];
            result[8] = input[2];
            result[12] = input[3];
            result[1] = input[4];
            result[5] = input[5];
            result[9] = input[6];
            result[13] = input[7];
            result[2] = input[8];
            result[6] = input[9];
            result[10] = input[10];
            result[14] = input[11];
            result[3] = input[12];
            result[7] = input[13];
            result[11] = input[14];
            result[15] = input[15];


            return result;

        }


        public void encryptionProgress(int roundNr)
        {
            switch (mode)
            {
                case 0:

                    if (keysize == 0)
                        progress = (roundNr + 2) * 0.076;
                    else if (keysize == 1)
                        progress = (roundNr + 2) * 0.066;
                    else
                        progress = (roundNr + 2) * 0.058;
                    break;

                case 1:

                    progress = (roundNr + 2) * 0.0526;
                    break;

                case 2:
                case 3:
                case 4:

                    progress = 0.5;
                    if (!string.IsNullOrEmpty(TB2.Text))
                        progress = 1;
                    break;
                default:
                    break;
            }

            avalancheVisualization.ProgressChanged(progress, 1);

        }


        #endregion

        // #region AES methods
        public void createSBox()
        {
            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            byte[] temp =
            {
                99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125,
                250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165,
                229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131,
                44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106,
                203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168,
                81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68,
                23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94,
                11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141,
                213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116,
                31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248,
                152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104,
                65, 153, 45, 15, 176, 84, 187, 22
            };
            x = 0;
            int y = 0;
            foreach (byte b in temp)
            {
                sBox[y][x] = b;
                x++;
                if (x == 16)
                {
                    y++;
                    x = 0;
                }
            }

        }

        public byte[][] setSBox()
        {

            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            List<int> temp = new List<int>();
            while (x < 256)
            {
                temp.Add(x);
                x++;
            }
            int y = 0;
            x = 0;
            int z;
            while (y < 16)
            {
                while (x < 16)
                {
                    z = rnd.Next(temp.Count);
                    sBox[y][x] = Convert.ToByte(temp[z]);
                    temp.RemoveAt(z);
                    x++;
                }
                y++;
                x = 0;
            }
            x = 0;
            y = 0;

            return sBox;
        }


        #region Event handlers 



        private void doneButton_Click(object sender, RoutedEventArgs e)
        {

            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;
            ChangesMadeButton.IsEnabled = true;
            ChangesMadeButton.Opacity = 1;

            if (mode == 0)
            {
                inputInBits.Visibility = Visibility.Hidden;

                string[] strSequence = new string[128];
                string[] result = new string[16];
                string[][] textBits = new string[16][];

                int l = 0;

                for (int i = 1; i < 129; i++)
                    strSequence[i - 1] = ((TextBlock)this.FindName(string.Format("bit{0}", i))).Text;

                string bitSequence = string.Join("", strSequence);

                /*for (int j = 0; j < 16; j++)
                    textBits[j] = new string[8];*/

                for (int k = 0; k < 16; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

                /*for (int i = 0; i < 16; i++)
                    textBits[i] = result;*/

                newText = result.Select(s => Convert.ToByte(s, 2)).ToArray();

                textB = newText;
                canStop = true;
                string keyBitSequence = "";
                string[] keyResult = new string[key.Length];
                string[][] keyBits = new string[key.Length][];

                string bitName = "keyBit{0}";
                int bits = 129;

                int m = 0;

                if (keysize == 1)
                {
                    string[] strKeySequence2 = new string[192];

                    for (int i = 1; i < 193; i++)
                        strKeySequence2[i - 1] = ((TextBlock)this.FindName(string.Format("keyBit192_{0}", i))).Text;

                    keyBitSequence = string.Join("", strKeySequence2);
                }
                else if (keysize == 2)
                {

                    string[] strKeySequence3 = new string[256];

                    for (int i = 1; i < 257; i++)
                        strKeySequence3[i - 1] = ((TextBlock)this.FindName(string.Format("keyBit256_{0}", i))).Text;

                    keyBitSequence = string.Join("", strKeySequence3);
                }
                else
                {
                    string[] strKeySequence = new string[128];

                    for (int i = 1; i < bits; i++)
                        strKeySequence[i - 1] = ((TextBlock)this.FindName(string.Format(bitName, i))).Text;

                    keyBitSequence = string.Join("", strKeySequence);
                }

                for (int j = 0; j < 16; j++)
                    keyBits[j] = new string[8];


                for (int k = 0; k < key.Length; k++)
                {
                    keyResult[k] = keyBitSequence.Substring(m, 8);
                    m += 8;
                }


                newKey = keyResult.Select(s => Convert.ToByte(s, 2)).ToArray();
                key = newKey;
                aesDiffusion = new AES(newKey, newText);


                aesDiffusion.checkKeysize();
                byte[] temporaryB = aesDiffusion.checkTextLength();
                aesDiffusion.executeAES(false);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   loadChangedMsg(temporaryB, true);
                   loadChangedKey(newKey);
                   //  setAndLoadButtons();

               }, null);

                coloringText();
                coloringKey();
                statesB = aesDiffusion.statesB;

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   setAndLoadButtons();

               }, null);

            }
            else
            {

                string[] strSequence = new string[64];
                string[] result = new string[8];
                string keyBitSequence = "";
                string[] keyResult = new string[keyA.Length];

                int l = 0;
                int m = 0;
                for (int i = 1; i < 65; i++)
                    strSequence[i - 1] = ((TextBlock)this.FindName(string.Format("desBit{0}", i))).Text;

                string bitSequence = string.Join("", strSequence);

                /*for (int j = 0; j < 16; j++)
                    textBits[j] = new string[8];*/


                for (int k = 0; k < 8; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

                /*for (int i = 0; i < 16; i++)
                    textBits[i] = result;*/

                newText = result.Select(s => Convert.ToByte(s, 2)).ToArray();

                textB = newText;
                canStop = true;
                string[] strKeySequence = new string[64];

                for (int i = 1; i < 65; i++)
                    strKeySequence[i - 1] = ((TextBlock)this.FindName(string.Format("desKeyBit{0}", i))).Text;

                keyBitSequence = string.Join("", strKeySequence);


                for (int k = 0; k < keyA.Length; k++)
                {
                    keyResult[k] = keyBitSequence.Substring(m, 8);
                    m += 8;
                }

                newKey = keyResult.Select(s => Convert.ToByte(s, 2)).ToArray();
                key = newKey;

                desDiffusion = new DES(newText, newKey);
                desDiffusion.textChanged = true;
                desDiffusion.DESProcess();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   loadChangedMsg(newText, true);
                   loadChangedKey(newKey);
                   setAndLoadButtons();

               }, null);

                coloringText();
                coloringKey();
                lrDataB = desDiffusion.lrDataB;
                currentDES = desDiffusion.outputCiphertext;

            }

            avalancheVisualization.OutputStream = string.Format("{0}{1}{2}", avalancheVisualization.generatedData(0), avalancheVisualization.generatedData(1), avalancheVisualization.generatedData(2));

            //  buttonNextClickedEvent.Set();

        }


        public void updateDataColor()
        {
            updateColor = true;
            clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!updateColor)
            {
                ChangesMadeButton.IsEnabled = false;
                ChangesMadeButton.Opacity = 0.3;
            }

            List<TextBlock> tBList = new List<TextBlock>();

            if (mode == 0)
            {

                for (int i = 1; i < 129; i++)
                    tBList.Add((TextBlock)this.FindName(string.Format("bit{0}", i)));


                if (keysize == 0)
                {

                    for (int i = 1; i < 129; i++)
                        tBList.Add((TextBlock)this.FindName(string.Format("keyBit{0}", i)));

                }
                else if (keysize == 1)
                {

                    for (int i = 1; i < 193; i++)
                        tBList.Add((TextBlock)this.FindName(string.Format("keyBit192_{0}", i)));

                }
                else
                {

                    for (int i = 1; i < 257; i++)
                        tBList.Add((TextBlock)this.FindName(string.Format("keyBit256_{0}", i)));

                }

            }
            else
            {
                for (int i = 1; i < 65; i++)
                {
                    tBList.Add((TextBlock)this.FindName(string.Format("desBit{0}", i)));
                    tBList.Add((TextBlock)this.FindName(string.Format("desKeyBit{0}", i)));
                }

            }



            foreach (TextBlock tb in tBList)
            {

                if (tb.Foreground == Brushes.Red)

                {
                    if (tb.Text == "1")
                        tb.Text = "0";
                    else
                        tb.Text = "1";

                    tb.Foreground = Brushes.Black;
                }

            }



        }

        private void inputDataButton_Click(object sender, RoutedEventArgs e)
        {
            removeBackground();
            updateColor = false;

            contButton.Visibility = Visibility.Hidden;
            toGeneral.Visibility = Visibility.Hidden;
            afterRoundsTitle.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            modifiedInitialStateGrid.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            buttonsTitle.Visibility = Visibility.Hidden;
            buttonsPanel.Visibility = Visibility.Hidden;
            inputDataButton.Visibility = Visibility.Hidden;
            showChangeTitle.Visibility = Visibility.Hidden;
            radioButtons.Visibility = Visibility.Hidden;

            ChangesMadeButton.Visibility = Visibility.Visible;

            if (mode == 0)
            {
                clearColors();
                clearKeyColors();
                initMsg.Text = Properties.Resources.InitialMessageHex;
                initKey.Text = Properties.Resources.InitialKeyHex;
                afterRoundsGrid.Visibility = Visibility.Hidden;
                bitRepresentationGrid.Visibility = Visibility.Hidden;
                curvedLinesCanvas.Visibility = Visibility.Hidden;
                OrigInitialStateGrid.Visibility = Visibility.Visible;
                generalViewAES.Visibility = Visibility.Hidden;
                extraordinaryOccurAes.Visibility = Visibility.Hidden;

                if (aesCheckBox.IsChecked == false)
                {
                    instructionsTxtBlock2.Visibility = Visibility.Hidden;
                    doneButton.Visibility = Visibility.Hidden;
                    clearButton.Visibility = Visibility.Hidden;

                }
            }
            else
            {
                bitGridDES.Visibility = Visibility.Hidden;
                generalViewDES.Visibility = Visibility.Hidden;
                extraordinaryOccur.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Hidden;
            }

            comparisonPane();

            if ((bool)aesCheckBox.IsChecked || (bool)desCheckBox.IsChecked)
            {
                instructionsTxtBlock2.Visibility = Visibility.Visible;
                doneButton.Visibility = Visibility.Visible;
                clearButton.Visibility = Visibility.Visible;

                if (mode == 0)
                    changeMsgAes.Visibility = Visibility.Hidden;
                if (mode == 1)
                    changeMsgDes.Visibility = Visibility.Hidden;
            }


        }



        public void emptyInformation()
        {
            stats1.Text = string.Empty;
            stats2.Text = string.Empty;
            stats3.Text = string.Empty;
            stats4.Text = string.Empty;
        }

        private void afterRound0Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[0], statesB[0]);
            int occurrence;

            clearElements();
            changeRoundNr(0);
            showElements();
            removeColors();
            removeBackground();

            afterRound0Button.Background = Brushes.Coral;
            encryptionProgress(0);
            slide = 3;

            if (mode == 0)
            {
                roundNumber = 1 + shift * 2 * keysize;
                action = 1;


                int nrDiffBits = nrOfBitsFlipped(states[0], statesB[0]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[0], statesB[0]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);



            }
            else
            {
                roundDES = 0;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);

                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);
            }


            //afterInitialRoundGrid.Visibility = Visibility.Visible;
        }



        private void afterRound1Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[4], statesB[4]);
            int occurrence;

            clearElements();
            changeRoundNr(1);
            showElements();
            removeColors();
            removeBackground();

            afterRound1Button.Background = Brushes.Coral;
            encryptionProgress(1);
            slide = 4;

            if (mode == 0)
            {

                roundNumber = 2 + shift * 2 * keysize;
                action = 1;


                int nrDiffBits = nrOfBitsFlipped(states[4], statesB[4]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[4], statesB[4]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);



            }
            else
            {
                roundDES = 1;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);

                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);
            }
        }

        private void afterRound2Button_Click(object sender, RoutedEventArgs e)
        {

            var strings = binaryStrings(states[8], statesB[8]);
            int occurrence;

            clearElements();
            changeRoundNr(2);
            showElements();
            removeColors();
            removeBackground();

            afterRound2Button.Background = Brushes.Coral;
            encryptionProgress(2);
            slide = 5;

            if (mode == 0)
            {

                roundNumber = 3 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[8], statesB[8]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[8], statesB[8]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
            else
            {
                roundDES = 2;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);

                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound3Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[12], statesB[12]);
            int occurrence;

            clearElements();
            changeRoundNr(3);
            showElements();
            removeColors();
            removeBackground();

            afterRound3Button.Background = Brushes.Coral;
            encryptionProgress(3);
            slide = 6;

            if (mode == 0)
            {

                roundNumber = 4 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[12], statesB[12]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[12], statesB[12]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
            else
            {
                roundDES = 3;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound4Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[12], statesB[12]);
            int occurrence;

            clearElements();
            changeRoundNr(4);
            showElements();
            removeColors();
            removeBackground();

            afterRound4Button.Background = Brushes.Coral;
            encryptionProgress(4);
            slide = 7;

            if (mode == 0)
            {
                roundNumber = 5 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[16], statesB[16]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[16], statesB[16]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                roundDES = 4;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound5Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[20], statesB[20]);
            int occurrence;

            clearElements();
            changeRoundNr(5);
            showElements();
            removeColors();
            removeBackground();

            afterRound5Button.Background = Brushes.Coral;
            encryptionProgress(5);
            slide = 8;

            if (mode == 0)
            {
                roundNumber = 6 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[20], statesB[20]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[20], statesB[20]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                roundDES = 5;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound6Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[24], statesB[24]);
            int occurrence;

            clearElements();
            changeRoundNr(6);
            showElements();
            removeColors();
            removeBackground();

            afterRound6Button.Background = Brushes.Coral;
            encryptionProgress(6);
            slide = 9;

            if (mode == 0)
            {
                roundNumber = 7 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[24], statesB[24]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[24], statesB[24]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                roundDES = 6;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }

        private void afterRound7Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[28], statesB[28]);
            int occurrence;

            clearElements();
            changeRoundNr(7);
            showElements();
            removeColors();
            removeBackground();

            afterRound7Button.Background = Brushes.Coral;
            encryptionProgress(7);
            slide = 10;

            if (mode == 0)
            {

                roundNumber = 8 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[28], statesB[28]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[28], statesB[28]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);



            }
            else
            {
                roundDES = 7;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound8Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[32], statesB[32]);
            int occurrence;

            clearElements();
            changeRoundNr(8);
            showElements();
            removeColors();
            removeBackground();

            afterRound8Button.Background = Brushes.Coral;
            encryptionProgress(8);
            slide = 11;

            if (mode == 0)
            {
                roundNumber = 9 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[32], statesB[32]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[32], statesB[32]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                roundDES = 8;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound9Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[36], statesB[36]);
            int occurrence;

            clearElements();
            changeRoundNr(9);
            showElements();
            removeColors();
            removeBackground();

            afterRound9Button.Background = Brushes.Coral;
            encryptionProgress(9);
            slide = 12;

            if (mode == 0)
            {
                roundNumber = 10 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[36], statesB[36]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[36], statesB[36]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
            else
            {
                roundDES = 9;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }

        //after 10
        private void afterRound10Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            int occurrence;

            clearElements();
            changeRoundNr(10);
            showElements();
            removeColors();
            removeBackground();

            afterRound10Button.Background = Brushes.Coral;
            encryptionProgress(10);
            slide = 13;

            if (mode == 0)
            {
                roundNumber = 11 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits;
                double angle_1;
                double angle_2;
                int lengthIdentSequence;
                int lengthFlippedSequence;
                emptyInformation();

                if (keysize == 0)
                {
                    toGeneral.Visibility = Visibility.Visible;
                    strings = binaryStrings(states[39], statesB[39]);
                    nrDiffBits = nrOfBitsFlipped(states[39], statesB[39]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {

                       final();
                       displayBinaryValues(states[39], statesB[39]);
                       showBitSequence(strings);
                       occurrence = countOccurrence(differentBits);
                       showOccurence(occurrence);
                       lengthIdentSequence = longestIdenticalSequence(differentBits);
                       lengthFlippedSequence = longestFlippedSequence(differentBits);
                       showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                       setColors();
                       setAngles(angle_1, angle_2);
                       setToolTips();



                   }, null);
                }
                else
                {
                    strings = binaryStrings(states[40], statesB[40]);
                    nrDiffBits = nrOfBitsFlipped(states[40], statesB[40]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {

                       printIntermediateStates(states, statesB);
                       displayBinaryValues(states[40], statesB[40]);
                       showBitSequence(strings);
                       occurrence = countOccurrence(differentBits);
                       showOccurence(occurrence);
                       lengthIdentSequence = longestIdenticalSequence(differentBits);
                       lengthFlippedSequence = longestFlippedSequence(differentBits);
                       showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                       setColors();
                       setAngles(angle_1, angle_2);
                       setToolTips();



                   }, null);
                }

            }
            else
            {
                roundDES = 10;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }

        private void afterRound11Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            clearElements();
            changeRoundNr(11);
            showElements();
            removeColors();
            removeBackground();

            afterRound11Button.Background = Brushes.Coral;
            encryptionProgress(11);
            slide = 14;

            if (mode == 0)
            {
                var strings = binaryStrings(states[44], statesB[44]);
                roundNumber = 12 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[44], statesB[44]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[44], statesB[44]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                var strings = binaryStrings(states[4], statesB[4]);
                roundDES = 11;
                encryptionProgress(roundDES);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }

        private void afterRound12Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            int occurrence;

            clearElements();
            changeRoundNr(12);
            showElements();
            removeColors();
            removeBackground();

            afterRound12Button.Background = Brushes.Coral;
            encryptionProgress(12);
            slide = 15;

            if (mode == 0)
            {
                roundNumber = 13 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits;
                double angle_1;
                double angle_2;
                int lengthIdentSequence;
                int lengthFlippedSequence;
                emptyInformation();


                if (keysize == 1)
                {
                    toGeneral.Visibility = Visibility.Visible;
                    strings = binaryStrings(states[47], statesB[47]);
                    nrDiffBits = nrOfBitsFlipped(states[47], statesB[47]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {

                       final();
                       displayBinaryValues(states[47], statesB[47]);
                       showBitSequence(strings);
                       occurrence = countOccurrence(differentBits);
                       showOccurence(occurrence);
                       lengthIdentSequence = longestIdenticalSequence(differentBits);
                       lengthFlippedSequence = longestFlippedSequence(differentBits);
                       showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                       setColors();
                       setAngles(angle_1, angle_2);
                       setToolTips();



                   }, null);
                }
                else
                {
                    strings = binaryStrings(states[48], statesB[48]);
                    nrDiffBits = nrOfBitsFlipped(states[48], statesB[48]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {

                       printIntermediateStates(states, statesB);
                       displayBinaryValues(states[48], statesB[48]);
                       showBitSequence(strings);
                       occurrence = countOccurrence(differentBits);
                       showOccurence(occurrence);
                       lengthIdentSequence = longestIdenticalSequence(differentBits);
                       lengthFlippedSequence = longestFlippedSequence(differentBits);
                       showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                       setColors();
                       setAngles(angle_1, angle_2);
                       setToolTips();



                   }, null);
                }



            }
            else
            {
                roundDES = 12;
                encryptionProgress(roundDES);
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }



        private void afterRound13Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            clearElements();
            changeRoundNr(13);
            showElements();
            removeColors();
            removeBackground();

            afterRound13Button.Background = Brushes.Coral;
            encryptionProgress(13);
            slide = 16;

            if (mode == 0)
            {
                var strings = binaryStrings(states[52], statesB[52]);


                roundNumber = 14 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[52], statesB[52]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   printIntermediateStates(states, statesB);
                   displayBinaryValues(states[52], statesB[52]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {
                roundDES = 13;
                encryptionProgress(roundDES);
                var strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {

                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);

            }
        }

        private void afterRound14Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            clearElements();
            changeRoundNr(14);
            showElements();
            removeColors();
            removeBackground();

            afterRound14Button.Background = Brushes.Coral;
            encryptionProgress(14);
            slide = 17;

            if (mode == 0)
            {
                var strings = binaryStrings(states[55], statesB[55]);

                action = 1;
                toGeneral.Visibility = Visibility.Visible;
                int nrDiffBits = nrOfBitsFlipped(states[55], statesB[55]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   final();
                   displayBinaryValues(states[55], statesB[55]);
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequence = longestIdenticalSequence(differentBits);
                   lengthFlippedSequence = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();

               }, null);


            }
            else
            {

                roundDES = 14;
                encryptionProgress(roundDES);
                var strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
               {
                   displayBinaryValuesDES();
                   showBitSequence(strings);
                   occurrence = countOccurrence(differentBits);
                   showOccurence(occurrence);
                   lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
                   showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   setColors();
                   setAngles(angle_1, angle_2);
                   setToolTips();
               }, null);

            }
        }

        private void afterRound15Button_Click(object sender, RoutedEventArgs e)
        {
            clearElements();
            changeRoundNr(15);
            showElements();
            removeColors();
            removeBackground();

            afterRound15Button.Background = Brushes.Coral;
            slide = 18;

            roundDES = 15;
            encryptionProgress(roundDES);
            var strings = binaryStrings(states[4], statesB[4]);
            toStringArray(roundDES);

            int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = calcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            int lengthFlippedSequenceDes;
            int occurrence;
            avgNrDiffBit = avgNrperByte(nrDiffBits);
            emptyInformation();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               displayBinaryValuesDES();
               showBitSequence(strings);
               occurrence = countOccurrence(differentBits);
               showOccurence(occurrence);
               lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
               lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
               showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
               setColors();
               setAngles(angle_1, angle_2);
               setToolTips();
           }, null);
        }

        private void afterRound16Button_Click(object sender, RoutedEventArgs e)
        {

            clearElements();
            changeRoundNr(16);
            showElements();
            removeColors();
            removeBackground();

            afterRound16Button.Background = Brushes.Coral;
            slide = 19;

            toGeneral.Visibility = Visibility.Visible;

            roundDES = 16;
            encryptionProgress(roundDES);
            var strings = binaryStrings(states[4], statesB[4]);
            toStringArray(roundDES);

            int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = calcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            int lengthFlippedSequenceDes;
            int occurrence;
            avgNrDiffBit = avgNrperByte(nrDiffBits);
            emptyInformation();


            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               displayBinaryValuesDES();
               showBitSequence(strings);
               occurrence = countOccurrence(differentBits);
               showOccurence(occurrence);
               lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
               lengthFlippedSequenceDes = longestFlippedSequence(differentBits);
               showStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
               setColors();
               setAngles(angle_1, angle_2);
               setToolTips();
           }, null);
        }

        public void final()
        {

            List<TextBlock> tmp = createTxtBlockList(3);
            List<TextBlock> tmpB = createTxtBlockList(4);

            byte[] state = { 0 };
            byte[] stateB = { 0 };

            switch (keysize)
            {
                case 0:
                    state = arrangeText(states[39]);
                    stateB = arrangeText(statesB[39]);
                    break;
                case 1:
                    state = arrangeText(states[47]);
                    stateB = arrangeText(statesB[47]);
                    break;
                case 2:
                    state = arrangeText(states[55]);
                    stateB = arrangeText(statesB[55]);
                    break;
                default:
                    break;
            }

            int i = 0;
            int j = 0;


            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }

        }

        private void radioButton1Checked(object sender, RoutedEventArgs e)
        {


            string strA = binaryAsString(textA).Replace(" ", "");
            string strB = binaryAsString(textB).Replace(" ", "");

            string firstHalf = strA.Substring(0, 32);
            string secondHalf = strA.Substring(32, 32);
            string firstHalfB = strB.Substring(0, 32);
            string secondHalfB = strB.Substring(32, 32);


            origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
            modTextDES.Text = string.Format("{0}{1}{2}", firstHalfB, Environment.NewLine, secondHalfB);


            string keyStrA = binaryAsString(keyA).Replace(" ", "");
            string keyStrB = binaryAsString(key).Replace(" ", "");

            string firstKeyHalf = keyStrA.Substring(0, 32);
            string secondKeyHalf = keyStrA.Substring(32, 32);
            string firstKeyHalfB = keyStrB.Substring(0, 32);
            string secondKeyHalfB = keyStrB.Substring(32, 32);

            origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalfB, Environment.NewLine, secondKeyHalfB);

            coloringText();
            coloringKey();

        }



        private void radioButton2Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                var encoding = Encoding.GetEncoding(437);

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString();
                    ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString();

                    i++;
                    j++;
                }

                if (keysize == 0)
                {
                    int k = 1;
                    int l = 17;
                    int m = 49;
                    while (k <= 16 && l <= 32 && m <= 64)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString();
                        k++;
                        l++;
                        m++;
                    }
                }
                else if (keysize == 1)
                {
                    int k = 1;


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("modKey192_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
                else
                {
                    int k = 1;


                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("modKey256_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
            }
            else if (mode == 1)
            {

                string strA = decimalAsString(textA);
                string strB = decimalAsString(textB);

                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = decimalAsString(keyA);
                string keyStrB = decimalAsString(key);

                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;

                coloringText();
                coloringKey();

            }
            else
            {
                if (modifiedMsg.Text != "")
                {
                    originalMsg.Text = decimalAsString(unchangedCipher);
                    modifiedMsg.Text = decimalAsString(changedCipher);
                }

                else
                    originalMsg.Text = decimalAsString(unchangedCipher);


            }
        }


        private void radioButton3Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString("X2");
                    ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString("X2");
                    i++;
                    j++;
                }

                if (keysize == 0)
                {
                    int k = 1;
                    int l = 17;
                    int m = 49;
                    while (k <= 16 && l <= 32 && m <= 64)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString("X2");
                        k++;
                        l++;
                        m++;
                    }
                }
                else if (keysize == 1)
                {
                    int k = 1;


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("modKey192_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }
                else
                {
                    int k = 1;


                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("modKey256_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }

            }
            else if (mode == 1)
            {

                string strA = hexaAsString(textA);
                string strB = hexaAsString(textB);

                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = hexaAsString(keyA);
                string keyStrB = hexaAsString(key);

                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;

                coloringText();
                coloringKey();

            }
            else
            {

                if (modifiedMsg.Text != "")
                {
                    originalMsg.Text = hexaAsString(unchangedCipher);
                    modifiedMsg.Text = hexaAsString(changedCipher);
                }

                else

                    originalMsg.Text = hexaAsString(unchangedCipher);
            }

        }


        private void radioText_Checked(object sender, RoutedEventArgs e)
        {

            string strA = Encoding.UTF8.GetString(unchangedCipher);

            if (modifiedMsg.Text != "")
            {
                string strB = Encoding.UTF8.GetString(changedCipher);
                originalMsg.Text = strA;
                modifiedMsg.Text = strB;
            }

            else
                originalMsg.Text = strA;
        }


        private void clearTextEffect()
        {
            if (mode == 1)
            {
                modTextDES.TextEffects.Clear();
                origTextDES.TextEffects.Clear();
            }

            if (mode == 0)
            {
                switch (keysize)
                {

                    case 0:

                        IEnumerable<TextBlock> textChilds = overviewAES.Children.OfType<TextBlock>();

                        foreach (TextBlock tb in textChilds)
                            tb.TextEffects.Clear();

                        break;
                    case 1:

                        IEnumerable<TextBlock> textChilds192 = overviewAES192.Children.OfType<TextBlock>();

                        foreach (TextBlock tb in textChilds192)
                            tb.TextEffects.Clear();

                        break;
                    case 2:

                        IEnumerable<TextBlock> textChilds256 = overviewAES256.Children.OfType<TextBlock>();

                        foreach (TextBlock tb in textChilds256)
                            tb.TextEffects.Clear();

                        break;
                    default:
                        break;
                }




            }
        }

        private void clearKeyEffect()
        {
            origKeyDES.TextEffects.Clear();
            modKeyDES.TextEffects.Clear();
        }



        public void instructions()
        {
            StartCanvas.Visibility = Visibility.Hidden;
            slide = 0;
            avalancheVisualization.ProgressChanged(0, 1);

            if (mode == 0 || mode == 1)
                InstructionsPrep.Visibility = Visibility.Visible;
            else
                InstructionsUnprep.Visibility = Visibility.Visible;
        }

        public void comparisonPane()
        {
            slide = 1;
            StartCanvas.Visibility = Visibility.Hidden;
            backButton.Visibility = Visibility.Visible;



            switch (mode)
            {
                //AES
                case 0:
                    OrigInitialStateGrid.Visibility = Visibility.Visible;
                    aesCheckBox.Visibility = Visibility.Visible;
                    inputInBits.Visibility = Visibility.Visible;

                    if (aesCheckBox.IsChecked == true)
                    {
                        instructionsTxtBlock2.Visibility = Visibility.Visible;
                        doneButton.Visibility = Visibility.Visible;
                        clearButton.Visibility = Visibility.Visible;
                    }

                    if (aesCheckBox.IsChecked == false)
                        changeMsgAes.Visibility = Visibility.Visible;


                    if (keysize == 1)
                    {
                        originalKeyGrid192.Visibility = Visibility.Visible;
                        BitKeyGrid192.Visibility = Visibility.Visible;
                    }
                    else if (keysize == 2)
                    {
                        originalKeyGrid256.Visibility = Visibility.Visible;
                        BitKeyGrid256.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        originalKeyGrid.Visibility = Visibility.Visible;
                        BitKeyGrid.Visibility = Visibility.Visible;
                    }


                    initStateTitle.Visibility = Visibility.Visible;

                    changeTitle();

                    int a = 0;
                    int b = 128;
                    while (a < 64 && b < 192)
                    {

                        ((TextBlock)this.FindName("txt" + b)).Foreground = Brushes.Black;
                        a++;
                        b++;
                    }

                    break;
                case 1:

                    if (desCheckBox.IsChecked == true)
                    {
                        instructionsTxtBlock2.Visibility = Visibility.Visible;
                        doneButton.Visibility = Visibility.Visible;
                        clearButton.Visibility = Visibility.Visible;
                    }

                    if (desCheckBox.IsChecked == false)
                        changeMsgDes.Visibility = Visibility.Visible;




                    inputGridDES.Visibility = Visibility.Visible;



                    break;
                case 2:
                case 3:
                case 4:

                    othersGrid.Visibility = Visibility.Visible;
                    changeTitle();

                    if (mode == 3)
                        radioText.Visibility = Visibility.Visible;

                    break;
                default:
                    break;

            }

        }


        public void removeElements()
        {

            skip.IsChecked = false;


            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            backButton.Visibility = Visibility.Hidden;

            if (mode == 0 || mode == 1)
            {

                radioDecimal.IsChecked = false;
                radioHexa.IsChecked = false;
                aesCheckBox.IsChecked = false;
                desCheckBox.IsChecked = false;
                newChanges = false;
                updateColor = false;
                ChangesMadeButton.IsEnabled = false;

                contButton.Visibility = Visibility.Hidden;
                toGeneral.Visibility = Visibility.Hidden;
                InstructionsPrep.Visibility = Visibility.Hidden;
                bitRepresentationGrid.Visibility = Visibility.Hidden;
                OrigInitialStateGrid.Visibility = Visibility.Hidden;
                afterRoundsGrid.Visibility = Visibility.Hidden;
                initStateTitle.Visibility = Visibility.Hidden;
                showChangeTitle.Visibility = Visibility.Hidden;
                modifiedInitialStateGrid.Visibility = Visibility.Hidden;
                buttonsPanel.Visibility = Visibility.Hidden;
                inputDataButton.Visibility = Visibility.Hidden;
                radioButtons.Visibility = Visibility.Hidden;
                buttonsTitle.Visibility = Visibility.Hidden;
                curvedLinesCanvas.Visibility = Visibility.Hidden;
                afterRoundsTitle.Visibility = Visibility.Hidden;
                afterRoundsSubtitle.Visibility = Visibility.Hidden;
                inputInBits.Visibility = Visibility.Hidden;
                instructionsTxtBlock2.Visibility = Visibility.Hidden;
                doneButton.Visibility = Visibility.Hidden;
                clearButton.Visibility = Visibility.Hidden;
                extraordinaryOccurAes.Visibility = Visibility.Hidden;
                aesCheckBox.Visibility = Visibility.Hidden;
                changeMsgAes.Visibility = Visibility.Hidden;
                generalViewAES.Visibility = Visibility.Hidden;
                inputGridDES.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Hidden;
                bitGridDES.Visibility = Visibility.Hidden;
                generalViewDES.Visibility = Visibility.Hidden;
                extraordinaryOccur.Visibility = Visibility.Hidden;
                ChangesMadeButton.Visibility = Visibility.Hidden;

                afterRound11Button.Visibility = Visibility.Collapsed;
                afterRound12Button.Visibility = Visibility.Collapsed;
                afterRound13Button.Visibility = Visibility.Collapsed;
                afterRound14Button.Visibility = Visibility.Collapsed;
                afterRound15Button.Visibility = Visibility.Collapsed;
                afterRound16Button.Visibility = Visibility.Collapsed;
                originalKeyGrid.Visibility = Visibility.Collapsed;
                originalKeyGrid192.Visibility = Visibility.Collapsed;
                originalKeyGrid256.Visibility = Visibility.Collapsed;
                BitKeyGrid.Visibility = Visibility.Collapsed;
                BitKeyGrid192.Visibility = Visibility.Collapsed;
                BitKeyGrid256.Visibility = Visibility.Collapsed;
                modifiedKeyGrid.Visibility = Visibility.Collapsed;
                modifiedKeyGrid192.Visibility = Visibility.Collapsed;
                modifiedKeyGrid256.Visibility = Visibility.Collapsed;
                overviewAES.Visibility = Visibility.Collapsed;
                overviewAES192.Visibility = Visibility.Collapsed;
                overviewAES256.Visibility = Visibility.Collapsed;

                changeMsgDes.Visibility = Visibility.Visible;

                initMsg.Text = Properties.Resources.InitialMessageHex;
                initKey.Text = Properties.Resources.InitialKeyHex;
                genOverviewAES.Text = Properties.Resources.OverviewAES128;

                clearColors();
                clearKeyColors();
                removeBackground();
                readjustStats();

                bitRepresentationSV.ScrollToHorizontalOffset(0.0);
                canModify = false;
                canModifyDES = false;
                slide = 0;

                if (canStop)
                {
                    updateDataColor();

                    List<TextBlock> tmp = createTxtBlockList(6);

                    foreach (TextBlock txtB in tmp)
                        txtB.Foreground = Brushes.Black;


                    int k = 33;
                    int l = 49;
                    int i = 1;


                    while (k <= 48 && l <= 64)
                    {

                        ((TextBlock)this.FindName("initStateTxtBlock" + k)).Text = string.Empty;

                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = string.Empty;
                        i++;
                        k++;
                        l++;
                    }

                    int j = 1;

                    while (j <= 24)
                    {
                        ((TextBlock)this.FindName("modKey192_" + j)).Text = string.Empty;
                        j++;

                    }

                    int m = 1;

                    while (m <= 32)
                    {
                        ((TextBlock)this.FindName("modKey256_" + m)).Text = string.Empty;
                        m++;

                    }

                    List<TextBlock> tmpDES = createTxtBlockList(7);

                    foreach (TextBlock txtB in tmpDES)
                        txtB.Foreground = Brushes.Black;

                }



            }

            else
            {
                TB1.Text = string.Empty;
                TB2.Text = string.Empty;
                TB3.Text = string.Empty;
                modifiedMsg.Text = string.Empty;
                originalMsg.Text = string.Empty;

                InstructionsUnprep.Visibility = Visibility.Hidden;
                Cbclass1.Visibility = Visibility.Hidden;
                Cbclass2.Visibility = Visibility.Hidden;
                othersGrid.Visibility = Visibility.Hidden;

                radioText.Visibility = Visibility.Collapsed;
                canModifyOthers = false;
            }

            // mode = 0;
            canStop = false;

            StartCanvas.Visibility = Visibility.Visible;
        }

        public void adjustStats()
        {
            if (mode == 1)
            {
                Grid.SetRow(bitsData, 1);
                Grid.SetColumn(bitsData, 0);
                Grid.SetColumnSpan(bitsData, 2);
                bitsData.HorizontalAlignment = HorizontalAlignment.Center;

                Thickness margin = bitsData.Margin;
                bitsData.Margin = new Thickness(10, 20, 0, 75);

                Grid.SetColumn(flippedBitsPiece, 1);
                Grid.SetColumnSpan(flippedBitsPiece, 2);
                Grid.SetColumn(unflippedBitsPiece, 1);
                Grid.SetColumnSpan(unflippedBitsPiece, 2);

                flippedBitsPiece.Margin = new Thickness(80, 15, 80, 20);
                unflippedBitsPiece.Margin = new Thickness(80, 15, 80, 20);

                Cb1.HorizontalAlignment = HorizontalAlignment.Right;
                Cb1.VerticalAlignment = VerticalAlignment.Center;
                Cb2.HorizontalAlignment = HorizontalAlignment.Right;
                Cb2.VerticalAlignment = VerticalAlignment.Center;

                Cb1.Margin = new Thickness(5, 0, 10, 100);
                Cb2.Margin = new Thickness(5, 0, 10, 60);


            }

            else
            {


                Grid.SetColumn(bitsData, 0);
                Grid.SetColumnSpan(bitsData, 2);
                bitsData.HorizontalAlignment = HorizontalAlignment.Center;
                bitsData.VerticalAlignment = VerticalAlignment.Center;


                bitsData.Margin = new Thickness(0, 60, 0, 75);

                stats4Bullet.Visibility = Visibility.Collapsed;

                Grid.SetColumn(flippedBitsPiece, 1);
                Grid.SetColumn(unflippedBitsPiece, 1);
                Grid.SetColumnSpan(flippedBitsPiece, 2);
                Grid.SetColumnSpan(unflippedBitsPiece, 2);

                flippedBitsPiece.VerticalAlignment = VerticalAlignment.Bottom;
                unflippedBitsPiece.VerticalAlignment = VerticalAlignment.Bottom;
                flippedBitsPiece.HorizontalAlignment = HorizontalAlignment.Stretch;
                unflippedBitsPiece.HorizontalAlignment = HorizontalAlignment.Stretch;

                flippedBitsPiece.Margin = new Thickness(140, -14, 80, 0);
                unflippedBitsPiece.Margin = new Thickness(140, -14, 80, 0);

                Cb1.VerticalAlignment = VerticalAlignment.Bottom;
                Cb2.VerticalAlignment = VerticalAlignment.Bottom;
                Cb1.HorizontalAlignment = HorizontalAlignment.Center;
                Cb2.HorizontalAlignment = HorizontalAlignment.Center;
                Cb1.Margin = new Thickness(45, 0, 0, 100);
                Cb2.Margin = new Thickness(45, 0, 0, 80);

            }
        }

        public void readjustStats()
        {



            Grid.SetColumnSpan(bitsData, 3);
            Grid.SetColumn(flippedBitsPiece, 2);
            Grid.SetColumn(unflippedBitsPiece, 2);
            Grid.SetColumnSpan(flippedBitsPiece, 1);
            Grid.SetColumnSpan(unflippedBitsPiece, 1);

            bitsData.HorizontalAlignment = HorizontalAlignment.Center;
            bitsData.VerticalAlignment = VerticalAlignment.Stretch;


            bitsData.Margin = new Thickness(40, 45, 0, 100);

            stats4Bullet.Visibility = Visibility.Visible;

            flippedBitsPiece.VerticalAlignment = VerticalAlignment.Top;
            unflippedBitsPiece.VerticalAlignment = VerticalAlignment.Top;
            flippedBitsPiece.HorizontalAlignment = HorizontalAlignment.Right;
            unflippedBitsPiece.HorizontalAlignment = HorizontalAlignment.Right;

            flippedBitsPiece.Margin = new Thickness(10, 15, -15, 15);
            unflippedBitsPiece.Margin = new Thickness(10, 15, -15, 15);

            Cb1.VerticalAlignment = VerticalAlignment.Bottom;
            Cb2.VerticalAlignment = VerticalAlignment.Bottom;
            Cb1.HorizontalAlignment = HorizontalAlignment.Right;
            Cb2.HorizontalAlignment = HorizontalAlignment.Right;

            Cb1.Margin = new Thickness(0, 0, 35, 80);
            Cb2.Margin = new Thickness(0, 0, 35, 60);

        }

        public void comparison()
        {
            emptyInformation();

            adjustStats();
            bitsData.Visibility = Visibility.Visible;
            avalancheVisualization.ProgressChanged(1, 1);

            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;

            if (mode != 3)
            {
                Cb1.Visibility = Visibility.Visible;
                Cb2.Visibility = Visibility.Visible;
                var strings = binaryStrings(unchangedCipher, changedCipher);
                int bitsFlipped = nrOfBitsFlipped(unchangedCipher, changedCipher);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avalanche = calcAvalancheEffect(bitsFlipped, strings);
                double angle_1 = flippedBitsPiece.calculateAngle(bitsFlipped, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - bitsFlipped, strings);
                showBitSequence(strings);
                lengthIdentSequence = longestIdenticalSequence(differentBits);
                lengthFlippedSequence = longestFlippedSequence(differentBits);
                showStatistics(bitsFlipped, lengthIdentSequence, lengthFlippedSequence, strings);
                setColors();
                setAngles(angle_1, angle_2);
                setToolTips();
            }
            else
            {

                Cbclass1.Visibility = Visibility.Visible;
                Cbclass2.Visibility = Visibility.Visible;

                var strings = binaryStrings(unchangedCipher, changedCipher);
                int nrBytesFlipped = bytesFlipped();
                avalanche = avalancheEffectBytes(nrBytesFlipped);


                double angle_1 = flippedBitsPiece.calculateAngleClassic(nrBytesFlipped, unchangedCipher);
                double angle_2 = unflippedBitsPiece.calculateAngleClassic(unchangedCipher.Length - nrBytesFlipped,
                    unchangedCipher);
                showBitSequence(strings);
                int LIBS = longestIdentSequenceBytes();
                int LFBS = longestFlippedSequenceBytes();
                classicStats(nrBytesFlipped, LIBS, LFBS);
                setColors();
                setAngles(angle_1, angle_2);
                setToolTips();
            }

        }

        public void classicStats(int bytesFlipped, int longestLength, int longestflipped)
        {
            stats1.Inlines.Add(new Run(" " + bytesFlipped.ToString())
            {
                Foreground = Brushes.Red,
                FontWeight = FontWeights.DemiBold
            });

            if (bytesFlipped > 1 || bytesFlipped == 0)
                stats1.Inlines.Add(
                    new Run(string.Format(Properties.Resources.StatsClassicBullet1_Plural, changedCipher.Length,
                        avalanche)));
            else
                stats1.Inlines.Add(
                    new Run(string.Format(Properties.Resources.StatsClassicBullet1, changedCipher.Length, avalanche)));

            stats2.Inlines.Add(
                new Run(string.Format(Properties.Resources.StatsClassicBullet2, longestLength.ToString(),
                    sequencePosition)));
            stats3.Inlines.Add(
                new Run(string.Format(Properties.Resources.StatsClassicBullet3, longestflipped.ToString(),
                    flippedSeqPosition)));

        }


        public int bytesFlipped()
        {
            int count = 0;

            for (int i = 0; i < changedCipher.Length; i++)
            {
                if (changedCipher[i] != unchangedCipher[i])
                    count++;

            }



            return count;
        }



        public double avalancheEffectBytes(int bytesFlipped)
        {

            double avalancheEffect = ((double)bytesFlipped / unchangedCipher.Length) * 100;
            double roundUp = Math.Round(avalancheEffect, 1, MidpointRounding.AwayFromZero);

            return roundUp;
        }

        public int longestIdentSequenceBytes()
        {
            int lastCount = 0;
            int longestCount = 0;
            int i = 0;

            int offset = 0;

            while (i < unchangedCipher.Length)
            {
                if (unchangedCipher[i] == changedCipher[i])
                {
                    lastCount++;

                    if (lastCount > longestCount)
                    {
                        longestCount = lastCount;
                        offset = i - lastCount + 1;
                    }
                }
                else
                {
                    lastCount = 0;
                }

                i++;
            }

            sequencePosition = offset;

            return longestCount;

        }

        public int longestFlippedSequenceBytes()
        {
            int lastCount = 0;
            int longestCount = 0;
            int i = 0;

            int offset = 0;

            while (i < unchangedCipher.Length)
            {
                if (unchangedCipher[i] != changedCipher[i])
                {
                    lastCount++;

                    if (lastCount > longestCount)
                    {
                        longestCount = lastCount;
                        offset = i - lastCount + 1;
                    }
                }
                else
                {
                    lastCount = 0;
                }

                i++;
            }

            flippedSeqPosition = offset;

            return longestCount;

        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sync = (sender == bar3) ? bar2 : bar3;
            ScrollViewer sync2 = (sender == bar3) ? bar1 : bar3;

            sync.ScrollToVerticalOffset(e.VerticalOffset);
            sync.ScrollToHorizontalOffset(e.HorizontalOffset);

            sync2.ScrollToVerticalOffset(e.VerticalOffset);
            sync2.ScrollToHorizontalOffset(e.HorizontalOffset);
        }



        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock txtBlock = sender as TextBlock;


            if ((bool)desCheckBox.IsChecked || (bool)aesCheckBox.IsChecked)
            {

                ChangesMadeButton.IsEnabled = false;
                ChangesMadeButton.Opacity = 0.3;

                if (txtBlock.Text == "0")
                {
                    txtBlock.Text = "1";

                    if (txtBlock.Foreground != Brushes.Red)
                        txtBlock.Foreground = Brushes.Red;
                    else
                        txtBlock.Foreground = Brushes.Black;
                }
                else
                {
                    txtBlock.Text = "0";

                    if (txtBlock.Foreground != Brushes.Red)
                        txtBlock.Foreground = Brushes.Red;
                    else
                        txtBlock.Foreground = Brushes.Black;
                }

            }

        }



        private void onVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (inputInBits.IsVisible)
            {

                arrow1.Visibility = Visibility.Visible;

                if (keysize == 2)
                    arrow3.Visibility = Visibility.Visible;
                else
                    arrow2.Visibility = Visibility.Visible;

            }
            else
            {
                arrow1.Visibility = Visibility.Hidden;

                if (keysize == 2)
                    arrow3.Visibility = Visibility.Hidden;
                else
                    arrow2.Visibility = Visibility.Hidden;

                //  buttonNextClickedEvent.Set();
            }

            if (modifiedInitialStateGrid.IsVisible || inputInBits.IsVisible)
            {
                encryptionProgress(-1);
                canModify = true;
            }
            if (afterRoundsGrid.IsVisible)
                canModify = false;

        }

        private void modifyOthers(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (othersGrid.IsVisible)
            {
                encryptionProgress(0);
                canModifyOthers = true;
            }

            if (InstructionsUnprep.IsVisible)
                canModifyOthers = false;
        }

        private void modify(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!modificationGridDES.IsVisible || !inputGridDES.IsVisible)
            {
                canModifyDES = false;


            }
            if (modificationGridDES.IsVisible || inputGridDES.IsVisible)
            {
                encryptionProgress(-1);
                canModifyDES = true;

            }
        }

        private void onTitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            /*  if (initStateTitle.IsVisible || modificationGridDES.IsVisible)
                  inputDataButton.IsEnabled = false;
              else
                  inputDataButton.IsEnabled = true;*/


            if (buttonsPanel.IsVisible && mode == 1)
            {
                radioBinaryDes.IsChecked = true;
                instructionsTxtBlock2.Visibility = Visibility.Hidden;
                clearButton.Visibility = Visibility.Hidden;
                doneButton.Visibility = Visibility.Hidden;
            }

            if (buttonsPanel.IsVisible && mode == 0)
            {
                radioHexa.IsChecked = true;
                changeMsgAes.Visibility = Visibility.Hidden;
                instructionsTxtBlock2.Visibility = Visibility.Hidden;
                clearButton.Visibility = Visibility.Hidden;
                doneButton.Visibility = Visibility.Hidden;
            }



        }


        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {


            if (mode == 1)
                changeMsgDes.Visibility = Visibility.Hidden;
            else
                changeMsgAes.Visibility = Visibility.Hidden;

            doneButton.Visibility = Visibility.Visible;
            clearButton.Visibility = Visibility.Visible;
            instructionsTxtBlock2.Visibility = Visibility.Visible;



        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {


            if (mode == 1)
                changeMsgDes.Visibility = Visibility.Visible;
            else
                changeMsgAes.Visibility = Visibility.Visible;

            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;

        }

        public void colorOverviewText(TextBlock txtB, List<byte> pos)
        {
            byte[] changePos = pos.ToArray();
            txtB.TextEffects.Clear();

            for (byte i = 0; i < changePos.Length; i++)
            {
                TextEffect te = new TextEffect();
                te.PositionStart = changePos[i];
                te.Foreground = Brushes.Red;
                te.PositionCount = 1;
                txtB.TextEffects.Add(te);
            }
        }

        public byte[][] loadInfo()
        {

            byte[] byteArr = arrangeColumn(statesB[0]);
            byte[] byteArr1 = arrangeColumn(statesB[4]);
            byte[] byteArr2 = arrangeColumn(statesB[8]);
            byte[] byteArr3 = arrangeColumn(statesB[12]);
            byte[] byteArr4 = arrangeColumn(statesB[16]);
            byte[] byteArr5 = arrangeColumn(statesB[20]);
            byte[] byteArr6 = arrangeColumn(statesB[24]);
            byte[] byteArr7 = arrangeColumn(statesB[28]);
            byte[] byteArr8 = arrangeColumn(statesB[32]);
            byte[] byteArr9 = arrangeColumn(statesB[36]);
            byte[] byteArr10 = new byte[16];
            byte[] byteArr11 = new byte[16];
            byte[] byteArr12 = new byte[16];
            byte[] byteArr13 = new byte[16];
            byte[] byteArr14 = new byte[16];

            switch (keysize)
            {
                case 0:
                    byteArr10 = arrangeColumn(statesB[39]);
                    break;
                case 1:
                    byteArr10 = arrangeColumn(statesB[40]);
                    byteArr11 = arrangeColumn(statesB[44]);
                    byteArr12 = arrangeColumn(statesB[47]);
                    break;
                case 2:
                    byteArr10 = arrangeColumn(statesB[40]);
                    byteArr11 = arrangeColumn(statesB[44]);
                    byteArr12 = arrangeColumn(statesB[48]);
                    byteArr13 = arrangeColumn(statesB[52]);
                    byteArr14 = arrangeColumn(statesB[55]);
                    break;
                default:
                    break;
            }


            byte[][] multDimArr = new byte[15][];

            multDimArr[0] = byteArr;
            multDimArr[1] = byteArr1;
            multDimArr[2] = byteArr2;
            multDimArr[3] = byteArr3;
            multDimArr[4] = byteArr4;
            multDimArr[5] = byteArr5;
            multDimArr[6] = byteArr6;
            multDimArr[7] = byteArr7;
            multDimArr[8] = byteArr8;
            multDimArr[9] = byteArr9;
            multDimArr[10] = byteArr10;
            multDimArr[11] = byteArr11;
            multDimArr[12] = byteArr12;
            multDimArr[13] = byteArr13;
            multDimArr[14] = byteArr14;

            return multDimArr;
        }

        public void showGeneralOverviewAES()
        {
            clearTextEffect();

            generalViewAES.Visibility = Visibility.Visible;

            byte[][] roundsInfo = loadInfo();

            if (keysize == 0)
            {
                genOverviewAES.Text = Properties.Resources.OverviewAES128;
                overviewAES.Visibility = Visibility.Visible;

                IEnumerable<TextBlock> textChilds = overviewAES.Children.OfType<TextBlock>();
                IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();


                for (int i = 0; i <= 10; i++)
                {
                    List<string> strList = new List<string>();

                    enumerator.MoveNext();
                    enumerator.Current.Visibility = Visibility.Visible;

                    foreach (byte b in roundsInfo[i])
                        strList.Add(b.ToString("X2"));

                    string cipherState = string.Join("-", strList.ToArray());
                    enumerator.Current.Text = cipherState;


                }

                IEnumerator<TextBlock> enumerator2 = textChilds.GetEnumerator();

                for (byte j = 0; j < 39; j += 4)
                {
                    enumerator2.MoveNext();
                    enumerator2.Current.Visibility = Visibility.Visible;

                    List<byte> tmp = new List<byte>();



                    for (byte k = 0; k < statesB[k].Length; k++)
                    {

                        if (states[j][k] != statesB[j][k])
                        {

                            List<int> changePos = changePosition();
                            TextEffect te = new TextEffect();
                            te.PositionStart = changePos[k];
                            te.Foreground = Brushes.Red;
                            te.PositionCount = 2;
                            enumerator2.Current.TextEffects.Add(te);
                        }

                    }

                }


                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {


                    if (states[39][k] != statesB[39][k])
                    {


                        List<int> changePos = changePosition();
                        TextEffect te = new TextEffect();
                        te.PositionStart = changePos[k];
                        te.Foreground = Brushes.Red;
                        te.PositionCount = 2;
                        enumerator2.Current.TextEffects.Add(te);
                    }
                }

            }

            else if (keysize == 1)
            {
                genOverviewAES.Text = Properties.Resources.OverviewAES192;
                overviewAES192.Visibility = Visibility.Visible;

                IEnumerable<TextBlock> textChilds = overviewAES192.Children.OfType<TextBlock>();
                IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();


                for (int i = 0; i <= 14; i++)
                {
                    List<string> strList = new List<string>();

                    enumerator.MoveNext();
                    enumerator.Current.Visibility = Visibility.Visible;

                    foreach (byte b in roundsInfo[i])
                        strList.Add(b.ToString("X2"));

                    string cipherState = string.Join("-", strList.ToArray());
                    enumerator.Current.Text = cipherState;


                }

                IEnumerator<TextBlock> enumerator2 = textChilds.GetEnumerator();

                for (byte j = 0; j < 47; j += 4)
                {
                    enumerator2.MoveNext();
                    enumerator2.Current.Visibility = Visibility.Visible;

                    List<byte> tmp = new List<byte>();



                    for (byte k = 0; k < statesB[k].Length; k++)
                    {

                        if (states[j][k] != statesB[j][k])
                        {

                            List<int> changePos = changePosition();
                            TextEffect te = new TextEffect();
                            te.PositionStart = changePos[k];
                            te.Foreground = Brushes.Red;
                            te.PositionCount = 2;
                            enumerator2.Current.TextEffects.Add(te);
                        }

                    }

                }



                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {
                    if (states[47][k] != statesB[47][k])
                    {


                        List<int> changePos = changePosition();
                        TextEffect te = new TextEffect();
                        te.PositionStart = changePos[k];
                        te.Foreground = Brushes.Red;
                        te.PositionCount = 2;
                        enumerator2.Current.TextEffects.Add(te);
                    }
                }

            }

            else
            {
                genOverviewAES.Text = Properties.Resources.OverviewAES256;
                overviewAES256.Visibility = Visibility.Visible;

                IEnumerable<TextBlock> textChilds = overviewAES256.Children.OfType<TextBlock>();
                IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();

                for (int i = 0; i <= 14; i++)
                {
                    List<string> strList = new List<string>();

                    enumerator.MoveNext();
                    enumerator.Current.Visibility = Visibility.Visible;

                    foreach (byte b in roundsInfo[i])
                        strList.Add(b.ToString("X2"));

                    string cipherState = string.Join("-", strList.ToArray());
                    enumerator.Current.Text = cipherState;


                }

                IEnumerator<TextBlock> enumerator2 = textChilds.GetEnumerator();

                for (byte j = 0; j < 55; j += 4)
                {
                    enumerator2.MoveNext();
                    enumerator2.Current.Visibility = Visibility.Visible;

                    List<byte> tmp = new List<byte>();



                    for (byte k = 0; k < statesB[k].Length; k++)
                    {

                        if (states[j][k] != statesB[j][k])
                        {

                            List<int> changePos = changePosition();
                            TextEffect te = new TextEffect();
                            te.PositionStart = changePos[k];
                            te.Foreground = Brushes.Red;
                            te.PositionCount = 2;
                            enumerator2.Current.TextEffects.Add(te);
                        }

                    }

                }


                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {
                    if (states[55][k] != statesB[55][k])
                    {
                        List<int> changePos = changePosition();
                        TextEffect te = new TextEffect();
                        te.PositionStart = changePos[k];
                        te.Foreground = Brushes.Red;
                        te.PositionCount = 2;
                        enumerator2.Current.TextEffects.Add(te);
                    }
                }



            }
        }

        public List<int> changePosition()
        {
            List<int> intList = new List<int>();

            int[] hexPos = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45 };
            intList.AddRange(hexPos);

            return intList;
        }

        public void percentageChanged()
        {
            List<double> percentages = new List<double>();



            if (mode == 1)
            {
                var strings = binaryStrings(states[4], statesB[4]);

                for (int desRound = 0; desRound < 17; desRound++)
                {
                    toStringArray(desRound);
                    int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);

                    percentages.Add(avalanche);
                }

                int i = 0;

                foreach (double dl in percentages)
                {
                    //((TextBlock)this.FindName("percent" + i)).Text = dl.ToString();
                    ((TextBlock)this.FindName(string.Format("percent{0}", i))).Text = string.Format("{0} %",
                        dl.ToString());
                    i++;
                }
            }

            if (mode == 0)
            {


                switch (keysize)
                {

                    case 0:

                        Tuple<string, string> strings;

                        for (int aesRound = 0; aesRound <= 36; aesRound += 4)
                        {

                            strings = binaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = nrOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = calcAvalancheEffect(nrDiffBits, strings);

                            percentages.Add(avalanche);
                        }

                        strings = binaryStrings(states[39], statesB[39]);
                        int nrDiffBits2 = nrOfBitsFlipped(states[39], statesB[39]);
                        avalanche = calcAvalancheEffect(nrDiffBits2, strings);

                        percentages.Add(avalanche);

                        int i = 0;

                        foreach (double dl in percentages)
                        {
                            ((TextBlock)this.FindName(string.Format("percentAes{0}", i))).Text = string.Format(
                                "{0} %", dl.ToString());
                            i++;
                        }

                        break;

                    case 1:

                        Tuple<string, string> strings2;

                        for (int aesRound = 0; aesRound <= 44; aesRound += 4)
                        {

                            strings2 = binaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = nrOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = calcAvalancheEffect(nrDiffBits, strings2);

                            percentages.Add(avalanche);
                        }

                        strings2 = binaryStrings(states[47], statesB[47]);
                        int nrDiffBits192 = nrOfBitsFlipped(states[47], statesB[47]);
                        avalanche = calcAvalancheEffect(nrDiffBits192, strings2);

                        percentages.Add(avalanche);

                        int j = 0;

                        foreach (double dl in percentages)
                        {
                            ((TextBlock)this.FindName(string.Format("percentAes192_{0}", j))).Text =
                                string.Format("{0} %", dl.ToString());
                            j++;
                        }

                        break;

                    case 2:

                        Tuple<string, string> strings3;

                        for (int aesRound = 0; aesRound <= 52; aesRound += 4)
                        {

                            strings3 = binaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = nrOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = calcAvalancheEffect(nrDiffBits, strings3);

                            percentages.Add(avalanche);
                        }

                        strings3 = binaryStrings(states[55], statesB[55]);
                        int nrDiffBits256 = nrOfBitsFlipped(states[55], statesB[55]);
                        avalanche = calcAvalancheEffect(nrDiffBits256, strings3);

                        percentages.Add(avalanche);

                        int k = 0;

                        foreach (double dl in percentages)
                        {
                            ((TextBlock)this.FindName(string.Format("percentAes256_{0}", k))).Text =
                                string.Format("{0} %", dl.ToString());
                            k++;
                        }

                        break;

                    default:
                        break;
                }
            }


        }

        public void showGeneralOverview()
        {
            generalViewDES.Visibility = Visibility.Visible;
            IEnumerable<TextBlock> textChilds = overviewDES.Children.OfType<TextBlock>();
            IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();

            for (int i = 0; i < 17; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                enumerator.Current.Text = lrDataB[i, 0];

                List<byte> tmp = new List<byte>();
                char[] oldArr = lrData[i, 0].ToCharArray();
                char[] changedArr = lrDataB[i, 0].ToCharArray();


                for (byte j = 0; j < lrData[i, 0].Length; j++)
                {
                    bool booly = oldArr[i].Equals(changedArr[i]);
                    if (!oldArr[j].Equals(changedArr[j]))
                        tmp.Add(j);

                    colorOverviewText(enumerator.Current, tmp);
                }
            }

            for (int i = 0; i < 17; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                enumerator.Current.Text = lrDataB[i, 1];

                List<byte> tmp = new List<byte>();
                char[] oldArr = lrData[i, 1].ToCharArray();
                char[] changedArr = lrDataB[i, 1].ToCharArray();


                for (byte j = 0; j < lrData[i, 1].Length; j++)
                {
                    bool booly = oldArr[i].Equals(changedArr[i]);
                    if (!oldArr[j].Equals(changedArr[j]))
                        tmp.Add(j);

                    colorOverviewText(enumerator.Current, tmp);
                }
            }

        }

        private void overviewButton_Click(object sender, RoutedEventArgs e)
        {
            //  end.Set();
            slide = 0;
            removeBackground();

            overviewButton.Background = Brushes.Coral;
            avalancheVisualization.ProgressChanged(1, 1);

            toGeneral.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;

            clearElements();

            if (mode == 1)
            {

                extraordinaryOccur.Visibility = Visibility.Hidden;
                bitGridDES.Visibility = Visibility.Hidden;
                showGeneralOverview();
                percentageChanged();
            }
            else
            {

                extraordinaryOccurAes.Visibility = Visibility.Hidden;
                afterRoundsGrid.Visibility = Visibility.Hidden;
                bitRepresentationGrid.Visibility = Visibility.Hidden;
                curvedLinesCanvas.Visibility = Visibility.Hidden;
                showGeneralOverviewAES();
                percentageChanged();
            }
        }


        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == 0 || mode == 1)
            {
                InstructionsPrep.Visibility = Visibility.Hidden;
                clearColors();
                clearKeyColors();

                if (ChangesMadeButton.IsEnabled)
                    ChangesMadeButton.Visibility = Visibility.Visible;


            }
            else
            {
                InstructionsUnprep.Visibility = Visibility.Hidden;

                if (!string.IsNullOrEmpty(TB2.Text))
                    comparison();
            }

            comparisonPane();



        }


        private void Cont_OnClick(object sender, RoutedEventArgs e)
        {


            switch (slide)
            {
                case 2:
                    afterRound0Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 3:
                    afterRound1Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 4:
                    afterRound2Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 5:
                    afterRound3Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 6:
                    afterRound4Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 7:
                    afterRound5Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 8:
                    afterRound6Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 9:
                    afterRound7Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 10:
                    afterRound8Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 11:
                    afterRound9Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 12:
                    afterRound10Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 13:

                    if (mode == 0)
                    {

                        if (keysize == 0)
                            overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        else if (keysize == 1 || keysize == 2)
                            afterRound11Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    if (mode == 1)
                        afterRound11Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    break;
                case 14:
                    afterRound12Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 15:

                    if (mode == 0)
                    {
                        if (keysize == 1)
                            overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        if (keysize == 2)
                            afterRound13Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    if (mode == 1)
                        afterRound13Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                    break;
                case 16:
                    afterRound14Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 17:

                    if (mode == 0)
                        overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (mode == 1)
                        afterRound15Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 18:
                    afterRound16Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 19:
                    overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                default:
                    break;
            }

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            removeBackground();

            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;
            backButton.Visibility = Visibility.Hidden;
            toGeneral.Visibility = Visibility.Hidden;
            contButton.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            afterRoundsTitle.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;
            buttonsPanel.Visibility = Visibility.Hidden;
            inputDataButton.Visibility = Visibility.Hidden;
            buttonsTitle.Visibility = Visibility.Hidden;
            ChangesMadeButton.Visibility = Visibility.Hidden;

            if (mode == 0)
            {
                inputInBits.Visibility = Visibility.Hidden;
                OrigInitialStateGrid.Visibility = Visibility.Hidden;
                initStateTitle.Visibility = Visibility.Hidden;
                aesCheckBox.Visibility = Visibility.Hidden;
                changeMsgAes.Visibility = Visibility.Hidden;
                radioButtons.Visibility = Visibility.Hidden;
                afterRoundsGrid.Visibility = Visibility.Hidden;
                bitRepresentationGrid.Visibility = Visibility.Hidden;
                curvedLinesCanvas.Visibility = Visibility.Hidden;
                extraordinaryOccurAes.Visibility = Visibility.Hidden;
                modifiedInitialStateGrid.Visibility = Visibility.Hidden;
                showChangeTitle.Visibility = Visibility.Hidden;
                generalViewAES.Visibility = Visibility.Hidden;
                canModify = false;
            }

            else if (mode == 1)
            {
                inputGridDES.Visibility = Visibility.Hidden;
                bitGridDES.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Hidden;
                extraordinaryOccur.Visibility = Visibility.Hidden;
                generalViewDES.Visibility = Visibility.Hidden;
                canModifyDES = false;
            }
            else
            {
                othersGrid.Visibility = Visibility.Hidden;
                Cbclass1.Visibility = Visibility.Hidden;
                Cbclass2.Visibility = Visibility.Hidden;
                canModifyOthers = false;
            }


            instructions();




        }

        private void ChangesMadeButton_OnClick(object sender, RoutedEventArgs e)
        {


            if (newText != null)

                doneButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            else
            {
                setAndLoadButtons();
                coloringText();
                coloringKey();

            }
        }

        private void Byte_OnMouseEnter(object sender, RoutedEventArgs e)
        {
            TextBlock txtBlock = sender as TextBlock;

            switch (txtBlock.Name)
            {
                case "Byte1":
                    Border1.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border17.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte2":
                    Border2.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border18.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte3":
                    Border3.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border19.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte4":
                    Border4.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border20.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte5":
                    Border5.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border21.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte6":
                    Border6.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border22.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte7":
                    Border7.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border23.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte8":
                    Border8.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border24.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte9":
                    Border9.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border25.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte10":
                    Border10.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border26.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte11":
                    Border11.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border27.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte12":
                    Border12.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border28.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte13":
                    Border13.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border29.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte14":
                    Border14.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border30.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte15":
                    Border15.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border31.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                case "Byte16":
                    Border16.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    Border32.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                    break;
                    Default:
                    break;

            }

        }

        private void Byte_OnMouseLeave(object sender, RoutedEventArgs e)
        {
            TextBlock txtBlock = sender as TextBlock;

            switch (txtBlock.Name)
            {
                case "Byte1":
                    Border1.Background = Brushes.Transparent;
                    Border17.Background = Brushes.Transparent;
                    break;
                case "Byte2":
                    Border2.Background = Brushes.Transparent;
                    Border18.Background = Brushes.Transparent;
                    break;
                case "Byte3":
                    Border3.Background = Brushes.Transparent;
                    Border19.Background = Brushes.Transparent;
                    break;
                case "Byte4":
                    Border4.Background = Brushes.Transparent;
                    Border20.Background = Brushes.Transparent;
                    break;
                case "Byte5":
                    Border5.Background = Brushes.Transparent;
                    Border21.Background = Brushes.Transparent;
                    break;
                case "Byte6":
                    Border6.Background = Brushes.Transparent;
                    Border22.Background = Brushes.Transparent;
                    break;
                case "Byte7":
                    Border7.Background = Brushes.Transparent;
                    Border23.Background = Brushes.Transparent;
                    break;
                case "Byte8":
                    Border8.Background = Brushes.Transparent;
                    Border24.Background = Brushes.Transparent;
                    break;
                case "Byte9":
                    Border9.Background = Brushes.Transparent;
                    Border25.Background = Brushes.Transparent;
                    break;
                case "Byte10":
                    Border10.Background = Brushes.Transparent;
                    Border26.Background = Brushes.Transparent;
                    break;
                case "Byte11":
                    Border11.Background = Brushes.Transparent;
                    Border27.Background = Brushes.Transparent;
                    break;
                case "Byte12":
                    Border12.Background = Brushes.Transparent;
                    Border28.Background = Brushes.Transparent;
                    break;
                case "Byte13":
                    Border13.Background = Brushes.Transparent;
                    Border29.Background = Brushes.Transparent;
                    break;
                case "Byte14":
                    Border14.Background = Brushes.Transparent;
                    Border30.Background = Brushes.Transparent;
                    break;
                case "Byte15":
                    Border15.Background = Brushes.Transparent;
                    Border31.Background = Brushes.Transparent;
                    break;
                case "Byte16":
                    Border16.Background = Brushes.Transparent;
                    Border32.Background = Brushes.Transparent;
                    break;
                    Default:
                    break;

            }

        }
    }
}

#endregion
