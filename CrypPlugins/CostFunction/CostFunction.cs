/*                              
   Copyright 2020 Team CrypTool (Nils Kopal, Sven Rech)

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
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CrypTool.Plugins.CostFunction
{
    [Author("Nils Kopal, Sven Rech", "Nils.Kopal@CrypTool.org", "CrypTool Team", "http://www.CrypTool.org")]
    [PluginInfo("CostFunction.Properties.Resources", "PluginCaption", "PluginTooltip", "CostFunction/DetailedDescription/doc.xml", "CostFunction/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CostFunction : ICrypComponent
    {
        #region private variables

        private CostFunctionSettings settings = new CostFunctionSettings();
        private byte[] inputText = null;
        private double value = 0.0;
        private bool stopped = true;
        private IControlCost controlSlave;
        private RegEx regularexpression = null;
             
        #endregion

        #region public interface

        public CostFunction()
        {

        }

        /// <summary>
        /// The Grams currently used with this cost function
        /// Public access is needed by e.g. the TranspositionAnalyzer to normalize the ngrams
        /// </summary>
        public Grams Grams
        {
            get;
            private set;
        }

        #endregion

        #region CostFunctionInOut

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip")]
        public byte[] InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        #region testing

        public void changeFunctionType(CostFunctionSettings.CostFunctionType type)
        {
            settings.changeFunctionType(type);
        }

        public void setRegEx(string regex)
        {
            settings.RegEx = regex;
        }

        public void changBytesToUse(string byts)
        {
            settings.BytesToUse = byts;
        }

        public void setBlocksizeToUse(int blocksize)
        {
            settings.BlockSize = blocksize;
        }

        public void setTextToUse(string bytesToUse)
        {
            settings.BytesToUse = bytesToUse;
        }

        #endregion

        [PropertyInfo(Direction.OutputData, "ValueCaption", "ValueTooltip")]
        public double Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlCost ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new CostFunctionControl(this);
                }
                return controlSlave;
            }
        }

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;


        public ISettings Settings
        {
            get => settings;
            set => settings = (CostFunctionSettings)value;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            //load selected ngrams for cost function usage
            if (settings.FunctionType == CostFunctionSettings.CostFunctionType.NGramsLog2)
            {
                //CreateNGrams returns null, if it is not possible to create an Grams object based on the given settings
                //this happens, e.g, when a n-gram size and language combination is selected, where we do not have a corrsponding language file
                Grams = LanguageStatistics.CreateGrams(settings.NGramSize, LanguageStatistics.LanguageCode(settings.Language), DirectoryHelper.DirectoryLanguageStatistics, settings.UseSpaces);
                if (Grams == null)
                {
                    GuiLogMessage(string.Format("CrypTool 2 has no {0}-grams for {1}. Falling back to English {0}-grams.", settings.NGramSize, LanguageStatistics.SupportedLanguages[settings.Language]), NotificationLevel.Error);
                    Grams = LanguageStatistics.CreateGrams(settings.NGramSize, "en", DirectoryHelper.DirectoryLanguageStatistics, settings.UseSpaces);
                }
            }
            stopped = false;
        }

        public void Execute()
        {
            if (InputText != null && stopped == false)
            {
                int bytesToUse;
                try
                {
                    bytesToUse = int.Parse(settings.BytesToUse);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Entered bytesToUse is not an integer: " + ex.Message, NotificationLevel.Error);
                    return;
                }

                int bytesOffset;
                try
                {
                    bytesOffset = int.Parse(settings.BytesOffset);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Entered bytesOffset is not an integer: " + ex.Message, NotificationLevel.Error);
                    return;
                }

                if (bytesToUse == 0 || bytesToUse > (InputText.Length - bytesOffset))
                {
                    bytesToUse = InputText.Length - bytesOffset;
                }

                //Create a new Array of size of bytesToUse
                byte[] array = new byte[bytesToUse];
                Array.Copy(InputText, bytesOffset, array, 0, bytesToUse);

                ProgressChanged(0.5, 1);

                Value = CalculateCost(array);

                ProgressChanged(1, 1);

            }//end if
        }
        public double CalculateCost(byte[] text)
        {
            switch (settings.FunctionType)
            {
                case CostFunctionSettings.CostFunctionType.IOC:
                    if (settings.BlockSize == 1)
                    {
                        return calculateFastIndexOfCoincidence(text, settings.BytesToUseInteger);
                    }
                    return calculateIndexOfCoincidence(text, settings.BytesToUseInteger, settings.BlockSize);

                case CostFunctionSettings.CostFunctionType.Entropy:
                    return calculateEntropy(text, settings.BytesToUseInteger);

                case CostFunctionSettings.CostFunctionType.NGramsLog2:
                    return Grams.CalculateCost(LanguageStatistics.MapTextIntoNumberSpace(Encoding.UTF8.GetString(text).ToUpper(),
                                               LanguageStatistics.Alphabets[LanguageStatistics.LanguageCode(settings.Language)]));

                case CostFunctionSettings.CostFunctionType.RegEx:
                    return regex(text);

                default:
                    throw new NotImplementedException("The cost function type " + settings.FunctionType + " is not implemented.");
            }
        }

        public void PostExecution()
        {
            stopped = true;
        }

        public void Stop()
        {
            stopped = false;
        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region private methods       

        private string lastRegex = null;
        private bool lastCaseInsensitive;

        public double regex(byte[] input)
        {
            if (settings.RegEx == null)
            {
                GuiLogMessage("There is no Regular Expression to be searched for. Please insert regex in the 'Regular Expression' - Textarea", NotificationLevel.Error);
                return -1.0;
            }

            if (lastRegex != settings.RegEx || lastCaseInsensitive != settings.CaseInsensitive)
            {
                regularexpression = new RegEx(settings.RegEx, settings.CaseInsensitive);
                lastRegex = settings.RegEx;
                lastCaseInsensitive = settings.CaseInsensitive;
            }

            try
            {
                return regularexpression.MatchesValue(input);
            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message, NotificationLevel.Error);
                return double.NegativeInfinity;
            }

        }

        /// <summary>
        /// Calculates the Index of Coincidence multiplied with 100 of
        /// a given byte array
        /// 
        /// for example a German text has about 7.62
        ///           an English text has about 6.61
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateIndexOfCoincidence(byte[] text, int blocksize = 1)
        {
            return calculateIndexOfCoincidence(text, text.Length, blocksize);
        }

        /// <summary>
        /// Calculates the Index of Coincidence of
        /// a given byte array using different "block sizes"
        /// block size means that a block of symbols is used as if it would be a single symbol
        /// This can be used for example for breaking the ADFGVX where bigrams count as a single symbol
        /// 
        /// for example a German text has about 0.0762
        ///           an English text has about 0.0661
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateIndexOfCoincidence(byte[] text, int bytesToUse, int blocksize = 1)
        {
            if (bytesToUse > text.Length)
            {
                bytesToUse = text.Length;
            }

            Dictionary<string, int> n = new Dictionary<string, int>();
            for (int i = 0; i < bytesToUse / blocksize; i++)
            {
                byte[] b = new byte[blocksize];
                Array.Copy(text, i * blocksize, b, 0, blocksize);
                string key = Encoding.ASCII.GetString(b);

                if (!n.Keys.Contains(key))
                {
                    n.Add(key, 0);
                }
                n[key] = n[key] + 1;
            }

            double coindex = 0;
            //sum them
            for (int i = 0; i < n.Count; i++)
            {
                coindex = coindex + n.Values.ElementAt(i) * (n.Values.ElementAt(i) - 1);
            }

            coindex = coindex / (bytesToUse / (double)blocksize);
            coindex = coindex / ((bytesToUse / (double)blocksize) - 1);

            return coindex;

        }

        /// <summary>
        /// Calculates the Index of Coincidence of
        /// a given byte array only for single letters
        /// (Fast implementation)
        /// 
        /// for example a German text has about 0.0762
        ///           an English text has about 0.0661
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateFastIndexOfCoincidence(byte[] text, int bytesToUse)
        {
            if (bytesToUse > text.Length)
            {
                bytesToUse = text.Length;
            }

            double[] n = new double[256];
            //count all ASCII symbols 
            int counter = 0;
            foreach (byte b in text)
            {
                n[b]++;
                counter++;
                if (counter == bytesToUse)
                {
                    break;
                }
            }

            double coindex = 0;
            //sum them
            for (int i = 0; i < n.Length; i++)
            {
                coindex = coindex + n[i] * (n[i] - 1);
            }

            coindex = coindex / (bytesToUse);
            coindex = coindex / (bytesToUse - 1);

            return coindex;

        }

        private int lastUsedSize = -1;
        private float[] xlogx;
        private readonly Mutex prepareMutex = new Mutex();

        private void prepareEntropy(int size)
        {
            xlogx = new float[size + 1];
            //precomputations for fast entropy calculation	
            xlogx[0] = 0.0f;
            for (int i = 1; i <= size; i++)
            {
                xlogx[i] = (float)(-1.0f * i * Math.Log(i / (double)size) / Math.Log(2.0));
            }
        }

        /// <summary>
        /// Calculates the Entropy of a given byte array 
        /// for example a German text has about 4.0629
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Entropy</returns>
        public double calculateEntropy(byte[] text)
        {
            return calculateEntropy(text, text.Length);
        }

        /// <summary>
        /// Calculates the Entropy of a given byte array 
        /// for example a German text has about 4.0629
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Entropy</returns>
        public double calculateEntropy(byte[] text, int bytesToUse)
        {
            switch (settings.EntropySelection)
            {
                case 0:
                    return NativeCryptography.Crypto.calculateEntropy(text, bytesToUse);
                case 1:
                    if (bytesToUse > text.Length)
                    {
                        bytesToUse = text.Length;
                    }

                    if (lastUsedSize != bytesToUse)
                    {
                        try
                        {
                            prepareMutex.WaitOne();
                            if (lastUsedSize != bytesToUse)
                            {
                                prepareEntropy(bytesToUse);
                                lastUsedSize = bytesToUse;
                            }
                        }
                        finally
                        {
                            prepareMutex.ReleaseMutex();
                        }
                    }

                    int[] n = new int[256];
                    //count all ASCII symbols
                    for (int counter = 0; counter < bytesToUse; counter++)
                    {
                        n[text[counter]]++;
                    }

                    float entropy = 0;
                    //calculate probabilities and sum entropy
                    for (int i = 0; i < 256; i++)
                    {
                        entropy += xlogx[n[i]];
                    }

                    return entropy / (double)bytesToUse;
                default:
                    return NativeCryptography.Crypto.calculateEntropy(text, bytesToUse);
            }
        }//end calculateEntropy

        #endregion
    }

    #region slave

    public class CostFunctionControl : IControlCost
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;

        private readonly CostFunction plugin;
        private readonly CostFunctionSettings settings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
        public CostFunctionControl(CostFunction plugin)
        {
            this.plugin = plugin;
            settings = (CostFunctionSettings)plugin.Settings;
        }

        /// <summary>
        /// Return bytes to use setting.
        /// Throws exception if setting is invalid.
        /// </summary>
        /// <returns></returns>
        public int GetBytesToUse()
        {
            try
            {
                return int.Parse(settings.BytesToUse);
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesToUse is not an integer: " + ex.Message);
            }
        }

        // Just return the number for internal use. Don't care about input errors, use whatever we have.
        private int bytesToUse()
        {
            return settings.BytesToUseInteger;
        }

        /// <summary>
        /// Return bytes offset setting.
        /// Throws exception if setting is invalid.
        /// </summary>
        /// <returns></returns>
        public int GetBytesOffset()
        {
            try
            {
                return int.Parse(settings.BytesOffset);
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesOffset is not an integer: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns the relation operator of the cost function which is set by by CostFunctionSettings
        /// </summary>
        /// <returns>RelationOperator</returns>
        public RelationOperator GetRelationOperator()
        {
            switch (settings.FunctionType)
            {
                case CostFunctionSettings.CostFunctionType.IOC:
                    return RelationOperator.LargerThen;
                case CostFunctionSettings.CostFunctionType.Entropy:
                    return RelationOperator.LessThen;
                case CostFunctionSettings.CostFunctionType.NGramsLog2:
                    return RelationOperator.LargerThen;
                case CostFunctionSettings.CostFunctionType.RegEx:
                    return RelationOperator.LargerThen;
                default:
                    throw new NotImplementedException("The cost function type " + settings.FunctionType + " is not implemented.");
            }//end switch
        }//end GetRelationOperator

        /// <summary>
        /// Calculates the cost function of the given text
        /// 
        /// Cost function can be set by CostFunctionSettings
        /// This algorithm uses a bytesToUse which can be set by CostFunctionSettings
        /// If bytesToUse is set to 0 it uses the whole text
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>cost</returns>
        public double CalculateCost(byte[] text)
        {
            /*
             * Note: If being used together with KeySearcher, the text given here is already shortened and thus
             * bytesToUse and bytesOffset will have no further effect (neither positive nor negative).
             */
            return plugin.CalculateCost(text);
        }

        public void NormalizeNGrams()
        {
            if (plugin.Grams != null && !plugin.Grams.IsNormalized)
            {
                plugin.Grams.Normalize(10_000_000);
            }
        }

        #endregion
    }
}