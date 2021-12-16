using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.Plugins.GrainV1.Attack
{
    //Information about the author
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("GrainV1Attack.Properties.Resources", "Grain V1 Attack", "Searches for weak key-IV pairs", "GrainV1Attack/userdoc.xml", new[] { "GrainV1Attack/GrainV1Attack.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]



    public class GrainV1Attack : ICrypComponent
    {
        //byte arrays for registers ( length=80, 1 byte stands for 1 bit)
        public byte[] lfsr;
        public byte[] nfsr;
        //byte array for input(10 bytes)
        public byte[] inputNFSR;
        //byte arrays for output
        public byte[] outputNFSR;
        public byte[] nfsr160State;
        public byte[] outputLFSR;

        // public Byte[] keystream;


        #region Data Properties
        //settings field 
        private GrainV1AttackSettings settings;
        //Constructor
        public GrainV1Attack()
        {   //initializing settigs for our algorithm
            settings = new GrainV1AttackSettings();
        }
        // Settings property (needed to be because of ICrypComponent interface implementing)
        // sets and gets settings field
        public ISettings Settings
        {
            get => settings;
            set => settings = (GrainV1AttackSettings)value;
        }
        //Input property. Fills NFSR in 160 round.
        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public byte[] InputNFSR
        {
            get => inputNFSR;
            set
            {
                inputNFSR = value;
                OnPropertyChanged("InputNFSR");
            }
        }
        //Output property. Displays NFSR in 0 round.
        [PropertyInfo(Direction.OutputData, "NFSR", "Outputs NFSR initial state", true)]
        public byte[] OutputNFSR
        {
            get => nfsr;
            set
            {
                nfsr = value;
                OnPropertyChanged("OutputNFSR");
            }
        }
        //Output property. Displays LFSR in 0 round.
        [PropertyInfo(Direction.OutputData, "LFSR", "Outputs LFSR initial state", true)]
        public byte[] OutputLFSR
        {
            get => outputNFSR;
            set
            {
                outputNFSR = value;
                OnPropertyChanged("OutputLFSR");
            }
        }

        [PropertyInfo(Direction.OutputData, "NFSR160State", "Outputs LFSR initial state", true)]
        public byte[] NFSR160State
        {
            get => nfsr160State;
            set
            {
                nfsr160State = value;
                OnPropertyChanged("NFSR160State");
            }
        }
        //  [PropertyInfo(Direction.OutputData, "keystream", "keystream during initialization", true)]
        //  public byte[] KeyStream
        //  {
        // get { return this.keystream; }
        // set
        //     {
        //        this.keystream = value;
        //       OnPropertyChanged("KeyStream");
        //     }
        // }
        #endregion

        // Check whether the parameters are entered correctly by the user
        private bool checkParameters()
        {   //checking if there is an input
            if (inputNFSR == null)
            {
                GuiLogMessage("No input given. Aborting.", NotificationLevel.Error);
                return false;
            }
            //checking how many bytes are in input(must be 10)
            if (inputNFSR.Length != 10)
            {
                GuiLogMessage("Wrong NFSR length " + inputNFSR.Length + " bytes. Key length must be 10 bytes.", NotificationLevel.Error);
                return false;
            }
            //if everything OK returning true
            return true;
        }
        /* Main method for launching the cipher */
        public void Execute()
        {
            //sets the progressbar
            ProgressChanged(0, 1);
            //if UseGenerator checkbox was selected
            //we use C# random number generator to fill the NFSR
            if (settings.UseGenerator)
            {
                GuiLogMessage("Using standart random number generator", NotificationLevel.Info);
                //Creating new Random cass object
                Random r = new Random();
                //initialize input array of size 10
                inputNFSR = new byte[10];
                //starting the cycle
                do
                {
                    //filling the array with random bytes
                    r.NextBytes(inputNFSR);
                    NFSR160State = inputNFSR;
                    //initializing the algorithm (filling the LFSR with zeros)
                    Init();
                    //setting NFSR with generated values
                    SetNFSR(InputNFSR);
                    //making the attack
                    Attack();
                }
                //cycle works until the Success() function return  true
                while (!Success());
                GuiLogMessage("Attack was successful", NotificationLevel.Info);
            }
            //if algorithm uses external generator
            else
            {
                //validates inputs
                //if something's wrong stops the executing
                if (!checkParameters())
                {
                    return;
                }
                //initializing the algorithm (filling the LFSR with zeros)
                Init();
                NFSR160State = InputNFSR;
                //setting NFSR with input values
                SetNFSR(InputNFSR);
                //making the attack
                Attack();
                //if attack was successful
                if (Success())
                {   //outputh the message about success
                    GuiLogMessage("Attack was successful", NotificationLevel.Info);
                }
                else
                {   //otherwise outputs a message about failure
                    GuiLogMessage("Attack failed. Please try fill NFSR with another value ", NotificationLevel.Warning);
                }
            }
            //sets the outputs with current values in registers
            OutputLFSR = CompressBytes(10, lfsr);
            OutputNFSR = CompressBytes(10, nfsr);
            //   KeyStream = CompressBytes(20, keystream);
            //set progressbar to 100%
            ProgressChanged(1, 1);
        }



        /* Reset method */
        public void Dispose()
        {
            inputNFSR = null;
            outputLFSR = null;
            outputNFSR = null;

        }
        //method needed by the ICrypComponent
        public void Initialize()
        {
        }
        //shifts the register values in another side and returns value of the previous last element in register(needed for algorithm)
        private byte ShiftBack(byte[] registr)
        {
            //get previous last bit of register
            byte result = registr[79];
            //cycle goes through all register
            for (int i = 79; i > 0; i--)
            {
                registr[i] = registr[i - 1];
            }
            //sets 0-element with 0
            registr[0] = 0;
            return result;
        }
        //function gets 0-element of lfsr
        private void GetNextLFSR(byte last)
        {   // the equation is the same as for new element (equations are given in the original Grain v1 cipher) 
            // but instead of 0-element we use previous last element of register
            int result = last ^ lfsr[13] ^ lfsr[23] ^ lfsr[38] ^ lfsr[51] ^ lfsr[62] ^ OutputFunction();
            lfsr[0] = Convert.ToByte(result);
        }
        //the same logic as for LFSR
        private void GetNextNFSR(byte last)
        {
            int result = lfsr[0] ^ nfsr[62] ^ nfsr[60] ^ nfsr[52] ^ nfsr[45] ^ nfsr[37] ^ nfsr[33] ^ nfsr[28] ^ nfsr[21] ^ nfsr[14] ^ nfsr[9] ^ last ^ (nfsr[63] & nfsr[60]) ^ (nfsr[37] & nfsr[33]) ^ (nfsr[15] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[45]) ^ (nfsr[33] & nfsr[28] & nfsr[21]) ^ (nfsr[63] & nfsr[45] & nfsr[28] & nfsr[9]) ^ (nfsr[60] & nfsr[52] & nfsr[37] & nfsr[33]) ^ (nfsr[63] & nfsr[60] & nfsr[21] & nfsr[15]) ^ (nfsr[63] & nfsr[60] & nfsr[52] & nfsr[45] & nfsr[37]) ^ (nfsr[33] & nfsr[28] & nfsr[21] & nfsr[15] & nfsr[9]) ^ (nfsr[52] & nfsr[45] & nfsr[37] & nfsr[33] & nfsr[28] & nfsr[21]) ^ OutputFunction();
            nfsr[0] = Convert.ToByte(result);
        }
        //run 160 rounds
        public void Attack()
        {
            //    keystream = new byte[160];
            for (int i = 0; i < 160; i++)
            {
                //   keystream[i] = OutputFunction();
                TaktBack();
            }
        }
        //makes 80 bits from 10-bytes array
        public void SetNFSR(byte[] val)
        {
            //counter variable
            int i = 0;
            //cycle goes through array
            foreach (byte c in val)
            {
                //convert value to binary form and padding left with '0' to get length in 8 bit
                System.Collections.Generic.IEnumerable<char> tmp = Convert.ToString(c, 2).PadLeft(8, '0').Reverse();
                //cycle goes through string with binary number
                foreach (char a in tmp)
                {
                    //first convert char into byte 
                    nfsr[i] = Convert.ToByte(a.ToString(), 2);
                    //incrementing counter
                    i++;
                }
            }
        }
        //method for checking LFSR state
        private bool Success()
        {
            //cycle runs through last 16 bits of LFSR
            for (int i = 63; i < 80; i++)
            {
                //if any bit is 0 then returns false
                if (lfsr[i] == 0)
                {
                    return false;
                }
            }
            //if all bits are 1, return 1
            return true;
        }
        //method compress byte array that represent bits into simple byte array
        private byte[] CompressBytes(int size, byte[] src)
        {
            //creating resulting array of size which was as argument
            byte[] res = new byte[size];
            //goes through every element in array
            for (int i = 0; i < size; i++)
            {
                //sets current array element value to 0
                res[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                    //set every bit for current byte as in src array(bit representation)
                    res[i] |= Convert.ToByte(src[(i * 8) + j] << j);
                }
            }
            //returning resulting array
            return res;

        }
        //method makes one takt back
        private void TaktBack()
        {
            //shifting back registers
            byte lastLFSR = ShiftBack(lfsr);
            byte lastNFSR = ShiftBack(nfsr);
            //sets 0-elements of registers 
            GetNextLFSR(lastLFSR);
            GetNextNFSR(lastNFSR);

        }
        //method counts resulting function of GrainV1
        private byte OutputFunction()
        {
            byte x0 = lfsr[3], x1 = lfsr[25], x2 = lfsr[46], x3 = lfsr[64], x4 = nfsr[63];
            int result = x1 ^ x4 ^ (x0 & x3) ^ (x2 & x3) ^ (x3 & x4) ^ (x0 & x1 & x2) ^ (x0 & x2 & x3) ^ (x0 & x2 & x4) ^ (x1 & x2 & x4) ^ (x2 & x3 & x4) ^ nfsr[1] ^ nfsr[2] ^ nfsr[4] ^ nfsr[10] ^ nfsr[31] ^ nfsr[43] ^ nfsr[56];
            return Convert.ToByte(result);
        }
        //method initialize arrays and fills lfsr with '0'
        public void Init()
        {
            nfsr = new byte[80];
            lfsr = new byte[80];
            for (int i = 0; i < 80; i++)
            {
                lfsr[i] = 0;
            }
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