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
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    [Author("George Lasry, Armin Krauß", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.PlayfairAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "PlayfairAnalyzer/DetailedDescription/doc.xml", new[] { "PlayfairAnalyzer/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class PlayfairAnalyzer : ICrypComponent
    {
        #region Private Variables
        
        private readonly PlayfairAnalyzerSettings settings = new PlayfairAnalyzerSettings();
        public bool stopped = false;

        private string plaintext;
        private string key;
        public Transform transform;
        public int[][] permutations;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip")]
        public string Ciphertext
        {
            get;
            set;
        }

        //[PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip")]
        public string Crib
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip")]
        public string Plaintext
        {
            get { return plaintext; }
            set
            {
                plaintext = value;
                OnPropertyChanged("Plaintext");
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            stopped = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            bool removeXZ = String.IsNullOrEmpty(Crib);
            removeXZ = false;
            int task = 1;

            byte[] ciphertext = Utils.getText(Ciphertext);

            GuiLogMessage(transform.printTransformationsCounts(), PluginBase.NotificationLevel.Debug);

            int cipherTextLength = (settings.MaxLength > 0) ? Math.Min(settings.MaxLength, ciphertext.Length) : ciphertext.Length;
            SimulatedAnnealing.solveSA(this, task, settings.MaxCycles, removeXZ, ciphertext, cipherTextLength, Crib);
            
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            stopped = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        public int DIM = 5;  // DIM = 6 works only with crib.
        public int ALPHABET_SIZE;
        public int SQUARE;

        private byte[,] POSITIONS_OF_PLAINTEXT_SYMBOL_1;
        private byte[,] POSITIONS_OF_PLAINTEXT_SYMBOL_2;

        public PlayfairAnalyzer()
        {
            ALPHABET_SIZE = DIM == 5 ? 26 : 36;
            SQUARE = DIM * DIM;

            POSITIONS_OF_PLAINTEXT_SYMBOL_1 = positionsOfPlaintextSymbol1();
            POSITIONS_OF_PLAINTEXT_SYMBOL_2 = positionsOfPlaintextSymbol2();

            permutations = DIM == 5 ? Permutations.PERMUTATIONS5 : Permutations.PERMUTATIONS6;
            transform = new Transform(this);
        }

        private byte row(byte pos)
        {
            return (byte)(pos / DIM);
        }

        private byte col(byte pos)
        {
            return (byte)(pos % DIM);
        }

        private byte pos(int r, int c)
        {
            if (r >= DIM) r -= DIM;
            else if (r < 0) r += DIM;

            if (c >= DIM) c -= DIM;
            else if (c < 0) c += DIM;

            return (byte)(DIM * r + c);
        }

        private byte positionOfPlaintextSymbol1(byte cipherPositionOfSymbol1, byte cipherPositionOfSymbol2)
        {
            byte c1 = col(cipherPositionOfSymbol1);
            byte r1 = row(cipherPositionOfSymbol1);
            byte c2 = col(cipherPositionOfSymbol2);
            byte r2 = row(cipherPositionOfSymbol2);

            if (r1 == r2) return pos(r1, c1 - 1);
            if (c1 == c2) return pos(r1 - 1, c1);
            return pos(r1, c2);
        }

        private byte[,] positionsOfPlaintextSymbol1()
        {
            byte[,] p1 = new byte[SQUARE, SQUARE];

            for (byte i = 0; i < SQUARE; i++)
                for (byte j = 0; j < SQUARE; j++)
                    p1[i,j] = positionOfPlaintextSymbol1(i, j);

            return p1;
        }

        private byte positionOfPlaintextSymbol2(byte cipherPositionOfSymbol1, byte cipherPositionOfSymbol2)
        {
            byte c1 = col(cipherPositionOfSymbol1);
            byte r1 = row(cipherPositionOfSymbol1);
            byte c2 = col(cipherPositionOfSymbol2);
            byte r2 = row(cipherPositionOfSymbol2);

            if (r1 == r2) return pos(r2, c2 - 1);
            if (c1 == c2) return pos(r2 - 1, c1);
            return pos(r2, c1);
        }

        private byte[,] positionsOfPlaintextSymbol2()
        {
            byte[,] p2 = new byte[SQUARE,SQUARE];

            for (byte i = 0; i < SQUARE; i++)
                for (byte j = 0; j < SQUARE; j++)
                    p2[i,j] = positionOfPlaintextSymbol2(i, j);

            return p2;
        }

        public int decrypt(byte[] cipherText, int length, byte[] plainText, bool removeXZ, Key key)
        {
            byte[] inverseKey = new byte[ALPHABET_SIZE];
            for (byte position = 0; position < SQUARE; position++)
                inverseKey[key.key[position]] = position;

            int plainTextLength = 0;

            byte lastPlaintextSymbol1 = 100;
            byte lastPlaintextSymbol2 = 100;
            byte plaintextSymbol1, plaintextSymbol2;
            int cipherPositionOfSymbol1, cipherPositionOfSymbol2;

            for (int n = 0; n < length; n += 2)
            {
                cipherPositionOfSymbol1 = inverseKey[cipherText[n]];
                cipherPositionOfSymbol2 = inverseKey[cipherText[n + 1]];

                plaintextSymbol1 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_1[cipherPositionOfSymbol1, cipherPositionOfSymbol2]];
                plaintextSymbol2 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_2[cipherPositionOfSymbol1, cipherPositionOfSymbol2]];

                if (removeXZ && (lastPlaintextSymbol1 == plaintextSymbol1 && (lastPlaintextSymbol2 == Utils.X || lastPlaintextSymbol2 == Utils.Z) && plainTextLength > 0))
                    plainText[plainTextLength - 1] = plaintextSymbol1;
                else
                    plainText[plainTextLength++] = plaintextSymbol1;

                plainText[plainTextLength++] = plaintextSymbol2;

                lastPlaintextSymbol1 = plaintextSymbol1;
                lastPlaintextSymbol2 = plaintextSymbol2;
            }

            if (removeXZ && plainTextLength > 0)
            {
                while (plainText[plainTextLength - 1] == Utils.X || plainText[plainTextLength - 1] == Utils.Z)
                    plainTextLength--;
            }

            return plainTextLength;
        }
        
        private string preparePlainText(string p)
        {
            StringBuilder sb = new StringBuilder();
            
            if (DIM == 6)
                p = Regex.Replace(p.ToUpper(), "[^A-Z0-9]", string.Empty);
            else
                p = Regex.Replace(p.ToUpper(), "[^A-Z]", string.Empty).Replace('J', 'I');

            for (int i = 0; i < p.Length; i += 2)
            {
                sb.Append(p[i]);
                if (i + 1 < p.Length)
                {
                    if (p[i] == p[i + 1]) sb.Append("X");
                    sb.Append(p[i + 1]);
                }
            }

            return sb.ToString();
        }
    }
}