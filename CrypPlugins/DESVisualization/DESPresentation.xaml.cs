
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrypTool.DESVisualization
{
    /// <summary>
    /// Interaction logic for DESPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.DESVisualization.Properties.Resources")]
    public partial class DESPresentation : UserControl
    {

        // Constructor
        public DESPresentation(DESVisualization des)
        {
            InitializeComponent();

            desVisualization = des;
            playTimer.Tick += PlayTimer_Tick;
            playTimer.Interval = TimeSpan.FromSeconds(2);
            playTimer.IsEnabled = false;
            GetDiffusionBoxes();
            SetInitialState();
        }

        #region Attributes
        private int nextStepCounter;
        public int nextScreenID;
        private int roundCounter;

        private bool keyScheduleIsRunning;
        private bool desRoundsIsRunning;
        private bool binFinal;

        private IEnumerable<CheckBox> diffusionBoxes;
        private readonly SolidColorBrush greenBrush = new SolidColorBrush(Colors.LightGreen);
        private readonly SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Khaki);
        private readonly SolidColorBrush buttonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDE0E6"));

        public DispatcherTimer playTimer = new DispatcherTimer();
        public DESImplementation encOriginal;
        private DESImplementation encDiffusion;
        private bool diffusionIsActive;
        private readonly DESVisualization desVisualization;

        public double progress;

        #endregion Attributes

        #region Button-Methods

        // Button Row 1

        private void ShiftCButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 7;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void ShiftDButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 7;
            nextStepCounter = 3;
            ExecuteNextStep();
        }

        private void PC2Button_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 8;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void ExpansionButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 13;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void KeyAdditionButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 14;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void SBoxButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 15;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void PermutationButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 16;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void RoundAdditionButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 14;
            nextStepCounter = 3;
            ExecuteNextStep();
        }

        // Button Row 2

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            switch (nextScreenID)
            {
                case 1:
                    if (nextStepCounter == 2)
                    {
                        ShowInputDataScreen();
                    }
                    else if (nextStepCounter == 3)
                    {
                        roundCounter = 16;
                        ShowKeyScheduleScreen(5);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowFPScreen(1);
                    }
                    break;
                case 2:
                    if (nextStepCounter == 2)
                    {
                        ShowChapterScreen(1);
                    }
                    else if (nextStepCounter == 1)
                    {
                        ShowIntroScreen();
                    }

                    break;
                case 3: ShowInfoScreen(1); break;
                case 4: ShowInfoScreen(2); break;
                case 5:
                    if (nextStepCounter == 1)
                    {
                        ShowStructureScreen();
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowChapterScreen(2);
                    }

                    break;
                case 6:
                    if (nextStepCounter == 1 && roundCounter == 0)
                    {
                        ShowPC1Screen(1);
                    }
                    else if (nextStepCounter == 1 && roundCounter != 0)
                    {
                        ShowKeyScheduleScreen(5);
                    }
                    else if (nextStepCounter == 2 && roundCounter == 1)
                    {
                        ShowChapterScreen(3);
                        ShowPC1Screen(1);
                    }
                    else if (nextStepCounter == 2 && roundCounter != 1)
                    {
                        roundCounter = roundCounter - 2;
                        ShowKeyScheduleScreen(1);
                        ShowRoundKeyDataScreen(1);
                    }
                    else if (nextStepCounter == 3)
                    {
                        ShowShiftScreen(1);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowShiftScreen(3);
                    }
                    else if (nextStepCounter == 5)
                    {
                        ShowPC2Screen(1);
                    }
                    break;
                case 7:
                    if (nextStepCounter == 1)
                    {
                        roundCounter--;
                        ShowKeyScheduleScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowKeyScheduleScreen(2);
                    }
                    else if (nextStepCounter == 3)
                    {
                        ShowShiftScreen(1);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowKeyScheduleScreen(3);
                    }

                    break;
                case 8:
                    if (nextStepCounter == 1)
                    {
                        ShowShiftScreen(3);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowKeyScheduleScreen(4);
                    }

                    break;
                case 9:
                    if (nextStepCounter == 1)
                    {
                        ShowPC2Screen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowFPScreen(1);
                    }

                    break;
                case 10:
                    if (nextStepCounter == 1)
                    {
                        roundCounter = 0;
                        ShowKeyScheduleScreen(1);
                        roundCounter = 15;
                        ShowKeyScheduleScreen(1);
                        ShowRoundKeyDataScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowChapterScreen(3);
                    }
                    break;
                case 11:
                    if (nextStepCounter == 1 && roundCounter == 0)
                    {
                        ShowIPScreen(1);
                    }
                    else if (nextStepCounter == 1 && roundCounter != 0)
                    {
                        ShowDESRoundScreen(4);
                    }
                    else if (nextStepCounter == 2 && roundCounter == 1)
                    {
                        ShowFPScreen(1);
                        ShowIPScreen(1);
                    }
                    else if (nextStepCounter == 2 && roundCounter != 1)
                    {
                        roundCounter = roundCounter - 2;
                        ShowDESRoundScreen(1);
                        ShowRoundDataScreen(1);
                    }
                    else if (nextStepCounter == 3)
                    {
                        ShowPScreen(1);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowXORScreen(3);
                    }

                    break;
                case 12:
                    if (nextStepCounter == 1)
                    {
                        roundCounter--;
                        ShowDESRoundScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowDESRoundScreen(2);
                    }
                    else if (nextStepCounter == 3)
                    {
                        ShowExpansionScreen(1);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowXORScreen(1);
                    }
                    else if (nextStepCounter == 5)
                    {
                        nextStepCounter = 39;
                        ShowSBoxScreen(nextStepCounter);
                    }
                    else if (nextStepCounter == 6)
                    {
                        ShowPScreen(1);
                    }
                    break;
                case 13:
                    if (nextStepCounter == 1)
                    {
                        ShowRoundFunctionScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowRoundFunctionScreen(2);
                    }

                    break;
                case 14:
                    if (nextStepCounter == 1)
                    {
                        ShowExpansionScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowRoundFunctionScreen(3);
                    }
                    else if (nextStepCounter == 3)
                    {
                        ShowRoundFunctionScreen(6);
                    }
                    else if (nextStepCounter == 4)
                    {
                        ShowDESRoundScreen(3);
                    }

                    break;
                case 15:
                    if (nextStepCounter == 1)
                    {
                        ShowXORScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowRoundFunctionScreen(4);
                    }
                    else if (nextStepCounter > 2)
                    {
                        nextStepCounter = nextStepCounter - 2;
                        ShowSBoxScreen(nextStepCounter);
                    }

                    break;
                case 16:
                    if (nextStepCounter == 1)
                    {
                        ShowSBoxScreen(1);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowRoundFunctionScreen(5);
                    }

                    break;
                case 17:
                    if (nextStepCounter == 1)
                    {
                        ShowXORScreen(3);
                    }
                    else if (nextStepCounter == 2)
                    {
                        ShowChapterScreen(4);
                    }

                    break;
                case 18:
                    if (nextStepCounter == 1)
                    {
                        ShowDESRoundScreen(4);
                    }
                    else if (nextStepCounter == 2)
                    {
                        roundCounter = 0;
                        ShowDESRoundScreen(1);
                        roundCounter = 15;
                        ShowDESRoundScreen(1);
                        ShowRoundDataScreen(1);
                    }
                    break;
                case 19: ShowRoundKeyDataScreen(2); break;
                case 20: ShowRoundDataScreen(2); break;
                default: break;

            }
        }

        private void AutoTButton_Click(object sender, RoutedEventArgs e)
        {
            if (AutoTButton.IsChecked == true)
            {
                playTimer.Start();
            }
            else
            {
                playTimer.Stop();
            }
        }

        private void AutoSpeedSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            playTimer.Interval = TimeSpan.FromSeconds(4.5 - AutoSpeedSlider.Value);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteNextStep();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (roundCounter > 0 && roundCounter < 16)
            {
                if (desRoundsIsRunning)
                {
                    ShowDESRoundScreen(1);
                }
                else if (keyScheduleIsRunning)
                {
                    ShowKeyScheduleScreen(1);
                }
            }
            else if (roundCounter == 16)
            {
                if (desRoundsIsRunning)
                {
                    ShowFPScreen(1);
                }
                else if (keyScheduleIsRunning)
                {
                    ShowChapterScreen(3);
                }
                else
                {
                    ExecuteNextStep();
                }
            }
            else
            {
                if (nextStepCounter != 1)
                {
                    switch (nextScreenID)
                    {
                        case 2: ExecuteNextStep(); break;
                        case 5: ExecuteNextStep(); break;
                        case 10: ExecuteNextStep(); break;
                        case 18: ExecuteNextStep(); break;
                    }
                }
                ExecuteNextStep();
            }
        }

        // Button Row 3

        private void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 1;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void DataButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 3;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void PC1Button_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 5;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void KeyScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            roundCounter = 0;
            nextScreenID = 6;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void IPButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 10;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void DESButton_Click(object sender, RoutedEventArgs e)
        {
            if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            roundCounter = 0;
            nextScreenID = 11;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void FPButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 18;
            nextStepCounter = 1;
            ExecuteNextStep();
        }

        private void SummaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            nextScreenID = 1;
            nextStepCounter = 4;
            ExecuteNextStep();
        }

        // Button Row 4

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {

            roundCounter = Grid.GetColumn((Button)sender) - 1;
            if (desRoundsIsRunning)
            {
                nextScreenID = 11;
            }
            else
            {
                nextScreenID = 6;
            }

            nextStepCounter = 1;
            ExecuteNextStep();
        }

        // Button in SBoxScreen

        private void SBoxJumpButton_Click(object sender, RoutedEventArgs e)
        {
            nextScreenID = 15;
            nextStepCounter = 40;
            ExecuteNextStep();
        }

        // Button in FinalScreen

        private void FinalSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (binFinal)
            {
                FinalMessage.FontSize = 10.667;
                FinalKey.FontSize = 10.667;
                FinalCiphertextRec.Width = 123;
                Canvas.SetLeft(FinalCiphertextRec, 164);
                if (diffusionIsActive)
                {
                    FinalCiphertext.Text = BinStringToHexString(encDiffusion.ciphertext);
                    ColorText(FinalCiphertext, CompareStrings(BinStringToHexString(encOriginal.ciphertext), FinalCiphertext.Text));

                    FinalMessage.Text = BinStringToHexString(encDiffusion.message);
                    ColorText(FinalMessage, CompareStrings(BinStringToHexString(encOriginal.message), FinalMessage.Text));
                    FinalKey.Text = BinStringToHexString(encDiffusion.key);
                    ColorText(FinalKey, CompareStrings(BinStringToHexString(encOriginal.key), FinalKey.Text));
                }
                else
                {
                    FinalCiphertext.Text = BinStringToHexString(encOriginal.ciphertext);
                    FinalMessage.Text = BinStringToHexString(encOriginal.message);
                    FinalKey.Text = BinStringToHexString(encOriginal.key);
                }
            }
            else
            {
                FinalMessage.FontSize = 8;
                FinalKey.FontSize = 8;
                FinalCiphertextRec.Width = 377;
                Canvas.SetLeft(FinalCiphertextRec, 36);
                if (diffusionIsActive)
                {
                    FinalCiphertext.Text = encDiffusion.ciphertext;
                    ColorText(FinalCiphertext, CompareStrings(encOriginal.ciphertext, encDiffusion.ciphertext));

                    FinalMessage.Text = encDiffusion.message;
                    ColorText(FinalMessage, CompareStrings(encOriginal.message, FinalMessage.Text));
                    FinalKey.Text = encDiffusion.key;
                    ColorText(FinalKey, CompareStrings(encOriginal.key, FinalKey.Text));
                }
                else
                {
                    FinalCiphertext.Text = encOriginal.ciphertext;
                    FinalMessage.Text = encOriginal.message;
                    FinalKey.Text = encOriginal.key;
                }
            }
            binFinal = !binFinal;
        }

        // Buttons in DataScreen

        private void DiffTButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffTButton.IsChecked == true)
            {
                ShowDiffusionBoxes(true);
                DiffOkButton.Visibility = Visibility.Visible;
                DiffClearButton.Visibility = Visibility.Visible;
                DiffTButton.Content = Properties.Resources.DiffusionVisualizerHide;
            }
            else
            {
                ShowDiffusionBoxes(false);
                DiffOkButton.Visibility = Visibility.Hidden;
                DiffClearButton.Visibility = Visibility.Hidden;
                DiffInfoLabel.Visibility = Visibility.Hidden;
                DiffTButton.Content = Properties.Resources.DiffusionVisualizer;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            DiffInfoLabel.Visibility = Visibility.Visible;
            CheckBox box = (CheckBox)sender;
            int pos = Grid.GetColumn(box) - 1;
            if (box.IsChecked == true)
            {
                if (Grid.GetRow(box) == 9)
                {
                    ColorTextSingle(DataKey, (byte)(pos));
                    SwitchStringBit(DataKey, pos);
                    if ((pos + 1) % 8 == 0)
                    {
                        desVisualization.GuiLogMessage("A parity bit in the key was flipped. (Bit " + (pos + 1) + ")", PluginBase.NotificationLevel.Info);
                    }
                }
                else
                {
                    ColorTextSingle(DataMessage, (byte)(pos));
                    SwitchStringBit(DataMessage, pos);
                }
            }
            else
            {

                if (Grid.GetRow(box) == 9)
                {
                    SwitchStringBit(DataKey, pos);
                    TextEffect tmp = null;
                    TextEffectCollection.Enumerator enumerator = DataKey.TextEffects.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.PositionStart == pos)
                        {
                            tmp = enumerator.Current;
                        }
                    }
                    DataKey.TextEffects.Remove(tmp);
                    if ((pos + 1) % 8 == 0)
                    {
                        desVisualization.GuiLogMessage("A parity bit in the key was flipped. (Bit " + (pos + 1) + ")", PluginBase.NotificationLevel.Info);
                    }

                }
                else
                {
                    SwitchStringBit(DataMessage, pos);
                    TextEffect tmp = null;
                    TextEffectCollection.Enumerator enumerator = DataMessage.TextEffects.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.PositionStart == pos)
                        {
                            tmp = enumerator.Current;
                        }
                    }
                    DataMessage.TextEffects.Remove(tmp);
                }
            }
        }

        private void DiffClearButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.IsChecked = false;
            }

            DataKey.TextEffects.Clear();
            DataMessage.TextEffects.Clear();
            DataKey.Text = encOriginal.key;
            DataMessage.Text = encOriginal.message;

            DiffusionActiveLabel.Visibility = Visibility.Hidden;
            diffusionIsActive = false;
        }

        private void DiffOKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataKey.Text.Equals(encOriginal.key) || !DataMessage.Text.Equals(encOriginal.message))
            {
                byte[] key = StringToByteArray(DataKey.Text);
                byte[] msg = StringToByteArray(DataMessage.Text);
                encDiffusion = new DESImplementation(key, msg);
                encDiffusion.DES();
                diffusionIsActive = true;
                DiffusionActiveLabel.Visibility = Visibility.Visible;
            }
            else
            {
                diffusionIsActive = false;
                DiffusionActiveLabel.Visibility = Visibility.Hidden;
            }

            //hide Diffusion-Functionality
            ShowDiffusionBoxes(false);
            DiffOkButton.Visibility = Visibility.Hidden;
            DiffClearButton.Visibility = Visibility.Hidden;
            DiffInfoLabel.Visibility = Visibility.Hidden;
            DiffTButton.IsChecked = false;
            DiffTButton.Content = Properties.Resources.DiffusionVisualizer;
        }

        #endregion Button-Methods

        #region Screen-Methods (Show)

        public void ShowIntroScreen()
        {
            ResetAllScreens();
            IntroScreen.Visibility = Visibility.Visible;
            PrevButton.IsEnabled = false;
            ClearButtonsColor(false);
            nextScreenID = 1;
            nextStepCounter = 1;
        }

        public void ShowInfoScreen(int step)
        {
            ResetAllScreens();
            progress = 0;
            ClearButtonsColor(false);
            IntroButton.Background = buttonBrush;
            InfoScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;

            if (step == 1)
            {
                Title.Content = Properties.Resources.Background;
                HistoryText.Visibility = Visibility.Visible;
                nextScreenID = 2;
                nextStepCounter = 2;
            }
            else if (step == 2)
            {
                Title.Content = Properties.Resources.GeneralInformation;
                InfoText.Visibility = Visibility.Visible;
                nextScreenID = 3;
                nextStepCounter = 1;
            }
        }

        public void ShowChapterScreen(int step)
        {
            ResetAllScreens();
            ChapterScreen.Visibility = Visibility.Visible;
            switch (step)
            {
                case 1:
                    ChapterLabel.Content = Properties.Resources.Introduction;
                    ClearButtonsColor(false);
                    IntroButton.Background = buttonBrush;
                    nextScreenID = 2;
                    nextStepCounter = 1;
                    progress = 0;
                    break;
                case 2:
                    ChapterLabel.Content = Properties.Resources.KeySchedule;
                    ChapterLabel.VerticalAlignment = VerticalAlignment.Top;
                    ChapterTextKS.Visibility = Visibility.Visible;
                    ClearButtonsColor(false);
                    nextScreenID = 5;
                    nextStepCounter = 1;
                    progress = 0.0625;
                    break;
                case 3:
                    ChapterLabel.Content = Properties.Resources.DESEncryption;
                    ChapterLabel.VerticalAlignment = VerticalAlignment.Top;
                    ChapterTextDES.Visibility = Visibility.Visible;
                    nextScreenID = 10;
                    nextStepCounter = 1;
                    roundCounter = 0;
                    ActivateRoundButtons(false);
                    keyScheduleIsRunning = false;
                    ClearButtonsColor(true);
                    ClearButtonsColor(false);
                    ShiftDButton.Visibility = Visibility.Hidden;
                    ShiftCButton.Visibility = Visibility.Hidden;
                    PC2Button.Visibility = Visibility.Hidden;
                    SkipButton.Content = Properties.Resources.SkipStep;
                    progress = 0.4875;
                    break;
                case 4:
                    ChapterLabel.Content = Properties.Resources.Summary;
                    ClearButtonsColor(false);
                    SummaryButton.Background = buttonBrush;
                    nextScreenID = 9;
                    nextStepCounter = 2;
                    roundCounter = 16;
                    progress = 0.9375;
                    break;
            }
        }

        public void ShowFinalScreen()
        {
            ResetAllScreens();
            progress = 1;
            AutoTButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            SkipButton.IsEnabled = false;
            binFinal = true;
            FinalScreen.Visibility = Visibility.Visible;
            if (diffusionIsActive)
            {
                FinalCiphertext.Text = encDiffusion.ciphertext;
                ColorText(FinalCiphertext, CompareStrings(encOriginal.ciphertext, encDiffusion.ciphertext));

                FinalMessage.Text = encDiffusion.message;
                ColorText(FinalMessage, CompareStrings(encOriginal.message, FinalMessage.Text));
                FinalKey.Text = encDiffusion.key;
                ColorText(FinalKey, CompareStrings(encOriginal.key, FinalKey.Text));
            }
            else
            {
                FinalCiphertext.Text = encOriginal.ciphertext;
                FinalMessage.Text = encOriginal.message;
                FinalKey.Text = encOriginal.key;
            }
            nextScreenID = 20;
            nextStepCounter = 1;
            playTimer.Stop();
            AutoTButton.IsChecked = false;

        }

        public void ShowInputDataScreen()
        {
            ResetAllScreens();
            progress = 0;
            InputDataScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.InputData;

            if (diffusionIsActive == false)
            {
                IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator.Current.IsChecked = false;
                }

                DataKey.TextEffects.Clear();
                DataMessage.TextEffects.Clear();
                if (encOriginal != null)
                {
                    DataKey.Text = encOriginal.key;
                    DataMessage.Text = encOriginal.message;
                }
            }
            else
            {
                DataKey.TextEffects.Clear();
                DataMessage.TextEffects.Clear();
                List<byte> messageList = CompareStrings(encDiffusion.message, encOriginal.message);
                List<byte> keyList = CompareStrings(encDiffusion.key, encOriginal.key);
                IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CheckBox tmp = enumerator.Current;
                    if (Grid.GetRow(tmp) == 4)
                    {
                        if (messageList.Contains((byte)(Grid.GetColumn(tmp) - 1)))
                        {
                            tmp.IsChecked = true;
                        }
                        else
                        {
                            tmp.IsChecked = false;
                        }
                    }
                    else
                    {
                        if (keyList.Contains((byte)(Grid.GetColumn(tmp) - 1)))
                        {
                            tmp.IsChecked = true;
                        }
                        else
                        {
                            tmp.IsChecked = false;
                        }
                    }
                }
                DataKey.Text = encDiffusion.key;
                DataMessage.Text = encDiffusion.message;
                ColorText(DataMessage, messageList);
                ColorText(DataKey, keyList);
            }

            ClearButtonsColor(false);
            DataButton.Background = buttonBrush;
            nextScreenID = 4;
            nextStepCounter = 1;
        }

        public void ShowRoundDataScreen(int step)
        {
            ResetAllScreens();
            RoundDataScreen.Visibility = Visibility.Visible;
            ArrowRounds.Margin = new Thickness(0, 0, 579, 220 - roundCounter * 30);
            IEnumerable<TextBlock> textChilds = RoundDataGrid.Children.OfType<TextBlock>();
            IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();
            for (int i = 0; i < roundCounter + 1; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionIsActive)
                {
                    enumerator.Current.Text = encDiffusion.lrData[i, 0];
                    ColorText(enumerator.Current, CompareStrings(encOriginal.lrData[i, 0], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = encOriginal.lrData[i, 0];
                }
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionIsActive)
                {
                    enumerator.Current.Text = encDiffusion.lrData[i, 1];
                    ColorText(enumerator.Current, CompareStrings(encOriginal.lrData[i, 1], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = encOriginal.lrData[i, 1];
                }
            }
            if (roundCounter == 16 && step == 1)
            {
                nextScreenID = 18;
                nextStepCounter = 1;
                ClearButtonsColor(true);
            }
            else if (roundCounter < 16 && step == 1)
            {
                nextScreenID = 11;
                nextStepCounter = 1;
                ClearButtonsColor(true);
            }
            else
            {
                nextScreenID = 19;
                nextStepCounter = 1;
                ArrowRounds.Visibility = Visibility.Hidden;
            }
        }

        public void ShowRoundKeyDataScreen(int step)
        {
            ResetAllScreens();
            RoundKeyDataScreen.Visibility = Visibility.Visible;
            ArrowSubKeys.Margin = new Thickness(30, 0, 579, 220 - (roundCounter - 1) * 30);
            IEnumerable<TextBlock> textChilds = RoundKeyDataGrid.Children.OfType<TextBlock>();
            IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();
            for (int i = 0; i < roundCounter; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionIsActive)
                {
                    enumerator.Current.Text = encDiffusion.roundKeys[i];
                    ColorText(enumerator.Current, CompareStrings(encOriginal.roundKeys[i], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = encOriginal.roundKeys[i];
                }
            }

            if (roundCounter == 16 && step == 1)
            {
                nextScreenID = 1;
                nextStepCounter = 3;
                ClearButtonsColor(true);
            }
            else if (roundCounter < 16 && step == 1)
            {
                nextScreenID = 6;
                nextStepCounter = 1;
                ClearButtonsColor(true);
            }
            else
            {
                nextScreenID = 17;
                nextStepCounter = 2;
                ArrowSubKeys.Visibility = Visibility.Hidden;
            }

        }

        public void ShowSBoxScreen(int step)
        {
            int sBoxctr = (step - 1) / 5;
            int sBoxstep = (step - 1) % 5;
            ResetAllScreens();

            SBoxScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.SBoxesLabel;
            SLabel.Content = "S" + (sBoxctr + 1);
            switch (sBoxctr)
            {
                case 0: S1Box.Visibility = Visibility.Visible; break;
                case 1: S2Box.Visibility = Visibility.Visible; break;
                case 2: S3Box.Visibility = Visibility.Visible; break;
                case 3: S4Box.Visibility = Visibility.Visible; break;
                case 4: S5Box.Visibility = Visibility.Visible; break;
                case 5: S6Box.Visibility = Visibility.Visible; break;
                case 6: S7Box.Visibility = Visibility.Visible; break;
                case 7: S8Box.Visibility = Visibility.Visible; break;
            }

            if (sBoxstep >= 0)
            {
                SBoxInput.Visibility = Visibility.Visible;
                Canvas.SetLeft(SBoxInMarker, 91 + (sBoxctr * 75));

                StringBuilder builderInOrig = new StringBuilder();
                for (int i = 0; i < 8; i++)
                {
                    builderInOrig.Append(encOriginal.sBoxStringDetails[roundCounter - 1, i * 4]);
                    builderInOrig.Append(" ");
                }
                StringBuilder builderOut = new StringBuilder();
                if (diffusionIsActive)
                {
                    for (int i = 0; i < sBoxctr; i++)
                    {
                        builderOut.Append(encDiffusion.sBoxStringDetails[roundCounter - 1, i * 4 + 3]);
                    }
                    SBoxOut.Text = builderOut.ToString();
                    ColorText(SBoxOut, CompareStrings(encDiffusion.roundDetails[roundCounter - 1, 2], encOriginal.roundDetails[roundCounter - 1, 2]));

                    SBoxInput.Text = encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0];
                    ColorText(SBoxInput, CompareStrings(encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0], encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0]));

                    StringBuilder builderInDiff = new StringBuilder();
                    for (int i = 0; i < 8; i++)
                    {
                        builderInDiff.Append(encDiffusion.sBoxStringDetails[roundCounter - 1, i * 4]);
                        builderInDiff.Append(" ");
                    }

                    SBoxIn.Text = builderInDiff.ToString();
                    ColorText(SBoxIn, CompareStrings(SBoxIn.Text, builderInOrig.ToString()));
                }
                else
                {
                    for (int i = 0; i < sBoxctr; i++)
                    {
                        builderOut.Append(encOriginal.sBoxStringDetails[roundCounter - 1, i * 4 + 3]);
                    }
                    SBoxOut.Text = builderOut.ToString();

                    SBoxInput.Text = encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0];

                    SBoxIn.Text = builderInOrig.ToString();
                }
                if (sBoxctr == 0)
                {
                    ClearButtonsColor(true);
                    SBoxButton.Background = buttonBrush;
                }

            }
            if (sBoxstep >= 1)
            {
                SBoxRow.Visibility = Visibility.Visible;
                if (diffusionIsActive)
                {
                    SBoxRow.Text = encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1].Insert(1, "_____"); //        
                    ColorText(SBoxRow, CompareStrings(SBoxRow.Text, encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1].Insert(1, "_____")));
                    SBoxRow.Text += "     ≙ " + encDiffusion.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }
                else
                {
                    SBoxRow.Text = encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1].Insert(1, "_____") + "     ≙ " + encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }

            }
            if (sBoxstep >= 2)
            {
                SBoxColumn.Visibility = Visibility.Visible;
                if (diffusionIsActive)
                {
                    SBoxColumn.Text = "_" + encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2] + "_";
                    ColorText(SBoxColumn, CompareStrings(SBoxColumn.Text, "_" + encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2] + "_"));
                    SBoxColumn.Text += "      ≙ " + encDiffusion.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                }
                else
                {
                    SBoxColumn.Text = "_" + encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2] + "_" + "      ≙ " + encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                }

            }
            if (sBoxstep >= 3)
            {
                SBoxOutput.Visibility = Visibility.Visible;

                int column, row;
                if (diffusionIsActive)
                {
                    SBoxOutput.Text = encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                    ColorText(SBoxOutput, CompareStrings(encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3], encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3]));
                    SBoxOutput.Text += "         ≙ " + encDiffusion.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 2];
                    column = encDiffusion.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                    row = encDiffusion.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];

                    int columnAlt = encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                    int rowAlt = encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];

                    // Set alternative SBoxJumper at the right place                
                    SBoxJumperAlt.Visibility = Visibility.Visible;
                    if (columnAlt < 10)
                    {
                        Canvas.SetLeft(SBoxJumperAlt, 77 + columnAlt * 21.556);
                    }
                    else
                    {
                        Canvas.SetLeft(SBoxJumperAlt, 297 + (columnAlt % 10) * 25.4);
                    }

                    Canvas.SetTop(SBoxJumperAlt, 137 + rowAlt * 20.45);
                }
                else
                {
                    SBoxOutput.Text = encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3] + "         ≙ " + encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 2];
                    column = encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                    row = encOriginal.sBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }
                // Set SBoxJumper at the right place                
                SBoxJumper.Visibility = Visibility.Visible;
                if (column < 10)
                {
                    Canvas.SetLeft(SBoxJumper, 77 + column * 21.556);
                }
                else
                {
                    Canvas.SetLeft(SBoxJumper, 297 + (column % 10) * 25.4);
                }

                Canvas.SetTop(SBoxJumper, 137 + row * 20.45);

            }
            if (sBoxstep >= 4)
            {
                SBoxOut.Visibility = Visibility.Visible;

                if (diffusionIsActive)
                {
                    SBoxOut.Text += encDiffusion.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                    ColorText(SBoxOut, CompareStrings(encDiffusion.roundDetails[roundCounter - 1, 2], encOriginal.roundDetails[roundCounter - 1, 2]));
                }
                else
                {
                    SBoxOut.Text += encOriginal.sBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                }

                if (sBoxctr == 7)
                {
                    nextScreenID = 12;
                    nextStepCounter = 5;
                    return;
                }

            }
            nextScreenID = 15;
            nextStepCounter++;

        }

        public void ShowShiftScreen(int step)
        {
            ResetAllScreens();
            bool firstShift = step < 3;
            int shiftStep = (step - 1) % 2;
            ShiftScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.CyclicShift;
            if (DESImplementation.byteShifts[roundCounter - 1] == 1)
            {
                SingleShift.Visibility = Visibility.Visible;
            }
            else
            {
                DoubleShift.Visibility = Visibility.Visible;
            }

            if (firstShift)
            {
                ShiftTopName.Content = "C" + (roundCounter - 1);
                ShiftBottomName.Content = "C" + roundCounter;
                if (diffusionIsActive)
                {
                    ShiftTop.Text = InsertSpaces(encDiffusion.keySchedule[roundCounter - 1, 0]);
                    ColorText(ShiftTop, CompareStrings(InsertSpaces(encOriginal.keySchedule[roundCounter - 1, 0]), ShiftTop.Text));
                    ShiftBottom.Text = InsertSpaces(encDiffusion.keySchedule[roundCounter, 0]);
                    ColorText(ShiftBottom, CompareStrings(InsertSpaces(encOriginal.keySchedule[roundCounter, 0]), ShiftBottom.Text));
                }
                else
                {
                    ShiftTop.Text = InsertSpaces(encOriginal.keySchedule[roundCounter - 1, 0]);
                    ShiftBottom.Text = InsertSpaces(encOriginal.keySchedule[roundCounter, 0]);
                }
                if (shiftStep == 0)
                {
                    ShiftCButton.Background = buttonBrush;
                }
            }
            else
            {
                ShiftTopName.Content = "D" + (roundCounter - 1);
                ShiftBottomName.Content = "D" + roundCounter;
                if (diffusionIsActive)
                {
                    ShiftTop.Text = InsertSpaces(encDiffusion.keySchedule[roundCounter - 1, 1]);
                    ColorText(ShiftTop, CompareStrings(InsertSpaces(encOriginal.keySchedule[roundCounter - 1, 1]), ShiftTop.Text));
                    ShiftBottom.Text = InsertSpaces(encDiffusion.keySchedule[roundCounter, 1]);
                    ColorText(ShiftBottom, CompareStrings(InsertSpaces(encOriginal.keySchedule[roundCounter, 1]), ShiftBottom.Text));
                }
                else
                {
                    ShiftTop.Text = InsertSpaces(encOriginal.keySchedule[roundCounter - 1, 1]);
                    ShiftBottom.Text = InsertSpaces(encOriginal.keySchedule[roundCounter, 1]);
                }
                if (shiftStep == 0)
                {
                    ShiftDButton.Background = buttonBrush;
                }
            }

            if (shiftStep == 1)
            {
                ShiftBottom.Visibility = Visibility.Visible;
                if (firstShift)
                {
                    nextScreenID = 6;
                    nextStepCounter = 3;
                }
                else
                {
                    nextScreenID = 6;
                    nextStepCounter = 4;
                }
            }
            else
            {
                if (firstShift)
                {
                    nextScreenID = 7;
                    nextStepCounter = 2;
                }
                else
                {
                    nextScreenID = 7;
                    nextStepCounter = 4;
                }
            }

            if (roundCounter < 10)
            {
                Canvas.SetLeft(RoundTable, 173 + (roundCounter - 1) * 21.625);
            }
            else
            {
                Canvas.SetLeft(RoundTable, 372 + (roundCounter - 10) * 30.3333);
            }
        }

        public void ShowStructureScreen()
        {
            ResetAllScreens();
            progress = 0.03125;
            StructureScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.GeneralStructure;

            nextScreenID = 1;
            nextStepCounter = 2;
        }

        public void ShowKeyScheduleScreen(int step)
        {
            ResetAllScreens();
            ClearButtonsColor(true);
            if (step == 1)
            {
                nextStepCounter = 2;
                nextScreenID = 6;
                roundCounter++;
                if (roundCounter == 1)
                {
                    ClearButtonsColor(false);
                    ActivateRoundButtons(true);
                    keyScheduleIsRunning = true;
                    SkipButton.Content = Properties.Resources.SkipRound;
                }
                ShiftDButton.Visibility = Visibility.Visible;
                ShiftDButton.Content = "Shift(D" + (roundCounter - 1) + ")";
                ShiftCButton.Visibility = Visibility.Visible;
                ShiftCButton.Content = "Shift(C" + (roundCounter - 1) + ")";
                PC2Button.Visibility = Visibility.Visible;
                ClearButtonsColor(true);
                ColorRoundButton();
                KeyScheduleButton.Background = buttonBrush;
                progress = 0.0875 + roundCounter * 0.025;
            }
            KeyScheduleScreen.Visibility = Visibility.Visible;
            KeyScheduleRoundKey.Content = "" + roundCounter;
            KeyScheduleLabel.Content = string.Format(Properties.Resources.Runde_16, roundCounter);
            KeyScheduleCRoundName.Content = "C" + (roundCounter - 1) + ":";
            KeyScheduleDRoundName.Content = "D" + (roundCounter - 1) + ":";
            if (diffusionIsActive)
            {
                KeyScheduleCRound.Text = encDiffusion.keySchedule[roundCounter - 1, 0];
                ColorText(KeyScheduleCRound, CompareStrings(encOriginal.keySchedule[roundCounter - 1, 0], KeyScheduleCRound.Text));
                KeyScheduleDRound.Text = encDiffusion.keySchedule[roundCounter - 1, 1];
                ColorText(KeyScheduleDRound, CompareStrings(encOriginal.keySchedule[roundCounter - 1, 1], KeyScheduleDRound.Text));
            }
            else
            {
                KeyScheduleCRound.Text = encOriginal.keySchedule[roundCounter - 1, 0];
                KeyScheduleDRound.Text = encOriginal.keySchedule[roundCounter - 1, 1];
            }
            if (roundCounter == 1)
            {
                KeyScheduleTopArrowRound1.Visibility = Visibility.Visible;
                KeySchedulePC1Label.Visibility = Visibility.Visible;
                KeySchedulePC1Box.Visibility = Visibility.Visible;
                KeyScheduleTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(KeyScheduleTopLine, 149.18);
                KeyScheduleDownArrow1.Visibility = Visibility.Visible;
                KeyScheduleDownArrow2.Visibility = Visibility.Visible;
            }
            else if (roundCounter == 16)
            {
                KeyScheduleTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(KeyScheduleTopLine, 720);
                KeyScheduleRightArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleLeftArrowRound2.Visibility = Visibility.Visible;
            }
            else
            {
                KeyScheduleRightArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleLeftArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleDownArrow1.Visibility = Visibility.Visible;
                KeyScheduleDownArrow2.Visibility = Visibility.Visible;
            }
            if (step >= 2)
            {
                KeyScheduleShiftBox1.Fill = yellowBrush;
                KeyScheduleShiftBox1.StrokeThickness = 5;
                nextScreenID = 7;
                nextStepCounter = 1;
            }
            if (step >= 3)
            {
                KeyScheduleShiftBox1.Fill = greenBrush;
                KeyScheduleShiftBox1.StrokeThickness = 2.63997;
                KeyScheduleShiftBox2.Fill = yellowBrush;
                KeyScheduleShiftBox2.StrokeThickness = 5;
                nextScreenID = 7;
                nextStepCounter = 3;
            }
            if (step >= 4)
            {
                KeyScheduleShiftBox2.Fill = greenBrush;
                KeyScheduleShiftBox2.StrokeThickness = 2.63997;
                KeySchedulePC2Box.Fill = yellowBrush;
                KeySchedulePC2Box.StrokeThickness = 5;
                nextScreenID = 8;
                nextStepCounter = 1;
            }
            if (step >= 5)
            {
                KeySchedulePC2Box.Fill = greenBrush;
                KeySchedulePC2Box.StrokeThickness = 2.63997;
                nextScreenID = 9;
                nextStepCounter = 1;
            }
        }

        public void ShowDESRoundScreen(int step)
        {
            ResetAllScreens();
            ClearButtonsColor(true);
            if (step == 1)
            {
                nextStepCounter = 2;
                nextScreenID = 11;
                roundCounter++;
                if (roundCounter == 1)
                {
                    ClearButtonsColor(false);
                    ActivateRoundButtons(true);
                    desRoundsIsRunning = true;
                    SkipButton.Content = Properties.Resources.SkipRound;
                }
                ExpansionButton.Visibility = Visibility.Visible;
                KeyAdditionButton.Visibility = Visibility.Visible;
                SBoxButton.Visibility = Visibility.Visible;
                PermutationButton.Visibility = Visibility.Visible;
                RoundAdditionButton.Visibility = Visibility.Visible;
                RoundAdditionButton.Content = "L" + (roundCounter - 1) + " ⊕ f (R" + (roundCounter - 1) + ")";
                ClearButtonsColor(true);
                ColorRoundButton();
                DESButton.Background = buttonBrush;
                progress = 0.5125 + roundCounter * 0.025;
            }

            DESRoundScreen.Visibility = Visibility.Visible;
            DESRoundKey.Content = "" + roundCounter;
            DESRoundLabel.Content = string.Format(Properties.Resources.Runde_16, roundCounter);
            DESRoundR0Name.Content = "R" + (roundCounter - 1) + ":";
            DESRoundL0Name.Content = "L" + (roundCounter - 1) + ":";
            DESRoundR1Name.Content = "R" + (roundCounter) + ":";
            DESRoundL1Name.Content = "L" + (roundCounter) + ":";
            if (diffusionIsActive)
            {
                DESRoundL0.Text = encDiffusion.lrData[roundCounter - 1, 0];
                ColorText(DESRoundL0, CompareStrings(encOriginal.lrData[roundCounter - 1, 0], DESRoundL0.Text));
                DESRoundR0.Text = encDiffusion.lrData[roundCounter - 1, 1];
                ColorText(DESRoundR0, CompareStrings(encOriginal.lrData[roundCounter - 1, 1], DESRoundR0.Text));

                DESRoundL1.Text = encDiffusion.lrData[roundCounter, 0];
                ColorText(DESRoundL1, CompareStrings(encOriginal.lrData[roundCounter, 0], DESRoundL1.Text));
                DESRoundR1.Text = encDiffusion.lrData[roundCounter, 1];
                ColorText(DESRoundR1, CompareStrings(encOriginal.lrData[roundCounter, 1], DESRoundR1.Text));
            }
            else
            {
                DESRoundL0.Text = encOriginal.lrData[roundCounter - 1, 0];
                DESRoundR0.Text = encOriginal.lrData[roundCounter - 1, 1];

                DESRoundL1.Text = encOriginal.lrData[roundCounter, 0];
                DESRoundR1.Text = encOriginal.lrData[roundCounter, 1];
            }
            if (roundCounter == 1)
            {
                DESRoundTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(DESRoundTopLine, 20.18);
            }
            else if (roundCounter == 16)
            {
                DESRoundTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(DESRoundTopLine, 685.18);
            }

            if (step >= 2)
            {
                DESRoundFunctionPath.Fill = yellowBrush;
                DESRoundFunctionPath.StrokeThickness = 5;
                DESRoundL1Name.Visibility = Visibility.Visible;
                DESRoundL1.Visibility = Visibility.Visible;
                nextScreenID = 12;
                nextStepCounter = 1;
            }
            if (step >= 3)
            {
                DESRoundFunctionPath.Fill = greenBrush;
                DESRoundFunctionPath.StrokeThickness = 2.63997;
                RoundAdditionPath.Fill = yellowBrush;
                RoundAdditionPath.StrokeThickness = 5;
                nextScreenID = 14;
                nextStepCounter = 3;
            }
            if (step >= 4)
            {
                RoundAdditionPath.Fill = greenBrush;
                RoundAdditionPath.StrokeThickness = 2.63997;
                DESRoundR1Name.Visibility = Visibility.Visible;
                DESRoundR1.Visibility = Visibility.Visible;
                nextScreenID = 17;
                nextStepCounter = 1;
            }
        }

        public void ShowRoundFunctionScreen(int step)
        {
            ResetAllScreens();
            ClearButtonsColor(true);
            RoundFunctionScreen.Visibility = Visibility.Visible;
            FScreenFunctionR.Content = "" + (roundCounter - 1);
            FScreenFunctionKey.Content = "" + roundCounter;
            FScreenFunctionInfoKey.Content = "" + roundCounter;
            FScreenFunctionInfoR.Content = "" + (roundCounter - 1);
            FScreenFunctionInfoRound.Content = "" + roundCounter;

            if (step == 1)
            {
                nextStepCounter = 2;
                nextScreenID = 12;
            }
            if (step >= 2)
            {
                ExpansionPath.Fill = yellowBrush;
                ExpansionPath.StrokeThickness = 2;
                nextScreenID = 13;
                nextStepCounter = 1;
            }
            if (step >= 3)
            {
                ExpansionPath.Fill = greenBrush;
                ExpansionPath.StrokeThickness = 1.43999;
                FScreenXorPath.Fill = yellowBrush;
                FScreenXorPath.StrokeThickness = 2;
                nextScreenID = 14;
                nextStepCounter = 1;
            }
            if (step >= 4)
            {
                FScreenXorPath.Fill = greenBrush;
                FScreenXorPath.StrokeThickness = 1.43999;
                FScreenSPath1.Fill = yellowBrush;
                FScreenSPath1.StrokeThickness = 2;
                FScreenSPath2.Fill = yellowBrush;
                FScreenSPath2.StrokeThickness = 2;
                FScreenSPath3.Fill = yellowBrush;
                FScreenSPath3.StrokeThickness = 2;
                FScreenSPath4.Fill = yellowBrush;
                FScreenSPath4.StrokeThickness = 2;
                FScreenSPath5.Fill = yellowBrush;
                FScreenSPath5.StrokeThickness = 2;
                FScreenSPath6.Fill = yellowBrush;
                FScreenSPath6.StrokeThickness = 2;
                FScreenSPath7.Fill = yellowBrush;
                FScreenSPath7.StrokeThickness = 2;
                FScreenSPath8.Fill = yellowBrush;
                FScreenSPath8.StrokeThickness = 2;
                nextScreenID = 15;
                nextStepCounter = 1;
            }
            if (step >= 5)
            {
                FScreenSPath1.Fill = greenBrush;
                FScreenSPath1.StrokeThickness = 1.43999;
                FScreenSPath2.Fill = greenBrush;
                FScreenSPath2.StrokeThickness = 1.43999;
                FScreenSPath3.Fill = greenBrush;
                FScreenSPath3.StrokeThickness = 1.43999;
                FScreenSPath4.Fill = greenBrush;
                FScreenSPath4.StrokeThickness = 1.43999;
                FScreenSPath5.Fill = greenBrush;
                FScreenSPath5.StrokeThickness = 1.43999;
                FScreenSPath6.Fill = greenBrush;
                FScreenSPath6.StrokeThickness = 1.43999;
                FScreenSPath7.Fill = greenBrush;
                FScreenSPath7.StrokeThickness = 1.43999;
                FScreenSPath8.Fill = greenBrush;
                FScreenSPath8.StrokeThickness = 1.43999;
                FScreenPPermutationBox.Fill = yellowBrush;
                FScreenPPermutationBox.StrokeThickness = 2;
                nextScreenID = 16;
                nextStepCounter = 1;
            }
            if (step >= 6)
            {
                FScreenPPermutationBox.Fill = greenBrush;
                FScreenPPermutationBox.StrokeThickness = 1.43999;
                nextScreenID = 11;
                nextStepCounter = 3;
            }
        }

        public void ShowXORScreen(int step)
        {
            ResetAllScreens();
            bool keyAddition = step < 3;
            int xorStep = (step - 1) % 2;
            XORScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.BitwiseXOROperation;
            if (keyAddition)
            {
                XorOperator1Name.Content = "K" + roundCounter + ":";
                XorOperator2Name.Content = "Exp:";
                XorResultName.Content = "";
                if (diffusionIsActive)
                {
                    XorOperator1.Text = encDiffusion.roundKeys[roundCounter - 1];
                    ColorText(XorOperator1, CompareStrings(encOriginal.roundKeys[roundCounter - 1], XorOperator1.Text));
                    XorOperator2.Text = encDiffusion.roundDetails[roundCounter - 1, 0];
                    ColorText(XorOperator2, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 0], XorOperator2.Text));
                    XorResult.Text = encDiffusion.roundDetails[roundCounter - 1, 1];
                    ColorText(XorResult, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 1], XorResult.Text));
                }
                else
                {
                    XorOperator1.Text = encOriginal.roundKeys[roundCounter - 1];
                    XorOperator2.Text = encOriginal.roundDetails[roundCounter - 1, 0];
                    XorResult.Text = encOriginal.roundDetails[roundCounter - 1, 1];
                }
                if (xorStep == 0)
                {
                    ClearButtonsColor(true);
                    KeyAdditionButton.Background = buttonBrush;
                }
            }
            else
            {
                XorOperator1Name.Content = "L" + (roundCounter - 1) + ":";
                XorOperator2Name.Content = "f(R" + (roundCounter - 1) + "):";
                XorResultName.Content = "R" + roundCounter + ":";
                if (diffusionIsActive)
                {
                    XorOperator1.Text = encDiffusion.lrData[roundCounter - 1, 0];
                    ColorText(XorOperator1, CompareStrings(encOriginal.lrData[roundCounter - 1, 0], XorOperator1.Text));
                    XorOperator2.Text = encDiffusion.roundDetails[roundCounter - 1, 3];
                    ColorText(XorOperator2, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 3], XorOperator2.Text));
                    XorResult.Text = encDiffusion.lrData[roundCounter, 1];
                    ColorText(XorResult, CompareStrings(encOriginal.lrData[roundCounter, 1], XorResult.Text));
                }
                else
                {
                    XorOperator1.Text = encOriginal.lrData[roundCounter - 1, 0];
                    XorOperator2.Text = encOriginal.roundDetails[roundCounter - 1, 3];
                    XorResult.Text = encOriginal.lrData[roundCounter, 1];
                }
                if (xorStep == 0)
                {
                    ClearButtonsColor(true);
                    RoundAdditionButton.Background = buttonBrush;
                }
            }
            FScreenFunctionR.Content = "" + (roundCounter - 1);
            FScreenFunctionKey.Content = "" + roundCounter;
            FScreenFunctionInfoKey.Content = "" + roundCounter;
            FScreenFunctionInfoR.Content = "" + (roundCounter - 1);
            FScreenFunctionInfoRound.Content = "" + roundCounter;

            if (xorStep == 1)
            {
                XorResult.Visibility = Visibility.Visible;
                XorResultName.Visibility = Visibility.Visible;
                if (keyAddition)
                {
                    XorResultNameLong.Visibility = Visibility.Visible;
                    nextScreenID = 12;
                    nextStepCounter = 4;
                }
                else
                {
                    nextScreenID = 11;
                    nextStepCounter = 4;
                }
            }
            else
            {
                if (keyAddition)
                {
                    nextScreenID = 14;
                    nextStepCounter = 2;
                }
                else
                {
                    nextScreenID = 14;
                    nextStepCounter = 4;
                }
            }
        }

        public void ShowIPScreen(int step)
        {
            ResetAllScreens();
            IPScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = "Initial Permutation";
            roundCounter = 0;
            progress = 0.5125;

            if (diffusionIsActive)
            {
                IpTop.Text = encDiffusion.message;
                ColorText(IpTop, CompareStrings(encOriginal.message, IpTop.Text));

                string old = encOriginal.lrData[0, 0] + encOriginal.lrData[0, 1];
                string changed = encDiffusion.lrData[0, 0] + encDiffusion.lrData[0, 1];
                IpBottom.Text = changed;
                ColorText(IpBottom, CompareStrings(old, changed));
            }
            else
            {
                IpTop.Text = encOriginal.message;
                IpBottom.Text = encOriginal.lrData[0, 0] + encOriginal.lrData[0, 1];
            }
            if (step == 2)
            {
                IpBottom.Visibility = Visibility.Visible;
                nextScreenID = 11;
                nextStepCounter = 1;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 10;
                ClearButtonsColor(false);
                IPButton.Background = buttonBrush;
            }
        }

        public void ShowPC1Screen(int step)
        {
            ResetAllScreens();
            PC1Screen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = "Permuted Choice 1";
            roundCounter = 0;
            progress = 0.0875;

            if (diffusionIsActive)
            {
                Pc1Top.Text = encDiffusion.key;
                ColorText(Pc1Top, CompareStrings(encOriginal.key, Pc1Top.Text));

                string old = encOriginal.keySchedule[0, 0] + encOriginal.keySchedule[0, 1];
                string changed = encDiffusion.keySchedule[0, 0] + encDiffusion.keySchedule[0, 1];
                Pc1Bottom.Text = changed;
                ColorText(Pc1Bottom, CompareStrings(old, changed));
            }
            else
            {
                Pc1Top.Text = encOriginal.key;
                Pc1Bottom.Text = encOriginal.keySchedule[0, 0] + encOriginal.keySchedule[0, 1];
            }
            if (step == 2)
            {
                Pc1Bottom.Visibility = Visibility.Visible;
                nextScreenID = 6;
                nextStepCounter = 1;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 5;
                ClearButtonsColor(false);
                PC1Button.Background = buttonBrush;
            }
        }

        public void ShowPC2Screen(int step)
        {
            ResetAllScreens();
            PC2Screen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = "Permuted Choice 2";

            if (diffusionIsActive)
            {
                string old = encOriginal.keySchedule[roundCounter, 0] + encOriginal.keySchedule[roundCounter, 1];
                string changed = encDiffusion.keySchedule[roundCounter, 0] + encDiffusion.keySchedule[roundCounter, 1];
                Pc2Top.Text = changed;
                ColorText(Pc2Top, CompareStrings(old, changed));

                Pc2Bottom.Text = encDiffusion.roundKeys[roundCounter - 1];
                ColorText(Pc2Bottom, CompareStrings(encOriginal.roundKeys[roundCounter - 1], Pc2Bottom.Text));
            }
            else
            {
                Pc2Top.Text = encOriginal.keySchedule[roundCounter, 0] + encOriginal.keySchedule[roundCounter, 1];
                Pc2Bottom.Text = encOriginal.roundKeys[roundCounter - 1];
            }
            if (step == 2)
            {
                Pc2Bottom.Visibility = Visibility.Visible;
                nextScreenID = 6;
                nextStepCounter = 5;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 8;
                ClearButtonsColor(true);
                PC2Button.Background = buttonBrush;
            }
        }

        public void ShowExpansionScreen(int step)
        {
            ResetAllScreens();
            ExpansionScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = "Expansion";

            if (diffusionIsActive)
            {
                ExpansionTop.Text = encDiffusion.lrData[roundCounter - 1, 1];
                ColorText(ExpansionTop, CompareStrings(encOriginal.lrData[roundCounter - 1, 1], ExpansionTop.Text));

                ExpansionBottom.Text = encDiffusion.roundDetails[roundCounter - 1, 0];
                ColorText(ExpansionBottom, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 0], ExpansionBottom.Text));
            }
            else
            {
                ExpansionTop.Text = encOriginal.lrData[roundCounter - 1, 1];
                ExpansionBottom.Text = encOriginal.roundDetails[roundCounter - 1, 0];
            }
            if (step == 2)
            {
                ExpansionBottom.Visibility = Visibility.Visible;
                nextScreenID = 12;
                nextStepCounter = 3;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 13;
                ClearButtonsColor(true);
                ExpansionButton.Background = buttonBrush;
            }
        }

        public void ShowPScreen(int step)
        {
            ResetAllScreens();
            PScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = Properties.Resources.PermutationFunction;

            if (diffusionIsActive)
            {
                PTop.Text = encDiffusion.roundDetails[roundCounter - 1, 2];
                ColorText(PTop, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 2], PTop.Text));

                PBottom.Text = encDiffusion.roundDetails[roundCounter - 1, 3];
                ColorText(PBottom, CompareStrings(encOriginal.roundDetails[roundCounter - 1, 3], PBottom.Text));
            }
            else
            {
                PTop.Text = encOriginal.roundDetails[roundCounter - 1, 2];
                PBottom.Text = encOriginal.roundDetails[roundCounter - 1, 3];
            }
            if (step == 2)
            {
                PBottom.Visibility = Visibility.Visible;
                nextScreenID = 12;
                nextStepCounter = 6;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 16;
                ClearButtonsColor(true);
                PermutationButton.Background = buttonBrush;
            }
        }

        public void ShowFPScreen(int step)
        {
            ResetAllScreens();
            FPScreen.Visibility = Visibility.Visible;
            Title.Visibility = Visibility.Visible;
            Title.Content = "Final Permutation";
            progress = 0.9375;

            if (diffusionIsActive)
            {
                string old = encOriginal.lrData[16, 1] + encOriginal.lrData[16, 0];
                string changed = encDiffusion.lrData[16, 1] + encDiffusion.lrData[16, 0];
                FpTop.Text = changed;
                ColorText(FpTop, CompareStrings(old, changed));

                FpBottom.Text = encDiffusion.ciphertext;
                ColorText(FpBottom, CompareStrings(encOriginal.ciphertext, encDiffusion.ciphertext));
            }
            else
            {
                FpTop.Text = encOriginal.lrData[16, 1] + encOriginal.lrData[16, 0];
                FpBottom.Text = encOriginal.ciphertext;
            }
            if (step == 2)
            {
                FpBottom.Visibility = Visibility.Visible;
                nextScreenID = 1;
                nextStepCounter = 4;
            }
            else
            {
                nextStepCounter = 2;
                nextScreenID = 18;
                ClearButtonsColor(false);
                FPButton.Background = buttonBrush;
                ActivateRoundButtons(false);
                ClearButtonsColor(true);
                roundCounter = 0;
                SkipButton.Content = Properties.Resources.SkipStep;
                ExpansionButton.Visibility = Visibility.Hidden;
                KeyAdditionButton.Visibility = Visibility.Hidden;
                SBoxButton.Visibility = Visibility.Hidden;
                PermutationButton.Visibility = Visibility.Hidden;
                RoundAdditionButton.Visibility = Visibility.Hidden;
                desRoundsIsRunning = false;
            }

        }

        #endregion Screen-Methods (Show)

        #region Screen-Methods (Reset)

        public void ResetIntroScreen()
        {
            IntroScreen.Visibility = Visibility.Hidden;
            PrevButton.IsEnabled = true;
        }

        public void ResetInfoScreen()
        {
            InfoScreen.Visibility = Visibility.Hidden;
            InfoText.Visibility = Visibility.Hidden;
            HistoryText.Visibility = Visibility.Hidden;
            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetChapterScreen()
        {
            ChapterScreen.Visibility = Visibility.Hidden;
            ChapterTextKS.Visibility = Visibility.Hidden;
            ChapterTextDES.Visibility = Visibility.Hidden;
            ChapterLabel.VerticalAlignment = VerticalAlignment.Center;
            ChapterLabel.Content = "";
        }

        public void ResetFinalScreen()
        {
            FinalScreen.Visibility = Visibility.Hidden;
            FinalCiphertext.TextEffects.Clear();
            FinalKey.TextEffects.Clear();
            FinalMessage.TextEffects.Clear();
            FinalMessage.FontSize = 8;
            FinalKey.FontSize = 8;
            FinalCiphertextRec.Width = 377;
            Canvas.SetLeft(FinalCiphertextRec, 36);
            AutoTButton.IsEnabled = true;
            NextButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
        }

        public void ResetInputDataScreen()
        {
            InputDataScreen.Visibility = Visibility.Hidden;
            DiffTButton.IsChecked = false;
            DiffTButton.Content = Properties.Resources.DiffusionVisualizer;
            DiffClearButton.Visibility = Visibility.Hidden;
            DiffOkButton.Visibility = Visibility.Hidden;
            DiffInfoLabel.Visibility = Visibility.Hidden;
            ShowDiffusionBoxes(false);
            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetRoundDataScreen()
        {
            RoundDataScreen.Visibility = Visibility.Hidden;
            L0.Visibility = Visibility.Hidden;
            L1.Visibility = Visibility.Hidden;
            L2.Visibility = Visibility.Hidden;
            L3.Visibility = Visibility.Hidden;
            L4.Visibility = Visibility.Hidden;
            L5.Visibility = Visibility.Hidden;
            L6.Visibility = Visibility.Hidden;
            L7.Visibility = Visibility.Hidden;
            L8.Visibility = Visibility.Hidden;
            L9.Visibility = Visibility.Hidden;
            L10.Visibility = Visibility.Hidden;
            L11.Visibility = Visibility.Hidden;
            L12.Visibility = Visibility.Hidden;
            L13.Visibility = Visibility.Hidden;
            L14.Visibility = Visibility.Hidden;
            L15.Visibility = Visibility.Hidden;
            L16.Visibility = Visibility.Hidden;
            R0.Visibility = Visibility.Hidden;
            R1.Visibility = Visibility.Hidden;
            R2.Visibility = Visibility.Hidden;
            R3.Visibility = Visibility.Hidden;
            R4.Visibility = Visibility.Hidden;
            R5.Visibility = Visibility.Hidden;
            R6.Visibility = Visibility.Hidden;
            R7.Visibility = Visibility.Hidden;
            R8.Visibility = Visibility.Hidden;
            R9.Visibility = Visibility.Hidden;
            R10.Visibility = Visibility.Hidden;
            R11.Visibility = Visibility.Hidden;
            R12.Visibility = Visibility.Hidden;
            R13.Visibility = Visibility.Hidden;
            R14.Visibility = Visibility.Hidden;
            R15.Visibility = Visibility.Hidden;
            R16.Visibility = Visibility.Hidden;
            L0.TextEffects.Clear();
            L1.TextEffects.Clear();
            L2.TextEffects.Clear();
            L3.TextEffects.Clear();
            L4.TextEffects.Clear();
            L5.TextEffects.Clear();
            L6.TextEffects.Clear();
            L7.TextEffects.Clear();
            L8.TextEffects.Clear();
            L9.TextEffects.Clear();
            L10.TextEffects.Clear();
            L11.TextEffects.Clear();
            L12.TextEffects.Clear();
            L13.TextEffects.Clear();
            L14.TextEffects.Clear();
            L15.TextEffects.Clear();
            L16.TextEffects.Clear();
            R0.TextEffects.Clear();
            R1.TextEffects.Clear();
            R2.TextEffects.Clear();
            R3.TextEffects.Clear();
            R4.TextEffects.Clear();
            R5.TextEffects.Clear();
            R6.TextEffects.Clear();
            R7.TextEffects.Clear();
            R8.TextEffects.Clear();
            R9.TextEffects.Clear();
            R10.TextEffects.Clear();
            R11.TextEffects.Clear();
            R12.TextEffects.Clear();
            R13.TextEffects.Clear();
            R14.TextEffects.Clear();
            R15.TextEffects.Clear();
            R16.TextEffects.Clear();

            ArrowRounds.Margin = new Thickness(0, 0, 579, 220);
            ArrowRounds.Visibility = Visibility.Visible;
        }

        public void ResetRoundKeyDataScreen()
        {
            RoundKeyDataScreen.Visibility = Visibility.Hidden;
            K1.Visibility = Visibility.Hidden;
            K2.Visibility = Visibility.Hidden;
            K3.Visibility = Visibility.Hidden;
            K4.Visibility = Visibility.Hidden;
            K5.Visibility = Visibility.Hidden;
            K6.Visibility = Visibility.Hidden;
            K7.Visibility = Visibility.Hidden;
            K8.Visibility = Visibility.Hidden;
            K9.Visibility = Visibility.Hidden;
            K10.Visibility = Visibility.Hidden;
            K11.Visibility = Visibility.Hidden;
            K12.Visibility = Visibility.Hidden;
            K13.Visibility = Visibility.Hidden;
            K14.Visibility = Visibility.Hidden;
            K15.Visibility = Visibility.Hidden;
            K16.Visibility = Visibility.Hidden;

            K1.TextEffects.Clear();
            K2.TextEffects.Clear();
            K3.TextEffects.Clear();
            K4.TextEffects.Clear();
            K5.TextEffects.Clear();
            K6.TextEffects.Clear();
            K7.TextEffects.Clear();
            K8.TextEffects.Clear();
            K9.TextEffects.Clear();
            K10.TextEffects.Clear();
            K11.TextEffects.Clear();
            K12.TextEffects.Clear();
            K13.TextEffects.Clear();
            K14.TextEffects.Clear();
            K15.TextEffects.Clear();
            K16.TextEffects.Clear();
            ArrowSubKeys.Margin = new Thickness(30, 0, 579, 220);
            ArrowSubKeys.Visibility = Visibility.Visible;
        }

        public void ResetSBoxScreen()
        {
            SBoxScreen.Visibility = Visibility.Hidden;
            S1Box.Visibility = Visibility.Hidden;
            S2Box.Visibility = Visibility.Hidden;
            S3Box.Visibility = Visibility.Hidden;
            S4Box.Visibility = Visibility.Hidden;
            S5Box.Visibility = Visibility.Hidden;
            S6Box.Visibility = Visibility.Hidden;
            S7Box.Visibility = Visibility.Hidden;
            S8Box.Visibility = Visibility.Hidden;
            SBoxIn.TextEffects.Clear();
            SBoxInput.Visibility = Visibility.Hidden;
            SBoxInput.TextEffects.Clear();
            SBoxRow.Visibility = Visibility.Hidden;
            SBoxRow.TextEffects.Clear();
            SBoxColumn.Visibility = Visibility.Hidden;
            SBoxColumn.TextEffects.Clear();
            SBoxOutput.Visibility = Visibility.Hidden;
            SBoxOutput.TextEffects.Clear();
            SBoxJumper.Visibility = Visibility.Hidden;
            Canvas.SetLeft(SBoxJumper, 77);
            Canvas.SetTop(SBoxJumper, 137);
            SBoxJumperAlt.Visibility = Visibility.Hidden;
            Canvas.SetLeft(SBoxJumperAlt, 77);
            Canvas.SetTop(SBoxJumperAlt, 137);
            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
            SBoxOut.Text = "";
            SBoxOut.TextEffects.Clear();
        }

        public void ResetShiftScreen()
        {
            ShiftScreen.Visibility = Visibility.Hidden;
            Canvas.SetLeft(RoundTable, 173);
            ShiftBottom.Visibility = Visibility.Hidden;
            ShiftBottom.TextEffects.Clear();
            ShiftTop.TextEffects.Clear();
            SingleShift.Visibility = Visibility.Hidden;
            DoubleShift.Visibility = Visibility.Hidden;
            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetStructureScreen()
        {
            StructureScreen.Visibility = Visibility.Hidden;
            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetKeyScheduleScreen()
        {
            KeyScheduleScreen.Visibility = Visibility.Hidden;
            KeyScheduleTopLine.Visibility = Visibility.Hidden;
            KeyScheduleTopArrowRound1.Visibility = Visibility.Hidden;
            KeyScheduleRightArrowRound2.Visibility = Visibility.Hidden;
            KeyScheduleLeftArrowRound2.Visibility = Visibility.Hidden;
            KeyScheduleDownArrow2.Visibility = Visibility.Hidden;
            KeyScheduleDownArrow1.Visibility = Visibility.Hidden;
            KeySchedulePC1Box.Visibility = Visibility.Hidden;
            KeySchedulePC1Label.Visibility = Visibility.Hidden;
            KeyScheduleShiftBox1.ClearValue(Shape.FillProperty);
            KeyScheduleShiftBox1.StrokeThickness = 2.63997;
            KeyScheduleShiftBox2.ClearValue(Shape.FillProperty);
            KeyScheduleShiftBox2.StrokeThickness = 2.63997;
            KeySchedulePC2Box.ClearValue(Shape.FillProperty);
            KeySchedulePC2Box.StrokeThickness = 2.63997;
            KeyScheduleCRound.TextEffects.Clear();
            KeyScheduleDRound.TextEffects.Clear();
        }

        public void ResetDESRoundScreen()
        {
            DESRoundScreen.Visibility = Visibility.Hidden;
            DESRoundTopLine.Visibility = Visibility.Hidden;

            DESRoundFunctionPath.ClearValue(Shape.FillProperty);
            DESRoundFunctionPath.StrokeThickness = 2.63997;
            RoundAdditionPath.ClearValue(Shape.FillProperty);
            RoundAdditionPath.StrokeThickness = 2.63997;

            DESRoundL1Name.Visibility = Visibility.Hidden;
            DESRoundR1Name.Visibility = Visibility.Hidden;
            DESRoundL1.Visibility = Visibility.Hidden;
            DESRoundR1.Visibility = Visibility.Hidden;

            DESRoundL0.TextEffects.Clear();
            DESRoundL1.TextEffects.Clear();
            DESRoundR0.TextEffects.Clear();
            DESRoundR1.TextEffects.Clear();
        }

        public void ResetRoundFunctionScreen()
        {
            RoundFunctionScreen.Visibility = Visibility.Hidden;

            ExpansionPath.ClearValue(Shape.FillProperty);
            ExpansionPath.StrokeThickness = 1.43999;
            FScreenXorPath.ClearValue(Shape.FillProperty);
            FScreenXorPath.StrokeThickness = 1.43999;
            FScreenSPath1.ClearValue(Shape.FillProperty);
            FScreenSPath1.StrokeThickness = 1.43999;
            FScreenSPath2.ClearValue(Shape.FillProperty);
            FScreenSPath2.StrokeThickness = 1.43999;
            FScreenSPath3.ClearValue(Shape.FillProperty);
            FScreenSPath3.StrokeThickness = 1.43999;
            FScreenSPath4.ClearValue(Shape.FillProperty);
            FScreenSPath4.StrokeThickness = 1.43999;
            FScreenSPath5.ClearValue(Shape.FillProperty);
            FScreenSPath5.StrokeThickness = 1.43999;
            FScreenSPath6.ClearValue(Shape.FillProperty);
            FScreenSPath6.StrokeThickness = 1.43999;
            FScreenSPath7.ClearValue(Shape.FillProperty);
            FScreenSPath7.StrokeThickness = 1.43999;
            FScreenSPath8.ClearValue(Shape.FillProperty);
            FScreenSPath8.StrokeThickness = 1.43999;
            FScreenPPermutationBox.ClearValue(Shape.FillProperty);
            FScreenPPermutationBox.StrokeThickness = 1.43999;

        }

        public void ResetXORScreen()
        {
            XORScreen.Visibility = Visibility.Hidden;
            XorResult.Visibility = Visibility.Hidden;
            XorResultName.Visibility = Visibility.Hidden;
            XorResultNameLong.Visibility = Visibility.Hidden;
            XorOperator1.TextEffects.Clear();
            XorOperator2.TextEffects.Clear();
            XorResult.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetIPScreen()
        {
            IPScreen.Visibility = Visibility.Hidden;
            IpBottom.Visibility = Visibility.Hidden;
            IpBottom.TextEffects.Clear();
            IpTop.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetPC1Screen()
        {
            PC1Screen.Visibility = Visibility.Hidden;
            Pc1Bottom.Visibility = Visibility.Hidden;
            Pc1Bottom.TextEffects.Clear();
            Pc1Top.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetPC2Screen()
        {
            PC2Screen.Visibility = Visibility.Hidden;
            Pc2Bottom.Visibility = Visibility.Hidden;
            Pc2Bottom.TextEffects.Clear();
            Pc2Top.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetExpansionScreen()
        {
            ExpansionScreen.Visibility = Visibility.Hidden;
            ExpansionBottom.Visibility = Visibility.Hidden;
            ExpansionBottom.TextEffects.Clear();
            ExpansionTop.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetPScreen()
        {
            PScreen.Visibility = Visibility.Hidden;
            PBottom.Visibility = Visibility.Hidden;
            PBottom.TextEffects.Clear();
            PTop.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        public void ResetFPScreen()
        {
            FPScreen.Visibility = Visibility.Hidden;
            FpBottom.Visibility = Visibility.Hidden;
            FpBottom.TextEffects.Clear();
            FpTop.TextEffects.Clear();

            Title.Content = "";
            Title.Visibility = Visibility.Hidden;
        }

        #endregion Screen-Methods (Reset)

        #region Helper-Methods

        public void SetInitialState()
        {
            if (desRoundsIsRunning)
            {
                ShowFPScreen(1);
            }
            else if (keyScheduleIsRunning)
            {
                ShowChapterScreen(3);
            }
            if (diffusionIsActive)
            {
                IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator.Current.IsChecked = false;
                }

                DataKey.TextEffects.Clear();
                DataMessage.TextEffects.Clear();
                DataKey.Text = encOriginal.key;
                DataMessage.Text = encOriginal.message;
            }
            ResetAllScreens();
            nextStepCounter = 0;
            nextScreenID = 0;
            roundCounter = 0;
            progress = 0;
            keyScheduleIsRunning = false;
            desRoundsIsRunning = false;
            diffusionIsActive = false;
            ClearButtonsColor(false);
            ClearButtonsColor(true);
            ShowIntroScreen();
            ActivateNavigationButtons(false);
            DiffusionActiveLabel.Visibility = Visibility.Hidden;

        }

        private void ResetAllScreens()
        {
            ResetIntroScreen();
            ResetInfoScreen();
            ResetChapterScreen();
            ResetInputDataScreen();
            ResetStructureScreen();
            ResetPC1Screen();
            ResetKeyScheduleScreen();
            ResetShiftScreen();
            ResetPC2Screen();
            ResetRoundKeyDataScreen();
            ResetIPScreen();
            ResetDESRoundScreen();
            ResetRoundFunctionScreen();
            ResetExpansionScreen();
            ResetXORScreen();
            ResetSBoxScreen();
            ResetPScreen();
            ResetRoundDataScreen();
            ResetFPScreen();
            ResetFinalScreen();
        }

        private void ExecuteNextStep()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                switch (nextScreenID)
                {
                    case 1: ShowChapterScreen(nextStepCounter); break;
                    case 2: ShowInfoScreen(nextStepCounter); break;
                    case 3: ShowInputDataScreen(); break;
                    case 4: ShowStructureScreen(); break;
                    case 5: ShowPC1Screen(nextStepCounter); break;
                    case 6: ShowKeyScheduleScreen(nextStepCounter); break;
                    case 7: ShowShiftScreen(nextStepCounter); break;
                    case 8: ShowPC2Screen(nextStepCounter); break;
                    case 9: ShowRoundKeyDataScreen(nextStepCounter); break;
                    case 10: ShowIPScreen(nextStepCounter); break;
                    case 11: ShowDESRoundScreen(nextStepCounter); break;
                    case 12: ShowRoundFunctionScreen(nextStepCounter); break;
                    case 13: ShowExpansionScreen(nextStepCounter); break;
                    case 14: ShowXORScreen(nextStepCounter); break;
                    case 15: ShowSBoxScreen(nextStepCounter); break;
                    case 16: ShowPScreen(nextStepCounter); break;
                    case 17: ShowRoundDataScreen(nextStepCounter); break;
                    case 18: ShowFPScreen(nextStepCounter); break;
                    case 19: ShowFinalScreen(); break;
                }
            }, null);

        }

        public void ActivateNavigationButtons(bool active)
        {
            IntroButton.IsEnabled = active;
            DataButton.IsEnabled = active;
            PC1Button.IsEnabled = active;
            KeyScheduleButton.IsEnabled = active;
            IPButton.IsEnabled = active;
            DESButton.IsEnabled = active;
            FPButton.IsEnabled = active;
            SummaryButton.IsEnabled = active;
            NextButton.IsEnabled = active;
            PrevButton.IsEnabled = active;
            SkipButton.IsEnabled = active;
            AutoTButton.IsEnabled = active;
            AutoSpeedSlider.IsEnabled = active;
        }

        private void ShowDiffusionBoxes(bool show)
        {
            IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (show)
                {
                    enumerator.Current.Visibility = Visibility.Visible;
                }
                else
                {
                    enumerator.Current.Visibility = Visibility.Hidden;
                }
            }
        }

        private void GetDiffusionBoxes()
        {
            diffusionBoxes = DiffusionGrid.Children.OfType<CheckBox>();
        }

        /// <summary>
        /// This method compares each character of the two strings. If the character at a position is different
        /// in the two strings this position is added to a list.
        /// </summary>
        /// <returns>A list of the positions with different characters</returns>
        private List<byte> CompareStrings(string old, string changed)
        {
            List<byte> tmp = new List<byte>();
            char[] oldArray = old.ToCharArray();
            char[] changedArray = changed.ToCharArray();

            for (byte i = 0; i < old.Length; i++)
            {
                if (!oldArray[i].Equals(changedArray[i]))
                {
                    tmp.Add(i);
                }
            }
            return tmp;
        }

        private void SwitchStringBit(TextBlock text, int pos)
        {
            string tmp = text.Text;
            if (tmp.ElementAt(pos).Equals('0'))
            {
                tmp = tmp.Remove(pos, 1);
                tmp = tmp.Insert(pos, "1");
            }
            else
            {
                tmp = tmp.Remove(pos, 1);
                tmp = tmp.Insert(pos, "0");
            }
            text.Text = tmp;

        }

        /// <summary>
        /// This method colors the characters at the given positions of a TextBlock red. 
        /// </summary>
        /// <param name="text">TextBlock which will be colored</param>
        /// <param name="pos">Contains all positions which characters will be colored</param>
        private void ColorText(TextBlock text, List<byte> pos)
        {
            byte[] changePos = pos.ToArray();
            text.TextEffects.Clear();
            for (byte i = 0; i < changePos.Length; i++)
            {
                ColorTextSingle(text, changePos[i]);
            }

        }

        private void ColorTextSingle(TextBlock text, byte pos)
        {
            TextEffect te = new TextEffect
            {
                PositionStart = pos,
                Foreground = Brushes.Red,
                PositionCount = 1
            };
            text.TextEffects.Add(te);
        }

        private byte[] StringToByteArray(string str)
        {
            char[] strArray = str.ToCharArray();
            string[] byteStrings = new string[8];
            byteStrings[0] = "" + strArray[0] + strArray[1] + strArray[2] + strArray[3] + strArray[4] + strArray[5] + strArray[6] + strArray[7];
            byteStrings[1] = "" + strArray[8] + strArray[9] + strArray[10] + strArray[11] + strArray[12] + strArray[13] + strArray[14] + strArray[15];
            byteStrings[2] = "" + strArray[16] + strArray[17] + strArray[18] + strArray[19] + strArray[20] + strArray[21] + strArray[22] + strArray[23];
            byteStrings[3] = "" + strArray[24] + strArray[25] + strArray[26] + strArray[27] + strArray[28] + strArray[29] + strArray[30] + strArray[31];
            byteStrings[4] = "" + strArray[32] + strArray[33] + strArray[34] + strArray[35] + strArray[36] + strArray[37] + strArray[38] + strArray[39];
            byteStrings[5] = "" + strArray[40] + strArray[41] + strArray[42] + strArray[43] + strArray[44] + strArray[45] + strArray[46] + strArray[47];
            byteStrings[6] = "" + strArray[48] + strArray[49] + strArray[50] + strArray[51] + strArray[52] + strArray[53] + strArray[54] + strArray[55];
            byteStrings[7] = "" + strArray[56] + strArray[57] + strArray[58] + strArray[59] + strArray[60] + strArray[61] + strArray[62] + strArray[63];
            byte[] byteBytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                byteBytes[i] = Convert.ToByte(byteStrings[i], 2);
            }
            return byteBytes;

        }

        private string BinStringToHexString(string strBinary)
        {
            string strHex = Convert.ToInt64(strBinary, 2).ToString("X");
            while (strHex.Length < 16)
            {
                strHex = strHex.Insert(0, "0");
            }
            for (int i = 14; i >= 2; i = i - 2)
            {
                strHex = strHex.Insert(i, " ");
            }
            return strHex;
        }

        private string InsertSpaces(string str)
        {
            StringBuilder builder = new StringBuilder();
            char[] strArray = str.ToCharArray();
            for (int i = 0; i < strArray.Length; i++)
            {
                builder.Append(strArray[i] + "  ");
            }
            return builder.ToString();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            ExecuteNextStep();
        }

        private void ActivateRoundButtons(bool active)
        {
            Round1Button.IsEnabled = active;
            Round2Button.IsEnabled = active;
            Round3Button.IsEnabled = active;
            Round4Button.IsEnabled = active;
            Round5Button.IsEnabled = active;
            Round6Button.IsEnabled = active;
            Round7Button.IsEnabled = active;
            Round8Button.IsEnabled = active;
            Round9Button.IsEnabled = active;
            Round10Button.IsEnabled = active;
            Round11Button.IsEnabled = active;
            Round12Button.IsEnabled = active;
            Round13Button.IsEnabled = active;
            Round14Button.IsEnabled = active;
            Round15Button.IsEnabled = active;
            Round16Button.IsEnabled = active;

        }

        private void ClearButtonsColor(bool topOnly)
        {
            if (topOnly)
            {
                ShiftCButton.ClearValue(BackgroundProperty);
                ShiftDButton.ClearValue(BackgroundProperty);
                PC2Button.ClearValue(BackgroundProperty);
                ExpansionButton.ClearValue(BackgroundProperty);
                KeyAdditionButton.ClearValue(BackgroundProperty);
                SBoxButton.ClearValue(BackgroundProperty);
                PermutationButton.ClearValue(BackgroundProperty);
                RoundAdditionButton.ClearValue(BackgroundProperty);
            }
            else
            {
                IntroButton.ClearValue(BackgroundProperty);
                DataButton.ClearValue(BackgroundProperty);
                PC1Button.ClearValue(BackgroundProperty);
                KeyScheduleButton.ClearValue(BackgroundProperty);
                IPButton.ClearValue(BackgroundProperty);
                DESButton.ClearValue(BackgroundProperty);
                FPButton.ClearValue(BackgroundProperty);
                SummaryButton.ClearValue(BackgroundProperty);
                Round1Button.ClearValue(BackgroundProperty);
                Round2Button.ClearValue(BackgroundProperty);
                Round3Button.ClearValue(BackgroundProperty);
                Round4Button.ClearValue(BackgroundProperty);
                Round5Button.ClearValue(BackgroundProperty);
                Round6Button.ClearValue(BackgroundProperty);
                Round7Button.ClearValue(BackgroundProperty);
                Round8Button.ClearValue(BackgroundProperty);
                Round9Button.ClearValue(BackgroundProperty);
                Round10Button.ClearValue(BackgroundProperty);
                Round11Button.ClearValue(BackgroundProperty);
                Round12Button.ClearValue(BackgroundProperty);
                Round13Button.ClearValue(BackgroundProperty);
                Round14Button.ClearValue(BackgroundProperty);
                Round15Button.ClearValue(BackgroundProperty);
                Round16Button.ClearValue(BackgroundProperty);
            }

        }

        private void ColorRoundButton()
        {
            ClearButtonsColor(false);
            switch (roundCounter)
            {
                case 1: Round1Button.Background = buttonBrush; break;
                case 2: Round2Button.Background = buttonBrush; break;
                case 3: Round3Button.Background = buttonBrush; break;
                case 4: Round4Button.Background = buttonBrush; break;
                case 5: Round5Button.Background = buttonBrush; break;
                case 6: Round6Button.Background = buttonBrush; break;
                case 7: Round7Button.Background = buttonBrush; break;
                case 8: Round8Button.Background = buttonBrush; break;
                case 9: Round9Button.Background = buttonBrush; break;
                case 10: Round10Button.Background = buttonBrush; break;
                case 11: Round11Button.Background = buttonBrush; break;
                case 12: Round12Button.Background = buttonBrush; break;
                case 13: Round13Button.Background = buttonBrush; break;
                case 14: Round14Button.Background = buttonBrush; break;
                case 15: Round15Button.Background = buttonBrush; break;
                case 16: Round16Button.Background = buttonBrush; break;
            }
        }

        #endregion Helper-Methods

    }
}

