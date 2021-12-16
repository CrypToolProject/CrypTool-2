using System.Diagnostics;
using System.Timers;

namespace CrypTool.CrypWin.Helper
{
    public class PriorityChangedListener
    {
        private static ProcessPriorityClass lastPriorityClass;
        public delegate void PriorityChangedDelegate(ProcessPriorityClass newPriority);

        public static event PriorityChangedDelegate PriorityChanged;

        static PriorityChangedListener()
        {
            lastPriorityClass = Process.GetCurrentProcess().PriorityClass;

            System.Timers.Timer timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Process.GetCurrentProcess().PriorityClass != lastPriorityClass)
            {
                lastPriorityClass = Process.GetCurrentProcess().PriorityClass;
                if (PriorityChanged != null)
                {
                    PriorityChanged(lastPriorityClass);
                }
            }
        }

    }
}
