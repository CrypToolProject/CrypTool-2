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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;



namespace CrypTool.Plugins.NFSFactorizer
{
    [Author("Inigo Querejeta", "i.querejeta.azurmendi@student.tue.nl", "Technical University of Eindhoven", "https://www.tue.nl/")]
    [PluginInfo("NFS Factorizer", "Factoring numbers with, among other algorithms, the NFS.", "NFSFactorizer/DetailedDescription/userdoc.xml", new[] { "NFSFactorizer/images.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]

    public class NFSFactorizer : ICrypComponent
    {
        #region Variables
        private readonly string _directoryName;
        private BigInteger inputNumber;
        private BigInteger[] FactorArray;
        private string CoFactArray;
        private string[] LogFileArray;
        private string ecmtmp;
        private BigInteger InputNumber1 = 1;
        private static readonly Random rnd = new Random();
        private string logFileName = rnd.Next(1000, 5000).ToString();
        private string siqsPath;

        private readonly NFSFactorizerSettings settings = new NFSFactorizerSettings();

        public List<User> lvi = new List<User>();

        public string cmndLine = "";
        public string redirectInfo = "";
        public string verbosity = "";
        public string timeLimit = "";

        private int nfsStart = 0;
        private string relationsNeeded = "";
        private string relationsFound = "";
        private double relationsRatio;
        private DateTime start;
        #endregion

        #region INotifyPropertyChanged Members
        public NFSFactorizer()
        {
            settings = new NFSFactorizerSettings();
            // settings.PropertyChanged += settings_PropertyChanged;
            Presentation = new NFSFactorizerPresentation();
            _directoryName = DirectoryHelper.DirectoryLocalTemp;
        }


        private NFSFactorizerPresentation nfsFactQuickWatchPresentation => Presentation as NFSFactorizerPresentation;

        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Input Number", "Enter the Number you want to Factorize")]
        public BigInteger InputNumber
        {
            get => inputNumber;
            set
            {
                if (InputNumber1 == 1)
                {
                    inputNumber = value;
                    FirePropertyChangedEvent("InputNumber");
                }
                else
                {
                    inputNumber = InputNumber1;
                    FirePropertyChangedEvent("InputNumber");
                }


            }
        }

        [PropertyInfo(Direction.OutputData, "Output Factors", "Returns the list of factors", true)]
        public BigInteger[] Factors
        {
            get => FactorArray;
            set
            {
                FactorArray = value;
                FirePropertyChangedEvent("Factors");
            }
        }

        [PropertyInfo(Direction.OutputData, "Primality of factors", "Determines whether the factorization was successful into prime factors", false)]
        public string CoFact
        {
            get => CoFactArray;
            set
            {
                CoFactArray = value;
                FirePropertyChangedEvent("CoFact");
            }
        }

        [PropertyInfo(Direction.OutputData, "log-file", "log-file", false)]
        public string[] LogFile
        {
            get => LogFileArray;
            set
            {
                LogFileArray = value;
                FirePropertyChangedEvent("LogFile");
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate



            {
                nfsFactQuickWatchPresentation.resultInfo.ItemsSource = null;
                nfsFactQuickWatchPresentation.Details.Text = "";
                nfsFactQuickWatchPresentation.QSNFS7.Content = "";
                nfsFactQuickWatchPresentation.QSNFS6.Content = "Algorithm - ";
                nfsFactQuickWatchPresentation.QSNFS5.Content = "Algebra step - ";
                nfsFactQuickWatchPresentation.QSNFS4.Content = "Number of Relations - ";
                nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - ";
                nfsFactQuickWatchPresentation.QSNFS2.Content = "Polynomial Selection - ";

            }
                    , null);

        }
        public void CheckLogFile()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(logFileName),

                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,

                Filter = Path.GetFileName(logFileName)
            };
            watcher.Changed += new FileSystemEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;

        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            nfsStart = 0;
            //string line;

            /*StreamReader file = new StreamReader(logFileName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("nfs: best poly ="))
                    nfsStart = 0;
            }*/

            //file.Close();
        }

        public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {

                redirectInfo += "\r\n" + e.Data.ToString();

                if (nfsStart == 0)
                {


                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (e.Data.StartsWith("ecm: "))
                        {
                            ecmtmp = nfsFactQuickWatchPresentation.Details.Text.ToString();
                            nfsFactQuickWatchPresentation.Details.Text = ecmtmp.Substring(0, ecmtmp.LastIndexOf(Environment.NewLine)) + "\r\n" + e.Data;
                        }
                        else if (e.Data.Contains(" rels found: "))
                        {
                            nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Visible;
                            ecmtmp = nfsFactQuickWatchPresentation.Details.Text.ToString();
                            nfsFactQuickWatchPresentation.Details.Text = ecmtmp.Substring(0, ecmtmp.LastIndexOf(Environment.NewLine)) + "\r\n" + e.Data;
                            relationsFound = e.Data.Before(" rels found: ");
                            ProgressChanged(int.Parse(relationsFound) * 100 / int.Parse(relationsNeeded), 100);
                            DateTime present = DateTime.Now;
                            TimeSpan total = present.Subtract(start);
                            double ratio = (total.TotalMilliseconds / 1000) / double.Parse(relationsFound);
                            relationsRatio = Math.Abs((double.Parse(relationsNeeded) - double.Parse(relationsFound)) * ratio);
                            nfsFactQuickWatchPresentation.QSNFS4.Content = "Number of relations - " + relationsFound;
                            if (!(Math.Round(relationsRatio, 0).ToString() == "0"))
                            {
                                nfsFactQuickWatchPresentation.QSNFS7.Content = "ETA : " + Math.Round(relationsRatio, 0).ToString() + " seconds";
                            }

                        }
                        else if (e.Data.StartsWith("==== post"))
                        {
                            nfsStart = 2;
                        }
                        else if (e.Data.StartsWith("==== sieve "))
                        {
                            nfsStart = 2;
                        }
                        else if (e.Data.Contains("==== sieving in progress ("))
                        {
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                            start = DateTime.Now;

                            relationsNeeded = e.Data.Between("): ", " relations needed");
                            nfsFactQuickWatchPresentation.QSNFS6.Content = "Algorithm - Quadratic sieve";
                            nfsFactQuickWatchPresentation.QSNFS2.Content = "Polynomial Selection - N/A";
                            nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - " + relationsNeeded + " relations needed";
                        }

                        else if (e.Data.Contains("error - 11 reading relation"))
                        {
                            Stop();
                        }
                        else
                        {
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                        }







                        if (e.Data.Contains("found prime") | e.Data.Contains("found prp"))
                        {

                            BigInteger nmbr = BigInteger.Parse(e.Data.After("= "));
                            lvi.Add(new User() { Factors = nmbr, Primality = nmbr.IsProbablePrime().ToString(), Algorithm = e.Data.Before(": ") });
                            nfsFactQuickWatchPresentation.resultInfo.ItemsSource = null;
                            nfsFactQuickWatchPresentation.resultInfo.ItemsSource = lvi;

                        }



                        if (e.Data.Contains("N too big"))
                        {



                            CoFact = "N too big, choose another algorithm.";
                        }


                        if (e.Data.Contains("commencing number field sieve polynomial selection"))
                        {
                            nfsStart = 1;
                            nfsFactQuickWatchPresentation.resultInfo.Visibility = Visibility.Visible;
                            CheckLogFile();
                        }
                        nfsFactQuickWatchPresentation.Scroll.ScrollToBottom();
                    }, null);



























                }
                else if (nfsStart == 2)
                {
                    if (e.Data.StartsWith("trial factoring") | e.Data.StartsWith("commencing Lanczos"))
                    {
                        nfsStart = 0;
                    }
                }
                /*nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (nfsStart == 0)
                    {
                        if (e.Data.StartsWith("ecm: "))
                        {
                            ecmtmp = nfsFactQuickWatchPresentation.Details.Text.ToString();
                            nfsFactQuickWatchPresentation.Details.Text = ecmtmp.Substring(0, ecmtmp.LastIndexOf(Environment.NewLine)) + "\r\n" + e.Data;
                        }
                        else if (e.Data.Contains(" rels found: "))
                        {
                            nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Visible;
                            ecmtmp = nfsFactQuickWatchPresentation.Details.Text.ToString();
                            nfsFactQuickWatchPresentation.Details.Text = ecmtmp.Substring(0, ecmtmp.LastIndexOf(Environment.NewLine)) + "\r\n" + e.Data;
                            relationsFound = e.Data.Before(" rels found: ");
                            ProgressChanged(int.Parse(relationsFound) * 100 / int.Parse(relationsNeeded), 100);
                            DateTime present = DateTime.Now;
                            TimeSpan total = present.Subtract(start);
                            double ratio = (total.TotalMilliseconds / 1000) / double.Parse(relationsFound);
                            relationsRatio = Math.Abs((double.Parse(relationsNeeded) - double.Parse(relationsFound)) * ratio);
                            nfsFactQuickWatchPresentation.QSNFS4.Content = "Number of relations - " + relationsFound;
                            if (!(Math.Round(relationsRatio, 0).ToString() == "0"))
                            {
                                nfsFactQuickWatchPresentation.QSNFS7.Content = "ETA : " + Math.Round(relationsRatio, 0).ToString() + " seconds";
                            }

                        }
                        else if (e.Data.StartsWith("==== post"))
                            nfsStart = 1;
                        /*else if (e.Data.StartsWith("==== sieve "))

                            nfsStart = 1;
                        else if (e.Data.Contains("==== sieving in progress ("))
                        {
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                            start = DateTime.Now;

                            relationsNeeded = e.Data.Between("): ", " relations needed");
                            nfsFactQuickWatchPresentation.QSNFS6.Content = "Algorithm - Quadratic sieve";
                            nfsFactQuickWatchPresentation.QSNFS2.Content = "Polynomial Selection - N/A";
                            nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - " + relationsNeeded + " relations needed";
                        }
                        else
                        {
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                        }

                        if (e.Data.Contains("found prime") | e.Data.Contains("found prp"))
                        {
                            BigInteger nmbr = BigInteger.Parse(e.Data.After("= "));
                            lvi.Add(new User() { Factors = nmbr, Primality = nmbr.IsProbablePrime().ToString(), Algorithm = e.Data.Before(": ") });
                            nfsFactQuickWatchPresentation.resultInfo.ItemsSource = null;
                            nfsFactQuickWatchPresentation.resultInfo.ItemsSource = lvi;

                        }
                        if (e.Data.Contains("N too big"))
                        {
                            CoFact = "N too big, choose another algorithm.";
                        }
                        if (e.Data.Contains("nfs: "))
                        {
                            nfsStart = 1;
                            nfsFactQuickWatchPresentation.resultInfo.Visibility = Visibility.Visible;
                        }

                    }
                    else
                    {
                        nfsFactQuickWatchPresentation.Details.Text += "\r\n" + nfsStart.ToString() + "  " + e.Data.ToString();

                        /*
                        /*if (e.Data.StartsWith("nfs: "))
                        {
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                        }
                        if (e.Data.StartsWith("trial factoring"))
                            nfsStart = 0;
                        else if (e.Data.StartsWith("commencing Lanczos"))
                        {
                            nfsStart = 0;
                            nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();
                            nfsFactQuickWatchPresentation.QSNFS5.Content += "Algebra - Lanczos started";
                        }
                        if (e.Data.StartsWith("nfs: commencing nfs "))
                        {
                            nfsFactQuickWatchPresentation.QSNFS6.Content = "Algorithm - Number Field Sieve";
                            nfsFactQuickWatchPresentation.QSNFS7.Content = "Searching for a special form of the number";
                        }
                        if (e.Data.StartsWith("nfs: commencing polynomial search"))
                        {
                            nfsFactQuickWatchPresentation.QSNFS7.Content = "No special form found in the inputed number";
                            nfsFactQuickWatchPresentation.QSNFS2.Content = "Polynomial Selection - Started, be patient.";
                        }
                        if (e.Data.StartsWith("nfs: commencing algebraic side"))
                        {
                            nfsStart = 0;
                            nfsFactQuickWatchPresentation.QSNFS7.Content = "";
                            nfsFactQuickWatchPresentation.QSNFS2.Content = "Polynomial Selection - Completed";
                            nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - Algebraic side lattice sieving";
                        }
                        if (e.Data.StartsWith("nfs: commencing msieve filtering"))
                        {
                            nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - Filtering the relations";
                        }
                        if (e.Data.StartsWith("nfs: commencing msieve linear algebra"))
                        {
                            nfsFactQuickWatchPresentation.QSNFS3.Content = "Sieving - Completed";
                            nfsFactQuickWatchPresentation.QSNFS5.Content = "Algebra - Started finding dependencies";
                        }
                        if (e.Data.StartsWith("nfs: commencing msieve sqrt"))
                        {
                            nfsFactQuickWatchPresentation.QSNFS5.Content = "Algebra - Completed";
                            nfsFactQuickWatchPresentation.QSNFS7.Content = "Calculating the square root";
                        }
                        if (e.Data.StartsWith("NFS elapsed time"))
                        {
                            nfsFactQuickWatchPresentation.QSNFS7.Content = "Completed";
                        }
                    }

                    /*if (!e.Data.Contains("ctrl-c"))
                    {
                        nfsFactQuickWatchPresentation.Details.Text += "\r\n" + e.Data.ToString();



                    }
                    nfsFactQuickWatchPresentation.Scroll.ScrollToBottom();
                }

                    , null);*/


            }


        }
        public class User
        {
            public BigInteger Factors { get; set; }
            public string Primality { get; set; }
            public string Algorithm { get; set; }
        }

        private bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }
        public void ConvertToMPEG()
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = cmndLine;
            cmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\yafu-1.34";

            cmd.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);

            cmd.Start();
            cmd.BeginOutputReadLine();
        }

        public void Execute()
        {
            ProgressChanged(0, 100);
            siqsPath = Path.Combine(_directoryName, "siqs.dat");
            if (File.Exists(siqsPath))
            {
                File.Delete(siqsPath);
            }

            logFileName = Path.Combine(_directoryName, rnd.Next(1000, 5000).ToString());
            if (InputNumber <= 0)
            {
                FireOnGuiLogNotificationOccuredEventError("Input must be a natural number > 0");
                return;
            }
            string method = "";
            string Choice = "";
            string option = "";
            string idle = "";
            string SecondArg = "";
            switch (settings.Algs)
            {
                case 0:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Visible;
                    }
                    , null);
                    method = "siqs";
                    Choice = "Quadratic Sieve";
                    if (settings.TimeLimit)
                    {
                        option += " -siqsT " + settings.Seconds;
                    }

                    break;
                case 1:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "smallmpqs";
                    Choice = "Multy Polynomial QS";
                    break;
                case 2:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Visible;
                    }
                    , null);
                    method = "nfs";
                    Choice = "Number Field Sieve";
                    option = " -v";
                    if (settings.TimeLimit)
                    {
                        option += " -ggnfsT " + settings.Seconds;
                    }

                    break;
                case 3:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "squfof";
                    Choice = "SqufOf";
                    break;
                case 4:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "pm1";
                    Choice = "Pollards p minus 1";
                    break;
                case 5:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "pp1";
                    Choice = "Williams p plus one";
                    break;
                case 6:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "rho";
                    Choice = "Pollards Rho method";
                    break;
                case 7:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "trial";
                    Choice = "Trial Division";
                    SecondArg = "," + settings.BruteForceLimit;
                    break;
                case 8:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "ecm";
                    Choice = "Elliptic curve method";
                    break;
                case 9:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    method = "fermat";
                    Choice = "Fermats method";
                    SecondArg = "," + settings.MaxIt;
                    break;
                case 10:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Visible;
                    }
                    , null);
                    Choice = "Special number field Sieve";
                    if (settings.knownFactor)
                    {
                        method = "snfs";
                        SecondArg = "," + settings.Factor;
                        option = " -v";
                    }
                    else
                    {
                        method = "nfs";
                        option = " -v";
                    }
                    if (settings.TimeLimit)
                    {
                        option += " -ggnfsT " + settings.Seconds;
                    }

                    break;
                case 11:
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Border.Visibility = Visibility.Hidden;
                    }
                    , null);
                    Choice = "different algorithms";
                    method = "factor";
                    switch (settings.Plan)
                    {
                        case 0:
                            option = "-plan none -R -v";


                            break;
                        case 1:
                            option = "-plan noecm -R -v";


                            break;
                        case 2:
                            option = "-plan light -R -v";


                            break;
                        case 3:
                            option = "-plan normal -R -v";


                            break;
                        case 4:
                            option = "-plan deep -R -v";
                            break;
                    }
                    if (settings.OneFactor)
                    {
                        option += " -one";
                    }

                    break;
            }

            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                nfsFactQuickWatchPresentation.NmbrToFactor.Content = inputNumber;
                nfsFactQuickWatchPresentation.ComplementaryInfo.Content = BigIntegerHelper.BitCount(inputNumber) + " bit number with " + inputNumber.ToString().Length + " decimal digits."; // probably too long.
                nfsFactQuickWatchPresentation.SelMethod.Content = "Factorization with " + Choice + ".";
            }
            , null);

            if (settings.Action)
            {
                idle = "-p";
            }

            if (settings.Verbosity)
            {
                verbosity = "-v";
            }

            cmndLine = string.Format("/c yafu-x64.exe \"{0}({1}{2})\" -R -threads {3} {4} {5} -logfile {6} {7}", method, InputNumber, SecondArg, settings.Threads + 1, option, idle, logFileName, verbosity); //D:\\Documents\\CrypTool2\\CrypBuild\\Debug\\
            redirectInfo = "";
            ConvertToMPEG();
            while (!redirectInfo.Contains("ans = "))
            {
            }



            string tmp = redirectInfo;
            List<string> primality = new List<string>();

            string facts = tmp.Between("***factors found***", "ans = ");
            string remaind = tmp.After("ans = ");
            BigInteger remainder = BigInteger.Parse(remaind);

            string cofact;
            string coFact = "1";
            string extraProgress = "";
            List<BigInteger> Facts = new List<BigInteger>();
            if (facts.Contains("***co-factor***"))
            {
                ProgressChanged(2, 100);
                facts = tmp.Between("***factors found***", "***co-factor***");
                cofact = tmp.Between("***co-factor***", "ans = ");
                foreach (string myString in cofact.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    coFact = myString.After("= ");
                }
            }
            else if (remainder != 1)
            {
                coFact = remainder.ToString();
            }

            foreach (string myString in facts.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                Facts.Add(BigInteger.Parse(myString.After("= ")));
            }

            if (coFact != "1" && !Facts.Contains(BigInteger.Parse(coFact)))
            {
                Facts.Add(BigInteger.Parse(coFact));
            }
            if (settings.Algs == 3)
            {
                Facts.Add(InputNumber / BigInteger.Parse(coFact));
            }

            Facts.Sort();
            Factors = Facts.ToArray();
            CoFact = "Factor are all prime. Factorization ended succesfully.";
            foreach (BigInteger myString in Facts)
            {
                primality.Add(myString.IsProbablePrime().ToString());
                if (!myString.IsProbablePrime())
                {
                    if (settings.TimeLimit)
                    {
                        CoFact = "Time finished with no result";
                        extraProgress = "Failed factoring due to time limit. Either augment, or factor using another algorithm.";
                    }

                    else
                    {
                        CoFact = "At least one of the factors is not prime, please use another algorithm or augment the boundaries.";
                        extraProgress = "\n At least one of the factors is not prime. This may be due to the fact that the algorithm chosen outputs a difference of squares, \n or that it is not powerfull enough to factor the number imputed. \n \n Change the algorithm (General Factorization will give all factors) or try with higher boundaries.";
                    }

                }
            }



            var numbersAndWords = Facts.Zip(primality, (n, w) => new { Number = n, Word = w });
            foreach (var l in numbersAndWords)
            {
                if (!lvi.Exists(x => x.Factors == l.Number))
                {
                    lvi.Add(new User() { Factors = l.Number, Primality = l.Word.ToString(), Algorithm = "" });
                }
            }

            if (CoFact == "Factor are all prime. Factorization ended succesfully.")
            {
                extraProgress = "\n Factorization terminated successfully finding a total of " + lvi.Count.ToString() + " factors.";
            }
            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(100, 100);
            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                nfsFactQuickWatchPresentation.resultInfo.ItemsSource = null;
                nfsFactQuickWatchPresentation.resultInfo.ItemsSource = lvi;
                nfsFactQuickWatchPresentation.Details.Text = redirectInfo + "\n" + extraProgress + "\n";
                nfsFactQuickWatchPresentation.QSNFS7.Content = "Completed";
                if (nfsFactQuickWatchPresentation.ShowDetailsButton.IsChecked == true)
                {
                    List<string> tempLog = new List<string>();
                    string[] lines = File.ReadAllLines(logFileName);
                    foreach (string line in lines)
                    {
                        tempLog.Add(line.After(", "));
                    }
                    LogFile = tempLog.ToArray();
                }
                else
                {
                    string[] arr = new string[] { };
                    LogFile = arr;
                }
            }
            , null);
        }

        public void PostExecution()
        {
            File.Delete(logFileName);
        }

        public void Stop()
        {
            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                nfsFactQuickWatchPresentation.Details.Text = redirectInfo;
            }
            , null);
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.StartInfo.Arguments = "/c taskkill /F /IM yafu-x64.exe & gnfs-*";
            cmd.Start();
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
        private void FireOnGuiLogNotificationOccuredEvent(string message, NotificationLevel lvl)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, lvl));
            }
        }
        private void FireOnGuiLogNotificationOccuredEventError(string message)
        {
            FireOnGuiLogNotificationOccuredEvent(message, NotificationLevel.Error);
        }

        #endregion // Good
    }
}