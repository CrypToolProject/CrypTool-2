/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen
           Nils Kopal, CrypTool 2 Team

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
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerators;
using RandomNumberGenerator.Properties;
using RandomNumberGenerator.RandomNumberGenerators;

namespace CrypTool.Plugins.RandomNumberGenerator
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("RandomNumberGenerator.Properties.Resources", "PluginCaption", "PluginTooltip", "RandomNumberGenerator/userdoc.xml", new[] { "RandomNumberGenerator/images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsRandomNumbers)]
    public class RandomNumberGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly RandomNumberGeneratorSettings _settings = new RandomNumberGeneratorSettings();
        private RandomGenerator _generator;
        private int _outputlength = 0;
        private BigInteger _seed = BigInteger.Zero;
        private BigInteger _modulus = BigInteger.Zero;        
        private BigInteger _a = BigInteger.Zero;
        private BigInteger _b = BigInteger.Zero;

        private object _output;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputCaption", false)]
        public object Output => _output;

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            //Step 1: read and check settings
            ReadSettings();

            //Step 2: create algorithm object
            CreateRandomAlgorithm();

            //Step 3: generate output data
            GenerateRandomOutputData();

            ProgressChanged(1, 1);
        }

        private void CreateRandomAlgorithm()
        {
            switch (_settings.AlgorithmType)
            {
                case AlgorithmType.RandomRandom:

                    if (string.IsNullOrEmpty(_settings.Seed))
                    {
                        _generator = new NetRandomGenerator(_outputlength);
                    }
                    else
                    {
                        _generator = new NetRandomGenerator(_seed, _outputlength);
                    }
                    break;
                case AlgorithmType.RNGCryptoServiceProvider:
                    _generator = new NetCryptoRandomGenerator(_outputlength);
                    break;
                case AlgorithmType.X2modN:
                    _generator = new X2(_seed, _modulus, _outputlength);
                    break;
                case AlgorithmType.LCG:
                    _generator = new LCG(_seed, _modulus, _a, _b, _outputlength);
                    break;
                case AlgorithmType.ICG:
                    _generator = new ICG(_seed, _modulus, _a, _b, _outputlength);
                    break;
                case AlgorithmType.SubtractiveGenerator:
                    _generator = new SubtractiveGenerator(_seed, _outputlength);
                    break;
                case AlgorithmType.XORShift:
                    _generator = new XORShift(_seed, _outputlength, _settings.XORShiftType);
                    break;
                default:
                    throw new Exception(string.Format("Algorithm type {0} not implemented", _settings.AlgorithmType.ToString()));
            }
        }

        private void GenerateRandomOutputData()
        {
            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        _output = _generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        byte[] output = _generator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        byte[] output = _generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        //if the highest bit is set, we have to add 1 byte to allow
                        //the maximum number range and keep the number positive (highest bit =0)
                        if ((output[output.Length - 1] & 0b10000000) > 0)
                        {
                            byte[] temp = output;
                            output = new byte[output.Length + 1];
                            Array.Copy(temp, 0, output, 0, temp.Length);
                        }
                        _output = new BigInteger(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.NumberArray:
                    {
                        int outputamount;
                        if (string.IsNullOrEmpty(_settings.OutputAmount))
                        {
                            outputamount = 1;
                        }
                        else
                        {
                            try
                            {
                                outputamount = int.Parse(_settings.OutputAmount);
                            }
                            catch (Exception)
                            {
                                GuiLogMessage(string.Format(Resources.InvalidOutputAmount, _settings.Modulus), NotificationLevel.Error);
                                return;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = _generator.GenerateRandomByteArray();
                            //if the highest bit is set, we have to add 1 byte to allow
                            //the maximum number range and keep the number positive (highest bit =0)
                            if ((output[output.Length - 1] & 0b10000000) > 0)
                            {
                                byte[] temp = output;
                                output = new byte[output.Length + 1];
                                Array.Copy(temp, 0, output, 0, temp.Length);
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        _output = _generator.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
        }

        private void ReadSettings()
        {
            try
            {
                if (!string.IsNullOrEmpty(_settings.Seed))
                {
                    _seed = BigInteger.Parse(_settings.Seed);
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(_settings.Modulus))
                {
                    _modulus = BigInteger.Parse(_settings.Modulus);
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidModulus, _settings.Modulus), NotificationLevel.Error);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_settings.OutputLength))
                {
                    _outputlength = int.Parse(_settings.OutputLength);
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.OutputLength), NotificationLevel.Error);
                return;
            }
            if (_outputlength <= 0)
            {
                _outputlength = 1;
            }

            try
            {
                if (!string.IsNullOrEmpty(_settings.a))
                {
                    _a = BigInteger.Parse(_settings.a);
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidaValue, _settings.Modulus), NotificationLevel.Error);
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(_settings.b))
                {
                    _b = BigInteger.Parse(_settings.b);
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidbValue, _settings.Modulus), NotificationLevel.Error);
                return;
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
    }
}
