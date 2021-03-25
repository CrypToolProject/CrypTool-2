 using KeySearcher.P2P.Presentation;

namespace KeySearcher.P2P.Storage
{
    class StatusUpdater
    {
        private readonly StatusContainer status;
        private readonly string statusKey;

        public StatusUpdater(StatusContainer status, string statusKey)
        {
            this.status = status;
            this.statusKey = statusKey;
        }

        public void SendUpdate()
        {
           
        }
    }
}
