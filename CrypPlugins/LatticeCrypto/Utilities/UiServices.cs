using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LatticeCrypto.Utilities
{
    /// <summary>
    ///   Contains helper methods for UI, so far just one for showing a waitcursor
    /// </summary>
    public static class UiServices
    {

        /// <summary>
        ///   A value indicating whether the UI is currently busy
        /// </summary>
        private static bool IsBusy;

        /// <summary>
        /// Sets the busystate as busy.
        /// </summary>
        public static void SetBusyState()
        {
            SetBusyState(true);
        }

        /// <summary>
        /// Sets the busystate to busy or not busy.
        /// </summary>
        /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
        private static void SetBusyState(bool busy)
        {
            if (busy == IsBusy)
            {
                return;
            }

            IsBusy = busy;
            Mouse.OverrideCursor = busy ? Cursors.Wait : null;

            if (IsBusy)
            {
                new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, dispatcherTimer_Tick, Application.Current.Dispatcher);
            }
        }

        /// <summary>
        /// Handles the Tick event of the dispatcherTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = sender as DispatcherTimer;
            if (dispatcherTimer == null)
            {
                return;
            }

            SetBusyState(false);
            dispatcherTimer.Stop();
        }
    }
}
