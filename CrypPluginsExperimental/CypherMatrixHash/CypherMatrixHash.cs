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
using System.Numerics;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.CypherMatrixHash
{
    [Author("Michael Schäfer", "michael.schaefer@rub.de", null, null)]
    [PluginInfo("CypherMatrixHash.Properties.Resources", "PluginCaption", "PluginTooltip", "CypherMatrixHash/doc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class CypherMatrixHash : ICrypComponent
    {
        #region Private variables and public constructor

        private readonly CypherMatrixHashSettings settings;         //Nutzereinstellungen
        private BigInteger outputHash;                              //Ausgabedaten
        private CStreamWriter debugDataWriter;                      //Debugdaten
        private CStreamReader inputStreamReader;                    //Eingabedaten
        private byte[] matrixKey;                                   //Rundenseed für Generator
        private byte[] cm1;
        private byte[] cm3;
        private bool stop = false;                                  //soll das Plugin unterbrochen werden?
        private readonly Encoding encoding = Encoding.UTF8;                  //Standardausgabecodierung
        private readonly Encoding schnoor = Encoding.GetEncoding("437");     //DOS-US, wird von Schnoor's Programm genutzt
        private readonly byte[] DigitSet;                                    //von Schnoor gewählte Zeichen für Zahlen in größeren Zahlensystemen

        public CypherMatrixHash()
        {
            settings = new CypherMatrixHashSettings();
            cm1 = new byte[256];
            cm3 = new byte[256];
            string DigitSetStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz&#@αßΓπΣσµτΦΘΩδ∞φε∩{|}ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº⌐¬½¼";
            DigitSet = schnoor.GetBytes(DigitSetStr);
        }
        #endregion

        #region Data Properties and private writers

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public BigInteger OutputHash
        {
            get => outputHash;
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "DebugDataCaption", "DebugDataTooltip", false)]
        public ICrypToolStream OutputDebug
        {
            get => debugDataWriter;
            set
            {
                // empty
            }
        }

        /// <summary>
        /// Function to write data to the DebugStream.
        /// </summary>
        /// <param name="str">the string that should be written</param>
        private void WriteDebug(string str)
        {
            debugDataWriter.Write(encoding.GetBytes(str));
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
            stop = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            try
            {
                inputStreamReader = InputStream.CreateReader();
                //inputStreamReader.WaitEof();
                if (!ValidateInputs())
                {
                    return;     // beende die Ausführung bei Problemen mit den Eingabedaten
                }

                debugDataWriter = new CStreamWriter();

                if (settings.Debug)
                {
                    WriteDebug("Starting computation ...\r\n\r\n");
                    WriteDebug(string.Format("mode: {0}\r\n", settings.HashMode));
                    if (settings.HashMode != CypherMatrixHashSettings.CypherMatrixHashMode.Mini)
                    {
                        WriteDebug(string.Format("permutation: {0}\r\n", settings.Perm));
                    }
                    else
                    {
                        WriteDebug("permutation: none\r\n");
                    }

                    WriteDebug("\r\n\r\n");
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                Hash();

                sw.Stop();
                if (!stop)
                {
                    GuiLogMessage(string.Format("Processed {0:N} KiB data in {1} ms.", (double)InputStream.Length / 1024, sw.ElapsedMilliseconds), NotificationLevel.Info);
                    GuiLogMessage(string.Format("Achieved data throughput: {0:N} kB/s", (double)InputStream.Length / sw.ElapsedMilliseconds), NotificationLevel.Info);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message + "\r\n" + exception.StackTrace, NotificationLevel.Error);
                if (settings.Debug)
                {
                    WriteDebug("\r\nThe following error occurred during execution:\r\n");
                    WriteDebug(exception.Message + "\r\n" + exception.StackTrace + "\r\n");
                }
            }
            finally
            {
                if (stop)
                {
                    GuiLogMessage("Computation aborted!", NotificationLevel.Warning);
                    stop = false;
                }
                if (settings.Debug)
                {
                    WriteDebug("\r\n>>>>>>>>>> END OF OPERATION <<<<<<<<<<");
                }
                else
                {
                    WriteDebug("You have to enable the debug logging to see the internal variables here.");
                }

                debugDataWriter.Flush();
                debugDataWriter.Close();
                OnPropertyChanged("OutputDebug");
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            // lösche die angefallenen Werte
            cm1 = new byte[256];
            cm3 = new byte[256];
            matrixKey = null;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            stop = true;
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

        #region CypherMatrix

        // base function 
        private void Generator(int r)
        {
            // das Ergebnis der Mod Operation kann negativ sein! Im Skript wird immer von einer positiven Zahl ausgegangen
            // Initialliserung der Variablen
            int H_k = 0;
            int i, j, k, l;
            long H_p = 0, s_i = 0;
            List<int> d = new List<int>();
            int[] perm = new int[16];   // Permutationsarray bei Permutation mit Variate C
            byte[] rndX = null, rndY = null;     // Permutationsarrays bei Permutation mit Variate D

            int C_k = matrixKey.Length * (matrixKey.Length - 2) + settings.Code;

            for (i = 1; i <= matrixKey.Length; i++)
            {
                //!!Die alte Variante ist nur für Testzwecke gedacht!!
                //neue Variante
                H_k += (matrixKey[i - 1] + 1) * (i + C_k + r);   // i-1, da das Array von 0 bis matrixKey.Length-1 läuft, im Paper von 1 bis matrixKey.Length
            }
            //alte Variante
            //H_k += (matrixKey[i - 1] + 1) * (i + C_k);   // i-1, da das Array von 0 bis matrixKey.Length-1 läuft, im Paper von 1 bis matrixKey.Length

            // Berechnung der Hashfunktionsfolge
            for (i = 1; i <= matrixKey.Length; i++)
            {
                s_i = (((long)matrixKey[i - 1] + 1) * i * H_k + (i + settings.Code + r));    // i-1, da das Array von 0 bis matrixKey.Length-1 läuft, im Paper von 1 bis matrixKey.Length; Erhöhung der Präzision durch cast auf long, wichtig!
                LongToBaseX(s_i, d, settings.Basis);
                H_p += s_i;
            }

            long H_ges = H_p + H_k;
            List<int> tmp = new List<int>(d);
            LongToBaseX(H_ges, d, settings.Basis);
            tmp.Reverse();
            d.AddRange(tmp);
            tmp.Clear();

            // Berechnung der Parameter
            int variante = (H_k % 11) + 1;
            int Alpha = (int)(H_ges % 255 + 1);
            int Beta = H_k % 169 + 1;
            int Gamma = (int)((H_p + settings.Code) % 196 + 1);
            int Delta = (int)(H_ges % 155 + settings.Code);
            int Theta = H_k % 32 + 1;
            int Omega = H_k % 95 + 1;

            // Generierung der Basis-Variation
            k = 0;
            for (byte e = 0; k < 256; k++)
            {
                e = (byte)(BaseXToIntSafe(d, k + variante - 1, 3, settings.Basis + 1) - Theta);    // k + variante - 1, weil array d bei 0 anfängt; beim byte-cast wird automatisch mod 256 gerechnet
                //Logik zum testen ob ein Wert schon im Array vorhanden ist
                while (Array.IndexOf(cm1, e, 0, k) >= 0)
                {
                    e++;
                }
                cm1[k] = e;
            }

            // 3-fach Permutation der Basis-Variation
            switch (settings.Perm)
            {
                case CypherMatrixHashSettings.Permutation.None:
                    {
                        cm3 = cm1;
                        break;
                    }
                case CypherMatrixHashSettings.Permutation.B:
                    {
                        i = 1; k = 0; l = 0;
                        for (byte pos = (byte)(Alpha - 1); i <= 16; i++)
                        {
                            for (j = 1; j <= 16; j++)
                            {
                                k = i - j;
                                if (k <= 0)
                                {
                                    k += 16;
                                }

                                l = k - j;
                                if (l <= 0)
                                {
                                    l += 16;
                                }

                                cm3[(k - 1) * 16 + (l - 1)] = cm1[pos];
                                pos++;  // wird automatisch mod 256 gerechnet, da byte-Wert
                            }
                        }
                        break;
                    }
                case CypherMatrixHashSettings.Permutation.D:
                    {
                        rndX = new byte[16];
                        rndY = new byte[16];
                        i = 0;
                        for (byte x = (byte)Delta, y = (byte)Omega, z; i < 16; i++)
                        {
                            z = (byte)((cm1[x] + Theta) % 16);
                            while (Array.IndexOf(rndX, z, 0, i) >= 0)
                            {
                                z++;
                                if (z > 15)
                                {
                                    z -= 16;
                                }
                            }
                            rndX[i] = z;  // +1 weggelassen, da es im nächsten Schritt sonst wieder rückgängig gemacht werden müsste

                            z = (byte)((cm1[y] + Theta) % 16);
                            while (Array.IndexOf(rndY, z, 0, i) >= 0)
                            {
                                z++;
                                if (z > 15)
                                {
                                    z -= 16;
                                }
                            }
                            rndY[i] = z;  // +1 weggelassen, da es im nächsten Schritt sonst wieder rückgängig gemacht werden müsste

                            x++; y++;
                        }

                        i = 0;
                        for (byte pos = (byte)(Alpha - 1); i < 16; i++)
                        {
                            for (j = 0; j < 16; j++)
                            {
                                cm3[rndY[i] * 16 + rndX[j]] = cm1[pos];
                                pos++;  // wird automatisch mod 256 gerechnet, da byte-Wert
                            }
                        }
                        break;
                    }
                default: throw new NotSupportedException("Unknown permutation function!");
            }

            // Debugdaten schreiben, Teil 1
            if (settings.Debug)
            {
                WriteDebug(string.Format("Data of round {0}\r\n\r\n", r));
                WriteDebug(string.Format("code = {0}\r\n", settings.Code));
                WriteDebug(string.Format("basis = {0}\r\n", settings.Basis));
                WriteDebug(string.Format("matrixKeyLen = {0}\r\n", matrixKey.Length));
                WriteDebug(string.Format("hashBlockLen = {0}\r\n", settings.HashBlockLen));
                WriteDebug(string.Format("\r\nstartSequence (hex): \r\n "));
                for (i = 0; i < matrixKey.Length; i++)
                {
                    WriteDebug(string.Format(" {0:X2}", matrixKey[i]));
                }

                WriteDebug(string.Format("\r\n\r\nn = {0}\r\n", matrixKey.Length));
                WriteDebug(string.Format("C_k = {0}\r\n", C_k));
                WriteDebug(string.Format("H_k = {0}\r\n", H_k));
                WriteDebug(string.Format("H_p = {0}\r\n", H_p));
                WriteDebug("\r\nd (hex): \r\n ");
                foreach (int v in d)
                {
                    WriteDebug(string.Format(" {0:X2}", v));
                }

                WriteDebug(string.Format("\r\n\r\nvariante = {0}\r\n", variante));
                WriteDebug(string.Format("Alpha = {0}\r\n", Alpha));
                WriteDebug(string.Format("Beta = {0}\r\n", Beta));
                WriteDebug(string.Format("Gamma = {0}\r\n", Gamma));
                WriteDebug(string.Format("Delta = {0}\r\n", Delta));
                WriteDebug(string.Format("Theta = {0}\r\n", Theta));
                WriteDebug(string.Format("Omega = {0}\r\n", Omega));
                WriteDebug("\r\ncm1 (hex): \r\n");
                for (i = 0; i < 256;)
                {
                    for (j = 0; j < 16; j++)
                    {
                        WriteDebug(string.Format(" {0:X2}", cm1[i]));
                        i++;
                    }
                    WriteDebug("\r\n");
                }
                switch (settings.Perm)
                {
                    case CypherMatrixHashSettings.Permutation.D:
                        {
                            WriteDebug("\r\nrndX: \r\n");
                            for (j = 0; j < 16; j++)
                            {
                                WriteDebug(string.Format(" {0}", rndX[j]));
                            }
                            WriteDebug("\r\n");
                            WriteDebug("\r\nrndY: \r\n");
                            for (j = 0; j < 16; j++)
                            {
                                WriteDebug(string.Format(" {0}", rndY[j]));
                            }
                            WriteDebug("\r\n");
                            break;
                        }
                }

                WriteDebug("\r\ncm3 (hex): \r\n");
                for (i = 0; i < 256;)
                {
                    for (j = 0; j < 16; j++)
                    {
                        WriteDebug(string.Format(" {0:X2}", cm3[i]));
                        i++;
                    }
                    WriteDebug("\r\n");
                }
            }
        }

        // Hashfunktion, step
        private ulong HashStep(byte[] data, int dataSize, int r)
        {
            ulong H_k = 0, H_p = 0;

            matrixKey = new byte[dataSize];
            Buffer.BlockCopy(data, 0, matrixKey, 0, dataSize);
            Generator(r);

            int n = cm3.Length;
            int C_k = n * (n - 2) + settings.Code;

            for (int i = 1; i <= n; i++)
            {
                H_k += ((uint)cm3[i - 1] + 1) * (ulong)(i + C_k + r);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n
            }

            for (uint i = 1; i <= n; i++)
            {
                H_p += (((ulong)cm3[i - 1] + 1) * i * H_k + (uint)(i + settings.Code + r));    // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; Erhöhung der Präzision durch cast auf long, wichtig!
            }

            return H_p;
        }

        private BigInteger Hash_SMX()
        {
            ulong hashPart = 0;
            BigInteger hashSum = new BigInteger();
            int round = 1, bytesRead = 0, roundMax = 0;

            byte[] dataBlock = new byte[settings.HashBlockLen];

            while ((bytesRead = inputStreamReader.ReadFully(dataBlock)) > 0)
            {
                hashPart = HashStep(dataBlock, bytesRead, round);
                hashSum += hashPart;

                // Debugdaten schreiben
                if (settings.Debug)
                {
                    WriteDebug(string.Format("\r\n\r\nhashPart: {0}\r\n", hashPart));
                    WriteDebug("\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
                }

                if (stop)
                {
                    hashSum = 0;
                    break;
                }

                // Vorbereitungen für nächste Runde
                round++;
                roundMax = (int)(inputStreamReader.Length / settings.HashBlockLen) + 1;
                ProgressChanged(round, roundMax);
            }
            return hashSum;
        }

        private BigInteger Hash_FMX()
        {
            ulong hashPart = 0;
            BigInteger hashSum = new BigInteger();
            int round = 1, bytesRead = 0, roundMax = 0;

            byte[] dataBlock = new byte[settings.HashBlockLen];

            while ((bytesRead = inputStreamReader.ReadFully(dataBlock)) > 0)
            {
                hashPart = HashStep(dataBlock, bytesRead, round);
                hashSum += hashPart;

                // Debugdaten schreiben
                if (settings.Debug)
                {
                    WriteDebug(string.Format("\r\n\r\nhashPart: {0}\r\n", hashPart));
                    WriteDebug("\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
                }

                if (stop)
                {
                    hashSum = 0;
                    break;
                }

                // Vorbereitungen für nächste Runde
                round++;
                roundMax = (int)(inputStreamReader.Length / settings.HashBlockLen) + 1;
                ProgressChanged(round, roundMax);
            }

            // Generierung des finalen Hashstrings
            List<int> tmp = new List<int>();
            StringBuilder finalInput = DezNachSystem(62, hashSum);
            finalInput.Append(hashSum);
            finalInput.AppendFormat("{0:X2}", hashSum);
            finalInput.Append(round - 1);

            //Berechnung des finalen Hashwerts
            byte[] finalBytes = schnoor.GetBytes(finalInput.ToString());

            ulong H_k = 0, hashFinal = 0;
            int n = finalBytes.Length;
            int C_k = n * (n - 2) + settings.Code;

            for (uint i = 1; i <= n; i++)
            {
                H_k += ((uint)finalBytes[i - 1] + 1) * (ulong)(i + C_k);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; r = 0
            }

            for (uint i = 1; i <= n; i++)
            {
                hashFinal += (((ulong)finalBytes[i - 1] + 1) * i * H_k + (uint)(i + settings.Code));    // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; Erhöhung der Präzision durch cast auf long, wichtig!; r = 0
            }

            // Debugdaten schreiben
            if (settings.Debug)
            {
                WriteDebug(string.Format("hashPart last cycle: {0}\r\n", hashFinal));
            }

            hashSum += hashFinal;

            return hashSum;
        }

        private BigInteger Hash_Mini()
        {
            ulong hashPart = 0;
            BigInteger hashSum = new BigInteger();
            int round = 1, bytesRead = 0, roundMax = 0, C_k = 0;

            byte[] dataBlock = new byte[settings.HashBlockLen];

            while ((bytesRead = inputStreamReader.ReadFully(dataBlock)) > 0)
            {
                hashPart = 0;
                C_k = bytesRead * (bytesRead - 2) + settings.Code;
                for (int i = 1; i <= dataBlock.Length; i++)
                {
                    hashPart += ((uint)dataBlock[i - 1] + 1) * (ulong)(i + C_k + round);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n
                }

                hashSum += hashPart;

                // Debugdaten schreiben
                if (settings.Debug)
                {
                    WriteDebug(string.Format("Data of round {0}\r\n\r\n", round));
                    WriteDebug(string.Format("code = {0}\r\n", settings.Code));
                    WriteDebug(string.Format("basis = {0}\r\n", settings.Basis));
                    WriteDebug(string.Format("matrixKeyLen = {0}\r\n", matrixKey.Length));
                    WriteDebug(string.Format("hashBlockLen = {0}\r\n", settings.HashBlockLen));
                    WriteDebug(string.Format("\r\nstartSequence (hex): \r\n "));
                    for (int i = 0; i < dataBlock.Length; i++)
                    {
                        WriteDebug(string.Format(" {0:X2}", dataBlock[i]));
                    }

                    WriteDebug(string.Format("\r\n\r\nn = {0}\r\n", dataBlock.Length));
                    WriteDebug(string.Format("C_k = {0}\r\n", C_k));
                    WriteDebug(string.Format("hashPart: {0}\r\n", hashPart));
                    WriteDebug("\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
                }

                if (stop)
                {
                    hashSum = 0;
                    break;
                }

                // Vorbereitungen für nächste Runde
                round++;
                roundMax = (int)(inputStreamReader.Length / settings.HashBlockLen) + 1;
                ProgressChanged(round, roundMax);
            }

            // Generierung des finalen Hashstrings
            List<int> tmp = new List<int>();
            StringBuilder finalInput = DezNachSystem(62, hashSum);
            finalInput.Append(hashSum);
            finalInput.AppendFormat("{0:X2}", hashSum);
            finalInput.Append(round - 1);

            //Berechnung des finalen Hashwerts
            byte[] finalBytes = schnoor.GetBytes(finalInput.ToString());

            ulong H_k = 0, hashFinal = 0;
            int n = finalBytes.Length;
            C_k = n * (n - 2) + settings.Code;

            for (uint i = 1; i <= n; i++)
            {
                H_k += ((uint)finalBytes[i - 1] + 1) * (ulong)(i + C_k);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; r = 0
            }

            for (uint i = 1; i <= n; i++)
            {
                hashFinal += (((ulong)finalBytes[i - 1] + 1) * i * H_k + (uint)(i + settings.Code));    // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; Erhöhung der Präzision durch cast auf long, wichtig!; r = 0
            }

            // Debugdaten schreiben
            if (settings.Debug)
            {
                WriteDebug(string.Format("hashPart last cycle: {0}\r\n", hashFinal));
            }
            hashSum += hashFinal;

            return hashSum;
        }

        private void Hash()
        {
            BigInteger hash = 0;
            switch (settings.HashMode)
            {
                case CypherMatrixHashSettings.CypherMatrixHashMode.SMX:
                    {
                        hash = Hash_SMX();
                        break;
                    }
                case CypherMatrixHashSettings.CypherMatrixHashMode.FMX:
                    {
                        hash = Hash_FMX();
                        break;
                    }
                case CypherMatrixHashSettings.CypherMatrixHashMode.Mini:
                    {
                        hash = Hash_Mini();
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("Unknown hash function!");
                    }
            }
            if (hash != 0)
            {
                outputHash = hash;
                OnPropertyChanged("OutputHash");
            }

            // Debugdaten schreiben
            if (settings.Debug)
            {
                WriteDebug(string.Format("final hash value: {0}\r\n", hash));
            }
        }
        /// <summary>
        /// Function for valitating inputs.
        /// </summary>
        /// <param name="mode">Validate inputs for given mode.</param>
        /// <returns>false if an error occured, true if all is ok</returns>
        private bool ValidateInputs()
        {
            if (InputStream == null || InputStream.Length == 0)
            {
                GuiLogMessage("No input data, aborting now.", NotificationLevel.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        // function for changing to a choosen base
        private void LongToBaseX(long number, List<int> list, int x)
        {
            List<int> a = new List<int>();
            while (number != 0)
            {
                a.Add((int)(number % x));
                number = number / x;
            }
            for (int i = a.Count - 1; i >= 0; i--)
            {
                list.Add(a[i]);
            }
        }

        // function for changing back to the base 10
        private int BaseXToInt(List<int> list, int start, int length, int Base)
        {
            int b = 1, a = 0;
            for (int i = start + length - 1; i >= start; i--)
            {
                a += list[i] * b;
                b *= Base;
            }
            return a;
        }

        // function for changing back to the base 10, without errors
        // verhällt sich wie die Funktion im Basiccode, wenn außerhalb des gültigen Wertebereichs von list gelesen werden soll
        private int BaseXToIntSafe(List<int> list, int start, int length, int Base)
        {
            if (list.Count < start + length)
            {
                if (list.Count < start)
                {
                    return 0;   // start zeigt in nicht von list genutzten Speicher
                }

                length = list.Count - start;    // Berechne length neu
            }
            return BaseXToInt(list, start, length, Base);
        }

        /// <summary>
        /// Funktion um eine Zahl in ein gewähltes Zahlensystem umzuwandeln. Diese Funktion erzeugt die gleiche Ausgabe wie die korrespondierene im Basiccode.
        /// </summary>
        /// <param name="basis">gehähltes Zahlensystem</param>
        /// <param name="zahl">umzuwandelnte Zahl</param>
        /// <returns>umgewandelte Zahl (als StringBuilder)</returns>
        private StringBuilder DezNachSystem(uint basis, BigInteger zahl)
        {
            if (basis > 128)
            {
                throw new ArgumentException("The maximum for conversion supported base is 128.", "basis");
            }

            StringBuilder output = new StringBuilder();
            BigInteger number = zahl;
            uint tmp = 0;
            char[] charTmp = { (char)0x00 };
            byte[] byteTmp = { 0x00 };

            while (number != 0)
            {
                tmp = (uint)(number % basis);
                if (tmp > 9)
                {
                    byteTmp[0] = DigitSet[tmp - 10]; // -9 - 1 = -10; -1 weil Arr[] bei 0 beginnt
                    charTmp = schnoor.GetChars(byteTmp);
                    output.Insert(0, charTmp[0]);
                }
                else
                {
                    output.Insert(0, tmp);
                }
                number = number / basis;
            }

            return output;
        }

        // function to filter certain chars
        private bool CharFilter(byte i)
        {
            return i < 33 || i == 34 || i == 44 || i == 176 || i == 177 || i == 178 || i == 213 || i == 219 || i == 220 || i == 221 || i == 222 || i == 223;
        }

        #endregion
    }
}
