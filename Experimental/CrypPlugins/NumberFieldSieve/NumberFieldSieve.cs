/*  
   Copyright 2011 Selim Arikan, Istanbul University

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
using Ionic.Zip;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace NumberFieldSieve
{
    [Author("Sven Rech", "rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("NumberFieldSieve.Properties.Resources", "NumberFieldSieveCaption", "NumberFieldSieveTooltip", "NumberFieldSieve/DetailedDescription/doc.xml", "NumberFieldSieve/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class NumberFieldSieve : ICrypComponent
    {
        private readonly NumberFieldSieveSettings _settings;
        private readonly NumberFieldSievePresentation _presentation;
        private readonly string _directoryName;
        private BigInteger _inputNumber;
        private BigInteger[] _outputFactors;
        private string _status;
        private bool _stop;
        private ScriptScope _scope;
        private int _statusNr = 0;
        private Thread _executingThread;
        private Action _kill;

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                    if (value != null)
                    {
                        GuiLogMessage("Status update: " + value, NotificationLevel.Info);
                    }
                }
            }
        }

        /// <summary>
        /// Getter / Setter for the input number which should be factorized
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputNumberCaption", "InputNumberTooltip")]
        public BigInteger InputNumber
        {
            get => _inputNumber;
            set
            {
                _inputNumber = value;
                OnPropertyChanged("InputNumber");
            }
        }

        /// <summary>
        /// Getter / Setter for the factors calculated by msieve
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputFactorsCaption", "OutputFactorsTooltip")]
        public BigInteger[] OutputFactors
        {
            get => _outputFactors;
            set
            {
                _outputFactors = value;
                OnPropertyChanged("OutputFactors");
            }
        }

        public NumberFieldSieve()
        {
            _settings = new NumberFieldSieveSettings();
            _presentation = new NumberFieldSievePresentation
            {
                DataContext = this
            };

            _directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "nfs");
            if (!Directory.Exists(_directoryName))
            {
                Directory.CreateDirectory(_directoryName);
            }
        }

        private void ExtractGGNFS()
        {
            string ggnfsDir = Path.Combine(_directoryName, "ggnfs");
            if (Directory.Exists(ggnfsDir))
            {
                Directory.Delete(ggnfsDir, true);
            }
            Uri resUri = new Uri("pack://application:,,,/NumberFieldSieve;component/ggnfs.zip");
            //Extract archive:
            using (Stream resStream = Application.GetResourceStream(resUri).Stream)
            using (ZipFile zipPackage = ZipFile.Read(resStream))
            {
                zipPackage.ExtractAll(_directoryName, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public void Execute()
        {
            _statusNr = 0;
            _stop = false;
            OutputFactors = null;
            ProgressChanged(0, 100);

            try
            {
                if (InputNumber < 1)
                {
                    GuiLogMessage("Can't factorize number smaller than 1", NotificationLevel.Error);
                    return;
                }

                string inputString = InputNumber.ToString();
                string name;
                if (inputString.Length > 10)
                {
                    name = inputString.Substring(0, 10) + "_" + inputString.GetHashCode();
                }
                else
                {
                    name = inputString;
                }
                string nFile = Path.Combine(_directoryName, name + ".n");
                if (!File.Exists(nFile))
                {
                    using (StreamWriter numberFile = File.CreateText(nFile))
                    {
                        numberFile.WriteLine("n: " + inputString);
                    }
                }

                Directory.SetCurrentDirectory(_directoryName);
                string ggnfsDir = Path.Combine(_directoryName, "ggnfs") + Path.DirectorySeparatorChar;
                ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();

                ICollection<string> searchPaths = engine.GetSearchPaths();
                searchPaths.Add(Path.Combine(ggnfsDir, "pythonlib"));
                engine.SetSearchPaths(searchPaths);
                using (GGNFSOutputStream outputStream = new GGNFSOutputStream(delegate (string buffer)
                                                                    {
                                                                        _presentation.Append(buffer);
                                                                        SetStatus(_scope.GetVariable<int>("status"),
                                                                                  _scope.GetVariable<double>(
                                                                                      "sievingProgress"));
                                                                    }))
                using (GGNFSOutputStream errorOutputStream = new GGNFSOutputStream(buffer => GuiLogMessage(buffer, NotificationLevel.Error)))
                {
                    engine.Runtime.IO.SetOutput(outputStream, Encoding.ASCII);
                    engine.Runtime.IO.SetErrorOutput(errorOutputStream, Encoding.ASCII);

                    ScriptScope scope = engine.CreateScope();
                    ScriptSource source = engine.CreateScriptSourceFromFile(Path.Combine(ggnfsDir, "factmsieve.py"));

                    source.Execute(scope);
                    _scope = scope;
                    scope.SetVariable("NAME", name);
                    scope.SetVariable("GGNFS_PATH", ggnfsDir);
                    scope.SetVariable("MSIEVE_PATH", ggnfsDir);
                    scope.SetVariable("NUM_CORES", _settings.NumCores);
                    scope.SetVariable("THREADS_PER_CORE", _settings.NumThreadsPerCore);
                    scope.SetVariable("USE_CUDA", _settings.UseCUDA);
                    scope.SetVariable("GPU_NUM", 1);

                    _executingThread = Thread.CurrentThread;
                    _kill = scope.GetVariable<Action>("Kill");
                    Func<List> main = scope.GetVariable<Func<List>>("Main");
                    List res = main.Invoke();

                    if (_stop || res == null || res.Count == 0)
                    {
                        return;
                    }

                    //give out factors:
                    List<BigInteger> factorList = new List<BigInteger>();
                    foreach (object factor in res)
                    {
                        if (factor is BigInteger)
                        {
                            factorList.Add((BigInteger)factor);
                        }
                        else if (factor is int)
                        {
                            factorList.Add((int)factor);
                        }
                        else if (factor is long)
                        {
                            factorList.Add((long)factor);
                        }
                    }
                    OutputFactors = factorList.ToArray();
                }
            }
            catch (SystemExitException)
            {
                if (!_stop)
                {
                    ShowStopMessage();
                }
            }
            catch (ThreadAbortException)
            {
                ShowStopMessage();
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                GuiLogMessage(Properties.Resources._Error_executing_number_field_sieve__ + ex.Message, NotificationLevel.Error);
            }
            finally
            {
                Status = null;
                _executingThread = null;
                _kill = null;
            }
        }

        private void ShowStopMessage()
        {
            GuiLogMessage(Properties.Resources._was_stopped_, NotificationLevel.Info);
        }

        private void SetStatus(int status, double sievingProgress)
        {
            if (!_stop && _statusNr != status)
            {
                switch (status)
                {
                    case 0:
                        if (_statusNr > 0)
                        {
                            Status = Properties.Resources.Finished;
                            ProgressChanged(100, 100);
                        }
                        else
                        {
                            ProgressChanged(0, 100);
                        }
                        break;
                    case 1:
                        Status = Properties.Resources.Finding_polynomial;
                        ProgressChanged(1, 100);
                        break;
                    case 2:
                        Status = Properties.Resources.Setting_up_factorization_step;
                        ProgressChanged(10, 100);
                        break;
                    case 3:
                        Status = Properties.Resources.Sieving;
                        ProgressChanged(11, 100);
                        break;
                    case 4:
                        Status = Properties.Resources.Solving_matrix;
                        ProgressChanged(90, 100);
                        break;
                }
                _statusNr = status;
            }
            else if (status == 3)   //update progress in sieving step
            {
                double pc = 11 + (sievingProgress / 100) * 80;
                ProgressChanged(Math.Min(89.9, pc), 100);
            }
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
            _stop = true;
            if (_kill != null)
            {
                try
                {
                    _kill.Invoke();
                }
                catch (SystemExitException)
                {
                    ShowStopMessage();
                    return;
                }
            }
            if (_executingThread != null)
            {
                _executingThread.Abort();
            }
        }

        public void Initialize()
        {
        }

        public void PreExecution()
        {
            ExtractGGNFS();
        }

        public void PostExecution()
        {
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));

            }
        }

        internal void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }
    }

    internal class GGNFSOutputStream : Stream
    {
        private readonly Action<string> _writeCallback;

        public GGNFSOutputStream(Action<string> writeCallback)
        {
            _writeCallback = writeCallback;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writeCallback(Encoding.ASCII.GetString(buffer, offset, count));
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position { get; set; }
    }
}
