using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.IO;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SATAttack.Properties;

namespace CrypTool.Plugins.SATAttack
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("SATAttack.Properties.Resources", "PluginCaption", "PluginDescription", "SATAttack/Documentation/doc.xml", new[] { "SATSolver/Images/sat-race-logo.gif" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SATAttack : ICrypComponent
    {
        #region Private Variables

        private readonly SATAttackSettings settings = new SATAttackSettings();
        private CStreamWriter outputStream;
        private CStreamWriter cbmcOutputStream;
        private CStreamWriter satSolverOutputStream;
        private StringBuilder cbmcOutputString;
        private StringBuilder satSolverOutputString;
        private Encoding encoding = Encoding.UTF8;
        private static string pluginDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            + "\\Data\\SATAttack\\";
        private string codefileName = "codefile.c";

        // CBMC output files
        private static string outputCnfFileName = "output.cnf.txt";
        private static string inputMappingFileName = "output.inputMapping.txt";
        private static string outputMappingFileName = "output.outputMapping.txt";

        private string outputCnfFilePath = pluginDataPath + outputCnfFileName;
        private string inputMappingFilePath = pluginDataPath + inputMappingFileName;
        private string outputMappingFilePath = pluginDataPath + outputMappingFileName;

        private Process cbmcProcess;
        private Process satSolverProcess;

        StringBuilder outputStringBuilder = new StringBuilder();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", false)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CbmcOutputStreamCaption", "CbmcOutputStreamTooltip", false)]
        public ICrypToolStream CbmcOutputStream
        {
            get
            {
                return cbmcOutputStream;
            }
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "SatSolverOutputStreamCaption", "SatSolverOutputStreamTooltip", false)]
        public ICrypToolStream SatSolverOutputStream
        {
            get
            {
                return satSolverOutputStream;
            }
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get
            {
                return outputStream;
            }
            set
            {
                // empty
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
            if (File.Exists(outputCnfFilePath))
                File.Delete(outputCnfFilePath);

            if (File.Exists(inputMappingFilePath))
                File.Delete(inputMappingFilePath);

            if (File.Exists(outputMappingFilePath))
                File.Delete(outputMappingFilePath);
        }

        public void PostExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 100);

            #region preparations

            /* reset output string */
            outputStringBuilder = null;
            outputStringBuilder = new StringBuilder();

            /* reset cbmc output stream */
            cbmcOutputString = null;
            cbmcOutputString = new StringBuilder();

            /* reset sat solver output stream */
            satSolverOutputString = null;
            satSolverOutputString = new StringBuilder();

            #endregion

            #region cbmc

            /* get the file which contains the C code */
            string codefilePath = GetCodefilePath();

            string cbmcMainFunctionName = settings.MainFunctionName;

            /* measure time */
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();

            stopwatch.Start();

            if (CallCbmcProcess(codefilePath, cbmcMainFunctionName) != 0)
            {
                /* write info to output stream */
                outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                GuiLogMessage(Resources.CbmcProcessError, NotificationLevel.Error);
                return;
            }

            stopwatch.Stop();
            TimeSpan cbmcTime = stopwatch.Elapsed;

            GuiLogMessage(String.Format(Resources.CbmcProcessReturnedSuccessfully, cbmcTime.ToString(Resources.TimeFormat)), NotificationLevel.Info);

            /* write info to output stream */
            outputStringBuilder.AppendFormat(Environment.NewLine + Resources.CbmcProcessReturnedSuccessfully + Environment.NewLine, cbmcTime.ToString(Resources.TimeFormat));
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* move cbmc files to the plugin's folder */
            if (File.Exists(outputCnfFileName))
                File.Move(outputCnfFileName, outputCnfFilePath);

            if (File.Exists(inputMappingFileName))
                File.Move(inputMappingFileName, inputMappingFilePath);

            if (File.Exists(outputMappingFileName))
                File.Move(outputMappingFileName, outputMappingFilePath);

            #endregion cbmc

            ProgressChanged(33, 100);

            #region cnf encoding and options

            switch (settings.AttackMode)
            {
                case 0: // preimage attack
                    {
                        #region hash encoding

                        string hashString = settings.InputHashValue;

                        /* append encoding of the output hash to the cnf */
                        if (encodeOutputBitsInCnf(hashString, outputMappingFilePath, outputCnfFilePath) != 0)
                        {
                            /* write info to output stream */
                            outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                            outputStream = new CStreamWriter();
                            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                            outputStream.Close();
                            OnPropertyChanged("OutputStream");

                            GuiLogMessage(Resources.HashEncodingError, NotificationLevel.Error);
                            return;
                        }

                        #endregion
                        break;
                    }
                case 1: // second preimage attack
                    {
                        #region hash encoding

                        string hashString = settings.InputHashValue;

                        /* append encoding of the output hash to the cnf */
                        if (encodeOutputBitsInCnf(hashString, outputMappingFilePath, outputCnfFilePath) != 0)
                        {
                            /* write info to output stream */
                            outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                            outputStream = new CStreamWriter();
                            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                            outputStream.Close();
                            OnPropertyChanged("OutputStream");

                            GuiLogMessage(Resources.HashEncodingError, NotificationLevel.Error);
                            return;
                        }

                        #endregion
                        #region second preimage encoding

                        /* append encoding of second preimage */
                        if (encodeSecondPreimageInCnf(inputMappingFilePath, outputCnfFilePath) != 0)
                        {
                            /* write info to output stream */
                            outputStringBuilder.Append(Resources.FailedString);
                            outputStream = new CStreamWriter();
                            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                            outputStream.Close();
                            OnPropertyChanged("OutputStream");

                            GuiLogMessage(Resources.SecondPreimageEncodingError, NotificationLevel.Error);
                            return;
                        }

                        #endregion
                        break;
                    }
                case 2: // key recovery attack
                    {
                        #region ciphertext encoding

                        string ciphertextString = settings.Ciphertext;

                        /* append encoding of the ciphertext to the cnf */
                        if (encodeOutputBitsInCnf(ciphertextString, outputMappingFilePath, outputCnfFilePath) != 0)
                        {
                            /* write info to output stream */
                            outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                            outputStream = new CStreamWriter();
                            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                            outputStream.Close();
                            OnPropertyChanged("OutputStream");

                            GuiLogMessage(Resources.CiphertextEncodingError, NotificationLevel.Error);
                            return;
                        }

                        #endregion
                        #region plaintext encoding

                        string plaintextString = settings.Plaintext;

                        /* append encoding of the plaintext to the cnf */
                        if (encodePlaintextBitsInCnf(plaintextString, inputMappingFilePath, outputCnfFilePath) != 0)
                        {
                            /* write info to output stream */
                            outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                            outputStream = new CStreamWriter();
                            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                            outputStream.Close();
                            OnPropertyChanged("OutputStream");

                            GuiLogMessage(Resources.PlaintextEncodingError, NotificationLevel.Error);
                            return;
                        }

                        #endregion

                        break;
                    }
                default: break;
            }

            #region fixed bits encoding

            /* append encoding of fixed bits to the cnf, if parallelized attack is selected */
            if (settings.FixBits)
            {
                if (encodeFixedBitsInCnf(inputMappingFilePath, outputCnfFilePath) != 0)
                {
                    /* write info to output stream */
                    outputStringBuilder.Append(Resources.FailedString + Environment.NewLine);
                    outputStream = new CStreamWriter();
                    outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                    outputStream.Close();
                    OnPropertyChanged("OutputStream");

                    GuiLogMessage(Resources.FixedBitsEncodingError, NotificationLevel.Error);
                    return;
                }
            }

            #endregion

            #region copy cnf

            if (settings.CnfFileName != "")
            {
                /* write info to output stream */
                outputStringBuilder.AppendFormat(Resources.CopyingCnfString, settings.CnfFileName);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                File.Copy(outputCnfFilePath, settings.CnfFileName);

                /* write info to output stream */
                outputStringBuilder.Append(Resources.SuccessfulString + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");
            }

            #endregion

            #region cnf output only

            if (settings.OnlyCnfOutput)
            {
                /* write info to output stream */
                outputStringBuilder.Append(Resources.SkippingSatSolverString + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                ProgressChanged(1, 1);
                return;
            }

            #endregion

            #endregion

            ProgressChanged(66, 100);

            #region sat solver

            #region call sat solver
            /* call SAT solver and pass cnf file to it */
            string satSolverOutputFilename = "solver.output.txt";
            string satSolverOutputFilePath = pluginDataPath + satSolverOutputFilename;

            stopwatch.Reset();
            TimeSpan satSolverTime;

            if (callSatSolver(outputCnfFilePath, satSolverOutputFilePath) != 10) // 10: sat, 20: unsat
            {
                stopwatch.Stop();
                satSolverTime = stopwatch.Elapsed;

                //GuiLogMessage(String.Format("SatProcessReturnedUnsat", satSolverTime.ToString(Resources.TimeFormat)), NotificationLevel.Info);

                /* write info to output stream */
                outputStringBuilder.AppendFormat(Resources.SatProcessReturnedUnsat, satSolverTime.ToString(Resources.TimeFormat) + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                return;
            }

            stopwatch.Stop();
            satSolverTime = stopwatch.Elapsed;

            //GuiLogMessage(String.Format("SatProcessReturnedSuccessful", satSolverTime.ToString(Resources.TimeFormat)), NotificationLevel.Info);

            /* write info to output stream */
            outputStringBuilder.AppendFormat(Resources.SatProcessReturnedSuccessful + Environment.NewLine, satSolverTime.ToString(Resources.TimeFormat));
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");
            #endregion

            #region interpret sat solver output

            #region number of plaintext variables
            /* if key recovery attack is chosen, determine amount of non key variables such that processSatSolverOutput interprets the number of input variables */
            int numberOfPlaintextVariables = 0;
            if (settings.AttackMode == 2)
            {
                int sizeOfInputVariables = getSizeOfInputVariables(inputMappingFilePath);
                if (sizeOfInputVariables == -1)
                {
                    GuiLogMessage(Resources.InputVariablesSizeFailureString, NotificationLevel.Error);
                    return;
                }

                int sizeOfPlaintextInBit = settings.Plaintext.Substring(0, 2) == "0x" ? (settings.Plaintext.Length - 2) * 4 :
                    settings.Plaintext.Substring(0, 2) == "0b" ? (settings.Plaintext.Length - 2) :
                    -1;

                if (sizeOfPlaintextInBit == -1)
                {
                    GuiLogMessage(Resources.PlaintextPrefixInfoString, NotificationLevel.Error);
                    return;
                }

                numberOfPlaintextVariables = sizeOfPlaintextInBit / sizeOfInputVariables;
            }
            #endregion

            string outputString = processSatSolverOutput(satSolverOutputFilePath, inputMappingFilePath, numberOfPlaintextVariables);

            if (outputString == null)
            {
                GuiLogMessage(Resources.SatOutputProcessError, NotificationLevel.Error);
                return;
            }

            outputStringBuilder.Append(outputString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");
            #endregion

            #endregion

            ProgressChanged(100, 100);
        }

        public void Stop()
        {
            try
            {
                cbmcProcess.Kill();
                GuiLogMessage(Resources.CbmcProcessKillSuccess, NotificationLevel.Debug);
            }
            catch (Exception e)
            {
                GuiLogMessage(String.Format(Resources.CbmcProcessKillException, Environment.NewLine, e), NotificationLevel.Debug);
            }

            try
            {
                satSolverProcess.Kill();
                GuiLogMessage(Resources.SatProcessKillSuccess, NotificationLevel.Debug);
            }
            catch (Exception e)
            {
                GuiLogMessage(String.Format(Resources.SatProcessKillException, Environment.NewLine, e), NotificationLevel.Debug);
            }
        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
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

        #region Custom Methods

        /// <summary>
        /// This event is fired when the CBMC process writes data to standard output.
        /// </summary>
        void CbmcProcess_OutputDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                cbmcOutputString.AppendLine(data.Data.ToString());

                cbmcOutputStream = new CStreamWriter();
                cbmcOutputStream.Write(encoding.GetBytes(cbmcOutputString.ToString()));
                cbmcOutputStream.Close();
                OnPropertyChanged("CbmcOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the CBMC process writes data to standard error.
        /// </summary>
        void CbmcProcess_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                cbmcOutputString.AppendLine(data.Data.ToString());

                cbmcOutputStream = new CStreamWriter();
                cbmcOutputStream.Write(encoding.GetBytes(cbmcOutputString.ToString()));
                cbmcOutputStream.Close();
                OnPropertyChanged("CbmcOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the Sat solver process writes data to standard output.
        /// </summary>
        void SatSolverProcess_OutputDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                satSolverOutputString.AppendLine(data.Data.ToString());

                satSolverOutputStream = new CStreamWriter();
                satSolverOutputStream.Write(encoding.GetBytes(satSolverOutputString.ToString()));
                satSolverOutputStream.Close();
                OnPropertyChanged("SatSolverOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the Sat solver process writes data to standard error.
        /// </summary>
        void SatSolverProcess_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                satSolverOutputString.AppendLine(data.Data.ToString());

                satSolverOutputStream = new CStreamWriter();
                satSolverOutputStream.Write(encoding.GetBytes(satSolverOutputString.ToString()));
                satSolverOutputStream.Close();
                OnPropertyChanged("SatSolverOutputStream");
            }
        }

        /// <summary>
        /// Writes a stream of type ICrypToolStream to a file. The parameter filepath should end with "\\".
        /// </summary>
        void ReadInputStreamToCodefile(ICrypToolStream inputStream, string pluginDataPath, string codefileName)
        {
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[1024];

                /* create directory if not existent */
                bool dirExists = Directory.Exists(pluginDataPath);

                if (!dirExists)
                    Directory.CreateDirectory(pluginDataPath);

                FileStream fs = File.Open(pluginDataPath + codefileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    bw.Write(buffer, 0, bytesRead);
                }

                fs.Close();
                bw.Close();
            }
        }

        /// <summary>
        /// Get path and filename of the C code input file.
        /// </summary>
        string GetCodefilePath()
        {
            string inputFilename = "";

            /* either read from InputStream or get file location from settings.InputFile */
            int inputSelection = settings.InputSelection;

            if (inputSelection == 0) // via InputStream
            {
                ReadInputStreamToCodefile(InputStream, pluginDataPath, codefileName);

                inputFilename = pluginDataPath + codefileName;
            }
            else if (inputSelection == 1) // via settings.InputFile
            {
                inputFilename = settings.InputFile;
            }

            return inputFilename;
        }

        /// <summary>
        /// Calls the CBMC process. When successful, a file output.cnf.txt is created which contains the cnf representation
        /// of the code in codefile.c.
        /// </summary>
        /// <param name="codefilePath">Full path to the codefile, including the name of the codefile.</param>
        /// <returns>
        /// 0 if successful or 1 the input codefile could not be found
        /// </returns>
        int CallCbmcProcess(string codefilePath, string mainFunctionName)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.CallingCbmcProcessString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            if (!File.Exists(codefilePath))
            {
                GuiLogMessage(String.Format(Resources.CodefileNotFoundString, codefilePath), NotificationLevel.Error);
                return 1;
            }

            string cbmcExeFileName = "cbmc.exe";
            string cbmcExeFilePath = pluginDataPath + cbmcExeFileName;

            if (!File.Exists(cbmcExeFilePath))
            {
                GuiLogMessage(String.Format(Resources.CbmcExeNotFound, cbmcExeFilePath), NotificationLevel.Error);
                return 1;
            }

            /* build args, path is changed from MSDOS format to cygwin posix format to supress the MSDOS path format warning */
            string cbmcProcessArgs = "\"" + "/cygdrive/" + codefilePath.Replace("\\", "/").Replace(":", "") + "\"";
            if (mainFunctionName != "" && mainFunctionName != null)
                cbmcProcessArgs += " --function " + mainFunctionName;

            //GuiLogMessage(String.Format("cbmc.exe path is {0}",
            //    (pluginDataPath + cbmcExecutableFilename)), NotificationLevel.Info);

            cbmcProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cbmcExeFilePath,
                    CreateNoWindow = true,
                    Arguments = cbmcProcessArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            cbmcProcess.OutputDataReceived += new DataReceivedEventHandler(CbmcProcess_OutputDataReceived);
            cbmcProcess.ErrorDataReceived += new DataReceivedEventHandler(CbmcProcess_ErrorDataReceived);

            cbmcProcess.Start();
            cbmcProcess.BeginOutputReadLine();
            cbmcProcess.BeginErrorReadLine();
            cbmcProcess.WaitForExit();

            GuiLogMessage(String.Format(String.Format(Resources.CbmcProcessReturnedWithExitcode,
                cbmcProcess.ExitCode)), NotificationLevel.Debug);

            //TODO: review cbmc process exitcodes
            return cbmcProcess.ExitCode;
        }

        /// <summary>
        /// Converts a hex string into a bit array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        BitArray HexStringToBitArray(string hex)
        {
            BitArray bitArray;

            if (!IsHexString(hex)) // check if all characters are hex
            {
                GuiLogMessage(Resources.HexStringError, NotificationLevel.Error);
                return null;
            }
            else if (hex.Length % 2 != 0) // hash value must be an even amount of hex values
            {
                GuiLogMessage(Resources.HexStringError, NotificationLevel.Error);
                return null;
            }

            byte[] byteArray = HexStringToByteArray(hex);
            bitArray = new BitArray(byteArray);

            return bitArray;
        }

        /// <summary>
        /// Converts a bit string into a bit array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        BitArray BitStringToBitArray(string bitstring)
        {
            BitArray bitArray;

            if (!IsBitString(bitstring)) // check if all characters are binary
            {
                GuiLogMessage(Resources.BinaryStringError, NotificationLevel.Error);
                return null;
            }

            int numberOfBits = bitstring.Length;

            /* reverse bitstring to obtain the correct ordering (original ordering is msb to lsb, we need lsb to msb) */
            char[] bits = bitstring.ToCharArray();
            Array.Reverse(bits);
            string reversedBitstring = new string(bits);

            Boolean[] hashBools = new Boolean[numberOfBits];

            for (int i = 0; i < numberOfBits; i++)
            {
                if (reversedBitstring.ElementAt(i).Equals('0'))
                    hashBools[i] = false;
                else if (reversedBitstring.ElementAt(i).Equals('1'))
                    hashBools[i] = true;
            }

            bitArray = new BitArray(hashBools);

            return bitArray;
        }

        /// <summary>
        /// Converts a BitArray into a hex string. The first bit in the array should be the least significant bit and the last bit in the array should be the most significant bit.
        /// </summary>
        string BitArrayToHexString(BitArray ba)
        {
            byte[] data = new byte[ba.Length / 8];
            ba.CopyTo(data, 0);

            // reverse to obtain the correct bit ordering
            Array.Reverse(data);

            string hexString = BitConverter.ToString(data);

            /* replace all dashes in hexString (0xab-cd-ef -> 0xabcdef) */
            string output = hexString.Replace("-", "");

            return output;
        }

        /// <summary>
        /// Converts a hex string into a byte array.
        /// </summary>
        byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                // begin with the last two nibbles and iterate from right to left to respect byte ordering
                bytes[(numberChars / 2) - 1 - i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        string BinaryStringToHexString(string binary)
        {
            string true_binary = binary.Replace('x', '0');      // if binary contains the wildcard character 'x' replace them with '0' by default

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        /// <summary>
        /// Check if a string only contains hex characters.
        /// </summary>
        bool IsHexString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        /// <summary>
        /// Check if a string only contains bit characters (0 or 1).
        /// </summary>
        bool IsBitString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-1]+\b\Z");
        }

        /// <summary>
        /// Convert byte to bits
        /// </summary>
        IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        /// <summary>
        /// Returns an int array which contains the labels of literals (from lsb to msb).
        /// </summary>
        int[][] getMapping(string mappingFilePath)
        {
            bool mappingFileExists = File.Exists(mappingFilePath);

            if (!mappingFileExists)
            {
                GuiLogMessage(String.Format(Resources.MappingFileNotFoundString, mappingFilePath), NotificationLevel.Error);
                return null;
            }

            /* read mapping file  */
            string mappingFileContent = readFileToString(mappingFilePath);
            string[] mappingFileLines = mappingFileContent.Split(Environment.NewLine.ToCharArray());

            string[] infoLine = mappingFileLines[0].Split(' ');        // format: <type> <number of variables> <size of variables>

            /* get type of mapping (INPUT or OUTPUT) */
            string variableType = infoLine[0];

            int numberOfVariables;
            if (int.TryParse(infoLine[1], out numberOfVariables) == false)
            {
                GuiLogMessage(String.Format(Resources.VariableNumberParsingFailedString, variableType), NotificationLevel.Error);
            }

            int sizeOfVariables;
            if (int.TryParse(infoLine[2], out sizeOfVariables) == false)
            {
                GuiLogMessage(String.Format(Resources.VariableSizeParsingFailedString, variableType), NotificationLevel.Error);
            }

            /* build mapping array */
            int[][] mapping = new int[numberOfVariables][];

            string variableNumberAsString;
            int variableNumber;
            string bitNumberAsString;
            int bitNumber;
            int positionOfUnderscore;
            int positionOfColon;
            int literalValue;
            string[] tmp;
            string variableString;
            string literalString;

            foreach (string line in mappingFileLines)
            {
                tmp = null;
                tmp = line.Split(' ');

                if (tmp.Length == 2) // skip first (info) line
                {
                    variableString = tmp[0];
                    literalString = tmp[1];

                    positionOfUnderscore = variableString.IndexOf("_");
                    positionOfColon = variableString.IndexOf(":");
                    variableNumberAsString = variableString.Substring(positionOfUnderscore + 1, positionOfColon - (positionOfUnderscore + 1));
                    bitNumberAsString = variableString.Substring(positionOfColon + 1);

                    if (int.TryParse(variableNumberAsString, out variableNumber))
                    {
                        if (int.TryParse(bitNumberAsString, out bitNumber))
                        {
                            if (int.TryParse(literalString, out literalValue))
                            {
                                if (variableNumber < mapping.Length)
                                {
                                    if (mapping[variableNumber] == null)
                                        mapping[variableNumber] = new int[sizeOfVariables];

                                    mapping[variableNumber][bitNumber] = literalValue;
                                }
                                else
                                {
                                    GuiLogMessage(String.Format(Resources.VariableDefinitionsError, variableType.ToLower(), variableType), NotificationLevel.Error);
                                    return null;
                                }
                            }
                            else
                            {
                                GuiLogMessage(String.Format(Resources.LiteralParseError, variableType.ToLower(), variableString, literalString), NotificationLevel.Error);
                                return null;
                            }
                        }
                        else
                        {
                            GuiLogMessage(String.Format(Resources.BitNumberParseError, variableType.ToLower(), variableString, bitNumberAsString), NotificationLevel.Error);
                            return null;
                        }
                    }
                    else
                    {
                        GuiLogMessage(String.Format(Resources.VariableIndexParseError, variableType.ToLower(), variableString, variableNumberAsString), NotificationLevel.Error);
                        return null;
                    }
                }
            }

            /* print warning for unassigned variables */
            for (int i = 0; i < mapping.Length; i++)
            {
                for (int j = 0; j < mapping[i].Length; j++)
                {
                    if (mapping[i][j] == 0)
                        GuiLogMessage(String.Format(Resources.UnassignedVariableWarning, variableType, i, j, variableType.ToLower()), NotificationLevel.Warning);
                }
            }

            return mapping;
        }

        /// <summary>
        /// Reads the output mapping file generated by cbmc and reads the output value provided by the user in the plugin parameters.
        /// Opens the cnf file generated by cbmc and appends the encoding of the output value.
        /// </summary>
        int encodeOutputBitsInCnf(string outputString, string outputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.EncodingOutputBitsString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* get the output literals */
            int[][] outputMapping;
            if ((outputMapping = getMapping(outputMappingFilePath)) == null)
            {
                GuiLogMessage(Resources.OutputMappingError, NotificationLevel.Error);
                return 1;
            }

            /* parse output string which can be either a binary or hexadecimal value */
            string outputStringFormat = outputString.Substring(0, 2);
            string outputStringValue = outputString.Substring(2);

            BitArray outputBits;

            if (outputStringFormat == "0x")
            {
                /* get the output string as bit array (ranging from lsb to msb) */
                if ((outputBits = HexStringToBitArray(outputStringValue)) == null)
                {
                    GuiLogMessage(Resources.OutputBitsError, NotificationLevel.Error);
                    return 1;
                }
            }
            else if (outputStringFormat == "0b")
            {
                /* get the output string as bit array (ranging from lsb to msb) */
                if ((outputBits = BitStringToBitArray(outputStringValue)) == null)
                {
                    GuiLogMessage(Resources.OutputBitsError, NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage(Resources.PrefixError, NotificationLevel.Error);
                return 1;
            }

            /* ensure the size of output bits matches the size of hash value / ciphertext */
            int outputMappingLength = 0;
            for (int i = 0; i < outputMapping.Length; i++)
            {
                outputMappingLength += outputMapping[i].Length;
            }

            if (outputBits.Length != outputMappingLength)
            {
                GuiLogMessage(String.Format(Resources.OutputSizeError, Environment.NewLine, outputBits.Length, outputMappingLength), NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode the output bits */
            StringBuilder outputBitEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; i < outputMapping.Length; i++)
            {
                for (int j = 0; j < outputMapping[i].Length; j++)
                {
                    if (outputBits[offset + j])    // 1
                        sign = "";
                    else                    // 0
                        sign = "-";

                    outputBitEncoding.Append(String.Format("{0}{1} 0" + Environment.NewLine, sign, outputMapping[i][j])); // clause line
                }

                offset += outputMapping[i].Length;  // maintain the correct offset in the one dimensional array hashBits
            }

            /* append hash encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format(Resources.CnfFileNotFoundError, outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(outputBitEncoding.ToString()), 0, outputBitEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append(Resources.SuccessfulString + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        /// <summary>
        /// Reads the input mapping file generated by cbmc and reads the input value provided by the user in the plugin parameters.
        /// Opens the cnf file generated by cbmc and appends the encoding of the intput value (currently only used to encode the plaintext in a key recovery attack).
        /// </summary>
        int encodePlaintextBitsInCnf(string inputString, string inputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.EncodingInputBitsString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* get the output literals */
            int[][] inputMapping;
            if ((inputMapping = getMapping(inputMappingFilePath)) == null)
            {
                GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                return 1;
            }

            /* parse plaintext string which can be either a binary or hexadecimal value */
            string inputStringFormat = inputString.Substring(0, 2);
            string inputStringValue = inputString.Substring(2);

            BitArray plaintextBits;

            if (inputStringFormat == "0x")
            {
                /* get the plaintext string as bit array (ranging from lsb to msb) */
                if ((plaintextBits = HexStringToBitArray(inputStringValue)) == null)
                {
                    GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                    return 1;
                }
            }
            else if (inputStringFormat == "0b")
            {
                /* get the plaintext string as bit array (ranging from lsb to msb) */
                if ((plaintextBits = BitStringToBitArray(inputStringValue)) == null)
                {
                    GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage(Resources.PrefixError, NotificationLevel.Error);
                return 1;
            }

            /* ensure the size of input bits is larger than the provided plaintext */
            int inputMappingLength = 0;
            for (int i = 0; i < inputMapping.Length; i++)
            {
                inputMappingLength += inputMapping[i].Length;
            }

            if (plaintextBits.Length > inputMappingLength)
            {
                GuiLogMessage(String.Format(Resources.PlaintextSizeError, Environment.NewLine, plaintextBits.Length, inputMappingLength), NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode the input bits */
            StringBuilder inputBitEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; i < inputMapping.Length; i++)
            {
                for (int j = 0; j < inputMapping[i].Length && (offset + j) < plaintextBits.Length; j++)
                {
                    if (plaintextBits[offset + j])    // 1
                        sign = "";
                    else                    // 0
                        sign = "-";

                    inputBitEncoding.Append(String.Format("{0}{1} 0" + Environment.NewLine, sign, inputMapping[i][j])); // clause line
                }

                offset += inputMapping[i].Length;  // maintain the correct offset in the one dimensional array inputBits
            }

            /* append hash encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format(Resources.CnfFileNotFoundError, outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(inputBitEncoding.ToString()), 0, inputBitEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append(Resources.SuccessfulString + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        /// <summary>
        /// Calls the SAT solver Cryptominisat and passes the CNF file at `cnfFilePath`. The output of the SAT solver (literal assignment) is written to `satSolverOutputFilepath`.
        /// </summary>
        /// <param name="cnfFilePath">Path to the CNF file</param>
        /// <param name="satSolverOutputFilePath">Path where the SAT solver output file is written to (including filename)</param>
        /// <returns></returns>
        int callSatSolver(string cnfFilePath, string satSolverOutputFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.CallingSatSolverString + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            if (!File.Exists(cnfFilePath))
            {
                GuiLogMessage(String.Format(Resources.CnfFileNotFoundError, cnfFilePath), NotificationLevel.Error);
                return 1;
            }

            string satSolverFileName = "cryptominisat32.exe";
            string satSolverFilePath = pluginDataPath + satSolverFileName;

            if (!File.Exists(satSolverFilePath))
            {
                GuiLogMessage(String.Format(Resources.satSolverExeNotFound, satSolverFilePath), NotificationLevel.Error);
                return 1;
            }

            /* build args */
            string satSolverProcessArgs = "\"" + cnfFilePath + "\" \"" + satSolverOutputFilePath + "\"";

            satSolverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = satSolverFilePath,
                    CreateNoWindow = true,
                    Arguments = satSolverProcessArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            GuiLogMessage(String.Format(String.Format(Resources.CallingSatSolverWithParametersString,
                pluginDataPath + satSolverFileName, satSolverProcessArgs)), NotificationLevel.Debug);

            satSolverProcess.OutputDataReceived += new DataReceivedEventHandler(SatSolverProcess_OutputDataReceived);
            satSolverProcess.ErrorDataReceived += new DataReceivedEventHandler(SatSolverProcess_ErrorDataReceived);

            satSolverProcess.Start();
            satSolverProcess.BeginOutputReadLine();
            satSolverProcess.BeginErrorReadLine();
            satSolverProcess.WaitForExit();

            GuiLogMessage(String.Format(String.Format(Resources.SatSolverReturnedWithExitcode,
                satSolverProcess.ExitCode)), NotificationLevel.Debug);

            //TODO: review cmsat exitcodes
            return satSolverProcess.ExitCode;
        }

        int getSizeOfInputVariables(string inputMappingFilePath)
        {
            bool mappingFileExists = File.Exists(inputMappingFilePath);

            if (!mappingFileExists)
            {
                GuiLogMessage(String.Format(Resources.MappingFileNotFoundString, inputMappingFilePath), NotificationLevel.Error);
                return -1;
            }

            /* read mapping file  */
            string mappingFileContent = readFileToString(inputMappingFilePath);
            string[] mappingFileLines = mappingFileContent.Split(Environment.NewLine.ToCharArray());

            string[] infoLine = mappingFileLines[0].Split(' ');        // format: <type> <number of variables> <size of variables>

            /* get type of mapping (INPUT or OUTPUT) */
            string variableType = infoLine[0];

            int sizeOfVariables = -1;
            if (int.TryParse(infoLine[2], out sizeOfVariables) == false)
            {
                GuiLogMessage(String.Format(Resources.VariableSizeParsingFailedString, variableType), NotificationLevel.Error);
            }

            return sizeOfVariables;
        }

        /// <summary>
        /// Interprets the SAT solver's output literal assignment to define the bits of the input variables.
        /// </summary>
        /// <param name="satSolverOutputFilePath">Path to the file which contains the literal assignments obtained by the SAT solver</param>
        /// <param name="inputMappingFilePath">Path to the input mapping file obtained by CBMC which maps the input variables to CNF literals</param>
        /// <returns>A string which describes the assignment of the input variables in binary and hexadecimal form</returns>
        string processSatSolverOutput(string satSolverOutputFilePath, string inputMappingFilePath, int numberOfPlaintextVariables = 0)
        {
            bool satSolverOutputFileExists = File.Exists(satSolverOutputFilePath);

            if (!satSolverOutputFileExists)
            {
                GuiLogMessage(String.Format(Resources.SatSolverOutputFileError, satSolverOutputFilePath), NotificationLevel.Error);
                return null;
            }

            /* read sat solver output file */
            string satSolverOutputFileContent = readFileToString(satSolverOutputFilePath);

            string[] lines = satSolverOutputFileContent.Split(Environment.NewLine.ToCharArray());

            int[][] inputBits;

            if (lines[0] == "SAT")
            {
                int[][] inputMapping = getMapping(inputMappingFilePath);
                if (inputMapping == null)
                {
                    GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                    return null;
                }

                string[] inputLiterals = lines[1].Split(' ');

                /* initialize inputBits array with the value -1 */
                inputBits = new int[inputMapping.Length - numberOfPlaintextVariables][];

                for (int i = 0; i < inputBits.Length; i++)
                {
                    inputBits[i] = new int[inputMapping[i].Length];

                    for (int j = 0; j < inputBits[i].Length; j++)
                    {
                        inputBits[i][j] = -1;
                    }
                }

                int signedLiteralValue;
                int literalValue;
                int sign;

                foreach (string literal in inputLiterals)
                {
                    if (int.TryParse(literal, System.Globalization.NumberStyles.AllowLeadingSign, null, out signedLiteralValue))
                    {
                        if (signedLiteralValue == 0)     // skip last zero which occurs at the end of the sat solver variable assignments
                            continue;

                        if (signedLiteralValue < 0)      // literal looks like "-x"
                        {
                            sign = 0;
                            literalValue = Math.Abs(signedLiteralValue);  // get absolute value without sign
                        }
                        else                            // literal looks like "x"
                        {
                            sign = 1;
                            literalValue = signedLiteralValue;
                        }

                        int variableNumber = -1;
                        int bitPosition = -1;

                        /* get the variable number and bit position to which the current literal refers (e.g. variable number = 1 and bit position = 7 for "INPUT_1:7") */
                        for (int i = numberOfPlaintextVariables; i < inputMapping.Length; i++)
                        {
                            bitPosition = Array.IndexOf(inputMapping[i], literalValue);

                            if (bitPosition != -1)
                            {
                                variableNumber = i - numberOfPlaintextVariables;

                                /* assign the sign to the related input bit if it occurs in the input mapping array (literals 1 and 2 will not appear there, since they are given out by cbmc to encode the constant zero and one gates) */
                                inputBits[variableNumber][bitPosition] = sign;

                                break;
                            }
                        }
                    }
                    else
                    {
                        GuiLogMessage(String.Format(Resources.LiteralParseError2, literal), NotificationLevel.Error);
                        return null;
                    }
                }
            }
            else
            {
                GuiLogMessage(String.Format(Resources.SatSolverOutputFirstLineError, satSolverOutputFilePath), NotificationLevel.Error);
                return null;
            }

            StringBuilder messageBitsString = new StringBuilder();

            //int separatorCounter = 0;

            /* by using insert instead of append, the string shows the lsb on the right and msb on the left */
            for (int i = 0; i < inputBits.Length; i++)
            {
                for (int j = 0; j < inputBits[i].Length; j++)
                {
                    if (inputBits[i][j] == 1)
                        messageBitsString.Insert(0, "1");
                    else if (inputBits[i][j] == 0)
                        messageBitsString.Insert(0, "0");
                    else if (inputBits[i][j] == -1)
                        messageBitsString.Insert(0, "x");   // this means the related bit can be either 0 or 1, i.e. its value does not matter since it does not affect the output hash value
                }
            }

            string inputBitsBinaryString = messageBitsString.ToString();
            string inputBitsHexString = "";
            string inputHexCaption = "";

            if (inputBitsBinaryString.Length % 8 == 0)
            {
                inputBitsHexString = BinaryStringToHexString(inputBitsBinaryString) + Environment.NewLine + Environment.NewLine;
                inputHexCaption = Resources.HexadecimalString;
            }

            string inputFoundString;

            if (settings.AttackMode == 0)
                inputFoundString = Resources.InputFoundStringPreimage;
            else if (settings.AttackMode == 1)
                inputFoundString = Resources.InputFoundStringSecondPreimage;
            else if (settings.AttackMode == 2)
                inputFoundString = Resources.InputFoundStringKeyRecovery;
            else
                inputFoundString = Resources.InputFoundString;

            string outputString = "-----------------------------------------" + Environment.NewLine
                + inputFoundString + Environment.NewLine + Environment.NewLine
                + inputHexCaption + inputBitsHexString
                + Resources.BinaryString + inputBitsBinaryString;

            return outputString;
        }

        /// <summary>
        /// Reads a file and returns its content in a string object.
        /// </summary>
        /// <param name="filepath">Path to the file to be read</param>
        /// <returns>String which contains the content of the file</returns>
        string readFileToString(string filepath)
        {
            using (FileStream fs = File.Open(filepath, FileMode.Open))
            {
                StringBuilder sb = new StringBuilder();

                if (fs.Length > 65536) // stop if the mapping file is too big (arbitrary big number) (paranoid)
                {
                    GuiLogMessage(String.Format(Resources.FileBigWarning, filepath, fs.Length), NotificationLevel.Warning);
                }

                byte[] buffer = new byte[fs.Length];

                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {
                    sb.Append(encoding.GetString(buffer));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts a bit string into a char array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        char[] BitStringToFixedBitsCharArray(string fixedBitsString)
        {
            /* reverse bitstring to obtain the correct ordering (original ordering is msb to lsb, we need lsb to msb) */
            char[] reversedFixedBits = fixedBitsString.ToCharArray();
            Array.Reverse(reversedFixedBits);

            return reversedFixedBits;
        }

        /// <summary>
        /// Converts a hex string into a char array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        char[] HexStringToFixedBitsCharArray(string fixedBitsString)
        {
            StringBuilder fixedBitsStringBuilder = new StringBuilder();
            string nibble2bin;
            
            foreach (char nibble in fixedBitsString)
            {
                if (nibble == '*')
                {
                    fixedBitsStringBuilder.Append("****");
                }
                else if (IsHexString(nibble.ToString()))
                {
                    nibble2bin = Convert.ToString(Convert.ToInt32(nibble.ToString(), 16), 2);                    
                    fixedBitsStringBuilder.Append(nibble2bin.PadLeft(4, '0'));
                }
                else
                {
                    GuiLogMessage(String.Format(Resources.NotAHexCharError, nibble), NotificationLevel.Error);
                    return null;
                }
            }

            char[] reversedFixedBits = fixedBitsStringBuilder.ToString().ToCharArray();
            Array.Reverse(reversedFixedBits);

            return reversedFixedBits;
        }

        int encodeFixedBitsInCnf(string inputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.FixedBitsEncoding);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            if (settings.FixedBits.Length <= 2)
            {
                GuiLogMessage(Resources.FixedBitsWarning, NotificationLevel.Warning);
                return 0;
            }

            string fixedBitsStringFormat = settings.FixedBits.Substring(0, 2);
            string fixedBitsString = settings.FixedBits.Substring(2);

            char[] fixedBits;

            if (fixedBitsStringFormat == "0x")
            {
                /* get the fixed bits string as bit array (ranging from lsb to msb) */
                if ((fixedBits = HexStringToFixedBitsCharArray(fixedBitsString)) == null)
                {
                    GuiLogMessage(Resources.FixedBitsError, NotificationLevel.Error);
                    return 1;
                }
            }
            else if (fixedBitsStringFormat == "0b")
            {
                /* get the fixed bits string as bit array (ranging from lsb to msb) */
                if ((fixedBits = BitStringToFixedBitsCharArray(fixedBitsString)) == null)
                {
                    GuiLogMessage(Resources.FixedBitsError, NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage(Resources.PrefixError, NotificationLevel.Error);
                return 1;
            }

            /* check if fixed bits string only contains 0, 1 or * (the '*' means "don't fix this bit", e.g. "0**1" means set lsb to false and msb to true, the other bits are not fixed) */
            foreach (char c in fixedBits)
            {
                if (!c.Equals('0') && !c.Equals('1') && !c.Equals('*'))
                {
                    GuiLogMessage(String.Format(Resources.FixedBitsEncodingHint, c), NotificationLevel.Error);
                    return 1;
                }
            }

            /* get input mapping */
            int[][] inputMapping = getMapping(inputMappingFilePath);
            if (inputMapping == null)
            {
                GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                return 1;
            }

            /* ensure fixed bits are less or equal to the amoung of input bits */
            int inputMappingLength = 0;
            for (int i = 0; i < inputMapping.Length; i++)
            {
                inputMappingLength += inputMapping[i].Length;
            }

            if (!(fixedBits.Length <= inputMappingLength))
            {
                GuiLogMessage(String.Format(Resources.FixedBitsSizeError, Environment.NewLine, fixedBits.Length, inputMappingLength), NotificationLevel.Warning);
            }

            /* build the clauses that encode the fixed bits */
            StringBuilder fixedBitsEncoding = new StringBuilder();

            string sign = "";
            int offset = 0;

            for (int i = 0; (i < inputMapping.Length) && (offset < fixedBits.Length); i++)
            {
                for (int j = 0; (j < inputMapping[i].Length) && (offset + j < fixedBits.Length); j++)
                {
                    if (fixedBits[offset + j].Equals('0'))
                        sign = "-";
                    else if (fixedBits[offset + j].Equals('1'))
                        sign = "";
                    else if (fixedBits[offset + j].Equals('*'))
                        continue;

                    fixedBitsEncoding.Append(String.Format("{0}{1} 0" + Environment.NewLine, sign, inputMapping[i][j])); // clause line
                }

                offset += inputMapping[i].Length; // maintain the correct offset in the one dimensional array fixedBits
            }

            /* append fixed bits encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format(Resources.CnfFileNotFoundError, outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(fixedBitsEncoding.ToString()), 0, fixedBitsEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append(Resources.SuccessfulString + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        int encodeSecondPreimageInCnf(string inputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append(Resources.SecondPreimageEncodingString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* get input mapping */
            int[][] inputMapping = getMapping(inputMappingFilePath);
            if (inputMapping == null)
            {
                GuiLogMessage(Resources.InputMappingError, NotificationLevel.Error);
                return 1;
            }

            /* parse second preimage value which can be either a binary or hexadecimal value */
            string secondPreimageString = settings.SecondPreimage;
            string secondPreimageFormat = secondPreimageString.Substring(0, 2);
            string secondPreimageValue = secondPreimageString.Substring(2);

            BitArray secondPreimageBits;

            if (secondPreimageFormat == "0x")
            {
                /* get the second preimage value as bit array (ranging from lsb to msb) */
                if ((secondPreimageBits = HexStringToBitArray(secondPreimageValue)) == null)
                {
                    GuiLogMessage(Resources.SecondPreimageRetrievingError, NotificationLevel.Error);
                    return 1;
                }
            }
            else if (secondPreimageFormat == "0b")
            {
                /* get the second preimage value as bit array (ranging from lsb to msb) */
                if ((secondPreimageBits = BitStringToBitArray(secondPreimageValue)) == null)
                {
                    GuiLogMessage(Resources.SecondPreimageRetrievingError, NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage(Resources.PrefixError, NotificationLevel.Error);
                return 1;
            }

            /* ensure second preimage and output bits have the same size */
            int inputMappingLength = 0;
            for (int i = 0; i < inputMapping.Length; i++)
            {
                inputMappingLength += inputMapping[i].Length;
            }

            if (secondPreimageBits.Length != inputMappingLength)
            {
                GuiLogMessage(String.Format(Resources.SecondPreimageSizeError, Environment.NewLine, secondPreimageBits.Length, inputMappingLength), NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode the second preimage */
            StringBuilder secondPreimageEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; i < inputMapping.Length; i++)
            {
                for (int j = 0; j < inputMapping[i].Length; j++)
                {
                    if (secondPreimageBits[offset + j])  // 1
                        sign = "-";                     // invert literal
                    else                            // 0
                        sign = "";                      // invert literal

                    secondPreimageEncoding.Append(String.Format("{0}{1} ", sign, inputMapping[i][j])); // clause
                }

                offset += inputMapping[i].Length; // maintain the correct offset in the one dimensional array secondPreimageBits
            }

            secondPreimageEncoding.Append("0" + Environment.NewLine); // append clause terminating zero

            /* append second preimage encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format(Resources.CnfFileNotFoundError, outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(secondPreimageEncoding.ToString()), 0, secondPreimageEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append(Resources.SuccessfulString + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        #endregion
    }
}
