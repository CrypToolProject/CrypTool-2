using System;
using System.Windows.Input;

namespace LatticeCrypto.Utilities
{
    public class RelayCommand : ICommand
    {
        //MVVM
        //http://msdn.microsoft.com/de-de/magazine/dd419663.aspx

        //execute: zeigt auf eine Methode, die ein Objekt als Parameter hat ohne Rückgabewert.
        private readonly Action<object> execute;

        //canExecute: zeigt auf eine Methode, die ein Objekt als Parameter hat mit bool als Rückgabewert.
        private readonly Predicate<object> canExecute;


        //1. Konstruktor wird benutzt, falls ein Kommand immer aufrufbar ist. z.B. AddCommand
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        //2. Konstruktor wird benutzt, falls man mit der Ausführbarkeit eines Kommands kontrollieren möchte. z.B. Löschen nur wenn was ausgwählt wurde.
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }


        #region ICommand Members

        //Darf die Execute-Methode ausgeführt werden?
        public bool CanExecute(object parameter)
        {
            //Gibt es keinen canExecute-Zeiger, ist der Kommand immer ausführbar. Die Enscheidung ist true.
            //Gibt es einen canExecute-Zeiger, liegt die Entscheidung bei der gelieferten Methode.
            return canExecute == null || canExecute(parameter);
        }

        //Falls dieses Event ausgelöst wird, wird die CanExecute-Methode ausgeführt.
        public event EventHandler CanExecuteChanged;

        //Was der Kommand ausführt.
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion

        //Löse das Ereignis "CanExecuteChanged" auf. 
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
