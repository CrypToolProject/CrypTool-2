using System.Numerics;
using System.Threading;
using VoluntLib2.ComputationLayer;

namespace CrypCloud.Core.CloudComponent
{
    internal class CloudWorker : AWorker
    {
        private readonly ACloudComponent cloudComponent;

        public CloudWorker(ACloudComponent cloudComponent)
        {
            this.cloudComponent = cloudComponent;
        }

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken)
        {
            return new CalculationResult
            {
                BlockID = blockId,
                LocalResults = cloudComponent.CalculateBlock(blockId, cancelToken)
            };
        }
    }
}
