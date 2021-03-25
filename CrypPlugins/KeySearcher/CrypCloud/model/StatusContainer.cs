using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Threading;
using CrypTool.PluginBase;
using KeySearcherPresentation.Controls;

namespace KeySearcher.CrypCloud
{
    public class StatusContainer : INotifyPropertyChanged
    {
        private BigInteger localFinishedChunks;
        private BigInteger localAbortChunks;
        private String localFinishedChunksSession = "(0)";
        private String currentChunk;
        private double progressOfCurrentChunk;
        private bool isCurrentProgressIndeterminate;
        private double globalProgress;
        private BigInteger keysPerSecond;
        private DateTime startDate;
        private long jobSubmitterID;
        private TimeSpan elapsedTime;
        private TimeSpan remainingTime;
        private TimeSpan remainingTimeTotal;
        private BigInteger totalAmountOfParticipants;
        private string estimatedFinishDate;
        private BigInteger totalDhtRequests;
        private BigInteger requestsPerNode;
        private BigInteger retrieveRequests;
        private BigInteger removeRequests;
        private BigInteger storeRequests;
        private TimeSpan dhtOverheadInReadableTime;
        private string dhtOverheadInPercent;
        private bool isSearchingForReservedNodes;
        private long storedBytes;
        private long retrievedBytes;
        private String currentOperation;
        private long totalBytes;
        private long sentBytesByLinkManager;
        private long receivedBytesByLinkManager;
        private long totalBytesByLinkManager;
        private BigInteger jobID;
        private TimeSpan avgTimePerChunk;


        public StatusContainer()
        {
            EstimatedFinishDate = "-";
            DhtOverheadInPercent = "-";
            TotalAmountOfParticipants = 1;
            CurrentOperation = "Idle";

            TopList = new ObservableCollection<KeyResultEntry>();
        }

        public void BindToView(P2PQuickWatchPresentation presentation)
        {
            presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (SendOrPostCallback)delegate { presentation.DataContext = this; }, null);
        }

        public ObservableCollection<KeyResultEntry> TopList { get; set; }

        #region properties with propChange handler

        public String CurrentOperation
        {
            get { return currentOperation; }
            set
            {
                currentOperation = value;
                OnPropertyChanged("CurrentOperation");
            }
        }
        
        public BigInteger LocalFinishedChunks
        {
            get { return localFinishedChunks; }
            set
            {
                localFinishedChunks = value;
                OnPropertyChanged("LocalFinishedChunks");
            }
        }

        public BigInteger LocalAbortChunks
        {
            get { return localAbortChunks; }
            set
            {
                localAbortChunks = value;
                OnPropertyChanged("LocalAbortChunks");
            }
        }

        public BigInteger JobID
        {
            get { return jobID; }
            set
            {
                jobID = value;
                OnPropertyChanged("JobID");
            }
        }

        public String LocalFinishedChunksSession
        {
            get { return localFinishedChunksSession; }
            set
            {
                localFinishedChunksSession = value;
                OnPropertyChanged("LocalFinishedChunksSession");
            }
        } 

        public String CurrentChunk
        {
            get { return currentChunk; }
            set
            {
                currentChunk = value;
                OnPropertyChanged("CurrentChunk");
            }
        }
        public double ProgressOfCurrentChunk
        {
            get { return progressOfCurrentChunk; }
            set
            {
                progressOfCurrentChunk = value;
                OnPropertyChanged("ProgressOfCurrentChunk");
            }
        }
        public double GlobalProgress
        {
            get { return globalProgress; }
            set
            {
                globalProgress = value;
                OnPropertyChanged("GlobalProgress");
            }
        }
        public bool IsCurrentProgressIndeterminate
        {
            get { return isCurrentProgressIndeterminate; }
            set
            {
                isCurrentProgressIndeterminate = value;
                OnPropertyChanged("IsCurrentProgressIndeterminate");
            }
        }
        public BigInteger KeysPerSecond
        {
            get { return keysPerSecond; }
            set
            {
                keysPerSecond = value;
                OnPropertyChanged("KeysPerSecond");
            }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        public long JobSubmitterID
        {
            get { return jobSubmitterID; }
            set
            {
                jobSubmitterID = value;
                OnPropertyChanged("JobSubmitterID");
            }
        }

        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
            set
            {
                elapsedTime = value;
                OnPropertyChanged("ElapsedTime");
            }
        }
        
        public TimeSpan AvgTimePerChunk
        {
            get { return avgTimePerChunk; }
            set
            {
                avgTimePerChunk = value;
                OnPropertyChanged("AvgTimePerChunk");
            }
        }

        public TimeSpan RemainingTime
        {
            get { return remainingTime; }
            set
            {
                remainingTime = value;
                OnPropertyChanged("RemainingTime");
            }
        }

        public TimeSpan RemainingTimeTotal
        {
            get { return remainingTimeTotal; }
            set
            {
                remainingTimeTotal = value;
                OnPropertyChanged("RemainingTimeTotal");
            }
        }

        public BigInteger TotalAmountOfParticipants
        {
            get { return totalAmountOfParticipants; }
            set
            {
                totalAmountOfParticipants = value;
                OnPropertyChanged("TotalAmountOfParticipants");
            }
        }

        public string EstimatedFinishDate
        {
            get { return estimatedFinishDate; }
            set
            {
                estimatedFinishDate = value;
                OnPropertyChanged("EstimatedFinishDate");
            }
        }

      
        public BigInteger TotalDhtRequests
        {
            get { return totalDhtRequests; }
            set
            {
                totalDhtRequests = value;
                OnPropertyChanged("TotalDhtRequests");
            }
        }
        
        public BigInteger RequestsPerNode
        {
            get { return requestsPerNode; }
            set
            {
                requestsPerNode = value;
                OnPropertyChanged("RequestsPerNode");
            }
        }
        
        public BigInteger RetrieveRequests
        {
            get { return retrieveRequests; }
            set
            {
                retrieveRequests = value;
                OnPropertyChanged("RetrieveRequests");
            }
        }

        public BigInteger RemoveRequests
        {
            get { return removeRequests; }
            set
            {
                removeRequests = value;
                OnPropertyChanged("RemoveRequests");
            }
        }

        public BigInteger StoreRequests
        {
            get { return storeRequests; }
            set
            {
                storeRequests = value;
                OnPropertyChanged("StoreRequests");
            }
        }

     
        public TimeSpan DhtOverheadInReadableTime
        {
            get { return dhtOverheadInReadableTime; }
            set
            {
                dhtOverheadInReadableTime = value;
                OnPropertyChanged("DhtOverheadInReadableTime");
            }
        }

        public string DhtOverheadInPercent
        {
            get { return dhtOverheadInPercent; }
            set
            {
                dhtOverheadInPercent = value;
                OnPropertyChanged("DhtOverheadInPercent");
            }
        }

        public bool IsSearchingForReservedNodes
        {
            get { return isSearchingForReservedNodes; }
            set
            {
                isSearchingForReservedNodes = value;
                OnPropertyChanged("IsSearchingForReservedNodes");
            }
        }

        public long StoredBytes
        {
            get { return storedBytes; }
            set
            {
                storedBytes = value;
                OnPropertyChanged("StoredBytes");
            }
        }
         
        public long RetrievedBytes
        {
            get { return retrievedBytes; }
            set
            {
                retrievedBytes = value;
                OnPropertyChanged("RetrievedBytes");
            }
        }

        public long TotalBytes
        {
            get { return totalBytes; }
            set
            {
                totalBytes = value;
                OnPropertyChanged("TotalBytes");
            }
        }

      
        public long SentBytesByLinkManager
        {
            get { return sentBytesByLinkManager; }
            set
            {
                sentBytesByLinkManager = value;
                OnPropertyChanged("SentBytesByLinkManager");
            }
        }
       
        public long ReceivedBytesByLinkManager
        {
            get { return receivedBytesByLinkManager; }
            set
            {
                receivedBytesByLinkManager = value;
                OnPropertyChanged("ReceivedBytesByLinkManager");
            }
        }
        
        public long TotalBytesByLinkManager
        {
            get { return totalBytesByLinkManager; }
            set
            {
                totalBytesByLinkManager = value;
                OnPropertyChanged("TotalBytesByLinkManager");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
