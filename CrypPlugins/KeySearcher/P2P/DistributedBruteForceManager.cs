using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Windows.Threading; 
using CrypTool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Helper;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;
using KeySearcher.Presentation;
using KeySearcher.Presentation.Controls;
using KeySearcherPresentation.Controls;
using KeySearcher.Properties;
using System.Timers;
using Timer = System.Timers.Timer;

namespace KeySearcher.P2P
{
    internal class DistributedBruteForceManager
    {
        private readonly StorageKeyGenerator keyGenerator;
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;
        private readonly KeyQualityHelper keyQualityHelper;
        private readonly P2PQuickWatchPresentation quickWatch;
        private readonly KeyPoolTreePresentation _keyPoolTreePresentation;
        private readonly KeyPatternPool patternPool;
        private readonly StatusContainer status;
        internal readonly StatisticsGenerator StatisticsGenerator;
        internal readonly Stopwatch StopWatch;

        private KeyPoolTree keyPoolTree;
        private AutoResetEvent systemJoinEvent = new AutoResetEvent(false);

        public DistributedBruteForceManager(KeySearcher keySearcher, KeyPattern.KeyPattern keyPattern, KeySearcherSettings settings,
                                            KeyQualityHelper keyQualityHelper, P2PQuickWatchPresentation quickWatch, KeyPoolTreePresentation keyPoolTreePresentation)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
            this.keyQualityHelper = keyQualityHelper;
            this.quickWatch = quickWatch;
            _keyPoolTreePresentation = keyPoolTreePresentation;

            // TODO when setting is still default (21), it is only displayed as 21 - but the settings-instance contains 0 for that key!
            if (settings.NumberOfBlocks == 0)
            {
                settings.NumberOfBlocks = 3;
            }

            StopWatch = new Stopwatch();
            status = new StatusContainer(keySearcher);
            status.IsCurrentProgressIndeterminate = true;

            keyGenerator = new StorageKeyGenerator(keySearcher, settings);
            patternPool = new KeyPatternPool(keyPattern, new BigInteger(Math.Pow(2, settings.NumberOfBlocks)));
            StatisticsGenerator = new StatisticsGenerator(status, quickWatch, keySearcher, settings, this);
            quickWatch.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateStatusContainerInQuickWatch));

            _keyPoolTreePresentation.PatternPool = patternPool;
            _keyPoolTreePresentation.KeyQualityHelper = keyQualityHelper;
            _keyPoolTreePresentation.KeyGenerator = keyGenerator;
            _keyPoolTreePresentation.StatusContainer = status;
        }

        public void Execute()
        {
            status.CurrentOperation = Resources.Initializing_connection_to_the_peer_to_peer_system;
            new ConnectionHelper(keySearcher, settings).ValidateConnectionToPeerToPeerSystem();
        }


        private void UpdateStatusContainerInQuickWatch()
        {
            quickWatch.DataContext = status;
            quickWatch.UpdateSettings(keySearcher, settings);
        }
    }
}