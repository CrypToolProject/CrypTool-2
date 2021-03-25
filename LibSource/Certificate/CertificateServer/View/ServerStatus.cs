using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrypTool.CertificateLibrary.Util;

namespace CrypTool.CertificateServer.View
{
    public partial class ServerStatus : UserControl
    {

        #region Private members

        private CertificateServer server = null;

        private CertificationAuthority certificationAuthority = null;

        #endregion


        #region Constructor

        public ServerStatus()
        {
            InitializeComponent();
            this.CreateHandle();
        }

        #endregion


        #region Initialization + Setter

        public void SetServer(CertificateServer server)
        {
            this.server = server;

            this.server.DatabaseBroken += new EventHandler<DatabaseErrorEventArgs>(OnDatabaseBroken);
            this.server.DatabaseConnected += new EventHandler<EventArgs>(OnDatabaseConnected);
            this.server.StatusChanged += new EventHandler<ServerStatusChangeEventArgs>(OnServerStatusChanged);

        }

        public void SetCertificationAuthority(CertificationAuthority ca)
        {
            this.certificationAuthority = ca;

            this.certificationAuthority.CACertificateChanged += new EventHandler<CACertificateEventArgs>(OnCACertificateChanged);
            this.certificationAuthority.TlsCertificateChanged += new EventHandler<PeerCertificateEventArgs>(OnTlsCertificateChanged);
            this.certificationAuthority.PeerCertificateStored += new EventHandler<PeerCertificateEventArgs>(OnPeerCertificateGenerated);
        }

        #endregion


        #region Event Handler

        private void OnDatabaseConnected(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.databaseValue.Text = ((CertificateDatabase)sender).Host + ":" + ((CertificateDatabase)sender).Port;
                this.databaseInfoValue.Text = ((CertificateDatabase)sender).DBName + "/" + ((CertificateDatabase)sender).User;
            });
            return;
        }

        private void OnDatabaseBroken(object sender, DatabaseErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.databaseValue.Text = "Not connected!";
                this.databaseInfoValue.Text = "n/a";
                //this.databaseValue.Text = "Connection is broken!";
                //this.databaseInfoValue.Text = "n/a";
            });
        }

        private void OnServerStatusChanged(object sender, ServerStatusChangeEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                if (e.IsRunning)
                {
                    serverStatusValue.Text = "Running";
                }
                else
                {
                    serverStatusValue.Text = "Stopped";
                }
            });
        }

        private void OnCACertificateChanged(object sender, CACertificateEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                if (!e.Certificate.IsLoaded)
                {
                    this.nameValue.Text = "n/a";
                    this.organisationValue.Text = "n/a";
                    this.organisationalUnitValue.Text = "n/a";
                    this.countryValue.Text = "n/a";
                    this.emailValue.Text = "n/a";
                    this.dateOfIssueValue.Text = "n/a";
                    this.dateOfExpireValue.Text = "n/a";
                    this.registeredPeersValue.Text = "0";
                    this.lastRegisterValue.Text = "n/a";
                    return;
                }
                this.nameValue.Text = e.Certificate.Issuer.CommonName;
                this.organisationValue.Text = e.Certificate.Issuer.Organisation;
                this.organisationalUnitValue.Text = e.Certificate.Issuer.OrganisationalUnit;
                this.countryValue.Text = Country.getCountry(e.Certificate.Issuer.Country);
                this.emailValue.Text = e.Certificate.Issuer.EmailAddress;
                this.dateOfIssueValue.Text = e.Certificate.CaX509.NotBefore.ToLocalTime().Date.ToShortDateString();
                this.dateOfExpireValue.Text = e.Certificate.CaX509.NotAfter.ToLocalTime().Date.ToShortDateString();

                this.registeredPeersValue.Text = certificationAuthority.PeerCertificateCount.ToString();

                if (certificationAuthority.DateOfLastRegister != DateTime.MinValue)
                {
                    this.lastRegisterValue.Text = certificationAuthority.DateOfLastRegister.ToLocalTime().ToString();
                }
                else
                {
                    this.lastRegisterValue.Text = "n/a";
                }
            });
        }

        private void OnTlsCertificateChanged(object sender, PeerCertificateEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                if (e.Certificate.IsLoaded)
                {
                    this.tlsValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    this.tlsValue.Text = e.Certificate.Subject.CommonName;
                    this.tlsValue.Click -= new EventHandler(tlsValue_Click);
                }
                else
                {
                    this.tlsValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    this.tlsValue.Text = "Not loaded";
                    this.tlsValue.Click += new EventHandler(tlsValue_Click);
                }
            });
        }

        private void OnPeerCertificateGenerated(object sender, PeerCertificateEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.registeredPeersValue.Text = certificationAuthority.PeerCertificateCount.ToString();
                this.lastRegisterValue.Text = e.Certificate.PeerX509.NotBefore.ToLocalTime().ToString();
            });
        }

        private void tlsValue_Click(object sender, EventArgs e)
        {
            if (this.ImportTlsCertificateRequested != null)
            {
                this.ImportTlsCertificateRequested.Invoke(sender, new EventArgs());
            }
        }

        #endregion


        #region Events

        public event EventHandler<EventArgs> ImportTlsCertificateRequested;

        #endregion

    }
}
