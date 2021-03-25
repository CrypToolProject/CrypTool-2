using System;
using CrypTool.CertificateLibrary.Certificates;
using System.Net;

namespace CrypTool.CertificateLibrary.Network
{

    public class CertificateReceivedEventArgs : EventArgs
    {
        public CertificateReceivedEventArgs(PeerCertificate peerCert, string password)
        {
            this.Certificate = peerCert;
            this.Password = password;
        }

        public PeerCertificate Certificate { get; private set; }

        public string Password { get; private set; }
    }

    public class ProcessingErrorEventArgs : EventArgs
    {
        public ProcessingErrorEventArgs(ErrorType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public ErrorType Type { get; private set; }

        public string Message { get; private set; }
    }

    public class ProtocolEventArgs : EventArgs
    {
        public ProtocolEventArgs(IProtocol protocol)
        {
            this.Protocol = protocol;
        }

        public IProtocol Protocol { get; private set; }
    }

    public class ProxyEventArgs : EventArgs
    {
        public ProxyEventArgs(HttpStatusCode code, string message = null)
        {
            this.StatusCode = code;
            this.Message = message;
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
