using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.GrainV0.Attack
{
    //Information about the author
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("GrainV0Attack.Properties.Resources", "GrainV0 Attack", "Algorithm gets weak key and weak IV for GrainV0 algorithm", "GrainV0Attack/userdoc.xml", new[] { "GrainV0Attack/Images/grain.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]


    //Class
    public class GrainV0Attack : ICrypComponent
    {

        private const int REGISTERS_SIZE_BITS = 80;
        private byte[] weakKey;
        private byte[] weakIV;
        private byte[] initialNFSR;
        private byte[] testingNFSR;
        private byte[] inputNFSR;
        private int[] nfsr;
        private int[] lfsr;
        private int[] trialNFSR;

        #region Data Properties
        //settings field 
        private GrainV0AttackSettings settings;
        //Constructor
        public GrainV0Attack()
        {   //initializing settigs for our algorithm
            settings = new GrainV0AttackSettings();
        }
        // Settings property (needed to be because of ICrypComponent interface implementing)
        // sets and gets settings field
        public ISettings Settings
        {
            get => settings;
            set => settings = (GrainV0AttackSettings)value;
        }

        [PropertyInfo(Direction.InputData, "NFSR filling", "optional", false)]
        public byte[] NFSRFilling
        {
            get => inputNFSR;
            set
            {
                inputNFSR = value;
                OnPropertyChanged("NFSRFilling");
            }
        }
        [PropertyInfo(Direction.OutputData, "Key", "Weak key", true)]
        public byte[] Key
        {
            get => weakKey;
            set
            {
                weakKey = value;
                OnPropertyChanged("Key");
            }
        }
        [PropertyInfo(Direction.OutputData, "IV", "Weak IV", true)]
        public byte[] IV
        {
            get => weakIV;
            set
            {
                weakIV = value;
                OnPropertyChanged("IV");
            }
        }

        [PropertyInfo(Direction.OutputData, "NFSR", "Outputs NFSR generated value in 160th state", true)]
        public byte[] NFSR160State
        {
            get => initialNFSR;
            set
            {
                initialNFSR = value;
                OnPropertyChanged("NFSR160State");
            }
        }

        [PropertyInfo(Direction.OutputData, "NFSR that is being tested", "", true)]
        public byte[] CurrentTestingValue
        {
            get => testingNFSR;
            set
            {
                testingNFSR = value;
                OnPropertyChanged("CurrentTestingValue");
            }
        }

        #endregion
        // attack is successful only if LFSR results in 1s from position 64 to 79
        private bool AttackSuccess()
        {
            const int FIRST_BIT_OF_SEVEN_BYTE = 64;
            bool successful = true;
            for (int i = FIRST_BIT_OF_SEVEN_BYTE; i < REGISTERS_SIZE_BITS; i++)
            {
                if (lfsr[i] == 0)
                {
                    successful = false;
                }
            }
            return successful;
        }
        // initializing NFSR and LFSR with zero values
        private void InitRegisters()
        {
            lfsr = new int[REGISTERS_SIZE_BITS];
            nfsr = new int[REGISTERS_SIZE_BITS];
            for (int i = 0; i < REGISTERS_SIZE_BITS; i++)
            {
                lfsr[i] = 0;
                nfsr[i] = 0;
            }
        }
        // update function output
        private int OutputFunction()
        {
            int x0 = lfsr[3], x1 = lfsr[25], x2 = lfsr[46], x3 = lfsr[64], x4 = nfsr[63];
            int result = x1 ^ x4 ^ (x0 & x3) ^ (x2 & x3) ^ (x3 & x4) ^ (x0 & x1 & x2) ^ (x0 & x2 & x3) ^ (x0 & x2 & x4) ^ (x1 & x2 & x4) ^ (x2 & x3 & x4);
            return result;
        }
        // generate next NFSR bit
        private void GetNFSROut(int lastBitLfsr)
        {
            int result = lastBitLfsr ^ lfsr[0] ^ lfsr[13] ^ lfsr[23] ^ lfsr[38] ^ lfsr[51] ^ lfsr[62] ^ OutputFunction();
            nfsr[0] = result;
        }
        // generate next LFSR bit
        private void GetLFSROut(int lastBitNfsr)
        {
            int result = nfsr[63] ^ nfsr[60] ^ nfsr[52] ^ nfsr[45] ^ nfsr[37] ^ nfsr[33] ^ nfsr[28] ^ nfsr[21] ^ nfsr[15] ^ nfsr[9] ^ lastBitNfsr ^ (nfsr[63] & nfsr[60]) ^ (nfsr[37] & nfsr[33]) ^ (nfsr[15] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[45]) ^ (nfsr[33] & nfsr[28] & nfsr[21]) ^ (nfsr[63] & nfsr[45] & nfsr[28] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[37] & nfsr[33]) ^ (nfsr[63] & nfsr[60] & nfsr[21] & nfsr[15]) ^ (nfsr[63] & nfsr[60] & nfsr[52] & nfsr[45] & nfsr[37]) ^ (nfsr[33] & nfsr[28] & nfsr[21] & nfsr[15] & nfsr[9]) ^ (nfsr[52] & nfsr[45] & nfsr[37] & nfsr[33] & nfsr[28] & nfsr[21]) ^ OutputFunction();
            lfsr[0] = result;
        }



        // one clocking back
        private void TactBack()
        {
            int lastLFSR = ShiftBack(lfsr);
            int lastNFSR = ShiftBack(nfsr);

            GetLFSROut(lastNFSR);
            GetNFSROut(lastLFSR);
        }
        // random filling of the NFSR in last state
        private void GetRandomFilling(int[] register)
        {
            Random r = new Random();
            byte[] temporary = new byte[10];
            r.NextBytes(temporary);
            CurrentTestingValue = temporary;
            string s = "";
            for (int i = 0; i < 10; i++)
            {
                s += Convert.ToString(temporary[i], 2).PadLeft(8, '0');
            }
            for (int i = 0; i < 80; i++)
            {
                register[i] = Convert.ToInt32(s[i].ToString(), 2);
            }
        }
        // shifting back registers
        private int ShiftBack(int[] reg)
        {
            int result = reg[79];
            for (int i = 79; i > 0; i--)
            {
                reg[i] = reg[i - 1];
            }
            reg[0] = 0;
            return result;
        }
        private void ClearLFSR()
        {
            for (int i = 0; i < REGISTERS_SIZE_BITS; i++)
            {
                lfsr[i] = 0;
            }
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
            {//cycle goes through all bits in current byte
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
        public void StartAttack()
        { // get the last state of NFSR
            if (inputNFSR != null)
            {
                trialNFSR = GetBitRepresentation(inputNFSR, REGISTERS_SIZE_BITS);
                Array.Copy(trialNFSR, nfsr, REGISTERS_SIZE_BITS);
                // set last state of LFSR to zero 
                ClearLFSR();
                // shift back 160 times
                for (int i = 0; i < 160; i++)
                {
                    TactBack();
                }
                // if attack is successful, return key and IV
                if (AttackSuccess())
                {
                    MakeOutput();
                    return;
                }
                else
                {
                    GuiLogMessage("Attack failed with this NFSR filling, starting getting random values", NotificationLevel.Warning);
                }
            }
            // NFSR in last state can be taken with random filling
            trialNFSR = new int[REGISTERS_SIZE_BITS];
            while (true)
            { // logic remains the same: set the last state of LFSR to zero state and shift registers back 160 times
                GetRandomFilling(trialNFSR);
                Array.Copy(trialNFSR, nfsr, 80);
                ClearLFSR();
                for (int i = 0; i < 160; i++)
                {
                    TactBack();
                }
                // if attack is successful, return key, IV and NFSR last state
                if (AttackSuccess())
                {
                    MakeOutput();
                    break;
                }

            }

        }
        private void MakeOutput()
        {
            const int KEY_SIZE_BYTES = 10;
            const int REFISTER_SIZE_BYTES = 10;
            const int IV_SIZE_BYTES = 8;
            byte[] tempWeakKey = new byte[KEY_SIZE_BYTES];
            byte[] tempWeakIV = new byte[IV_SIZE_BYTES];
            byte[] tempInitialNFSR = new byte[REFISTER_SIZE_BYTES];
            GetByteRepresentation(nfsr, tempWeakKey);
            GetByteRepresentation(lfsr, tempWeakIV);
            GetByteRepresentation(trialNFSR, tempInitialNFSR);
            Key = tempWeakKey;
            IV = tempWeakIV;
            // we may not add this as output when we enter the value of NFSR in last state manually, however this is neccessary to be added when we input
            // NFSR in last state with random generator
            NFSR160State = tempInitialNFSR;
        } // convert bit representation to byte representation
        private void GetByteRepresentation(int[] inArray, byte[] outArray)
        {
            int temp;
            for (int i = 0; i < outArray.Length; i++)
            {
                temp = 0;
                for (int j = 0; j < 8; j++)
                {
                    temp |= inArray[(i * 8) + j] << j;
                }
                outArray[i] = Convert.ToByte(temp);
            }
        }


        public void Execute()
        {
            //sets the progressbar
            ProgressChanged(0, 1);

            //if UseGenerator checkbox was selected
            //we use C# random number generator to fill the NFSR
            InitRegisters();
            StartAttack();
            GuiLogMessage("Attack was successful", NotificationLevel.Info);
            ProgressChanged(1, 1);
        }
        /* Reset method */
        public void Dispose()
        {
            Key = null;
            IV = null;
            NFSR160State = null;
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