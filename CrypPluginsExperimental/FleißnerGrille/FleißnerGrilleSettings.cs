/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using FleißnerGrille.Properties;

namespace CrypTool.Plugins.FleißnerGrille
{
    // HOWTO: rename class (click name, press F2)
    public class FleißnerGrilleSettings : ISettings
    {
        #region Public Fleißner specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Fleißner plugin
        /// </summary>
        public delegate void FleißnerLogMessage(string msg, NotificationLevel loglevel);

        public enum FleißnerMode { Encrypt = 0, Decrypt = 1 };
        public enum FleißnerRotate { Left = 0, Right = 1 };
        public enum RandomLetter { Big = 0, Lower = 1, BigLower = 2 };

        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event FleißnerLogMessage LogMessage;

        #endregion

        #region Private Variables and public constructor

        private FleißnerMode selectedModeAction = FleißnerMode.Encrypt;
        private FleißnerRotate selectedRotateAction = FleißnerRotate.Left;
        private RandomLetter selectedRandomAction = RandomLetter.Big;
        private int textSize = 3;
        private int Presentation_Speed = 1;
        private String stencilString = "010101000010001000010010000001000100";
        private bool[,] stencil;

        public FleißnerGrilleSettings()
        {
            SetKeyByValue();
        }

        public bool[,] myStencil
        {
            get { return stencil; }
            set { stencil = value; }
        }

        public FleißnerMode myFleißnerMode
        {
            get { return selectedModeAction; }
            set { selectedModeAction = value; }
        }

        public RandomLetter myRandomLetter
        {
            get { return selectedRandomAction; }
            set { selectedRandomAction = value; }
        }

        public FleißnerRotate myFleißnerRotate
        {
            get { return selectedRotateAction; }
            set { selectedRotateAction = value; }
        }

        #endregion

        #region Private methods

        public void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
        }

        /// <summary>
        /// Set the new shiftValue and the new shiftString to offset % alphabet.Length
        /// </summary>
        public void SetKeyByValue(bool firePropertyChanges = true)
        {
            //stencilList.Add(true);
            //stencilList.Add(true);
            // Anounnce this to the settings pane
            if (firePropertyChanges)
            {
                OnPropertyChanged("ShiftValue");
                OnPropertyChanged("ShiftString");
            }
            // print some info in the log.
            //OnLogMessage("Accepted new shift value: " + offset, NotificationLevel.Debug);
        }

        /// <summary>
        /// fill 2-dimensional char Array with String rowwise
        /// </summary>
        public bool isCorrectStencil(string input)
        {
            int n = input.Length;
            string replaceWith = "";
            input = input.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
            n = input.Length; // n*n-Stencil
            // checks the correct shape of the stencil
            if (Math.Sqrt(input.Length) - (int)Math.Sqrt(input.Length) > 0) // sqrt from length is odd
            {
                return false;
            }
            else // sqrt from lenght is even
            {
                n = (int)Math.Sqrt(input.Length);
            }
            // check whether the correct characters have been entered
            foreach (char ch in input)
            {
                if (!(ch.Equals('0') || ch.Equals('1'))) // at least one sign isn't 0 or 1
                    return false;
            }
            bool[,] stencil = StringToStencil(input);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (stencil[i, j] == true) // in stencil is a hole
                    {
                        bool right;
                        if (this.myFleißnerRotate.Equals(1)) //rotate right
                        {
                            right = true;
                        }
                        else
                        {
                            right = false;
                        }
                        // checked after rotation of the stencil if everything is correct
                        for (int rotate = 0; rotate < 3; rotate++)
                        {
                            
                            stencil = RotateStencil(stencil, right);
                            // after rotation of the stencil at the same position is a hole
                            if (stencil[i, j] == true)
                            {
                                return false; //stencil isn't correct
                            }
                        }
                        stencil = RotateStencil(stencil, right);
                    }
                }
            }
            return true; // stencil is correct
        }

        /// <summary>
        /// fill 2-dimensional char Array with String rowwise
        /// </summary>
        public bool[,] StringToStencil(string input)
        {
            int n = (int)Math.Sqrt(input.Length);
            int count = 0;
            string replaceWith = "";
            input = input.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
            bool[,] matrix = new bool[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (count < input.Length)
                    {
                        if (input[count].Equals('1')) // in stencil is a hole
                        {
                            matrix[i, j] = true;
                        }
                        else // in stencil isn't a hole
                        {
                            matrix[i, j] = false;
                        }
                        count++;
                    }
                }
            }
            return matrix;
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public bool[,] RotateStencil(bool[,] stencil, bool right)
        {
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            bool[,] ret = null;
            if (ActionRotate == FleißnerGrilleSettings.FleißnerRotate.Right)
            {
                ret = rotate(stencil);
            }
            else 
            {
                for (int i = 0; i < 3; i++) 
                {
                    ret = rotate(stencil);   
                }
            }
            return ret;
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public bool[,] rotate(bool[,] stencil)
        {
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            bool[,] ret = new bool[stencilLength, stencilLength];
            for (int i = 0; i < stencilLength; ++i)
            {
                for (int j = 0; j < stencilLength; ++j)
                {
                    ret[i, j] = stencil[stencilLength - j - 1, i];
                }
            }
            return ret;
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(1)]
        [TaskPane("ActionModeCaption", "ActionModeTooltip", "ActionSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "ActionModeList1", "ActionModeList2" })]
        public FleißnerMode ActionMode
        {
            get
            {
                return this.selectedModeAction;
            }
            set
            {
                if (value != selectedModeAction)
                {
                    this.selectedModeAction = value;
                    OnPropertyChanged("ActionMode");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane("ActionRotateCaption", "ActionRotateTooltip", "ActionSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "ActionRotateList1", "ActionRotateList2" })]
        public FleißnerRotate ActionRotate
        {
            get
            {
                return this.selectedRotateAction;
            }
            set
            {
                if (value != selectedRotateAction)
                {
                    this.selectedRotateAction = value;
                    OnPropertyChanged("ActionRotate");
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("ActionRandomCaption", "ActionRandomTooltip", "ActionSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "ActionRandomList1", "ActionRandomList2", "ActionRandomList3" })]
        public RandomLetter ActionRandom
        {
            get
            {
                return this.selectedRandomAction;
            }
            set
            {
                if (value != selectedRandomAction)
                {
                    this.selectedRandomAction = value;
                    OnPropertyChanged("ActionRandom");
                }
            }
        }

        #endregion

        #region Stencil settings

        //[PropertySaveOrder(7)]
        //[TaskPane(null, "GrilleStringTooltip", "GrilleGroup", 4, false, ControlType.TextBox, "")]
        public string StencilString
        {
            get { return this.stencilString; }
            set
            {
                string replaceWith = "";
                string input = value.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
                bool b = isCorrectStencil(input);
                //string a = removeEqualChars(value);
                if (value.Length == 0) // cannot accept empty alphabets
                {
                    OnLogMessage(Resources.IGNORING_EMPTY+ "\n \"" + stencilString + "\"" + Resources.With_A + (int)Math.Sqrt(stencilString.Length) + Resources.SQUARE_GRILLE1, NotificationLevel.Info);
                }
                else if (isCorrectStencil(value))
                {
                    //if(value.Length<) //TODO: stencil is korrect but text is longer
                    //{

                    //}
                    this.stencilString = value;
                    //SetKeyByValue(shiftValue); //re-evaluate if the shiftvalue is still within the range
                    OnLogMessage(Resources.ACCEPTED_NEW + "\n\"" + stencilString + "\"" + Resources.With_A + (int)Math.Sqrt(stencilString.Length) + Resources.SQUARE_GRILLE1, NotificationLevel.Info);
                    OnPropertyChanged("StencilString");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 6, true, ControlType.Slider, 1, 100)]
        public int PresentationSpeed
        {
            get { return (int)Presentation_Speed; }
            set
            {
                if ((value) != Presentation_Speed)
                {
                    this.Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {

        }

        #endregion
    }
}
