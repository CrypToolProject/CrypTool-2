/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using CadoNFS.Presentation;
using CadoNFS.Processing;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.CadoNFS
{
    [Author("Sven Rech", "rech@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CADO-NFS", 
        "Factorization of numbers using number field sieve algorithm from CADO-NFS", 
        "CadoNFS/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CadoNFS : ICrypComponent
    {
        #region Private Variables

        private readonly DirectoryInfo workingDir;
        private BigInteger inputNumber;
        private BigInteger[] outputFactors;
        private readonly CadoNFSSettings settings = new CadoNFSSettings();
        private readonly ViewModel presentationViewModel = new ViewModel();
        private readonly Presentation presentation;
        private readonly PythonThread pythonThread = new PythonThread();
        private Interrupter lastInterrupter;

        #endregion

        #region Data Properties

        /// <summary>
        /// The input number to factorize.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputNumberCaption", "InputNumberTooltip")]
        public BigInteger InputNumber
        {
            get
            {
                return inputNumber;
            }
            set
            {
                this.inputNumber = value;
                OnPropertyChanged(nameof(InputNumber));
            }
        }

        /// <summary>
        /// The factors of the input number.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputFactorsCaption", "OutputFactorsTooltip")]
        public BigInteger[] OutputFactors
        {
            get
            {
                return outputFactors;
            }
            set
            {
                this.outputFactors = value;
                OnPropertyChanged(nameof(OutputFactors));
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        public CadoNFS()
        {
            presentation = new Presentation(presentationViewModel);
            workingDir = Directory.CreateDirectory(Path.Combine(DirectoryHelper.DirectoryLocalTemp, "cadonfs"));
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            lastInterrupter?.Ignore();
            presentationViewModel.SetFactors(Array.Empty<BigInteger>());
            presentationViewModel.ClearLoggingPresentations();

            var cadoNfsDir = Path.Combine(workingDir.FullName, "application");
            var scriptPath = Path.Combine(cadoNfsDir, "cado-nfs.py");
            if (!File.Exists(scriptPath))
            {
                GuiLogMessage($"CADO-NFS not available. Please place it manually into path '{cadoNfsDir}'.", NotificationLevel.Error);
                return;
            }

            var server = new CadoNFSServer(cadoNfsDir);
            presentationViewModel.AddLoggingPresentation("Server", server.Logging);

            var processStatusHandler = new ProcessStatusHandler(server.Logging, presentationViewModel);
            processStatusHandler.HttpServerRunningEvent += () =>
            {
                GuiLogMessage("HTTP server running. Starting workers now.", NotificationLevel.Info);
                for (var workerNr = 1; workerNr <= settings.CoresUsedIndex + 1; workerNr++)
                {
                    var client = new CadoNFSClient(workerNr, cadoNfsDir);
                    //Individual logging of worker not working correctly right now:
                    //presentationViewModel.AddLoggingPresentation($"Worker {clientNr}", client.Logging);
                    var workerThread = new Thread(RunWorkerThread);
                    workerThread.Start(client);
                }
            };

            ProgressChanged(0, 1);

            var factors = pythonThread.Exec(() => 
            {
                PythonEnvironment.Prepare(cadoNfsDir);
                return server.Factorize(InputNumber)?.ToArray();
            });

            if (factors == null)
            {
                GuiLogMessage("Factorization failed.", NotificationLevel.Error);
                return;
            }

            presentationViewModel.CurrentProcessingStep = ViewModel.ProcessingSteps.Completed;
            presentationViewModel.CurrentProcessingStepProgress = 100;
            presentationViewModel.SetFactors(factors);
            OutputFactors = factors;

            ProgressChanged(1, 1);
        }

        private void RunWorkerThread(object arg)
        {
            var client = (CadoNFSClient)arg;
            try
            {
                client.Run();
            }
            catch (Exception ex)
            {
                GuiLogMessage($"Error in thread of worker {client.WorkerNr}: {ex.Message}", NotificationLevel.Error);
            }
            finally
            {
                GuiLogMessage($"Thread of worker {client.WorkerNr} stopped.", NotificationLevel.Info);
            }
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
            lastInterrupter = new Interrupter();
            lastInterrupter.Interrupt();

            //Does not work (crashes):
            //Python.Runtime.PythonEngine.Shutdown();
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            pythonThread.Start();
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
