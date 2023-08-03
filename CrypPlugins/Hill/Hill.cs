/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.Hill
{
    [Author("Armin Krauß", "coredevs@CrypTool.org", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.Hill.Properties.Resources", "PluginCaption", "PluginTooltip", "Hill/DetailedDescription/doc.xml", new[] { "Hill/Hill.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Hill : ICrypComponent
    {
        #region Private Variables

        private readonly HillSettings settings = new HillSettings();
        private ModMatrix mat, inv;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public string Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "MatrixCaption", "MatrixTooltip")]
        public BigInteger[] Matrix
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        private bool CheckParameters()
        {
            int dim;
            string[] s = null;
            BigInteger[] matelements;

            if (Matrix == null)
            {
                // get matrix from settings
                s = settings.Matrix.Split(',');
                dim = (int)Math.Sqrt(s.Length);
                matelements = new BigInteger[dim * dim];
            }
            else
            {
                // get matrix from matrix input
                matelements = Matrix;
                dim = (int)Math.Sqrt(Matrix.Length);
            }

            if (dim < 1)
            {
                GuiLogMessage(Properties.Resources.MatrixDimensionMustBeGreaterThan0, NotificationLevel.Error);
                return false;
            }

            if (dim * dim != matelements.Length)
            {
                GuiLogMessage(Properties.Resources.NumberOfElementsInMatrixMustBeSquareNumber, NotificationLevel.Error);
                return false;
            }

            if (settings.Modulus < 2)
            {
                GuiLogMessage(Properties.Resources.InputAlphabetMustContainAtLeast2DifferentCharacters, NotificationLevel.Error);
                return false;
            }

            if (Input.Length % dim != 0)
            {
                GuiLogMessage(string.Format(Properties.Resources.InputWasPadded, dim), NotificationLevel.Warning);
                char paddingChar = settings.Alphabet.Contains("X") ? 'X' : (settings.Alphabet.Contains("x") ? 'x' : settings.Alphabet[settings.Modulus - 1]);
                Input += new string(paddingChar, dim - (Input.Length % dim));
            }

            for (int j = 0; j < Input.Length; j++)
            {
                if (settings.Alphabet.IndexOf(Input[j]) < 0)
                {
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, Input[j], j), NotificationLevel.Error);
                    return false;
                }
            }

            if (Matrix == null)
            {
                // read the matrix from the settings string

                int i = 0;

                try
                {
                    for (i = 0; i < matelements.Length; i++)
                    {
                        matelements[i] = BigInteger.Parse(s[i]);
                    }
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Properties.Resources.ErrorWhileParsingMatrixElement, i, s[i]), NotificationLevel.Error);
                    return false;
                }
            }

            mat = new ModMatrix(dim, settings.Modulus);

            int k = -1;
            for (int y = 0; y < mat.Dimension; y++)
            {
                for (int x = 0; x < mat.Dimension; x++)
                {
                    mat[x, y] = matelements[++k];
                }
            }

            try
            {
                inv = mat.invert();
            }
            catch (ArithmeticException)
            {
                throw new ArithmeticException(string.Format(Properties.Resources.MatrixNotInvertible, mat));
            }

            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            try
            {
                if (!CheckParameters())
                {
                    return;
                }
            }
            catch (ArithmeticException ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Warning);
                if (settings.Action)    // decrypt
                {
                    //we cannot decrypt, when we have an arithmetic exception, e.g. cannot compute an inverse matrix
                    return;
                }
            }

            if (settings.Action)
            {
                mat = inv;    // decrypt
            }

            Output = "";

            BigInteger[] result, vector = new BigInteger[mat.Dimension];

            for (int j = 0; j < Input.Length; j += mat.Dimension)
            {
                for (int k = 0; k < mat.Dimension; k++)
                {
                    vector[k] = settings.Alphabet.IndexOf(Input[j + k]);
                }
                result = mat * vector;
                for (int k = 0; k < mat.Dimension; k++)
                {
                    Output += settings.Alphabet[(int)result[k]];
                }
            }

            OnPropertyChanged(nameof(Output));

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
            Input = null;
            Output = null;
            Matrix = null;
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

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