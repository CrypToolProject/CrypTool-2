using System;

namespace CrypTool.Plugins.NetworkSender
{
    public class PresentationPackage
    {
        public PresentationPackage()
        {
            TimeOfReceiving = DateTime.Now.ToString("HH:mm:ss:fff");
        }

        public string TimeOfReceiving { get; set; }
        public string IPFrom { get; set; }
        public string Payload { get; set; }
        public string PackageSize { get; set; }
    }

}
