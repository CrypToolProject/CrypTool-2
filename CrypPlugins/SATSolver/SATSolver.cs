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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.SATSolver
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("SATSolver.Properties.Resources", "PluginCaption", "PluginDescription", "SATSolver/Documentation/doc.xml", new[] { "SATSolver/Images/sat-race-logo.gif" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class SATSolver : ICrypComponent
    {

        #region Private Variables

        private readonly SATSolverSettings settings = new SATSolverSettings();

        private StringBuilder outputConsoleString;
        private CStreamWriter outputConsoleStream;
        private CStreamWriter outputResultStream;
        private CStreamReader inputStream;
        private readonly Encoding encoding = Encoding.UTF8;
        private Process solverProcess = null;
        private StreamReader reader = null;

        private string outputResultFilename = null;
        private string tempCnfFilename = null;


        public static readonly string TEMP_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"CrypTool2\Temp\SATSolver");
        private static readonly string OUTPUT_RESULT_FILENAME = "SATresult";
        private static readonly string TEMP_CNF_FILENAME = "SATtemp";

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputConsoleCaption", "OutputConsoleTooltip", true)]
        public ICrypToolStream OutputConsoleStream
        {
            get => outputConsoleStream;
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputResultCaption", "OutputResultTooltip", true)]
        public ICrypToolStream OutputResultStream
        {
            get => outputResultStream;
            set
            {
                // empty
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            try
            {
                if (!Directory.Exists(TEMP_FOLDER))
                {
                    Directory.CreateDirectory(TEMP_FOLDER);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Can not create temp folder \"{0}\": {1}", TEMP_FOLDER, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 100);

            //generate random id for temp files to allow SATSolver being executed in parallel
            uint randomId = (uint)Guid.NewGuid().GetHashCode();
            outputResultFilename = OUTPUT_RESULT_FILENAME + "_" + randomId;
            tempCnfFilename = TEMP_CNF_FILENAME + "_" + randomId;

            #region lokale variablen

            const int BUFFERSIZE = 524288;
            int exitcode = -1;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string solverExe = settings.solverExe[(int)settings.SatString];

            //GuiLogMessage(path + "\\" + solverExe, NotificationLevel.Debug);

            outputResultStream = new CStreamWriter();
            inputStream = InputStream.CreateReader();
            BinaryWriter bw;

            if (settings.ClearOutputHandling == 0)
            {
                outputConsoleString = null; // delete output from last execution
                outputConsoleString = new StringBuilder();
            }
            else
            {
                outputConsoleString.AppendLine();
                outputConsoleString.AppendLine();
            }

            #endregion

            try
            {
                //GuiLogMessage("Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), NotificationLevel.Debug); // right
                //GuiLogMessage("Environment.CurrentDirectory " + path, NotificationLevel.Debug); // wrong

                // read from input stream and save as temporary file (which will later be passed to the SAT Solver)
                int bytesRead = 0;
                byte[] bytes = new byte[BUFFERSIZE];

                using (bw = new BinaryWriter(File.Open(Path.Combine(TEMP_FOLDER, tempCnfFilename), FileMode.Create)))
                {
                    while ((bytesRead = inputStream.ReadFully(bytes)) > 0)
                    {
                        bw.Write(bytes, 0, bytesRead);
                    }
                }

                // build argument string
                string args = buildArgs();

                solverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path + "\\" + solverExe,
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                    }
                };


                // DEBUG print args
                /*
                string debug_args = "";
                if (args.Contains("-"))
                {
                    debug_args = args.Substring(args.IndexOf("-"), args.Length - args.IndexOf("-"));
                }
                outputConsoleString.AppendLine("Arguments: " + debug_args);
                outputConsoleStream = new CStreamWriter();
                outputConsoleStream.Write(encoding.GetBytes(outputConsoleString.ToString()));
                outputConsoleStream.Close();
                OnPropertyChanged("OutputConsoleStream");
                */

                // attach event handler for process' data output
                solverProcess.OutputDataReceived += new DataReceivedEventHandler(ReadProcessOutput);

                solverProcess.Start();
                ProgressChanged(1, 100);

                // begin asynchronous read
                solverProcess.BeginOutputReadLine();
                solverProcess.WaitForExit();
                exitcode = solverProcess.ExitCode;

                // action depends on exitcode and used solver (error handling / result output)
                if (exitcode != -1)
                {
                    exitcodeHandling(exitcode);
                }
                else
                {
                    GuiLogMessage("Solver returned without exitcode!", NotificationLevel.Warning);
                }

                outputResultStream.Close();
                OnPropertyChanged("OutputResultStream");
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message + "\r\n" + exception.StackTrace, NotificationLevel.Error);
            }
            finally
            {
                File.Delete(Path.Combine(TEMP_FOLDER, outputResultFilename));
                File.Delete(Path.Combine(TEMP_FOLDER, tempCnfFilename));
            }
            ProgressChanged(100, 100);
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
            try
            {
                solverProcess.Kill();
            }
            catch (Exception)
            {
            }
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

        #region Helper Methoden

        private void ReadProcessOutput(object sendingProcess, DataReceivedEventArgs data)
        {
            //Debug.WriteLine("ReadProcessOutput event fired at {0}", System.DateTime.UtcNow);
            switch (settings.SatString)
            {
                case SATSolverSettings.Solver.MiniSAT:
                    {
                        if (!(data.Data == null))
                        {
                            string str = data.Data.ToString();
                            if (str.Contains("WARNING!"))
                            {
                                GuiLogMessage(data.Data.ToString(), NotificationLevel.Warning);
                            }
                            else
                            {
                                if (str.Contains("Parse time"))
                                {
                                    ProgressChanged(5, 100);
                                }
                                else if (str.Contains("Simplification time"))
                                {
                                    ProgressChanged(10, 100);
                                }
                                else if (str.Contains("restarts"))
                                {
                                    ProgressChanged(99, 100);
                                }
                                outputConsoleString.AppendLine(data.Data.ToString());
                                outputConsoleStream = new CStreamWriter();
                                outputConsoleStream.Write(encoding.GetBytes(outputConsoleString.ToString()));
                                outputConsoleStream.Close();
                                OnPropertyChanged("OutputConsoleStream");
                            }

                        }
                        break;
                    }
                default: break;
            }
        }

        private string buildArgs()
        {
            StringBuilder sb = new StringBuilder("\"" + Path.Combine(TEMP_FOLDER, tempCnfFilename) + "\" \"" + Path.Combine(TEMP_FOLDER, outputResultFilename) + "\"");
            double dbl;
            int intgr;

            switch (settings.SatString)
            {
                case SATSolverSettings.Solver.MiniSAT:
                    {
                        if (!settings.RFirstHandling.Equals("100"))
                        {
                            try
                            {
                                intgr = int.Parse(settings.RFirstHandling);
                                if (intgr < 2147483647 && intgr > 0)
                                {
                                    sb.Append(" -rfirst=" + settings.RFirstHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (!settings.RIncHandling.Equals("2"))
                        {
                            try
                            {
                                dbl = double.Parse(settings.RIncHandling);
                                if (dbl < 2147483647 && dbl > 0)
                                {
                                    sb.Append(" -rinc=" + settings.RIncHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (settings.RandomInitHandling != 1)
                        {
                            sb.Append(" -rnd-init");
                        }
                        if (settings.LubyHandling != 0)
                        {
                            sb.Append(" -no-luby");
                        }
                        if (!settings.RandomFreqHandling.Equals("0"))
                        {
                            try
                            {
                                dbl = double.Parse(settings.RandomFreqHandling);
                                if (dbl >= 0 && dbl <= 1)
                                {
                                    sb.Append(" -rnd-freq=" + settings.RandomFreqHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (!settings.VarDecayHandling.Equals("0.95"))
                        {
                            try
                            {
                                dbl = double.Parse(settings.VarDecayHandling);
                                if (dbl > 0 && dbl < 1)
                                {
                                    sb.Append(" -var-decay=" + settings.VarDecayHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (!settings.ClauseDecayHandling.Equals("0.999"))
                        {
                            try
                            {
                                dbl = double.Parse(settings.ClauseDecayHandling);
                                if (dbl < 1 && dbl > 0)
                                {
                                    sb.Append(" -cla-decay=" + settings.ClauseDecayHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (settings.PhaseSavingHandling != 2)
                        {
                            if (settings.PhaseSavingHandling == 0)
                            {
                                sb.Append(" -phase-saving=0");
                            }
                            if (settings.PhaseSavingHandling == 1)
                            {
                                sb.Append(" -phase-saving=1");
                            }
                        }
                        if (settings.CCMinHandling != 2)
                        {
                            if (settings.CCMinHandling == 0)
                            {
                                sb.Append(" -ccmin-mode=0");
                            }
                            if (settings.CCMinHandling == 1)
                            {
                                sb.Append(" -ccmin-mode=1");
                            }
                        }
                        if (settings.VerbosityHandling != 1)
                        {
                            if (settings.VerbosityHandling == 0)
                            {
                                sb.Append(" -verb=0");
                            }
                            if (settings.VerbosityHandling == 2)
                            {
                                sb.Append(" -verb=2");
                            }
                        }
                        if (settings.PreprocessHandling != 0)
                        {
                            sb.Append(" -no-pre");
                        }
                        /*
                        if (!settings.CPULimitHandling.Equals("2147483647"))
                        {
                            try
                            {
                                dbl = Double.Parse(settings.CPULimitHandling);
                                if (dbl <= 2147483647 && dbl > 0)
                                {
                                    sb.Append(" -cpu-lim=" + settings.CPULimitHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (!settings.MEMLimitHandling.Equals("2147483647"))
                        {
                            try
                            {
                                dbl = Double.Parse(settings.MEMLimitHandling);
                                if (dbl <= 2147483647 && dbl > 0)
                                {
                                    sb.Append(" -mem-lim=" + settings.MEMLimitHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        */
                        if (settings.DimacsHandling != 1)
                        {
                            sb.Append(" -dimacs=" + Path.Combine(TEMP_FOLDER, outputResultFilename));
                        }
                        if (settings.ElimHandling != 0)
                        {
                            sb.Append(" -no-elim");
                        }
                        if (!settings.SubLimitHandling.Equals("1000"))
                        {
                            try
                            {
                                intgr = int.Parse(settings.SubLimitHandling);
                                if (intgr <= 2147483647 && intgr >= -1)
                                {
                                    sb.Append(" -sub-lim=" + settings.SubLimitHandling);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (settings.RCheckHandling != 1)
                        {
                            sb.Append(" -rcheck");
                        }
                        break;
                    }
                default: break;
            }


            return sb.ToString();
        }

        private void exitcodeHandling(int exitcode)
        {
            switch (settings.SatString)
            {
                case SATSolverSettings.Solver.MiniSAT:
                    {
                        switch (exitcode)
                        {
                            case 1:
                                {
                                    GuiLogMessage("ERROR: Could not open file", NotificationLevel.Error);
                                    break;
                                }
                            case 2:
                                {
                                    GuiLogMessage("INTERRUPTED", NotificationLevel.Info);
                                    break;
                                }
                            case 3:
                                {
                                    GuiLogMessage("PARSE ERROR: Unexpected char in cnf (Maybe a clause is not terminated by '0'?)", NotificationLevel.Error);
                                    break;
                                }
                            case 4:
                                {
                                    GuiLogMessage("INDETERMINATE ERROR! (Maybe out of memory?)", NotificationLevel.Error);
                                    break;
                                }
                            case 5:
                                {
                                    GuiLogMessage("ERROR: Invalid Option", NotificationLevel.Error);
                                    break;
                                }
                            case 10: goto case 20; // case 10: UNSAT, case 20: SAT
                            case 20:
                                {
                                    string line;
                                    //while ((line = reader.ReadLine()) != null)
                                    //{
                                    //    line += "\n";
                                    //    outputConsoleStream.Write(encoding.GetBytes(line));
                                    //}
                                    //reader.Close();

                                    using (reader = new StreamReader(Path.Combine(TEMP_FOLDER, outputResultFilename)))
                                    {
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            line += "\n";
                                            outputResultStream.Write(encoding.GetBytes(line));
                                        }
                                    }
                                    break;
                                }
                            //case 30: // print usage help
                            //    {
                            //        string line;
                            //        while ((line = errorReader.ReadLine()) != null)
                            //        {
                            //            line += "\n";
                            //            outputConsoleStream.Write(encoding.GetBytes(line));
                            //        }
                            //        errorReader.Close();
                            //        break;
                            //    }
                            case 40: goto case 20; // print dimacs (output preprocessed cnf)
                            default:
                                {
                                    GuiLogMessage("Exitcode " + exitcode, NotificationLevel.Debug);
                                    break;
                                }
                        }
                        break;
                    }
                default: break;
            }
        }

        #endregion
    }
}
