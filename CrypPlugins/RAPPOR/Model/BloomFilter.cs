 using System;
//Extra system library for the bloom filter
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace RAPPOR.Model
{
    /// <summary>
    /// Represents the Boolean filter which is beeing used in the RAPPOR component. This class 
    /// allows the creation of a Boolean filter of any length from 0 to 256 and any input. Every
    /// input is beeing seen as an string input.
    /// </summary>
    public class BloomFilter : INotifyPropertyChanged
    {
        /// <summary>This is the Bloom filter array.</summary>
        private readonly bool[] boolArray;

        /// <summary>The string array representing the input of the component.</summary>
        private readonly string[] stringArray;

        /// <summary>Internal bit array for the creation of the Bloom filter.</summary>
        private readonly BitArray hashBits;

        /// <summary>The size of the Bloom filter.</summary>
        private readonly int _sizeOfBloomFilter;

        /// <summary>The ammount of hash functions used in the Bloom filter.</summary>
        private readonly int _amountOfHashFunctions;

        /// <summary>The history of the bloom filter. This represents the results of every
        /// hash function calculation for every input string.</summary>
        private readonly int[][] history;

        private Boolean running;

        /// <summary>
        /// Initializes the instance of the Bloom filter. It initialized the given parameters of
        /// the class, notably creating a Bitarray for the hashBits variable, creating the history
        /// in the format history[Input.Length] and history[i][amountofHashfunctions], adding the
        /// calculated hash values of the hashfunctions which have been created from the input to 
        /// hash bits while then adding the hashbits values to the boolean array.
        /// </summary>
        /// <param name="input">The input which is provided to the component</param>
        /// <param name="sizeOfBloomFilter">The size of the bloom filter which  is beeing used.</param>
        /// <param name="amountOfHashFunctions">The ammount fo hash functions which are beeing utilized in the bloom filter</param>
        public BloomFilter(string[] input, int sizeOfBloomFilter, int amountOfHashFunctions)
        {
            _amountOfHashFunctions = amountOfHashFunctions;

            _sizeOfBloomFilter = sizeOfBloomFilter;

            boolArray = new bool[_sizeOfBloomFilter];

            //The input is being split in an string array with the divider ','.
            stringArray = input;

            hashBits = new BitArray(_sizeOfBloomFilter);

            //Creating the history for the bloom filter animation
            history = new int[stringArray.Length][];
            for (int i = 0; i < stringArray.Length; i++)
            {
                history[i] = new int[amountOfHashFunctions];
            }
            //Adding the input to the Bloom filter bit array
            for (int i = 0; i < stringArray.Length; i++)
            {
                Add(stringArray[i], history[i]);
            }

            //Creating a boolean array and filling the bit values in there.
            if (input[0] == "" && input.Length == 1)
            {
                for (int i = 0; i < _sizeOfBloomFilter; i++)
                {
                    boolArray[i] = false;
                }
            }
            else
            {
                for (int i = 0; i < _sizeOfBloomFilter; i++)
                {
                    boolArray[i] = hashBits.Get(i);
                }
            }

            
        }

        public BloomFilter(string[] input, int sizeOfBloomFilter, int amountOfHashFunctions, Boolean ru)
        {
            _amountOfHashFunctions = amountOfHashFunctions;

            _sizeOfBloomFilter = sizeOfBloomFilter;

            boolArray = new bool[_sizeOfBloomFilter];

            //The input is being split in an string array with the divider ','.
            stringArray = input;

            hashBits = new BitArray(_sizeOfBloomFilter);

            //Creating the history for the bloom filter animation
            history = new int[stringArray.Length][];
            for (int i = 0; i < stringArray.Length; i++)
            {
                history[i] = new int[amountOfHashFunctions];
            }
            //Adding the input to the Bloom filter bit array
            for (int i = 0; i < stringArray.Length; i++)
            {
                Add(stringArray[i], history[i]);
            }

            //Creating a boolean array and filling the bit values in there.
            if (input[0] == "" && input.Length == 1)
            {
                for (int i = 0; i < _sizeOfBloomFilter; i++)
                {
                    boolArray[i] = false;
                }
            }
            else
            {
                for (int i = 0; i < _sizeOfBloomFilter; i++)
                {
                    boolArray[i] = hashBits.Get(i);
                }
            }

            running = ru;
            SetIsActionPossible(ru);

        }

        #region Internal methods of the Bloom filter

        /// <summary>
        /// This method is used to calculate the hash values of an input for further use by
        /// the RAPPOR mechanism. It utilizes the sha256 cryptographic hash function.
        /// </summary>
        /// <param name="input">The string input which is to be processesd into a hash value
        /// </param>
        /// <returns>The calculated value of the initial string input, turned into a
        /// integer</returns>
        private int HashString(string input)
        {
            //Implementation inspired from https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256

            //Using the sha256 version to hash values
            SHA256Managed sha256 = new SHA256Managed();

            //Creating a byte array for storing the hash values.
            byte[] crypto = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            //Creating a stringbuilder to store the resulting hash values
            StringBuilder resultHash = new StringBuilder();
            foreach (byte theByte in crypto)
            {
                resultHash.Append(theByte.ToString("x2"));
            }
            //Parsing the string into a big integer
            BigInteger bigInteger = BigInteger.Parse(resultHash.ToString(), NumberStyles.HexNumber);
            //Calculating the integer from the biginteger and the length of the hash bits array.
            int result = (int)(bigInteger % hashBits.Length);//boolArray.Length before
            return result;
        }
        /// <summary>
        /// Adds a new string input to the filter.
        /// </summary>
        /// <param name="input">The string input.</param>
        /// <param name="h">The integer array which represents the history of the bloom
        /// filter.</param>
        public void Add(string input, int[] h)
        {
            //Calculating a hash value for every hash function
            for (int i = 0; i < _amountOfHashFunctions; i++)
            {
                //For every hash function an ascii char is added to the input string to enable
                //differing values. Since SHA 256 is  cryptographic it posseses the property of 
                //diffusion, meaning that adding a char leads to a fully different outcome.
                //It is still deterministic though as the same char is always added.
                int hashedValue = Math.Abs(HashString(input + (char)i + 48));

                //Setting the calculated value of the hashbits array to true.
                hashBits[hashedValue] = true;

                //Setting the history of the bloom filter.
                h[i] = hashedValue;
            }
        }
        /// <summary>
        /// A function that can be used to hash input.
        /// </summary>
        /// <param name="input">The values to be hashed.</param>
        /// <returns>The resulting hash code.</returns>
        public delegate int HashFunction(string input);

        #endregion

        /// <summary>
        /// Returns the Bloom filter of the RAPPOR component
        /// </summary>
        /// <returns>The bloom filter of the component.</returns>
        public bool[] GetBoolArray()
        {
            return boolArray;
        }
        /// <summary>
        /// Returns the history int array of the Bloom filter.
        /// </summary>
        /// <returns>The history int array of the Bloom filter.</returns>
        public int[][] GetHistory()
        {
            return history;
        }
        /// <summary>
        /// Returns a string representation of the Bloom filter.
        /// </summary>
        /// <returns>A string representation of the Bloom filter.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(boolArray.Length);
            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i].ToString() == "True")
                {
                    stringBuilder.Append("1");
                }
                else
                {
                    stringBuilder.Append("0");
                }
            }
            return stringBuilder.ToString();
        }


        /// <summary>
        /// The event handle for the propertys of the Bloom filter.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This method changes the property if an event has occured.
        /// </summary>
        /// <param name="propertyName">The name of the parameter which is to be changed</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// The canvas of the Bloom filter array.
        /// </summary>
        private Canvas _BloomFilterArray;

        /// <summary>
        /// Getter and setter for the Bloom filter array canvas.
        /// </summary>
        public Canvas BloomFilterArray
        {
            get
            {
                if (_BloomFilterArray == null)
                {
                    return null;
                }

                return _BloomFilterArray;
            }
            set
            {
                _BloomFilterArray = value;
                OnPropertyChanged("BloomFilterArray");
            }
        }

        //Disabling button infrastructure
        private bool _isActionPossible;
        public bool IsActionPossible
        {
            get { return _isActionPossible; }
            set
            {
                _isActionPossible = value;
                OnPropertyChanged("IsActionPossible");
            }
        }

        public void SetIsActionPossible(bool isActionPossible)
        {
            IsActionPossible = isActionPossible;
        }
    }
}
