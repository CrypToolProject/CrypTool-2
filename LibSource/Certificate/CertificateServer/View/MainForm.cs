using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary;
using Org.BouncyCastle.Math;
using CrypTool.CertificateLibrary.Certificates;
using MySql.Data.MySqlClient;

namespace CrypTool.CertificateServer.View
{
    public partial class MainForm : Form
    {

        #region Private members

        private Configuration config;

        private UserControl currentView = null;

        private CertificateServer server;

        private CertificateDatabase certificateDatabase;

        private CertificationAuthority certificationAuthority;

        private RegistrationAuthority registrationAuthority;

        private DirectoryServer directoryServer;

        private SMTPEmailClient smtpClient;

        #endregion


        #region Initialization

        public MainForm()
        {
            InitializeComponent();
            InitializeView();
            InitializeBackends();

            this.serverStatusView.ImportTlsCertificateRequested += new EventHandler<EventArgs>(ShowImportCertificateDialog_Click);
            LoadRecentCertificates();
            ParseCommandLineParameter();
        }

        private void InitializeView()
        {
            this.currentView = serverStatusView;
            this.viewPanel.SuspendLayout();
            this.viewPanel.Controls.Add(serverStatusView);
            this.viewPanel.ResumeLayout();
            this.CreateHandle();
        }

        private void InitializeBackends()
        {
            this.config = Configuration.GetConfiguration();
            Log.Info(config.ToString());

            this.smtpClient = new SMTPEmailClient(config.SmtpServer.Server, config.SmtpServer.Port);

            this.certificateDatabase = new CertificateDatabase(
                config.Database.DatabaseName,
                config.Database.Host,
                config.Database.Password,
                config.Database.User,
                config.Database.Port,
                config.Database.Timeout);
            this.certificateDatabase.ConnectionSuccesfullyEstablished += new EventHandler<EventArgs>(OnDatabaseConnected);
            this.certificateDatabase.ConnectionBroken += new EventHandler<DatabaseErrorEventArgs>(OnDatabaseBroken);

            this.certificationAuthority = new CertificationAuthority(certificateDatabase, this.smtpClient);
            this.certificationAuthority.CACertificateChanged += new EventHandler<CACertificateEventArgs>(OnCACertificateChanged);
            this.certificationAuthority.TlsCertificateChanged += new EventHandler<PeerCertificateEventArgs>(OnTlsCertificateLoaded);
            this.serverStatusView.SetCertificationAuthority(this.certificationAuthority);
            this.createCACertView.SetCertificationAuthority(this.certificationAuthority);

            this.registrationAuthority = new RegistrationAuthority(this.certificationAuthority, this.certificateDatabase, this.smtpClient);
            this.registrationAuthorityView.SetRegistrationAuthority(this.registrationAuthority);

            this.directoryServer = new DirectoryServer(this.certificationAuthority, this.registrationAuthority, this.certificateDatabase, this.smtpClient);

            this.server = new CertificateServer(this.certificateDatabase, this.certificationAuthority, this.registrationAuthority, this.directoryServer, this.smtpClient);
            this.server.StatusChanged += new EventHandler<ServerStatusChangeEventArgs>(OnServerStatusChanged);
            this.serverStatusView.SetServer(this.server);
            this.createCACertView.SetServer(this.server);
        }

        /// <summary>
        /// Load recently used CA and TLS certificate (if exists)
        /// </summary>
        private void LoadRecentCertificates()
        {
            if (String.IsNullOrEmpty(config.CertificationAuthority.RecentCertificate.Serialnumber) || String.IsNullOrEmpty(config.CertificationAuthority.RecentCertificate.Password))
            {
                certificateDatabase.CanPing();
                return;
            }
            try
            {
                byte[] caCert = certificateDatabase.SelectCaOrTlsPkcs12(true, new BigInteger(config.CertificationAuthority.RecentCertificate.Serialnumber));
                if (caCert == null)
                {
                    Log.Warn(String.Format("No CA certificate with serialnumber {0} found in the database", config.CertificationAuthority.RecentCertificate.Serialnumber));
                    return;
                }

                byte[] tlsCert = certificateDatabase.SelectCaOrTlsPkcs12(false, new BigInteger(config.CertificationAuthority.RecentCertificate.Serialnumber));
                if (tlsCert == null)
                {
                    Log.Warn("No TLS certificate for the CA certificate found in the database");
                    return;
                }

                using (MemoryStream mstream = new MemoryStream(caCert))
                {
                    certificationAuthority.LoadCACertificate(mstream, config.CertificationAuthority.RecentCertificate.Password);
                }
                using (MemoryStream mstream = new MemoryStream(tlsCert))
                {
                    certificationAuthority.LoadTlsCertificate(mstream, config.CertificationAuthority.RecentCertificate.Password);
                }
            }
            catch (FormatException ex)
            {
                Log.Error(String.Format("Could not load recent CA certificate. The serialnumber {0} is not in a proper format:\n", config.CertificationAuthority.RecentCertificate.Serialnumber), ex);
            }
            catch (IOException ex)
            {
                Log.Error("Could not load recent CA certificate", ex);
            }
            catch (PKCS12FormatException ex)
            {
                Log.Error("Could not load recent CA certificate", ex);
            }
            catch (X509CertificateFormatException ex)
            {
                Log.Error("Could not load recent CA certificate", ex);
            }
            catch (DatabaseException)
            {
                // Message was already logged, so do nothing
            }
            catch (Exception ex)
            {
                Log.Error("Could not load recent CA certificate", ex);
                MessageBox.Show(this, "The certificate listed in your configuration file could not be opened.\n\n" + 
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                    "Error while importing certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParseCommandLineParameter()
        {
            foreach (string str in Environment.GetCommandLineArgs())
            {
                if (str.ToLower() == "/start")
                {
                    try
                    {
                        server.Start();
                        this.toolBarStartStopButton.Enabled = false;
                    }
                    catch
                    {
                        // Could not start, message already logged
                    }
                }
            }
        }

        #endregion


        #region Change View Event Handler

        private void MainForm_FormClosing(object sender, EventArgs e)
        {
            if (server.IsRunning)
            {
                server.Shutdown();
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowServerStatusView_Click(object sender, EventArgs e)
        {
            this.viewPanel.SuspendLayout();
            this.currentView.Visible = false;
            this.viewPanel.Controls.RemoveByKey(currentView.Name);
            this.viewPanel.Controls.Add(serverStatusView);
            this.serverStatusView.Visible = true;
            this.viewPanel.ResumeLayout();
            this.currentView = serverStatusView;
        }

        private void ShowLogView_Click(object sender, EventArgs e)
        {
            this.viewPanel.SuspendLayout();
            this.currentView.Visible = false;
            this.viewPanel.Controls.RemoveByKey(currentView.Name);
            this.viewPanel.Controls.Add(serverLogView);
            this.serverLogView.Visible = true;
            this.viewPanel.ResumeLayout();
            this.currentView = serverLogView;
        }

        private void ShowCreateCACertView_Click(object sender, EventArgs e)
        {
            this.viewPanel.SuspendLayout();
            this.currentView.Visible = false;
            this.viewPanel.Controls.RemoveByKey(currentView.Name);
            this.viewPanel.Controls.Add(createCACertView);
            this.createCACertView.Visible = true;
            this.createCACertView.TidyUpOnShow();
            this.viewPanel.ResumeLayout();
            this.currentView = createCACertView;
        }

        private void ShowRegistrationAuthorityView_Click(object sender, EventArgs e)
        {
            this.viewPanel.SuspendLayout();
            this.currentView.Visible = false;
            this.registrationAuthorityView.Reload();
            this.viewPanel.Controls.RemoveByKey(currentView.Name);
            this.viewPanel.Controls.Add(registrationAuthorityView);
            this.registrationAuthorityView.Visible = true;
            this.viewPanel.ResumeLayout();
            this.currentView = registrationAuthorityView;
        }

        #endregion


        #region Start server

        private void StartStopServer_Click(object sender, EventArgs e)
        {
            if (!server.IsRunning)
            {
                try
                {
                    server.Start();
                    this.toolBarStartStopButton.Enabled = false;
                }
                catch (CertificateException ex)
                {
                    MessageBox.Show(this, "Could not start P@Porator.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (DatabaseException ex)
                {
                    MessageBox.Show(this, "A database error occured.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message) +
                        "\n\nThe database is neccessary to store generated peer certificates, so you will not be able to start the server", "Error while importing certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                server.Stop();
                this.toolBarStartStopButton.Enabled = true;
            }
        }

        #endregion

        
        #region Import/Export CA and TLS certificates

        private void ShowImportCertificateDialog_Click(object sender, EventArgs e)
        {
            DialogResult result = importCertificateFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            string password = PasswordDialog.ShowDialog(
                "Enter Password",
                "Please enter the password to open the PKCS #12 Personanl Information Exchange store.\n\n" + this.importCertificateFileDialog.FileName,
                DialogType.Ask,
                DialogButtons.OKCancel);
            if (password == null)
            {
                return;
            }
            // Stop server to avoid inconsistency
            server.Stop();

            FileStream stream = null;
            try
            {
                stream = new FileStream(importCertificateFileDialog.FileName, FileMode.Open, FileAccess.Read);
                if (TlsClicked(sender))
                {
                    certificationAuthority.LoadTlsCertificate(stream, password);
                }
                else
                {
                    certificationAuthority.LoadCACertificate(stream, password);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while importing certificate.\n\n" +
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private void ShowExportCertificateDialog_Click(object sender, EventArgs e)
        {
            DialogResult result = exportCertificateFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            FileStream stream = null;
            try
            {
                stream = new FileStream(exportCertificateFileDialog.FileName, FileMode.Create, FileAccess.Write);
                if (TlsClicked(sender))
                {
                    if (exportCertificateFileDialog.FilterIndex == 1)
                    {
                        certificationAuthority.TlsCertificate.SaveAsCrt(stream);
                    }
                    else
                    {
                        certificationAuthority.TlsCertificate.SaveAsPkcs12(stream);
                    }
                }
                else
                {
                    if (exportCertificateFileDialog.FilterIndex == 1)
                    {
                        certificationAuthority.CaCertificate.SaveAsCrt(stream);
                    }
                    else
                    {
                        certificationAuthority.CaCertificate.SaveAsPkcs12(stream);
                    }
                }
                MessageBox.Show(this, "The certificate has been successfully exported.", "Certificate exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while exporting certificate.\n" +
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private void DeriveTlsCertificate_Click(object sender, EventArgs e)
        {
            try
            {
                certificationAuthority.DeriveTlsCertificate();
                MessageBox.Show(this, "A TLS certificate has been successfully derived from CA certificate", "TLS Certificate derived", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while generating TLS certificate.\n\n" +
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StoreInDatabaseMenuItem_Click(object sender, EventArgs e)
        {
            if (!certificationAuthority.CaCertificate.IsLoaded)
            {
                MessageBox.Show(this, "You do not have a valid CA certificate loaded.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!certificationAuthority.TlsCertificate.IsLoaded)
            {
                MessageBox.Show(this, "You do not have a valid TLS certificate loaded.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                certificationAuthority.StoreCaAndTlsCertificate(certificationAuthority.CaCertificate, certificationAuthority.TlsCertificate);
                this.config.CertificationAuthority.RecentCertificate.Serialnumber = certificationAuthority.CaCertificate.CaX509.SerialNumber.ToString();
                this.config.CertificationAuthority.RecentCertificate.Password = certificationAuthority.CaCertificate.Password;
                this.config.Save();
                MessageBox.Show(this, "The CA and TLS certificates have been successfully stored in the database", "Certificates successfully stored", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while storing CA and TLS certificates in the database.\n\n" +
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion


        #region Server, database and CA Event Handler

        private void OnServerStatusChanged(object sender, ServerStatusChangeEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.toolBar.SuspendLayout();
                if (e.IsRunning)
                {
                    this.toolBarStartStopButton.Image = global::CrypTool.CertificateServer.Properties.Resources.server_stop;
                    this.toolBarStartStopButton.ToolTipText = "Stop certification server";
                    this.statusBarStatusLabel.Text = "Server is running";
                    this.statusBarStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                    this.statusBarStatusLabel.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this.toolBarStartStopButton.Image = global::CrypTool.CertificateServer.Properties.Resources.server_start;
                    this.toolBarStartStopButton.ToolTipText = "Start certification server";
                    this.statusBarStatusLabel.Text = "Server is stopped";
                    this.statusBarStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                    // This line is c/p from VS generated code, very cool stuff ;)
                    this.statusBarStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                }
                this.toolBarStartStopButton.Enabled = true;
                this.toolBar.ResumeLayout();
            });
        }

        private void OnDatabaseConnected(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.statusBarDatabaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                this.statusBarDatabaseLabel.ForeColor = System.Drawing.Color.Green;
                this.statusBarDatabaseLabel.Text = String.Format("Database: {0}:{1}", certificateDatabase.Host, certificateDatabase.Port);
                if (certificationAuthority.CaCertificate.IsLoaded && certificationAuthority.TlsCertificate.IsLoaded)
                {
                    this.storeInDatabaseMenuItem.Enabled = true;
                }
            });
        }

        private void OnDatabaseBroken(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.statusBarDatabaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                this.statusBarDatabaseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                this.statusBarDatabaseLabel.Text = String.Format("Database: Connection is broken!");
                this.storeInDatabaseMenuItem.Enabled = false;
            });
        }

        private void OnCACertificateChanged(object sender, CACertificateEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                if (!e.Certificate.IsLoaded)
                {
                    this.toolBarStartStopButton.Enabled = false;
                    this.tlsCertificateMenuItem.Enabled = false;
                    this.exportCACertMenuItem.Enabled = false;
                    this.statusBarCertLabel.Text = "No CA certificate loaded";
                    this.statusBarCertLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                    this.statusBarCertLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                    return;
                }

                this.statusBarCertLabel.Text = "No TLS certificate loaded";
                this.statusBarCertLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                this.statusBarCertLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                this.tlsCertificateMenuItem.Enabled = true;
                this.exportCACertMenuItem.Enabled = true;

                this.SuspendLayout();
                this.viewPanel.Controls.RemoveByKey(currentView.Name);
                this.viewPanel.Controls.Add(serverStatusView);
                this.ResumeLayout();
                this.currentView = serverStatusView;
            });
        }

        private void OnTlsCertificateLoaded(object sender, PeerCertificateEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                if (!e.Certificate.IsLoaded)
                {
                    this.exportTlsMenuItem.Enabled = false;
                    this.storeInDatabaseMenuItem.Enabled = false;
                    this.toolBarStartStopButton.Enabled = false;
                    return;
                }

                this.statusBarCertLabel.Text = "Certificate: " + e.Certificate.Issuer.CommonName;
                this.statusBarCertLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                this.statusBarCertLabel.ForeColor = System.Drawing.Color.Green;
                this.toolBarStartStopButton.Enabled = true;
                this.exportTlsMenuItem.Enabled = true;
                this.storeInDatabaseMenuItem.Enabled = true;

                this.SuspendLayout();
                this.viewPanel.Controls.RemoveByKey(currentView.Name);
                this.viewPanel.Controls.Add(serverStatusView);
                this.ResumeLayout();
                this.currentView = serverStatusView;
            });
        }

        #endregion


        #region Some help methods

        /// <summary>
        /// Little helping method that returns true if the import/export TLS certificate menu item was clicked.
        /// Otherwise the import/export CA certificate menu item or button was clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private bool TlsClicked(object sender)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null && (item.Name.Equals(this.exportCACertMenuItem.Name) || item.Name.Equals(this.importCACertMenuItem.Name)))
            {
                return false;
            }
            ToolStripButton button = sender as ToolStripButton;
            if (button != null && button.Name.Equals(this.toolBarImportButton.Name))
            {
                return false;
            }
            return true;
        }

        #endregion

    }
}
