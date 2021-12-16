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
