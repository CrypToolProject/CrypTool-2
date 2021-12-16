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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;
// for QuickwatchPresentation
// for RegEx
using System.Text.RegularExpressions;
using System.Windows.Controls;
// MathParser

namespace CrypTool.NLFSR
{
    [Author("Soeren Rinne", "soeren.rinne@CrypTool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("CrypTool.NLFSR.Properties.Resources", "PluginCaption", "PluginTooltip", "NLFSR/DetailedDescription/doc.xml", "NLFSR/Images/NLFSR.png", "NLFSR/Images/encrypt.png", "NLFSR/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.ToolsRandomNumbers)]
    public class NLFSR : ICrypComponent
    {
        #region IPlugin Members

        private NLFSRSettings settings;
        private string inputTapSequence;
        private string inputSeed;
        private string outputString;
        private bool outputBool;
        private bool inputClockBool;
        private bool outputClockingBit;
        //private string tapSequenceString = null;

        private readonly NLFSRPresentation NLFSRPresentation;

        #endregion

        #region public variables

        public bool stop = false;
        public bool newSeed = true;
        public string seedbuffer = "0";
        public string tapSequencebuffer = "1";
        public char outputbuffer = '0';
        public bool lastInputPropertyWasBoolClock = false;

        // for process()
        public char[] tapSequenceCharArray = null;
        public int seedBits = 1; // dummy value for compiler
        public int actualRounds = 1; // dummy value for compiler
        public bool myClock = true;
        public char[] seedCharArray = null;
        public int clocking;
        public string outputStringBuffer = null;
        public char outputBit;

        #endregion

        #region public interfaces

        public NLFSR()
        {
            settings = new NLFSRSettings();
            settings.PropertyChanged += settings_PropertyChanged;

            NLFSRPresentation = new NLFSRPresentation();
            Presentation = NLFSRPresentation;
            //NLFSRPresentation.textBox0.TextChanged += textBox0_TextChanged;
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InitNLFSR")
            {
                preprocessNLFSR();
            }

            if (e.PropertyName == "SaveCurrentState")
            {
                if (settings.SaveCurrentState)
                {
                    settings.CurrentState = seedbuffer;
                }
                else
                {
                    settings.CurrentState = null;
                }
            }
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (NLFSRSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputTapSequenceCaption", "InputTapSequenceTooltip", false)]
        public string InputTapSequence
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputTapSequence;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputTapSequence = value;
                OnPropertyChanged("InputTapSequence");
                lastInputPropertyWasBoolClock = false;
            }
        }

        [PropertyInfo(Direction.InputData, "InputSeedCaption", "InputSeedTooltip", false)]
        public string InputSeed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputSeed;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputSeed = value;
                OnPropertyChanged("InputSeed");
                lastInputPropertyWasBoolClock = false;
            }
        }

        [PropertyInfo(Direction.InputData, "InputClockBoolCaption", "InputClockBoolTooltip", false)]
        public bool InputClockBool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputClockBool;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputClockBool = value;
                OnPropertyChanged("InputClockBool");
                lastInputPropertyWasBoolClock = true;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value.ToString();
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBoolCaption", "OutputBoolTooltip", false)]
        public bool OutputBool
        {
            get => outputBool;
            set => outputBool = value;//OnPropertyChanged("OutputBool");
        }

        private bool[] outputBoolArray = new bool[1];
        [PropertyInfo(Direction.OutputData, "OutputBoolArrayCaption", "OutputBoolArrayTooltip", false)]
        public bool[] OutputBoolArray
        {
            get => outputBoolArray;
            set => outputBoolArray = value;//OnPropertyChanged("OutputBool");
        }

        [PropertyInfo(Direction.OutputData, "OutputClockingBitCaption", "OutputClockingBitTooltip", false)]
        public bool OutputClockingBit
        {
            get => outputClockingBit;
            set
            {
                outputClockingBit = value;
                OnPropertyChanged("OutputClockingBit");
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
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            stop = false;
        }

        #endregion

        #region private functions

        private int checkForInputTapSequence()
        {
            if ((inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0)) && (settings.Polynomial == null || (settings.Polynomial != null && settings.Polynomial.Length == 0)))
            {
                // create some input
                string dummystring = "x0 * x1";
                // this.inputTapSequence = new String();
                inputTapSequence = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No TapSequence provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private int checkForInputSeed()
        {
            if ((inputSeed == null || (inputSeed != null && inputSeed.Length == 0)) && (settings.Seed == null || (settings.Seed != null && settings.Seed.Length == 0)))
            {
                // create some input
                string dummystring = "1010";
                // this.inputSeed = new CrypToolStream();
                inputSeed = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No Seed provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        // Function to make binary representation of polynomial
        /*private String MakeBinary(String strPoly)
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
            //GuiLogMessage("strPolyBinary is: " + strPolyBinary, NotificationLevel.Info);

            return strPolyBinary;
        }*/

        // Function to test for NLFSR Polnyomial
        private bool IsPolynomial(string strPoly)
        {
            // delete spaces
            strPoly = strPoly.Replace(" ", "");
            //(x\^([2-9]|[0-9][0-9])\+)*[x]?([\+]?1){1}
            // TODO
            //Regex objPolynomial = new Regex("(x\\^([0-9]|[1-9][0-9])([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^]))*");
            Regex objBoolExpression = new Regex("([0-1]([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^]))*");
            //Regex keinHoch0 = new Regex("(x\\^[0]\\+)+");
            //return !keinHoch0.IsMatch(strPoly) && objBoolExpression.IsMatch(strPoly);
            return objBoolExpression.IsMatch(strPoly);
        }

        // Function to turn around tapSequence (01101 -> 10110)
        private char[] ReverseOrder(char[] tapSequence)
        {
            //String tempString = new String(tapSequence);
            //GuiLogMessage("tapSequence before = " + tempString, NotificationLevel.Info);
            char[] tempCharArray = new char[tapSequence.Length];

            int temp;
            for (int j = tapSequence.Length - 1; j >= 0; j--)
            {
                temp = (j - tapSequence.Length + 1) % (tapSequence.Length);
                if (temp < 0)
                {
                    temp *= -1;
                }
                //GuiLogMessage("temp = " + temp, NotificationLevel.Info);
                tempCharArray[j] = tapSequence[temp];
            }
            //tempString = new String(tempCharArray);
            //GuiLogMessage("tapSequence after = " + tempString, NotificationLevel.Info);
            return tempCharArray;
        }

        /*private string BuildPolynomialFromBinary(char [] tapSequence)
        {
            string polynomial = "Feedback polynomial: \n";
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
        }*/

        private string makeStarsInText(Match m)
        {
            // Get the matched string.
            string x = m.ToString();
            // insert an * before the x
            x = x.Insert(x.Length - 1, "*");
            return x;
        }


        private string ReplaceVariables(string strExpressionWithVariables, char[] currentState)
        {
            // remove all spaces in function
            string strExpression = strExpressionWithVariables.Replace(" ", "");
            // first, lets put in *, if there aren't any (and should be there)
            // example: x^2+x^2x^3 ==> x^2+x^2*x^3
            Regex makeStars = new Regex("([0-9])x");
            strExpression = makeStars.Replace(strExpression, new MatchEvaluator(makeStarsInText));

            //GuiLogMessage("strExpression /w vars: " + strExpression, NotificationLevel.Info);
            char[] strFSRValues = new char[currentState.Length]; // fix the length

            // TODO: [i-1] ist falschrum -> gelöst durch ReverseOrder?
            char[] temp = new char[currentState.Length];
            temp = ReverseOrder(currentState);
            string replacement = null;

            for (int i = strFSRValues.Length - 1; i >= 0; i--)
            {
                replacement = "x" + i;
                strExpression = strExpression.Replace(replacement, temp[i].ToString());
                //GuiLogMessage("temp[i-1]: " + temp[i - 1].ToString(), NotificationLevel.Info);
            }
            //strExpression = strExpression.Replace("x", currentState[currentState.Length-1].ToString());

            // replace AND, NAND, OR, NOR, XOR, NXOR with symbols
            // NAND => -
            //strExpression = strExpression.Replace("NAND", "-");
            // AND => +
            strExpression = strExpression.Replace("AND", "*");

            // NOR => _
            //strExpression = strExpression.Replace("NOR", "_");

            // NXOR => °
            //strExpression = strExpression.Replace("NXOR", "°");
            // XOR => *
            strExpression = strExpression.Replace("XOR", "+");

            // OR => |
            //strExpression = strExpression.Replace("OR", "|");

            // replace ^ and v with symbols
            // ^ => AND => +
            //strExpression = strExpression.Replace("^", "+");

            // v => OR => |
            //strExpression = strExpression.Replace("v", "|");

            //GuiLogMessage("strExpression w/o vars: " + strExpression, NotificationLevel.Info);

            return strExpression;
        }

        // solves string with variables replaced by values
        private bool EvaluateString(string function)
        {
            // test for AND aka '*'
            int positionAND = function.IndexOf("*");

            while (positionAND != -1)
            {
                //GuiLogMessage("Position of '*': " + positionAND, NotificationLevel.Debug);

                // get both operands
                string operator1 = function.Substring(positionAND - 1, 1);
                string operator2 = function.Substring(positionAND + 1, 1);
                //GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Debug);

                string product = (int.Parse(operator1) & int.Parse(operator2)).ToString();
                //GuiLogMessage("product: " + product, NotificationLevel.Debug);
                // remove old values
                function = function.Remove(positionAND - 1, 3);
                // insert new value
                function = function.Insert(positionAND - 1, product);
                //GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other ANDs in there?
                positionAND = function.IndexOf("*");
            }

            // test for XOR aka '+'
            int positionXOR = function.IndexOf("+");

            while (positionXOR != -1)
            {
                //GuiLogMessage("Position of '+': " + positionXOR, NotificationLevel.Debug);

                // get both operands
                string operator1 = function.Substring(positionXOR - 1, 1);
                string operator2 = function.Substring(positionXOR + 1, 1);
                //GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Debug);

                string sum = (int.Parse(operator1) ^ int.Parse(operator2)).ToString();
                //GuiLogMessage("sum: " + sum, NotificationLevel.Debug);
                // remove old values
                function = function.Remove(positionXOR - 1, 3);
                // insert new value
                function = function.Insert(positionXOR - 1, sum);
                //GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other XORs in there?
                positionXOR = function.IndexOf("+");
            }

            bool result = Convert.ToBoolean(int.Parse(function));

            return result;
        }

        #endregion

        public void Execute()
        {
            //NLFSRPresentation.DeleteAll(100);
            processNLFSR();
        }

        private void preprocessNLFSR()
        {
            if (checkForInputTapSequence() == 1)
            {
                return;
            }

            if (checkForInputSeed() == 1)
            {
                return;
            }

            // read tapSequence
            if (settings.Polynomial == null || settings.Polynomial.Length == 0)
            {
                tapSequencebuffer = inputTapSequence;
            }
            else
            {
                tapSequencebuffer = settings.Polynomial;
            }

            //read seed
            if (settings.SaveCurrentState && settings.CurrentState != null && settings.CurrentState.Length != 0 && settings.CurrentState != "0")
            {
                seedbuffer = settings.CurrentState;
            }
            else if (settings.Seed == null || settings.Seed.Length == 0)
            {
                seedbuffer = inputSeed;
            }
            else
            {
                seedbuffer = settings.Seed;
            }

            // convert tapSequence into char array
            //tapSequenceCharArray = ReverseOrder(tapSequencebuffer.ToCharArray());
            tapSequenceCharArray = seedbuffer.ToCharArray();

            int tapSequenceBits = seedbuffer.Length;
            seedBits = seedbuffer.Length;

            //GuiLogMessage("inputTapSequence length [bits]: " + tapSequenceBits.ToString(), NotificationLevel.Debug);
            //GuiLogMessage("inputSeed length [bits]: " + seedBits.ToString(), NotificationLevel.Debug);

            //check if last tap is 1, otherwise stop
            /*if (tapSequenceCharArray[tapSequenceCharArray.Length - 1] == '0')
            {
                GuiLogMessage("ERROR - Last tap of tapSequence must be 1. Aborting now.", NotificationLevel.Error);
                return;
            }*/

            // convert seed into char array
            seedCharArray = seedbuffer.ToCharArray();


            // check if seed is binary
            for (int z = 0; z < seedCharArray.Length; z++)
            {
                if (seedCharArray[z] != '0' && seedCharArray[z] != '1')
                {
                    GuiLogMessage("ERROR 0 - Seed has to be binary. Aborting now. Character is: " + seedCharArray[z], NotificationLevel.Error);
                    return;
                }
                tapSequenceCharArray[z] = '0';
            }

            // create tapSequence for drawing NLFSR
            Regex rgx = new Regex(@"x\d+");

            foreach (Match match in rgx.Matches(tapSequencebuffer))
            {
                int z = int.Parse(match.Value.Substring(1));
                if (z < 0 || z > seedCharArray.Length)
                {
                    GuiLogMessage("Illegal variable " + match.Value, NotificationLevel.Error);
                    return;
                }
                tapSequenceCharArray[seedCharArray.Length - z - 1] = '1';
            }

            if (settings.UseClockingBit)
            {
                if (settings.ClockingBit < seedCharArray.Length)
                {
                    clocking = (seedCharArray.Length - settings.ClockingBit - 1);
                }
                else
                {
                    clocking = -1;
                    GuiLogMessage("WARNING: Clocking Bit is too high. Ignored.", NotificationLevel.Warning);
                }

            }
            else
            {
                clocking = -1;
            }

            // check if Rounds are given
            int defaultRounds = 10;

            // check if Rounds in settings are given and use them only if no bool clock is selected
            if (!settings.UseBoolClock)
            {
                if (settings.Rounds == 0)
                {
                    actualRounds = defaultRounds;
                }
                else
                {
                    actualRounds = settings.Rounds;
                }
            }
            else
            {
                actualRounds = 1;
            }

            // draw presentation
            // (re-)draw NLFSR Quickwatch
            if (!settings.NoQuickwatch)
            {
                NLFSRPresentation.DeleteAll();
                NLFSRPresentation.DrawNLFSR(seedCharArray, tapSequenceCharArray, clocking);
                NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, ' ', getNewBit(), tapSequencebuffer);
            }
        }

        private void processNLFSR()
        {
            settings.PluginIsRunning = true;
            // check if event was from the boolean clock input
            // if so, check if boolean clock should be used
            // if not, do not process NLFSR
            if (lastInputPropertyWasBoolClock)
            {
                if (!settings.UseBoolClock)
                {
                    return;
                }
                //GuiLogMessage("First if.", NotificationLevel.Info);
            }
            // if last event wasn't from the clock but clock shall be
            // the only event to start from, do not go on
            else
            {
                if (settings.UseBoolClock)
                {
                    return;
                }
                //GuiLogMessage("Second if.", NotificationLevel.Info);
            }
            // process NLFSR

            try
            {
                // make all this stuff only one time at the beginning of our chainrun
                if (newSeed)
                {
                    preprocessNLFSR();
                }

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

                // (re-)draw NLFSR Quickwatch
                if (!settings.NoQuickwatch)
                {
                    NLFSRPresentation.DeleteAll();
                    NLFSRPresentation.DrawNLFSR(seedCharArray, tapSequenceCharArray, clocking);
                }

                //GuiLogMessage("Action is: Now!", NotificationLevel.Debug);
                DateTime startTime = DateTime.Now;

                //////////////////////////////////////////////////////
                // compute NLFSR //////////////////////////////////////
                //////////////////////////////////////////////////////
                //GuiLogMessage("Starting computation", NotificationLevel.Debug);

                int i = 0;

                for (i = 0; i < actualRounds; i++)
                {
                    if (stop)
                    {
                        return;
                    }
                    // compute only if clock = 1 or true
                    if (myClock)
                    {
                        StatusChanged((int)NLFSRImage.Encode);

                        // make bool output
                        if (seedCharArray[seedBits - 1] == '0')
                        {
                            outputBool = false;
                        }
                        else
                        {
                            outputBool = true;
                        }
                        //GuiLogMessage("OutputBool is: " + outputBool.ToString(), NotificationLevel.Info);

                        // write last bit to output buffer, output stream buffer, stream and bool
                        outputbuffer = seedCharArray[seedBits - 1];
                        //outputStream.Write((Byte)outputbuffer);
                        outputStringBuffer += seedCharArray[seedBits - 1];
                        outputBoolArray[0] = outputBool;

                        // update outputs
                        OnPropertyChanged("OutputBool");
                        OnPropertyChanged("OutputBoolArray");
                        OnPropertyChanged("OutputStream");

                        ////////////////////
                        // compute new bit

                        // keep output bit for presentation
                        outputBit = seedCharArray[seedBits - 1];

                        // shift seed array
                        char newBit = getNewBit();

                        for (int j = seedBits - 1; j > 0; j--)
                        {
                            seedCharArray[j] = seedCharArray[j - 1];
                        }

                        seedCharArray[0] = newBit;


                        //update quickwatch presentation
                        if (!settings.NoQuickwatch)
                        {
                            NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputBit, getNewBit(), tapSequencebuffer);
                        }

                        // write current "seed" back to seedbuffer
                        seedbuffer = null;
                        foreach (char c in seedCharArray)
                        {
                            seedbuffer += c;
                        }

                        //GuiLogMessage("New Bit: " + newBit.ToString(), NotificationLevel.Info);
                    }
                    else
                    {
                        StatusChanged((int)NLFSRImage.Decode);

                        if (settings.AlwaysCreateOutput)
                        {
                            /////////
                            // but nevertheless fire an output event with old value
                            /////////

                            if (newSeed)
                            {
                                outputBool = false;
                                outputbuffer = '0';
                            }
                            else
                            {
                                if (outputBit == '0')
                                {
                                    outputBool = false;
                                }
                                else
                                {
                                    outputBool = true;
                                }

                                outputbuffer = outputBit;
                            }
                            //GuiLogMessage("OutputBool is: " + outputBool.ToString(), NotificationLevel.Info);

                            // write bit to output buffer, stream and bool
                            outputBoolArray[0] = outputBool;
                            OnPropertyChanged("OutputBool");
                            OnPropertyChanged("OutputBoolArray");

                            // update quickwatch presentation
                            if (!settings.NoQuickwatch)
                            {
                                NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputbuffer, getNewBit(), tapSequencebuffer);
                            }
                            /////////
                        }
                        else
                        {
                            // update quickwatch with current state but without any output bit
                            if (!settings.NoQuickwatch)
                            {
                                NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, ' ', getNewBit(), tapSequencebuffer);
                            }
                        }

                        //GuiLogMessage("NLFSR Clock is 0, no computation.", NotificationLevel.Info);
                        //return;
                    }
                    // in both cases update "clocking bit" if set in settings
                    if (settings.UseClockingBit)
                    {
                        // make clocking bit output only if its not out of bounds
                        if (clocking != -1)
                        {
                            if (seedCharArray[clocking] == '0')
                            {
                                outputClockingBit = false;
                            }
                            else
                            {
                                outputClockingBit = true;
                            }

                            OnPropertyChanged("OutputClockingBit");
                        }
                    }

                    // reset newSeed after first round
                    newSeed = false;
                    if (!settings.UseBoolClock)
                    {
                        ProgressChanged(i, actualRounds);
                    }
                }

                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;

                // change progress to 100%
                ProgressChanged(1.0, 1.0);

                if (!stop)
                {
                    // finally write output string
                    outputString = outputStringBuffer;
                    OnPropertyChanged("OutputString");

                    //GuiLogMessage("Complete!", NotificationLevel.Debug);
                    //GuiLogMessage("Time used: " + duration, NotificationLevel.Debug);
                }

                if (stop)
                {
                    outputStringBuffer = null;
                    GuiLogMessage("Aborted!", NotificationLevel.Debug);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
                settings.PluginIsRunning = false;
            }
        }

        private char getNewBit()
        {
            string tapPolynomial = ReplaceVariables(tapSequencebuffer, seedCharArray);

            if (!IsPolynomial(tapPolynomial))
            {
                GuiLogMessage("ERROR - " + tapSequencebuffer + " is NOT a valid polynomial. Aborting now.", NotificationLevel.Error);
                return ' ';
            }

            return EvaluateString(tapPolynomial) ? '1' : '0';
        }

        #region events and stuff

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
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
                {
                    settings.CurrentState = seedbuffer;
                }
                else
                {
                    settings.CurrentState = null;
                }

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
            StatusChanged((int)NLFSRImage.Default);
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

    internal enum NLFSRImage
    {
        Default,
        Encode,
        Decode
    }

    #endregion

}
