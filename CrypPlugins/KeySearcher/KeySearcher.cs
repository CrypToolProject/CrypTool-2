/*                              
   Copyright 2009 Sven Rech, Nils Kopal, Uni Duisburg-Essen

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

using CrypCloud.Core.CloudComponent;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.IO;
using KeySearcher.CrypCloud;
using KeySearcher.Helper;
using KeySearcher.Properties;
using KeySearcherPresentation;
using KeySearcherPresentation.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace KeySearcher
{
    [Author("Sven Rech, Nils Kopal, Raoul Falk, Dennis Nolte", "rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("KeySearcher.Properties.Resources", "PluginCaption", "PluginTooltip", "KeySearcher/DetailedDescription/doc.xml", "KeySearcher/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class KeySearcher : ACloudCompatible
    {

        /// <summary>
        /// Delegate for outputting user selected plaintext and key
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        public delegate void UpdateOutput(string plaintext, string key);

        /// <summary>
        /// used for creating the UserStatistics
        /// </summary>
        private Dictionary<string, Dictionary<long, Information>> statistic;
        private Dictionary<long, MachInfo> machineHierarchy;

        /// <summary>
        /// used for creating the TopList
        /// </summary>
        private Queue valuequeue;
        private double valueThreshold;

        /// <summary>
        /// the thread with the most keys left
        /// </summary>
        private int maxThread;
        private readonly Mutex maxThreadMutex = new Mutex();
        private ArrayList threadsStopEvents;
        public bool IsKeySearcherRunning;

        private readonly DateTime defaultstart = DateTime.MinValue;
        private string username;
        private readonly long maschineid;

        // GUI
        private readonly P2PQuickWatchPresentation p2PQuickWatchPresentation;
        private readonly LocalQuickWatchPresentation localQuickWatchPresentation;

        private readonly Stopwatch localBruteForceStopwatch;

        private KeyPattern.KeyPattern pattern;
        public KeyPattern.KeyPattern Pattern
        {
            get => pattern;
            set
            {
                pattern = value;
                settings.KeyManager.Format = value.GetPattern();
            }
        }

        public bool IsKeySearcherFinished
        {
            get; private set;
        }

        internal bool stop;

        internal bool update;


        #region IControlEncryption + IControlCost + InputFields

        #region IControlEncryption Members

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip")]
        public IControlEncryption ControlMaster
        {
            get => controlMaster;
            set
            {
                if (controlMaster != null)
                {
                    controlMaster.KeyPatternChanged -= keyPatternChanged;
                }
                if (value != null)
                {
                    Pattern = new KeyPattern.KeyPattern(value.GetKeyPattern());
                    value.KeyPatternChanged += keyPatternChanged;
                    controlMaster = value;
                    OnPropertyChanged("ControlMaster");

                }
                else
                {
                    controlMaster = null;
                }
            }
        }



        #endregion

        #region IControlCost Members

        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip")]
        public IControlCost CostMaster
        {
            get => costMaster;
            set => costMaster = value;
        }

        #endregion

        /* BEGIN: following lines are from Arnie - 2010.01.12 */
        private ICrypToolStream csEncryptedData;
        [PropertyInfo(Direction.InputData, "CSEncryptedDataCaption", "CSEncryptedDataTooltip", false)]
        public virtual ICrypToolStream CSEncryptedData
        {
            get => csEncryptedData;
            set
            {
                if (value != csEncryptedData)
                {
                    csEncryptedData = value;
                    encryptedData = GetByteFromCrypToolStream(value);
                    OnPropertyChanged("CSEncryptedData");
                }
            }
        }

        private byte[] encryptedData;
        private byte[] encryptedDataOptimized;
        [PropertyInfo(Direction.InputData, "EncryptedDataCaption", "EncryptedDataTooltip", false)]
        public virtual byte[] EncryptedData
        {
            get => encryptedData;
            set
            {
                if (value != encryptedData)
                {
                    encryptedData = value;
                    OnPropertyChanged("EncryptedData");
                }
            }
        }

        /// <summary>
        /// When the Input-Slot changed, set this variable to true, so the new Stream will be transformed to byte[]
        /// </summary>
        private byte[] GetByteFromCrypToolStream(ICrypToolStream CrypToolStream)
        {
            byte[] encryptedByteData = null;

            if (CrypToolStream != null)
            {
                using (CStreamReader reader = CrypToolStream.CreateReader())
                {
                    if (reader.Length > int.MaxValue)
                    {
                        throw (new Exception("CrypToolStream length is longer than the Int32.MaxValue"));
                    }

                    encryptedByteData = reader.ReadFully();
                }
            }
            return encryptedByteData;
        }

        private byte[] initVector;
        private byte[] initVectorOptimized;
        [PropertyInfo(Direction.InputData, "InitVectorCaption", "InitVectorTooltip", false)]
        public virtual byte[] InitVector
        {
            get => initVector;
            set
            {
                initVector = value;
                OnPropertyChanged("InitVector");
            }
        }
        /* END: Lines above are from Arnie - 2010.01.12 */

        private ValueKey top1ValueKey;
        private byte[] top1FullPlaintext;
        public virtual ValueKey Top1
        {
            private set
            {
                top1ValueKey = value;
                top1Key = value.keya;
                top1FullPlaintext = sender.Decrypt(encryptedData, value.keya, initVector);

                OnPropertyChanged("Top1Message");
                OnPropertyChanged("Top1Key");
            }
            get => top1ValueKey;
        }

        [PropertyInfo(Direction.OutputData, "Top1MessageCaption", "Top1MessageTooltip")]
        public byte[] Top1Message
        {
            get => top1FullPlaintext;
            private set
            {
                top1FullPlaintext = value;
                OnPropertyChanged("Top1Message");
            }
        }

        private byte[] top1Key;
        [PropertyInfo(Direction.OutputData, "Top1KeyCaption", "Top1KeyTooltip")]
        public byte[] Top1Key
        {
            get => top1Key;
            private set
            {
                top1Key = value;
                OnPropertyChanged("Top1Key");
            }
        }

        #endregion

        #region external client variables 
        private readonly IKeyTranslator externalKeyTranslator;
        private BigInteger externalKeysProcessed;
        /// <summary>
        /// List of clients which connected while there was no job available. Will receive
        /// a job a soon as one is available.
        /// </summary>
        private readonly List<EndPoint> waitingExternalClients = new List<EndPoint>();
        private Guid currentExternalJobGuid = Guid.NewGuid();
        private readonly AutoResetEvent waitForExternalClientToFinish = new AutoResetEvent(false);
        private readonly DateTime assignTime;
        private readonly bool externalClientJobsAvailable = false;

        /// <summary>
        /// id of the client which calculated the last pattern
        /// </summary>
        public long ExternaClientId { get; private set; }

        /// <summary>
        /// Hostname of the client which calculated the last pattern
        /// </summary>
        public string ExternalClientHostname { get; private set; }
        #endregion

        public KeySearcher()
        {
            IsKeySearcherRunning = false;
            username = "";
            maschineid = 0;

            settings = new KeySearcherSettings(this);

            Presentation = new QuickWatch();
            localQuickWatchPresentation = ((QuickWatch)Presentation).LocalQuickWatchPresentation;
            p2PQuickWatchPresentation = ((QuickWatch)Presentation).P2PQuickWatchPresentation;

            settings.PropertyChanged += SettingsPropertyChanged;
            localBruteForceStopwatch = new Stopwatch();
            if (JobId != 0)
            {
                p2PQuickWatchPresentation.ViewModel.UpdateStaticView(JobId, this, settings);
            }

            localQuickWatchPresentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
            p2PQuickWatchPresentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
        }

        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {
            Top1Key = StringToByteArray(keyString.Replace("-", ""));
            Top1Message = Encoding.GetEncoding(1252).GetBytes(plaintextString);
        }

        public byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {

                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            }
            return bytes;
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            p2PQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             new Action(UpdateQuickwatchSettings));
        }

        private void UpdateQuickwatchSettings()
        {
            ((QuickWatch)Presentation).IsP2PEnabled = settings.UsePeerToPeer;
            p2PQuickWatchPresentation.UpdateSettings(this, settings);
        }

        #region IPlugin Members

        public override event StatusChangedEventHandler OnPluginStatusChanged;
        public override event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public override event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private readonly KeySearcherSettings settings;
        private readonly AutoResetEvent connectResetEvent;

        public override ISettings Settings => settings;

        public override UserControl Presentation
        {
            get;
            set;
        }

        public override void PreExecution()
        {
            update = false;

            if (JobId != 0)
            {
                p2PQuickWatchPresentation.ViewModel.UpdateStaticView(JobId, this, settings);
            }
        }

        // because Encryption PlugIns were changed radical, the new StartPoint is here - Arnie 2010.01.12
        public override void Execute()
        {
            IsKeySearcherFinished = false;
            try
            {
                IsKeySearcherRunning = true;
                localBruteForceStopwatch.Reset();

                //either byte[] CStream input or CrypToolStream Object input
                if (encryptedData != null || csEncryptedData != null) //to prevent execution on initialization
                {
                    if (ControlMaster == null)
                    {
                        GuiLogMessage(Resources.You_have_to_connect_the_KeySearcher_with_the_Decryption_Control_, NotificationLevel.Warning);
                    }
                    else if (CostMaster == null)
                    {
                        GuiLogMessage(Resources.You_have_to_connect_the_KeySearcher_with_the_Cost_Control_, NotificationLevel.Warning);
                    }
                    else
                    {
                        process(ControlMaster);
                    }
                }
            }
            finally
            {
                IsKeySearcherFinished = true;
            }
            //the following code avoids to let KeySearcher terminate while CrypCloud is running
            //this termination caused a "too early" 100% progress
            if (settings.UsePeerToPeer)
            {
                while (IsKeySearcherRunning)
                {
                    Thread.Sleep(150);
                }
            }
        }

        public override void PostExecution()
        {
        }

        public override void Stop()
        {
            IsKeySearcherRunning = false;
            stop = true;
            waitForExternalClientToFinish.Set();
            if (cloudKeySearcher != null)
            {
                cloudKeySearcher.Stop();
            }
        }

        public override void Initialize()
        {
            if (p2PQuickWatchPresentation != null)
            {
                p2PQuickWatchPresentation.UpdateSettings(this, settings);
                if (JobId != 0)
                {
                    p2PQuickWatchPresentation.ViewModel.UpdateStaticView(JobId, this, settings);
                }
            }
        }

        public override void Dispose()
        {
        }

        #endregion

        #region whole KeySearcher functionality

        private class ThreadStackElement
        {
            public AutoResetEvent ev;
            public int threadid;
        }

        #region code for the worker threads

        /// <summary>
        /// This is the working method for a worker thread which task it is to bruteforce.
        /// </summary>
        private void KeySearcherJob(object param)
        {
            AutoResetEvent stopEvent = new AutoResetEvent(false);
            threadsStopEvents.Add(stopEvent);

            //extract parameters:
            object[] parameters = (object[])param;
            KeyPattern.KeyPattern[] patterns = (KeyPattern.KeyPattern[])parameters[0];
            int threadid = (int)parameters[1];
            BigInteger[] doneKeysArray = (BigInteger[])parameters[2];
            BigInteger[] keycounterArray = (BigInteger[])parameters[3];
            BigInteger[] keysLeft = (BigInteger[])parameters[4];
            IControlEncryption sender = (IControlEncryption)parameters[5];
            int bytesToUse = (int)parameters[6];
            Stack threadStack = (Stack)parameters[7];
           
            //Bruteforce until stop:
            try
            {
                while (patterns[threadid] != null)
                {
                    BigInteger size = patterns[threadid].size();
                    keysLeft[threadid] = size;

                    IKeyTranslator keyTranslator = ControlMaster.GetKeyTranslator();
                    keyTranslator.SetKeys(patterns[threadid]);

                    bool finish = false;

                    do
                    {
                        //if we are the thread with most keys left, we have to share them:
                        keyTranslator = ShareKeys(patterns, threadid, keysLeft, keyTranslator, threadStack);
                        finish = BruteforceCPU(keyTranslator, sender, bytesToUse);
                        int progress = keyTranslator.GetProgress();

                        doneKeysArray[threadid] += progress;
                        keycounterArray[threadid] += progress;
                        keysLeft[threadid] -= progress;

                    } while (!finish && !stop);

                    if (stop)
                    {
                        return;
                    }

                    //Let's wait until another thread is willing to share with us:
                    WaitForNewPattern(patterns, threadid, threadStack);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error while trying to bruteforce: {0}", ex.Message), NotificationLevel.Error);
            }
            finally
            {
                sender.Dispose();
                stopEvent.Set();
            }
        }

        private bool BruteforceCPU(IKeyTranslator keyTranslator, IControlEncryption sender, int bytesToUse)
        {
            bool finish = false;
            for (int count = 0; count < 256 * 256; count++)
            {
                byte[] keya = keyTranslator.GetKey();

                if (!decryptAndCalculate(sender, bytesToUse, keya, keyTranslator))
                {
                    throw new Exception("Bruteforcing not possible!");
                }

                finish = !keyTranslator.NextKey();
                if (finish)
                {
                    break;
                }
            }
            return finish;
        }

        private bool decryptAndCalculate(IControlEncryption sender, int bytesToUse, byte[] keya, IKeyTranslator keyTranslator)
        {
            ValueKey valueKey = new ValueKey();
            try
            {
                if (encryptedDataOptimized != null && encryptedDataOptimized.Length > 0)
                {
                    valueKey.decryption = sender.Decrypt(encryptedDataOptimized, keya, initVectorOptimized, bytesToUse);
                }
                else
                {
                    GuiLogMessage("Can't bruteforce empty input!", NotificationLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Decryption is not possible: " + ex.Message, NotificationLevel.Error);
                GuiLogMessage("Stack Trace: " + ex.StackTrace, NotificationLevel.Error);
                return false;
            }

            try
            {
                valueKey.value = CostMaster.CalculateCost(valueKey.decryption);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Cost calculation is not possible: " + ex.Message, NotificationLevel.Error);
                return false;
            }

            if (costMaster.GetRelationOperator() == RelationOperator.LargerThen)
            {
                if (valueKey.value > valueThreshold)
                {
                    valueKey.key = keyTranslator.GetKeyRepresentation();
                    valueKey.keya = (byte[])keya.Clone();
                    valueKey.decryption = sender.Decrypt(encryptedDataOptimized, keya, initVectorOptimized, encryptedDataOptimized.Length);
                    EnhanceUserName(ref valueKey);
                    valuequeue.Enqueue(valueKey);
                }
            }
            else
            {
                if (valueKey.value < valueThreshold)
                {
                    valueKey.key = keyTranslator.GetKeyRepresentation();
                    valueKey.keya = (byte[])keya.Clone();
                    valueKey.decryption = sender.Decrypt(encryptedDataOptimized, keya, initVectorOptimized, encryptedDataOptimized.Length);
                    EnhanceUserName(ref valueKey);
                    valuequeue.Enqueue(valueKey);
                }
            }
            return true;
        }
        private IKeyTranslator ShareKeys(KeyPattern.KeyPattern[] patterns, int threadid, BigInteger[] keysLeft, IKeyTranslator keyTranslator, Stack threadStack)
        {
            BigInteger size;
            if (maxThread == threadid && threadStack.Count != 0)
            {
                try
                {
                    maxThreadMutex.WaitOne();
                    if (maxThread == threadid && threadStack.Count != 0)
                    {
                        KeyPattern.KeyPattern[] split = patterns[threadid].split();
                        if (split != null)
                        {
                            patterns[threadid] = split[0];
                            keyTranslator = ControlMaster.GetKeyTranslator();
                            keyTranslator.SetKeys(patterns[threadid]);

                            ThreadStackElement elem = (ThreadStackElement)threadStack.Pop();
                            patterns[elem.threadid] = split[1];
                            elem.ev.Set();    //wake the other thread up                                    
                            size = patterns[threadid].size();
                            keysLeft[threadid] = size;
                        }
                        maxThread = -1;
                    }
                }
                finally
                {
                    maxThreadMutex.ReleaseMutex();
                }
            }
            return keyTranslator;
        }

        private void WaitForNewPattern(KeyPattern.KeyPattern[] patterns, int threadid, Stack threadStack)
        {
            ThreadStackElement el = new ThreadStackElement
            {
                ev = new AutoResetEvent(false),
                threadid = threadid
            };
            patterns[threadid] = null;
            threadStack.Push(el);
            GuiLogMessage("Thread waiting for new keys.", NotificationLevel.Debug);
            el.ev.WaitOne();
            if (!stop)
            {
                GuiLogMessage("Thread waking up with new keys.", NotificationLevel.Debug);
            }
        }

        #region bruteforce methods



        #endregion

        #endregion

        public void process(IControlEncryption sender)
        {
            if (sender == null || costMaster == null)
            {
                return;
            }

            Pattern.WildcardKey = settings.Key;
            this.sender = sender;

            bruteforcePattern(Pattern);
        }

        internal LinkedList<ValueKey> costList = new LinkedList<ValueKey>();
        private int bytesToUse;
        private IControlEncryption sender;
        private DateTime beginBruteforcing;
        private BigInteger keysInThisChunk;

        // main entry point to the KeySearcher
        private LinkedList<ValueKey> bruteforcePattern(KeyPattern.KeyPattern pattern)
        {
            beginBruteforcing = DateTime.Now;
            GuiLogMessage(Resources.Start_bruteforcing_pattern__ + pattern.getKey() + "'", NotificationLevel.Debug);

            int maxInList = 10;
            costList = new LinkedList<ValueKey>();
            FillListWithDummies(maxInList, costList);
            valuequeue = Queue.Synchronized(new Queue());

            ResetStatistics();

            stop = false;
            if (!pattern.testWildcardKey(settings.Key))
            {
                GuiLogMessage(Resources.Wrong_key_pattern_, NotificationLevel.Error);
                return null;
            }

            try
            {
                bytesToUse = CostMaster.GetBytesToUse();
            }
            catch (Exception ex)
            {
                GuiLogMessage(Resources.Bytes_used_not_valid__ + ex.Message, NotificationLevel.Error);
                return null;
            }

            try
            {
                int bytesOffset = CostMaster.GetBytesOffset();

                // wander 2011-04-16:
                // Use offset to cut-off input data and save optimized input in "out" fields
                // Every bruteforce decryption attempt shall use optimized data
                SkipBlocks(encryptedData, initVector, bytesOffset, out encryptedDataOptimized, out initVectorOptimized);
            }
            catch (Exception ex)
            {
                GuiLogMessage(Resources.Bytes_offset_not_valid__ + ex.Message, NotificationLevel.Error);
                return null;
            }

            try
            {
                if (settings.UsePeerToPeer)
                {
                    BruteForceWithPeerToPeerSystem();
                    return null;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                return null;
            }
            return BruteForceWithLocalSystem(pattern);
        }

        /// <summary>
        /// Put in encrypted data, IV and offset, put out modified encrypted data and IV.
        /// Returns input without modification if offset is not set correctly.
        /// 
        /// The modification of the input data requires the random access property which is true for
        /// ECB, CBC or CFB, but not for OFB. Decryption will fail without note for OFB!
        /// </summary>
        private void SkipBlocks(byte[] dataInput, byte[] ivInput, int bytesOffset, out byte[] dataOutput,
            out byte[] ivOutput)
        {
            // nothing to do
            if (bytesOffset == 0)
            {
                dataOutput = dataInput;
                ivOutput = ivInput;
                return;
            }

            // invalid offset, ignore
            if (dataInput.Length - bytesOffset <= 0)
            {
                // TODO: externalize
                GuiLogMessage(
                    string.Format("Ignoring BytesOffset as it is greater or equal to input data: {0}>={1}", bytesOffset,
                        dataInput.Length), NotificationLevel.Warning);
                dataOutput = dataInput;
                ivOutput = ivInput;
                return;
            }

            int blockSize = sender.GetBlockSizeAsBytes(); // may throw exception if sender does not support this

            if (bytesOffset % blockSize != 0)
            {
                // TODO: externalize
                GuiLogMessage(
                    string.Format("BytesOffset {0} is not a multiple of cipher blocksize {1}, will skip less bytes",
                        bytesOffset, blockSize), NotificationLevel.Warning);
            }

            int omitBlocks = bytesOffset / blockSize;
            if (omitBlocks == 0) // no cut-off? nothing to do
            {
                dataOutput = dataInput;
                ivOutput = ivInput;
                return;
            }

            dataOutput = new byte[dataInput.Length - (omitBlocks * blockSize)];
            ivOutput = new byte[blockSize];

            // set predecessor block (current-1) of new ciphertext as IV
            int offsetPredecessorBlock = (omitBlocks - 1) * blockSize;
            Array.Copy(dataInput, offsetPredecessorBlock, ivOutput, 0, blockSize);

            // set short ciphertext
            Array.Copy(dataInput, omitBlocks * blockSize, dataOutput, 0, dataOutput.Length);
        }


        internal LinkedList<ValueKey> BruteForceWithLocalSystem(KeyPattern.KeyPattern pattern, bool redirectResultsToStatisticsGenerator = false)
        {
            if (!redirectResultsToStatisticsGenerator)
            {
                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(SetStartDate));
                localBruteForceStopwatch.Start();
            }

            keysInThisChunk = pattern.size();
            KeyPattern.KeyPattern[] patterns = splitPatternForThreads(pattern);
            if (patterns == null || patterns.Length == 0)
            {
                GuiLogMessage(Resources.No_ressources_to_BruteForce_available__Check_the_KeySearcher_settings_,
                    NotificationLevel.Error);
                throw new Exception("No ressources to BruteForce available. Check the KeySearcher settings!");
            }

            BigInteger[] doneKeysA = new BigInteger[patterns.Length];
            BigInteger[] keycounters = new BigInteger[patterns.Length];
            BigInteger[] keysleft = new BigInteger[patterns.Length];
            Stack threadStack = Stack.Synchronized(new Stack());
            threadsStopEvents = ArrayList.Synchronized(new ArrayList());

            StartThreads(sender, bytesToUse, patterns, doneKeysA, keycounters, keysleft, threadStack);

            DateTime lastTime = DateTime.Now;

            //update message:
            BigInteger totalDoneKeys = 0;

            while (!stop)
            {
                Thread.Sleep(1000);

                updateToplist();

                #region calculate global counters from local counters

                BigInteger doneKeys = doneKeysA.Aggregate<BigInteger, BigInteger>(0, (current, dk) => current + dk);
                BigInteger keycounter = keycounters.Aggregate<BigInteger, BigInteger>(0, (current, kc) => current + kc);

                #endregion

                if (keycounter > keysInThisChunk)
                {
                    GuiLogMessage(Resources.There_must_be_an_error__because_we_bruteforced_too_much_keys___, NotificationLevel.Error);
                }

                #region determination of the thread with most keys

                if (keysInThisChunk - keycounter > 1000)
                {
                    try
                    {
                        maxThreadMutex.WaitOne();
                        BigInteger max = 0;
                        int id = -1;
                        for (int i = 0; i < patterns.Length; i++)
                        {
                            if (keysleft[i] != null && keysleft[i] > max)
                            {
                                max = keysleft[i];
                                id = i;
                            }
                        }

                        maxThread = id;
                    }
                    finally
                    {
                        maxThreadMutex.ReleaseMutex();
                    }
                }

                #endregion

                long keysPerSecond = (long)((long)doneKeys / (DateTime.Now - lastTime).TotalSeconds);
                totalDoneKeys += doneKeys;
                lastTime = DateTime.Now;

                showProgress(costList, keysInThisChunk, keycounter, keysPerSecond);              

                #region set doneKeys to 0

                doneKeys = 0;
                
                for (int i = 0; i < doneKeysA.Length; i++)
                {
                    doneKeysA[i] = 0;
                }              

                #endregion

                if (keycounter >= keysInThisChunk)
                {
                    break;
                }
            } //end while

            long totalKeysPerSecond = (long)((long)totalDoneKeys / localBruteForceStopwatch.Elapsed.TotalSeconds);
            showProgress(costList, 1, 1, totalKeysPerSecond);

            //wake up all sleeping threads, so they can stop:
            while (threadStack.Count != 0)
            {
                ((ThreadStackElement)threadStack.Pop()).ev.Set();
            }

            //wait until all threads finished:
            foreach (AutoResetEvent stopEvent in threadsStopEvents)
            {
                stopEvent.WaitOne();
            }

            if (!stop && !redirectResultsToStatisticsGenerator)
            {
                ProgressChanged(1, 1);
            }

            TimeSpan bruteforcingTime = DateTime.Now.Subtract(beginBruteforcing);
            StringBuilder sbBFTime = new StringBuilder();
            if (bruteforcingTime.Days > 0)
            {
                sbBFTime.Append(bruteforcingTime.Days + Resources._days_);
            }

            if (bruteforcingTime.Hours > 0)
            {
                if (bruteforcingTime.Hours <= 9)
                {
                    sbBFTime.Append("0");
                }

                sbBFTime.Append(bruteforcingTime.Hours + ":");
            }
            if (bruteforcingTime.Minutes <= 9)
            {
                sbBFTime.Append("0");
            }

            sbBFTime.Append(bruteforcingTime.Minutes + ":");
            if (bruteforcingTime.Seconds <= 9)
            {
                sbBFTime.Append("0");
            }

            sbBFTime.Append(bruteforcingTime.Seconds + "-");
            if (bruteforcingTime.Milliseconds <= 9)
            {
                sbBFTime.Append("00");
            }

            if (bruteforcingTime.Milliseconds <= 99)
            {
                sbBFTime.Append("0");
            }

            sbBFTime.Append(bruteforcingTime.Milliseconds.ToString());

            GuiLogMessage(
                Resources.Ended_bruteforcing_pattern__ + pattern.getKey() + Resources.___Bruteforcing_TimeSpan__ +
                sbBFTime, NotificationLevel.Debug);

            return costList;
        }

        private void SetStartDate()
        {
            localQuickWatchPresentation.StartTime.Value = DateTime.Now.ToString("g", Thread.CurrentThread.CurrentCulture);
        }

        internal void showProgress(LinkedList<ValueKey> costList, BigInteger size, BigInteger keycounter, long keysPerSecond)
        {
            ProgressChanged((double)keycounter / (double)size, 1.0);

            if (!localQuickWatchPresentation.IsVisible || stop)
            {
                return;
            }

            if (keysPerSecond == 0)
            {
                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printEntries(costList);
                }
                , null);
                return;
            }

            TimeSpan timeleft = new TimeSpan();

            try
            {
                long ticksPerSecond = 10000000;
                double secstodo = (double)((size - keycounter) / keysPerSecond);
                timeleft = new TimeSpan((long)(secstodo * ticksPerSecond));
            }
            catch
            {
                timeleft = new TimeSpan(-1);
            }

            double testetBits = 0;
            try
            {
                testetBits = Math.Ceiling(BigInteger.Log10(pattern.size()) / Math.Log10(2));
            }
            catch (Exception)
            {
                //cannot calculate testedBits
            }

            localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                localQuickWatchPresentation.TestedBits.Value = string.Format("{0}", testetBits);
                localQuickWatchPresentation.ElapsedTime.Value = TimeSpanString(localBruteForceStopwatch.Elapsed);
                localQuickWatchPresentation.KeysPerSecond.Value = string.Format("{0:0,0}", keysPerSecond);

                if (timeleft != new TimeSpan(-1))
                {
                    localQuickWatchPresentation.TimeLeft.Value = TimeSpanString(timeleft);
                    try
                    {
                        localQuickWatchPresentation.EndTime.Value = "" + DateTime.Now.Add(timeleft).ToString("g", Thread.CurrentThread.CurrentCulture);
                    }
                    catch
                    {
                        localQuickWatchPresentation.EndTime.Value = Resources.in_a_galaxy_far__far_away___;
                    }
                }
                else
                {
                    localQuickWatchPresentation.TimeLeft.Value = Resources.incalculable____;
                    localQuickWatchPresentation.EndTime.Value = Resources.in_a_galaxy_far__far_away___;
                }

                printEntries(costList);
            }
            , null);
        }

        private void printEntries(LinkedList<ValueKey> costList)
        {
            localQuickWatchPresentation.Entries.Clear();

            int i = 1;

            for (LinkedListNode<ValueKey> node = costList.First; node != null; node = node.Next)
            {
                string plaintext = Encoding.GetEncoding(1252).GetString(node.Value.decryption);
                ResultEntry entry = new ResultEntry
                {
                    Ranking = i++,
                    Value = "" + Math.Round(node.Value.value, 3),
                    Key = node.Value.key,
                    Text = plaintext.Length > 32 ? plaintext.Substring(0, 32) + "..." : plaintext,
                    FullText = plaintext
                };
                localQuickWatchPresentation.Entries.Add(entry);
            }
        }

        #region For TopList and Statistics

        private void FillListWithDummies(int maxInList, LinkedList<ValueKey> costList)
        {
            ValueKey valueKey = new ValueKey();
            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                valueKey.value = double.MaxValue;
            }
            else
            {
                valueKey.value = double.MinValue;
            }

            valueKey.key = Resources.dummykey;
            valueKey.decryption = new byte[0];
            valueThreshold = valueKey.value;
            LinkedListNode<ValueKey> node = costList.AddFirst(valueKey);
            for (int i = 1; i < maxInList; i++)
            {
                node = costList.AddAfter(node, valueKey);
            }
        }


        /// <summary>
        /// Reseting statistic values to avoid too high sums in case of network failure
        /// </summary>
        public void ResetStatistics()
        {
            statistic = null;
            statistic = new Dictionary<string, Dictionary<long, Information>>();
            machineHierarchy = null;
            machineHierarchy = new Dictionary<long, MachInfo>();
        }

        private bool memory = false;
        private int cUsers = 0;

        /// <summary>
        /// Initialisation for fixed and current values every utime/30 minutes
        /// </summary>
        public void InitialiseInformationQuickwatch()
        {
            if (Pattern == null || !Pattern.testWildcardKey(settings.Key) || settings.NumberOfBlocks == 0)
            {
                return;
            }

            CalcCurrentStats();
            GenerateMaschineStats();

            //if we have two time values to compare
            if (!memory)
            {
                memory = true;
            }
        }

        /// <summary>
        /// Calculating the current users/machines and setting their current/dead flag
        /// </summary>
        internal void CalcCurrentStats()
        {
            cUsers = 0;
            DateTime testdate = DateTime.UtcNow;

            if (statistic != null)
            {
                //for each user...
                foreach (string avatar in statistic.Keys)
                {
                    int useradd = 0;

                    //...and for each machine of this user...
                    foreach (long mid in statistic[avatar].Keys)
                    {
                        //...calculate if the machine is current/dead
                        if (statistic[avatar][mid].Date.AddMinutes(30) > testdate) //30 min current criterium
                        {
                            statistic[avatar][mid].Current = true;
                            statistic[avatar][mid].Dead = false;
                            useradd = 1;
                        }
                        else
                        {
                            if (testdate > statistic[avatar][mid].Date.AddMinutes(2880)) //after 2 days
                            {
                                statistic[avatar][mid].Dead = true;
                            }
                            else
                            {
                                statistic[avatar][mid].Dead = false;
                            }
                            statistic[avatar][mid].Current = false;
                        }
                    }
                    cUsers = cUsers + useradd;
                }
            }
        }


        /// <summary>
        /// Write the user statistics to an external csv-document
        /// </summary>
        internal void WriteStatistics(string dataIdentifier)
        {
            //using the chosen csv file
            string path = settings.CsvPath;

            if (path == "")
            {
                //using the default save folder %APPDATA%\Local\CrypTool2
                path = string.Format("{0}\\UserRanking{1}.csv", DirectoryHelper.DirectoryLocal, dataIdentifier);
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("Avatarname" + ";" + "MaschineID" + ";" + "Hostname" + ";" + "Pattern Count" + ";" + "Last Update");
                    foreach (string avatar in statistic.Keys)
                    {
                        foreach (long mID in statistic[avatar].Keys)
                        {
                            sw.WriteLine(avatar + ";" + mID.ToString() + ";" + statistic[avatar][mID].Hostname + ";" + statistic[avatar][mID].Count + ";" + statistic[avatar][mID].Date);
                        }
                    }
                }
            }
            catch (Exception)
            {
                GuiLogMessage(string.Format("Failed to write Userstatistics to {0}", path), NotificationLevel.Error);
            }

            //This writes the Maschinestatistics to the main folder if no different path was chosen
            if (settings.CsvPath == "")
            {
                try
                {
                    //using the default save folder %APPDATA%\Local\CrypTool2
                    using (StreamWriter sw = new StreamWriter(string.Format("{0}\\Maschine{1}.csv", DirectoryHelper.DirectoryLocal, dataIdentifier)))
                    {
                        sw.WriteLine("Maschineid" + ";" + "Name" + ";" + "Sum" + ";" + "Users");
                        foreach (long mID in machineHierarchy.Keys)
                        {
                            sw.WriteLine(mID + ";" + machineHierarchy[mID].Hostname + ";" + machineHierarchy[mID].Sum + ";" + machineHierarchy[mID].Users);
                        }
                    }
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format("Failed to write Maschinestatistics to {0}", path), NotificationLevel.Error);
                }
            }
        }

        /// <summary>
        /// Creating the machine view of the statistics
        /// </summary>
        internal void GenerateMaschineStats()
        {
            machineHierarchy = null;
            machineHierarchy = new Dictionary<long, MachInfo>();

            foreach (string avatar in statistic.Keys)
            {
                Dictionary<long, Information> Maschines = statistic[avatar];

                //add the maschine count to the maschinestatistics
                foreach (long mid in Maschines.Keys)
                {
                    //if the maschine exists in maschinestatistic add it to the sum
                    if (machineHierarchy.ContainsKey(mid))
                    {
                        machineHierarchy[mid].Sum = machineHierarchy[mid].Sum + Maschines[mid].Count;
                        machineHierarchy[mid].Hostname = Maschines[mid].Hostname;
                        machineHierarchy[mid].Users = machineHierarchy[mid].Users + avatar + " | ";
                        machineHierarchy[mid].Date = Maschines[mid].Date > machineHierarchy[mid].Date ? Maschines[mid].Date : machineHierarchy[mid].Date;

                        //taking the current/dead flags
                        if (!machineHierarchy[mid].Current)
                        {
                            machineHierarchy[mid].Current = Maschines[mid].Current;
                        }
                        if (machineHierarchy[mid].Dead)
                        {
                            machineHierarchy[mid].Dead = Maschines[mid].Dead;
                        }
                    }
                    else
                    {
                        //else make a new entry
                        machineHierarchy.Add(mid, new MachInfo
                        {
                            Sum = Maschines[mid].Count,
                            Hostname = Maschines[mid].Hostname,
                            Users = "| " + avatar + " | ",
                            Date = Maschines[mid].Date,
                            Current = Maschines[mid].Current,
                            Dead = Maschines[mid].Dead
                        });
                    }
                }
            }

            machineHierarchy = machineHierarchy.OrderByDescending((x) => x.Value.Sum).ToDictionary(x => x.Key, y => y.Value);
        }

        /// <summary>
        /// Finding the total amount of chunks calculated from the statistics
        /// </summary>
        internal BigInteger calculatedChunks()
        {
            return machineHierarchy.Keys.Aggregate<long, BigInteger>(0, (current, mid) => current + machineHierarchy[mid].Sum);
        }

        /// <summary>
        /// Enhancing the user information to the found key/value pairs in this calculation
        /// </summary>
        private void EnhanceUserName(ref ValueKey vk)
        {
            DateTime chunkstart = DateTime.UtcNow;
            username = "";

            //Enhance our userdata if there exists a valid user:
            if ((username != null) && (!username.Equals("")))
            {
                vk.user = username;
                vk.time = chunkstart;
                vk.maschid = maschineid;
                vk.maschname = MachineName.MachineNameToUse;
            }
            else
            {
                vk.user = "Unknown";
                vk.time = defaultstart;
                vk.maschid = 666;
                vk.maschname = "Devil";
            }
        }

        /// <summary>
        /// Updating the top list for the key value pairs
        /// </summary>
        internal void updateToplist()
        {
            LinkedListNode<ValueKey> node;
            while (valuequeue.Count != 0)
            {
                ValueKey vk = (ValueKey)valuequeue.Dequeue();

                //if (costList.Contains(vk)) continue;
                IEnumerable<ValueKey> result = costList.Where(valueKey => valueKey.key == vk.key);
                if (result.Count() > 0)
                {
                    continue;
                }

                if (costMaster.GetRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
                                if (node == costList.First)
                                {
                                    Top1 = vk;
                                }

                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                valueThreshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                        }//end while
                    }//end if
                }
                else
                {
                    if (vk.value < costList.Last().value)
                    {
                        node = costList.First;
                        while (node != null)
                        {
                            if (vk.value < node.Value.value)
                            {
                                if (node == costList.First)
                                {
                                    Top1 = vk;
                                }

                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                valueThreshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                        }//end while
                    }//end if
                }
            }
        }

        #endregion

        private void StartThreads(IControlEncryption sender, int bytesToUse, KeyPattern.KeyPattern[] patterns, BigInteger[] doneKeysA, BigInteger[] keycounters, BigInteger[] keysleft, Stack threadStack)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                WaitCallback worker = new WaitCallback(KeySearcherJob);
                doneKeysA[i] = new BigInteger();
                keycounters[i] = new BigInteger();

                ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, keycounters, keysleft, sender, bytesToUse, threadStack, null });
            }
        }

        private KeyPattern.KeyPattern[] splitPatternForThreads(KeyPattern.KeyPattern pattern)
        {
            int threads = settings.CoresUsed;
            
            if (threads < 1)
            {
                return null;
            }

            KeyPattern.KeyPattern[] patterns = new KeyPattern.KeyPattern[threads];
            if (threads > 1)
            {
                KeyPattern.KeyPattern[] patterns2 = pattern.split();
                if (patterns2 == null)
                {
                    patterns2 = new KeyPattern.KeyPattern[1];
                    patterns2[0] = pattern;
                    return patterns2;
                }
                patterns[0] = patterns2[0];
                patterns[1] = patterns2[1];
                int p = 1;
                threads -= 2;

                while (threads > 0)
                {
                    int maxPattern = -1;
                    BigInteger max = 0;
                    for (int i = 0; i <= p; i++)
                    {
                        if (patterns[i].size() > max)
                        {
                            max = patterns[i].size();
                            maxPattern = i;
                        }
                    }

                    KeyPattern.KeyPattern[] patterns3 = patterns[maxPattern].split();
                    if (patterns3 == null)
                    {
                        patterns3 = new KeyPattern.KeyPattern[p + 1];
                        for (int i = 0; i <= p; i++)
                        {
                            patterns3[i] = patterns[i];
                        }

                        return patterns3;
                    }
                    patterns[maxPattern] = patterns3[0];
                    patterns[++p] = patterns3[1];
                    threads--;
                }
            }
            else
            {
                patterns[0] = pattern;
            }

            return patterns;
        }

        private void keyPatternChanged()
        {
            Pattern = new KeyPattern.KeyPattern(controlMaster.GetKeyPattern());
        }

        #endregion

        public override event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        /// <summary>
        /// used for delivering the results from the worker threads to the main thread:
        /// </summary>
        public struct ValueKey
        {
            public override int GetHashCode()
            {
                return keya.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj != null && obj is ValueKey && keya != null && ((ValueKey)obj).keya != null)
                {
                    return (keya.SequenceEqual(((ValueKey)obj).keya));
                }
                else
                {
                    return false;
                }
            }

            public override string ToString()
            {
                return string.Format("({5}): {0} -> {1:N} (by {2} at {3}-{4})", key, value, user, maschid, maschname, time);
            }

            public double value;
            public string key;
            public byte[] decryption;
            public byte[] keya;
            public string user { get; set; }
            public DateTime time { get; set; }
            public long maschid { get; set; }
            public string maschname { get; set; }
        };

        /// <summary>
        /// calculate a String which shows the timespan
        /// 
        /// example
        /// 
        ///     4 days
        /// or
        ///     2 minutes
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        private string TimeSpanString(TimeSpan ts)
        {
            string res = "";
            if (ts.Days > 1)
            {
                res = ts.Days + " " + typeof(KeySearcher).GetPluginStringResource("days") + " ";
            }

            if (ts.Days == 1)
            {
                res = ts.Days + " " + typeof(KeySearcher).GetPluginStringResource("day") + " ";
            }

            if (ts.Hours > 1 || res.Length != 0)
            {
                res += ts.Hours + " " + typeof(KeySearcher).GetPluginStringResource("hours") + " ";
            }

            if (ts.Hours == 1)
            {
                res += ts.Hours + " " + typeof(KeySearcher).GetPluginStringResource("hour") + " ";
            }

            if (ts.Minutes > 1)
            {
                res += ts.Minutes + " " + typeof(KeySearcher).GetPluginStringResource("minutes") + " ";
            }

            if (ts.Minutes == 1)
            {
                res += ts.Minutes + " " + typeof(KeySearcher).GetPluginStringResource("minute") + " ";
            }

            if (res.Length == 0 && ts.Seconds > 1)
            {
                res += ts.Seconds + " " + typeof(KeySearcher).GetPluginStringResource("seconds");
            }

            if (res.Length == 0 && ts.Seconds == 1)
            {
                res += ts.Seconds + " " + typeof(KeySearcher).GetPluginStringResource("second");
            }

            return res;
        }

        #region cloud KeySearcher

        private CloudKeySearcher cloudKeySearcher;

        private void BruteForceWithPeerToPeerSystem()
        {
            byte[] optimizedCryp = OptimizedCryp(out byte[] optimizedInitVector);

            JobDataContainer jobDataContainer = new JobDataContainer
            {
                JobId = JobId,
                BytesToUse = bytesToUse,
                InitVector = optimizedInitVector,
                Ciphertext = optimizedCryp,
                CryptoAlgorithm = ControlMaster,
                CostAlgorithm = CostMaster,
                NumberOfBlocks = BigInteger.Pow(2, settings.NumberOfBlocks)
            };

            p2PQuickWatchPresentation.ViewModel.BytesToUse = bytesToUse;

            if (cloudKeySearcher != null)
            {
                cloudKeySearcher.Stop();
            }

            cloudKeySearcher = new CloudKeySearcher(jobDataContainer, Pattern, p2PQuickWatchPresentation, this);
            cloudKeySearcher.Start();
        }

        private byte[] OptimizedCryp(out byte[] optimizedInitVector)
        {

            SkipBlocks(encryptedData, initVector, CostMaster.GetBytesOffset(), out byte[] optimizedCryp, out optimizedInitVector);
            if (optimizedCryp == null || optimizedCryp.Length > 0)
            {
                optimizedCryp = encryptedData;
            }

            if (optimizedInitVector == null || optimizedInitVector.Length > 0)
            {
                optimizedInitVector = initVector;
            }
            return optimizedCryp;
        }

        public void SetTop1Entry(KeyResultEntry result)
        {
            Top1Message = sender.Decrypt(encryptedData, result.KeyBytes, initVector);
            Top1Key = result.KeyBytes;
        }

        public override BigInteger NumberOfBlocks => BigInteger.Pow(2, settings.NumberOfBlocks);

        #endregion

    }
}