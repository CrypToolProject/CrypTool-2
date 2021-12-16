using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace CrypCloud.Core.CloudComponent
{
    public abstract class ACloudComponent : ACloudCompatible
    {
        private readonly CrypCloudCore cryptCloudCore = CrypCloudCore.Instance;
        private readonly CalculationTemplate calculationTemplate;
        private CancellationTokenSource offlineCancellation;

        protected ACloudComponent()
        {
            calculationTemplate = new CalculationTemplate(this);
            CurrentBestlist = new List<byte[]>();
        }

        public List<byte[]> CurrentBestlist { get; private set; }

        public override void Stop()
        {
            if (IsOnline())
            {
                cryptCloudCore.StopLocalCalculation(JobId);
            }
            else
            {
                offlineCancellation.Cancel(false);
            }

            StopLocal();
        }

        public override void PreExecution()
        {

            GuiLogMessage("preEx isOnline: " + IsOnline(), NotificationLevel.Error);
            PreExecutionLocal();

            if (WorkspaceHasBeenModified())
            {
                GuiLogMessage("Could not start calculation. Workspace has been modified.", NotificationLevel.Error);
            }

            if (IsOnline())
            {
                cryptCloudCore.StartLocalCalculation(JobId, calculationTemplate);
            }
            else
            {
                StartOfflineCalculation();
            }
        }

        private void StartOfflineCalculation()
        {
            offlineCancellation = new CancellationTokenSource();
            for (int i = 0; i < NumberOfBlocks && !offlineCancellation.IsCancellationRequested; i++)
            {
                List<byte[]> block = CalculateBlock(i, offlineCancellation.Token);
                CurrentBestlist = MergeBlockResults(CurrentBestlist, new List<byte[]>(block));
            }
        }



        #region abstract member

        /// <summary>
        /// Represents the logic for calculation a single "cloud" block
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public abstract List<byte[]> CalculateBlock(BigInteger blockId, CancellationToken cancelToken);

        /// <summary>
        /// Merges two CalculateBlock-results.
        /// </summary> 
        /// <returns></returns>
        public abstract List<byte[]> MergeBlockResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList);

        /// <summary>
        /// Will be called once before local Calculation is started. May be used to set up data used for execution.
        /// </summary>
        public abstract void PreExecutionLocal();

        /// <summary>
        /// Will be called after the local calculation has stoped.
        /// </summary>
        public abstract void StopLocal();

        #endregion


        #region logger

        public override event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }

}
