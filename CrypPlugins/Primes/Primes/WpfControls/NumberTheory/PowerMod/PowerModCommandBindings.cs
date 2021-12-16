using System.Windows.Input;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    public partial class PowerModControl
    {
        private void initBindings()
        {
            CommandBindings.Add(
              new CommandBinding(
                PowerModCommands.ReOrderPointsCommand,
                new ExecutedRoutedEventHandler(ReOrderPoints),
                new CanExecuteRoutedEventHandler(CanReOrderPoints)));
        }
    }
}
