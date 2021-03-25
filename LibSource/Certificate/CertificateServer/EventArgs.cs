using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.CertificateLibrary.Certificates;

namespace CrypTool.CertificateServer
{
    /// <summary>
    /// A database error
    /// </summary>
    public class DatabaseErrorEventArgs : EventArgs
    {
        public DatabaseErrorEventArgs(string error)
        {
            this.Error = error;
        }

        public string Error { private set; get; }
    }

    public class CACertificateEventArgs : EventArgs
    {
        public CACertificateEventArgs(CACertificate caCert)
        {
            this.Certificate = caCert;
        }

        public CACertificate Certificate { get; private set; }
    }

    public class PeerCertificateEventArgs : EventArgs
    {
        public PeerCertificateEventArgs(PeerCertificate peerCert)
        {
            this.Certificate = peerCert;
        }

        public PeerCertificate Certificate { get; private set; }
    }

    public class ServerStatusChangeEventArgs : EventArgs
    {
        public ServerStatusChangeEventArgs(bool isRunning)
        {
            this.IsRunning = isRunning;
        }

        public bool IsRunning { get; private set; }
    }

}
