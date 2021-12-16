namespace CrypCloud.Manager.Services
{
    internal class WorkspaceHelper
    {
        private const string FileDialogExtention = ".cwm";
        private const string FileDialogFilter = "Workspace (.cwm)|*.cwm";

        public static string OpenFilePickerAndReturnPath()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = FileDialogExtention, Filter = FileDialogFilter };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }
            return "";
        }

    }
}
