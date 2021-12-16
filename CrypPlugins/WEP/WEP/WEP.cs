using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.WEP
{

    /// <summary>
    /// Implements the WEP protocol. When encrypting data, 802.11 information are added
    /// (CRC32 checksum at the end of the frame, LLC/SNAP header in front of frame (both
    /// also encrpyted) and the for encryption used IV in clear, in front of packet).
    /// </summary>
    [Author("Stefan Schröder",
        "stef.schroeder@gmx.de",
        "Uni Siegen",
        "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.Properties.Resources",
        "PluginCaption",
        "PluginTooltip",
        "WEP/DetailedDescription/doc.xml",
        "WEP/icon.jpg", "WEP/Images/encrypt.png", "WEP/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class WEP : ICrypComponent
    {
        #region Private variables
        /* Input / output variables */
        private ICrypToolStream inputStream;
        private byte[] inputByteKey;
        private CStreamWriter outputStreamWriter;

        /* Organisational varibale */
        private List<byte[]> internalInputByteList;

        /* Is stop button pressed? */
        private bool stop = false;

        private WEPSettings settings;

        /* Internal help variables */
        private byte[] iV;
        private byte[] outputByte;
        private byte[] key;
        private int lengthOfInputByte;
        private int lengthOfOutputByte;
        private int counter;
        private CRC32 crc32;

        private static readonly byte[] header = new byte[] { 0xD4, 0xC3, 0xB2, 0xA1, 0x02, 0x00, 0x04, 0x00,
                                                               0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                               0xFF, 0xFF, 0x00, 0x00, 0x69, 0x00, 0x00, 0x00};

        [PropertyInfo(Direction.InputData,
            "InputStreamCaption",
            "InputStreamTooltip",
            false)]
        public ICrypToolStream InputStream
        {
            get => inputStream;
            set => inputStream = value;// mwander 20100503: not necessary for input property//OnPropertyChanged("InputStream");
        }

        [PropertyInfo(
            Direction.InputData,
            "InputByteKeyCaption",
            "InputByteKeyTooltip",
            true)]
        public byte[] InputByteKey
        {
            get => inputByteKey;
            set
            {
                inputByteKey = value;
                OnPropertyChanged("InputByteKey");
            }
        }

        [PropertyInfo(Direction.OutputData,
            "OutputStreamCaption",
            "OutputStreamTooltip",
            false)]
        public ICrypToolStream OutputStream
        {
            get => outputStreamWriter;
            set
            {
            }
        }

        #endregion

        #region Public interface

        public WEP()
        {
            settings = new WEPSettings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int makeIntFromFourBytes(byte[] b)
        {
            return b[3] << 24 | (b[2] & 0xff) << 16 | (b[1] & 0xff) << 8 | (b[0] & 0xff);
        }

        /// <summary>
        /// Converts an int value into a four byte array.
        /// </summary>
        /// <param name="value">The value to be converted into four bytes.</param>
        /// <returns>The byte array containing four byte values.</returns>
        private static byte[] get4BytesFromInt(int value)
        {
            return new byte[] { (byte)(value), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
        }

        /// <summary>
        /// Checks if the key is either of size 5 or 13 bytes.
        /// </summary>
        /// <returns>Integer value. 0 = valid key, 1 = null-reference or of length 0, 2 = wrong size</returns>
        private int checkForValidKey()
        {
            if (inputByteKey == null
                || ((inputByteKey != null) && (inputByteKey.Length == 0)))
            { return 1; }
            if ((inputByteKey.Length != 5) && (inputByteKey.Length != 13))
            { return 2; }
            else { return 0; }
        }

        /// <summary>
        /// Creates an IV if no one exists or increments it byte by byte if one exists.
        /// </summary>
        private void getInitialisationVector()
        {
            if (iV == null)
            {
                iV = new byte[3];
                iV[0] = 0;
                iV[1] = 0;
                iV[2] = 0;
            }
            else
            {
                iV[2]++;
                if ((iV[2] % 256) == 0)
                {
                    iV[1]++;
                }

                if (((iV[1] % 256) == 0) && (iV[2] % 256) == 0)
                {
                    iV[0]++;
                }
            }
        }

        /// <summary>
        /// Creates a LLC header according to 802.11 standard. This data are the first byte
        /// going to be encrypted, directly followed by use data.
        /// </summary>
        /// <returns>The LLC/SNAP header.</returns>
        private byte[] createLLCAndSNAPHeader()
        {
            /* LLC header: see http://de.wikipedia.org/wiki/Logical_Link_Control
             * 
             * 4 Bytes:
             * 1.: DSAP (Destination Service Access Point)
             * 2.: SSAP (Source Service Access Point)
             * 3. & 4.: Control field
             * 
             * SNAP header (subnetwork access protocol):
             * 
             * According to the PTW paper, this 8 bytes are allways
             * 0xAA AA 03 00 00 00 08 00
             * 
             */
            if (lengthOfInputByte == 46)
            {
                return new byte[] { 0xAA, 0xAA, 0x03, 0x00, 0x00, 0x00, 0x08, 0x06 };
            }
            else
            {
                return new byte[] { 0xAA, 0xAA, 0x03, 0x00, 0x00, 0x00, 0x08, 0x00 };
            }
        }

        /// <summary>
        /// Sets the IV in front of the frame. Is not very realistic, but
        /// other information of 802.11 header are not needed.
        /// </summary>
        private void concatenateWithIV()
        {
            int lengthOfOldOutputByte = outputByte.Length;

            Array.Resize(ref outputByte, lengthOfOldOutputByte + 3);
            lengthOfOldOutputByte = outputByte.Length;
            // shift array 3 positions
            for (int i = lengthOfOldOutputByte - 1; i >= 0; i--)
            {
                outputByte[i + 3] = outputByte[i];
            }
            outputByte[0] = iV[0];
            outputByte[1] = iV[1];
            outputByte[2] = iV[2];
        }

        /// <summary>
        /// Removes the integrity check value (ICV, in this case CRC-32) from the packet.
        /// </summary>
        /// <param name="a">The packet.</param>
        /// <returns>Packet without ICV.</returns>
        private byte[] removeICV(byte[] a)
        {
            byte[] tmp = new byte[lengthOfInputByte - 4];
            for (int i = 0; i < lengthOfInputByte - 4; i++)
            {
                tmp[i] = a[i];
            }
            lengthOfInputByte -= 4;
            lengthOfOutputByte -= 4;
            return tmp;
        }

        /// <summary>
        /// Removes first 16 bytes of packet (size, time of sniffing etc.) and returns a without those 16 bytes.
        /// </summary>
        /// <param name="a">The array containing the packet, including the requested header information.</param>
        /// <returns>The first 16 bytes.</returns>
        private byte[] removePacketIndividualHeader(byte[] a)
        {
            byte[] tmp = new byte[a.Length - 16];
            for (int i = 0; i < a.Length - 16; i++)
            {
                tmp[i] = a[i + 16];
            }
            return tmp;
        }

        /// <summary>
        /// Removes first 28 bytes from packet a and returns a without those 28 bytes.
        /// </summary>
        /// <param name="a">The array containing the packet, including the IEEE header information.</param>
        /// <returns>The packet without the IEEE header.</returns>
        private byte[] removeFirst28Bytes(byte[] a)
        {
            byte[] tmp = new byte[a.Length - 28];

            // Short input byte 28 bytes (IV + key index are removed)
            for (int i = 0; i < a.Length - 28; i++)
            {
                tmp[i] = a[i + 28];
            }
            return tmp;
        }

        /// <summary>
        /// Removes WEP parameters from IEEE header, namely the IV and the key index are removed.
        /// Returns the IEEE header without the WEP parameters.
        /// </summary>
        /// <param name="a">The IEEE header.</param>
        /// <returns>The IEEE header without the WEP parameters.</returns>
        private byte[] removeWEPParameters(byte[] a)
        {
            byte[] tmp = new byte[a.Length - 4];
            for (int i = 0; i < a.Length - 4; i++)
            {
                tmp[i] = a[i];
            }
            return tmp;
        }

        /// <summary>
        /// Removes the first 28 bytes (IEEE header) and returns that header.
        /// </summary>
        /// <param name="a">The packet containing the requested header.</param>
        /// <returns>The first 28 bytes of a.</returns>
        private byte[] provideFirst28Bytes(byte[] a)
        {
            byte[] tmp = new byte[28];
            for (int i = 0; i < 28; i++)
            {
                tmp[i] = a[i];
                if (i == 1)
                {
                    tmp[i] = (byte)(tmp[i] ^ 0x40);
                }
            }
            return tmp;
        }

        /// <summary>
        /// Removes the packet individual header and returns that header.
        /// </summary>
        /// <param name="a">The packet containing the requested header.</param>
        /// <returns>The first 16 bytes of a.</returns>
        private byte[] providePacketIndividualHeader(byte[] a)
        {
            byte[] tmp = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                tmp[i] = a[i];
            }
            return tmp;
        }

        /// <summary>
        /// If encrpytion is done, the used IV has to be concatenated to the packet. This is what
        /// this methode does.
        /// </summary>
        private void addIVToPacket()
        {
            int oldLength = lengthOfOutputByte;
            lengthOfOutputByte += 3;
            Array.Resize(ref outputByte, lengthOfOutputByte);

            // shift array 3 positions
            for (int i = oldLength - 1; i >= 0; i--)
            {
                outputByte[i + 3] = outputByte[i];
            }
            outputByte[0] = iV[0];
            outputByte[1] = iV[1];
            outputByte[2] = iV[2];
        }

        /// <summary>
        /// Concatenates two given byte arrays in the form a || b with || as symbol for
        /// concatenation.
        /// </summary>
        /// <param name="a">The array which stands at the first position in the concatenated array.</param>
        /// <param name="b">The array which stands at the second position in the concatenated array.</param>
        /// <returns></returns>
        private byte[] concatenateTwoArrays(byte[] a, byte[] b)
        {
            byte[] ret = null;
            ret = new byte[a.Length + b.Length];
            for (int i = a.Length + b.Length - 1; i >= 0; i--)
            {
                if (i - a.Length >= 0)
                {
                    ret[i] = b[i - a.Length];
                }
                if (i - a.Length < 0)
                {
                    ret[i] = a[i];
                }
            }
            return ret;
        }

        /// <summary>
        /// Simulates loading a List<byte[]> out of a file, in fact the data cames from another plugin.
        /// </summary>
        private void loadList(CStreamReader reader)
        {
            if (!fileIsValid(reader))
            {
                GuiLogMessage("Couldn't read data. Aborting now.", NotificationLevel.Error);
                return;
            }
            else
            {
                int streamlength;
                int packetsize;
                bool stillMorePackets = true;
                streamlength = 0;
                packetsize = 0;
                streamlength = (int)reader.Length;
                Debug.Assert(reader.CanSeek);
                reader.Position = 24;

                byte[] protection = new byte[1];
                byte[] size = new byte[4];
                while ((stillMorePackets) && (internalInputByteList.Count < 1000000))
                {
                    reader.Position += 12;
                    reader.Read(size, 0, 4);
                    reader.Position += 1;
                    reader.Read(protection, 0, 1);
                    packetsize = makeIntFromFourBytes(size);

                    if ((settings.Action == 1)
                        && ((protection[0] & 0x40) == 0x40)
                        && (protection[0] != 0xFF)
                        && (packetsize > 50))
                    {
                        byte[] tmp = new byte[packetsize + 16];
                        reader.Position -= 18;
                        reader.Read(tmp, 0, packetsize + 16);
                        internalInputByteList.Add(tmp);
                        tmp = null;
                    }
                    // Data is not WEP protected, so ignore first 2 MAC addresses
                    else if ((settings.Action == 0)
                        && (((protection[0] & 0x40) != 0x40) || (protection[0] == 0xFF))
                        && (packetsize > 50))
                    {
                        byte[] tmp = new byte[packetsize - 14];
                        reader.Position += 12;
                        reader.Read(tmp, 0, packetsize - 14);
                        internalInputByteList.Add(tmp);
                        tmp = null;
                    }

                    else
                    {
                        reader.Position += packetsize - 2;
                    }
                    if (reader.Position >= streamlength)
                    {
                        stillMorePackets = false;
                    }
                }
                if (internalInputByteList.Count == 1)
                {
                    GuiLogMessage("Got 1 packet.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Got " + internalInputByteList.Count.ToString("#,#", CultureInfo.InstalledUICulture) + " packets.", NotificationLevel.Info);
                }
            }
        }

        /// <summary>
        /// This methods checks, if a file is a valid file with captured packages 
        /// and no other kind of file (image, pdf, ...).        
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool fileIsValid(CStreamReader reader)
        {
            if (reader.Length < 10)
            {
                return false;
            }

            // Header of pcab file 
            byte[] headerData = new byte[10];
            reader.Read(headerData, 0, headerData.Length);
            // Test, if header is correct
            if (!checkIfEqual(headerData, new byte[] { 0xD4, 0xC3, 0xB2, 0xA1, 0x02, 0x00, 0x04, 0x00, 0x00, 0x00 }))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares two byte arrays. Returns true if they are equal (same values), false otherwise.
        /// </summary>
        /// <param name="a">First array to compare.</param>
        /// <param name="b">Second array to compare.</param>
        /// <returns>A boolean value indicating if the two arrays are equal or not.</returns>
        private bool checkIfEqual(byte[] a, byte[] b)
        {
            if ((null == a) || (null == b))
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IPlugin Member

        public void Dispose()
        {
            try
            {
                stop = false;
                counter = 0;
                lengthOfInputByte = 0;
                lengthOfOutputByte = 0;
                inputByteKey = null;
                iV = null;
                outputByte = null;
                crc32 = null;
                key = null;
                if (internalInputByteList != null) { internalInputByteList.Clear(); }
                internalInputByteList = null;
                inputStream = null;
                //if (outputByteList != null) { outputByteList.Clear(); }
                //outputByteList = null;
                outputStreamWriter = null;
                if (internalInputByteList != null) { internalInputByteList.Clear(); }
                //internalInputByteList = null;
            }
            catch (Exception exc)
            {
                GuiLogMessage(exc.Message.ToString(), NotificationLevel.Error);
            }
            stop = false;
        }

        public void Execute()
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                return;
            }

            try
            {
                internalInputByteList = new List<byte[]>();
                using (CStreamReader reader = inputStream.CreateReader())
                {
                    reader.WaitEof(); // does not support chunked streaming

                    /// Checks if the input stream contains a valid value. If not, class waits for input AND DOES NOTHING.
                    /// XXX: Execute() does not stop. Bug?
                    loadList(reader);

                    //GuiLogMessage("Ich habe jetzt " + internalInputByteList.Count + " Pakete....", NotificationLevel.Warning);
                    // Is there a key? - If yes, go on. If no: Give out a warning!
                    switch (checkForValidKey())
                    {
                        // Key is valid, so do nothing.
                        case 0:
                            break;
                        // Key is null reference or of length 0
                        case 1:
                            // Warning to the outside world and exit.
                            GuiLogMessage("WARNING - No key provided. Aborting now.", NotificationLevel.Error);
                            return;
                        // Key is of wrong size. Warning and create a dummey key.
                        case 2:
                            GuiLogMessage("WARNING -- wrong key size. Must be 5 or 13 bytes.", NotificationLevel.Error);
                            return;
                        default:
                            break;
                    }
                    outputStreamWriter = new CStreamWriter();
                    outputStreamWriter.Write(header, 0, header.Length);
                    key = new byte[inputByteKey.Length + 3];
                    for (int i = 0; i < inputByteKey.Length; i++)
                    {
                        key[i + 3] = inputByteKey[i];
                    }
                    counter = 0;
                    byte[] packetIndividualHeader = new byte[16];
                    packetIndividualHeader[0] = 0x0D;
                    packetIndividualHeader[1] = 0x12;
                    packetIndividualHeader[2] = 0xC9;
                    packetIndividualHeader[3] = 0x48;
                    packetIndividualHeader[4] = 0x78;
                    packetIndividualHeader[5] = 0x70;
                    packetIndividualHeader[6] = 0x01;
                    packetIndividualHeader[7] = 0x00;
                    // size of sniffed packet, used for looking in packet with Wireshark
                    packetIndividualHeader[8] = 0x00;
                    packetIndividualHeader[9] = 0x00;
                    packetIndividualHeader[10] = 0x00;
                    packetIndividualHeader[11] = 0x00;
                    // size
                    packetIndividualHeader[12] = 0x00;
                    packetIndividualHeader[13] = 0x00;
                    packetIndividualHeader[14] = 0x00;
                    packetIndividualHeader[15] = 0x00;

                    DateTime startTime = DateTime.Now;
                    for (int j = 0; j < internalInputByteList.Count; j++)
                    {
                        if (stop) { break; }
                        byte[] tempInputByte = internalInputByteList.ElementAt(j);
                        lengthOfInputByte = tempInputByte.Length;
                        lengthOfOutputByte = lengthOfInputByte;
                        // Dependeing on action, there are some modifications to the packet necessary. That's done here.
                        switch (settings.Action)
                        {
                            case 0:
                                tempInputByte = concatenateTwoArrays(createLLCAndSNAPHeader(), tempInputByte);
                                lengthOfInputByte = lengthOfOutputByte = tempInputByte.Length;

                                crc32 = new CRC32();
                                byte[] icv = crc32.ComputeHash(tempInputByte);

                                lengthOfInputByte += 4;
                                lengthOfOutputByte += 4;

                                Array.Resize(ref tempInputByte, lengthOfInputByte);
                                tempInputByte[lengthOfInputByte - 4] = icv[0];
                                tempInputByte[lengthOfInputByte - 3] = icv[1];
                                tempInputByte[lengthOfInputByte - 2] = icv[2];
                                tempInputByte[lengthOfInputByte - 1] = icv[3];

                                byte[] size = get4BytesFromInt(lengthOfInputByte + 28);
                                packetIndividualHeader[8] = size[0];
                                packetIndividualHeader[9] = size[1];
                                packetIndividualHeader[10] = size[2];
                                packetIndividualHeader[11] = size[3];

                                packetIndividualHeader[12] = size[0];
                                packetIndividualHeader[13] = size[1];
                                packetIndividualHeader[14] = size[2];
                                packetIndividualHeader[15] = size[3];

                                //permutation = new byte[256];
                                getInitialisationVector();
                                //outputByte = new byte[lengthOfOutputByte];
                                key[0] = iV[0];
                                key[1] = iV[1];
                                key[2] = iV[2];

                                outputByte = RC4.rc4encrypt(tempInputByte, key);

                                // Key index
                                outputByte = concatenateTwoArrays(new byte[] { 0x00 }, outputByte);
                                // initialisation vector
                                outputByte = concatenateTwoArrays(iV, outputByte);
                                // sequence controll
                                outputByte = concatenateTwoArrays(new byte[] { 0xA5 }, outputByte);
                                // Frame number
                                outputByte = concatenateTwoArrays(new byte[] { 0xC0 }, outputByte);
                                // MAC address destination
                                outputByte = concatenateTwoArrays(new byte[] { 0x00, 0x12, 0xBF, 0xDC, 0x4E, 0x7A }, outputByte);
                                // MAC address source
                                outputByte = concatenateTwoArrays(new byte[] { 0x00, 0xA0, 0xD1, 0x25, 0xB9, 0xEC }, outputByte);
                                // BSS ID
                                outputByte = concatenateTwoArrays(new byte[] { 0x00, 0x12, 0xBF, 0xDC, 0x4E, 0x7C }, outputByte);
                                // IEEE 802.11 header
                                outputByte = concatenateTwoArrays(new byte[] { 0x08, 0x41, 0x75, 0x00 }, outputByte);
                                // packet individual header, size and some other information
                                outputByte = concatenateTwoArrays(packetIndividualHeader, outputByte);
                                //outputByteList.Add(outputByte);
                                outputStreamWriter.Write(outputByte, 0, outputByte.Length);

                                crc32 = null;
                                icv = null;
                                size = null;
                                //permutation = null;
                                tempInputByte = null;
                                outputByte = null;
                                break;

                            case 1:
                                byte[] pIH = providePacketIndividualHeader(tempInputByte);
                                tempInputByte = removePacketIndividualHeader(tempInputByte);

                                byte[] iEEEHeaderInformation = provideFirst28Bytes(tempInputByte);
                                tempInputByte = removeFirst28Bytes(tempInputByte);

                                key[0] = iEEEHeaderInformation[24];
                                key[1] = iEEEHeaderInformation[25];
                                key[2] = iEEEHeaderInformation[26];


                                iEEEHeaderInformation = removeWEPParameters(iEEEHeaderInformation);

                                lengthOfInputByte = lengthOfOutputByte = tempInputByte.Length;

                                // new packet size = packetsize + 28 (=IEEE header) - WEP parameters (IV & key index, 4 bytes) - ICV (4 bytes)
                                byte[] sizeCase1 = get4BytesFromInt(lengthOfOutputByte + 20);
                                pIH[8] = sizeCase1[0];
                                pIH[9] = sizeCase1[1];
                                pIH[10] = sizeCase1[2];
                                pIH[11] = sizeCase1[3];

                                pIH[12] = sizeCase1[0];
                                pIH[13] = sizeCase1[1];
                                pIH[14] = sizeCase1[2];
                                pIH[15] = sizeCase1[3];

                                outputByte = RC4.rc4encrypt(tempInputByte, key);

                                outputByte = removeICV(tempInputByte);

                                // IEEE header and output byte are concatenated
                                outputByte = concatenateTwoArrays(iEEEHeaderInformation, outputByte);

                                // packet individual header, size and some other information
                                outputByte = concatenateTwoArrays(pIH, outputByte);
                                //outputByteList.Add(outputByte);
                                outputStreamWriter.Write(outputByte, 0, outputByte.Length);

                                outputByte = null;
                                pIH = null;
                                iEEEHeaderInformation = null;
                                size = null;
                                break;

                            default:
                                break;
                        }
                        counter++;
                        if (internalInputByteList != null) { ProgressChanged(counter, internalInputByteList.Count); }
                        tempInputByte = null;
                    }
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (!stop)
                    {
                        if (settings.Action == 0)
                        {
                            GuiLogMessage("Encryption complete!", NotificationLevel.Info);
                        }
                        if (settings.Action == 1)
                        {
                            GuiLogMessage("Decryption complete!", NotificationLevel.Info);
                        }
                        if (counter == 1)
                        {
                            GuiLogMessage("Time used [h:min:sec]: " + duration.ToString(), NotificationLevel.Info);
                        }
                        else
                        {
                            GuiLogMessage("Time used [h:min:sec]: " + duration.ToString() + " for " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets.", NotificationLevel.Info);
                        }
                        outputStreamWriter.Close();
                        OnPropertyChanged("OutputStream");
                    }
                    if (stop)
                    {
                        outputStreamWriter.Close();
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                    internalInputByteList.Clear();
                    internalInputByteList = null;
                    key = null;
                }
            }
            catch (Exception exc)
            {
                GuiLogMessage(exc.Message.ToString(), NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1.0, 1.0);
            }
        }

        public void Initialize()
        {
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation => null;

        public ISettings Settings
        {
            get => settings;
            set => settings = (WEPSettings)value;
        }

        public void Stop()
        {
            stop = true;
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
