using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
namespace CrypTool.Plugins.GrainV0.Chipher
{
    //Information about the author
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("GrainV0.Properties.Resources", "GrainV0", "GrainV0 algorithm generates keystream of defined length", "GrainV0Attack/userdoc.xml", new[] { "GrainV0/Images/grain.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]


    //Class
    public class GrainV0 : ICrypComponent
    {


        private const int KEY_SIZE_BYTES = 10;
        private const int IV_SIZE_BYTES = 8;
        private const int REGISTERS_SIZE_BITS = 80;
        private int[] nfsr;
        private int[] lfsr;
        private byte[] key;
        private byte[] iv;
        private byte[] keystream;
        private int keystreamLength;

        //settings field 
        private GrainV0Settings settings;
        //Constructor
        public GrainV0()
        {   //initializing settigs for our algorithm
            settings = new GrainV0Settings();
        }
        // Settings property (needed to be because of ICrypComponent interface implementing)
        // sets and gets settings field
        public ISettings Settings
        {
            get => settings;
            set => settings = (GrainV0Settings)value;
        }
        //Key
        [PropertyInfo(Direction.InputData, "Key", "", true)]
        public byte[] Key
        {
            get => key;
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }
        [PropertyInfo(Direction.InputData, "KeystreamLength", " Length of keystream which will be generated in bytes", true)]
        public int KeystreamLength
        {
            get => keystreamLength;
            set
            {
                keystreamLength = value;
                OnPropertyChanged("KeystreamLength");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "Initital Vector", true)]
        public byte[] IV
        {
            get => iv;
            set
            {
                iv = value;
                OnPropertyChanged("IV");
            }
        }

        [PropertyInfo(Direction.OutputData, "Keystream", "Outputs keystream", true)]
        public byte[] Keystream
        {
            get => keystream;
            set
            {
                keystream = value;
                OnPropertyChanged("Keystream");
            }
        }

        public void InitGrainV0Chipher()
        {
            FillNFSR();
            FillLFSR();
            Initialization();
            GuiLogMessage("Registers filling before keystream generation:\n" + ToString(), NotificationLevel.Info);
        }
        private void FillNFSR()
        {// NFSR is filled with the key
            nfsr = GetBitRepresentation(key, REGISTERS_SIZE_BITS);
        }
        private void FillLFSR()
        {   // LFSR is being filled with the IV
            // positions from 64 to 79 are being filled with 1s
            const int SEVEN_BYTE_FIRST_BIT = 64;
            const int EIGHT_BYTE_LAST_BIT = 79;
            lfsr = GetBitRepresentation(iv, REGISTERS_SIZE_BITS);
            for (int i = SEVEN_BYTE_FIRST_BIT; i <= EIGHT_BYTE_LAST_BIT; i++)
            {
                lfsr[i] = 1;
            }
        }

        // generate next LFSR bit
        private int LFSROut()
        {
            int result = lfsr[0] ^ lfsr[13] ^ lfsr[23] ^ lfsr[38] ^ lfsr[51] ^ lfsr[62];
            return result;
        }
        // generate next NFSR bit
        private int NFSROut()
        {
            int result = nfsr[63] ^ nfsr[60] ^ nfsr[52] ^ nfsr[45] ^ nfsr[37] ^ nfsr[33] ^ nfsr[28] ^ nfsr[21] ^ nfsr[15] ^ nfsr[9] ^ nfsr[0] ^ (nfsr[63] & nfsr[60]) ^ (nfsr[37] & nfsr[33]) ^ (nfsr[15] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[45]) ^ (nfsr[33] & nfsr[28] & nfsr[21]) ^ (nfsr[63] & nfsr[45] & nfsr[28] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[37] & nfsr[33]) ^ (nfsr[63] & nfsr[60] & nfsr[21] & nfsr[15]) ^ (nfsr[63] & nfsr[60] & nfsr[52] & nfsr[45] & nfsr[37]) ^ (nfsr[33] & nfsr[28] & nfsr[21] & nfsr[15] & nfsr[9]) ^ (nfsr[52] & nfsr[45] & nfsr[37] & nfsr[33] & nfsr[28] & nfsr[21]);
            return result;
        }
        // update function output
        private int OutputFunction()
        {
            int x0 = lfsr[3], x1 = lfsr[25], x2 = lfsr[46], x3 = lfsr[64], x4 = nfsr[63];
            int result = x1 ^ x4 ^ (x0 & x3) ^ (x2 & x3) ^ (x3 & x4) ^ (x0 & x1 & x2) ^ (x0 & x2 & x3) ^ (x0 & x2 & x4) ^ (x1 & x2 & x4) ^ (x2 & x3 & x4) ^ nfsr[0];
            return result;
        }
        //method which converts byte array to bit representation
        private int[] GetBitRepresentation(byte[] byteArray, int bitArraySize = -1)
        {
            int[] bitArray;
            //if size was not defined as the argument
            if (bitArraySize == -1)
            { //use default size (each byte = 8 bits)
                bitArray = new int[byteArray.Length * 8];
            }
            else
            { //initializing resulting array
                bitArray = new int[bitArraySize];
            }
            int currentBit;
            //cycle goes through all bytes in the array
            for (int i = 0; i < byteArray.Length; i++)
            { //cycle goes through all bits in current byte
                for (int j = 0; j < 8; j++)
                {
                    currentBit = i * 8 + j;
                    if (currentBit >= bitArraySize)
                    {
                        return bitArray;
                    }
                    //shifts the number to get current bit value
                    bitArray[currentBit] = (byteArray[i] >> j) & 0x1;

                }

            }

            return bitArray;
        }
        // shifting the registers
        private int[] Shift(int[] register, int input)
        {
            for (int i = 0; i < 79; i++)
            {
                register[i] = register[i + 1];
            }
            register[79] = input;
            return register;
        }
        private void Initialization()
        // initializing the cipher
        {  // shifting the registers twice the key size
           // without producing any keystream, because the output of the filter function is being feed back into both shift registers
            const int INIT_TACTS_NUMBER = 160;
            int output, l0;
            for (int i = 0; i < INIT_TACTS_NUMBER; i++)
            {
                output = OutputFunction();
                l0 = lfsr[0];
                lfsr = Shift(lfsr, LFSROut() ^ output);
                nfsr = Shift(nfsr, NFSROut() ^ l0 ^ output);
            }
        }
        // one cipher round
        private int tact()
        {
            int output;
            output = OutputFunction();
            lfsr = Shift(lfsr, LFSROut());
            nfsr = Shift(nfsr, NFSROut() ^ lfsr[0]);
            return output;
        }
        public int[] GenerateKeystream(int length)
        { // generate keystream according to the length provided
            int[] keystream = new int[length];
            for (int i = 0; i < length; i++)
            {
                keystream[i] = tact();
            }
            return keystream;
        }

        // Check whether the parameters are entered correctly by the user
        private bool checkParameters()
        {

            //checking if there is an input
            if (Key == null || IV == null)
            {
                return false;
            }
            //checking how many bytes are in input(must be 10)
            if (Key.Length != KEY_SIZE_BYTES)
            {
                GuiLogMessage("Wrong key length " + Key.Length + " bytes. Key length must be " + KEY_SIZE_BYTES + " bytes.", NotificationLevel.Error);
                return false;
            }
            if (IV.Length != IV_SIZE_BYTES)
            {
                GuiLogMessage("Wrong key length " + Key.Length + " bytes. Key length must be " + IV_SIZE_BYTES + " bytes.", NotificationLevel.Error);
                return false;
            }
            //if everything OK returning true
            return true;
        }

        // Converts bit representation to byte array
        private void GetByteRepresentation(int[] inArray, byte[] outArray)
        {
            int temp;
            //cycle goes through all bytes
            for (int i = 0; i < outArray.Length; i++)
            {
                temp = 0;
                //cycle goes through all bits in current byte
                for (int j = 0; j < 8; j++)
                {    // first we shift the bit to its positon in byte
                     //binary or operation sets the bit to temporary value
                    temp |= inArray[(i * 8) + j] << j;
                }
                //sets temporary value to array cell
                outArray[i] = Convert.ToByte(temp);
            }
        }

        // Main method for launching the cipher
        public void Execute()
        {
            //sets the progressbar
            ProgressChanged(0, 1);
            if (checkParameters())
            { // checks the parameters
                // initialize the cipher with the key and IV
                // generate keystream bits and 
                // represent them in byte
                InitGrainV0Chipher();
                int[] keystreamBits;
                keystreamBits = GenerateKeystream(keystreamLength * 8);
                byte[] keystreamBytes = new byte[keystreamLength];
                GetByteRepresentation(keystreamBits, keystreamBytes);
                Keystream = keystreamBytes;
            }

            //set progressbar to 100%
            ProgressChanged(1, 1);
        }
        public override string ToString()
        {
            StringBuilder l = new StringBuilder();
            StringBuilder n = new StringBuilder();
            for (int i = 0; i < REGISTERS_SIZE_BITS; i++)
            {
                l.Append(lfsr[i]);
                n.Append(nfsr[i]);
            }
            return "nfsr:\n" + n.ToString() + "\nlfsr:\n" + l.ToString();
        }
        /* Reset method */
        public void Dispose()
        {
            keystream = null;
            lfsr = null;
            nfsr = null;
            key = null;
            iv = null;
        }
        //method needed by the ICrypComponent
        public void Initialize()
        {
        }


        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation => null;

        public void Stop()
        {
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion

    }
}