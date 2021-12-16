using System;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class ApplicationSettingsHelper
    {
        /// <summary>
        /// Saves the Application settings. If save fails it tries to save again. If
        /// 2. try fails too app settings wont be saved
        /// </summary>
        public static void SaveApplicationsSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                //if saving failed try one more time
                try
                {
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                    //if saving failed again we do not try it again
                }
            }
        }
    }
}
