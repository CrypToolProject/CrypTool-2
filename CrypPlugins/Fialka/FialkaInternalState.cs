/*
   Copyright 2016, Eugen Antal, FEI STU Bratislava

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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CrypTool.Fialka
{
    public class FialkaInternalState
    {
        /*
        * These variables are assigned only during the initialisation process when the rotor series are set.
        * Variables cannot be set separately.
        */
        #region Immutable part of the key
        /// <summary>
        /// Two dimensional array (10 (rotors) x 30 (contacts)), contains the selected rotor wiring (permutation).
        /// For rotors in base order and position.
        /// </summary>
        private int[,] _rotorWirings;
        public int[,] rotorWirings => _rotorWirings;
        /// <summary>
        /// Inverse permutation of the selected rotors.
        /// For rotors in base order and position.
        /// </summary>
        private int[,] _inverseRotorWirings;
        public int[,] inverseRotorWirings => _inverseRotorWirings;
        /// <summary>
        /// Two dimensional array (10 (rotors) x 30 (contacts)). Contains the positions of the blocking pins on the selected rotors.
        /// For rotors in base order and position.
        /// </summary>
        private int[,] _pinPositions;
        public int[,] pinPositions => _pinPositions;
        /// <summary>
        /// Two dimensional array (10 (rotors) x 30 (contacts)), contains the selected rotor wiring for flipped core (permutation).
        /// For rotors in base order and position. PROTON II only.
        /// </summary>
        private int[,] _rotorWiringsFlippedCores;
        public int[,] rotorWiringsFlippedCores => _rotorWiringsFlippedCores;
        /// <summary>
        /// Inverse permutation of the selected rotors  for flipped core.
        /// For rotors in base order and position. PROTON II only.
        /// </summary>
        private int[,] _inverseRotorWiringsFlippedCores;
        public int[,] inverseRotorWiringsFlippedCores => _inverseRotorWiringsFlippedCores;
        /// <summary>
        /// Enumeration type specifies and set the selected rotor wiring/pin tables (unfortunatelly only 3K, 5K and 6K rotors are available).
        /// </summary>
        private FialkaEnums.rotorSeries _rotSeries;
        public FialkaEnums.rotorSeries rotorSeries
        {
            get => _rotSeries;
            set
            {
                _rotSeries = value;
                _rotorWirings = FialkaConstants.getRotorWiring(_rotSeries);
                _inverseRotorWirings = FialkaConstants.getInverseRotorWiring(_rotSeries);
                _pinPositions = FialkaConstants.getPinPositions(_rotSeries);
                // PROTON II (core flipping)
                _rotorWiringsFlippedCores = FialkaConstants.getRotorWiringFlippedCores(_rotSeries);
                _inverseRotorWiringsFlippedCores = FialkaConstants.getInverseRotorWiringFlippedCores(_rotSeries);
            }
        }
        #endregion

        /*
         * These variables influence the encryption process.
         */
        #region Machine settings
        /// <summary>
        /// Enumeration type specifies the selected machine type (M-125 and M-125-3).
        /// For M-125 machine the textOperationmMode should be set to Letters and numlockType to 30.
        /// </summary>
        private FialkaEnums.machineModel _model;
        public FialkaEnums.machineModel model
        {
            get => _model;
            set
            {
                _model = value;
                if (_model == FialkaEnums.machineModel.M125)
                {
                    txtOpMode = FialkaEnums.textOperationmMode.Letters;
                    numlockType = FialkaEnums.numLockType.NumLock30;
                }
            }
        }
        /// <summary>
        /// Enumeration type.
        /// Model specific setting (available only in M-125-3). Describes what input is allowed (enabled keys on the keyboard). 
        /// Two values are presented: 'NumLock30' (all inputs are available) and 'NumLock10' (only numbers can by set as input).
        /// NumLock10 mode will work only if the cyrillic printHead is presented.
        /// </summary>   
        private FialkaEnums.numLockType _numlockType;

        public FialkaEnums.numLockType numlockType
        {
            get => _numlockType;
            set
            {
                _numlockType = value;
                if (_numlockType == FialkaEnums.numLockType.NumLock10)
                {
                    // !!!
                    // NumLock10 works only with cyrillic print head, auto set 
                    if (printHead == FialkaEnums.printHeadMapping.Latin)
                    {
                        printHead = FialkaEnums.printHeadMapping.Cyrillic;
                    }
                    // NumLock10 works only with numers text operation mode, auto set
                    if (txtOpMode != FialkaEnums.textOperationmMode.Numbers)
                    {
                        txtOpMode = FialkaEnums.textOperationmMode.Numbers;
                    }
                }
            }
        }

        /// <summary>
        /// Enumeration type specifies what operation mode is used. The Fialka machine can be used in 3 modes. 
        /// Because the permutation on the reflector is not an involution (contains cycles of length 3) different mode should be used for ecryption.
        /// Fialka can be also used in PlainText mode - without encryption.
        /// Values: 'Encrypt', 'Decrypt' and 'Plain'.
        /// </summary> 
        public FialkaEnums.operationMode opMode;
        /// <summary>
        /// Enumeration type specifies the type of used rotors.
        /// 'PROTON I' rotor is the earlier type. Contains the blocking pin positions. 
        /// 'PROTON II' is the newest. Additionaly the core (that contains the rotor wiring) can be shifted or inserted into another rotor.
        /// It's important that the core represents only the permutation, the rotor stepping depends only on the pin positions.
        /// PROTON II rotors without any manipulation with the core works the same as PROTON I rotors.
        /// The setter automatically handle the PROTON I and II compatibility issue.
        /// </summary>
        private FialkaEnums.rotorTypes _rotorType;
        public FialkaEnums.rotorTypes rotorType
        {
            get => _rotorType;
            set
            {
                _rotorType = value;
                if (_rotorType == FialkaEnums.rotorTypes.PROTON_I)
                {
                    // additional compatibility settings
                    coreOrders = rotorOrders;// for PROTON I pass the same reference as rotor order (same for wiring and pin)
                    coreOffsets = FialkaConstants.nullOffset();
                    coreOrientation = FialkaConstants.deafultCoreOrientation();
                }
                else if (_rotorType == FialkaEnums.rotorTypes.PROTON_II)
                {
                    // in case of PROTON II rotor order specifies the pin positions, core order the wiring
                    // the cores can be set separately
                    coreOrders = (int[])DeepCopy(rotorOrders); // save a copy
                }
            }
        }
        #endregion

        #region Input output settings
        /// <summary>
        /// Enumeration type. Represents the country specific keyboard and print head layout.
        /// </summary>
        public FialkaEnums.countryLayout countryLayout;
        /// <summary>
        /// Enumeration type represents the print head charset (letterShist an numberShift).
        /// </summary>
        public FialkaEnums.printHeadShift printHeadShift;
        /// <summary>
        /// Enumeration type. Represents the output mapping. 
        /// </summary>
        private FialkaEnums.printHeadMapping _printHead;
        public FialkaEnums.printHeadMapping printHead
        {
            get => _printHead;
            set => _printHead = value;
        }
        /// <summary>
        /// Enumeration type. Represents the output mapping for M-125-3 devices (also with the print head). In case of M-125 should be set to Letters mode.
        /// </summary>
        private FialkaEnums.textOperationmMode _txtOpMode;
        public FialkaEnums.textOperationmMode txtOpMode
        {
            get => _txtOpMode;
            set
            {
                _txtOpMode = value;
                // set the print head based on text operation mode, for letters and mixed set letterShift, for numbers set numberShift
                switch (_txtOpMode)
                {
                    case FialkaEnums.textOperationmMode.Mixed:
                    case FialkaEnums.textOperationmMode.Letters:
                        printHeadShift = FialkaEnums.printHeadShift.LetterShift;
                        break;

                    case FialkaEnums.textOperationmMode.Numbers:
                        printHeadShift = FialkaEnums.printHeadShift.NumberShift;
                        break;

                }
            }
        }

        /// <summary>
        /// Enumeration type. Represents how to handle and present invalid inputs in output string. 
        /// </summary>            
        public FialkaEnums.handleInvalidInput inputHandler = FialkaEnums.handleInvalidInput.Remove;

        #endregion

        /*
         * RotorOffsets represents the internal state, that changes after pressing (encrypting) a single key.
         * The rest of the variables - positions are not changing during the message encryption.
         */
        #region Mutable part of the key - daily key
        /// <summary>
        /// Order of the rotors, how they are inserted into the machine. The base wiring and pin tables are for rotors in base position (baseRotorPositions constant).
        /// When rotors are reordered, this array is used to calculate the position in the default tables.
        /// A:
        /// Specify the pin positions in case of PROTON I and II rotors. The 'pinPositions' variable contains the pin positions in specific 
        /// rotor order (rotors 1,2,..10). If the rotors are reordered it's necessary to handle the reindexing.
        /// B:
        /// It's necessary to also specify the correct rotor permutation based on this position, because the 'rotorWirings' variable contains
        /// the permutations in a specific order (rotors 1,2,..10). If the rotors are reordered this variable specify the index in 'rotorWirings'. The 'coreOrders' variable is
        /// used to calculate the wiring instead. In case of PROTON I this should be the same as coreOrders.
        /// </summary>
        public int[] rotorOrders;
        /// <summary>
        /// Offset of the labes on the rotor at the specific position.
        /// Index 0 is the leftmost rotor. 
        /// </summary>
        public int[] ringOffsets;
        /// <summary>
        /// Offset of rotors (shifted clockwise position).
        /// Index 0 is the leftmost rotor.
        /// </summary>
        public int[] rotorOffsets;
        /// <summary>
        /// The position of the wiring. In case of PROTON I rotors this should be the same as the order of the rotors ('rotorOrders' variable).
        /// If the cores are reordered this variable specify the index in the 'rotorWirings'.
        /// </summary>
        public int[] coreOrders;  // order (rotor wiring)
        /// <summary>
        /// Offset of cores (shifted clockwise position).
        /// Index 0 is the leftmost rotor.
        /// </summary>
        public int[] coreOffsets;
        /// <summary>
        /// The orientation of the cores, how they are inserted. (PROTON II only)
        /// Index 0 is the leftmost rotor.
        /// </summary>
        public int[] coreOrientation;
        /// <summary>
        /// Permutation on the punch card.
        /// </summary>
        /// 
        private int[] _punchCard;
        public int[] punchCard
        {
            get => _punchCard;
            set
            {
                _punchCard = value;
                punchCardInverse = FialkaConstants.getInversePermutation(_punchCard);
            }
        }
        /// <summary>
        /// Inverse permutation on the punch card. It's calculated automatically when the 'punchCard' setter is used.
        /// </summary>
        public int[] punchCardInverse { get; private set; }
        #endregion

        /*
         * Automatic counter - should be called after a key is pressed. 
         * Initial value = 0; Maximal value is defined in FialkaConstants, after reaching this value it's restarted.
         */
        #region Counter
        private int counter = 0;

        public void resetCounter()
        {
            counter = 0;
        }

        public string getCounter()
        {
            return counter / FialkaConstants.counter5LetterDigitMod + "|" + counter % FialkaConstants.counter5LetterDigitMod;
        }

        private void updateCounter()
        {
            counter++;
            counter %= FialkaConstants.counterMax;
        }
        #endregion

        #region Constructors and initialization
        /// <summary>
        /// Constructor, calls a default initialization. 
        /// </summary>
        /// <returns >A new initialized instance of FialkaInternalState.</returns>
        public FialkaInternalState()
        {
            init();
        }

        /// <summary>
        /// Internal state initialization.
        /// Initial state: baseOrder, basePosition, zero offsets, no punch card (set as identity),
        /// machine type is M-125, 6K PROTON I rotor series, Czechslovakian keyboard layout and latin print head.
        /// </summary>
        private void init()
        {
            // initial rotor series
            rotorSeries = FialkaEnums.rotorSeries.K6; // auto sets the wirings and pins
            // base rotor settings
            rotorOrders = FialkaConstants.baseRotorPositions();
            rotorOffsets = FialkaConstants.baseRotorPositions(); // each rotor has his label's value as offset
            ringOffsets = FialkaConstants.nullOffset();
            rotorType = FialkaEnums.rotorTypes.PROTON_I;
            // AUTO SET:
            //base positions - the cores are in the corresponding rotor (M-125 compatible mode)
            //this.coreOrders = FialkaConstants.baseRotorPositions(); //auto set in rotorType setter
            //this.coreOffsets = FialkaConstants.nullOffset(); //auto set in rotorType setter
            //this.coreOrientation = FialkaConstants.deafultCoreOrientation(); //auto set in rotorType setter
            // punch card - identity
            punchCard = FialkaConstants.punchCardIdentity();
            // AUTO SET:
            //this.punchCardInverse = FialkaConstants.punchCardIdentity(); // auto set in punchCardsetter
            // modes
            model = FialkaEnums.machineModel.M125;
            // AUTO SET:
            //this.numlockType = FialkaEnums.numLockType.NumLock30; // auto set in model setter
            //this.txtOpMode = FialkaEnums.textOperationmMode.Letters; // auto set in model setter
            opMode = FialkaEnums.operationMode.Encrypt; // auto set in model setter
            // input/output
            countryLayout = FialkaEnums.countryLayout.Czechoslovakia;
            printHead = FialkaEnums.printHeadMapping.Latin;
            // AUTO SET:
            //this.printHeadShift = FialkaEnums.printHeadShift.LetterShift; // auto set in txtOpMode setter
        }


        /// <summary>
        /// Set the daily key for PROTON II rotors.
        /// </summary>
        /// <param name="rotorOrders">Order of rotors.</param>
        /// <param name="rotorOffsets">Base rotor offsets.</param>
        /// <param name="ringOffsets">Ring offset on rotors.</param>
        /// <param name="punchCard">The punch card permutation.</param>
        /// <param name="coreOrders">Order of the cores.</param>
        /// <param name="coreOrientation">Orientation of the cores (if the core is flipped).</param>
        /// <param name="coreOffsets">Core offset on rotors.</param>
        public void setDailyKey(int[] rotorOrders, int[] rotorOffsets, int[] ringOffsets, int[] punchCard, int[] coreOrders, int[] coreOrientation, int[] coreOffsets)
        {
            setDailyKey(rotorOrders, rotorOffsets, ringOffsets, punchCard);
            this.coreOrders = coreOrders;
            this.coreOffsets = coreOffsets;
            this.coreOrientation = coreOrientation;

        }


        /// <summary>
        /// Set the daily key for PROTON I type rotors.
        /// </summary>
        /// <param name="rotorOrders">Order of rotors.</param>
        /// <param name="rotorOffsets">Base rotor offsets.</param>
        /// <param name="ringOffsets">Ring offset on rotors.</param>
        /// <param name="punchCard">The punch card permutation.</param>
        public void setDailyKey(int[] rotorOrders, int[] rotorOffsets, int[] ringOffsets, int[] punchCard)
        {
            this.rotorOrders = rotorOrders;
            this.rotorOffsets = rotorOffsets;
            this.ringOffsets = ringOffsets;
            this.punchCard = punchCard;
        }

        /// <summary>
        /// List of callback instances. The interface's internalStateChanged() method is called for all elements in the list.
        /// </summary>
        public List<FialkaStateChangeCallback> stateChangeCallbacks = new List<FialkaStateChangeCallback>();
        /// <summary>
        /// This method is called from the core to set the counter and send a notofocation
        /// after the rotors are set to a new position when an input letter is encrypted.
        /// Also the callbacks are notified about the internal state change (only after the rotor stepping !).
        /// </summary>
        public void notifyInternalStatChangede()
        {
            updateCounter(); // auto counter 
            foreach (FialkaStateChangeCallback cb in stateChangeCallbacks)
            {
                cb.internalStateChanged(rotorOffsets);
            }

        }

        #endregion

        #region DeepCopyHelper
        public FialkaInternalState DeepCopy()
        {
            return (FialkaInternalState)DeepCopy(this);
        }
        /// <summary>
        /// C# deep copy
        /// Source: http://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-an-object-in-net-c-specifically
        /// extended to handle multi-dimensional arrays.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private object DeepCopy(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            else if (type.IsArray)
            {
                if (type.Name.Contains("[]"))
                {
                    Type elementType = Type.GetType(
                    type.FullName.Replace("[]", string.Empty));
                    Array array = obj as Array;
                    Array copied = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        copied.SetValue(DeepCopy(array.GetValue(i)), i);
                    }
                    return Convert.ChangeType(copied, obj.GetType());
                }
                else if (type.Name.Contains("[,]"))
                { // my own code to handle multi dimensional array
                    Type elementType = Type.GetType(
                    type.FullName.Replace("[,]", string.Empty));
                    Array array = obj as Array;
                    int rows = array.GetLength(0);
                    int cols = array.GetLength(1);
                    Array copied = Array.CreateInstance(elementType, rows, cols);
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            copied.SetValue(DeepCopy(array.GetValue(i, j)), i, j);
                        }
                    }
                    return Convert.ChangeType(copied, obj.GetType());
                }
                else
                {
                    return null;
                }

            }
            else if (type.IsClass)
            {

                object toret = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                    {
                        continue;
                    }

                    field.SetValue(toret, DeepCopy(fieldValue));
                }
                return toret;
            }
            else
            {
                throw new ArgumentException("Unknown type");
            }
        }

        #endregion

        #region Key representation
        /// <summary>
        /// Internal state representation (formatted key) based on the rotor type.
        /// </summary>
        /// <returns>Formatted key (actual internal state).</returns>
        public string getFormattedKey()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Rotor order:\r");
            for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
            {
                sb.Append(rotorOrders[i]);
                if (i < FialkaConstants.numberOfRotors - 1)
                {
                    sb.Append("-");
                }
            }
            sb.Append("\rRotor offset:\r");
            for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
            {
                sb.Append(rotorOffsets[i]);
                if (i < FialkaConstants.numberOfRotors - 1)
                {
                    sb.Append("-");
                }
            }
            sb.Append("\rRing offsets:\r");
            for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
            {
                sb.Append(ringOffsets[i]);
                if (i < FialkaConstants.numberOfRotors - 1)
                {
                    sb.Append("-");
                }
            }

            if (rotorType == FialkaEnums.rotorTypes.PROTON_II)
            {
                sb.Append("\rCore order:\r");
                for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
                {
                    sb.Append(coreOrders[i]);
                    if (i < FialkaConstants.numberOfRotors - 1)
                    {
                        sb.Append("-");
                    }
                }
                sb.Append("\rCore sides:\r");
                for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
                {
                    sb.Append(coreOrientation[i] == 1 ? (1) : (2));
                    if (i < FialkaConstants.numberOfRotors - 1)
                    {
                        sb.Append("-");
                    }
                }
                sb.Append("\rCore offsets:\r");
                for (int i = 0; i < FialkaConstants.numberOfRotors; i++)
                {
                    sb.Append(coreOffsets[i]);
                    if (i < FialkaConstants.numberOfRotors - 1)
                    {
                        sb.Append("-");
                    }
                }

            }

            return sb.ToString();
        }
    }
    #endregion

}
