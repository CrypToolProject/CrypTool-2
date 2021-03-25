using System.Threading;

namespace KeySearcher.P2P.Helper
{
    public class ConnectionHelper
    {
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;
        private AutoResetEvent connectResetEvent;

        public ConnectionHelper(KeySearcher keySearcher, KeySearcherSettings settings)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
        }

        public void ValidateConnectionToPeerToPeerSystem()
        {

        }


        void HandleConnectionStateChange(object sender, bool newState)
        {
            connectResetEvent.Set();
        }
    }
}
