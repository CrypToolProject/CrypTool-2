using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.KasiskiTest
{
    [Author("Georgi Angelov & Danail Vazov", "vazov@CrypTool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("KasiskiTest.Properties.Resources", "PluginCaption", "PluginTooltip", "KasiskiTest/DetailedDescription/doc.xml", "KasiskiTest/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class KasiskiTest : ICrypComponent
    {
        #region Private Variables
        private int[] integerArray;
        private string stringOutput;
        private string stringInput;
        #endregion
        public static DataSource Data = new DataSource();
        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.InputData, "StringInputCaption", "StringInputTooltip", true)]
        public string StringInput
        {
            get => stringInput;
            set { stringInput = value; OnPropertyChanged("StringInput"); }
        }

        [PropertyInfo(Direction.OutputData, "IntegerArrayCaption", "IntegerArrayTooltip", true)]
        public int[] IntegerArray
        {
            get => integerArray;
            set
            {
                if (value != integerArray)
                {
                    integerArray = value;
                    OnPropertyChanged("IntegerArray");
                }
            }
        }


        [PropertyInfo(Direction.OutputData, "StringOutputCaption", "StringOutputTooltip", true)]
        public string StringOutput
        {
            get => stringOutput;
            set
            {
                stringOutput = value; OnPropertyChanged("StringOutput");
            }

        }




        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private KasiskiTestSettings settings;
        public ISettings Settings
        {
            get => settings;
            set => settings = (KasiskiTestSettings)value;
        }



        public KasiskiTestPresentation presentation;
        public KasiskiTest()
        {
            settings = new KasiskiTestSettings();
            presentation = new KasiskiTestPresentation(this);
            Presentation = presentation;
        }
        public UserControl Presentation { get; private set; }


        public void PreExecution()
        {
            //return null;
        }

        public void Execute()
        {
            Progress(0.0, 1.0);

            if (stringInput != null)
            {

                // BEGIN PREWORK TO INPUT STRING DEPENDING ON USER CHOICE IN SETTINGS PANEL

                string workString = stringInput; //Copy the input string to a work string in order to make changes to it if neccessary
                string workstring2 = "";

                //UNKNOWN SYMBOL HANDLING


                if (settings.unknownSymbolHandling == 1) //Determine (via setting in Settings Panel) if unknown symbols (e.g. , : \ /) should be discarded from the text which is to be analysed
                {
                    char[] validchars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; //Sets alphabet of valid characters
                    string strValidChars = new string(validchars);
                    StringBuilder workstring1 = new StringBuilder();
                    foreach (char c in workString.ToCharArray())
                    {
                        if (strValidChars.IndexOf(c) >= 0) //If a char from the workString is valid, it is  appended to a newly built workstring1
                        {
                            workstring1.Append(c);
                        }
                    }

                    workstring2 = workstring1.ToString(); // Now copy workstring1 to workstring2. Needed because workstring1 cannot be altered once its built
                }
                else
                {
                    workstring2 = workString; // In case unknown symbols are to be ignored copy workString to workstring2, and proceed with analysis of workstring2
                }

                //CASE SENSITIVITY

                if (settings.caseSensitivity == 0)
                {
                    workstring2 = workstring2.ToUpper();
                }
                ArrayList checkedGramms = new ArrayList();
                ArrayList distances = new ArrayList();
                string grammToSearch;




                //TODO "add functionality that allows the user to choose grammLength - Done" , "add another for loop which starts from 3 until gramLength is reached" - array size of checkedGramms[],factors[] and distances[] needs tweaking, otherwise done


                //BEGIN TO SEARCH FOR EQUAL SUBSTRINGS AND TO COUNT DISTANCES BETWEEN THEM


                for (int d = 3; d <= settings.grammLength; d++)
                {
                    for (int i = 0; i <= workstring2.Length - settings.grammLength; i++)   //go through string 
                    {
                        grammToSearch = workstring2.Substring(i, d);   //  get every gramm(substring) with gramLength from Settings

                        for (int n = i + settings.grammLength; n <= workstring2.Length - settings.grammLength; n++)  //go through workString starting after the end of the taken grammToSearch
                        {
                            if (grammToSearch == workstring2.Substring(n, d)) //if grammToSearch in workString
                            {
                                distances.Add(n - i);               //save the distance in index 'g' of distances exactly where gramToSearch in checkedGramms is placed  
                                checkedGramms.Add(grammToSearch);                     //put in unused space of checkedGramms

                                break;
                            }


                        }

                    }
                }

                //BEGIN TO FACTORIZE THE DISTANCES




                //TODO: SPEED !@#$%^& !!!

                int x = 0;


                int[,] factors = new int[distances.Count, settings.factorSize /*sqrtOfLargestDist */];    //Rectangular array factors     

                int[] factorCounter = new int[(settings.factorSize + 1) /*sqrtOfLargestDist */];                //Vector array each factor is the index of the array, the element at this index illustrates how many times the factor is met 

                for (int z = 0; z <= distances.Count - 1; z++)       //for each distance
                {
                    int numberToFactorize = Convert.ToInt32(distances[z]);             //assign variable
                    x = 0;

                    for (int y = 2; y <= settings.factorSize/*Math.Sqrt(numberToFactorize)*/; y++) // for each number starting from 2 until sqrt(number to factorize)
                    {
                        if (numberToFactorize == 0) { break; }
                        if (numberToFactorize % y == 0 /*& numberToFactorize!=0*/)     // if devisibe without rest 
                        {
                            factors[z, x] = y;
                            x++;
                            factorCounter[y]++;

                            // while (numberToFactorize % y == 0)  //if a factor is found numberToFactorize is divided by it until division without rest is not possible anymore
                            // {
                            //     numberToFactorize = numberToFactorize / y;
                            // }
                        }
                    }
                    //if (factors[z, 0] == 0) { factors[z, 0] = numberToFactorize; } //numberToFactorize is prime and is put on first place

                }
                integerArray = factorCounter;
                OnPropertyChanged("IntegerArray");

                Data.ValueCollection.Clear();
                double bigestheight = factorCounter[2];
                for (int z = 3; z <= factorCounter.Count() - 1; z++)
                {
                    if (bigestheight < factorCounter[z])
                    {
                        bigestheight = factorCounter[z];
                    }
                }

                for (int n = 2; n <= factorCounter.Count() - 1; n++)
                {

                    CollectionElement row = new CollectionElement(n, factorCounter[n], (factorCounter[n] * (180 / bigestheight)));
                    Data.ValueCollection.Add(row);
                }

                // OUTPUT
                StringOutput = "";
                for (int i = 2; i <= factorCounter.Count() - 1; i++)
                {
                    StringOutput += i + ":" + Convert.ToString(factorCounter[i]) + Environment.NewLine;
                    OnPropertyChanged("StringOutput");
                }

                presentation.OpenPresentationFile();

            }

            Progress(1.0, 1.0);
        }

        //P.S. The arrays used in the solution are:
        //checkedGramms -the array contains all checked gramms :)  
        //distances - The same size as checkedGramms, if the gramm doesn't repeat itself the value of distances[k] corresponding to checkedGramms[k] equals 0
        //factors - same size as checkedGramms and distances on one side ,and 20 (for now)on the other side for the factors , if the corresponding distance is 0 all the factors are also 0




        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            // throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
