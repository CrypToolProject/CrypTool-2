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
using System.Numerics;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.CypherMatrix
{
    [Author("Michael Schäfer", "michael.schaefer@rub.de", null, null)]
    [PluginInfo("CypherMatrix.Properties.Resources", "PluginCaption", "PluginTooltip", "CypherMatrix/doc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class CypherMatrix : ICrypComponent
    {
        #region Private variables and public constructor

        private readonly CypherMatrixSettings settings;             //Nutzereinstellungen
        private CStreamWriter outputStreamWriter;                   //Ausgabedaten
        private CStreamWriter debugDataWriter;                      //Debugdaten
        private CStreamReader inputStreamReader;                    //Eingabedaten
        private byte[] passwordBytes;                               //Passwort
        private List<byte> cipherChars;                             //Substitutionstabelle für Verschlüsselung
        private List<byte> blockKey;                                //XOR-Schlüssel
        private byte[] matrixKey;                                   //Rundenseed für Generator
        private byte[] cm1;
        private byte[] cm3;
        private bool stop = false;                                  //soll das Plugin unterbrochen werden?
        private readonly Encoding encoding = Encoding.UTF8;                  //Standardausgabecodierung

        public CypherMatrix()
        {
            settings = new CypherMatrixSettings();
            cm1 = new byte[256];
            cm3 = new byte[256];
            cipherChars = new List<byte>(128);
        }
        #endregion

        #region Data Properties and private writers

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputPasswordCaption", "InputPasswordTooltip", false)]
        public byte[] InputByteArray
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => outputStreamWriter;
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
        /// Function to write data to the OutputStream.
        /// </summary>
        /// <param name="value">the value that should be written</param>
        private void WriteOutput(ulong value)
        {
            outputStreamWriter.Write(encoding.GetBytes(value.ToString()));
        }

        /// <summary>
        /// Function to write data to the OutputStream.
        /// </summary>
        /// <param name="value">the value that should be written</param>
        private void WriteOutput(BigInteger value)
        {
            outputStreamWriter.Write(encoding.GetBytes(value.ToString()));
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

                outputStreamWriter = new CStreamWriter();
                debugDataWriter = new CStreamWriter();
                blockKey = new List<byte>(settings.BlockKeyLen);

                if (settings.Debug)
                {
                    WriteDebug("Starting computation ...\r\n\r\n");
                    WriteDebug(string.Format("mode: {0}\r\n", settings.Action));
                    WriteDebug(string.Format("permutation: {0}\r\n", settings.Perm));
                    WriteDebug("\r\n\r\n");
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                switch (settings.Action)
                {
                    case CypherMatrixSettings.CypherMatrixMode.Encrypt:
                        {
                            Encrypt();
                            break;
                        }
                    case CypherMatrixSettings.CypherMatrixMode.Decrypt:
                        {
                            Decrypt();
                            break;
                        }
                    default:
                        {
                            sw.Stop();
                            outputStreamWriter.Close();
                            throw new NotSupportedException("Unkown execution mode!");
                            //break;
                        }
                }
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

                outputStreamWriter.Flush();
                outputStreamWriter.Close();
                debugDataWriter.Flush();
                debugDataWriter.Close();
                OnPropertyChanged("OutputStream");
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
            cipherChars = new List<byte>(128);
            blockKey = null;
            passwordBytes = null;
            matrixKey = null;
            InputByteArray = null;  //Passphraseneingang löschen
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

        private void Encrypt()
        {
            int length = settings.BlockKeyLen, bytesRead = 0;
            int roundMax = (int)(inputStreamReader.Length / length) + 1;
            List<byte> xor = new List<byte>();
            List<byte> index = new List<byte>();
            List<byte> ciphertext = new List<byte>();
            byte[] plaintext = new byte[length];

            matrixKey = new byte[passwordBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, matrixKey, 0, passwordBytes.Length);
            int round = 1;

            while ((bytesRead = inputStreamReader.ReadFully(plaintext)) > 0)
            {
                if (bytesRead < length)
                {
                    // in der letzten Runde Padding durch hinzufügen von Leerzeichen bis der Puffer voll ist
                    for (int i = bytesRead; i < plaintext.Length; i++)
                    {
                        plaintext[i] = 0x20;
                    }
                }


                // Schlüssel generieren
                Generator(round);

                // Verschlüsseln
                // 1. Klartext XOR Blockschlüssel
                for (int i = 0; i < length; i++)
                {
                    xor.Add((byte)(plaintext[i] ^ blockKey[i]));
                }

                // bit conversation
                int puffer = 0;
                int bitCount = 0;

                for (int i = 0; i < length; i++)
                {
                    puffer <<= 8;       // mache für die nächsten 8 Bits Platz
                    puffer |= xor[i];   // schreibe die nächsten 8 Bits in den Puffer
                    bitCount += 8;             // bitCount als Zähler für die Bits im Puffer, erhöhe um 8
                    index.Add((byte)(puffer >> (bitCount - 7) & 0x7F));    // lies die obersten 7 Bits aus
                    bitCount -= 7;             // verringere um 7, da 7 Bits ausgelesen wurden
                    // aus Performancegründen werden die gelesenen Bits nicht gelöscht
                    if (bitCount == 7)
                    {
                        index.Add((byte)(puffer & 0x7F));       // haben sich 7 Bits angesammelt, so ließ sie aus
                        bitCount = 0;          // 7 Bits gelesen, 7 - 7 = 0
                    }
                }
                switch (bitCount)
                {
                    case 1: index.Add((byte)(puffer & 0x01)); break;
                    case 2: index.Add((byte)(puffer & 0x03)); break;
                    case 3: index.Add((byte)(puffer & 0x07)); break;
                    case 4: index.Add((byte)(puffer & 0x0F)); break;
                    case 5: index.Add((byte)(puffer & 0x1F)); break;
                    case 6: index.Add((byte)(puffer & 0x3F)); break;
                    default: break;
                }

                // Abbildung auf Chiffre-Alphabet
                for (int i = 0; i < index.Count; i++)
                {
                    outputStreamWriter.WriteByte(cipherChars[index[i]]);
                }

                // Debugdaten schreiben
                if (settings.Debug)
                {
                    WriteDebug("\r\n\r\nplaintext (hex): \r\n ");
                    foreach (byte b in plaintext)
                    {
                        WriteDebug(string.Format(" {0:X2}", b));
                    }

                    WriteDebug("\r\n\r\nplaintext xor block key (hex): \r\n ");
                    xor.ForEach(delegate (byte b)
                    {
                        WriteDebug(string.Format(" {0:X2}", b));
                    });
                    WriteDebug("\r\n\r\nindex: \r\n ");
                    index.ForEach(delegate (byte b)
                    {
                        WriteDebug(string.Format(" {0}", b));
                    });
                    WriteDebug("\r\n\r\nciphertext (hex): \r\n ");
                    CStreamReader reader = outputStreamWriter.CreateReader();
                    int start = settings.BlockKeyLen * (round - 1);
                    reader.Seek(start, SeekOrigin.Begin);
                    for (int i = 0; i < index.Count; i++)
                    {
                        WriteDebug(string.Format(" {0:X2}", (byte)reader.ReadByte()));
                    }

                    WriteDebug("\r\n\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
                }

                // Vorbereitungen für die nächste Runde
                cipherChars.Clear();
                blockKey.Clear();
                xor.Clear();
                index.Clear();

                if (stop)
                {
                    break;
                }

                roundMax = (int)(inputStreamReader.Length / length) + 1;
                ProgressChanged(round, roundMax);
                round++;
            }
        }

        private void Decrypt()
        {
            // zur Berechnung der Länge eines Chiffreblocks wird mathematisch gerundet

            int length = settings.BlockKeyLen, bytesRead = 0, len7 = (int)Math.Round((double)settings.BlockKeyLen * 8 / 7);
            int roundMax = (int)(inputStreamReader.Length / length) + 1;
            List<byte> xor = new List<byte>();
            List<byte> index = new List<byte>();
            byte[] cipherBlock = new byte[len7];
            matrixKey = new byte[passwordBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, matrixKey, 0, passwordBytes.Length);
            int round = 1;

            while ((bytesRead = inputStreamReader.ReadFully(cipherBlock)) > 0)
            {
                if (bytesRead < len7)
                {
                    len7 = cipherBlock.Length;      // in der letzten Runde ist der Klartext warscheinlich nicht einen Block breit
                }

                // Schlüssel generieren
                Generator(round);

                int puffer = 0;
                // Analyse Chiffrat, char zu 7-Bit Index
                foreach (byte b in cipherBlock)
                {
                    if ((puffer = cipherChars.IndexOf(b)) >= 0)
                    {
                        index.Add((byte)puffer);
                    }
                    else
                    {
                        throw new ArgumentException("Decryption failed!", "cipherBlock");
                    }
                }

                // Bit-Konversion, 7-Bit Index zu 8-Bit Werten
                puffer = 0;
                int bitCount = 0;
                for (int i = 0; i < index.Count; i++)
                {
                    puffer <<= 7;           // mache für die nächsten 7 Bits Platz
                    puffer |= index[i];     // schreibe die nächsten 7 Bits in den Puffer
                    bitCount += 7;          // bitCount als Zähler für die Bits im Puffer, erhöhe um 7
                    // aus Performancegründen werden die gelesenen Bits nicht gelöscht
                    if (bitCount > 7)
                    {
                        xor.Add((byte)(puffer >> (bitCount - 8) & 0xFF));       // haben sich 8 Bits angesammelt, so ließ sie aus
                        bitCount -= 8;          // 8 Bits gelesen
                    }
                }
                switch (bitCount)
                {
                    case 1: xor.Add((byte)(puffer & 0x01)); break;
                    case 2: xor.Add((byte)(puffer & 0x03)); break;
                    case 3: xor.Add((byte)(puffer & 0x07)); break;
                    case 4: xor.Add((byte)(puffer & 0x0F)); break;
                    case 5: xor.Add((byte)(puffer & 0x1F)); break;
                    case 6: xor.Add((byte)(puffer & 0x3F)); break;
                    case 7: xor.Add((byte)(puffer & 0x7F)); break;
                    default: break;
                }

                // XOR mit Blockschlüssel, Rückgewinnung des Klartextes
                for (int i = 0; i < xor.Count; i++)
                {
                    //plaintext.Append((char)(xor[i] ^ blockKey[i]));
                    outputStreamWriter.WriteByte((byte)(xor[i] ^ blockKey[i]));
                }

                // Debugdaten schreiben
                if (settings.Debug)
                {
                    WriteDebug("\r\n\r\nciphertext (hex): \r\n ");
                    foreach (byte b in cipherBlock)
                    {
                        WriteDebug(string.Format(" {0:X2}", b));
                    }

                    WriteDebug("\r\n\r\nindex: \r\n ");
                    index.ForEach(delegate (byte b)
                    {
                        WriteDebug(string.Format(" {0}", b));
                    });
                    WriteDebug("\r\n\r\nplaintext xor block key (hex): \r\n ");
                    xor.ForEach(delegate (byte b)
                    {
                        WriteDebug(string.Format(" {0:X2}", b));
                    });
                    WriteDebug("\r\n\r\nplaintext (hex): \r\n ");
                    CStreamReader reader = outputStreamWriter.CreateReader();
                    int start = settings.BlockKeyLen * (round - 1);
                    reader.Seek(start, SeekOrigin.Begin);
                    for (int i = 0; i < xor.Count; i++)
                    {
                        WriteDebug(string.Format(" {0:X2}", (byte)reader.ReadByte()));
                    }

                    WriteDebug("\r\n\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
                }

                // Vorbereitungen für die nächste Runde
                cipherChars.Clear();
                blockKey.Clear();
                xor.Clear();
                index.Clear();

                if (stop)
                {
                    break;
                }

                roundMax = (int)(inputStreamReader.Length / length) + 1;
                ProgressChanged(round, roundMax);
                round++;
            }
        }

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
                case CypherMatrixSettings.Permutation.None:
                    {
                        cm3 = cm1;
                        break;
                    }
                case CypherMatrixSettings.Permutation.B:
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
                case CypherMatrixSettings.Permutation.D:
                    {
                        //VarFolge(K)=DatFolge(K)-Theta //VarFolge(K) == cm1[k]
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
                WriteDebug(string.Format("blockKeyLen = {0}\r\n", settings.BlockKeyLen));
                WriteDebug(string.Format("matrixKeyLen = {0}\r\n", settings.MatrixKeyLen));
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
                    case CypherMatrixSettings.Permutation.D:
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

            // Entnahme Chiffre-Alphabet
            cipherChars.AddRange(cm3);
            cipherChars.AddRange(cm3);
            cipherChars.RemoveRange(0, Alpha - 1);
            cipherChars.RemoveAll(CharFilter);
            if (cipherChars.Count > 128)
            {
                cipherChars.RemoveRange(128, cipherChars.Count - 128);
            }

            // Block-Schlüssel
            for (i = 0; i < settings.BlockKeyLen; i++)
            {
                blockKey.Add(cm3[Beta - 1 + i]);
            }

            // Matrix-Schlüssel
            matrixKey = new byte[settings.MatrixKeyLen];
            for (i = 0; i < settings.MatrixKeyLen; i++)
            {
                matrixKey[i] = cm3[Gamma - 1 + i];
            }

            // Debugdaten schreiben, Teil 2
            if (settings.Debug)
            {
                WriteDebug("\r\ncipherChars (hex): \r\n ");
                foreach (byte b in cipherChars)
                {
                    WriteDebug(string.Format(" {0:X2}", b));
                }

                WriteDebug("\r\n\r\nblockKey (hex): \r\n ");
                foreach (byte b in blockKey)
                {
                    WriteDebug(string.Format(" {0:X2}", b));
                }

                WriteDebug("\r\n\r\nmatrixKey (hex): \r\n ");
                foreach (byte b in matrixKey)
                {
                    WriteDebug(string.Format(" {0:X2}", b));
                }
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
            if (InputByteArray == null || InputByteArray.Length == 0)
            {
                GuiLogMessage("No password bytes, aborting now.", NotificationLevel.Error);
                return false;
            }

            if (InputByteArray.Length > 64)
            {
                passwordBytes = new byte[64];
                Buffer.BlockCopy(InputByteArray, 0, passwordBytes, 0, 64);
                GuiLogMessage("Password has more then 64 bytes! It is truncated to the first 64.", NotificationLevel.Warning);
            }
            else
            {
                passwordBytes = new byte[InputByteArray.Length];
                Buffer.BlockCopy(InputByteArray, 0, passwordBytes, 0, InputByteArray.Length);
                if (InputByteArray.Length < 36)
                {
                    GuiLogMessage("For security reasons it is recommended to choose a password with at least 36 characters!", NotificationLevel.Warning);
                }
            }
            return true;
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

        // function to filter certain chars
        private bool CharFilter(byte i)
        {
            return i < 33 || i == 34 || i == 44 || i == 176 || i == 177 || i == 178 || i == 213 || i == 219 || i == 220 || i == 221 || i == 222 || i == 223;
        }

        #endregion
    }
}
