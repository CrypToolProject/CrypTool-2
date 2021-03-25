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

namespace CrypTool.Plugins.FleißnerGrilleGenerator
{
    // HOWTO: rename class (click name, press F2)
    public class FleißnerGrilleGeneratorSettings : ISettings
    {
        #region Private Variables

        private String stencilString = "010101000010001000010010000001000100";
        private int Presentation_Speed = 100;
        private int[,] grille_Random;
        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Fleißner plugin
        /// </summary>
        public delegate void FleißnerGeneratorLogMessage(string msg, NotificationLevel loglevel);
        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event FleißnerGeneratorLogMessage LogMessage;
        public enum FleißnerGeneratorRandom { Random = 0, Not_Random = 1 };
        private FleißnerGeneratorRandom selectedRandomAction = FleißnerGeneratorRandom.Random;

        #endregion

        #region Private methods

        public void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
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
                        bool right = false;
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
            if (right)
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

        #region TaskPane Settings

        [PropertySaveOrder(1)]
        [TaskPane("ActionRandomCaption", "ActionRandomTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionRandomList1", "ActionRandomList2" })]
        public FleißnerGeneratorRandom ActionRandom
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

        public int[,] grille
        {
            get { return (int[,])grille_Random; }
            set
            {
                if ((value) != grille_Random)
                {
                    this.grille_Random = value;
                    OnPropertyChanged("Grille");
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
