/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase.Attributes;
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Localization("CrypTool.Plugins.DECRYPTTools.Properties.Resources")]
    [SettingsTab("DECRYPTSettingsTab", "/MainSettings/")]
    public partial class DECRYPTSettingsTab : UserControl
    {
        private static readonly RNGCryptoServiceProvider _rNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsStyle"></param>
        public DECRYPTSettingsTab(Style settingsStyle)
        {
            InitializeComponent();
            Resources.Add("settingsStyle", settingsStyle);
            UsernameTextbox.Text = Properties.Settings.Default.Username;
            if (Properties.Settings.Default.Password != null)
            {
                try
                {
                    if(!string.IsNullOrEmpty(Properties.Settings.Default.Password) && 
                       !string.IsNullOrEmpty(Properties.Settings.Default.PasswordIV))
                    {
                        byte[] iv = Convert.FromBase64String(Properties.Settings.Default.PasswordIV);
                        PasswordTextbox.Password = UTF8Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Properties.Settings.Default.Password), iv, DataProtectionScope.CurrentUser));
                    }                                        
                }
                catch (Exception)
                {
                    //An exception occured during restore of password
                }
            }
            InitColorPalette();
        }

        /// <summary>
        /// Initializes the color palette by loading it from the settings
        /// </summary>
        private void InitColorPalette()
        {
            TagElementColor.SelectedColor = DrawingToMedia(Properties.Settings.Default.TagElementColor);
            NullElementColor.SelectedColor = DrawingToMedia(Properties.Settings.Default.NullElementColor);
            RegularElementColor.SelectedColor = DrawingToMedia(Properties.Settings.Default.RegularElementColor);
            NomenclatureElementColor.SelectedColor = DrawingToMedia(Properties.Settings.Default.NomenclatureElementColor);
            CommentColor.SelectedColor = DrawingToMedia(Properties.Settings.Default.CommentColor);
        }

        /// <summary>
        /// Called every time the username textbox has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsernameTextbox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Username = UsernameTextbox.Text;
            Properties.Settings.Default.Save();
            JsonDownloaderAndConverter.LogOut();
        }

        /// <summary>
        /// Called every time the password textbox has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordTextbox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            byte[] iv = new byte[16];
            _rNGCryptoServiceProvider.GetBytes(iv);
            Properties.Settings.Default.PasswordIV = Convert.ToBase64String(iv);
            Properties.Settings.Default.Password = Convert.ToBase64String(ProtectedData.Protect(UTF8Encoding.UTF8.GetBytes(PasswordTextbox.Password), iv, DataProtectionScope.CurrentUser));
            Properties.Settings.Default.Save();
            JsonDownloaderAndConverter.LogOut();
        }

        /// <summary>
        /// Returns the username of the DECRYPT user
        /// </summary>
        /// <returns></returns>
        internal static string GetUsername()
        {
            return Properties.Settings.Default.Username;
        }

        /// <summary>
        /// Returns the password of the DECRYPT user
        /// </summary>
        /// <returns></returns>
        internal static string GetPassword()
        {
            byte[] iv = Convert.FromBase64String(Properties.Settings.Default.PasswordIV);
            return UTF8Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Properties.Settings.Default.Password), iv, DataProtectionScope.CurrentUser));

        }

        /// <summary>
        /// Method to test login data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestLoginDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool loginOk = JsonDownloaderAndConverter.Login(GetUsername(), GetPassword());
                if (loginOk)
                {
                    MessageBox.Show(Properties.Resources.CredentialsOK, Properties.Resources.CredentialsOKTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Properties.Resources.CredentialsWrong, Properties.Resources.CredentialsWrongTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Called, when the user changed a color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (sender == TagElementColor)
            {
                Properties.Settings.Default.TagElementColor = MediaToDrawing(e.NewValue);
                Properties.Settings.Default.Save();
            }
            else if (sender == NullElementColor)
            {
                Properties.Settings.Default.NullElementColor = MediaToDrawing(e.NewValue);
                Properties.Settings.Default.Save();
            }
            else if (sender == RegularElementColor)
            {
                Properties.Settings.Default.RegularElementColor = MediaToDrawing(e.NewValue);
                Properties.Settings.Default.Save();
            }
            else if (sender == NomenclatureElementColor)
            {
                Properties.Settings.Default.NomenclatureElementColor = MediaToDrawing(e.NewValue);
                Properties.Settings.Default.Save();
            }
            else if (sender == CommentColor)
            {
                Properties.Settings.Default.CommentColor = MediaToDrawing(e.NewValue);
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Converts Color from System.Drawing.Color to System.Windows.Media.Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color DrawingToMedia(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts Color from System.Windows.Media.Color to System.Drawing.Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Drawing.Color MediaToDrawing(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Resets the color to default values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetColorButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TagElementColor = System.Drawing.Color.Black;
            Properties.Settings.Default.NullElementColor = System.Drawing.Color.Gray;
            Properties.Settings.Default.RegularElementColor = System.Drawing.Color.DarkBlue;
            Properties.Settings.Default.NomenclatureElementColor = System.Drawing.Color.DarkGreen;
            Properties.Settings.Default.CommentColor = System.Drawing.Color.Black;
            Properties.Settings.Default.Save();
            InitColorPalette();
        }
    }
}
