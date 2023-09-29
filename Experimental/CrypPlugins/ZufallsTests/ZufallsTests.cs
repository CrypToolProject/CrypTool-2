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
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using ZufallsTests;
using System.Collections.Generic;
using CrypTool.PluginBase.IO;

namespace CrypTool.Plugins.ZufallsTests
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Philipp Eisen", "philipp.iron@gmail.com", "University of Mannheim", "http://www.uni-mannheim.de/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("PluginCaption", "PluginTooltip", "ZufallsTests/doc.xml", new[] { "ZufallsTests/Images/Random.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class ZufallsTests : ICrypComponent
    {
        // DLLImports for the DLL methods
        // InitializeTestTypes is used for setting up the generators and tests inside the DLL correctly
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern uint initializeTest_types();

        // setGloabals as the name says is just setting the global variables
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_globals();

        // setGenerator determines which generator should be used
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_generator(int g);

        // Choose_Rng is setting up the DLL for the chosen generator (must be executed after setGenerator)
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void choose_rng();

        // SetAll sets the global vlues inside the DLL for executing all tests at once
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_ALL(int b);

        // run_tests runs the tests in the DLL 
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern bool run_tests();

        // setDtestNum determines the TestNumber which should be executed
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void set_DtestNum(int num);

        // getter for the result of the test
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern int get_passed();

        // getter for the EndOfFile exception
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern int get_EOF();

        // setter for the FasterFactor
        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void setFasterFactor(int f);

        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void setNTuple(int n);

        [DllImport("ZufallsGeneratorenDLL.dll")]
        internal static extern void closeFile();

        #region Private Variables

        private readonly ZufallsTestSettings settings = new ZufallsTestSettings();
        private int selected;
        private Dictionary<int, int> dTestNumbers = new Dictionary<int, int>();
        private int reachedEOF;
        private ICrypToolStream dataAsStream;
        private FileStream fileStream;
        private Dictionary<int, int> minimumDataAmount = new Dictionary<int, int>();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "StreamInputCaption", "StreamInputTooltip")]
        public ICrypToolStream StreamInput
        {
            get
            {
                return dataAsStream;
            }
            set
            {
                if (value != null)
                {
                    dataAsStream = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "ResultCaption", "ResultTooltip")]
        public String Result
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SuccessCaption", "SuccessTooltip")]
        public bool Success
        {
            get;
            set;
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
            // settings.SelectedTest just returns the number of the selectedTest in the input.
            // But inside the DLL, the numbers for the Tests are ordered in another way.
            // Thats why I matched the DLL intern Test numbers with the numbers coming from the settings
            selected = (int)settings.SelectedTest;
            selected = dTestNumbers[selected];
        }

        public void Execute()
        {
            ProgressChanged(0, 1);


            deleteDataFile();


            minimumDataAmount.Clear();
            fillDataDic();

            // this block is reading the CStream and writing the data into a file.
            // probably this is the biggest lack of performance.
            // the C# Class is reading a stream and writing it onto another FileStream so the DLL can read it ...
            ProgressChanged(0.2, 1);
            using (CStreamReader reader = dataAsStream.CreateReader())
            {
                reader.WaitEof();
                if ((int)reader.Length < (minimumDataAmount[selected] * 4))
                {
                    GuiLogMessage("More data is needed to execute this test properly. \nThe minimum amount of numbers required for this Test is: " + (minimumDataAmount[selected] ), NotificationLevel.Info);
                    Result = "More data is needed to execute this test properly. \nThe minimum amount of numbers required for this Test is: " + (minimumDataAmount[selected] );
                    Success = false;
                    OnPropertyChanged("Result");
                    OnPropertyChanged("Success");
                    ProgressChanged(1, 1);

                    //fileStream = null;
                    return;
                }
                int bytesRead;
                byte[] buffer = new byte[4];

                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    // Note: buffer can contain less data than buffer.Length, therefore consider bytesRead
                    writeData(buffer, bytesRead);
                }
            }
            ProgressChanged(0.5, 1);
            //fileStream.Close();				


            // The execution of the DLL methods documented above
            initializeTest_types();
            set_globals();
            // but this time the generator must be 200 ( File input Generator to read the written file )
            set_generator(200);
            choose_rng();

            

            // singleTest case
            if (selected != 999)
            {
                // sets the testNumber correctly to execute the requested test
                set_DtestNum(selected);
                // sets the fasterFactor (explanation to this factor will come in the docu)
                // at least the fasterFactor is shorten the needed amount of numbers.
                //setFasterFactor(settings.FasterFactor+1);
                setNTuple(settings.NTuple);
                run_tests();
                //GuiLogMessage("Just one test will be executed", NotificationLevel.Info);
            }
                // alltest case
            else
            {
                set_ALL(1);
                //setFasterFactor(settings.FasterFactor + 1);
                setNTuple(settings.NTuple);
                run_tests();
                //GuiLogMessage("All the tests will be executed", NotificationLevel.Info);
            }

            int passed = get_passed();
            switch (passed)
            {
                case 1:
                    Result = "The data PASSED this test on randomness";
                    Success = true;
                    break;
                case 0:
                    Result = "The data is WEAK according to its randomness";
                    Success = true;
                    break;
                case -1:
                    Result = "The data FAILED on this test on randomness";
                    Success = false;
                    break;
                default:
                    Result = "Something went wrong";
                    Success = false;
                    break;
            }

            reachedEOF = get_EOF();
            if (reachedEOF == 1)
            {
                GuiLogMessage("More data is needed to execute this test properly. \nHave a look at the tool description to get information about the minimum data needed for each test", NotificationLevel.Info);
                Result = "More data is needed to execute this test properly. \nHave a look at the tool description to get information about the minimum data needed for each test";
                Success = false;
            }

            OnPropertyChanged("Result");
            OnPropertyChanged("Success");

            ProgressChanged(1, 1);

            //fileStream.Dispose();
            //fileStream.Close();
            //fileStream = null;
        }

        public void PostExecution()
        {
            deleteDataFile();
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            fillDtestDic();
            fillDataDic();
        }

        public void Dispose()
        {
        }

        private void writeDataOld(byte[] buffer, int bytesRead)
        {
            if(fileStream == null)
            {
                fileStream = new FileStream(@"data.txt", FileMode.Create);
            }
            fileStream.Write(buffer, 0, bytesRead);
        }


        private void writeData(byte[] buffer, int bytesRead)
        {
            using (FileStream fileStream = new FileStream("data.txt",
            FileMode.Append, FileAccess.Write, FileShare.Write))
            {
                fileStream.Write(buffer, 0, bytesRead);
            }

        }

        private void deleteDataFile()
        {
            try
            {
                File.Delete(@"data.txt");
            }
            catch(Exception e)
            {
                closeFile();
                File.Delete(@"data.txt");
            }
        }

        private void fillDtestDic()
        {
            for (int i = 0; i < 16; i++)
            {
                dTestNumbers.Add(i, i);
            }
            dTestNumbers.Add(16, 100);
            dTestNumbers.Add(17, 101);
            dTestNumbers.Add(18, 102);
            for (int i = 19; i < 30; i++)
            {
                dTestNumbers.Add(i, 181 + i);
            }
            dTestNumbers.Add(30, 999);
        }

        private void fillDataDic()
        {
            minimumDataAmount.Add(0,3840006);
            minimumDataAmount.Add(1, 100000500);
            minimumDataAmount.Add(2, 128000000);
            minimumDataAmount.Add(3, 60000000);
            minimumDataAmount.Add(4, 26214600);
            minimumDataAmount.Add(5, 65537100);
            minimumDataAmount.Add(6, 6400100);
            minimumDataAmount.Add(7, 128000000);
            minimumDataAmount.Add(8, 2400000);
            minimumDataAmount.Add(9, 1600000);
            minimumDataAmount.Add(10, 1200000);
            minimumDataAmount.Add(11, 230640860);
            minimumDataAmount.Add(12, 19900);
            minimumDataAmount.Add(13, 10000000);
            minimumDataAmount.Add(14, 135012382);
            minimumDataAmount.Add(15, 2000000000);

            minimumDataAmount.Add(100, 10000000);
            minimumDataAmount.Add(101, 10000000);
            minimumDataAmount.Add(102, 10000000);

            minimumDataAmount.Add(200, settings.NTuple * 20000000 + 100);
            minimumDataAmount.Add(201, settings.NTuple * 10000000);
            minimumDataAmount.Add(202, 50000000);
            minimumDataAmount.Add(203, 100000000);
            minimumDataAmount.Add(204, 10000000);
            minimumDataAmount.Add(205, 153600000);
            minimumDataAmount.Add(206, 12800000);
            minimumDataAmount.Add(207, 112989058);
            minimumDataAmount.Add(208, 29216429);
            minimumDataAmount.Add(209, 65000000);
            minimumDataAmount.Add(210, 3400000);
            //minimumDataAmount.Add(211, 12800000);
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
