/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System;
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase.Miscellaneous;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;
// for QuickwatchPresentation
// for RegEx
using System.Text.RegularExpressions;
// for Brushes
using System.Windows.Media;

namespace CrypTool.LFSR
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("CrypTool.LFSR.Properties.Resources", "PluginCaption", "PluginTooltip", "LFSR/DetailedDescription/doc.xml", "LFSR/Images/LFSR.png", "LFSR/Images/encrypt.png", "LFSR/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class LFSR : ICrypComponent
    {
        #region IPlugin Members

        private LFSRSettings settings;
        private String inputTapSequence;
        private String inputSeed;
        private String outputString;
        private bool outputBool;
        private bool inputClockBool = true;
        private bool outputClockingBit;
        private bool[] outputBoolArray;
        
        private LFSRPresentation lFSRPresentation;

        #endregion

        #region public variables

        public bool stop = false;
        public bool newSeed = true;
        public String seedbuffer = null;
        public String tapSequencebuffer = "1";
        public Char outputbuffer = '0';
        public bool lastInputPropertyWasBoolClock = false;

        // for process()
        public char[] tapSequenceCharArray = {'0'}; // dummy value for preprocessing()
        public int seedBits = 1; // dummy value for compiler
        public int actualRounds = 1; // dummy value for compiler
        public Boolean myClock = true;
        public char[] seedCharArray = null;
        public int clocking;
        public string outputStringBuffer = null;
        public char outputBit;

        #endregion

        #region public interfaces

        public LFSR()
        {
            this.settings = new LFSRSettings();
            settings.PropertyChanged += settings_PropertyChanged;

            lFSRPresentation = new LFSRPresentation();
            Presentation = lFSRPresentation;
            //lFSRPresentation.textBox0.TextChanged += textBox0_TextChanged;
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InitLFSR")
                try
                {
                    preprocessLFSR(true);
                }
                catch (Exception) { }

            if (e.PropertyName == "SaveCurrentState")
                settings.CurrentState = settings.SaveCurrentState ? seedbuffer : null;

            if (e.PropertyName == "Polynomial" || e.PropertyName == "Seed")
                setPeriod();
        }

        void setPeriod()
        {
            try
            {
                int period = computePeriod();
                settings.Period = period + (period == (1 << seedbuffer.Length) - 1 ? " (max.)" : "");
            }
            catch (Exception)
            {
            }
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (LFSRSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputTapSequenceCaption", "InputTapSequenceTooltip", false)]
        public String InputTapSequence
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputTapSequence; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputTapSequence = value;
                OnPropertyChanged("InputTapSequence");
                lastInputPropertyWasBoolClock = false;
            }
        }

        [PropertyInfo(Direction.InputData, "InputSeedCaption", "InputSeedTooltip", false)]
        public String InputSeed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputSeed; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputSeed = value;
                OnPropertyChanged("InputSeed");
                lastInputPropertyWasBoolClock = false;
            }
        }

        [PropertyInfo(Direction.InputData, "InputClockBoolCaption", "InputClockBoolTooltip", false)]
        public Boolean InputClockBool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputClockBool; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputClockBool = value;
                OnPropertyChanged("InputClockBool");
                lastInputPropertyWasBoolClock = true;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public String OutputString
        {
            get { return outputString; }
            set
            {
                outputString = value.ToString();
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBoolCaption", "OutputBoolTooltip", false)]
        public bool OutputBool
        {
            get { return outputBool; }
            set
            {
                outputBool = (bool)value;
                //OnPropertyChanged("OutputBool");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputClockingBitCaption", "OutputClockingBitTooltip", false)]
        public bool OutputClockingBit
        {
            get { return outputClockingBit; }
            set
            {
                outputClockingBit = (bool)value;
                OnPropertyChanged("OutputClockingBit");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBoolArrayCaption", "OutputBoolArrayTooltip", false)]
        public bool[] OutputBoolArray
        {
            get { return outputBoolArray; }
            set
            {
                outputBoolArray = value;
                OnPropertyChanged("OutputBoolArray");
            }
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                outputString = null;
                outputStringBuffer = null;
                inputTapSequence = null;
                inputSeed = null;
                newSeed = true;
                seedbuffer = "0";
                tapSequencebuffer = "1";
                outputbuffer = '0';
                lastInputPropertyWasBoolClock = false;

                // for process()
                tapSequenceCharArray = null;
                seedBits = 1; // dummy value for compiler
                actualRounds = 1; // dummy value for compiler
                myClock = true;
                seedCharArray = null;
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error, true);
            }
            this.stop = false;
        }

        #endregion

        #region private functions

        private int checkForInputTapSequence()
        {
            /*if ((settings.Polynomial == null || (settings.Polynomial != null && settings.Polynomial.Length == 0)))
            {
                // awaiting polynomial from input
                GuiLogMessage("No feedback polynomial given in settings. Awaiting external input.", NotificationLevel.Info, true);
            }
            else
            {
                inputTapSequence = settings.Polynomial;
                return 1;
            }*/

            if ((inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0)) && (settings.Polynomial == null || (settings.Polynomial != null && settings.Polynomial.Length == 0)))
            {
                // create some input
                String dummystring = "1011";
                // this.inputTapSequence = new String();
                inputTapSequence = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No TapSequence provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning, true);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private int checkForInputSeed()
        {
            /*if ((settings.Seed == null || (settings.Seed != null && settings.Seed.Length == 0)))
            {
                // awaiting seed from input
                GuiLogMessage("No seed given in settings. Awaiting external input.", NotificationLevel.Info, true);
            }
            else
            {
                inputSeed = settings.Seed;
                return 1;
            }*/

            if ((inputSeed == null || (inputSeed != null && inputSeed.Length == 0)) && (settings.Seed == null || (settings.Seed != null && settings.Seed.Length == 0)))
            {
                // create some input
                String dummystring = "1010";
                // this.inputSeed = new CrypToolStream();
                inputSeed = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No Seed provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning, true);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        // Function to make binary representation of polynomial
        private String MakeBinary(String strPoly)
        {
            bool gotX = false;
            // remove spaces
            strPoly = strPoly.Replace(" ", "");

            Regex gotXRegEx = new Regex("(\\+x\\+1)+$");
            if (gotXRegEx.IsMatch(strPoly)) gotX = true;
            // remove last '1'
            strPoly = strPoly.Remove(strPoly.Length - 1, 1);
            // remove all x
            strPoly = strPoly.Replace("x", "");
            // remove all ^
            strPoly = strPoly.Replace("^", "");

            // split in string array
            string[] strPolySplit = strPoly.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
            // convert to integer
            int[] intPolyInteger = new int[strPolySplit.Length];
            for (int i = strPolySplit.Length - 1; i >= 0; i--)
            {
                intPolyInteger[i] = Convert.ToInt32(strPolySplit[i]);
            }

            string strPolyBinary = null;
            if (strPoly.Length != 0)
            {
                Array.Sort(intPolyInteger);
                int highestOne = intPolyInteger[intPolyInteger.Length - 1];

                int j = intPolyInteger.Length - 1;
                for (int i = highestOne; i > 1; i--)
                {
                    if (j < 0 && (i != 1 || i != 0)) strPolyBinary += 0;
                    else if (intPolyInteger[j] == i)
                    {
                        strPolyBinary += 1;
                        j--;
                    }
                    else strPolyBinary += 0;
                }
            }
            if (gotX) strPolyBinary += 1;
            else strPolyBinary += 0;

            strPolyBinary = new String(ReverseOrder(strPolyBinary.ToCharArray()));
            //GuiLogMessage("strPolyBinary is: " + strPolyBinary, NotificationLevel.Info, true);

            return strPolyBinary;
        }

        // Function to test for LFSR Polnyomial
        private bool IsPolynomial(String strPoly)
        {
            // delete spaces
            strPoly = strPoly.Replace(" ", "");
            //(x\^([2-9]|[0-9][0-9])\+)*[x]?([\+]?1){1}
            Regex objPolynomial = new Regex("(x\\^([2-9]|[0-9][0-9])\\+)+[x]?([\\+]?1){1}");
            Regex keinHoch0oder1 = new Regex("(x\\^[0-1]\\+)+");
            return !keinHoch0oder1.IsMatch(strPoly) && objPolynomial.IsMatch(strPoly);
        }

        // Function to turn around tapSequence (01101 -> 10110)
        private char[] ReverseOrder(char[] tapSequence)
        {
            //String tempString = new String(tapSequence);
            //GuiLogMessage("tapSequence before = " + tempString, NotificationLevel.Info, true);
            char[] tempCharArray = new char[tapSequence.Length];

            int temp;
            for (int j = tapSequence.Length - 1; j >= 0; j--)
            {
                temp = (j - tapSequence.Length + 1) % (tapSequence.Length);
                if (temp < 0) temp *= -1;
                //GuiLogMessage("temp = " + temp, NotificationLevel.Info, true);
                tempCharArray[j] = tapSequence[temp];
            }
            //tempString = new String(tempCharArray);
            //GuiLogMessage("tapSequence after = " + tempString, NotificationLevel.Info, true);
            return tempCharArray;
        }

        private string BuildPolynomialFromBinary(char [] tapSequence)
        {
            string polynomial = CrypTool.LFSR.Properties.Resources.Feedback_polynomial + ": \n";
            char[] tempTapSequence = ReverseOrder(tapSequence);
            int power;

            //build polynomial
            for (int i = 0; i < tapSequence.Length; i++)
            {
                power = (i - tapSequence.Length + 1) * -1 % tapSequence.Length + 1;
                if (tempTapSequence[i] == '1')
                {
                    if (power == 1) polynomial += "x + ";
                    else if (power != 0) polynomial += "x^" + power + " + ";
                    //else polynomial += "1";
                }
            }
            // add last "+1"
            polynomial += "1";

            return  polynomial;
        }

        private int computePeriod()
        {
            int period = 0;

            try
            {
                preprocessLFSR(false);
            }
            catch (Exception) { }

            if (lFSRPresentation.ReturnBackgroundColour() == Brushes.White)
            {
                try {
                    // compute period if everything is OK
                    string compareToSeedbuffer = seedbuffer;

                    for (int i = 1; i <= 1000; i++)
                    {
                        char newBit = '0';

                        // compute new bit
                        bool firstDone = false;
                        for (int j = 0; j < seedBits; j++)
                        {
                            // check if tapSequence is 1
                            if (tapSequenceCharArray[j] == '1')
                            {
                                // if it is the first one, just take it
                                if (!firstDone)
                                {
                                    newBit = seedCharArray[j];
                                    firstDone = true;
                                }
                                // or do an XOR with the last computed bit
                                else
                                {
                                    newBit = (newBit ^ seedCharArray[j]).ToString()[0];
                                }
                            }
                        }

                        // shift seed array
                        for (int j = seedBits - 1; j > 0; j--)
                        {
                            seedCharArray[j] = seedCharArray[j - 1];
                        }
                        seedCharArray[0] = newBit;

                        // write current "seed" back to seedbuffer
                        seedbuffer = null;
                        foreach (char c in seedCharArray) seedbuffer += c;

                        if (seedbuffer == compareToSeedbuffer)
                            return i;
                    }
                } catch (Exception) {

                }
            }

            return period;
        }

        #endregion

        public void Execute()
        {
            processLFSR();
        }

        private void preprocessLFSR(bool createLog)
        {
            if (checkForInputTapSequence() == 1) return;
            if (checkForInputSeed() == 1) return;

            // read tapSequence
            if (settings.Polynomial == null || (settings.Polynomial != null && settings.Polynomial.Length == 0))
                tapSequencebuffer = inputTapSequence;
            else
                tapSequencebuffer = settings.Polynomial;

            //read seed
            if (settings.SaveCurrentState && settings.CurrentState != null && settings.CurrentState.Length != 0 && settings.CurrentState != "0")
                seedbuffer = settings.CurrentState;
            else if (settings.Seed == null || (settings.Seed != null && settings.Seed.Length == 0))
                seedbuffer = inputSeed;
            else
                seedbuffer = settings.Seed;

            // check if tapSequence is binary
            bool tapSeqisBool = true;
            foreach (char character in tapSequencebuffer)
            {
                if (character != '0' && character != '1')
                {
                    tapSeqisBool = false;
                    //return;
                }
            }

            // if tapSequence is not binary, await polynomial
            if (!tapSeqisBool)
            {
                GuiLogMessage("TapSequence is not binary. Awaiting polynomial.", NotificationLevel.Info, createLog);
                if (IsPolynomial(tapSequencebuffer))
                {
                    GuiLogMessage(tapSequencebuffer + " is a valid polynomial.", NotificationLevel.Info, createLog);
                    tapSequencebuffer = MakeBinary(tapSequencebuffer);
                    GuiLogMessage("Polynomial in binary form: " + tapSequencebuffer, NotificationLevel.Info, createLog);

                    // check if polynomial has false length
                    if (tapSequencebuffer.Length != seedbuffer.Length)
                    {
                        /*// check if its too long
                        if (inputSeed.Length - tapSequencebuffer.Length < 0)
                        {
                            GuiLogMessage("ERROR - Your polynomial " + tapSequencebuffer + " is TOO LONG (" + tapSequencebuffer.Length + " Bits) for your seed. Aborting now.", NotificationLevel.Error, true);
                            if (!settings.UseBoolClock) inputClock.Close();
                            return;
                        }
                        // seems to be too short, so fill it with zeros at the beginning
                        else
                        {
                            for (int j = inputSeed.Length - tapSequencebuffer.Length; j > 0; j--)
                            {
                                tapSequencebuffer = "0" + tapSequencebuffer;
                            }
                        }*/
                        GuiLogMessage("ERROR - Your polynomial " + tapSequencebuffer + " has to be the same length (" + tapSequencebuffer.Length + " Bits) as your seed (" + seedbuffer.Length + " Bits). Aborting now.", NotificationLevel.Error, createLog);
                        Dispose();
                        return;
                    }
                }
                else
                {
                    GuiLogMessage("ERROR - " + tapSequencebuffer + " is NOT a valid polynomial. Aborting now.", NotificationLevel.Error, createLog);
                    Dispose();
                    return;
                }

                // convert tapSequence into char array
                tapSequenceCharArray = tapSequencebuffer.ToCharArray();
            }
            else
            {
                GuiLogMessage("Polynomial in binary form: " + tapSequencebuffer, NotificationLevel.Info, createLog);
                // convert tapSequence into char array
                tapSequenceCharArray = ReverseOrder(tapSequencebuffer.ToCharArray());
            }

            if (tapSequencebuffer.Length != seedbuffer.Length)
            {
                // stop, because seed and tapSequence must have same length
                GuiLogMessage("ERROR - Seed and tapSequence must have same length. Aborting now.", NotificationLevel.Error, createLog);
                Dispose();
                return;
            }

            int tapSequenceBits = tapSequencebuffer.Length;
            seedBits = seedbuffer.Length;

            //check if last tap is 1, otherwise stop
            if (tapSequenceCharArray[tapSequenceCharArray.Length - 1] == '0')
            {
                GuiLogMessage("ERROR - First tap of tapSequence must be 1. Aborting now.", NotificationLevel.Error, createLog);
                return;
            }

            // convert seed into char array
            seedCharArray = seedbuffer.ToCharArray();

            // check if seed is binary
            foreach (char character in seedCharArray)
            {
                if (character != '0' && character != '1')
                {
                    GuiLogMessage("ERROR - Seed has to be binary. Aborting now. Character is: " + character, NotificationLevel.Error, createLog);
                    return;
                }
            }
            if (settings.UseAdditionalOutputBit)
            {
                if (settings.ClockingBit < seedCharArray.Length) clocking = (seedCharArray.Length - settings.ClockingBit - 1);
                else
                {
                    clocking = -1;
                    GuiLogMessage("WARNING: Clocking Bit is too high. Ignored.", NotificationLevel.Warning, createLog);
                }

            }
            else clocking = -1;

            // check if Rounds are given
            int defaultRounds = 10;

            // check if Rounds in settings are given and use them only if no bool clock is selected
            if (!settings.UseBoolClock)
            {
                if (settings.Rounds == 0) actualRounds = defaultRounds; else actualRounds = settings.Rounds;
            }
            else actualRounds = 1;

            // draw LFSR Quickwatch
            if (!settings.NoQuickwatch)
            {
                lFSRPresentation.DeleteAll(100);
                lFSRPresentation.DrawLFSR(seedCharArray, tapSequenceCharArray, clocking);
                lFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, ' ', getNewBit(), BuildPolynomialFromBinary(tapSequenceCharArray));
            }
        }

        private void processLFSR()
        {
            settings.PluginIsRunning = true;
            // check if event was from the boolean clock input
            // if so, check if boolean clock should be used
            // if not, do not process LFSR
            if (lastInputPropertyWasBoolClock)
            {
                if (!settings.UseBoolClock) return;
                //GuiLogMessage("First if.", NotificationLevel.Info, true);
            }
                // if last event wasn't from the clock but clock shall be
                // the only event to start from, do not go on
            else
            {
                // do nothing if we should use bool clock, but become event from other inputs
                if (settings.UseBoolClock) return;
            }
            // process LFSR
            
            try
            {
                // make all this stuff only one time at the beginning of our chainrun
                if (newSeed)
                    setPeriod();
                
                // Here we go!
                // check which clock to use
                if (settings.UseBoolClock)
                {
                    myClock = inputClockBool;
                }
                else if (!settings.UseBoolClock)
                {
                    myClock = true;
                }

                // (re-)draw LFSR Quickwatch
                if (!settings.NoQuickwatch)
                {
                    lFSRPresentation.DeleteAll(100);
                    lFSRPresentation.DrawLFSR(seedCharArray, tapSequenceCharArray, clocking);
                }

                //GuiLogMessage("Action is: Now!", NotificationLevel.Debug, true);
                //DateTime startTime = DateTime.Now;

                //////////////////////////////////////////////////////
                // compute LFSR //////////////////////////////////////
                //////////////////////////////////////////////////////
                
                int i = 0;
                
                for (i = 0; i < actualRounds; i++)
                {
                    // compute only if clock = 1 or true
                    if (myClock)
                    {
                        StatusChanged((int)LFSRImage.Encode);

                        // make bool output
                        outputBool = seedCharArray[seedBits - 1] == '1';

                        // write last bit to output buffer, output stream buffer, stream and bool
                        outputbuffer = seedCharArray[seedBits - 1];
                        outputStringBuffer += seedCharArray[seedBits - 1];

                        // update outputs
                        OnPropertyChanged("OutputBool");

                        // shift seed array

                        // keep output bit for presentation
                        outputBit = seedCharArray[seedBits - 1];

                        // shift seed array
                        char newBit = getNewBit();

                        for (int j = seedBits - 1; j > 0; j--)
                            seedCharArray[j] = seedCharArray[j - 1];

                        seedCharArray[0] = newBit;

                        //update quickwatch presentation
                        if (!settings.NoQuickwatch)
                        {
                            lFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputBit, getNewBit(), BuildPolynomialFromBinary(tapSequenceCharArray));
                        }

                        // write current "seed" back to seedbuffer
                        seedbuffer = null;
                        foreach (char c in seedCharArray) seedbuffer += c;
                    }
                    else // clock is false
                    {
                        StatusChanged((int)LFSRImage.Decode);

                        if (settings.AlwaysCreateOutput)
                        {
                            
                            // make bool output = 0 if it is the first round
                            if (newSeed)
                            {
                                outputBool = false;
                                outputbuffer = '0';
                                outputBit = '0';
                                // write last bit to output buffer, output stream buffer, stream and bool
                                outputbuffer = outputBit;
                                outputStringBuffer += outputBit;
                                OnPropertyChanged("OutputBool");
                            }
                            else
                            {
                                if (outputBit == '0')
                                    outputBool = false;
                                else
                                    outputBool = true;
                                // write last bit to output buffer, output stream buffer, stream and bool
                                outputbuffer = outputBit;
                                outputStringBuffer += outputBit;
                                OnPropertyChanged("OutputBool");
                            }

                            // update quickwatch presentation
                            if (!settings.NoQuickwatch)
                            {
                                lFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputbuffer, getNewBit(), BuildPolynomialFromBinary(tapSequenceCharArray));
                            }

                        }
                        else
                        {
                            // update quickwatch with current state but without any output bit
                            if (!settings.NoQuickwatch)
                            {
                                lFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, ' ', getNewBit(), BuildPolynomialFromBinary(tapSequenceCharArray));
                            }
                        }
                    }
                    // in both cases update additional output bit if set in settings
                    if (settings.UseAdditionalOutputBit)
                    {
                        // make clocking bit output only if its not out of bounds
                        if (clocking != -1)
                        {
                            if (seedCharArray[clocking] == '0') outputClockingBit = false;
                            else outputClockingBit = true;
                            OnPropertyChanged("OutputClockingBit");
                        }
                    }
                    // in both cases also output all stages if set in settings
                    if (settings.OutputStages)
                    {
                        outputBoolArray = MakeBooleanArrayFromCharArray(seedCharArray);
                        OnPropertyChanged("OutputBoolArray");
                    }

                    // reset newSeed
                    newSeed = false;
                }

                if (!stop)
                {
                    // finally write output string
                    outputString = outputStringBuffer;
                    OnPropertyChanged("OutputString");
                }

                if (stop)
                {
                    outputStringBuffer = null;
                    GuiLogMessage("Aborted!", NotificationLevel.Debug, true);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error, true);
            }
            finally
            {
                ProgressChanged(1, 1);
                settings.PluginIsRunning = false;
            }
        }

        private char getNewBit()
        {
            bool bit = false;

            for (int j = 0; j < seedBits; j++)
                bit ^= (tapSequenceCharArray[j] == '1') && (seedCharArray[j] == '1');

            return bit ? '1' : '0';
        }

        private bool[] MakeBooleanArrayFromCharArray(char[] charArray)
        {
            bool[] boolArray = new bool[charArray.Length];

            for (int i = 0; i < charArray.Length; i++)
                boolArray[i] = charArray[i] != '0';

            return boolArray;
        }

        #region events and stuff

        public void Initialize()
        {
            lFSRPresentation.ChangeBackground(NotificationLevel.Info);
            settings.UpdateTaskPaneVisibility();
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel, bool createLogOrChangeBackground)
        {
            if (createLogOrChangeBackground) EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
            else lFSRPresentation.ChangeBackground(logLevel);
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            settings.PluginIsRunning = true;
            try
            {
                if (settings.SaveCurrentState)
                    settings.CurrentState = seedbuffer;
                else
                    settings.CurrentState = null;
                Dispose();
            }
            finally
            {
                settings.PluginIsRunning = false;
            }
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            StatusChanged((int)LFSRImage.Default);
            newSeed = true;
            stop = true;
        }

        public UserControl Presentation { get; private set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            //EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    #region Image

    enum LFSRImage
    {
        Default,
        Encode,
        Decode
    }

    #endregion
}
