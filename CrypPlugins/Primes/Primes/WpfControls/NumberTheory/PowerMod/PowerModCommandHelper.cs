using System.Windows.Input;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    public partial class PowerModControl
    {
        private void ReOrderPoints(object sender, ExecutedRoutedEventArgs args)
        {
            m_SortAsc = args.OriginalSource == rbOne;
            CreatePoints();
        }

        private void CanReOrderPoints(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }
    }
}