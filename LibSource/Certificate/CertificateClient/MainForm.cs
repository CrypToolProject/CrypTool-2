using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Util;
using CrypTool.CertificateLibrary;
using CrypTool.CertificateLibrary.Network;
using System.IO;

namespace CrypTool.CertificateClientSimpleGUI
{
    public partial class MainForm : Form
    {

        #region Constant and static readonly

        private static readonly Color INVALID_INPUT_BG_COLOR = Color.MistyRose;

        #endregion


        #region Private member

        private CertificateClient client;

        #endregion


        public MainForm()
        {
            InitializeComponent();
            this.certRegisterProcessingIcon.Hide();
            this.emailVerificationProcessingIcon.Hide();
            this.certRequestProcessingIcon.Hide();
            this.passwordResetProcessingIcon.Hide();
            this.passwordResetVerificationProcessingIcon.Hide();
            this.passwordChangeProcessingIcon.Hide();
            FillComboBoxes();

            this.client = new CertificateClient();
            this.client.ProgramName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            this.client.ProgramVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.client.ProgramLocale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            client.ProxyErrorOccured += new EventHandler<ProxyEventArgs>(OnProxyErrorOccured);
            client.NoProxyConfigured += new EventHandler<EventArgs>(OnNoProxyConfigured);
            client.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(OnHttpTunnelEstablished);
            client.SslCertificateRefused += new EventHandler<EventArgs>(OnSslCertificateRefused);

            client.CertificateReceived += new EventHandler<CertificateReceivedEventArgs>(OnCertificateReceived);
            client.CertificateAuthorizationRequired += new EventHandler<EventArgs>(OnCertificateAuthorizationRequired);
            client.EmailVerificationRequired += new EventHandler<EventArgs>(OnEmailVerificationRequired);
            client.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(OnServerErrorOccurred);
            client.NewProtocolVersion += new EventHandler<EventArgs>(OnNewProtocolVersion);

            InitializecConfigTab();
        }


        #region Method to configure the client on actions

        private void ConfigureClient()
        {
            // Proxy settings
            int serverPort = 0;
            if (Int32.TryParse(this.configServerPort.Text.Trim(), out serverPort))
            {
                this.client.ServerPort = serverPort;
            }
            else
            {
                throw new Exception("The entered server port is invalid");
            }

            if (!this.configProxyPort.Text.Equals(String.Empty))
            {
                int proxyPort = 0;
                if (Int32.TryParse(this.configProxyPort.Text.Trim(), out proxyPort))
                {
                    this.client.ProxyPort = proxyPort;
                }
                else
                {
                    throw new Exception("The entered proxy port is invalid");
                }
            }

            this.client.UseProxy = this.configUseProxy.Checked;
            this.client.UseSystemWideProxy = this.configUseSystemProxy.Checked;
            
            // Server settings
            this.client.ServerAddress = this.configServerAddress.Text;
            this.client.ProxyAddress = this.configProxyAddress.Text;

            this.client.ProxyAuthName = this.configProxyUsername.Text;
            this.client.ProxyAuthPassword = this.configProxyPassword.Text;

            this.statusLabel.Text = "";
        }

        #endregion


        #region Helper to organize the GUI

        private void actionTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage.Name.Equals("certRegisterTab"))
            {
                this.certRegisterStatusLabel.Text = "Enter your data to get a new account";
                this.certRegisterAvatarTextBox.Text = String.Empty;
                this.certRegisterAvatarTextBox.BackColor = SystemColors.Window;
                this.certRegisterEmailTextBox.Text = String.Empty;
                this.certRegisterEmailTextBox.BackColor = SystemColors.Window;
                this.certRegisterWorldTextBox.Text = String.Empty;
                this.certRegisterWorldTextBox.BackColor = SystemColors.Window;
                this.certRegisterPasswordTextBox.Text = String.Empty;
                this.certRegisterPasswordTextBox.BackColor = SystemColors.Window;
                this.certRegisterConfirmPasswordTextBox.Text = String.Empty;
                this.certRegisterConfirmPasswordTextBox.BackColor = SystemColors.Window;
            }
            else if (e.TabPage.Name.Equals("emailVerificationTab"))
            {
                this.emailVerificationStatusLabel.Text = "Enter the verification code as well as your password";
                this.emailVerificationCodeTextBox.Text = String.Empty;
                this.emailVerificationCodeTextBox.BackColor = SystemColors.Window;
            }
            else if (e.TabPage.Name.Equals("certRequestTab"))
            {
                this.certRequestStatusLabel.Text = "Enter avatar or email address as well as your password to get your certificate";
                this.certRequestAvatarEmailTextBox.Text = String.Empty;
                this.certRequestAvatarEmailTextBox.BackColor = SystemColors.Window;
                this.certRequestPasswordTextBox.Text = String.Empty;
                this.certRequestPasswordTextBox.BackColor = SystemColors.Window;
                if (this.certRequestComboBox.Items.Count > 0)
                {
                    this.certRequestComboBox.SelectedIndex = 0;
                }
            }
            else if (e.TabPage.Name.Equals("passwordResetTab"))
            {
                this.passwordResetStatusLabel.Text = "Enter your avatar or email address to reset your password";
                this.passwordResetAvatarEmailTextBox.Text = String.Empty;
                this.passwordResetAvatarEmailTextBox.BackColor = SystemColors.Window;
                if (this.passwordResetComboBox.Items.Count > 0)
                {
                    this.passwordResetComboBox.SelectedIndex = 0;
                }
            }
            else if (e.TabPage.Name.Equals("passwordChangeTab"))
            {
                this.passwordChangeStatusLabel.Text = "Enter your data to change your password";
                this.passwordChangeAvatarEmailTextBox.Text = String.Empty;
                this.passwordChangeAvatarEmailTextBox.BackColor = SystemColors.Window;
                this.passwordChangeOldPasswordTextBox.Text = String.Empty;
                this.passwordChangeOldPasswordTextBox.BackColor = SystemColors.Window;
                this.passwordChangeNewPasswordTextBox.Text = String.Empty;
                this.passwordChangeNewPasswordTextBox.BackColor = SystemColors.Window;
                this.passwordChangeConfirmPasswordTextBox.Text = String.Empty;
                this.passwordChangeConfirmPasswordTextBox.BackColor = SystemColors.Window;
                if (this.passwordChangeComboBox.Items.Count > 0)
                {
                    this.passwordChangeComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.passwordResetVerificationStatusLabel.Text = "Enter your verification code as well as your new password";
                this.passwordResetVerificationCodeTextBox.Text = String.Empty;
                this.passwordResetVerificationCodeTextBox.BackColor = SystemColors.Window;
                this.passwordResetVerificationPasswordTextBox.Text = String.Empty;
                this.passwordResetVerificationPasswordTextBox.BackColor = SystemColors.Window;
                this.passwordResetVerificationConfirmTextBox.Text = String.Empty;
                this.passwordResetVerificationConfirmTextBox.BackColor = SystemColors.Window;
            }
        }

        private void FillComboBoxes()
        {
            string[] entries = new string[] { "Avatar", "Email Address" };
            // Fill the comboboxes
            foreach (string entry in entries)
            {
                this.certRequestComboBox.Items.Add(entry);
                this.passwordResetComboBox.Items.Add(entry);
                this.passwordChangeComboBox.Items.Add(entry);
            }
        }

        private void InitializecConfigTab()
        {
            this.configServerAddress.Text = global::CrypTool.CertificateClientSimpleGUI.Properties.Settings.Default.SERVER;
            this.configServerPort.Text = global::CrypTool.CertificateClientSimpleGUI.Properties.Settings.Default.PORT.ToString();

            // Check proxy configuration
            this.configProxyAddress.Text = client.GetSystemProxyAddress();
            int proxyPort = client.GetSystemProxyPort();
            this.configProxyPort.Text = (proxyPort == -1) ? "" : proxyPort.ToString();

            // if proxy is configured, enable and use it
            this.SetUseSystemProxyElements(!this.configProxyAddress.Text.Equals(""));
            this.SetUseProxyElements(!this.configProxyAddress.Text.Equals(""));
        }

        private void SetUseProxyElements(bool useProxy)
        {
            this.configUseProxy.Checked = useProxy;
            this.configUseSystemProxy.Enabled = !useProxy;

            this.configProxyAddress.Enabled = useProxy && !this.configUseSystemProxy.Checked;
            this.configProxyPort.Enabled = useProxy && !this.configUseSystemProxy.Checked;
            this.configProxyUsername.Enabled = useProxy;
            this.configProxyPassword.Enabled = useProxy;
            this.configUseSystemProxy.Enabled = useProxy;
        }

        private void SetUseSystemProxyElements(bool useSystemProxy)
        {
            this.configUseSystemProxy.Checked = useSystemProxy;
            this.configProxyAddress.Enabled = this.configUseProxy.Checked && !useSystemProxy;
            this.configProxyPort.Enabled = this.configUseProxy.Checked && !useSystemProxy;

            configProxyPort.BackColor = System.Drawing.SystemColors.Window;

            if (useSystemProxy)
            {
                // Get current proxy configuration
                this.configProxyAddress.Text = client.GetSystemProxyAddress();
                int proxyPort = client.GetSystemProxyPort();
                this.configProxyPort.Text = (proxyPort == -1) ? "" : proxyPort.ToString();
            }
        }

        #endregion


        #region User Control Event Handler (Certificate Registration)

        private void certRegisterButton_Click(object sender, EventArgs e)
        {
            this.certRegisterAvatarTextBox.BackColor = SystemColors.Window;
            this.certRegisterEmailTextBox.BackColor = SystemColors.Window;
            this.certRegisterWorldTextBox.BackColor = SystemColors.Window;
            this.certRegisterPasswordTextBox.BackColor = SystemColors.Window;
            this.certRegisterConfirmPasswordTextBox.BackColor = SystemColors.Window;

            if (!Verification.IsValidAvatar(certRegisterAvatarTextBox.Text))
            {
                this.certRegisterStatusLabel.Text = "Please enter a valid avatar to register a certificate";
                this.certRegisterPasswordTextBox.Clear();
                this.certRegisterConfirmPasswordTextBox.Clear();
                this.certRegisterAvatarTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterAvatarTextBox.Focus();
                return;
            }
            if (!Verification.IsValidEmailAddress(certRegisterEmailTextBox.Text))
            {
                this.certRegisterStatusLabel.Text = "Please enter a valid email address to register a certificate";
                this.certRegisterPasswordTextBox.Clear();
                this.certRegisterConfirmPasswordTextBox.Clear();
                this.certRegisterEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterEmailTextBox.Focus();
                return;
            }
            if (!Verification.IsValidWorld(certRegisterWorldTextBox.Text))
            {
                this.certRegisterStatusLabel.Text = "Please enter a valid world to register a certificate";
                this.certRegisterPasswordTextBox.Clear();
                this.certRegisterConfirmPasswordTextBox.Clear();
                this.certRegisterWorldTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterWorldTextBox.Focus();
                return;
            }
            if (!Verification.IsValidPassword(certRegisterPasswordTextBox.Text))
            {
                this.certRegisterStatusLabel.Text = "Please enter a valid password to register a certificate";
                this.certRegisterPasswordTextBox.Clear();
                this.certRegisterConfirmPasswordTextBox.Clear();
                this.certRegisterPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterPasswordTextBox.Focus();
                return;
            }
            if (!certRegisterPasswordTextBox.Text.Equals(certRegisterConfirmPasswordTextBox.Text))
            {
                certRegisterStatusLabel.Text = "You entered two different passwords.";
                certRegisterPasswordTextBox.Clear();
                certRegisterConfirmPasswordTextBox.Clear();
                certRegisterPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                certRegisterConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                certRegisterPasswordTextBox.Focus();
                return;
            }

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.certRegisterProcessingIcon.Show();

            CertificateRegistration certRegistration = new CertificateRegistration(certRegisterAvatarTextBox.Text, certRegisterEmailTextBox.Text, certRegisterWorldTextBox.Text, certRegisterPasswordTextBox.Text);

            BackgroundWorker sendCertificateRegistrationtWorker = new BackgroundWorker();
            sendCertificateRegistrationtWorker.DoWork += new DoWorkEventHandler(SendCertificateRegistration);
            sendCertificateRegistrationtWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunCertificateRegistrationCompleted);
            sendCertificateRegistrationtWorker.RunWorkerAsync(certRegistration);
        }

        private void RunCertificateRegistrationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.certRegisterProcessingIcon.Hide();
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                client.InvalidCertificateRegistration -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidCertificateRegistration);
            });
        }

        #endregion


        #region Textbox Validation (Certificate Registration)

        private void certRegisterAvatarTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRegisterAvatarTextBox.Text = this.certRegisterAvatarTextBox.Text.Trim();
            if (!Verification.IsValidAvatar(this.certRegisterAvatarTextBox.Text))
            {
                this.certRegisterAvatarTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.certRegisterAvatarTextBox.BackColor = SystemColors.Window;
            }
        }

        private void certRegisterEmailTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRegisterEmailTextBox.Text = this.certRegisterEmailTextBox.Text.Trim();
            if (!Verification.IsValidEmailAddress(certRegisterEmailTextBox.Text))
            {
                this.certRegisterEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.certRegisterEmailTextBox.BackColor = SystemColors.Window;
            }
        }

        private void certRegisterWorldTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRegisterWorldTextBox.Text = this.certRegisterWorldTextBox.Text.Trim();
            if (!Verification.IsValidWorld(this.certRegisterWorldTextBox.Text))
            {
                this.certRegisterWorldTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.certRegisterWorldTextBox.BackColor = SystemColors.Window;
            }
        }

        private void certRegisterConfirmPasswordTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRegisterPasswordTextBox.Text = this.certRegisterPasswordTextBox.Text.Trim();
            this.certRegisterConfirmPasswordTextBox.Text = this.certRegisterConfirmPasswordTextBox.Text.Trim();
            if (!Verification.IsValidPassword(this.certRegisterPasswordTextBox.Text) || !this.certRegisterPasswordTextBox.Text.Equals(this.certRegisterConfirmPasswordTextBox.Text))
            {
                this.certRegisterPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRegisterConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.certRegisterPasswordTextBox.BackColor = SystemColors.Window;
                this.certRegisterConfirmPasswordTextBox.BackColor = SystemColors.Window;
            }
        }

        #endregion


        #region Send message to server (Certificate Registration)

        private void SendCertificateRegistration(object sender, DoWorkEventArgs e)
        {
            try 
            {
                CertificateRegistration certRegistration = e.Argument as CertificateRegistration;
                if (certRegistration == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no CertificateRegistration object!");
                }

                this.ConfigureClient();
                client.InvalidCertificateRegistration += new EventHandler<ProcessingErrorEventArgs>(OnInvalidCertificateRegistration);
                this.client.RegisterCertificate(certRegistration);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Certificate registration has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while registering a new certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region User Control Event Handler (Email Verification)

        private void emailVerificationButton_Click(object sender, EventArgs e)
        {
            this.emailVerificationCodeTextBox.BackColor = SystemColors.Window;

            if (!Verification.IsValidCode(this.emailVerificationCodeTextBox.Text))
            {
                this.emailVerificationStatusLabel.Text = "Please enter a valid verification code";
                this.emailVerificationCodeTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.emailVerificationCodeTextBox.Focus();
                return;
            }

            EmailVerification emailVerification = new EmailVerification(this.emailVerificationCodeTextBox.Text, this.emailVerificationDeleteCheckBox.Checked);

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.emailVerificationProcessingIcon.Show();

            BackgroundWorker sendEmailVerificationWorker = new BackgroundWorker();
            sendEmailVerificationWorker.DoWork += new DoWorkEventHandler(SendEmailVerification);
            sendEmailVerificationWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunEmailVerificationCompleted);
            sendEmailVerificationWorker.RunWorkerAsync(emailVerification);
        }

        private void RunEmailVerificationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.emailVerificationProcessingIcon.Hide();
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                this.client.InvalidEmailVerification -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidEmailVerification);
                this.client.RegistrationDeleted -= new EventHandler<EventArgs>(OnRegistrationDeleted);
                this.client.EmailVerified -= new EventHandler<EventArgs>(OnEmailVerified);
            });
        }

        #endregion


        #region Textbox Validation (Email Verification)

        private void emailVerificationCodeTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.emailVerificationCodeTextBox.Text = this.emailVerificationCodeTextBox.Text.Trim();
            this.emailVerificationCodeTextBox.BackColor = (!Verification.IsValidCode(this.emailVerificationCodeTextBox.Text))
                ? INVALID_INPUT_BG_COLOR
                : SystemColors.Window;
        }

        private void emailVerificationCodeTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.emailVerificationCodeTextBox.Text))
            {
                this.emailVerificationCodeTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        #endregion


        #region Send message to server (Email Verification)

        private void SendEmailVerification(object sender, DoWorkEventArgs e)
        {
            try
            {
                EmailVerification emailVerification = e.Argument as EmailVerification;
                if (emailVerification == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no EmailVerification object!");
                }

                this.ConfigureClient();
                this.client.InvalidEmailVerification += new EventHandler<ProcessingErrorEventArgs>(OnInvalidEmailVerification);
                this.client.RegistrationDeleted += new EventHandler<EventArgs>(OnRegistrationDeleted);
                this.client.EmailVerified += new EventHandler<EventArgs>(OnEmailVerified);
                this.client.VerifyEmail(emailVerification);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Email Verification has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while requesting a certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region User Control Event Handler (Certificate Request)

        private void certRequestButton_Click(object sender, EventArgs e)
        {
            this.certRequestAvatarEmailTextBox.BackColor = SystemColors.Window;
            this.certRequestPasswordTextBox.BackColor = SystemColors.Window;

            if (!Verification.IsValidPassword(this.certRequestPasswordTextBox.Text))
            {
                this.certRequestStatusLabel.Text = "Please enter a valid password to request a certificate";
                this.certRequestPasswordTextBox.Clear();
                this.certRequestPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.certRequestPasswordTextBox.Focus();
                return;
            }

            CT2CertificateRequest certRequest = null;
            if (this.certRequestComboBox.SelectedIndex == this.certRequestComboBox.Items.IndexOf("Avatar"))
            {
                if (!Verification.IsValidAvatar(this.certRequestAvatarEmailTextBox.Text))
                {
                    this.certRequestStatusLabel.Text = "Please enter a valid avatar to request a certificate";
                    this.certRequestPasswordTextBox.Clear();
                    this.certRequestAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.certRequestAvatarEmailTextBox.BackColor = SystemColors.Window;
                    certRequest = new CT2CertificateRequest(certRequestAvatarEmailTextBox.Text, null, certRequestPasswordTextBox.Text);
                }
            }
            else
            {
                if (!Verification.IsValidEmailAddress(this.certRequestAvatarEmailTextBox.Text))
                {
                    this.certRequestStatusLabel.Text = "Please enter a valid email to request a certificate";
                    this.certRequestPasswordTextBox.Clear();
                    this.certRequestAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.certRequestAvatarEmailTextBox.BackColor = SystemColors.Window;
                    certRequest = new CT2CertificateRequest(null, certRequestAvatarEmailTextBox.Text, certRequestPasswordTextBox.Text);
                }
            }

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.certRequestProcessingIcon.Show();

            BackgroundWorker sendCertificateRequestWorker = new BackgroundWorker();
            sendCertificateRequestWorker.DoWork += new DoWorkEventHandler(SendCertificateRequest);
            sendCertificateRequestWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunCertificateRequestCompleted);
            sendCertificateRequestWorker.RunWorkerAsync(certRequest);
        }

        private void RunCertificateRequestCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.certRequestProcessingIcon.Hide();
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                client.InvalidCertificateRequest -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidCertificateRequest);
            });
        }

        #endregion


        #region Textbox Validation (Certificate Request)

        private void certRequestAvatarEmailTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRequestAvatarEmailTextBox.Text = this.certRequestAvatarEmailTextBox.Text.Trim();
            if (this.certRequestComboBox.SelectedIndex == this.certRequestComboBox.Items.IndexOf("Avatar"))
            {
                this.certRequestAvatarEmailTextBox.BackColor = (!Verification.IsValidAvatar(this.certRequestAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
            else
            {
                this.certRequestAvatarEmailTextBox.BackColor = (!Verification.IsValidEmailAddress(this.certRequestAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
        }

        private void certRequestAvatarEmailTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.certRequestAvatarEmailTextBox.Text))
            {
                this.certRequestAvatarEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void certRequestPasswordTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.certRequestPasswordTextBox.Text = this.certRequestPasswordTextBox.Text.Trim();
            this.certRequestPasswordTextBox.BackColor = (!Verification.IsValidPassword(this.certRequestPasswordTextBox.Text))
                ? INVALID_INPUT_BG_COLOR
                : SystemColors.Window;
        }

        private void certRequestPasswordTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.certRequestPasswordTextBox.Text))
            {
                this.certRequestPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void certRequestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.certRequestAvatarEmailTextBox.Text))
            {
                this.certRequestAvatarEmailTextBox_Validating(this, new CancelEventArgs());
            }
        }

        #endregion


        #region Send message to server (Certificate Request)

        private void SendCertificateRequest(object sender, DoWorkEventArgs e)
        {
            try
            {
                CT2CertificateRequest certRequest = e.Argument as CT2CertificateRequest;
                if (certRequest == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no CT2CertificateRequest object!");
                }

                this.ConfigureClient();
                client.InvalidCertificateRequest += new EventHandler<ProcessingErrorEventArgs>(OnInvalidCertificateRequest);
                client.RequestCertificate(certRequest);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Certificate request has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while requesting a certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region User Control Event Handler (Password Reset)

        private void passwordResetButton_Click(object sender, EventArgs e)
        {
            this.passwordResetAvatarEmailTextBox.BackColor = SystemColors.Window;

            PasswordReset passwordReset = null;
            if (this.passwordResetComboBox.SelectedIndex == this.passwordResetComboBox.Items.IndexOf("Avatar"))
            {
                if (!Verification.IsValidAvatar(this.passwordResetAvatarEmailTextBox.Text))
                {
                    this.passwordResetStatusLabel.Text = "Please enter a valid avatar to reset the password of your certificate";
                    this.passwordResetAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.passwordResetAvatarEmailTextBox.BackColor = SystemColors.Window;
                    passwordReset = new PasswordReset(this.passwordResetAvatarEmailTextBox.Text, null);
                }
            }
            else
            {
                if (!Verification.IsValidEmailAddress(this.passwordResetAvatarEmailTextBox.Text))
                {
                    this.passwordResetStatusLabel.Text = "Please enter a valid email address to reset the password of your certificate";
                    this.passwordResetAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.passwordResetAvatarEmailTextBox.BackColor = SystemColors.Window;
                    passwordReset = new PasswordReset(null, this.passwordResetAvatarEmailTextBox.Text);
                }
            }

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.passwordResetProcessingIcon.Show();

            BackgroundWorker sendCertificateRequestWorker = new BackgroundWorker();
            sendCertificateRequestWorker.DoWork += new DoWorkEventHandler(SendPasswordReset);
            sendCertificateRequestWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunPasswordResetCompleted);
            sendCertificateRequestWorker.RunWorkerAsync(passwordReset);
        }

        private void RunPasswordResetCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.passwordResetProcessingIcon.Hide();
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                client.InvalidPasswordReset -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordReset);
                client.PasswordResetVerificationRequired -= new EventHandler<EventArgs>(OnPasswordResetVerificationRequired);
            });
        }

        #endregion


        #region Textbox Validation (Password Reset)

        private void passwordResetAvatarEmailTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordResetAvatarEmailTextBox.Text = this.passwordResetAvatarEmailTextBox.Text.Trim();
            if (this.passwordResetComboBox.SelectedIndex == this.passwordResetComboBox.Items.IndexOf("Avatar"))
            {
                this.passwordResetAvatarEmailTextBox.BackColor = (!Verification.IsValidAvatar(this.passwordResetAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
            else
            {
                this.passwordResetAvatarEmailTextBox.BackColor = (!Verification.IsValidEmailAddress(this.passwordResetAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
        }

        private void passwordResetAvatarEmailTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordResetAvatarEmailTextBox.Text))
            {
                this.passwordResetAvatarEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void passwordResetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.passwordResetAvatarEmailTextBox.Text))
            {
                this.passwordResetAvatarEmailTextBox_Validating(this, new CancelEventArgs());
            }
        }

        #endregion


        #region Send message to server (Password Reset)

        private void SendPasswordReset(object sender, DoWorkEventArgs e)
        {
            try
            {
                PasswordReset passwordReset = e.Argument as PasswordReset;
                if (passwordReset == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no PasswordReset object!");
                }

                this.ConfigureClient();
                client.InvalidPasswordReset += new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordReset);
                client.PasswordResetVerificationRequired += new EventHandler<EventArgs>(OnPasswordResetVerificationRequired);
                client.ResetPassword(passwordReset);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Password reset has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while requesting a password reset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region User Control Event Handler (Password Reset Verification)

        private void passwordResetVerificationButton_Click(object sender, EventArgs e)
        {
            this.passwordResetVerificationConfirmTextBox.BackColor = SystemColors.Window;
            this.passwordResetVerificationPasswordTextBox.BackColor = SystemColors.Window;
            this.passwordResetVerificationCodeTextBox.BackColor = SystemColors.Window;

            if (!Verification.IsValidPassword(this.passwordResetVerificationPasswordTextBox.Text) || !this.passwordResetVerificationPasswordTextBox.Text.Equals(this.passwordResetVerificationConfirmTextBox.Text))
            {
                this.passwordResetVerificationStatusLabel.Text = "Please enter a valid new password";
                this.passwordResetVerificationPasswordTextBox.Clear();
                this.passwordResetVerificationPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordResetVerificationPasswordTextBox.Focus();
                return;
            }
            if (!Verification.IsValidCode(this.passwordResetVerificationCodeTextBox.Text))
            {
                this.passwordResetVerificationStatusLabel.Text = "Please enter a valid verification code to reset your password";
                this.passwordResetVerificationPasswordTextBox.Clear();
                this.passwordResetVerificationCodeTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordResetVerificationCodeTextBox.Focus();
                return;
            }

            PasswordResetVerification passwordResetVerification = new PasswordResetVerification(this.passwordResetVerificationPasswordTextBox.Text, this.passwordResetVerificationCodeTextBox.Text);

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.passwordResetVerificationProcessingIcon.Show();

            BackgroundWorker sendPasswordResetVerificationWorker = new BackgroundWorker();
            sendPasswordResetVerificationWorker.DoWork += new DoWorkEventHandler(SendPasswordResetVerification);
            sendPasswordResetVerificationWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunPasswordResetVerificationCompleted);
            sendPasswordResetVerificationWorker.RunWorkerAsync(passwordResetVerification);
        }

        private void RunPasswordResetVerificationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                this.passwordResetVerificationProcessingIcon.Hide();
                client.InvalidPasswordResetVerification -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordResetVerification);
            });
        }

        #endregion


        #region Textbox Validation (Password Reset Verification)

        private void passwordResetVerificationConfirmTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordResetVerificationPasswordTextBox.Text = this.passwordResetVerificationPasswordTextBox.Text.Trim();
            this.passwordResetVerificationConfirmTextBox.Text = this.passwordResetVerificationConfirmTextBox.Text.Trim();
            if (!Verification.IsValidPassword(this.passwordResetVerificationPasswordTextBox.Text) || !this.passwordResetVerificationPasswordTextBox.Text.Equals(this.passwordResetVerificationConfirmTextBox.Text))
            {
                this.passwordResetVerificationPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordResetVerificationConfirmTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.passwordResetVerificationPasswordTextBox.BackColor = SystemColors.Window;
                this.passwordResetVerificationConfirmTextBox.BackColor = SystemColors.Window;
            }
        }

        private void passwordResetVerificationConfirmTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordResetVerificationPasswordTextBox.Text) || String.IsNullOrEmpty(this.passwordResetVerificationConfirmTextBox.Text))
            {
                this.passwordResetVerificationPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordResetVerificationConfirmTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void passwordResetVerificationCodeTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordResetVerificationCodeTextBox.Text = this.passwordResetVerificationCodeTextBox.Text.Trim();
            this.passwordResetVerificationCodeTextBox.BackColor = (!Verification.IsValidCode(this.passwordResetVerificationCodeTextBox.Text))
                ? INVALID_INPUT_BG_COLOR
                : SystemColors.Window;
        }

        private void passwordResetVerificationCodeTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordResetVerificationCodeTextBox.Text))
            {
                this.passwordResetVerificationCodeTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        #endregion


        #region Send message to server (Password Reset Verification)

        private void SendPasswordResetVerification(object sender, DoWorkEventArgs e)
        {
            try
            {
                PasswordResetVerification passwordResetVerification = e.Argument as PasswordResetVerification;
                if (passwordResetVerification == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no PasswordResetVerification object!");
                }

                this.ConfigureClient();
                client.InvalidPasswordResetVerification += new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordResetVerification);
                client.VerifyPasswordReset(passwordResetVerification);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Password Reset Verification has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while validating password reset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region User Control Event Handler (Password Change)

        private void passwordChangeButton_Click(object sender, EventArgs e)
        {
            this.passwordChangeAvatarEmailTextBox.BackColor = SystemColors.Window;
            this.passwordChangeOldPasswordTextBox.BackColor = SystemColors.Window;
            this.passwordChangeNewPasswordTextBox.BackColor = SystemColors.Window;
            this.passwordChangeConfirmPasswordTextBox.BackColor = SystemColors.Window;

            if (!Verification.IsValidPassword(this.passwordChangeNewPasswordTextBox.Text) || !this.passwordChangeNewPasswordTextBox.Text.Equals(this.passwordChangeConfirmPasswordTextBox.Text))
            {
                this.passwordChangeStatusLabel.Text = "Please enter a valid new password";
                this.passwordChangeNewPasswordTextBox.Clear();
                this.passwordChangeNewPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordChangeNewPasswordTextBox.Focus();
                this.passwordChangeConfirmPasswordTextBox.Clear();
                this.passwordChangeConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                return;
            }
            if (!Verification.IsValidPassword(this.passwordChangeOldPasswordTextBox.Text))
            {
                this.passwordChangeStatusLabel.Text = "Please enter your old password";
                this.passwordChangeOldPasswordTextBox.Clear();
                this.passwordChangeOldPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordChangeOldPasswordTextBox.Focus();
                return;
            }

            PasswordChange passwordChange = null;
            if (this.passwordChangeComboBox.SelectedIndex == this.passwordChangeComboBox.Items.IndexOf("Avatar"))
            {
                if (!Verification.IsValidAvatar(this.passwordChangeAvatarEmailTextBox.Text))
                {
                    this.passwordChangeStatusLabel.Text = "Please enter a valid avatar";
                    this.passwordChangeNewPasswordTextBox.Clear();
                    this.passwordChangeConfirmPasswordTextBox.Clear();
                    this.passwordChangeAvatarEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                    this.passwordChangeAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.passwordChangeAvatarEmailTextBox.BackColor = SystemColors.Window;
                    passwordChange = new PasswordChange(this.passwordChangeAvatarEmailTextBox.Text, null, this.passwordChangeOldPasswordTextBox.Text, this.passwordChangeNewPasswordTextBox.Text);
                }
            }
            else
            {
                if (!Verification.IsValidEmailAddress(passwordChangeAvatarEmailTextBox.Text))
                {
                    this.passwordChangeStatusLabel.Text = "Please enter a valid email address";
                    this.passwordChangeNewPasswordTextBox.Clear();
                    this.passwordChangeConfirmPasswordTextBox.Clear();
                    this.passwordChangeAvatarEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                    this.passwordChangeAvatarEmailTextBox.Focus();
                    return;
                }
                else
                {
                    this.passwordChangeAvatarEmailTextBox.BackColor = SystemColors.Window;
                    passwordChange = new PasswordChange(null, this.passwordChangeAvatarEmailTextBox.Text, this.passwordChangeOldPasswordTextBox.Text, this.passwordChangeNewPasswordTextBox.Text);
                }
            }

            // Everythings ok, now start a new thread that sends a request
            this.certRegisterButton.Enabled = false;
            this.emailVerificationButton.Enabled = false;
            this.certRequestButton.Enabled = false;
            this.passwordResetButton.Enabled = false;
            this.passwordResetVerificationButton.Enabled = false;
            this.passwordChangeButton.Enabled = false;
            this.passwordChangeProcessingIcon.Show();

            BackgroundWorker sendPasswordChangeWorker = new BackgroundWorker();
            sendPasswordChangeWorker.DoWork += new DoWorkEventHandler(SendPasswordChange);
            sendPasswordChangeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunPasswordChangeCompleted);
            sendPasswordChangeWorker.RunWorkerAsync(passwordChange);
        }

        private void RunPasswordChangeCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.certRegisterButton.Enabled = true;
                this.emailVerificationButton.Enabled = true;
                this.certRequestButton.Enabled = true;
                this.passwordResetButton.Enabled = true;
                this.passwordResetVerificationButton.Enabled = true;
                this.passwordChangeButton.Enabled = true;
                this.passwordChangeProcessingIcon.Hide();
                client.InvalidPasswordChange -= new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordChange);
            });
        }

        #endregion


        #region Textbox Validation (Password Change)

        private void passwordChangeConfirmPasswordTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordChangeNewPasswordTextBox.Text = this.passwordChangeNewPasswordTextBox.Text.Trim();
            this.passwordChangeConfirmPasswordTextBox.Text = this.passwordChangeConfirmPasswordTextBox.Text.Trim();
            if (!Verification.IsValidPassword(this.passwordChangeNewPasswordTextBox.Text) || !this.passwordChangeNewPasswordTextBox.Text.Equals(this.passwordChangeConfirmPasswordTextBox))
            {
                this.passwordChangeNewPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordChangeConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
            else
            {
                this.passwordChangeNewPasswordTextBox.BackColor = SystemColors.Window;
                this.passwordChangeConfirmPasswordTextBox.BackColor = SystemColors.Window;
            }
        }

        private void passwordChangeConfirmPasswordTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordChangeNewPasswordTextBox.Text) || String.IsNullOrEmpty(this.passwordChangeConfirmPasswordTextBox.Text))
            {
                this.passwordChangeNewPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                this.passwordChangeConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void passwordChangeAvatarEmailTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordChangeAvatarEmailTextBox.Text = this.passwordChangeAvatarEmailTextBox.Text.Trim();
            if (this.passwordChangeComboBox.SelectedIndex == this.passwordChangeComboBox.Items.IndexOf("Avatar"))
            {
                this.passwordChangeAvatarEmailTextBox.BackColor = (!Verification.IsValidAvatar(this.passwordChangeAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
            else
            {
                this.passwordChangeAvatarEmailTextBox.BackColor = (!Verification.IsValidEmailAddress(this.passwordChangeAvatarEmailTextBox.Text))
                    ? INVALID_INPUT_BG_COLOR
                    : SystemColors.Window;
            }
        }

        private void passwordChangeOldPasswordTextBox_Validating(object sender, CancelEventArgs e)
        {
            this.passwordChangeOldPasswordTextBox.Text = this.passwordChangeOldPasswordTextBox.Text.Trim();
            this.passwordChangeOldPasswordTextBox.BackColor = (!Verification.IsValidPassword(this.passwordChangeOldPasswordTextBox.Text))
                ? INVALID_INPUT_BG_COLOR
                : SystemColors.Window;
        }

        private void passwordChangeOldPasswordTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordChangeOldPasswordTextBox.Text))
            {
                this.passwordChangeOldPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void passwordChangeAvatarEmailTextBox_Validated(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.passwordChangeAvatarEmailTextBox.Text))
            {
                this.passwordChangeAvatarEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
            }
        }

        private void passwordChangeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.passwordChangeAvatarEmailTextBox.Text))
            {
                this.passwordChangeAvatarEmailTextBox_Validating(this, new CancelEventArgs());
            }
        }

        #endregion


        #region Send message to server (Password Change)

        private void SendPasswordChange(object sender, DoWorkEventArgs e)
        {
            try
            {
                PasswordChange passwordChange = e.Argument as PasswordChange;
                if (passwordChange == null)
                {
                    throw new ArgumentException("DoWorkEventArgs argument is no PasswordChange object!");
                }

                this.ConfigureClient();
                client.InvalidPasswordChange += new EventHandler<ProcessingErrorEventArgs>(OnInvalidPasswordChange);
                client.ChangePassword(passwordChange);
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Password Change has failed.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while changing password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        #endregion


        #region Configuration EventHandler

        private void configServerPort_Leave(object sender, EventArgs e)
        {
            configServerPort.BackColor = System.Drawing.SystemColors.Window;
            int port = 0;
            if (!Int32.TryParse(this.configServerPort.Text.Trim(), out port))
            {
                configServerPort.BackColor = (this.configServerPort.Text.Equals(""))
                    ? System.Drawing.SystemColors.Window
                    : INVALID_INPUT_BG_COLOR;
            }
        }

        private void configProxyPort_Leave(object sender, EventArgs e)
        {
            configProxyPort.BackColor = System.Drawing.SystemColors.Window;
            int port = 0;
            if (!Int32.TryParse(this.configProxyPort.Text.Trim(), out port))
            {
                configProxyPort.BackColor = (this.configProxyPort.Text.Equals(""))
                    ? System.Drawing.SystemColors.Window
                    : INVALID_INPUT_BG_COLOR;
            }
        }

        private void configUseProxy_CheckedChanged(object sender, EventArgs e)
        {
            this.SetUseProxyElements(this.configUseProxy.Checked);
        }

        private void configProxyUseSystem_CheckedChanged(object sender, EventArgs e)
        {
            this.SetUseSystemProxyElements(this.configUseSystemProxy.Checked);
        }

        #endregion


        #region Network Message Event Handler

        private void OnCertificateReceived(object sender, CertificateReceivedEventArgs e)
        {
            try
            {
                e.Certificate.SavePkcs12ToAppData(e.Password);
                e.Certificate.SaveCrtToAppData();
                string msg = String.Format("Peer certificate for user {0} successfully saved in AppData", e.Certificate.Avatar);
                Log.Info(msg);
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, msg,
                        "Certificate successfully received", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    MessageBox.Show(this, "Could not save the received certificate to your AppData folder.\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error while saving the certificate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void OnRegistrationDeleted(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "The certificate registration was successfully deleted.", "Registration deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        
        private void OnEmailVerificationRequired(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "The certificate server wants to validate your email address. An email containing a validation code has been sent to your email address.", "Email address verification required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnEmailVerified(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "The email address has been successfully verified. You can now request your certificate.", "Email address verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnPasswordResetVerificationRequired(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "The certificate server needs to validate your password reset request. An email containing a validation code has been sent to your email address.", "Password reset verification required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnCertificateAuthorizationRequired(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "The certificate server signals that new certificate requests needs to be authorized by an admin of the Peers@Play Project.", "Certificate authorization required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnInvalidCertificateRegistration(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                MessageBoxIcon icon = MessageBoxIcon.Error;
                switch (e.Type)
                {
                    case ErrorType.AdditionalFieldsIncorrect:
                        this.certRegisterAvatarTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = "The registration request was denied because the additional fields were invalid." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.AvatarAlreadyExists:
                        this.certRegisterAvatarTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = "The avatar has already been used for registration. Please choose another one." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.AvatarFormatIncorrect:
                        this.certRegisterAvatarTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The avatar format is incorrect, but the server didn't provide a reason.";
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.EmailAlreadyExists:
                        this.certRegisterEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = "The email address has already been used for registration. Please choose another one." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.EmailFormatIncorrect:
                        this.certRegisterEmailTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The email address format is incorrect, but the server didn't provide a reason.";
                        break;
                    case ErrorType.PasswordFormatIncorrect:
                        this.certRegisterPasswordLabel.BackColor = INVALID_INPUT_BG_COLOR;
                        this.certRegisterConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The password format is incorrect, but the server didn't provide a reason.";
                        break;
                    case ErrorType.SmtpServerDown:
                        msg = "Your registration has been successfully processed, but the server could not send an email verification code because the corresponding SMTP server is down. You will receive the email with some delay." + ((e.Message != null) ? "\n" + e.Message : "");
                        icon = MessageBoxIcon.Information;
                        break;
                    case ErrorType.WorldFormatIncorrect:
                        this.certRegisterWorldTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The world format is incorrect, but the server didn't provide a reason.";
                        break;
                    default:
                        msg = "The server signaled an invalid certificate registration, but the error type could not be read";
                        break;
                }
                MessageBox.Show(this, msg, "Certificate Registration", MessageBoxButtons.OK, icon);
            });
        }

        private void OnInvalidEmailVerification(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                switch (e.Type)
                {
                    case ErrorType.AlreadyVerified:
                        msg = "The email address has already been verfied. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.NoCertificateFound:
                        msg = "There is no password reset for the entered verification code open." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    default:
                        msg = "The server signaled an invalid email verification, but the error type could not be read";
                        break;
                }

                MessageBox.Show(this, msg, "Email Verification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnInvalidCertificateRequest(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                MessageBoxIcon icon = MessageBoxIcon.Error;
                switch (e.Type)
                {
                    case ErrorType.CertificateNotYetAuthorized:
                        msg = "The delivery of your certificate was not authorized by an admin yet. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.CertificateRevoked:
                        msg = "The requested certificate has been revoked. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.EmailNotYetVerified:
                        msg = "You need to validate your email address to get your certificate. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.NoCertificateFound:
                        msg = "No certificate for this avatar/email found. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.SmtpServerDown:
                        msg = "You need to validate your email address, but the server could not send an email verification code because the corresponding SMTP server is down. You will receive the email with some delay." + ((e.Message != null) ? "\n" + e.Message : "");
                        icon = MessageBoxIcon.Information;
                        break;
                    case ErrorType.WrongPassword:
                        this.certRequestPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = "The password you have entered is invalid. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    default:
                        msg = "The server signaled an invalid certificate request, but the error type could not be read";
                        break;
                }
                MessageBox.Show(this, msg, "Certificate Request", MessageBoxButtons.OK, icon);
            });
        }

        private void OnInvalidPasswordReset(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                MessageBoxIcon icon = MessageBoxIcon.Error;
                switch (e.Type)
                {
                    case ErrorType.CertificateNotYetAuthorized:
                        msg = "The modification of your certificate was not authorized by an admin yet. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.CertificateRevoked:
                        msg = "The requested certificate has been revoked. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.NoCertificateFound:
                        msg = "No certificate for this avatar/email found. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.SmtpServerDown:
                        msg = "You need to validate your authenticity to reset your password, but the server could not send a verification code because the corresponding SMTP server is down. You will receive the email with some delay." + ((e.Message != null) ? "\n" + e.Message : "");
                        icon = MessageBoxIcon.Information;
                        break;
                    default:
                        msg = "The server signaled an invalid password reset, but the error type could not be read";
                        break;
                }

                MessageBox.Show(this, msg, "Password Reset", MessageBoxButtons.OK, icon);
            });
        }

        private void OnInvalidPasswordResetVerification(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                switch (e.Type)
                {
                    case ErrorType.AlreadyVerified:
                        msg = "The password reset has already been verfied. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.CertificateNotYetAuthorized:
                        msg = "Your certificate has not been authorized by an admin yet. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.CertificateRevoked:
                        msg = "The certificate has been revoked." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.NoCertificateFound:
                        msg = "There is no password reset for the entered verification code open." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.PasswordFormatIncorrect:
                        this.passwordResetVerificationPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        this.passwordResetVerificationConfirmTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The password format is incorrect, but the server didn't provide a reason.";
                        break;
                    default:
                        msg = "The server signaled an invalid password reset, but the error type could not be read";
                        break;
                }

                MessageBox.Show(this, msg, "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnInvalidPasswordChange(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                switch (e.Type)
                {
                    case ErrorType.CertificateNotYetAuthorized:
                        msg = "Your certificate has not been authorized by an admin yet. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.CertificateRevoked:
                        msg = "The certificate has been revoked." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.DeserializationFailed:
                        msg = "The server received a malformed packet. Please resend your request. If this error continues to appear, check for a newer program version." + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.NoCertificateFound:
                        msg = "No certificate for this avatar/email found. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    case ErrorType.PasswordFormatIncorrect:
                        this.passwordChangeNewPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        this.passwordChangeConfirmPasswordTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = (e.Message != null) ? e.Message : "The password format is incorrect, but the server didn't provide a reason.";
                        break;
                    case ErrorType.WrongPassword:
                        this.passwordResetVerificationCodeTextBox.BackColor = INVALID_INPUT_BG_COLOR;
                        msg = "The old password you have entered is invalid. " + ((e.Message != null) ? "\n" + e.Message : "");
                        break;
                    default:
                        msg = "The server signaled an invalid password change, but the error type could not be read";
                        break;
                }

                MessageBox.Show(this, msg, "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnSslCertificateRefused(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.statusLabel.Text = "Servers SSL certificate has been refused!";
                MessageBox.Show(this, "Servers SSL certificate has been refused!", "SSL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnProxyErrorOccured(object sender, ProxyEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.statusLabel.Text = e.Message;
                MessageBox.Show(this, e.Message, "Proxy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnHttpTunnelEstablished(object sender, ProxyEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.statusLabel.Text = e.Message;
            });
        }

        private void OnNoProxyConfigured(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "No proxy configuration has been found.", "No Proxy Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnNewProtocolVersion(object sender, EventArgs e)
        {
            this.Invoke((Action)delegate
            {
                MessageBox.Show(this, "A new certificate client is available. Please update first, before you request a new certificate.", "New certificate client available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnServerErrorOccurred(object sender, ProcessingErrorEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                string msg = null;
                switch (e.Type)
                {
                    case ErrorType.Unknown:
                        msg = "The server signals an error while processing your request." + ((e.Message != null) ? " The server signals:\n\n" + e.Message : "");
                        break;
                    default:
                        msg = "The server signals an error while processing your request. Please try again later.";
                        break;
                }
                MessageBox.Show(this, msg, "Server reported an error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            int port = 0;
            if (!Int32.TryParse(this.configServerPort.Text.Trim(), out port))
            {
                port = 443;
            }
            global::CrypTool.CertificateClientSimpleGUI.Properties.Settings.Default.PORT = port;
            global::CrypTool.CertificateClientSimpleGUI.Properties.Settings.Default.SERVER = this.configServerAddress.Text;
            global::CrypTool.CertificateClientSimpleGUI.Properties.Settings.Default.Save();
        }

    }
}
