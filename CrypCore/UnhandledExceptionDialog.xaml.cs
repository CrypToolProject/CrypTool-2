/*
   Copyright 2008 - 2022 CrypTool Team

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
using System;
using System.Windows;

namespace CrypTool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class UnhandledExceptionDialog : Window
    {
        private readonly Exception _exception;
        private readonly Version _version;
        private readonly string _installationType;
        private readonly string _buildType;
        private readonly string _productName;

        public UnhandledExceptionDialog(Exception exception, Version version, string installationType, string buildType, string productName)
        {
            _exception = exception;
            _version = version;
            _installationType = installationType;
            _buildType = buildType;
            _productName = productName;
            InitializeComponent();
            ExceptionNameLabel.Content = exception.GetType().FullName;
            ExceptionMessageLabel.Text = exception.Message;
            StackTraceBox.Text = exception.StackTrace;
        }

        private void ReportButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ReportErrorDialog reportErrorDialog = new ReportErrorDialog(_exception, _version, _installationType, _buildType, _productName);
            reportErrorDialog.ShowDialog();
            Close(); // auto-close this window after ReportErrorDialog has been closed
        }

        private void CloseButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        public static void ShowModalDialog(Exception exception, Version version, string installationType, string buildType, string productName)
        {
            UnhandledExceptionDialog unhandledExceptionDialog = new UnhandledExceptionDialog(exception, version, installationType, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }
    }
}
