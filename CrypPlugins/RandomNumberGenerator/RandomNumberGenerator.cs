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

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerators;
using RandomNumberGenerator;
using RandomNumberGenerator.Properties;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace CrypTool.Plugins.RandomNumberGenerator
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("RandomNumberGenerator.Properties.Resources", "PluginCaption", "PluginTooltip", "RandomNumberGenerator/userdoc.xml", new[] { "RandomNumberGenerator/images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsRandomNumbers)]
    public class RandomNumberGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly RandomNumberGeneratorSettings _settings = new RandomNumberGeneratorSettings();

        private object _output;

        private BigInteger stdPrime = BigInteger.Parse("3");

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputCaption", false)]
        public object Output => _output;

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

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
            ProgressChanged(0, 1);

            bool executedWithoutError = false;

            switch (_settings.AlgorithmType)
            {
                case AlgorithmType.RandomRandom:
                    //Basic random number generator of .net:
                    executedWithoutError = ExecuteRandomRandomGenerator();
                    break;
                case AlgorithmType.RNGCryptoServiceProvider:
                    //CryptoService random number generator of .net
                    executedWithoutError = ExecuteRNGCryptoServiceProvider();
                    break;
                case AlgorithmType.X2modN:
                    executedWithoutError = ExecuteX2modNGenerator();
                    break;
                case AlgorithmType.LCG:
                    executedWithoutError = ExecuteLCG();
                    break;
                case AlgorithmType.ICG:
                    executedWithoutError = ExecuteICG();
                    break;
                case AlgorithmType.SubtractiveGenerator:
                    executedWithoutError = ExecuteXpat2();
                    break;
                case AlgorithmType.XORShift:
                    executedWithoutError = ExecuteXORShift();
                    break;
                default:
                    throw new Exception(string.Format("Algorithm type {0} not implemented", _settings.AlgorithmType.ToString()));
            }
            //We only show 100% if execution was without an error
            if (executedWithoutError)
            {
                ProgressChanged(1, 1);
            }
        }

        /// <summary>
        /// This method executes the basic random number generator of .net
        /// </summary>
        /// <returns></returns>
        private bool ExecuteRandomRandomGenerator()
        {
            Random random;
            if (string.IsNullOrEmpty(_settings.Seed))
            {
                random = new Random();
            }
            else
            {
                int seed;
                try
                {
                    seed = int.Parse(_settings.Seed);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                    return false;
                }
                random = new Random(seed);
            }

            int outputlength;
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }
            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        byte[] output = new byte[outputlength];
                        random.NextBytes(output);
                        _output = output;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        byte[] output = new byte[outputlength];
                        random.NextBytes(output);
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        byte[] output = new byte[outputlength];
                        random.NextBytes(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = new byte[outputlength];
                            random.NextBytes(output);
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;

                case OutputType.Bool:
                    {
                        _output = random.Next(2) == 1;
                        OnPropertyChanged("Output");
                    }
                    break;

                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
        }

        /// <summary>
        /// This method executes the RNGCryptoServiceProvider of .net
        /// </summary>
        /// <returns></returns>
        private bool ExecuteRNGCryptoServiceProvider()
        {
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {

                int outputlength;
                if (string.IsNullOrEmpty(_settings.OutputLength))
                {
                    outputlength = 0;
                }
                else
                {
                    try
                    {
                        outputlength = int.Parse(_settings.OutputLength);
                    }
                    catch (Exception)
                    {
                        GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                        return false;
                    }
                }
                switch (_settings.OutputType)
                {
                    case OutputType.ByteArray:
                        {
                            byte[] output = new byte[outputlength];
                            random.GetBytes(output);
                            _output = output;
                            OnPropertyChanged("Output");
                        }
                        break;
                    case OutputType.CrypToolStream:
                        {
                            byte[] output = new byte[outputlength];
                            random.GetBytes(output);
                            _output = new CStreamWriter(output);
                            OnPropertyChanged("Output");
                        }
                        break;
                    case OutputType.Number:
                        {
                            byte[] output = new byte[outputlength];
                            random.GetBytes(output);
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                    return false;
                                }
                            }
                            BigInteger[] array = new BigInteger[outputamount];
                            for (int i = 0; i < outputamount; i++)
                            {
                                byte[] output = new byte[outputlength];
                                random.GetBytes(output);
                                if (output.Length > 0)
                                {
                                    output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                                }
                                array[i] = new BigInteger(output);
                            }
                            _output = array;
                            OnPropertyChanged("Output");
                        }
                        break;
                    case OutputType.Bool:
                        {
                            byte[] output = new byte[1];
                            random.GetBytes(output);
                            _output = output[0] % 2 == 0;
                            OnPropertyChanged("Output");
                        }
                        break;

                    default:
                        throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
                }
                return true;
            }
        }

        /// <summary>
        /// Executes the X2modN random number generator
        /// </summary>
        /// <returns></returns>
        private bool ExecuteX2modNGenerator()
        {
            BigInteger seed;
            BigInteger modulus;
            int outputlength;
            try
            {
                seed = BigInteger.Parse(_settings.Seed);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return false;
            }
            try
            {
                modulus = BigInteger.Parse(_settings.Modulus);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidModulus, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }
            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        X2 x2Generator = new X2(seed, modulus, outputlength);
                        _output = x2Generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        X2 x2Generator = new X2(seed, modulus, outputlength);
                        byte[] output = x2Generator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        X2 x2Generator = new X2(seed, modulus, outputlength);
                        byte[] output = x2Generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        X2 x2Generator = new X2(seed, modulus, outputlength);
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = x2Generator.GenerateRandomByteArray();
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        X2 x2Generator = new X2(seed, modulus, 1);
                        _output = x2Generator.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
        }

        private bool ExecuteLCG()
        {
            BigInteger seed;
            BigInteger modulus;
            int outputlength;
            BigInteger a;
            BigInteger b;
            try
            {
                seed = BigInteger.Parse(_settings.Seed);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return false;
            }
            try
            {
                modulus = BigInteger.Parse(_settings.Modulus);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidModulus, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }
            try
            {
                a = BigInteger.Parse(_settings.a);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidaValue, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            try
            {
                b = BigInteger.Parse(_settings.b);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidbValue, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        LCG lcgGenerator = new LCG(seed, modulus, a, b, outputlength);
                        _output = lcgGenerator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        LCG lcgGenerator = new LCG(seed, modulus, a, b, outputlength);
                        byte[] output = lcgGenerator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        LCG lcgGenerator = new LCG(seed, modulus, a, b, outputlength);
                        byte[] output = lcgGenerator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        LCG lcgGenerator = new LCG(seed, modulus, a, b, outputlength);
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = lcgGenerator.GenerateRandomByteArray();
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        LCG lcgGenerator = new LCG(seed, modulus, a, b, outputlength);
                        _output = lcgGenerator.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
        }

        private bool ExecuteICG()
        {
            BigInteger seed;
            BigInteger modulus;
            int outputlength;
            BigInteger a;
            BigInteger b;
            try
            {
                seed = BigInteger.Parse(_settings.Seed);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return false;
            }
            try
            {
                modulus = BigInteger.Parse(_settings.Modulus);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidModulus, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }
            try
            {
                a = BigInteger.Parse(_settings.a);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidaValue, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            try
            {
                b = BigInteger.Parse(_settings.b);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidbValue, _settings.Modulus), NotificationLevel.Error);
                return false;
            }
            if (!isPrime(modulus))
            {
                GuiLogMessage(string.Format(Resources.ErrorPrime, _settings.Modulus, stdPrime), NotificationLevel.Warning);
                modulus = stdPrime;
            }
            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        ICG icgGenerator = new ICG(seed, modulus, a, b, outputlength);
                        _output = icgGenerator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        ICG icgGenerator = new ICG(seed, modulus, a, b, outputlength);
                        byte[] output = icgGenerator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        ICG icgGenerator = new ICG(seed, modulus, a, b, outputlength);
                        byte[] output = icgGenerator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        ICG icgGenerator = new ICG(seed, modulus, a, b, outputlength);
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = icgGenerator.GenerateRandomByteArray();
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        ICG icgGenerator = new ICG(seed, modulus, a, b, outputlength);
                        _output = icgGenerator.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
        }

        private bool ExecuteXpat2()
        {
            BigInteger seed;
            int outputlength;
            try
            {
                seed = BigInteger.Parse(_settings.Seed);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }

            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        SubtractiveGenerator xpat2Generator = new SubtractiveGenerator(seed, outputlength);
                        _output = xpat2Generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        SubtractiveGenerator icgGenerator = new SubtractiveGenerator(seed, outputlength);
                        byte[] output = icgGenerator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        SubtractiveGenerator xpat2Generator = new SubtractiveGenerator(seed, outputlength);
                        byte[] output = xpat2Generator.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        SubtractiveGenerator xpat2Generator = new SubtractiveGenerator(seed, outputlength);
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = xpat2Generator.GenerateRandomByteArray();
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        SubtractiveGenerator xpat2Generator = new SubtractiveGenerator(seed, outputlength);
                        _output = xpat2Generator.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
        }

        private bool ExecuteXORShift()
        {
            BigInteger seed;
            int outputlength;
            try
            {
                seed = BigInteger.Parse(_settings.Seed);
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format(Resources.InvalidSeedValue, _settings.Seed), NotificationLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(_settings.OutputLength))
            {
                outputlength = 0;
            }
            else
            {
                try
                {
                    outputlength = int.Parse(_settings.OutputLength);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format(Resources.InvalidOutputLength, _settings.Modulus), NotificationLevel.Error);
                    return false;
                }
            }

            switch (_settings.OutputType)
            {
                case OutputType.ByteArray:
                    {
                        XORShift xorShift = new XORShift(seed, outputlength, _settings.XORShiftType);
                        _output = xorShift.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.CrypToolStream:
                    {
                        XORShift icgGenerator = new XORShift(seed, outputlength, _settings.XORShiftType);
                        byte[] output = icgGenerator.GenerateRandomByteArray();
                        _output = new CStreamWriter(output);
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Number:
                    {
                        XORShift xorShift = new XORShift(seed, outputlength, _settings.XORShiftType);
                        byte[] output = xorShift.GenerateRandomByteArray();
                        OnPropertyChanged("Output");
                        _output = new CStreamWriter(output);
                        if (output.Length > 0)
                        {
                            output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
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
                                return false;
                            }
                        }
                        BigInteger[] array = new BigInteger[outputamount];
                        XORShift xorShift = new XORShift(seed, outputlength, _settings.XORShiftType);
                        for (int i = 0; i < outputamount; i++)
                        {
                            byte[] output = xorShift.GenerateRandomByteArray();
                            if (output.Length > 0)
                            {
                                output[output.Length - 1] &= 0x7F; // set sign bit 0 = positive
                            }
                            array[i] = new BigInteger(output);
                        }
                        _output = array;
                        OnPropertyChanged("Output");
                    }
                    break;
                case OutputType.Bool:
                    {
                        XORShift xorShift = new XORShift(seed, outputlength, _settings.XORShiftType);
                        _output = xorShift.GenerateRandomBit();
                        OnPropertyChanged("Output");
                    }
                    break;
                default:
                    throw new Exception(string.Format("Output type {0} not implemented", _settings.OutputType.ToString()));
            }
            return true;
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

        #region Helpermethods

        /// <summary>
        /// simple test for prime numbers
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private bool isPrime(BigInteger num)
        {
            if (num == 1)
            {
                return false;
            }

            if (num == 2)
            {
                return true;
            }

            BigInteger r = (BigInteger)Math.Sqrt((double)num);
            for (BigInteger i = 3; i <= r; i += 2)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }
            return true;
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
