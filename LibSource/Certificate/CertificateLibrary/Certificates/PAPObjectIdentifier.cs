namespace CrypTool.CertificateLibrary.Certificates
{
    public static class PAPObjectIdentifier
    {
        // Using the following Private Enterprise Numbers (Object Identifier) (officially registered peer@play OID)
        public static readonly string PeersAtPlay = "1.3.6.1.4.1.37313";

        public static readonly string CertificateUsage = PeersAtPlay + ".1";

        public static readonly string CertificateVersion = PeersAtPlay + ".2";

        public static readonly string WorldName = PeersAtPlay + ".3";

        public static readonly string HashedEmail = PeersAtPlay + ".4";
    }

    public static class CertificateUsageValue
    {
        public static readonly string CA = "ca_certificate";

        public static readonly string TLS = "tls_certificate";

        public static readonly string PEER = "peer_certificate";
    }
}
