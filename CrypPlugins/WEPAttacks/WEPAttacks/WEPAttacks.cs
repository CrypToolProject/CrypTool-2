using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.WEPAttacks
{
    /// <summary>
    /// Implements some attacks on the WEP protocol. So the FMS attack, the KoreK attack and the
    /// PTW attack are implemented.
    /// </summary>
    [Author("Stefan Schröder",
        "stef.schroeder@gmx.de",
        "Uni Siegen",
        "http://www.uni-siegen.de")]
    [PluginInfo("WEPAttacks.Properties.Resources",
        "PluginCaption",
        "PluginTooltip",
        "WEPAttacks/DetailedDescription/doc.xml",
        "WEPAttacks/Mallory.jpg")]
    [ComponentCategory(ComponentCategory.Protocols)]
    [AutoAssumeFullEndProgress(false)]
    public class WEPAttacks : ICrypComponent
    {
        #region Private variables
        /* in- and output matters */
        private ICrypToolStream inputStream;
        private CStreamWriter outputStreamWriter;
        private List<byte[]> internalByteList;

        /* help variables */
        private List<byte[]> tempList;
        private byte[] iVConcKey;
        private byte[] permutation;

        private int[,] fmsVotes;
        private int[,] koreKVotes;
        private int[,] ptwVotes;

        private Random rnd;

        private int counter = 0;
        private int usedPacktesCounter;
        private int attackedKeyByte;
        private int keysize;
        private bool success;
        private bool stop = false;
        private WEPAttacksSettings settings;
        private TimeSpan totalDuration;

        private static readonly byte[] header = new byte[] { 0xD4, 0xC3, 0xB2, 0xA1, 0x02, 0x00, 0x04, 0x00,
                                                               0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                               0xFF, 0xFF, 0x00, 0x00, 0x69, 0x00, 0x00, 0x00};

        /* external help objects */
        private readonly WEPAttacksPresentation presentation = new WEPAttacksPresentation();

        [PropertyInfo(Direction.InputData,
            "InputStreamCaption",
            "InputStreamTooltip",
            false)]
        public ICrypToolStream InputStream
        {
            get => inputStream;
            set => inputStream = value;// mwander 20100503: not necessary for input properties//OnPropertyChanged("InputStream");
        }

        [PropertyInfo(Direction.OutputData,
            "SuccessCaption",
            "SuccessTooltip",
            false)]
        public bool Success
        {
            get => success;
            set
            {
                success = value;
                OnPropertyChanged("Success");
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

        public WEPAttacks()
        {
            settings = new WEPAttacksSettings();
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
        /// This methods checks, if a file is a valid file with captured packages 
        /// and no other kind of file (image, pdf, ...).        
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool fileIsValid(CStreamReader reader)
        {
            // Header of pcab file 
            byte[] headerData = new byte[10];
            if (reader.Length < 10)
            {
                return false;
            }

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

        /// <summary>
        /// Simulates loading a List<byte[]> out of a file, in fact the data cames from another plugin.
        /// </summary>
        private List<byte[]> loadList(CStreamReader reader)
        {
            List<byte[]> tempList = new List<byte[]>();

            if (!fileIsValid(reader))
            {
                // XXX: will not abort Execute() run
                GuiLogMessage("Couldn't read data. Aborting now.", NotificationLevel.Error);
                return tempList;
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
                while (stillMorePackets && (tempList.Count < 1000000))
                {
                    reader.Position += 12;
                    reader.Read(size, 0, 4);
                    reader.Position += 1;
                    reader.Read(protection, 0, 1);
                    packetsize = makeIntFromFourBytes(size);
                    if (((protection[0] & 0x40) != 0x40) || (protection[0] == 0xFF))
                    {
                        reader.Position += packetsize - 2;
                    }
                    else
                    {
                        if (packetsize > 60)
                        {
                            byte[] tmp = new byte[packetsize + 16];
                            reader.Position -= 18;
                            reader.Read(tmp, 0, packetsize + 16);
                            tempList.Add(tmp);
                            tmp = null;
                        }
                        else
                        {
                            reader.Position += packetsize - 2;
                        }
                    }
                    if (reader.Position >= streamlength)
                    {
                        stillMorePackets = false;
                    }
                }
                if (tempList.Count == 1)
                {
                    GuiLogMessage("Got 1 packet.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Got " + tempList.Count.ToString("#,#", CultureInfo.InstalledUICulture) + " packets.", NotificationLevel.Info);
                }
                presentation.setNumberOfSniffedPackages(tempList.Count + counter);
            }

            return tempList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int makeIntFromFourBytes(byte[] b)
        {
            return b[3] << 24 | (b[2] & 0xff) << 16 | (b[1] & 0xff) << 8 | (b[0] & 0xff);
        }

        private static int makeIntFromTwoBytes(byte[] b)
        {
            return b[1] << 8 | (b[0] & 0xff);
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

        // <summary>
        /// Converts an int value into a two byte array.
        /// </summary>
        /// <param name="value">The value to be converted into two bytes.</param>
        /// <returns>The byte array containing two byte values.</returns>
        private static byte[] get2BytesFromInt(int value)
        {
            return new byte[] { (byte)(value), (byte)(value >> 8) };
        }


        /// <summary>
        /// Initializes all needed objects if not allready existing.
        /// </summary>
        private void init()
        {
            // fill all dimensions with zeros
            // not sure what is in array imediately after initialisation, so be sure...
            if (fmsVotes == null)
            {
                fmsVotes = new int[13, 256];
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        fmsVotes[i, j] = 0;
                    }
                }
            }

            if (koreKVotes == null)
            {
                koreKVotes = new int[13, 256];
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        koreKVotes[i, j] = 0;
                    }
                }
            }

            if (ptwVotes == null)
            {
                ptwVotes = new int[13, 256];
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        ptwVotes[i, j] = 0;
                    }
                }
            }

            if (rnd == null)
            {
                rnd = new Random();
            }
        }

        private void extractHeaderInformation()
        {
            int i, j;
            i = 0;
            for (int k = 0; k < tempList.Count; k++)
            {
                byte[] array = tempList.ElementAt(k);
                byte[] tmp = new byte[21];
                j = 0;
                for (i = 40; i < 43; i++)
                {
                    // extract IV and first 15 bytes
                    tmp[j] = array[i];
                    j++;
                }
                for (i = 44; i < 60; i++)
                {
                    tmp[j] = array[i];
                    j++;
                }
                byte[] size = get2BytesFromInt(array.Length);
                tmp[19] = size[0];
                tmp[20] = size[1];
                internalByteList.Add(tmp);

                tmp = null;
            }
            //if (tempList != null) { tempList.Clear(); }
        }

        /// <summary>
        /// Searches for the max value in an array of 2 dimensions in the given dimension and returns the
        /// index of the highest value.
        /// </summary>
        /// <param name="voteTable">The vote table in which the search has to be done.</param>
        /// <param name="dimension">The dimension in which the max value has to be searched.</param>
        /// <returns>The index of the highest value in the given dimension.</returns>
        private int findMaxVotedByte(int voteTable, int dimension)
        {
            int temp = 0;
            int index = 0;
            switch (voteTable)
            {
                case 0:
                    for (int i = 0; i < 256; i++)
                    {
                        if (fmsVotes[dimension, i] > temp)
                        {
                            temp = fmsVotes[dimension, i];
                            index = i;
                        }
                    }
                    break;

                case 1:
                    for (int i = 0; i < 256; i++)
                    {
                        if (koreKVotes[dimension, i] > temp)
                        {
                            temp = koreKVotes[dimension, i];
                            index = i;
                        }
                    }
                    break;

                case 2:
                    for (int i = 0; i < 256; i++)
                    {
                        if (ptwVotes[dimension, i] > temp)
                        {
                            temp = ptwVotes[dimension, i];
                            index = i;
                        }
                    }
                    break;

                default:
                    break;
            }

            return index;
        }

        private byte[] supplyFirst28Bytes(byte[] a)
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
        /// Removes first 28 bytes from packet a.
        /// </summary>
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
        /// If key is recovered, this method decrypts all packets in memory.
        /// </summary>
        private void decrypt()
        {
            byte[] array;
            byte[] key64 = new byte[8];
            byte[] key128 = new byte[16];

            byte[] iEEEHeaderInformation;

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

            byte[] decryptedOutput;
            outputStreamWriter.Write(header, 0, header.Length);
            for (int i = 0; i < tempList.Count; i++)
            {
                array = tempList.ElementAt(i);
                // Size - packet individual header (=16) - WEP Parameter (IV & Key index, =4)
                byte[] size = get4BytesFromInt(array.Length - 24);

                array = removePacketIndividualHeader(array);

                iEEEHeaderInformation = supplyFirst28Bytes(array);

                array = removeFirst28Bytes(array);

                packetIndividualHeader[8] = size[0];
                packetIndividualHeader[9] = size[1];
                packetIndividualHeader[10] = size[2];
                packetIndividualHeader[11] = size[3];

                packetIndividualHeader[12] = size[0];
                packetIndividualHeader[13] = size[1];
                packetIndividualHeader[14] = size[2];
                packetIndividualHeader[15] = size[3];

                outputStreamWriter.Write(packetIndividualHeader, 0, packetIndividualHeader.Length);

                for (int j = 0; j < 3; j++)
                {
                    key64[j] = iEEEHeaderInformation[j + 24];
                    key128[j] = iEEEHeaderInformation[j + 24];
                }
                outputStreamWriter.Write(iEEEHeaderInformation, 0, 24);

                if (keysize == 64)
                {
                    for (int j = 3; j < 8; j++)
                    {
                        key64[j] = iVConcKey[j];
                    }
                    decryptedOutput = RC4.rc4encrypt(array, key64);
                    outputStreamWriter.Write(decryptedOutput, 0, decryptedOutput.Length - 4);
                }

                if (keysize == 128)
                {
                    for (int j = 3; j < 16; j++)
                    {
                        key128[j] = iVConcKey[j];
                    }
                    decryptedOutput = RC4.rc4encrypt(array, key128);
                    outputStreamWriter.Write(decryptedOutput, 0, decryptedOutput.Length);
                }
            }
            outputStreamWriter.Close();
            array = null;
            key64 = null;
            key128 = null;
            packetIndividualHeader = null;
            iEEEHeaderInformation = null;
            decryptedOutput = null;
        }

        /// <summary>
        /// Calculates the key with the FMS attack.
        /// </summary>
        private void fMS()
        {
            presentation.setKindOfAttack(1);
            //int lengthOfActualPacket = 0;
            //byte index1, index2, swapTempValue, firstByteOfKeyStream;
            int i, j, k, numberOfPacketsToBeRead;
            byte[] jj;
            byte[] s;
            byte[] si;
            byte io1, o1, io2, o2;
            byte sq, dq, kq, jq, q;
            byte s1;

            DateTime startTime = DateTime.Now;

            numberOfPacketsToBeRead = internalByteList.Count;

            for (k = 0; k < numberOfPacketsToBeRead; k++)
            {
                if (stop) { break; }
                byte[] array = internalByteList.ElementAt(k);
                attackedKeyByte = 0;
                bool packetUsed = false;
                iVConcKey = new byte[16];
                iVConcKey[0] = array[0];
                iVConcKey[1] = array[1];
                iVConcKey[2] = array[2];

                while (attackedKeyByte < 13)
                {
                    io1 = o1 = io2 = o2 = sq = dq = kq = jq = s1 = 0;
                    i = j = 3;
                    q = (byte)(attackedKeyByte + 3);

                    // if any byte of secret key is already calculated, it has to be concatenated
                    // to the key used here
                    if (attackedKeyByte > 0)
                    {
                        for (int l = 0; l <= attackedKeyByte; l++)
                        {
                            iVConcKey[l + 3] = (byte)findMaxVotedByte(0, l);
                        }
                    }

                    jj = new byte[256];
                    s = new byte[256];
                    si = new byte[256];

                    for (i = 0; i < 256; i++)
                    {
                        s[i] = (byte)i;
                        si[i] = (byte)i;
                    }

                    for (i = j = 0; i < q; i++)
                    {
                        j = (j + s[i] + iVConcKey[i % q]) & 0xFF;
                        jj[i] = (byte)j;
                        byte temp = s[i];
                        s[i] = s[j];
                        s[j] = temp;
                    }
                    i = q;
                    do
                    {
                        i--;
                        byte tmp = si[i];
                        si[i] = si[jj[i]];
                        si[jj[i]] = tmp;
                    }
                    while (i != 0);

                    o1 = (byte)(array[3] ^ 0xAA); io1 = si[o1]; s1 = s[1];

                    sq = s[q]; dq = (byte)(sq + jj[q - 1]);

                    // Examine if IV is weak, so check the conditions
                    // First two conditions are "classic" FMS,
                    // second two improvements done by "KoreK"
                    if ((s1 < q) && (((s1 + s[s1] - q) & 0xFF) == 0) && (io1 != 1) && (io1 != s[s1]))
                    {
                        kq = (byte)(io1 - dq);
                        fmsVotes[attackedKeyByte, kq]++;
                        packetUsed = true;
                    }
                    attackedKeyByte++;
                    permutation = null;
                }
                if (packetUsed) { usedPacktesCounter++; }
                counter++;
                //presentation.setNumberOfSniffedPackages(counter);
                presentation.setUsedIVs(usedPacktesCounter);
                if (packetUsed && (counter > 300.000))
                {
                    int resultTest = checkKey(iVConcKey);
                    if (resultTest == 1)
                    {
                        keysize = 64;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBox(fmsVotes, true, 40, usedPacktesCounter, totalDuration, "plugin", "FMS", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                    if (resultTest == 2)
                    {
                        keysize = 128;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBox(fmsVotes, true, 104, usedPacktesCounter, totalDuration, "plugin", "FMS", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                }
                iVConcKey = null;
                if (counter % 10000 == 0) { presentation.setTextBox(fmsVotes, false, 0, usedPacktesCounter, totalDuration, "plugin", "FMS", stop); }
                if ((!success) && (counter > 990000)) { ProgressChanged(990000, 1000000); }
                else { ProgressChanged(counter, 1000000); }
            }
            tempList.Clear();
            tempList = null;
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            totalDuration += (stopTime - startTime);
            if ((!stop) && (settings.FileOrNot) && (!success))
            {
                presentation.setTextBox(fmsVotes, false, 0, usedPacktesCounter, totalDuration, "file", "FMS", stop);
                GuiLogMessage("Couldn't recover key. Maybe you need more packets.", NotificationLevel.Info);
                GuiLogMessage("For a 104 bit key you usually need 3 - 4 mio. encrypted packets.", NotificationLevel.Info);
                success = true;
                OnPropertyChanged("Success");
                return;
            }
            GuiLogMessage("Read " + k.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used [h:min:sec]: " + duration + ".", NotificationLevel.Info);
            if ((stop) && (!settings.FileOrNot))
            {
                GuiLogMessage("Aborted! Time used for analysis [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
            }
            if ((stop) && (settings.FileOrNot))
            {
                GuiLogMessage("Aborted!", NotificationLevel.Info);
            }
            success = false;
            OnPropertyChanged("Success");
        }

        /// <summary>
        /// Calculates the key with the KoreK attacks.
        /// </summary>
        private void koreK()
        {
            presentation.setKindOfAttack(2);
            int i, j, l, numberOfPacketsToBeRead;

            byte[] jj;
            byte[] s;
            byte[] si;
            byte io1, o1, io2, o2;
            byte sq, dq, kq, jq, q;
            byte s1, s2, j2, t2;

            DateTime startTime = DateTime.Now;

            numberOfPacketsToBeRead = internalByteList.Count;

            for (l = 0; l < numberOfPacketsToBeRead; l++)
            {
                if (stop) { break; }
                byte[] array = internalByteList.ElementAt(l);
                attackedKeyByte = 0;

                iVConcKey = new byte[16];

                iVConcKey[0] = array[0];
                iVConcKey[1] = array[1];
                iVConcKey[2] = array[2];

                bool usedPacket = false;
                while (attackedKeyByte < 13)
                {
                    io1 = o1 = io2 = o2 = sq = dq = kq = jq = s1 = s2 = j2 = t2 = 0;
                    i = j = 3;
                    q = (byte)(attackedKeyByte + 3);
                    // if any byte of secret key is already calculated, it has to be concatenated
                    // to the key used here
                    if (attackedKeyByte > 0)
                    {
                        for (int k = 0; k <= attackedKeyByte; k++)
                        {
                            iVConcKey[k + 3] = (byte)findMaxVotedByte(1, k);
                        }
                    }

                    jj = new byte[256];
                    s = new byte[256];
                    si = new byte[256];

                    for (i = 0; i < 256; i++)
                    {
                        s[i] = (byte)i;
                        si[i] = (byte)i;
                    }

                    for (i = j = 0; i < q; i++)
                    {
                        j = (j + s[i] + iVConcKey[i % q]) & 0xFF;
                        jj[i] = (byte)j;
                        byte temp = s[i];
                        s[i] = s[j];
                        s[j] = temp;
                    }
                    i = q;
                    do
                    {
                        i--;
                        byte tmp = si[i];
                        si[i] = si[jj[i]];
                        si[jj[i]] = tmp;
                    }
                    while (i != 0);

                    o1 = (byte)(array[3] ^ 0xAA); io1 = si[o1]; s1 = s[1];
                    o2 = (byte)(array[4] ^ 0xAA); io2 = si[o2]; s2 = s[2];

                    sq = s[q]; dq = (byte)(sq + jj[q - 1]);

                    if (s2 == 0)
                    {
                        if ((s1 == 2) && (o1 == 2))
                        {
                            kq = (byte)(1 - dq);
                            if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                            kq = (byte)(2 - dq);
                            if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                            usedPacket = true;
                        }
                        else if (o2 == 0)
                        {
                            kq = (byte)(2 - dq);
                            if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                            usedPacket = true;
                        }
                    }
                    else
                    {
                        // A_u15
                        if ((o2 == 0) && (sq == 0))
                        {
                            kq = (byte)(2 - dq);
                            koreKVotes[attackedKeyByte, kq] += 15;
                            usedPacket = true;
                        }
                    }

                    if ((s1 == 1) && (o1 == s2))
                    {
                        kq = (byte)(1 - dq);
                        if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                        kq = (byte)(2 - dq);
                        if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                        usedPacket = true;
                    }
                    if ((s1 == 0) && (s[0] == 1) && (o1 == 1))
                    {
                        kq = (byte)(0 - dq);
                        if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq] -= 20; }
                        kq = (byte)(1 - dq);
                        if (koreKVotes[attackedKeyByte, kq] >= 20) { koreKVotes[attackedKeyByte, kq]--; }
                        usedPacket = true;
                    }

                    if (s1 == q)
                    {
                        // A_s13
                        if (o1 == q)
                        {
                            kq = (byte)(si[0] - dq);
                            koreKVotes[attackedKeyByte, kq] += 13;
                            usedPacket = true;
                        }
                        // A_u13_1
                        else if (((1 - q - o1) & 0xFF) == 0)
                        {
                            kq = (byte)(io1 - dq);
                            koreKVotes[attackedKeyByte, kq] += 12;
                            usedPacket = true;
                        }
                        // A_u5_1
                        else if (io1 < q)
                        {
                            jq = si[(io1 - q) & 0xFF];
                            if (jq != 1)
                            {
                                kq = (byte)(jq - dq);
                                koreKVotes[attackedKeyByte, kq] += 3;
                                usedPacket = true;
                            }
                        }
                    }
                    // A_u5_2
                    if ((io1 == 2) && (s[q] == 1))
                    {
                        kq = (byte)(1 - dq);
                        koreKVotes[attackedKeyByte, kq] += 4;
                        usedPacket = true;
                    }

                    if (s[q] == q)
                    {
                        // A_u13_2
                        if ((s1 == 0) && (o1 == q))
                        {
                            kq = (byte)(1 - dq);
                            koreKVotes[attackedKeyByte, kq] += 12;
                            usedPacket = true;
                        }
                        // A_u13_3
                        else if ((((1 - q - s1) & 0xFF) == 0) && (o1 == s1))
                        {
                            kq = (byte)(1 - dq);
                            koreKVotes[attackedKeyByte, kq] += 12;
                            usedPacket = true;
                        }
                        // A_u5_3
                        else if ((s1 >= ((-q) & 0xFF)) && (((q + s1 - io1) & 0xFF) == 0))
                        {
                            kq = (byte)(1 - dq);
                            koreKVotes[attackedKeyByte, kq] += 3;
                            usedPacket = true;
                        }
                    }

                    // A_s5_1
                    if ((s1 < q) && (((s1 + s[s1] - q) & 0xFF) == 0) && (io1 != 1) && (io1 != s[s1]))
                    {
                        kq = (byte)(io1 - dq);
                        koreKVotes[attackedKeyByte, kq] += 5;
                        usedPacket = true;
                    }

                    if ((s1 > q) && (((s2 + s1 - q) & 0xFF) == 0))
                    {
                        // A_s5_2
                        if (o2 == s1)
                        {
                            jq = si[(s1 - s2) & 0xFF];
                            if ((jq != 1) && (jq != 2))
                            {
                                kq = (byte)(jq - dq);
                                koreKVotes[attackedKeyByte, kq] += 5;
                                usedPacket = true;
                            }
                        }
                        // A_s5_3
                        else if (o2 == ((2 - s2) & 0xFF))
                        {
                            jq = io2;
                            if ((jq != 1) && (jq != 2))
                            {
                                kq = (byte)(jq - dq);
                                koreKVotes[attackedKeyByte, kq] += 5;
                                usedPacket = true;
                            }
                        }
                    }

                    // A_s3
                    if ((s[1] != 2) && (s[2] != 0))
                    {
                        j2 = (byte)(s[1] + s[2]);
                        if (j2 < q)
                        {
                            t2 = (byte)(s[j2] + s[2]);
                            if ((t2 == q) && (io2 != 1) && (io2 != 2) && (io2 != j2))
                            {
                                kq = (byte)(io2 - dq);
                                koreKVotes[attackedKeyByte, kq] += 3;
                                usedPacket = true;
                            }
                        }
                    }

                    if (s[1] == 2)
                    {
                        if (q == 4)
                        {
                            // A_4s13
                            if (o2 == 0)
                            {
                                kq = (byte)(si[0] - dq);
                                koreKVotes[attackedKeyByte, kq] += 13;
                                usedPacket = true;
                            }
                            else
                            {
                                // A_4_u5_1
                                if ((jj[1] == 2) && (io2 == 0))
                                {
                                    kq = (byte)(si[254] - dq);
                                    koreKVotes[attackedKeyByte, kq] += 4;
                                    usedPacket = true;
                                }
                                // A_4_u5_2
                                if ((jj[1] == 2) && (io2 == 2))
                                {
                                    kq = (byte)(si[255] - dq);
                                    koreKVotes[attackedKeyByte, kq] += 4;
                                    usedPacket = true;
                                }
                            }
                        }
                        // A_u5_4
                        else if ((q > 4) && ((s[4] + 2) == q) && (io2 != 1) && (io2 != 4))
                        {
                            kq = (byte)(io2 - dq);
                            koreKVotes[attackedKeyByte, kq] += 4;
                            usedPacket = true;
                        }
                    }

                    s = si = jj = null;
                    attackedKeyByte++;
                }
                if (usedPacket) { usedPacktesCounter++; }
                counter++;
                //presentation.setNumberOfSniffedPackages(counter);
                presentation.setUsedIVs(usedPacktesCounter);
                if (usedPacket && (counter > 300000))
                {
                    int resultTest = checkKey(iVConcKey);
                    if (resultTest == 1)
                    {
                        keysize = 64;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBox(koreKVotes, true, 40, usedPacktesCounter, totalDuration, "plugin", "KoreK", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                    if (resultTest == 2)
                    {
                        keysize = 128;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBox(koreKVotes, true, 104, usedPacktesCounter, totalDuration, "plugin", "KoreK", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                }
                if (counter % 10000 == 0) { presentation.setTextBox(koreKVotes, false, 0, usedPacktesCounter, totalDuration, "plugin", "KoreK", stop); }
                if ((!success) && (counter > 1900000)) { ProgressChanged(490000, 500000); }
                else { ProgressChanged(counter, 2000000); }
                iVConcKey = null;
            }
            tempList.Clear();
            tempList = null;
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            totalDuration += (stopTime - startTime);
            if ((!stop) && (settings.FileOrNot) && (!success))
            {
                presentation.setTextBox(koreKVotes, false, 0, usedPacktesCounter, totalDuration, "file", "KoreK", stop);
                GuiLogMessage("Couldn't recover key. Maybe you need more packets.", NotificationLevel.Info);
                GuiLogMessage("For a 104 bit key you usually need 500.000 - 1 mio encrypted packets.", NotificationLevel.Info);
                success = true;
                OnPropertyChanged("Success");
                return;
            }
            GuiLogMessage("Read " + l.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used [h:min:sec]: " + duration + ".", NotificationLevel.Info);
            if ((stop) && (!settings.FileOrNot))
            {
                GuiLogMessage("Aborted! Time used for analysis [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
            }
            if ((stop) && (settings.FileOrNot))
            {
                GuiLogMessage("Aborted!", NotificationLevel.Info);
            }
            success = false;
            OnPropertyChanged("Success");
        }

        /// <summary>
        /// Calculates the key with the PTW attack.
        /// </summary>
        private void pTW()
        {
            presentation.setKindOfAttack(3);
            int keybyteSum = 0;
            int numberOfPacketsToBeRead;
            int j;
            byte index1, index2, swapTempValue;
            byte[] keystreamValues = new byte[16];
            Random rnd;

            DateTime startTime = DateTime.Now;

            numberOfPacketsToBeRead = internalByteList.Count;

            for (j = 0; j < numberOfPacketsToBeRead; j++)
            {
                if (stop) { break; }
                byte[] array = internalByteList.ElementAt(j);
                index1 = index2 = swapTempValue = 0;

                attackedKeyByte = 0;

                iVConcKey = new byte[16];

                iVConcKey[0] = array[0];
                iVConcKey[1] = array[1];
                iVConcKey[2] = array[2];

                keystreamValues[0] = (byte)(array[3] ^ 0xAA);
                keystreamValues[1] = (byte)(array[4] ^ 0xAA);
                keystreamValues[2] = (byte)(array[5] ^ 0x03);
                keystreamValues[3] = (byte)(array[6] ^ 0x00);
                keystreamValues[4] = (byte)(array[7] ^ 0x00);
                keystreamValues[5] = (byte)(array[8] ^ 0x00);
                keystreamValues[6] = (byte)(array[9] ^ 0x08);
                if (makeIntFromTwoBytes(new byte[] { array[19], array[20] }) == 102)
                {
                    keystreamValues[7] = (byte)(array[10] ^ 0x06);
                    keystreamValues[8] = (byte)(array[11] ^ 0x00);
                    keystreamValues[9] = (byte)(array[12] ^ 0x01);
                    keystreamValues[10] = (byte)(array[13] ^ 0x08);
                    keystreamValues[11] = (byte)(array[14] ^ 0x00);
                    keystreamValues[12] = (byte)(array[15] ^ 0x06);
                    keystreamValues[13] = (byte)(array[16] ^ 0x04);
                    keystreamValues[14] = (byte)(array[17] ^ 0x00);
                    // This value is every times 01, 02 would be ARP response,
                    // hose packages are not provided here...
                    keystreamValues[15] = (byte)(array[18] ^ 0x01);
                }
                else
                {
                    keystreamValues[7] = (byte)(array[10] ^ 0x00);
                    keystreamValues[8] = (byte)(array[11] ^ 0x45);
                    keystreamValues[9] = (byte)(array[12] ^ 0x00);
                    // size of IP packet is saved in array at the two last positions
                    keystreamValues[10] = (byte)(array[13] ^ array[20]);
                    keystreamValues[11] = (byte)(array[14] ^ array[19]);
                    rnd = new Random();
                    keystreamValues[12] = (byte)(array[15] ^ rnd.Next(256));
                    keystreamValues[13] = (byte)(array[16] ^ rnd.Next(256));
                    rnd = null;
                    keystreamValues[14] = (byte)(array[17] ^ 0x40);
                }

                permutation = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    permutation[i] = (byte)i;
                }

                for (int i = 0; i < attackedKeyByte + 3; i++)
                {
                    index2 = (byte)(index2 + permutation[index1] + iVConcKey[index1 % 3]);
                    swapTempValue = permutation[index1];
                    permutation[index1] = permutation[index2];
                    permutation[index2] = swapTempValue;
                    index1++;
                }
                while (attackedKeyByte < 13)
                {
                    if (attackedKeyByte == 0)
                    {
                        iVConcKey[3] = (byte)findMaxVotedByte(2, 0);
                    }
                    if (attackedKeyByte > 0)
                    {
                        for (int i = 1; i <= attackedKeyByte; i++)
                        {
                            iVConcKey[i + 3] = (byte)((findMaxVotedByte(2, i) - findMaxVotedByte(2, i - 1)) & 0xFF);
                        }
                    }

                    byte sum = 0;
                    for (int i = 3; i <= attackedKeyByte + 3; i++)
                    {
                        sum = (byte)(sum + permutation[i]);
                    }
                    keybyteSum = ((Array.IndexOf(permutation, (byte)((((3 + attackedKeyByte) - keystreamValues[2 + attackedKeyByte]) & 0xFF))) - (index2 + sum)) & 0xFF);
                    ptwVotes[attackedKeyByte, keybyteSum]++;
                    attackedKeyByte++;
                }
                counter++;
                usedPacktesCounter++;
                //presentation.setNumberOfSniffedPackages(counter);
                presentation.setUsedIVs(usedPacktesCounter);
                if (counter > 20000)
                {
                    int resultTest = checkKey(iVConcKey);
                    if ((resultTest == 1))
                    {
                        keysize = 64;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBoxPTW(ptwVotes, true, 40, counter, totalDuration, "plugin", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                    if (resultTest == 2)
                    {
                        keysize = 128;
                        DateTime stopTimeAfterTest = DateTime.Now;
                        totalDuration += (stopTimeAfterTest - startTime);
                        presentation.setTextBoxPTW(ptwVotes, true, 104, counter, totalDuration, "plugin", stop);
                        GuiLogMessage("Found possible key after reading " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                        success = true;
                        outputStreamWriter = new CStreamWriter();
                        decrypt();
                        OnPropertyChanged("Success");
                        OnPropertyChanged("OutputStream");
                        ProgressChanged(1, 1);
                        return;
                    }
                }
                presentation.setTextBoxPTW(ptwVotes, false, 0, counter, totalDuration, "plugin", stop);
                if ((!success) && (counter > 399999))
                {
                    DateTime stopTimeAfterTest = DateTime.Now;
                    totalDuration += (stopTimeAfterTest - startTime);
                    presentation.setTextBoxPTW(ptwVotes, false, 900, counter, totalDuration, "plugin", stop);
                    GuiLogMessage("Couldn't recover key. Propably it is a strong key.", NotificationLevel.Info);
                    GuiLogMessage("Read " + counter.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used (in total) [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
                    success = true;
                    OnPropertyChanged("Success");
                    return;
                }
                else { ProgressChanged(counter, 399999); }
            }
            keystreamValues = null;
            rnd = null;
            tempList.Clear();
            tempList = null;
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            totalDuration += (stopTime - startTime);
            if ((!stop) && (settings.FileOrNot) && (!success))
            {
                presentation.setTextBoxPTW(ptwVotes, false, 0, usedPacktesCounter, totalDuration, "file", stop);
                GuiLogMessage("Couldn't recover key. Maybe you need more packets.", NotificationLevel.Info);
                int number = 100000;
                GuiLogMessage("For a 104 bit key you usually need " + number.ToString("#,#", CultureInfo.InstalledUICulture) + " ARP packets or a few more IP packets.", NotificationLevel.Info);
                success = true;
                OnPropertyChanged("Success");
                return;
            }
            GuiLogMessage("Read " + j.ToString("#,#", CultureInfo.InstalledUICulture) + " packets. Time used [h:min:sec]: " + duration + ".", NotificationLevel.Info);
            if ((stop) && (!settings.FileOrNot))
            {
                GuiLogMessage("Aborted! Time used for analysis [h:min:sec]: " + totalDuration + ".", NotificationLevel.Info);
            }
            if ((stop) && (settings.FileOrNot))
            {
                GuiLogMessage("Aborted!", NotificationLevel.Info);
            }
            success = false;
            OnPropertyChanged("Success");
        }

        /// <summary>
        /// Checks if the key is correct.
        /// Returns: 0 if key is wrong, 1 if key is correct && of length 40 bit,
        /// 2 if key is correct && of length 104 bit.
        /// </summary>
        /// <param name="key">Actual key to be tested.</param>
        /// <param name="ks">Key stream sniffed from outside.</param>
        /// <returns>An integer value indicating if key is correct and length of key.</returns>
        private int checkKey(byte[] key)
        {
            int correctKey = 0;

            int badShortKey = 0;
            int badLongKey = 0;

            byte index1;
            byte index2;
            byte swapTempValue;
            byte x1;
            byte x2;

            int keylength = 16;

            byte[] internalKey = new byte[keylength];
            for (int k = 0; k < 30; k++)
            {
                byte[] check = internalByteList.ElementAt(k);

                for (int i = 0; i < keylength; i++)
                {
                    if (i < 3) { internalKey[i] = check[i]; }
                    if (i >= 3) { internalKey[i] = key[i]; }
                }

                for (int j = 0; j < 2; j++)
                {
                    index1 = 0;
                    index2 = 0;
                    swapTempValue = 0;

                    if (j == 0) { keylength = 8; }
                    if (j == 1) { keylength = 16; }

                    byte[] s = new byte[256];

                    for (int i = 0; i < 256; i++) { s[i] = (byte)i; }
                    for (int i = 0; i < 256; i++)
                    {
                        index2 = (byte)(index2 + s[index1] + internalKey[index1 % keylength]);
                        swapTempValue = s[index1];
                        s[index1] = s[index2];
                        s[index2] = swapTempValue;
                        index1++;
                    }

                    index1 = 0;
                    index2 = 0;
                    swapTempValue = 0;


                    index1 = 1; index2 = (byte)(0 + s[index1]);
                    swapTempValue = s[index1];
                    s[index1] = s[index2];
                    s[index2] = swapTempValue;


                    x1 = (byte)(check[3] ^ (s[(s[index1] + s[index2]) & 0xFF]));

                    index1 = 2; index2 = (byte)(index2 + s[index1]);
                    swapTempValue = s[index1];
                    s[index1] = s[index2];
                    s[index2] = swapTempValue;

                    x2 = (byte)(check[4] ^ (s[(s[index1] + s[index2]) & 0xFF]));

                    if ((x1 != 0xAA || x2 != 0xAA) &&
                        (x1 != 0xE0 || x2 != 0xE0) &&
                        (x1 != 0x42 || x2 != 0x42))
                    {
                        if (j == 0)
                        {
                            badShortKey++;
                        }
                        if (j == 1)
                        {
                            badLongKey++;
                        }
                    }
                    if ((badShortKey == 0) && (k == 29)) { correctKey = 1; }
                    if ((badLongKey == 0) && (k == 29)) { correctKey = 2; }
                    permutation = null;
                }
            }
            return correctKey;
        }

        #endregion

        #region IPlugin Member

        public void Dispose()
        {
            try
            {
                stop = false;
                success = false;
                inputStream = null;
                outputStreamWriter = null;
                if (tempList != null) { tempList.Clear(); }
                tempList = null;
                if (internalByteList != null) { internalByteList.Clear(); }
                internalByteList = null;

                counter = 0;
                usedPacktesCounter = 0;
                attackedKeyByte = 0;
                keysize = 0;
                presentation.setKindOfAttack(0);
                presentation.setNumberOfSniffedPackages(0);
                presentation.setUsedIVs(int.MaxValue);
                presentation.resetTextBox(string.Empty);
                totalDuration = TimeSpan.Zero;
                fmsVotes = null;
                koreKVotes = null;
                ptwVotes = null;
                iVConcKey = null;
                permutation = null;
                rnd = null;
            }
            catch (Exception exc)
            {
                GuiLogMessage(exc.Message, NotificationLevel.Error);
            }
            stop = false;
        }

        public void Execute()
        {
            try
            {
                internalByteList = new List<byte[]>();
                if (inputStream == null || inputStream.Length == 0)
                {
                    return;
                }

                using (CStreamReader reader = inputStream.CreateReader())
                {
                    reader.WaitEof(); // does not support chunked streaming

                    tempList = loadList(reader);

                    init();
                    extractHeaderInformation();
                    switch (settings.Action)
                    {
                        case 0: fMS(); break;
                        case 1: koreK(); break;
                        case 2: pTW(); break;
                        default: break;
                    }
                    if (stop)
                    {
                        presentation.setKindOfAttack(0);
                        presentation.setNumberOfSniffedPackages(0);
                        presentation.setUsedIVs(int.MaxValue);
                    }
                }
            }
            catch (Exception exc)
            {
                GuiLogMessage(exc.Message, NotificationLevel.Error);
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

        public UserControl Presentation => presentation;

        public ISettings Settings
        {
            get => settings;
            set => settings = (WEPAttacksSettings)value;
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
