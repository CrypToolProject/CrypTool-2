using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;


namespace CrypTool.Internet_frame_generator
{
    /// <summary>
    /// Creates internet traffic packages (IPv4). Either IP packages (ISO/OSI layer 3) or ARP request packages
    /// (ISO/OSI layer 2).
    /// </summary>
    [Author("Stefan Schröder",
        "stef.schroeder@gmx.de",
        "Uni Siegen",
        "http://www.uni-siegen.de")]
    [PluginInfo("Internet_frame_generator.Properties.Resources",
        "PluginCaption",
        "PluginTooltip",
        "Internet_frame_generator/DetailedDescription/doc.xml",
        "Internet_frame_generator/icon.jpg")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class Internet_frame_generator : ICrypComponent
    {
        #region private variables

        private Internet_frame_generatorSettings settings;
        private int inputInt;
        private CStreamWriter outputStream;
        private bool stop = false;
        private Random rnd;
        private int packetCounter;

        private static readonly byte[] header = new byte[] { 0xD4, 0xC3, 0xB2, 0xA1, 0x02, 0x00, 0x04, 0x00,
                                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                            0x60, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00};

        [PropertyInfo(Direction.InputData,
            "InputIntCaption",
            "InputIntTooltip",
            false)]
        public int InputInt
        {
            get => inputInt;
            set
            {
                inputInt = value;
                OnPropertyChanged("InputInt");
            }
        }

        [PropertyInfo(Direction.OutputData,
            "OutputStreamCaption",
            "OutputStreamTooltip",
            false)]
        public ICrypToolStream OutputStream
        {
            get => outputStream;
            set
            {
            }
        }

        public Random Rnd
        {
            get => rnd;
            set {  /*readonly */ }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Converts an int number in a byte value. Needed for IP packet values, which are
        /// of sizes 2 bytes.
        /// </summary>
        /// <param name="intValueToConvert">The number to convert into a byte value.</param>
        /// <returns>An array of byte values.</returns>
        private byte[] intToTwoBytes(int intValueToConvert)
        {
            return new byte[] { (byte)(intValueToConvert >> 8), (byte)intValueToConvert };
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
        /// Converts an UInt16 value into a two byte array.
        /// </summary>
        /// <param name="value">The UInt16 value to be converted into two bytes.</param>
        /// <returns>The byte array containing two byte values.</returns>
        private static byte[] get2BytesFromInt(ushort value)
        {
            return new byte[] { (byte)(value), (byte)(value >> 8) };
        }

        /// <summary>
        /// Generates IP packets of variable size.
        /// First a byte array is used to build the package, which is then
        /// converted into a CrypToolStream for output. 
        /// </summary>
        private byte[] generateIPPackets()
        {
            // Contents of IP packets (header): have a look at http://de.wikipedia.org/wiki/IP-Paket

            int size = 90;
            // Temp byte for the packet
            byte[] iPPaket = new byte[size];
            // Packet size and other information
            iPPaket[0] = 0x0D;
            iPPaket[1] = 0x12;
            iPPaket[2] = 0xC9;
            iPPaket[3] = 0x48;
            iPPaket[4] = 0x78;
            iPPaket[5] = 0x70;
            iPPaket[6] = 0x01;
            iPPaket[7] = 0x00;
            // size of sniffed packet, used for looking in packet with Wireshark
            iPPaket[8] = 0x4A;
            iPPaket[9] = 0x00;
            iPPaket[10] = 0x00;
            iPPaket[11] = 0x00;
            // size
            iPPaket[12] = 0x4A;
            iPPaket[13] = 0x00;
            iPPaket[14] = 0x00;
            iPPaket[15] = 0x00;

            /* ###################################################################
             * MAC address of destination
             * ###################################################################
             * 
             */
            iPPaket[16] = 0x00;
            iPPaket[17] = 0x12;
            iPPaket[18] = 0xBF;
            iPPaket[19] = 0xDC;
            iPPaket[20] = 0x4E;
            iPPaket[21] = 0x7A;

            /* ###################################################################
             * MAC address of source
             * ###################################################################
             * 
             * Here: 00:84:f4:8a:d3:5a
             */
            iPPaket[22] = 0x00;
            iPPaket[23] = 0xA0;
            iPPaket[24] = 0xD1;
            iPPaket[25] = 0x25;
            iPPaket[26] = 0xB9;
            iPPaket[27] = 0xEC;

            /* ###################################################################
             * Type
             * ###################################################################
             *
             * Type is IP, so value is (HEX) 08 00
             */
            iPPaket[28] = 0x08;
            iPPaket[29] = 0x00;

            /* ###################################################################
             * Version & IHL
             * ###################################################################
             *
             * Bits 00 - 03: version (IPv4 ==> 4(DEZ) == 0100 (BIN))
             * Bits 04 - 07: IHL (IP header length ==> 5 (DEZ) == 0101 (BIN))
             * (IHL is coded as an factor of 32 ==> 5 * 32 = 160)
             * If IHL > 5 there must be some values in field "Options and Padding"!
             * So first byte is concatenation of 4 and 5: 01000101 (BIN) == 69 (DEZ)
             */
            iPPaket[30] = 0x45;

            /* ###################################################################
             * TOS
             * ###################################################################
             * 
             * Bits 08 - 15: TOS (type of service)
             * Within this byte:
             * Bits 00 - 05: DSCP (differentiated services code point
             * Bits 06 - 07: explicit congestion notification
             */
            iPPaket[31] = 0x00;
            /* ###################################################################
             * Total length
             * ###################################################################
             * 
             * Bits 16 - 31: total length of packet including header == size
             * (that's why upper limit == 65.535 bytes = (2 ^ 16) - 1....)
             */

            iPPaket[32] = 0x00;
            iPPaket[33] = 0x3C;
            /* ###################################################################
             * Identification
             * ###################################################################
             * 
             * Bits 00 - 15: Identification for fragmentation matters.
             */
            iPPaket[34] = 0x11;
            iPPaket[35] = 0x2B;
            /* ###################################################################
             * First three bits : Flags
             * Last five bits: Fragment offset
             * ###################################################################
             * 
             * Flags:
             * Bit 00: reserved, have to be 0
             * Bit 01: 0 = fragmentation allowed, 1 = fragmentation not allowed
             * Bit 02: 0 = last fragment, 1 = there are more fragments to be transmitted
             * 
             * End of flags & beginning of fragment offset:
             * Bits 00 - 12: Number, which indicates the position of the packet within
             *               the fragmentation
             * Because of no fragmentation beeing realized here, there is also no fragmentation
             * offset needed.
             */
            iPPaket[36] = 0x40;
            iPPaket[37] = 0x00;
            /* ###################################################################
             * TTL
             * ###################################################################
             * 
             * TTL (Time to live)
             * Here: 64
             */
            iPPaket[38] = 0x40;
            /* ###################################################################
             * Protocol
             * ###################################################################
             * 
             * Indicates the protocol of the transmitted user data (e.g. 6 for TCP or
             * 17 for UDP). Here, this value is 6. Kind of traffic doesn't matter
             * for our purposes.
             */
            iPPaket[39] = 0x06;
            /* ###################################################################
             * Header Checksum
             * ###################################################################
             * 
             * "A checksum computed over the header to provide basic  protection
             * against corruption in transmission. This is not the more complex CRC
             * code typically used by data link layer technologies such  as
             * Ethernet; it's just a 16-bit checksum. It is calculated by dividing
             * the header bytes into words (a word is two bytes) and then adding them
             * together. The data is not checksummed, only the header. At each hop
             * the device receiving the datagram does the same checksum calculation
             * and on a mismatch, discards the datagram as damaged."
             * [http://www.tcpipguide.com/free/t_IPDatagramGeneralFormat.htm]
             */
            iPPaket[40] = 0x97;
            iPPaket[41] = 0xF8;

            /* ###################################################################
             * Source Adress
             * ###################################################################
             * 
             * IP Adress of sender of the packet.
             */
            iPPaket[42] = 0xC0;
            iPPaket[43] = 0xA8;
            iPPaket[44] = 0x02;
            iPPaket[45] = 0x66;
            /* ###################################################################
             * Destination Adress
             * ###################################################################
             * 
             * IP address of receiver of the packet.
             */
            iPPaket[46] = 0xC3;
            iPPaket[47] = 0x47;
            iPPaket[48] = 0x0B;
            iPPaket[49] = 0x43;

            /* ###################################################################
             * Options and Padding
             * ###################################################################
             * 
             * Additional information, size must be a factor of 32. Because of the
             * size of IHL (4 bit) this field is limited to 40 bytes. This field is
             * optional. Here, there are no informition added.
             */

            /*###################################################################
             * Rest of packet (TCP data)
             * ###################################################################
             * 
             */
            // Source port
            iPPaket[50] = 0xE5;
            iPPaket[51] = 0xD3;
            // Destination port
            iPPaket[52] = 0x00;
            iPPaket[53] = 0x50;
            // Sequence number
            iPPaket[54] = 0x83;
            iPPaket[55] = 0x74;
            iPPaket[56] = 0xA4;
            iPPaket[57] = 0xB9;
            iPPaket[58] = 0x00;
            iPPaket[59] = 0x00;
            iPPaket[60] = 0x00;
            iPPaket[61] = 0x00;
            iPPaket[62] = 0xA0;
            iPPaket[63] = 0x02;
            iPPaket[64] = 0x16;
            iPPaket[65] = 0xD0;
            iPPaket[66] = 0xDB;
            iPPaket[67] = 0x58;
            iPPaket[68] = 0x00;
            iPPaket[69] = 0x00;
            iPPaket[70] = 0x02;
            iPPaket[71] = 0x04;
            iPPaket[72] = 0x05;
            iPPaket[73] = 0xB4;
            iPPaket[74] = 0x04;
            iPPaket[75] = 0x02;
            iPPaket[76] = 0x08;
            iPPaket[77] = 0x0A;
            iPPaket[78] = 0x00;
            iPPaket[79] = 0x5B;
            iPPaket[80] = 0xB5;
            iPPaket[81] = 0x92;
            iPPaket[82] = 0x00;
            iPPaket[83] = 0x00;
            iPPaket[84] = 0x00;
            iPPaket[85] = 0x00;
            iPPaket[86] = 0x01;
            iPPaket[87] = 0x03;
            iPPaket[88] = 0x03;
            iPPaket[89] = 0x06;
            return iPPaket;
        }

        /// <summary>
        /// Generates ARP request packets. The MAC address for an IP random address is requested.
        /// </summary>
        /// <returns></returns>
        private byte[] generateARPRequestPackets()
        {
            // Contents of ARP packet header: see http://de.wikipedia.org/wiki/Address_Resolution_Protocol

            // Size of ARP paket is 7 * 32 bits = 224 bits = 28 bytes
            // additional header information brings it up to 99 bytes
            int size = 76;
            byte[] aRPPaket = new byte[size];
            // Temp byte array
            byte[] tmp;

            aRPPaket[0] = 0x01;
            aRPPaket[1] = 0x28;
            aRPPaket[2] = 0xC9;
            aRPPaket[3] = 0x48;
            aRPPaket[4] = 0xC0;
            aRPPaket[5] = 0x10;
            aRPPaket[6] = 0x0D;
            aRPPaket[7] = 0x00;

            tmp = get4BytesFromInt(size - 16);
            aRPPaket[8] = tmp[0];
            aRPPaket[9] = tmp[1];
            aRPPaket[10] = tmp[2];
            aRPPaket[11] = tmp[3];

            aRPPaket[12] = tmp[0];
            aRPPaket[13] = tmp[1];
            aRPPaket[14] = tmp[2];
            aRPPaket[15] = tmp[3];

            tmp = null;

            aRPPaket[16] = 0xFF;
            aRPPaket[17] = 0xFF;
            aRPPaket[18] = 0xFF;
            aRPPaket[19] = 0xFF;
            aRPPaket[20] = 0xFF;
            aRPPaket[21] = 0xFF;

            aRPPaket[22] = 0x00;
            aRPPaket[23] = 0x12;
            aRPPaket[24] = 0xBF;
            aRPPaket[25] = 0xDB;
            aRPPaket[26] = 0x5F;
            aRPPaket[27] = 0x7A;

            aRPPaket[28] = 0x08;
            aRPPaket[29] = 0x06;

            /* ###################################################################
             * Hardware Address Type
             * ###################################################################
             * 
             * In case of Ethernet this field is 1
             */
            aRPPaket[30] = 0x00;
            aRPPaket[31] = 0x01;

            /* ###################################################################
             * Protocol Type
             * ###################################################################
             * 
             * Contains protocol type of protocol, for which MAC address is requested
             * (in case of IPv4 like in this scenario: 0x0800 (2048)
             */
            aRPPaket[32] = 0x08;
            aRPPaket[33] = 0x00;

            /* ###################################################################
             * Hardware Address Size
             * ###################################################################
             * 
             * Contains size of hardware address (in case of Ethernet = 6)
             */
            aRPPaket[34] = 0x06;

            /* ###################################################################
             * Protocoll Adress Size
             * ###################################################################
             * 
             * Contains the size of protocol (in case of IPv4: 4)
             */
            aRPPaket[35] = 0x04;

            /* ###################################################################
             * Operation
             * ###################################################################
             * 
             * Contains the value for the operation requested to be operated
             * (1 = ARP request, 2 = ARP Response)
             * In this szenario for simplicity only request are generated.
             */
            aRPPaket[36] = 0x00;
            aRPPaket[37] = 0x01;

            /* ###################################################################
             * Source MAC Address
             * ###################################################################
             * 
             * Within an ARP request message this is the MAC address of the sender.
             * Here, some pseudo random value is used.
             */
            for (int i = 38; i < 44; i++)
            {
                aRPPaket[i] = (byte)rnd.Next(255);
            }

            /* ###################################################################
             * Source IP Address
             * ###################################################################
             * 
             * IP address of sender of the message.
             * In this scenario, sender has always IP 220.156.73.198.
             */
            aRPPaket[44] = 220;
            aRPPaket[45] = 156;
            aRPPaket[46] = 73;
            aRPPaket[47] = 198;

            /* ###################################################################
             * Destination MAC Address
             * ###################################################################
             * 
             * MAC Address of requesting host in a ARP response scenario,
             * in a ARP request scenario undefined.
             */
            for (int i = 48; i < 54; i++)
            {
                aRPPaket[i] = 0;
            }

            /* ###################################################################
             * Destination IP Address
             * ###################################################################
             * 
             * In case of ARP request this field contains IP address of host, which
             * MAC address is requested, in case of ARP response it contains the
             * IP address of requesting host.
             * In this scenario the searched host has always IP address 75.178.46.215.
             */
            aRPPaket[54] = 75;
            aRPPaket[55] = 178;
            aRPPaket[56] = 46;
            aRPPaket[57] = 215;
            return aRPPaket;
        }

        #endregion

        #region IPlugin Member

        public void Dispose()
        {
            try
            {
                inputInt = 0;
                packetCounter = 0;
                stop = false;
                outputStream = null;
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
                if (settings.NumberOfPacketsToBeCreated == 0)
                {
                    GuiLogMessage("ERROR. No number of packets to be created given. Aborting now.", NotificationLevel.Error);
                    return;
                }
                rnd = new Random();
                string successMessage = string.Empty;
                packetCounter = 0;
                outputStream = new CStreamWriter();
                outputStream.Write(header, 0, header.Length);
                // IPv4 packets are requested
                if (settings.Action == 0)
                {
                    DateTime startTime = DateTime.Now;
                    for (int i = 0; i < settings.NumberOfPacketsToBeCreated; i++)
                    {
                        if (stop)
                        {
                            break;
                        }
                        Progress(i, settings.NumberOfPacketsToBeCreated);
                        byte[] tmp = generateIPPackets();
                        outputStream.Write(tmp, 0, tmp.Length);
                        packetCounter++;
                    }
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (packetCounter == 1) { successMessage = "Successfully created " + packetCounter + " IPv4 packet."; }
                    else { successMessage = "Successfully created " + packetCounter.ToString("#,#", CultureInfo.InstalledUICulture) + " IPv4 packets."; }
                    if (!stop)
                    {
                        GuiLogMessage(successMessage, NotificationLevel.Info);
                        GuiLogMessage("Time used [h:min:sec]: " + duration, NotificationLevel.Info);
                        outputStream.Close();
                        OnPropertyChanged("OutputStream");
                    }
                }
                // ARP packets are requested
                if (settings.Action == 1)
                {
                    DateTime startTime = DateTime.Now;
                    for (int i = 0; i < settings.NumberOfPacketsToBeCreated; i++)
                    {
                        if (stop)
                        {
                            break;
                        }
                        Progress(i, settings.NumberOfPacketsToBeCreated);
                        byte[] tmp = generateARPRequestPackets();
                        //outputList.Add(tmp);
                        outputStream.Write(tmp, 0, tmp.Length);
                        packetCounter++;
                    }
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (packetCounter == 1) { successMessage = "Successfully created " + packetCounter + " ARP request packet."; }
                    else { successMessage = "Successfully created " + packetCounter.ToString("#,#", CultureInfo.InstalledUICulture) + " ARP request packets."; }
                    if (!stop)
                    {
                        GuiLogMessage(successMessage, NotificationLevel.Info);
                        GuiLogMessage("Time used [h:min:sec]: " + duration, NotificationLevel.Info);
                        outputStream.Close();
                        OnPropertyChanged("OutputStream");
                    }
                }
                if (stop)
                {
                    outputStream.Close();
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            catch (Exception exc)
            {
                GuiLogMessage(exc.Message, NotificationLevel.Error);
            }
            finally
            {
                Progress(1, 1);
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
        private void Progress(double value, double max)
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
            set => settings = (Internet_frame_generatorSettings)value;
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

        #region Public interface

        /// <summary>
        /// Constructor. Creates an instance of <see cref="IPPacketGenerator"/>IPPacketGenrator.
        /// </summary>
        public Internet_frame_generator()
        {
            settings = new Internet_frame_generatorSettings();
            //this.presentation = new Internet_frame_generator_Presentation();
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
    }
}
