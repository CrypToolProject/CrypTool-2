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
using System.Security.Cryptography;
using System.Windows.Controls;


namespace CrypTool.Plugins.BB84PhotonbaseGenerator
{

    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("CrypTool.Plugins.BB84PhotonbaseGenerator.Properties.Resources", "res_BaseGeneratorCaption", "res_BaseGeneratorTooltip", "BB84PhotonbaseGenerator/userdoc.xml", new[] { "BB84PhotonbaseGenerator/images/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]

    public class BB84PhotonbaseGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly BB84PhotonbaseGeneratorSettings settings = new BB84PhotonbaseGeneratorSettings();

        private object inputKey;
        private int keyLength;
        private int[] generatedRandom;
        private RNGCryptoServiceProvider sRandom;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_InputKeyCaption", "res_InputKeyTooltip", false)]
        public object InputKey
        {
            get => inputKey;
            set
            {
                if (value == null)
                {
                    return;
                }
                inputKey = value;
                if (inputKey is BigInteger)
                {
                    BigInteger temp = (BigInteger)inputKey;
                    keyLength = int.Parse(temp.ToString());
                }
                else if (inputKey is int)
                {
                    int temp = (int)inputKey;
                    keyLength = int.Parse(temp.ToString());
                }

                else if (inputKey is string)
                {
                    string temp = (string)inputKey;
                    temp = filterValidInput(temp);
                    keyLength = temp.Length;
                }
                else if (inputKey is int[])
                {
                    int[] temp = (int[])inputKey;
                    keyLength = temp.Length;
                }
                else if (inputKey is char[])
                {
                    char[] temp = (char[])inputKey;
                    keyLength = temp.Length;
                }
                else
                {
                    keyLength = (inputKey.ToString().Length);
                }


                generatedRandom = new int[keyLength];

            }
        }

        private string filterValidInput(string value)
        {
            string outputString = "";
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals('0') ||
                    value[i].Equals('1') ||
                    value[i].Equals('/') ||
                    value[i].Equals('\\') ||
                    value[i].Equals('-') ||
                    value[i].Equals('|') ||
                    value[i].Equals('x') ||
                    value[i].Equals('+'))
                {
                    outputString += value[i];
                }
            }

            return outputString;
        }


        [PropertyInfo(Direction.OutputData, "res_BaseOutputCaption", "res_BaseOutputTooltip", false)]
        public string OutputString
        {
            get
            {
                string output = "";

                for (int i = 0; i < generatedRandom.Length; i++)
                {
                    if (generatedRandom[i] == 0)
                    {
                        output += "+";
                    }
                    else
                    {
                        output += "x";
                    }

                }
                return output;
            }
            set { } //read-only


        }


        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
            generatedRandom = null;
        }

        public void Execute()
        {
            ProgressChanged(0, 100);
            generateRandomBases();
            notifyAllOutputs();
            ProgressChanged(100, 100);
        }

        private void generateRandomBases()
        {
            if (generatedRandom == null)
            {
                keyLength = settings.InputKey;
                generatedRandom = new int[keyLength];
            }

            for (int i = 0; i < generatedRandom.Length; i++)
            {
                sRandom = new RNGCryptoServiceProvider();
                byte[] buffer = new byte[4];
                sRandom.GetBytes(buffer);
                int result = BitConverter.ToInt32(buffer, 0);
                generatedRandom[i] = new Random(result).Next(2);
                ProgressChanged(i / (generatedRandom.Length - 1), (generatedRandom.Length - 1));
            }

        }

        private void notifyAllOutputs()
        {
            OnPropertyChanged("OutputString");
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            generatedRandom = new int[keyLength];
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
