using System;
using System.Windows;

namespace CrypTool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class UnhandledExceptionDialog : Window
    {
        private readonly Exception _e;
        private readonly Version _version;
        private readonly string _installationType;
        private readonly string _buildType;
        private readonly string _productName;

        public UnhandledExceptionDialog(Exception e, Version version, string installationType, string buildType, string productName)
        {
            _e = e;
            _version = version;
            _installationType = installationType;
            _buildType = buildType;
            _productName = productName;
            InitializeComponent();
            ExceptionNameLabel.Content = e.GetType().FullName;
            ExceptionMessageLabel.Text = e.Message;
            StackTraceBox.Text = e.StackTrace;
        }

        private void ReportButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ReportErrorDialog reportErrorDialog = new ReportErrorDialog(_e, _version, _installationType, _buildType, _productName);
            reportErrorDialog.ShowDialog();
            Close(); // auto-close this window after ReportErrorDialog has been closed
        }

        private void CloseButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        public static void ShowModalDialog(Exception e, Version version, string installationType, string buildType, string productName)
        {
            UnhandledExceptionDialog unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, installationType, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }
    }
}
