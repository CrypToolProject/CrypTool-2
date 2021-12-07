/*
   Copyright Eduard Scherf, 2021

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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrypTool.Plugins.Blockchain
{
    [Author("Eduard Scherf", "scherfeduard@gmail.com", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.Blockchain.Properties.Resources", "BlockchainCaption", "BlockchainTooltip", "Blockchain/userdoc.xml", "Blockchain/icon.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Blockchain : ICrypComponent
    {
        public const string MINING_REWARD_ADDRESS = "MINING_REWARD";

        #region Private Variables

        private readonly BlockchainSettings _settings = new BlockchainSettings();        
        private readonly List<Transaction> _pendingTransactions = new List<Transaction>();
        private readonly List<Address> _allAddresses = new List<Address>();
        private readonly List<Transaction> _failedTransactions = new List<Transaction>();
        private readonly BlockchainPresentation _presentation = new BlockchainPresentation();

        private List<Block> _chain = new List<Block>();
        private bool _executing = false;
        public string _hashAlgorithmName = string.Empty;
        private Address _miningAddress;        
        private HashAlgorithm _hashAlgorithm = null;
        private int miningDifficultyLimit = 0;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PreviousblockCaption", "PreviousblockTooltip", false)]
        public string PreviousBlock
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "TransactionCaption", "TransactionTooltip")]
        public string Transaction_data
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AddressesCaption", "AddressesTooltip", false)]
        public string Addresses
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "nextblockCaption", "nextblockTooltip", false)]
        public string NextBlock
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members
        public ISettings Settings
        {
            get { return _settings; }
        }
       
        public UserControl Presentation
        {
            get { return _presentation; }
        }
      
        public void PreExecution()
        {
            //reset the ui at first run
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.BlockId.Value = string.Empty;
                _presentation.BlockHash.Value = string.Empty;
                _presentation.PreviousBlockHash.Value = string.Empty;
                _presentation.Transactions.Value = string.Empty;
                _presentation.FailedTransactions.Value = string.Empty;
                _presentation.Timestamp.Value = string.Empty;
                _presentation.Nonce.Value = string.Empty;
                _presentation.CurrentHashingSpeed.Value = string.Empty;
                _presentation.MiningDifficulty.Value = string.Empty;
                _presentation.TransactionList.Clear();
                _presentation.BalanceList.Clear();
            }, null);
        }
        
        public void Execute()
        {
            _executing = true;
            ProgressChanged(0.25, 1);                       
            try
            {
                //reset internal lists
                _chain.Clear();
                _allAddresses.Clear();
                _failedTransactions.Clear();
                _pendingTransactions.Clear();

                // get addresses from input and create addresses, and set mining address from settings
                SetAddresses();

                //set the hash algorithm
                SetHashAlgorithm();

                //Check, if difficulty is possible
                if(VerifyDifficulty() == false)
                {
                    _executing = false;
                    return;
                }

                //init variables
                _pendingTransactions.Clear();
                var miningRewardAddress = new Address(MINING_REWARD_ADDRESS);
                
                //init previous blocks if not null
                if (!string.IsNullOrEmpty(PreviousBlock)) 
                {
                    if (IsJson(PreviousBlock) == true)
                    {
                        _chain = JsonConvert.DeserializeObject<List<Block>>(PreviousBlock);
                        //we have to give each transaction a hash algorithm since the transactions
                        //recompute their hashvalue based on their values
                        foreach(var block in _chain)
                        {
                            foreach(var transaction in block.Transactions)
                            {
                                transaction.SetHashAlgorithm(_hashAlgorithm);
                            }
                        }
                        //check, if the deserialized chain is valid
                        string errorMessage = null;
                        if(!IsChainValid(out errorMessage))
                        {
                            _executing = false;
                            GuiLogMessage(string.Format("{0}: {1}", Properties.Resources.InvalidChain, errorMessage), NotificationLevel.Error);
                            return;
                        }
                    }
                    else
                    {
                        _executing = false;
                        GuiLogMessage(Properties.Resources.NoJSONStringCaption,NotificationLevel.Error);
                        return;
                    }
                }
                //init gen block if previous blocks are null
                else
                {
                    List<Transaction> genesisList = new List<Transaction>();
                    DateTime generationTime = DateTime.Now;

                    foreach (var address in _allAddresses)
                    {
                        if (_settings.Mining_Address == address.Name)
                        {
                            _miningAddress = address;
                        }
                    }

                    if (_miningAddress == null)
                    {
                        _miningAddress = new Address(_settings.Mining_Address);
                        GuiLogMessage(Properties.Resources.NoValidMiningAddressCaption, NotificationLevel.Warning);
                    }
                    else
                    {
                        Transaction gen_transaction = new Transaction(miningRewardAddress, _miningAddress, _settings.MiningReward, "0", _hashAlgorithm);
                        gen_transaction.Status = Properties.Resources.SuccessfullCaption;
                        genesisList.Add(gen_transaction);
                    }                                                             
                    AddGenesisBlock(new Block(generationTime.ToString(), genesisList));               
                }

                ProgressChanged(0.5, 1);

                //set mining address
                foreach (var address in _allAddresses)
                {
                    if (_settings.Mining_Address.ToString() == address.Name)
                    {
                        _miningAddress = address;
                    }
                }

                if(_miningAddress == null)
                {
                    _miningAddress = new Address(_settings.Mining_Address);
                    GuiLogMessage(Properties.Resources.NoValidMiningAddressCaption, NotificationLevel.Warning);
                }

                //check transaction data
                if (!string.IsNullOrEmpty(Transaction_data))
                {                    
                    if (!string.IsNullOrEmpty(PreviousBlock)) 
                    {
                        ReadTransactions(Transaction_data);
                        MinePendingTransactions(_miningAddress, miningRewardAddress);
                        if (_executing == false) //it can happen, that the user pressed stop while mining
                        {
                            return;
                        }                        
                    }
                    else
                    {
                            GuiLogMessage(Properties.Resources.GenTransactionWarning, NotificationLevel.Warning);
                            //read transactions
                            ReadTransactions(Transaction_data);
                    }
                    //update user interface
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _presentation.TransactionList.Clear();
                        _presentation.BalanceList.Clear();
                    }, null);
                    UpdateBalanceUI();
                }
                else
                {
                    if (!string.IsNullOrEmpty(PreviousBlock)) 
                    {                       
                        MinePendingTransactions(_miningAddress, miningRewardAddress);                       
                    }
                    //update user interface
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _presentation.TransactionList.Clear();
                        _presentation.BalanceList.Clear();
                    }, null);
                    UpdateBalanceUI();
                }
                              
                //presentation
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    //transaction presentation
                    foreach (var transaction in GetPreviousBlock().Transactions)
                    {
                        _presentation.TransactionList.Add(transaction);
                    }

                    //failed transactions presentation
                    foreach (var transaction in _failedTransactions)
                    {                        
                        _presentation.TransactionList.Add(transaction);
                    }

                    var latestBlock = GetPreviousBlock();
                    _presentation.BlockId.Value = latestBlock.BlockId.ToString();
                    _presentation.BlockHash.Value = latestBlock.Hash;
                    _presentation.PreviousBlockHash.Value = latestBlock.PreviousHash;
                    _presentation.Transactions.Value = latestBlock.Transactions.Count.ToString();
                    _presentation.FailedTransactions.Value = _failedTransactions.Count.ToString();
                    _presentation.Timestamp.Value = latestBlock.Timestamp;
                    _presentation.Nonce.Value = latestBlock.Nonce.ToString("N0");
                    _presentation.HashAlgorithm.Value = _hashAlgorithmName;
                    _presentation.MiningDifficulty.Value = _settings.MiningDifficulty.ToString();
                }, null);

                NextBlock = JsonConvert.SerializeObject(_chain);
                OnPropertyChanged("NextBlock");
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ExceptionCaption, ex.Message), NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
            _executing = false;
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            _executing = false;          
        }

        public void Initialize()
        {
        }

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

        #region HelperMethods

        public void SetAddresses()
        {
            using (StringReader stringReader = new StringReader(Addresses))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("#"))
                    {
                        continue;
                    }
                    string[] data = line.Split(',');
                    if (CheckAddresses(line) == true)
                    {
                        if (!data[0].Equals(MINING_REWARD_ADDRESS))
                        {                           
                            var newAddress = new Address(data[0]);
                            newAddress.PublicKey = (BigInteger.Parse(data[1]), BigInteger.Parse(data[2]));
                            newAddress.PrivateKey = (BigInteger.Parse(data[1]), BigInteger.Parse(data[3]));
                            _allAddresses.Add(newAddress);                           
                        }
                        else
                        {
                            GuiLogMessage(Properties.Resources.InvalidAddressName + " '" + data[0] + "'", NotificationLevel.Warning);
                        }
                    }
                    else
                    {
                        GuiLogMessage(Properties.Resources.InvalidAddressCaption, NotificationLevel.Warning);
                    }
                }

                //default Mining Address
                bool mining_address_found = false;

                foreach (var address in _allAddresses)
                {
                    if (address.Name == _settings.Mining_Address.ToString())
                    {
                        SetMiningAddress(address);
                        mining_address_found = true;
                    }
                }

                if (mining_address_found == false)
                {
                    GuiLogMessage(Properties.Resources.MiningAddressErrorCaption, NotificationLevel.Warning);
                }
            }
        }

        public void AddGenesisBlock(Block genesisBlock)
        {
            int difficulty = _settings.MiningDifficulty;
            genesisBlock.PreviousHash = "0";
            genesisBlock.MineBlock(difficulty, ref _executing, _presentation, _hashAlgorithm);
            _chain.Add(genesisBlock);
        }

        public Block GetPreviousBlock()
        {
            return _chain[_chain.Count - 1];
        }

        public void MinePendingTransactions(Address miningRewardAddress, Address miningrewardfromaddress)
        {
            Transaction transaction = new Transaction(miningrewardfromaddress, miningRewardAddress, _settings.MiningReward, "0", _hashAlgorithm);
            transaction.Status = Properties.Resources.SuccessfullCaption;
            _pendingTransactions.Add(transaction);

            int difficulty = _settings.MiningDifficulty;
            DateTime now = DateTime.Now;
            Block newBlock = new Block(now.ToString(), _pendingTransactions);

            if (string.IsNullOrEmpty(PreviousBlock))
            {
                newBlock.BlockId = 0;
                newBlock.PreviousHash = "0";
            }
            else 
            {
                var previousBlock = GetPreviousBlock();
                newBlock.BlockId = previousBlock.BlockId + 1;
                newBlock.PreviousHash = previousBlock.Hash;
            }
            newBlock.MineBlock(difficulty, ref _executing, _presentation, _hashAlgorithm);
            _chain.Add(newBlock);
        }

        /// <summary>
        /// Checks, if the current stored chain is valid by checking hashes, ids, etc.
        /// </summary>
        /// <returns></returns>
        public bool IsChainValid(out string errorMessage)
        {
            Block previousBlock = null;
            foreach(var block in _chain)
            {          
                //check, if previous hash is the hash of the previous block 
                if(previousBlock != null)
                {                    
                    if(block.PreviousHash != previousBlock.Hash)
                    {
                        errorMessage = "Previous block hash wrong";
                        return false;
                    }
                    if (block.BlockId != previousBlock.BlockId + 1)
                    {
                        errorMessage = "Previous block id wrong";
                        return false;
                    }
                }
                //in case of genesis block, check if previous hash is "0" and block id is 0
                else
                {
                    if (!block.PreviousHash.Equals("0"))
                    {
                        errorMessage = "Genesis block previous hash has to be 0";
                        return false;
                    }
                    if (block.BlockId != 0)
                    {
                        errorMessage = "Genesis block previous block id has to be 0";
                        return false;
                    }
                }

                int numberOfZerosInHash = 0;
                //now, recompute the current blocks hash and compare it to the received one
                //also, we compute the number of zeros in that hash
                if (!block.ComputeBlockHashAndCountNumberOfZeros(_hashAlgorithm, out numberOfZerosInHash).Equals(block.Hash))
                {
                    errorMessage = "Block hash is invalid";
                    return false;
                }

                //now check, if the hash corresponds to promized difficulty of that block
                if(numberOfZerosInHash < block.Difficulty)
                {
                    errorMessage = "Number of zeros in block hash is too low";
                    return false;
                }
                previousBlock = block;
            }
            errorMessage = "No error";
            return true;
        }      

        public Balance GetBalance(Address address)
        {
            double balance = 0;
            foreach (var block in _chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.FromAddress.Name == address.Name)
                    {
                        balance -= transaction.Amount;
                    }
                    if (transaction.ToAddress.Name == address.Name)
                    {
                        balance += transaction.Amount;
                    }
                }
            }

            foreach (var transaction in _pendingTransactions)
            {
                if (transaction.FromAddress.Name == address.Name)
                {
                    balance -= transaction.Amount;
                }
                if (transaction.ToAddress.Name == address.Name)
                {
                    balance += transaction.Amount;
                }
            }

            return new Balance { Address = address, Value = balance};
        }

        public Balance GetBalanceForUI(Address address)
        {
            double balance = 0;
            foreach (var block in _chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.FromAddress.Name == address.Name)
                    {
                        balance -= transaction.Amount;
                    }
                    if (transaction.ToAddress.Name == address.Name)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return new Balance { Address = address, Value = balance };
        }

        public double GetPendingTransactionBalance(Address address)
        {
            double balance = 0;

            foreach (var transaction in _pendingTransactions)
            {
                if (transaction.FromAddress.Name == address.Name)
                {
                    balance -= transaction.Amount;
                }
                if (transaction.ToAddress.Name == address.Name)
                {
                    balance += transaction.Amount;
                }
            }

            return balance;
        }

        public void StringToTransaction(string transactionString)
        {
            string[] data = transactionString.Split(',');
            Address from = null;
            Address to = null;            
            if (data.Length != 4)
            {
                GuiLogMessage(Properties.Resources.InvalidTransactionCaption, NotificationLevel.Warning);
                return;
            }
            var amount = double.Parse(data[2], CultureInfo.InvariantCulture.NumberFormat);

            foreach (var address in _allAddresses)
            {
                if (address.Name == data[0])
                {
                    from = address;
                }
                if (address.Name == data[1])
                {
                    to = address;
                }
            }

            if (amount <= 0 )
            {
                Transaction transaction = new Transaction(from, to, amount, data[3], _hashAlgorithm);
                _failedTransactions.Add(transaction);
                transaction.Status = Properties.Resources.InvalidValueCaption;
                GuiLogMessage(Properties.Resources.NegativeValueCaption, NotificationLevel.Warning);
                return;
            }

            if(CheckAddressInTransaction(data[0]) == false || CheckAddressInTransaction(data[1]) == false)
            {
                GuiLogMessage(Properties.Resources.InvalidAddressInTransaction, NotificationLevel.Warning);
                return;
            }

            if (GetBalance(from).Value >= amount) 
            {
                if (_pendingTransactions.Count == 0)
                {
                    Transaction transaction = new Transaction(from, to, amount, data[3], _hashAlgorithm);

                    bool check = VerifySignature(from.PublicKey, from.Name, to.Name, transaction.Amount, transaction.Signature);
                    if (check == true)
                    {
                        _pendingTransactions.Add(transaction);
                        transaction.Note = Properties.Resources.SignatureVerifiedCaption;
                        transaction.Status = Properties.Resources.SuccessfullCaption;
                    }
                    else
                    {
                        _failedTransactions.Add(transaction);
                        transaction.Status = Properties.Resources.BadSignatureCaption;
                    }
                }
                else
                {
                    Transaction transaction = new Transaction(from, to, amount, GetLatestTransaction(_pendingTransactions).Hash, _hashAlgorithm);

                    if (VerifySignature(from.PublicKey, from.Name, to.Name, transaction.Amount, transaction.Signature))
                    {
                        _pendingTransactions.Add(transaction);
                        transaction.Note = Properties.Resources.SignatureVerifiedCaption;
                        transaction.Status = Properties.Resources.SuccessfullCaption;
                    }
                    else
                    {
                        _failedTransactions.Add(transaction);
                        transaction.Status = Properties.Resources.BadSignatureCaption;
                        GuiLogMessage(Properties.Resources.BadSignatureInCaption + transaction.FromAddress.Name + Properties.Resources.toCaption1 + transaction.ToAddress.Name + " " + transaction.Amount + " " + Properties.Resources.CoinCaption, NotificationLevel.Warning);
                    }
                }
            }
            else
            {
                if (_pendingTransactions.Count == 0)
                {
                    Transaction transaction = new Transaction(from, to, amount, "0", _hashAlgorithm);
                    _failedTransactions.Add(transaction);
                    GuiLogMessage(Properties.Resources.InsuffiecientBalanceInCaption + " " + transaction.FromAddress.Name + " " + Properties.Resources.toCaption1 + " " + transaction.ToAddress.Name + " " + transaction.Amount + " " + Properties.Resources.CoinCaption, NotificationLevel.Warning);
                    transaction.Status = Properties.Resources.InsufficientBalanceCaption;
                }
                else
                {
                    Transaction transaction = new Transaction(from, to, amount, GetLatestTransaction(_pendingTransactions).Hash,  _hashAlgorithm);
                    _failedTransactions.Add(transaction);
                    GuiLogMessage(Properties.Resources.InsuffiecientBalanceInCaption + " " + transaction.FromAddress.Name + " " + Properties.Resources.toCaption1 + " " + transaction.ToAddress.Name + " " + transaction.Amount + " " + Properties.Resources.CoinCaption, NotificationLevel.Warning);
                    transaction.Status = Properties.Resources.InsufficientBalanceCaption;
                }
            }
        }

        public void ReadTransactions(string Transaction_data)
        {
            using (var stringReader = new StringReader(Transaction_data))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("#"))
                    {
                        continue;
                    }
                    if (CheckTransaction(Transaction_data) == true)
                    {
                        StringToTransaction(line);
                    }
                    else
                    {
                        GuiLogMessage(Properties.Resources.InvalidTransactionCaption, NotificationLevel.Warning);
                    }
                }
            }
        }

        public void SetMiningAddress(Address address)
        {
            _miningAddress = address;
        }

        public Transaction GetLatestTransaction(List<Transaction> list)
        {
            return list[list.Count - 1];
        }

        public bool VerifySignature((BigInteger N, BigInteger e) pubKey, string from, string to, double amount, string signature)
        {            
            var preImage = Encoding.UTF8.GetBytes(from + to + amount);
            SHA256 sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(preImage);
            BigInteger hashBigInt = new BigInteger(hash);

            if (hashBigInt < BigInteger.Zero)
            {
                hashBigInt = hashBigInt * BigInteger.MinusOne;
            }

            hashBigInt = BigInteger.ModPow(hashBigInt, BigInteger.One, pubKey.N);
            BigInteger signatureBigInt = BigInteger.Parse(signature);
            BigInteger hashBigInt2 = BigInteger.ModPow(signatureBigInt, pubKey.e, pubKey.N);

            return hashBigInt == hashBigInt2;
        }

        public void SetHashAlgorithm()
        {
            switch (_settings.Hash_Algorithm)
            {
                default:
                case HashAlgorithms.SHA1:
                    _hashAlgorithm = new SHA1Managed();
                    _hashAlgorithmName = "SHA1";
                    miningDifficultyLimit = 160;
                    break;

                case HashAlgorithms.SHA256:
                    _hashAlgorithm = new SHA256Managed();
                    _hashAlgorithmName = "SHA256";
                    miningDifficultyLimit = 256;
                    break;

                case HashAlgorithms.SHA512:
                    _hashAlgorithm = new SHA512Managed();
                    _hashAlgorithmName = "SHA512";
                    miningDifficultyLimit = 512;
                    break;
            }
        }

        public bool VerifyDifficulty()
        {
            if (_hashAlgorithmName == "SHA1" && miningDifficultyLimit > 160)
            {
                GuiLogMessage(Properties.Resources.MiningDifficultyWarning, NotificationLevel.Error);
                return false;
            }
            if (_hashAlgorithmName == "SHA256" && miningDifficultyLimit > 256)
            {
                GuiLogMessage(Properties.Resources.MiningDifficultyWarning, NotificationLevel.Error);
                return false;
            }
            if (_hashAlgorithmName == "SHA512" && miningDifficultyLimit > 512)
            {
                GuiLogMessage(Properties.Resources.MiningDifficultyWarning, NotificationLevel.Error);
                return false;
            }
            return true;
        }

        public void UpdateBalanceUI()
        {
            foreach (var address in _allAddresses)
            {
                var balance = GetBalanceForUI(address);
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.BalanceList.Add(balance);
                }, null);
            }
        }

        public static bool IsJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) 
            { 
                return false; 
            }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }               
                catch (Exception) //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool CheckTransaction(string transaction)
        {
            if (Regex.IsMatch(transaction, "[a-zA-Z]+[,][a-zA-Z]+[,][0-9]+[.]*[0-9]*[,][0-9]+"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckAddressInTransaction(string address)
        {

            foreach (var address_single in _allAddresses)
            {
                if (address_single.Name == address)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckAddresses(string address)
        {
            if (Regex.IsMatch(address, "[#]?[a-zA-Z]+[,][0-9]+[0-9]+[,][0-9]+[,][0-9]+"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public class Block 
        {            
            public Block(string timestamp, List<Transaction> transactions) 
            {
                Timestamp = timestamp;
                Transactions = transactions;
            }

            /// <summary>
            /// This method is for the chain verification. It recomputes the hash value the same way the mining method does.
            /// Also, it counts the number of zeros
            /// </summary>
            /// <param name="hashAlgorithm"></param>
            /// <param name="numberOfZerosInHashvalue"></param>
            /// <returns></returns>
            public string ComputeBlockHashAndCountNumberOfZeros(HashAlgorithm hashAlgorithm, out int numberOfZerosInHashvalue)
            {
                byte[] hash;
                var transactions = CreateTransactionsHashString();
                var blockPreImageString = BlockId.ToString() + Timestamp + PreviousHash + Difficulty.ToString() + transactions;
                var blockPreImageBytes = Encoding.UTF8.GetBytes(blockPreImageString);
                var blockPreImageBytesPlusNonce = new byte[blockPreImageBytes.Length + 8];
                Array.Copy(blockPreImageBytes, blockPreImageBytesPlusNonce, blockPreImageBytes.Length);               
                var nonceBytes = BitConverter.GetBytes(Nonce);
                Array.Copy(nonceBytes, 0, blockPreImageBytesPlusNonce, blockPreImageBytesPlusNonce.Length - 8, 8);
                hash = CalculateHash(blockPreImageBytesPlusNonce, 10, hashAlgorithm);
                numberOfZerosInHashvalue = CountZeros(hash);
                return ConvertToHexString(hash);
            }

            public void MineBlock(int difficulty, ref bool executing, BlockchainPresentation presentation, HashAlgorithm hashAlgorithm)
            {
                Difficulty = difficulty; //Memorize difficulty used for mining this block
                byte[] hash;
                var transactions = CreateTransactionsHashString();
                var blockPreImageString = BlockId.ToString() + Timestamp + PreviousHash + difficulty + transactions;
                var blockPreImageBytes = Encoding.UTF8.GetBytes(blockPreImageString);
                var blockPreImageBytesPlusNonce = new byte[blockPreImageBytes.Length + 8];
                Array.Copy(blockPreImageBytes, blockPreImageBytesPlusNonce, blockPreImageBytes.Length);
                var updateUITime = DateTime.Now.AddSeconds(1);
                var counter = 0;

                do
                {
                    Nonce++;
                    var nonceBytes = BitConverter.GetBytes(Nonce);
                    Array.Copy(nonceBytes, 0, blockPreImageBytesPlusNonce, blockPreImageBytesPlusNonce.Length - 8, 8);
                    hash = CalculateHash(blockPreImageBytesPlusNonce, 10, hashAlgorithm);
                    counter++;

                    if (DateTime.Now >= updateUITime)
                    {
                        presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            presentation.CurrentHashingSpeed.Value = counter.ToString("N0");
                        }, null);

                        counter = 0;
                        updateUITime = DateTime.Now.AddSeconds(1);
                    }

                } while (executing && CountZeros(hash) < difficulty);

                Hash = ConvertToHexString(hash);
            }

            private string CreateTransactionsHashString()
            {
                var builder = new StringBuilder();
                foreach (var transaction in Transactions)
                {
                    builder.AppendLine(transaction.Hash);
                }
                return builder.ToString();
            }

            public static string ConvertToHexString(byte[] array)
            {
                return BitConverter.ToString(array).Replace("-", "");
            }

            public int BlockId
            { 
                get;
                set;
            }

            public string Hash
            {
                get;
                set;
            }

            public string PreviousHash
            {
                get;
                set;
            }

            public string Timestamp
            {
                get;
                set;
            }           

            public int Difficulty
            {
                get;
                set;
            }

            public long Nonce
            {
                get;
                set;
            }

            public List<Transaction> Transactions
            {
                get;
                set;
            }

            public static byte[] CalculateHash(byte[] preImage, int length = 10, HashAlgorithm algorithm = null)
            {
                if (algorithm == null)
                {
                    algorithm = new SHA256Managed();
                }
                var hash = algorithm.ComputeHash(preImage);
                var array = new byte[length];
                Array.Copy(hash, array, length);
                return array;
            }

            public static int CountZeros(byte[] array)
            {
                var count = 0;
                var position = 0;

                while (position < array.Length && array[position] == 0)
                {
                    count += 8;
                    position++;
                }

                if (position == array.Length - 1)
                {
                    return count;
                }
                var bitmask = 128;

                while ((bitmask & array[position]) == 0)
                {
                    count += 1;
                    bitmask = bitmask >> 1;
                }

                return count;
            }
        }

        public class Transaction 
        {
            private HashAlgorithm _hashAlgorithm;

            public Transaction(Address fromAddress, Address toAddress, double amount, string signature, HashAlgorithm hashAlgorithm) 
            {
                FromAddress = fromAddress;
                ToAddress = toAddress;
                Amount = amount;
                Timestamp = DateTime.Now.ToString();
                Signature = signature;
                Note = string.Empty;
                SetHashAlgorithm(hashAlgorithm);
            }

            public void SetHashAlgorithm(HashAlgorithm hashAlgorithm)
            {
                _hashAlgorithm = hashAlgorithm;
            }

            private byte[] CalculateHash(byte[] preImage, int length = 10)
            {              
                var hash = _hashAlgorithm.ComputeHash(preImage);
                var array = new byte[length];
                Array.Copy(hash, array, length);
                return array;
            }

            private string ConvertToHexString(byte[] array)
            {
                return BitConverter.ToString(array).Replace("-", "");
            }

            [JsonIgnore]
            public string Hash
            {
                get
                {
                    return ConvertToHexString(CalculateHash(Encoding.UTF8.GetBytes(FromAddress.Name + FromAddress.PublicKey + ToAddress.Name + ToAddress.PublicKey + Amount + Timestamp + Signature), 10));
                }
            }

            public Address FromAddress
            {
                get;
                set;
            }

            public Address ToAddress
            {
                get;
                set;
            }

            public double Amount
            {
                get;
                set;
            }

            public string Timestamp
            {
                get;
                set;
            }

            public string Signature
            {
                get;
                set;
            }

            [JsonIgnore]
            public string Status
            {
                get;
                set;
            }

            [JsonIgnore]
            public string Note
            {
                get;
                set;
            }

        }

        public class Address 
        {
            public Address(string name)
            {
                Name = name;
            }

            public string Name
            {
                get;
                set;
            }

            public (BigInteger N, BigInteger e) PublicKey
            {
                get;
                set;
            }

            [JsonIgnore]
            public (BigInteger N, BigInteger d) PrivateKey
            {
                get;
                set;
            }

            public override string ToString()
            {
                if (Name.Equals(MINING_REWARD_ADDRESS))
                {
                    return MINING_REWARD_ADDRESS;
                }
                return string.Format("{0} (N={1}, e={2})", Name, PublicKey.N, PublicKey.e);
            }

        }

        public class Balance
        {
            public double Value
            {
                get;
                set;
            }

            public Address Address
            {
                get;
                set;
            }

            public string Name
            {
                get
                {
                    if (Address == null || Address.Name == null)
                    {
                        return "null";
                    }
                    return Address.Name;
                }
            }
        }
    }
}
