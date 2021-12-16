using System.Resources;

namespace LatticeCrypto.OnlineHelp
{
    public static class OnlineHelpAccess
    {
        private static WindowOnlineHelp wndOnlineHelp;

        public static void ShowOnlineHelp(OnlineHelpActions action)
        {
            WindowOnlineHelp.NavigateTo(action.ToString());
        }

        private static ResourceManager m_HelpResourceManager;
        public static ResourceManager HelpResourceManager => m_HelpResourceManager ?? (m_HelpResourceManager = new ResourceManager("LatticeCrypto.Properties.HelpLanguages", typeof(OnlineHelpAccess).Assembly));
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
