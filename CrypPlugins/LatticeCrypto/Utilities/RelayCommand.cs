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
