/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.Plugins.AutokorrelationFunction
{
    [Author("Dennis Nolte", "nolte@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("AutokorrelationFunction.Properties.Resources", "PluginCaption", "PluginTooltip", "AutokorrelationFunction/DetailedDescription/doc.xml", "AutokorrelationFunction/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class AutokorrelationFunction : ICrypComponent
    {
        #region Private Variables

        private readonly AutocorrelationPresentation _presentation;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";       //used alphabet
        private string _cipher;                                             //The cipher to be analysed
        private int _probablelength;                                        //estimated keylength
        private double _probablekorr = -999999.999999;                      //initialized probable korrelation of the length        
        private double[] _ak;                                               //autokorrelation values
        private HistogramElement _bar;
        private readonly HistogramDataSource _data;

        #endregion

        #region Data Properties

        /// <summary>
        /// The input for the ciphertext 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCipherCaption", "InputCipherTooltip", true)]
        public string InputCipher
        {
            get => _cipher;
            set
            {
                _cipher = value;
                OnPropertyChanged("InputCipher");
            }
        }

        /// <summary>
        /// The output for the found shift value (most probable keylength) 
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputLengthCaption", "OutputLengthTooltip")]
        public int OutputLength
        {
            get => _probablelength;
            set
            {
                _probablelength = value;
                OnPropertyChanged("OutputLength");
            }
        }

        #endregion

        #region IPlugin Members

        public AutokorrelationFunction()
        {
            _presentation = new AutocorrelationPresentation();
            _data = new HistogramDataSource();
        }
        public ISettings Settings => null;

        public UserControl Presentation => _presentation;

        public void PreExecution()
        {
        }

        public void Execute()
        {

            //START------------------------------------------------------------------------------------------------------------
            //Preparations for the Analyse-------------------------------------------------------------------------------------

            if (InputCipher != null)                                //Starts only if a ciphertext is set
            {
                ProgressChanged(0, 1);

                _probablelength = 0;
                _probablekorr = -999999.999999;
                _cipher = InputCipher;                               //initialising the ciphertext
                _cipher = prepareForAnalyse(_cipher);                 //and prepare it for the analyse (-> see private methods section)

                _ak = new double[_cipher.Length];                     //initialise ak[]...there are n possible shifts where n is cipher.length

                _presentation.histogram.SetBackground(Brushes.Beige);              //sets the background colour for the quickwatch
                _presentation.histogram.SetHeadline(typeof(AutokorrelationFunction).GetPluginStringResource("Autocorrelation_matches"));    //sets its title

                //-----------------------------------------------------------------------------------------------------------------
                //Analyse----------------------------------------------------------------------------------------------------------
                //-----------------------------------------------------------------------------------------------------------------		

                //for each possible shift value...
                for (int t = 0; t < _cipher.Length; t++)
                {
                    int same = 0;

                    //...calculate how often the letters match...
                    for (int x = 0; x < _cipher.Length - t; x++)
                    {
                        if (_cipher[x] == _cipher[x + t])
                        {
                            same++;
                        }
                    }

                    try
                    {
                        //...and save the count for the matches at the shift position
                        _ak[t] = same;
                    }
                    catch
                    {
                    }
                }

                _data.ValueCollection.Clear();

                //for all observed shifts...
                for (int y = 1; y < _ak.Length; y++)
                {
                    //find the one with the highest match count...
                    if (_ak[y] > _probablekorr)
                    {
                        _probablekorr = _ak[y];
                        _probablelength = y;                 //...and remember this shift value
                    }
                }

                //find the top 13 matches...
                if (_ak.Length > 11)
                {
                    _ak = findTopThirteen(_ak);
                }

                for (int y = 1; y < _ak.Length; y++)
                {
                    if (_ak[y] > -1)                         //Adds a bar into the presentation if it is higher then the average matches
                    {
                        _bar = new HistogramElement(_ak[y], _ak[y], "" + y);
                        _data.ValueCollection.Add(_bar);
                    }
                }

                _presentation.histogram.SetHeadline(string.Format(typeof(AutokorrelationFunction).GetPluginStringResource("Highest_match_count_with_shift"), _probablekorr, _probablelength));

                if (_data != null)
                {
                    _presentation.histogram.ShowData(_data);
                }

                OutputLength = _probablelength;              //sending the keylength via output
                OnPropertyChanged("OutputLength");
            }


            ProgressChanged(1, 1);

            //EXECUTE END------------------------------------------------------------------------------------------------------

        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            _presentation.histogram.SetBackground(Brushes.LightGray);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        //PREPARE PART---------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove spaces and symbols not provided by the alphabet from the text
        /// </summary>
        private string prepareForAnalyse(string c)
        {
            string prepared = "";

            c = c.ToUpper();

            for (int x = 0; x < c.Length; x++)
            {
                if (getPos(c[x]) != -1)
                {
                    prepared = prepared + c[x];
                }
            }
            return prepared;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        //LETTER TO NUMBER----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert a the letter to an int-value that resembles his position in the given alphabet
        /// </summary>
        private int getPos(char c)
        {
            int pos = -1;
            for (int i = 0; i < Alphabet.Length; i++)
            {
                if (Alphabet[i] == c)
                {
                    pos = i;
                }
            }
            return pos;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------
        //FIND TOP 13----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Thirteen possible shift values with the highest match count are enough information
        /// </summary>
        private double[] findTopThirteen(double[] ak)
        {
            double[] top = ak;
            int thrownaway = 0;

            for (int match = 0; match < _probablekorr; match++)
            {
                for (int x = 0; x < ak.Length; x++)
                {
                    if (top[x] == match)
                    {
                        top[x] = -1;
                        thrownaway++;
                    }
                    if (thrownaway == (ak.Length) - 13)
                    {
                        return top;
                    }

                }
            }
            return top;
        }





        //---------------------------------------------------------------------------------------------------------------------------------------

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
