/*
   Copyright (C) CrypTool 2 Team

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
using System.Resources;

namespace Primes.OnlineHelp
{
    public static class OnlineHelpAccess
    {
        private static WindowOnlineHelp wndOnlineHelp;

        static OnlineHelpAccess()
        {
        }

        public static void ShowOnlineHelp(OnlineHelpActions action)
        {
            WindowOnlineHelp.NavigateTo(action.ToString());
        }

        private static ResourceManager m_HelpResourceManager;

        public static ResourceManager HelpResourceManager
        {
            get
            {
                if (m_HelpResourceManager == null)
                {
                    m_HelpResourceManager =
                      new ResourceManager("Primes.OnlineHelp.HelpFiles.Help", typeof(OnlineHelpAccess).Assembly);
                }

                return m_HelpResourceManager;
            }
        }

        public static void HelpWindowClosed()
        {
            if (wndOnlineHelp != null)
            {
                wndOnlineHelp.Close();
            }

            wndOnlineHelp = null;
        }

        public static bool HelpWindowIsActive => (wndOnlineHelp != null);

        public static void Activate()
        {
            if (wndOnlineHelp != null)
            {
                wndOnlineHelp.Activate();
            }
        }

        private static WindowOnlineHelp WindowOnlineHelp
        {
            get
            {
                if (wndOnlineHelp == null)
                {
                    wndOnlineHelp = new WindowOnlineHelp();
                }

                wndOnlineHelp.OnClose += HelpWindowClosed;
                return wndOnlineHelp;
            }
        }
    }
}
