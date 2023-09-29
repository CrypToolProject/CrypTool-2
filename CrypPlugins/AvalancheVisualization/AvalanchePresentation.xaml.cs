/*
   Copyright 2011-2023 CrypTool 2 Team <ct2contact@CrypTool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

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
        private double avalanche;
        private static readonly Random rnd = new Random();

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

        private readonly CrypTool.Plugins.AvalancheVisualization.AvalancheVisualization avalancheVisualization;
        #endregion

        #region constructor

        public AvalanchePresentation(CrypTool.Plugins.AvalancheVisualization.AvalancheVisualization av)
        {
            InitializeComponent();
            avalancheVisualization = av;
            inputInBits.IsVisibleChanged += OnVisibleChanged;
            OrigInitialStateGrid.IsVisibleChanged += OnVisibleChanged;
            modifiedInitialStateGrid.IsVisibleChanged += OnVisibleChanged;
            initStateTitle.IsVisibleChanged += OnTitleChanged;
            modificationGridDES.IsVisibleChanged += OnVisibleChanged;
            afterRoundsGrid.IsVisibleChanged += OnVisibleChanged;
            buttonsPanel.IsVisibleChanged += OnTitleChanged;
            inputGridDES.IsVisibleChanged += Modify;
            othersGrid.IsVisibleChanged += ModifyOthers;
            modificationGridDES.IsVisibleChanged += Modify;
        }

        #endregion

        #region methods

        /// <summary>
        /// Loads initial unchanged text
        /// </summary>
        /// <param name="initState"></param>
        /// <param name="initKey"></param>
        public void LoadInitialState(byte[] initState, byte[] initKey)
        {
            int i = 1;
            int j = 17;
            int k = 1;

            string binSequence = BinaryAsString(initState).Replace(" ", "");
            string keyBinSequence = BinaryAsString(initKey).Replace(" ", "");

            if (mode == 0)
            {
                while (i <= 16)
                {
                    ((TextBlock)FindName("initStateTxtBlock" + i)).Text = initState[i - 1].ToString("X2");
                    i++;
                }

                if (keysize == 0)
                {
                    int index128 = 0;
                    while (j <= 32 && index128 < 16)
                    {
                        ((TextBlock)FindName("initStateTxtBlock" + j)).Text = initKey[index128].ToString("X2");
                        j++;
                        index128++;
                    }
                    for (int a = 1; a <= binSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    }
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("keyBit{0}", a))).Text =
                            keyBinSequence[a - 1].ToString();
                    }
                }
                else if (keysize == 1)
                {
                    while (k <= 24)
                    {
                        ((TextBlock)FindName("initStateKey192_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;
                    }
                    for (int a = 1; a <= binSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    }
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("keyBit192_{0}", a))).Text =
                            keyBinSequence[a - 1].ToString();
                    }
                }
                else
                {
                    while (k <= 32)
                    {
                        ((TextBlock)FindName("initStateKey256_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;
                    }
                    for (int a = 1; a <= binSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    }
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                    {
                        ((TextBlock)FindName(string.Format("keyBit256_{0}", a))).Text = keyBinSequence[a - 1].ToString();
                    }
                }
            }
            else
            {
                for (int a = 1; a <= binSequence.Length; a++)
                {
                    ((TextBlock)FindName(string.Format("desBit{0}", a))).Text = binSequence[a - 1].ToString();
                }
                for (int a = 1; a <= keyBinSequence.Length; a++)
                {
                    ((TextBlock)FindName(string.Format("desKeyBit{0}", a))).Text =
                        keyBinSequence[a - 1].ToString();
                }

                string firstHalf = binSequence.Substring(0, 32);
                string secondHalf = binSequence.Substring(32, 32);
                string firstKeyHalf = keyBinSequence.Substring(0, 32);
                string secondKeyHalf = keyBinSequence.Substring(32, 32);

                origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
                origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            }
        }

        public void LoadChangedMsg(byte[] msg, bool textChanged)
        {
            int k = 33;
            int i = 1;

            if (mode == 0)
            {
                if (radioDecimal.IsChecked == true)
                {
                    while (k <= 48)
                    {
                        ((TextBlock)FindName("initStateTxtBlock" + k)).Text = msg[i - 1].ToString();
                        i++;
                        k++;
                    }
                }

                if (radioHexa.IsChecked == true)
                {
                    while (k <= 48)
                    {
                        ((TextBlock)FindName("initStateTxtBlock" + k)).Text = msg[i - 1].ToString("X2");
                        i++;
                        k++;
                    }
                }
            }
            else
            {
                if (radioBinaryDes.IsChecked == true)
                {
                    string binSequence = BinaryAsString(msg).Replace(" ", "");
                    string firstHalf = binSequence.Substring(0, 32);
                    string secondHalf = binSequence.Substring(32, 32);
                    modTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
                }

                if (radioDecimalDes.IsChecked == true)
                {
                    string strB = DecimalAsString(textB);
                    modTextDES.Text = strB;
                }

                if (radioHexaDes.IsChecked == true)
                {
                    string strB = HexaAsString(textB);
                    modTextDES.Text = strB;
                    string keyStrB = HexaAsString(key);
                    modKeyDES.Text = keyStrB;
                }
            }
        }

        public void LoadChangedKey(byte[] newKey)
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
                            ((TextBlock)FindName("modKey192_" + i)).Text = newKey[i - 1].ToString();
                            i++;

                        }
                    }
                    else if (keysize == 2)
                    {
                        int i = 1;
                        while (i <= 32)
                        {
                            ((TextBlock)FindName("modKey256_" + i)).Text = newKey[i - 1].ToString();
                            i++;
                        }
                    }
                    else
                    {
                        int l = 49;
                        int i = 1;
                        while (l <= 64)
                        {
                            ((TextBlock)FindName("initStateTxtBlock" + l)).Text = newKey[i - 1].ToString();
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
                            ((TextBlock)FindName("modKey192_" + i)).Text = newKey[i - 1].ToString("X2");
                            i++;
                        }
                    }
                    else if (keysize == 2)
                    {
                        int i = 1;
                        while (i <= 32)
                        {
                            ((TextBlock)FindName("modKey256_" + i)).Text = newKey[i - 1].ToString("X2");
                            i++;
                        }
                    }                    
                    else
                    {
                        int l = 49;
                        int i = 1;

                        while (l <= 64)
                        {
                            ((TextBlock)FindName("initStateTxtBlock" + l)).Text = newKey[i - 1].ToString("X2");
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
                    string keyBinSequence = BinaryAsString(newKey).Replace(" ", "");
                    string firstKeyHalf = keyBinSequence.Substring(0, 32);
                    string secondKeyHalf = keyBinSequence.Substring(32, 32);
                    modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
                }
                if (radioDecimalDes.IsChecked == true)
                {
                    string keyStrB = DecimalAsString(key);
                    modKeyDES.Text = keyStrB;
                }
                if (radioHexaDes.IsChecked == true)
                {
                    string keyStrB = HexaAsString(key);
                    modKeyDES.Text = keyStrB;
                }

            }
        }

        public void ColoringKey()
        {
            ClearKeyEffect();

            switch (mode)
            {
                case 0:

                    ClearKeyColors();
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
                        {
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                        }
                        a++;
                    }

                    foreach (Border bor in gridChildren2)
                    {
                        if (keyA[b] != key[b])
                        {
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                        }
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
                                List<int> positions = ArrangePos();
                                TextEffect textEffect = new TextEffect
                                {
                                    PositionStart = positions[i],
                                    Foreground = Brushes.Red
                                };
                                if (radioDecimalDes.IsChecked == true)
                                {
                                    textEffect.PositionCount = 4;
                                }
                                if (radioHexaDes.IsChecked == true)
                                {
                                    textEffect.PositionCount = 2;
                                }
                                origKeyDES.TextEffects.Add(textEffect);
                                modKeyDES.TextEffects.Add(textEffect);
                            }
                        }
                    }
                    else
                    {
                        for (byte i = 0; i < origKeyDES.Text.Length; i++)
                        {
                            if (origKeyDES.Text[i] != modKeyDES.Text[i])
                            {
                                TextEffect ti2 = new TextEffect
                                {
                                    PositionStart = i,
                                    PositionCount = 1,
                                    Foreground = Brushes.Red
                                };
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

        public void ColoringText()
        {
            ClearTextEffect();

            switch (mode)
            {

                case 0:

                    ClearColors();

                    IEnumerable<Border> gridChildren = Grid1.Children.OfType<Border>();
                    IEnumerable<Border> gridChildren2 = Grid2.Children.OfType<Border>();

                    int a = 0;
                    int b = 0;

                    foreach (Border bor in gridChildren)
                    {
                        if (textA[a] != textB[a])
                        {
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                        }
                        a++;
                    }
                    foreach (Border bor in gridChildren2)
                    {
                        if (textA[b] != textB[b])
                        {
                            bor.Background = (Brush)new BrushConverter().ConvertFromString("#f5f5dc");
                        }
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
                                List<int> positions = ArrangePos();
                                TextEffect ti = new TextEffect
                                {
                                    PositionStart = positions[i],
                                    Foreground = Brushes.Red
                                };
                                if (radioDecimalDes.IsChecked == true)
                                {
                                    ti.PositionCount = 4;
                                }
                                if (radioHexaDes.IsChecked == true)
                                {
                                    ti.PositionCount = 3;
                                }
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
                                TextEffect ti = new TextEffect
                                {
                                    PositionStart = i,
                                    Foreground = Brushes.Red,
                                    PositionCount = 1
                                };
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

        public void SetAndLoadButtons()
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
                SetButtonsScrollViewer();

                showChangeTitle.Visibility = Visibility.Visible;
                buttonsPanel.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;
                inputDataButton.Visibility = Visibility.Visible;

                if (keysize == 1)
                {
                    modifiedKeyGrid192.Visibility = Visibility.Visible;
                }
                else if (keysize == 2)
                {
                    modifiedKeyGrid256.Visibility = Visibility.Visible;
                }
                else
                {
                    modifiedKeyGrid.Visibility = Visibility.Visible;
                }

                showChangeTitle.Text = showChangeTitle.Text.TrimEnd('1', '2', '5', '6', '8', '9');
                if (keysize == 1)
                {
                    showChangeTitle.Inlines.Add(new Run("192") { Foreground = Brushes.White });
                }
                else if (keysize == 2)
                {
                    showChangeTitle.Inlines.Add(new Run("256") { Foreground = Brushes.White });
                }
                else
                {
                    showChangeTitle.Inlines.Add(new Run("128") { Foreground = Brushes.White });
                }
            }
            else
            {
                inputGridDES.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Visible;
                buttonsPanel.Visibility = Visibility.Visible;
                inputDataButton.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;

                SetButtonsScrollViewer();
            }
        }

        public bool stop = false;

        private List<int> ArrangePos()
        {
            List<int> intList = new List<int>();

            int[] decPos = new int[] { 0, 4, 7, 11, 15, 19, 23, 27 };
            int[] hexPos = new int[] { 0, 3, 6, 9, 12, 15, 18, 21 };

            if (mode == 1)
            {
                if (radioDecimalDes.IsChecked == true)
                {
                    intList.AddRange(decPos);
                }
                if (radioHexaDes.IsChecked == true)
                {
                    intList.AddRange(hexPos);
                }
            }
            else
            {
                intList.AddRange(hexPos);
            }
            return intList;
        }

        public void ClearColors()
        {
            IEnumerable<Border> gridChildren = Grid1.Children.OfType<Border>();
            IEnumerable<Border> gridChildren2 = Grid2.Children.OfType<Border>();
            foreach (Border bor in gridChildren)
            {
                bor.Background = Brushes.Transparent;
            }
            foreach (Border bor in gridChildren2)
            {
                bor.Background = Brushes.Transparent;
            }
        }

        public void ClearKeyColors()
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
            {
                bor.Background = Brushes.Transparent;
            }
            foreach (Border bor in gridChildren2)
            {
                bor.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// Loads byte information into the respective columns
        /// </summary>
        public void LoadBytePropagationData()
        {
            int a = 0;
            List<TextBox> tmp = CreateTxtBoxList();
            byte[] state = ArrangeColumn(states[0]);
            foreach (TextBox tb in tmp)
            {
                tb.Text = tempState[a].ToString("X2");
                a++;
            }

            int i = 1;
            byte[] subBytes1 = ArrangeColumn(states[1]);
            byte[] shiftRows1 = ArrangeColumn(states[2]);
            byte[] mixColumns1 = ArrangeColumn(states[3]);
            byte[] addKey1 = ArrangeColumn(states[4]);
            byte[] subBytes2 = ArrangeColumn(states[5]);
            byte[] shiftRows2 = ArrangeColumn(states[6]);
            byte[] mixColumns2 = ArrangeColumn(states[7]);
            byte[] addKey2 = ArrangeColumn(states[8]);
            byte[] subBytes3 = ArrangeColumn(states[9]);
            byte[] shiftRows3 = ArrangeColumn(states[10]);
            byte[] mixColumns3 = ArrangeColumn(states[11]);
            byte[] addKey3 = ArrangeColumn(states[12]);

            while (i <= 16)
            {
                ((TextBlock)FindName("roundZero" + i)).Text = state[i - 1].ToString("X2");
                ((TextBlock)FindName("sBoxRound1_" + i)).Text = subBytes1[i - 1].ToString("X2");
                ((TextBlock)FindName("shiftRowRound1_" + i)).Text = shiftRows1[i - 1].ToString("X2");
                ((TextBlock)FindName("mixColumns1_" + i)).Text = mixColumns1[i - 1].ToString("X2");
                ((TextBlock)FindName("addKey1_" + i)).Text = addKey1[i - 1].ToString("X2");
                ((TextBlock)FindName("sBoxRound2_" + i)).Text = subBytes2[i - 1].ToString("X2");
                ((TextBlock)FindName("shiftRowRound2_" + i)).Text = shiftRows2[i - 1].ToString("X2");
                ((TextBlock)FindName("mixColumns2_" + i)).Text = mixColumns2[i - 1].ToString("X2");
                ((TextBlock)FindName("addKey2_" + i)).Text = addKey2[i - 1].ToString("X2");
                ((TextBlock)FindName("sBoxRound3_" + i)).Text = subBytes3[i - 1].ToString("X2");
                ((TextBlock)FindName("shiftRowRound3_" + i)).Text = shiftRows3[i - 1].ToString("X2");
                ((TextBlock)FindName("mixColumns3_" + i)).Text = mixColumns3[i - 1].ToString("X2");
                ((TextBlock)FindName("addKey3_" + i)).Text = addKey3[i - 1].ToString("X2");
                i++;
            }
        }

        /// <summary>
        /// Applies a PKCS7 padding 
        /// </summary>
        public void Padding()
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

        /// <summary>
        /// Shows different intermediate states of the AES encryption process
        /// </summary>
        /// <param name="states"></param>
        /// <param name="statesB"></param>
        public void PrintIntermediateStates(byte[][] states, byte[][] statesB)
        {
            List<TextBlock> tmp = CreateTxtBlockList(3);
            List<TextBlock> tmpB = CreateTxtBlockList(4);
            byte[] state = ArrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = ArrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

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

        public string ToString(byte[] result)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append(b.ToString() + " ");
            }
            return sb.ToString();
        }

        private byte[] RearrangeText(byte[] input)
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

        /// <summary>
        /// Shows initial state
        /// </summary>
        /// <param name="states"></param>
        /// <param name="statesB"></param>
        public void SetUpSubByte(byte[][] states, byte[][] statesB)
        {
            List<TextBlock> tmp = CreateTxtBlockList(1);
            List<TextBlock> tmpB = CreateTxtBlockList(2);

            byte[] state = ArrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = ArrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

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

        /// <summary>
        /// Value of avalanche effect
        /// </summary>
        /// <param name="flippedBits"></param>
        /// <param name="strTuple"></param>
        /// <returns></returns>
        public double CalcAvalancheEffect(int flippedBits, Tuple<string, string> strTuple)
        {
            double avalancheEffect = ((double)flippedBits / strTuple.Item1.Length) * 100;
            double roundUp = Math.Round(avalancheEffect, 1, MidpointRounding.AwayFromZero);
            return roundUp;
        }

        /// <summary>
        /// Average number of flipped bits per byte
        /// </summary>
        /// <param name="flippedBits"></param>
        /// <returns></returns>
        public double AvgNrperByte(int flippedBits)
        {
            if (mode == 0)
            {
                avgNrDiffBit = ((double)flippedBits / 16);
            }
            else
            {
                avgNrDiffBit = ((double)flippedBits / 8);
            }

            avgNrDiffBit = Math.Round(avgNrDiffBit, 1, MidpointRounding.AwayFromZero);

            return avgNrDiffBit;

        }

        /// <summary>
        /// Calculates longest identical bit sequence
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int LongestIdenticalSequence(string[] str)
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

        /// <summary>
        /// Calculates longest flipped bit sequence
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int LongestFlippedSequence(string[] str)
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

        public int CountOccurrence(string[] str)
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
                    {
                        count++;
                    }

                    i++;
                }
                while (j <= 63)
                {
                    if (str[j].Equals("X"))
                    {
                        count2++;
                    }

                    j++;
                }

                //only left half
                if (!count.Equals(0) && count2.Equals(0))
                {
                    occurence = 0;
                }
                //only right half
                else if (!count2.Equals(0) && count.Equals(0))
                {
                    occurence = 1;
                }
                //no changes
                else if (count.Equals(0) && count2.Equals(0))
                {
                    occurence = 2;
                }
                else
                {
                    occurence = 3;
                }
            }
            else
            {
                int count = 0;
                foreach (string st in str)
                {
                    if (st.Equals("X"))
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    occurence = 2;
                }
            }
            return occurence;
        }

        public void ShowOccurence(int occurrence)
        {
            extraordinaryOccur.Text = string.Empty;

            if (occurrence != 3)
            {
                if (mode == 1)
                {
                    extraordinaryOccur.Visibility = Visibility.Visible;
                }

                if (occurrence == 0)
                {
                    extraordinaryOccur.Text = Properties.Resources.OnlyLeft;
                }
                else if (occurrence == 1)
                {
                    extraordinaryOccur.Text = Properties.Resources.OnlyRight;
                }
                else
                {
                    if (mode == 1)
                    {
                        extraordinaryOccur.Text = Properties.Resources.NoChanges;
                    }

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

        /// <summary>
        /// Set colors of pie chart
        /// </summary>
        public void SetColors()
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
            {
                unflippedBitsPiece.angle = 359.9;
            }
        }

        /// <summary>
        /// Prints out current statistical values of cipher
        /// </summary>
        /// <param name="bitsFlipped"></param>
        /// <param name="longestLength"></param>
        /// <param name="longestflipped"></param>
        /// <param name="strTuple"></param>
        public void ShowStatistics(int bitsFlipped, int longestLength, int longestflipped,
            Tuple<string, string> strTuple)
        {
            stats1.Inlines.Add(new Run(" " + bitsFlipped.ToString())
            {
                Foreground = Brushes.Red,
                FontWeight = FontWeights.DemiBold
            });

            if (bitsFlipped > 1 || bitsFlipped == 0)
            {
                stats1.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet1_Plural, strTuple.Item1.Length, avalanche)));
            }
            else
            {
                stats1.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet1, strTuple.Item1.Length, avalanche)));
            }

            stats2.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet2, longestLength.ToString(), sequencePosition)));
            stats3.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet3, longestflipped.ToString(), flippedSeqPosition)));
            if (mode != 2)
            {
                stats4.Inlines.Add(new Run(string.Format(Properties.Resources.StatsBullet4, avgNrDiffBit)));
            }
        }

        /// <summary>
        /// Signalizes flipped bits and highlight the differences 
        /// </summary>
        /// <param name="strTuple"></param>
        public void ShowBitSequence(Tuple<string, string> strTuple)
        {
            string[] diffBits = new string[strTuple.Item1.Length];
            for (int i = 0; i < strTuple.Item1.Length; i++)
            {
                if (strTuple.Item1[i] != strTuple.Item2[i])
                {
                    diffBits[i] = "X";
                }
                else
                {
                    diffBits[i] = " ";
                }
            }

            differentBits = diffBits;

            if (mode == 0)
            {
                int a = 0;
                int b = 256;
                while (a < 128 && b < 384)
                {
                    ((TextBlock)FindName("txt" + b)).Text = diffBits[a].ToString();
                    a++;
                    b++;
                }

                int j = 0;
                int k = 128;
                while (j < 128 && k < 256)
                {
                    if (diffBits[j] == "X")
                    {
                        ((TextBlock)FindName("txt" + j)).Background =
                            (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)FindName("txt" + k)).Background =
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
                    ((TextBlock)FindName("desTxt" + b)).Text = diffBits[a].ToString();
                    ((TextBlock)FindName("desTxt" + b)).Foreground = Brushes.Red;
                    a++;
                    b++;
                }

                int j = 1;
                int k = 65;
                while (j < 64 && k < 128)
                {
                    if (diffBits[j - 1] == "X")
                    {
                        ((TextBlock)FindName("desTxt" + j)).Background = (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)FindName("desTxt" + k)).Background = (Brush)new BrushConverter().ConvertFromString("#fe6f5e");

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

        /// <summary>
        /// Shows binary values of each cipher byte
        /// </summary>
        /// <param name="cipherStateA"></param>
        /// <param name="cipherStateB"></param>
        public void DisplayBinaryValues(byte[] cipherStateA, byte[] cipherStateB)
        {
            byte[] textToArrange = ArrangeText(cipherStateA);
            byte[] textToArrangeB = ArrangeText(cipherStateB);
            string encryptionStateA = BinaryAsString(textToArrange).Replace(" ", "");
            string encryptionStateB = BinaryAsString(textToArrangeB).Replace(" ", "");
            int a = 0;
            int b = 128;
            while (a < 128 && b < 256)
            {
                ((TextBlock)FindName("txt" + a)).Text = encryptionStateA[a].ToString();
                ((TextBlock)FindName("txt" + b)).Text = encryptionStateB[a].ToString();
                a++;
                b++;
            }
        }

        public void ToStringArray(int round)
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

            seqA = GetByteArray(bitSeqA);
            seqB = GetByteArray(bitseqB);
        }

        public byte[] GetByteArray(string str)
        {
            byte[] byteArray = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                byteArray[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);
            }

            return byteArray;
        }

        public void DisplayBinaryValuesDES()
        {
            bitGridDES.Visibility = Visibility.Visible;

            int a = 0;
            int b = 32;
            int c = 64;
            int d = 96;
            while (a < 32 && b < 64 && c < 96 && d < 128)
            {

                ((TextBlock)FindName("txt" + a)).Text = leftHalf[a];
                ((TextBlock)FindName("txt" + b)).Text = rightHalf[a];
                ((TextBlock)FindName("txt" + c)).Text = leftHalfB[a];
                ((TextBlock)FindName("txt" + d)).Text = rightHalfB[a];

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
                ((TextBlock)FindName("desTxt" + i)).Text = leftHalf[i - 1];
                ((TextBlock)FindName("desTxt" + j)).Text = rightHalf[i - 1];
                ((TextBlock)FindName("desTxt" + k)).Text = leftHalfB[i - 1];
                ((TextBlock)FindName("desTxt" + l)).Text = rightHalfB[i - 1];

                i++;
                j++;
                k++;
                l++;
            }
        }

        /// <summary>
        /// Transforms to string of binary values
        /// </summary>
        /// <param name="byteSequence"></param>
        /// <returns></returns>
        public string BinaryAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();
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

        /// <summary>
        /// String of decimal values
        /// </summary>
        /// <param name="byteSequence"></param>
        /// <returns></returns>
        public string DecimalAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteSequence)
            {
                sb.AppendFormat("{0:D3}{1}", b, " ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// String of hexadecimal values
        /// </summary>
        /// <param name="byteSequence"></param>
        /// <returns></returns>
        public string HexaAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte v in byteSequence)
            {
                sb.AppendFormat("{0:X2}{1}", v, " ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Counts how many bits are flipped after comparison
        /// </summary>
        /// <param name="nr1"></param>
        /// <param name="nr2"></param>
        /// <returns></returns>
        public int NoOfBitsFlipped(byte[] nr1, byte[] nr2)
        {
            int shift = 7;
            int count = 0;

            byte[] comparison = XOR(nr1, nr2);
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

        /// <summary>
        /// XOR operation
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        public byte[] XOR(byte[] input1, byte[] input2)
        {
            byte[] result = new byte[input1.Length];
            for (int i = 0; i < input1.Length; i++)
            {
                result[i] = (byte)(input1[i] ^ input2[i]);
            }

            return result;
        }

        /// <summary>
        /// Returns a tuple of strings
        /// </summary>
        /// <param name="cipherStateA"></param>
        /// <param name="cipherStateB"></param>
        /// <returns></returns>
        public Tuple<string, string> BinaryStrings(byte[] cipherStateA, byte[] cipherStateB)
        {
            byte[] textToArrange;
            byte[] textToArrangeB;
            string encryptionStateA;
            string encryptionStateB;

            if (mode == 0)
            {
                textToArrange = ArrangeText(cipherStateA);
                textToArrangeB = ArrangeText(cipherStateB);
                encryptionStateA = BinaryAsString(textToArrange).Replace(" ", "");
                encryptionStateB = BinaryAsString(textToArrangeB).Replace(" ", "");

            }
            else if (mode == 2 || mode == 3 || mode == 4)
            {
                encryptionStateA = BinaryAsString(cipherStateA).Replace(" ", "");
                encryptionStateB = BinaryAsString(cipherStateB).Replace(" ", "");
            }
            else
            {
                encryptionStateA = lrData[roundDES, 0] + lrData[roundDES, 1];
                encryptionStateB = lrDataB[roundDES, 0] + lrDataB[roundDES, 1];

            }
            Tuple<string, string> tuple = new Tuple<string, string>(encryptionStateA, encryptionStateB);
            return tuple;

        }

        /// <summary>
        /// Set content of toolTips
        /// </summary>
        public void SetToolTips()
        {
            tp1.Content = avalanche + " %";
            tp2.Content = (100 - avalanche) + " %";
        }

        private void RemoveBackground()
        {
            IEnumerable<Button> StackPanelChildren = buttonsPanel.Children.OfType<Button>();
            foreach (Button button in StackPanelChildren)
            {
                button.ClearValue(BackgroundProperty);
            }
        }

        /// <summary>
        /// Clear background colors
        /// </summary>
        private void RemoveColors()
        {
            if (mode == 0)
            {
                int a = 0;
                int b = 128;
                while (a < 128 && b < 256)
                {
                    ((TextBlock)FindName("txt" + a)).Background = Brushes.Transparent;
                    ((TextBlock)FindName("txt" + b)).Background = Brushes.Transparent;
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
                    ((TextBlock)FindName("desTxt" + j)).Background = Brushes.Transparent;
                    ((TextBlock)FindName("desTxt" + k)).Background = Brushes.Transparent;
                    j++;
                    k++;
                }
                modificationGridDES.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Updates roundnr displayed on GUI
        /// </summary>
        /// <param name="number"></param>
        public void ChangeRoundNr(int number)
        {
            afterRoundsTitle.Text = afterRoundsTitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            afterRoundsSubtitle.Text = afterRoundsSubtitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (mode == 0)
            {
                if (keysize == 1)
                {
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES192));
                }
                else if (keysize == 2)
                {
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES256));
                }
                else
                {
                    afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_AES128));
                }
            }
            else if (mode == 1)
            {
                afterRoundsTitle.Inlines.Add(new Run(Properties.Resources.Title_DES));
            }

            afterRoundsSubtitle.Inlines.Add(new Run(string.Format("{0}", number)));

        }

        public void ChangeTitle()
        {
            if (mode == 0)
            {
                initStateTitle.Text = initStateTitle.Text.TrimEnd('1', '2', '5', '6', '8', '9');
                if (keysize == 1)
                {
                    initStateTitle.Inlines.Add(new Run("192") { Foreground = Brushes.White });
                }
                else if (keysize == 2)
                {
                    initStateTitle.Inlines.Add(new Run("256") { Foreground = Brushes.White });
                }
                else
                {
                    initStateTitle.Inlines.Add(new Run("128") { Foreground = Brushes.White });
                }
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
                {
                    othersSubtitle.Text = Properties.Resources.ModernCipherSubtitle;
                }
            }
        }

        /// <summary>
        /// New position after shiftrows operation
        /// </summary>
        /// <param name="position"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public TextBlock AfterShifting(int position, int round)
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
                {
                    newPosition = 16;
                }
            }
            TextBlock txtBlock = ((TextBlock)FindName("shiftRowRound" + round + "_" + newPosition));
            return txtBlock;
        }

        public void ClearElements()
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

        public void ShowElements()
        {
            contButton.Visibility = Visibility.Visible;

            if (mode == 0)
            {
                afterRoundsGrid.Visibility = Visibility.Visible;
                bitRepresentationGrid.Visibility = Visibility.Visible;
                curvedLinesCanvas.Visibility = Visibility.Visible;
            }
            if (mode == 1)
            {
                AdjustStats();
            }

            bitsData.Visibility = Visibility.Visible;
            Cb1.Visibility = Visibility.Visible;
            Cb2.Visibility = Visibility.Visible;
            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;
            afterRoundsTitle.Visibility = Visibility.Visible;
            afterRoundsSubtitle.Visibility = Visibility.Visible;

        }

        public List<TextBox> CreateTxtBoxList()
        {
            List<TextBox> txtBoxList = new List<TextBox>();
            for (int i = 1; i <= 16; i++)
            {
                txtBoxList.Add((TextBox)FindName("txtBox" + i));
            }
            return txtBoxList;
        }

        public List<Border> CreateBorderList(int borderListNr)
        {
            List<Border> borderList = new List<Border>();

            switch (borderListNr)
            {
                case 1:
                    for (int i = 9; i <= 12; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 2:
                    for (int i = 9; i <= 12; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 3:
                    for (int i = 9; i <= 12; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                case 4:
                    for (int i = 9; i <= 12; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 5:
                    for (int i = 13; i <= 16; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 6:
                    for (int i = 13; i <= 16; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 7:
                    for (int i = 13; i <= 16; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                case 8:
                    for (int i = 13; i <= 16; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 9:
                    for (int i = 17; i <= 20; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 10:
                    for (int i = 17; i <= 20; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 11:
                    for (int i = 17; i <= 20; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                case 12:
                    for (int i = 17; i <= 20; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 13:
                    for (int i = 21; i <= 24; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 14:
                    for (int i = 21; i <= 24; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                case 15:
                    for (int i = 21; i <= 24; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 16:
                    for (int i = 21; i <= 24; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 17:
                    for (int i = 25; i <= 28; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                case 18:
                    for (int i = 25; i <= 28; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 19:
                    for (int i = 25; i <= 28; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 20:
                    for (int i = 25; i <= 28; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 21:
                    for (int i = 29; i <= 32; i++)
                    {
                        borderList.Add((Border)FindName("byte2_" + i));
                    }
                    break;
                case 22:
                    for (int i = 29; i <= 32; i++)
                    {
                        borderList.Add((Border)FindName("byte1_" + i));
                    }
                    break;
                case 23:
                    for (int i = 29; i <= 32; i++)
                    {
                        borderList.Add((Border)FindName("byte4_" + i));
                    }
                    break;
                case 24:
                    for (int i = 29; i <= 32; i++)
                    {
                        borderList.Add((Border)FindName("byte3_" + i));
                    }
                    break;
                default:
                    break;

            }
            return borderList;
        }

        public List<TextBlock> CreateTxtBlockList(int txtBlockType)
        {
            List<TextBlock> txtBlockList = new List<TextBlock>();
            switch (txtBlockType)
            {
                case 0:

                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("initStateTxtBlock" + i));
                    }
                    break;
                case 1:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("afterAddKey" + i));
                    }
                    break;
                case 2:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("afterAddKey" + i));
                    }
                    break;
                case 3:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("afterRoundTxt" + i));
                    }
                    break;
                case 4:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("afterRoundTxt" + i));
                    }
                    break;
                case 5:
                    for (int i = 256; i < 384; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("txt" + i));
                    }
                    break;
                case 6:
                    for (int i = 1; i < 129; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("bit" + i));
                        txtBlockList.Add((TextBlock)FindName("keyBit" + i));

                    }
                    break;
                case 7:
                    for (int i = 1; i < 65; i++)
                    {
                        txtBlockList.Add((TextBlock)FindName("desBit" + i));
                        txtBlockList.Add((TextBlock)FindName("desKeyBit" + i));
                    }
                    break;

                default:
                    break;

            }
            return txtBlockList;
        }


        public void SetButtonsScrollViewer()
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

        private byte[] ArrangeColumn(byte[] input)
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

        private byte[] ArrangeText(byte[] input)
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

        public void EcryptionProgress(int roundNr)
        {
            switch (mode)
            {
                case 0:
                    if (keysize == 0)
                    {
                        progress = (roundNr + 2) * 0.076;
                    }
                    else if (keysize == 1)
                    {
                        progress = (roundNr + 2) * 0.066;
                    }
                    else
                    {
                        progress = (roundNr + 2) * 0.058;
                    }
                    break;

                case 1:
                    progress = (roundNr + 2) * 0.0526;
                    break;

                case 2:
                case 3:
                case 4:
                    progress = 0.5;
                    if (!string.IsNullOrEmpty(TB2.Text))
                    {
                        progress = 1;
                    }
                    break;

                default:
                    break;
            }

            avalancheVisualization.ProgressChanged(progress, 1);
        }


        #endregion

        #region AES methods
        public void CreateSBox()
        {
            int x = 0;
            while (x < 16)
            {
                sBox[x] = new byte[16];
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

        public byte[][] SetSBox()
        {
            int x = 0;
            while (x < 16)
            {
                sBox[x] = new byte[16];
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

        #endregion

        #region Event handlers 

        private void DoneButton_Click(object sender, RoutedEventArgs e)
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
                {
                    strSequence[i - 1] = ((TextBlock)FindName(string.Format("bit{0}", i))).Text;
                }

                string bitSequence = string.Join("", strSequence);

                for (int k = 0; k < 16; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

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
                    {
                        strKeySequence2[i - 1] = ((TextBlock)FindName(string.Format("keyBit192_{0}", i))).Text;
                    }

                    keyBitSequence = string.Join("", strKeySequence2);
                }
                else if (keysize == 2)
                {
                    string[] strKeySequence3 = new string[256];
                    for (int i = 1; i < 257; i++)
                    {
                        strKeySequence3[i - 1] = ((TextBlock)FindName(string.Format("keyBit256_{0}", i))).Text;
                    }

                    keyBitSequence = string.Join("", strKeySequence3);
                }
                else
                {
                    string[] strKeySequence = new string[128];
                    for (int i = 1; i < bits; i++)
                    {
                        strKeySequence[i - 1] = ((TextBlock)FindName(string.Format(bitName, i))).Text;
                    }

                    keyBitSequence = string.Join("", strKeySequence);
                }

                for (int j = 0; j < 16; j++)
                {
                    keyBits[j] = new string[8];
                }

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
                    LoadChangedMsg(temporaryB, true);
                    LoadChangedKey(newKey);
                }, null);

                ColoringText();
                ColoringKey();
                statesB = aesDiffusion.statesB;

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    SetAndLoadButtons();

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
                {
                    strSequence[i - 1] = ((TextBlock)FindName(string.Format("desBit{0}", i))).Text;
                }

                string bitSequence = string.Join("", strSequence);
                for (int k = 0; k < 8; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

                newText = result.Select(s => Convert.ToByte(s, 2)).ToArray();
                textB = newText;
                canStop = true;

                string[] strKeySequence = new string[64];
                for (int i = 1; i < 65; i++)
                {
                    strKeySequence[i - 1] = ((TextBlock)FindName(string.Format("desKeyBit{0}", i))).Text;
                }

                keyBitSequence = string.Join("", strKeySequence);
                for (int k = 0; k < keyA.Length; k++)
                {
                    keyResult[k] = keyBitSequence.Substring(m, 8);
                    m += 8;
                }

                newKey = keyResult.Select(s => Convert.ToByte(s, 2)).ToArray();
                key = newKey;

                desDiffusion = new DES(newText, newKey)
                {
                    textChanged = true
                };
                desDiffusion.DESProcess();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    LoadChangedMsg(newText, true);
                    LoadChangedKey(newKey);
                    SetAndLoadButtons();
                }, null);

                ColoringText();
                ColoringKey();
                lrDataB = desDiffusion.lrDataB;
                currentDES = desDiffusion.outputCiphertext;
            }
            avalancheVisualization.OutputStream = string.Format("{0}{1}{2}", avalancheVisualization.GeneratedData(0), avalancheVisualization.GeneratedData(1), avalancheVisualization.GeneratedData(2));
        }


        public void UpdateDataColor()
        {
            updateColor = true;
            clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
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
                {
                    tBList.Add((TextBlock)FindName(string.Format("bit{0}", i)));
                }

                if (keysize == 0)
                {

                    for (int i = 1; i < 129; i++)
                    {
                        tBList.Add((TextBlock)FindName(string.Format("keyBit{0}", i)));
                    }
                }
                else if (keysize == 1)
                {
                    for (int i = 1; i < 193; i++)
                    {
                        tBList.Add((TextBlock)FindName(string.Format("keyBit192_{0}", i)));
                    }
                }
                else
                {
                    for (int i = 1; i < 257; i++)
                    {
                        tBList.Add((TextBlock)FindName(string.Format("keyBit256_{0}", i)));
                    }
                }

            }
            else
            {
                for (int i = 1; i < 65; i++)
                {
                    tBList.Add((TextBlock)FindName(string.Format("desBit{0}", i)));
                    tBList.Add((TextBlock)FindName(string.Format("desKeyBit{0}", i)));
                }
            }

            foreach (TextBlock tb in tBList)
            {
                if (tb.Foreground == Brushes.Red)
                {
                    if (tb.Text == "1")
                    {
                        tb.Text = "0";
                    }
                    else
                    {
                        tb.Text = "1";
                    }
                    tb.Foreground = Brushes.Black;
                }
            }
        }

        private void InputDataButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveBackground();
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
                ClearColors();
                ClearKeyColors();
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

            ComparisonPane();

            if ((bool)aesCheckBox.IsChecked || (bool)desCheckBox.IsChecked)
            {
                instructionsTxtBlock2.Visibility = Visibility.Visible;
                doneButton.Visibility = Visibility.Visible;
                clearButton.Visibility = Visibility.Visible;

                if (mode == 0)
                {
                    changeMsgAes.Visibility = Visibility.Hidden;
                }
                if (mode == 1)
                {
                    changeMsgDes.Visibility = Visibility.Hidden;
                }
            }
        }

        public void EmptyInformation()
        {
            stats1.Text = string.Empty;
            stats2.Text = string.Empty;
            stats3.Text = string.Empty;
            stats4.Text = string.Empty;
        }

        private void AfterRound0Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[0], statesB[0]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(0);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound0Button.Background = Brushes.Coral;
            EcryptionProgress(0);
            slide = 3;

            if (mode == 0)
            {
                roundNumber = 1 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[0], statesB[0]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[0], statesB[0]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 0;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);

                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();
                
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound1Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(1);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound1Button.Background = Brushes.Coral;
            EcryptionProgress(1);
            slide = 4;

            if (mode == 0)
            {
                roundNumber = 2 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[4], statesB[4]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[4], statesB[4]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 1;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);

                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();

                }, null);
            }
        }

        private void AfterRound2Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[8], statesB[8]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(2);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound2Button.Background = Brushes.Coral;
            EcryptionProgress(2);
            slide = 5;

            if (mode == 0)
            {
                roundNumber = 3 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[8], statesB[8]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[8], statesB[8]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 2;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);

                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);

            }
        }

        private void AfterRound3Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[12], statesB[12]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(3);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound3Button.Background = Brushes.Coral;
            EcryptionProgress(3);
            slide = 6;

            if (mode == 0)
            {
                roundNumber = 4 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[12], statesB[12]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[12], statesB[12]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 3;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound4Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[12], statesB[12]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(4);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound4Button.Background = Brushes.Coral;
            EcryptionProgress(4);
            slide = 7;

            if (mode == 0)
            {
                roundNumber = 5 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[16], statesB[16]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[16], statesB[16]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 4;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound5Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[20], statesB[20]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(5);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound5Button.Background = Brushes.Coral;
            EcryptionProgress(5);
            slide = 8;

            if (mode == 0)
            {
                roundNumber = 6 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[20], statesB[20]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[20], statesB[20]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 5;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound6Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[24], statesB[24]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(6);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound6Button.Background = Brushes.Coral;
            EcryptionProgress(6);
            slide = 9;

            if (mode == 0)
            {
                roundNumber = 7 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[24], statesB[24]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[24], statesB[24]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 6;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound7Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[28], statesB[28]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(7);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound7Button.Background = Brushes.Coral;
            EcryptionProgress(7);
            slide = 10;

            if (mode == 0)
            {
                roundNumber = 8 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[28], statesB[28]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[28], statesB[28]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 7;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound8Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[32], statesB[32]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(8);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound8Button.Background = Brushes.Coral;
            EcryptionProgress(8);
            slide = 11;

            if (mode == 0)
            {
                roundNumber = 9 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[32], statesB[32]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[32], statesB[32]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 8;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound9Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings = BinaryStrings(states[36], statesB[36]);
            int occurrence;

            ClearElements();
            ChangeRoundNr(9);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound9Button.Background = Brushes.Coral;
            EcryptionProgress(9);
            slide = 12;

            if (mode == 0)
            {
                roundNumber = 10 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[36], statesB[36]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[36], statesB[36]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 9;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                   DisplayBinaryValuesDES();
                   ShowBitSequence(strings);
                   occurrence = CountOccurrence(differentBits);
                   ShowOccurence(occurrence);
                   lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                   lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                   ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                   SetColors();
                   setAngles(angle_1, angle_2);
                   SetToolTips();
                }, null);
            }
        }

        private void AfterRound10Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            int occurrence;

            ClearElements();
            ChangeRoundNr(10);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound10Button.Background = Brushes.Coral;
            EcryptionProgress(10);
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
                EmptyInformation();

                if (keysize == 0)
                {
                    toGeneral.Visibility = Visibility.Visible;
                    strings = BinaryStrings(states[39], statesB[39]);
                    nrDiffBits = NoOfBitsFlipped(states[39], statesB[39]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = AvgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Final();
                        DisplayBinaryValues(states[39], statesB[39]);
                        ShowBitSequence(strings);
                        occurrence = CountOccurrence(differentBits);
                        ShowOccurence(occurrence);
                        lengthIdentSequence = LongestIdenticalSequence(differentBits);
                        lengthFlippedSequence = LongestFlippedSequence(differentBits);
                        ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                        SetColors();
                        setAngles(angle_1, angle_2);
                        SetToolTips();
                    }, null);
                }
                else
                {
                    strings = BinaryStrings(states[40], statesB[40]);
                    nrDiffBits = NoOfBitsFlipped(states[40], statesB[40]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = AvgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        PrintIntermediateStates(states, statesB);
                        DisplayBinaryValues(states[40], statesB[40]);
                        ShowBitSequence(strings);
                        occurrence = CountOccurrence(differentBits);
                        ShowOccurence(occurrence);
                        lengthIdentSequence = LongestIdenticalSequence(differentBits);
                        lengthFlippedSequence = LongestFlippedSequence(differentBits);
                        ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                        SetColors();
                        setAngles(angle_1, angle_2);
                        SetToolTips();
                    }, null);
                }
            }
            else
            {
                roundDES = 10;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
               }, null);
            }
        }

        private void AfterRound11Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            ClearElements();
            ChangeRoundNr(11);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound11Button.Background = Brushes.Coral;
            EcryptionProgress(11);
            slide = 14;

            if (mode == 0)
            {
                Tuple<string, string> strings = BinaryStrings(states[44], statesB[44]);
                roundNumber = 12 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[44], statesB[44]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[44], statesB[44]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
                roundDES = 11;
                EcryptionProgress(roundDES);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound12Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            int occurrence;

            ClearElements();
            ChangeRoundNr(12);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound12Button.Background = Brushes.Coral;
            EcryptionProgress(12);
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
                EmptyInformation();


                if (keysize == 1)
                {
                    toGeneral.Visibility = Visibility.Visible;
                    strings = BinaryStrings(states[47], statesB[47]);
                    nrDiffBits = NoOfBitsFlipped(states[47], statesB[47]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = AvgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Final();
                        DisplayBinaryValues(states[47], statesB[47]);
                        ShowBitSequence(strings);
                        occurrence = CountOccurrence(differentBits);
                        ShowOccurence(occurrence);
                        lengthIdentSequence = LongestIdenticalSequence(differentBits);
                        lengthFlippedSequence = LongestFlippedSequence(differentBits);
                        ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                        SetColors();
                        setAngles(angle_1, angle_2);
                        SetToolTips();
                   }, null);
                }
                else
                {
                    strings = BinaryStrings(states[48], statesB[48]);
                    nrDiffBits = NoOfBitsFlipped(states[48], statesB[48]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = AvgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        PrintIntermediateStates(states, statesB);
                        DisplayBinaryValues(states[48], statesB[48]);
                        ShowBitSequence(strings);
                        occurrence = CountOccurrence(differentBits);
                        ShowOccurence(occurrence);
                        lengthIdentSequence = LongestIdenticalSequence(differentBits);
                        lengthFlippedSequence = LongestFlippedSequence(differentBits);
                        ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                        SetColors();
                        setAngles(angle_1, angle_2);
                        SetToolTips();
                    }, null);
                }
            }
            else
            {
                roundDES = 12;
                EcryptionProgress(roundDES);
                strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound13Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            ClearElements();
            ChangeRoundNr(13);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound13Button.Background = Brushes.Coral;
            EcryptionProgress(13);
            slide = 16;

            if (mode == 0)
            {
                Tuple<string, string> strings = BinaryStrings(states[52], statesB[52]);


                roundNumber = 14 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = NoOfBitsFlipped(states[52], statesB[52]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PrintIntermediateStates(states, statesB);
                    DisplayBinaryValues(states[52], statesB[52]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {
                roundDES = 13;
                EcryptionProgress(roundDES);
                Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound14Button_Click(object sender, RoutedEventArgs e)
        {
            int occurrence;

            ClearElements();
            ChangeRoundNr(14);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound14Button.Background = Brushes.Coral;
            EcryptionProgress(14);
            slide = 17;

            if (mode == 0)
            {
                Tuple<string, string> strings = BinaryStrings(states[55], statesB[55]);

                action = 1;
                toGeneral.Visibility = Visibility.Visible;
                int nrDiffBits = NoOfBitsFlipped(states[55], statesB[55]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                EmptyInformation();
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Final();
                    DisplayBinaryValues(states[55], statesB[55]);
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequence = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequence = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequence, lengthFlippedSequence, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
            else
            {

                roundDES = 14;
                EcryptionProgress(roundDES);
                Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
                ToStringArray(roundDES);

                int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = CalcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                int lengthFlippedSequenceDes;
                avgNrDiffBit = AvgNrperByte(nrDiffBits);
                EmptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    DisplayBinaryValuesDES();
                    ShowBitSequence(strings);
                    occurrence = CountOccurrence(differentBits);
                    ShowOccurence(occurrence);
                    lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                    lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                    ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                    SetColors();
                    setAngles(angle_1, angle_2);
                    SetToolTips();
                }, null);
            }
        }

        private void AfterRound15Button_Click(object sender, RoutedEventArgs e)
        {
            ClearElements();
            ChangeRoundNr(15);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound15Button.Background = Brushes.Coral;
            slide = 18;

            roundDES = 15;
            EcryptionProgress(roundDES);
            Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
            ToStringArray(roundDES);

            int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = CalcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            int lengthFlippedSequenceDes;
            int occurrence;
            avgNrDiffBit = AvgNrperByte(nrDiffBits);
            EmptyInformation();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                DisplayBinaryValuesDES();
                ShowBitSequence(strings);
                occurrence = CountOccurrence(differentBits);
                ShowOccurence(occurrence);
                lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                SetColors();
                setAngles(angle_1, angle_2);
                SetToolTips();
            }, null);
        }

        private void AfterRound16Button_Click(object sender, RoutedEventArgs e)
        {

            ClearElements();
            ChangeRoundNr(16);
            ShowElements();
            RemoveColors();
            RemoveBackground();

            afterRound16Button.Background = Brushes.Coral;
            slide = 19;

            toGeneral.Visibility = Visibility.Visible;

            roundDES = 16;
            EcryptionProgress(roundDES);
            Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);
            ToStringArray(roundDES);

            int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = CalcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            int lengthFlippedSequenceDes;
            int occurrence;
            avgNrDiffBit = AvgNrperByte(nrDiffBits);
            EmptyInformation();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                DisplayBinaryValuesDES();
                ShowBitSequence(strings);
                occurrence = CountOccurrence(differentBits);
                ShowOccurence(occurrence);
                lengthIdentSequenceDes = LongestIdenticalSequence(differentBits);
                lengthFlippedSequenceDes = LongestFlippedSequence(differentBits);
                ShowStatistics(nrDiffBits, lengthIdentSequenceDes, lengthFlippedSequenceDes, strings);
                SetColors();
                setAngles(angle_1, angle_2);
                SetToolTips();
            }, null);
        }

        public void Final()
        {
            List<TextBlock> tmp = CreateTxtBlockList(3);
            List<TextBlock> tmpB = CreateTxtBlockList(4);

            byte[] state = { 0 };
            byte[] stateB = { 0 };

            switch (keysize)
            {
                case 0:
                    state = ArrangeText(states[39]);
                    stateB = ArrangeText(statesB[39]);
                    break;
                case 1:
                    state = ArrangeText(states[47]);
                    stateB = ArrangeText(statesB[47]);
                    break;
                case 2:
                    state = ArrangeText(states[55]);
                    stateB = ArrangeText(statesB[55]);
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

        private void RadioButton1Checked(object sender, RoutedEventArgs e)
        {
            string strA = BinaryAsString(textA).Replace(" ", "");
            string strB = BinaryAsString(textB).Replace(" ", "");
            string firstHalf = strA.Substring(0, 32);
            string secondHalf = strA.Substring(32, 32);
            string firstHalfB = strB.Substring(0, 32);
            string secondHalfB = strB.Substring(32, 32);

            origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
            modTextDES.Text = string.Format("{0}{1}{2}", firstHalfB, Environment.NewLine, secondHalfB);

            string keyStrA = BinaryAsString(keyA).Replace(" ", "");
            string keyStrB = BinaryAsString(key).Replace(" ", "");
            string firstKeyHalf = keyStrA.Substring(0, 32);
            string secondKeyHalf = keyStrA.Substring(32, 32);
            string firstKeyHalfB = keyStrB.Substring(0, 32);
            string secondKeyHalfB = keyStrB.Substring(32, 32);

            origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalfB, Environment.NewLine, secondKeyHalfB);

            ColoringText();
            ColoringKey();
        }

        private void RadioButton2Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                Encoding encoding = Encoding.GetEncoding(437);

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString();
                    ((TextBlock)FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString();

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
                        ((TextBlock)FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString();
                        ((TextBlock)FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString();
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
                        ((TextBlock)FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)FindName("modKey192_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
                else
                {
                    int k = 1;
                    while (k <= 32)
                    {
                        ((TextBlock)FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)FindName("modKey256_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
            }
            else if (mode == 1)
            {

                string strA = DecimalAsString(textA);
                string strB = DecimalAsString(textB);
                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = DecimalAsString(keyA);
                string keyStrB = DecimalAsString(key);
                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;

                ColoringText();
                ColoringKey();

            }
            else
            {
                if (modifiedMsg.Text != "")
                {
                    originalMsg.Text = DecimalAsString(unchangedCipher);
                    modifiedMsg.Text = DecimalAsString(changedCipher);
                }
                else
                {
                    originalMsg.Text = DecimalAsString(unchangedCipher);
                }
            }
        }

        private void RadioButton3Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString("X2");
                    ((TextBlock)FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString("X2");
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
                        ((TextBlock)FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString("X2");
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
                        ((TextBlock)FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)FindName("modKey192_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }
                else
                {
                    int k = 1;
                    while (k <= 32)
                    {
                        ((TextBlock)FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)FindName("modKey256_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }

            }
            else if (mode == 1)
            {

                string strA = HexaAsString(textA);
                string strB = HexaAsString(textB);
                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = HexaAsString(keyA);
                string keyStrB = HexaAsString(key);
                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;

                ColoringText();
                ColoringKey();

            }
            else
            {

                if (modifiedMsg.Text != "")
                {
                    originalMsg.Text = HexaAsString(unchangedCipher);
                    modifiedMsg.Text = HexaAsString(changedCipher);
                }
                else
                {
                    originalMsg.Text = HexaAsString(unchangedCipher);
                }
            }
        }


        private void RadioText_Checked(object sender, RoutedEventArgs e)
        {
            string strA = Encoding.UTF8.GetString(unchangedCipher);
            if (modifiedMsg.Text != "")
            {
                string strB = Encoding.UTF8.GetString(changedCipher);
                originalMsg.Text = strA;
                modifiedMsg.Text = strB;
            }
            else
            {
                originalMsg.Text = strA;
            }
        }

        private void ClearTextEffect()
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
                        {
                            tb.TextEffects.Clear();
                        }

                        break;
                    case 1:

                        IEnumerable<TextBlock> textChilds192 = overviewAES192.Children.OfType<TextBlock>();

                        foreach (TextBlock tb in textChilds192)
                        {
                            tb.TextEffects.Clear();
                        }

                        break;
                    case 2:

                        IEnumerable<TextBlock> textChilds256 = overviewAES256.Children.OfType<TextBlock>();

                        foreach (TextBlock tb in textChilds256)
                        {
                            tb.TextEffects.Clear();
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private void ClearKeyEffect()
        {
            origKeyDES.TextEffects.Clear();
            modKeyDES.TextEffects.Clear();
        }

        public void Instructions()
        {
            StartCanvas.Visibility = Visibility.Hidden;
            slide = 0;
            avalancheVisualization.ProgressChanged(0, 1);

            if (mode == 0 || mode == 1)
            {
                InstructionsPrep.Visibility = Visibility.Visible;
            }
            else
            {
                InstructionsUnprep.Visibility = Visibility.Visible;
            }
        }

        public void ComparisonPane()
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
                    {
                        changeMsgAes.Visibility = Visibility.Visible;
                    }

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

                    ChangeTitle();

                    int a = 0;
                    int b = 128;
                    while (a < 64 && b < 192)
                    {

                        ((TextBlock)FindName("txt" + b)).Foreground = Brushes.Black;
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
                    {
                        changeMsgDes.Visibility = Visibility.Visible;
                    }
                    inputGridDES.Visibility = Visibility.Visible;
                    break;

                case 2:
                case 3:
                case 4:
                    othersGrid.Visibility = Visibility.Visible;
                    ChangeTitle();

                    if (mode == 3)
                    {
                        radioText.Visibility = Visibility.Visible;
                    }

                    break;

                default:
                    break;

            }

        }

        public void RemoveElements()
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

                ClearColors();
                ClearKeyColors();
                RemoveBackground();
                ReadjustStats();

                bitRepresentationSV.ScrollToHorizontalOffset(0.0);
                canModify = false;
                canModifyDES = false;
                slide = 0;

                if (canStop)
                {
                    UpdateDataColor();
                    List<TextBlock> tmp = CreateTxtBlockList(6);

                    foreach (TextBlock txtB in tmp)
                    {
                        txtB.Foreground = Brushes.Black;
                    }

                    int k = 33;
                    int l = 49;
                    int i = 1;
                    while (k <= 48 && l <= 64)
                    {
                        ((TextBlock)FindName("initStateTxtBlock" + k)).Text = string.Empty;
                        ((TextBlock)FindName("initStateTxtBlock" + l)).Text = string.Empty;
                        i++;
                        k++;
                        l++;
                    }

                    int j = 1;

                    while (j <= 24)
                    {
                        ((TextBlock)FindName("modKey192_" + j)).Text = string.Empty;
                        j++;
                    }

                    int m = 1;

                    while (m <= 32)
                    {
                        ((TextBlock)FindName("modKey256_" + m)).Text = string.Empty;
                        m++;
                    }

                    List<TextBlock> tmpDES = CreateTxtBlockList(7);
                    foreach (TextBlock txtB in tmpDES)
                    {
                        txtB.Foreground = Brushes.Black;
                    }
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
            canStop = false;
            StartCanvas.Visibility = Visibility.Visible;
        }

        public void AdjustStats()
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

        public void ReadjustStats()
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

        public void Comparison()
        {
            EmptyInformation();
            AdjustStats();
            bitsData.Visibility = Visibility.Visible;
            avalancheVisualization.ProgressChanged(1, 1);
            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;

            if (mode != 3)
            {
                Cb1.Visibility = Visibility.Visible;
                Cb2.Visibility = Visibility.Visible;
                Tuple<string, string> strings = BinaryStrings(unchangedCipher, changedCipher);
                int bitsFlipped = NoOfBitsFlipped(unchangedCipher, changedCipher);
                int lengthIdentSequence;
                int lengthFlippedSequence;
                avalanche = CalcAvalancheEffect(bitsFlipped, strings);
                double angle_1 = flippedBitsPiece.calculateAngle(bitsFlipped, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - bitsFlipped, strings);
                ShowBitSequence(strings);
                lengthIdentSequence = LongestIdenticalSequence(differentBits);
                lengthFlippedSequence = LongestFlippedSequence(differentBits);
                ShowStatistics(bitsFlipped, lengthIdentSequence, lengthFlippedSequence, strings);
                SetColors();
                setAngles(angle_1, angle_2);
                SetToolTips();
            }
            else
            {
                Cbclass1.Visibility = Visibility.Visible;
                Cbclass2.Visibility = Visibility.Visible;
                Tuple<string, string> strings = BinaryStrings(unchangedCipher, changedCipher);
                int nrBytesFlipped = BytesFlipped();
                avalanche = AvalancheEffectBytes(nrBytesFlipped);
                double angle_1 = flippedBitsPiece.calculateAngleClassic(nrBytesFlipped, unchangedCipher);
                double angle_2 = unflippedBitsPiece.calculateAngleClassic(unchangedCipher.Length - nrBytesFlipped, unchangedCipher);
                ShowBitSequence(strings);
                int LIBS = LongestIdentSequenceBytes();
                int LFBS = LongestFlippedSequenceBytes();
                ClassicStats(nrBytesFlipped, LIBS, LFBS);
                SetColors();
                setAngles(angle_1, angle_2);
                SetToolTips();
            }
        }

        public void ClassicStats(int bytesFlipped, int longestLength, int longestflipped)
        {
            stats1.Inlines.Add(new Run(" " + bytesFlipped.ToString())
            {
                Foreground = Brushes.Red,
                FontWeight = FontWeights.DemiBold
            });

            if (bytesFlipped > 1 || bytesFlipped == 0)
            {
                stats1.Inlines.Add(new Run(string.Format(Properties.Resources.StatsClassicBullet1_Plural, changedCipher.Length, avalanche)));
            }
            else
            {
                stats1.Inlines.Add(new Run(string.Format(Properties.Resources.StatsClassicBullet1, changedCipher.Length, avalanche)));
            }

            stats2.Inlines.Add(new Run(string.Format(Properties.Resources.StatsClassicBullet2, longestLength.ToString(), sequencePosition)));
            stats3.Inlines.Add(new Run(string.Format(Properties.Resources.StatsClassicBullet3, longestflipped.ToString(), flippedSeqPosition)));
        }

        public int BytesFlipped()
        {
            int count = 0;
            for (int i = 0; i < changedCipher.Length; i++)
            {
                if (changedCipher[i] != unchangedCipher[i])
                {
                    count++;
                }
            }
            return count;
        }
        public double AvalancheEffectBytes(int bytesFlipped)
        {
            double avalancheEffect = ((double)bytesFlipped / unchangedCipher.Length) * 100;
            double roundUp = Math.Round(avalancheEffect, 1, MidpointRounding.AwayFromZero);
            return roundUp;
        }

        public int LongestIdentSequenceBytes()
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

        public int LongestFlippedSequenceBytes()
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
                    {
                        txtBlock.Foreground = Brushes.Red;
                    }
                    else
                    {
                        txtBlock.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    txtBlock.Text = "0";

                    if (txtBlock.Foreground != Brushes.Red)
                    {
                        txtBlock.Foreground = Brushes.Red;
                    }
                    else
                    {
                        txtBlock.Foreground = Brushes.Black;
                    }
                }
            }
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (inputInBits.IsVisible)
            {
                arrow1.Visibility = Visibility.Visible;
                if (keysize == 2)
                {
                    arrow3.Visibility = Visibility.Visible;
                }
                else
                {
                    arrow2.Visibility = Visibility.Visible;
                }
            }
            else
            {
                arrow1.Visibility = Visibility.Hidden;
                if (keysize == 2)
                {
                    arrow3.Visibility = Visibility.Hidden;
                }
                else
                {
                    arrow2.Visibility = Visibility.Hidden;
                }
            }
            if (modifiedInitialStateGrid.IsVisible || inputInBits.IsVisible)
            {
                EcryptionProgress(-1);
                canModify = true;
            }
            if (afterRoundsGrid.IsVisible)
            {
                canModify = false;
            }
        }

        private void ModifyOthers(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (othersGrid.IsVisible)
            {
                EcryptionProgress(0);
                canModifyOthers = true;
            }

            if (InstructionsUnprep.IsVisible)
            {
                canModifyOthers = false;
            }
        }

        private void Modify(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!modificationGridDES.IsVisible || !inputGridDES.IsVisible)
            {
                canModifyDES = false;
            }
            if (modificationGridDES.IsVisible || inputGridDES.IsVisible)
            {
                EcryptionProgress(-1);
                canModifyDES = true;
            }
        }

        private void OnTitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 1)
            {
                changeMsgDes.Visibility = Visibility.Hidden;
            }
            else
            {
                changeMsgAes.Visibility = Visibility.Hidden;
            }
            doneButton.Visibility = Visibility.Visible;
            clearButton.Visibility = Visibility.Visible;
            instructionsTxtBlock2.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mode == 1)
            {
                changeMsgDes.Visibility = Visibility.Visible;
            }
            else
            {
                changeMsgAes.Visibility = Visibility.Visible;
            }

            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;
        }

        public void ColorOverviewText(TextBlock txtB, List<byte> pos)
        {
            byte[] changePos = pos.ToArray();
            txtB.TextEffects.Clear();
            for (byte i = 0; i < changePos.Length; i++)
            {
                TextEffect te = new TextEffect
                {
                    PositionStart = changePos[i],
                    Foreground = Brushes.Red,
                    PositionCount = 1
                };
                txtB.TextEffects.Add(te);
            }
        }

        public byte[][] LoadInfo()
        {
            byte[] byteArr = ArrangeColumn(statesB[0]);
            byte[] byteArr1 = ArrangeColumn(statesB[4]);
            byte[] byteArr2 = ArrangeColumn(statesB[8]);
            byte[] byteArr3 = ArrangeColumn(statesB[12]);
            byte[] byteArr4 = ArrangeColumn(statesB[16]);
            byte[] byteArr5 = ArrangeColumn(statesB[20]);
            byte[] byteArr6 = ArrangeColumn(statesB[24]);
            byte[] byteArr7 = ArrangeColumn(statesB[28]);
            byte[] byteArr8 = ArrangeColumn(statesB[32]);
            byte[] byteArr9 = ArrangeColumn(statesB[36]);
            byte[] byteArr10 = new byte[16];
            byte[] byteArr11 = new byte[16];
            byte[] byteArr12 = new byte[16];
            byte[] byteArr13 = new byte[16];
            byte[] byteArr14 = new byte[16];

            switch (keysize)
            {
                case 0:
                    byteArr10 = ArrangeColumn(statesB[39]);
                    break;
                case 1:
                    byteArr10 = ArrangeColumn(statesB[40]);
                    byteArr11 = ArrangeColumn(statesB[44]);
                    byteArr12 = ArrangeColumn(statesB[47]);
                    break;
                case 2:
                    byteArr10 = ArrangeColumn(statesB[40]);
                    byteArr11 = ArrangeColumn(statesB[44]);
                    byteArr12 = ArrangeColumn(statesB[48]);
                    byteArr13 = ArrangeColumn(statesB[52]);
                    byteArr14 = ArrangeColumn(statesB[55]);
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
            ClearTextEffect();
            generalViewAES.Visibility = Visibility.Visible;

            byte[][] roundsInfo = LoadInfo();

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
                    {
                        strList.Add(b.ToString("X2"));
                    }
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
                            List<int> changePos = ChangePosition();
                            TextEffect te = new TextEffect
                            {
                                PositionStart = changePos[k],
                                Foreground = Brushes.Red,
                                PositionCount = 2
                            };
                            enumerator2.Current.TextEffects.Add(te);
                        }
                    }
                }

                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {
                    if (states[39][k] != statesB[39][k])
                    {
                        List<int> changePos = ChangePosition();
                        TextEffect te = new TextEffect
                        {
                            PositionStart = changePos[k],
                            Foreground = Brushes.Red,
                            PositionCount = 2
                        };
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
                    {
                        strList.Add(b.ToString("X2"));
                    }

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
                            List<int> changePos = ChangePosition();
                            TextEffect te = new TextEffect
                            {
                                PositionStart = changePos[k],
                                Foreground = Brushes.Red,
                                PositionCount = 2
                            };
                            enumerator2.Current.TextEffects.Add(te);
                        }
                    }
                }

                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {
                    if (states[47][k] != statesB[47][k])
                    {


                        List<int> changePos = ChangePosition();
                        TextEffect te = new TextEffect
                        {
                            PositionStart = changePos[k],
                            Foreground = Brushes.Red,
                            PositionCount = 2
                        };
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
                    {
                        strList.Add(b.ToString("X2"));
                    }
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

                            List<int> changePos = ChangePosition();
                            TextEffect te = new TextEffect
                            {
                                PositionStart = changePos[k],
                                Foreground = Brushes.Red,
                                PositionCount = 2
                            };
                            enumerator2.Current.TextEffects.Add(te);
                        }
                    }
                }

                enumerator2.MoveNext();

                for (byte k = 0; k < statesB[k].Length; k++)
                {
                    if (states[55][k] != statesB[55][k])
                    {
                        List<int> changePos = ChangePosition();
                        TextEffect te = new TextEffect
                        {
                            PositionStart = changePos[k],
                            Foreground = Brushes.Red,
                            PositionCount = 2
                        };
                        enumerator2.Current.TextEffects.Add(te);
                    }
                }
            }
        }

        public List<int> ChangePosition()
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
                Tuple<string, string> strings = BinaryStrings(states[4], statesB[4]);

                for (int desRound = 0; desRound < 17; desRound++)
                {
                    ToStringArray(desRound);
                    int nrDiffBits = NoOfBitsFlipped(seqA, seqB);
                    avalanche = CalcAvalancheEffect(nrDiffBits, strings);

                    percentages.Add(avalanche);
                }

                int i = 0;

                foreach (double percentage in percentages)
                {
                    ((TextBlock)FindName(string.Format("percent{0}", i))).Text = string.Format("{0} %", percentage.ToString());
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

                            strings = BinaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = NoOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = CalcAvalancheEffect(nrDiffBits, strings);

                            percentages.Add(avalanche);
                        }

                        strings = BinaryStrings(states[39], statesB[39]);
                        int nrDiffBits2 = NoOfBitsFlipped(states[39], statesB[39]);
                        avalanche = CalcAvalancheEffect(nrDiffBits2, strings);

                        percentages.Add(avalanche);

                        int i = 0;

                        foreach (double dl in percentages)
                        {
                            ((TextBlock)FindName(string.Format("percentAes{0}", i))).Text = string.Format(
                                "{0} %", dl.ToString());
                            i++;
                        }
                        break;

                    case 1:
                        Tuple<string, string> strings2;
                        for (int aesRound = 0; aesRound <= 44; aesRound += 4)
                        {

                            strings2 = BinaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = NoOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = CalcAvalancheEffect(nrDiffBits, strings2);

                            percentages.Add(avalanche);
                        }

                        strings2 = BinaryStrings(states[47], statesB[47]);
                        int nrDiffBits192 = NoOfBitsFlipped(states[47], statesB[47]);
                        avalanche = CalcAvalancheEffect(nrDiffBits192, strings2);
                        percentages.Add(avalanche);

                        int j = 0;
                        foreach (double percentage in percentages)
                        {
                            ((TextBlock)FindName(string.Format("percentAes192_{0}", j))).Text =
                                string.Format("{0} %", percentage.ToString());
                            j++;
                        }

                        break;

                    case 2:
                        Tuple<string, string> strings3;

                        for (int aesRound = 0; aesRound <= 52; aesRound += 4)
                        {
                            strings3 = BinaryStrings(states[aesRound], statesB[aesRound]);
                            int nrDiffBits = NoOfBitsFlipped(states[aesRound], statesB[aesRound]);
                            avalanche = CalcAvalancheEffect(nrDiffBits, strings3);
                            percentages.Add(avalanche);
                        }

                        strings3 = BinaryStrings(states[55], statesB[55]);
                        int nrDiffBits256 = NoOfBitsFlipped(states[55], statesB[55]);
                        avalanche = CalcAvalancheEffect(nrDiffBits256, strings3);

                        percentages.Add(avalanche);

                        int k = 0;

                        foreach (double percentage in percentages)
                        {
                            ((TextBlock)FindName(string.Format("percentAes256_{0}", k))).Text = string.Format("{0} %", percentage.ToString());
                            k++;
                        }
                        break;

                    default:
                        break;
                }
            }


        }

        public void ShowGeneralOverview()
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
                    {
                        tmp.Add(j);
                    }

                    ColorOverviewText(enumerator.Current, tmp);
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
                    {
                        tmp.Add(j);
                    }
                    ColorOverviewText(enumerator.Current, tmp);
                }
            }

        }

        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            slide = 0;
            RemoveBackground();

            overviewButton.Background = Brushes.Coral;
            avalancheVisualization.ProgressChanged(1, 1);

            toGeneral.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;

            ClearElements();

            if (mode == 1)
            {

                extraordinaryOccur.Visibility = Visibility.Hidden;
                bitGridDES.Visibility = Visibility.Hidden;
                ShowGeneralOverview();
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
                ClearColors();
                ClearKeyColors();

                if (ChangesMadeButton.IsEnabled)
                {
                    ChangesMadeButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                InstructionsUnprep.Visibility = Visibility.Hidden;

                if (!string.IsNullOrEmpty(TB2.Text))
                {
                    Comparison();
                }
            }

            ComparisonPane();
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
                        {
                            overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        else if (keysize == 1 || keysize == 2)
                        {
                            afterRound11Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                    if (mode == 1)
                    {
                        afterRound11Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    break;
                case 14:
                    afterRound12Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 15:

                    if (mode == 0)
                    {
                        if (keysize == 1)
                        {
                            overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }

                        if (keysize == 2)
                        {
                            afterRound13Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                    if (mode == 1)
                    {
                        afterRound13Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    break;
                case 16:
                    afterRound14Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case 17:
                    if (mode == 0)
                    {
                        overviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    if (mode == 1)
                    {
                        afterRound15Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
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
            RemoveBackground();

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
            Instructions();
        }

        private void ChangesMadeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (newText != null)
            {
                doneButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else
            {
                SetAndLoadButtons();
                ColoringText();
                ColoringKey();
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
                default:
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
                default:
                    break;
            }
        }
    }
}
#endregion