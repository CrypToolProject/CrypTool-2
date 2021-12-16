using CrypTool.PluginBase;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;

namespace CrypCloud.Core.CloudComponent
{
    public abstract class ACloudCompatible : ICrypComponent
    {
        protected ACloudCompatible()
        {
            JobId = -1;
        }

        /// <summary>
        /// Number Of Blocks that should be calculated.
        /// Has to be set before starting
        /// </summary>
        public virtual BigInteger NumberOfBlocks { get; set; }

        /// <summary>
        /// JobId of bound networkjob.
        /// If its -1 its not bound to any networkjob
        /// </summary>
        public BigInteger JobId { get; set; }

        /// <summary>
        /// Determine if this component is linked to a Networkjob
        /// </summary>
        /// <returns></returns>
        public bool IsOnline()
        {
            return JobId != -1;
        }

        public Func<byte[]> ComputeWorkspaceHash { get; set; }
        public byte[] ValidWorkspaceHash { get; set; }

        public bool WorkspaceHasBeenModified()
        {
            if (ValidWorkspaceHash == null)
            {
                return false;
            }

            byte[] computedWorkspaceHash = ComputeWorkspaceHash();
            bool workspaceHasBeenModified = !ValidWorkspaceHash.SequenceEqual(computedWorkspaceHash);
            return workspaceHasBeenModified;
        }

        public abstract event PropertyChangedEventHandler PropertyChanged;
        public abstract void Dispose();
        public abstract event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public abstract ISettings Settings { get; }
        public abstract UserControl Presentation { get; set; }
        public abstract void Execute();
        public abstract void Stop();
        public abstract void Initialize();
        public abstract void PreExecution();
        public abstract void PostExecution();
        public abstract event StatusChangedEventHandler OnPluginStatusChanged;
        public abstract event PluginProgressChangedEventHandler OnPluginProgressChanged;
    }
}
