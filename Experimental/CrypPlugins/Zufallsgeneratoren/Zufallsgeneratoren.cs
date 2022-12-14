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
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using CrypTool.PluginBase.IO;
using System.Numerics;

namespace CrypTool.Plugins.Zufallsgeneratoren
{
    [Author("Philipp Eisen", "philipp.iron@gmail.com", "University of Mannheim", "http://www.uni-mannheim.de/")]
    [PluginInfo("Zufallsgeneratoren.Properties.Resources", "PluginCaption", "PluginTooltip", "Zufallsgeneratoren/userdoc.xml", new[] { "Zufallsgeneratoren/Images/Random.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]

    public class Zufallsgeneratoren : ICrypComponent
    {
        // The required DLLImports for the used methods
        // InitializeTestTypes is used for setting up the generators and tests inside the DLL correctly
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern uint initializeTest_types();

        // setSeed sets the internal used seed for the chosen PRNG
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void setSeed(int seed);

        // setGloabals as the name says is just setting the global variables
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_globals();

        // setGenerator determines which generator should be used
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_generator(int g);

        // Choose_Rng is setting up the DLL for the chosen generator (must be executed after setGenerator)
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void choose_rng();

        // This is the method which give us the next UInt generated from the generator inside the DLL
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern uint getNextUInt();

        #region Private Variables

        //just a few private variables
        private readonly ZufallsgeneratorenSettings settings = new ZufallsgeneratorenSettings();
        private Dictionary<int, int> RNGDic = new Dictionary<int, int>();
        private int selectedRNG;
        private int amountOfNumbers;
        private int seed;
        private bool hasSeed = false;
        private ICrypToolStream data;
        private bool littleEndian;
        private BigInteger[] outputNumbers;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "AmountOfNumbersCaption", "AmountOfNumbersTooltip")]
        public int AmountOfNumbers
        {
            get
            {
                return amountOfNumbers;
            }
            set
            {
                amountOfNumbers = value;
            }
        }

        [PropertyInfo(Direction.InputData, "SeedCaption", "SeedTooltip")]
        public int Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;
                hasSeed = true;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false)]
        public ICrypToolStream OutputData
        {
            get
            {
                return data;
            }
            private set
            {
                data = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputNumbersCaption", "OutputNumbersTooltip", false)]
        public BigInteger[] OutputNumbers
        {
            get
            {
                return outputNumbers;
            }
            private set
            {
                outputNumbers = value;
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {

            selectedRNG = settings.SelectedRNg;
            // settings.SelectedRNg just returns the number of the selectedRandomNumberGenerator.
            // But inside the DLL, the numbers for the RNG`s are ordered in another way.
            // Thats why I matched the DLL intern RNg numbers with the numbers coming from the settings
            selectedRNG = RNGDic[selectedRNG];

            // a bool to check whether its a big or little endian machine.
            // I am not quite sure whether this is useful or not
            littleEndian = BitConverter.IsLittleEndian;

            // execution of the methods commented above
            initializeTest_types();
            ProgressChanged(0, 1);
            set_globals();

            if (hasSeed)
            {
                setSeed(seed);
            }

            try{
                set_generator(selectedRNG);
            }
            catch(Exception e)
            {
                // I can not find the reason for this Exception at the moment 
                //but the method will be executed well.
            }
            choose_rng();
        }

        public void Execute()
        {
            if (amountOfNumbers <= 0)
            {
                GuiLogMessage("Negative amount of numbers cant be generated", NotificationLevel.Warning);
                ProgressChanged(1, 1);
                return;
            }

            if (hasSeed)
            {
                setSeed(seed);
            }

            ProgressChanged(0, 1);

            CStreamWriter writer = new CStreamWriter();
            data = writer;
            uint current;
            byte[] nextData;
            outputNumbers = new BigInteger[amountOfNumbers];

            for (int i = 0; i < amountOfNumbers; i++)
            {
                // nothing to say here, it is simple writing the necessary amount of uints onto the stream
                current = getNextUInt();
                outputNumbers[i] = current;
                nextData = BitConverter.GetBytes(current);
                if(littleEndian)
                {
                    Array.Reverse(nextData);
                }
                writer.Write(nextData);
                ProgressChanged(i, amountOfNumbers);
            }
            writer.Close();

            OnPropertyChanged("OutputData");
            OnPropertyChanged("OutputNumbers");
            GuiLogMessage("Generation finished", NotificationLevel.Debug);

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            fillRNGDic();
        }

        private void fillRNGDic()
        {
            for (int i = 0; i < 62; i++)
            {
                RNGDic.Add(i, i);
            }
            for (int i = 62; i < 65; i++)
            {
                RNGDic.Add(i, i + 141);
            }
            for (int i = 65; i < 67; i++)
            {
                RNGDic.Add(i, i + 142);
            }
            for (int i = 67; i < 73; i++)
            {
                RNGDic.Add(i, i + 333);
            }
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
