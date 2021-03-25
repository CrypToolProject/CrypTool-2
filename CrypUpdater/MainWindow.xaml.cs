using System.Windows;

namespace CrypUpdater
{
    public partial class MainWindow : Window
    {
        private bool isFinished = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void CloseOnFinish()
        {
            isFinished = true;
            Dispatcher.Invoke(() => Close());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !isFinished;
        }

    }
}
