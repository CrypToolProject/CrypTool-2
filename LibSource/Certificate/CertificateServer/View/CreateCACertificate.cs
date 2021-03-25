using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrypTool.CertificateLibrary.Util;
using System.Globalization;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Network;

namespace CrypTool.CertificateServer.View
{
    public partial class CreateCACertificate : UserControl
    {

        #region Static readonly

        private static readonly Color invalidInputBackgroundColor = Color.MistyRose;

        #endregion


        #region Private members

        private CertificateServer server = null;

        private CertificationAuthority certificationAuthority = null;

        #endregion


        #region Constructor

        public CreateCACertificate()
        {
            InitializeComponent();
            FillCountryComboBox();
        }

        #endregion


        #region Setter

        public void SetServer(CertificateServer server)
        {
            this.server = server;
        }

        public void SetCertificationAuthority(CertificationAuthority ca)
        {
            this.certificationAuthority = ca;
        }

        #endregion


        #region Create certificate Event-Handler

        private void createCertButton_Click(object sender, EventArgs e)
        {
            nameTextBox.BackColor = Color.White;
            organisationTextBox.BackColor = Color.White;
            organisationalUnitTextBox.BackColor = Color.White;
            emailTextBox.BackColor = Color.White;
            passwordTextBox.BackColor = Color.White;
            confirmPasswordTextBox.BackColor = Color.White;

            if (!Verification.IsValidCommonName(nameTextBox.Text))
            {
                statusLabel.Text = "Please enter a valid common name to generate a CA certificate";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                nameTextBox.BackColor = invalidInputBackgroundColor;
                nameTextBox.Focus();
                return;
            }
            if (!Verification.IsValidOrganisation(organisationTextBox.Text))
            {
                statusLabel.Text = "Please enter a valid organisation to generate a CA certificate";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                organisationTextBox.BackColor = invalidInputBackgroundColor;
                organisationTextBox.Focus();
                return;
            }
            if (!Verification.IsValidOrganisationalUnit(organisationalUnitTextBox.Text))
            {
                statusLabel.Text = "Please enter a valid organisational name to generate a CA certificate";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                organisationalUnitTextBox.BackColor = invalidInputBackgroundColor;
                organisationalUnitTextBox.Focus();
                return;
            }
            if (!Verification.IsValidEmailAddress(emailTextBox.Text))
            {
                statusLabel.Text = "Please enter a valid email address to generate a CA certificate";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                emailTextBox.BackColor = invalidInputBackgroundColor;
                emailTextBox.Focus();
                return;
            }
            if (!Verification.IsValidPassword(passwordTextBox.Text))
            {
                statusLabel.Text = "Please enter a valid password to generate a CA certificate";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                passwordTextBox.BackColor = invalidInputBackgroundColor;
                confirmPasswordTextBox.BackColor = invalidInputBackgroundColor;
                passwordTextBox.Focus();
                return;
            }
            if (!passwordTextBox.Text.Equals(confirmPasswordTextBox.Text))
            {
                statusLabel.Text = "You entered two different passwords.";
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                passwordTextBox.BackColor = invalidInputBackgroundColor;
                confirmPasswordTextBox.BackColor = invalidInputBackgroundColor;
                passwordTextBox.Focus();
                return;
            }
            // Everythings ok, stop server, create CA peerCert and tidy up the textboxes
            this.createCertButton.Enabled = false;
            this.processingIcon.Show();
            server.Stop();

            Country country = this.countryComboBox.SelectedItem as Country;
            string[] input = new string[]{ this.nameTextBox.Text, this.organisationTextBox.Text, this.organisationalUnitTextBox.Text, country.Code, this.emailTextBox.Text, this.passwordTextBox.Text };

            BackgroundWorker processingWorker = new BackgroundWorker();
            processingWorker.DoWork += new DoWorkEventHandler(Generating);
            processingWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GeneratingDone);
            processingWorker.RunWorkerAsync(input);
        }

        private void Generating(object sender, DoWorkEventArgs e)
        {
            try
            {
                string[] input = (string[])e.Argument;

                // Generate new CA and TLS certificate for the Certification Authority
                bool generated= certificationAuthority.GenerateCaAndTlsCertificate(input[0], input[1], input[2], input[3], input[4], input[5]);
                if (!generated)
                {
                    this.Invoke((Action)delegate
                    {
                        string msg = "There already exists a CA certificate with these values in the database. Please load this certificate or change the values.\n\n";
                        MessageBox.Show(this, msg, "Certificate already exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }
            }
            catch (DatabaseException ex)
            {
                this.Invoke((Action)delegate
                {
                    string msg = "The certificates could not be stored in the database.\n\n";
                    MessageBox.Show(this, msg +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((Action)delegate
                {
                    string msg = "An error occured during CA certificate generation";
                    Log.Error(msg, ex);
                    MessageBox.Show(this, msg + "\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void GeneratingDone(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke((Action)delegate
            {
                this.processingIcon.Hide();
                this.createCertButton.Enabled = true;
            });
        }

        #endregion


        #region TextBox Validation Event Handler

        private void nameTextBox_Validating(object sender, CancelEventArgs e)
        {
            nameTextBox.Text = nameTextBox.Text.Trim();
            if (!Verification.IsValidCommonName(nameTextBox.Text))
            {
                nameTextBox.BackColor = invalidInputBackgroundColor;
            }
        }

        private void nameTextBox_Validated(object sender, EventArgs e)
        {
            if (Verification.IsValidCommonName(nameTextBox.Text))
            {
                statusLabel.Text = String.Empty;
                nameTextBox.BackColor = Color.White;
            }
        }

        private void organisationTextBox_Validating(object sender, CancelEventArgs e)
        {
            organisationTextBox.Text = organisationTextBox.Text.Trim();
            if (!Verification.IsValidOrganisation(organisationTextBox.Text))
            {
                organisationTextBox.BackColor = invalidInputBackgroundColor;
            }
        }

        private void organisationTextBox_Validated(object sender, EventArgs e)
        {
            if (Verification.IsValidOrganisation(organisationTextBox.Text))
            {
                statusLabel.Text = String.Empty;
                organisationTextBox.BackColor = Color.White;
            }
        }

        private void organisationalUnitTextBox_Validating(object sender, CancelEventArgs e)
        {
            organisationalUnitTextBox.Text = organisationalUnitTextBox.Text.Trim();
            if (!Verification.IsValidOrganisationalUnit(organisationalUnitTextBox.Text))
            {
                organisationalUnitTextBox.BackColor = invalidInputBackgroundColor;
            }
        }

        private void organisationalUnitTextBox_Validated(object sender, EventArgs e)
        {
            if (Verification.IsValidOrganisationalUnit(organisationalUnitTextBox.Text))
            {
                statusLabel.Text = String.Empty;
                organisationalUnitTextBox.BackColor = Color.White;
            }
        }

        private void emailTextBox_Validating(object sender, CancelEventArgs e)
        {
            emailTextBox.Text = emailTextBox.Text.Trim();
            if (!Verification.IsValidEmailAddress(emailTextBox.Text))
            {
                emailTextBox.BackColor = invalidInputBackgroundColor;
            }
        }

        private void emailTextBox_Validated(object sender, EventArgs e)
        {
            if (Verification.IsValidEmailAddress(emailTextBox.Text))
            {
                statusLabel.Text = String.Empty;
                emailTextBox.BackColor = Color.White;
            }
        }

        private void passwordTextBox_Validating(object sender, CancelEventArgs e)
        {
            passwordTextBox.Text = passwordTextBox.Text.Trim();
            confirmPasswordTextBox.Text = confirmPasswordTextBox.Text.Trim();
            if (!Verification.IsValidPassword(passwordTextBox.Text) || !passwordTextBox.Text.Equals(confirmPasswordTextBox))
            {
                passwordTextBox.BackColor = invalidInputBackgroundColor;
                confirmPasswordTextBox.BackColor = invalidInputBackgroundColor;
            }
        }

        private void passwordTextBox_Validated(object sender, EventArgs e)
        {
            if (Verification.IsValidPassword(passwordTextBox.Text) && Verification.IsValidPassword(confirmPasswordTextBox.Text) && passwordTextBox.Text.Equals(confirmPasswordTextBox.Text))
            {
                statusLabel.Text = String.Empty;
                passwordTextBox.BackColor = Color.White;
                confirmPasswordTextBox.BackColor = Color.White;            
            }
        }

        #endregion


        #region Helpers to organise this view

        /// <summary>
        /// Reset all textboxes and sets focus.
        /// Useful when a user filled some textboxes, left this view and now comes back.
        /// </summary>
        public void TidyUpOnShow()
        {
            if (this.countryComboBox.Items.Count > 0)
            {
                this.countryComboBox.SelectedIndex = 1;
            }
            this.statusLabel.Text = String.Empty;
            this.nameTextBox.BackColor = Color.White;
            this.organisationTextBox.BackColor = Color.White;
            this.organisationalUnitTextBox.BackColor = Color.White;
            this.emailTextBox.BackColor = Color.White;
            this.passwordTextBox.BackColor = Color.White;
            this.confirmPasswordTextBox.BackColor = Color.White;
            this.nameTextBox.Clear();
            this.organisationTextBox.Clear();
            this.organisationalUnitTextBox.Clear();
            this.emailTextBox.Clear();
            this.passwordTextBox.Clear();
            this.confirmPasswordTextBox.Clear();
            this.nameTextBox.Focus();
            this.createCertButton.Enabled = true;
            this.processingIcon.Hide();
        }

        private void FillCountryComboBox()
        {
            // Fill the country combobox
            foreach (Country reg in Country.getCountries())
            {
                if (reg != null)
                {
                    this.countryComboBox.Items.Add(reg);
                }
            }
            if (this.countryComboBox.Items.Count > 0)
            {
                this.countryComboBox.SelectedIndex = 0;
            }
        }

        #endregion

    }
}
