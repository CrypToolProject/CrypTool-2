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
using System.Windows;

namespace CrypTool.CrypWin.Helper
{
    public static class MessageBoxHelper
    {
        public static void Information(string text)
        {
            Information(text, Properties.Resources.Information);
        }
        public static void Information(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool Question(string text)
        {
            return Question(text, Properties.Resources.Question);
        }
        public static bool Question(string text, string caption)
        {
            MessageBoxResult res =
              MessageBox.Show(text, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static MessageBoxResult SaveChanges(string projectFilename)
        {
            if (projectFilename != "" && projectFilename != null)
            {
                string filename = System.IO.Path.GetFileName(projectFilename);

                return MessageBox.Show(string.Format(Properties.Resources.Do_you_want_to_save_the_changes_you_made_in_the_project, filename), Properties.Resources.Unsaved_changes, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }
            else
            {
                return MessageBox.Show(Properties.Resources.Do_you_want_to_save_your_new_project, Properties.Resources.Unsaved_changes, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }
        }

    }
}
