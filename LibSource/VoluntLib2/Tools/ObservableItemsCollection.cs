/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace VoluntLib2.Tools
{
    /// <summary>
    /// Extended Version of ObservableCollection which also observes properties of it items
    /// </summary>
    /// <typeparam name="type"></typeparam>
    public class ObservableItemsCollection<type> : ObservableCollection<type> where type : INotifyPropertyChanged
    {
        /// <summary>
        /// Called, when items are added, replaced, or removed
        /// </summary>
        /// <param name="notifyCollectionChangedEventArgs"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                Register(notifyCollectionChangedEventArgs.NewItems);
            }
            else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Replace)
            {
                Unregister(notifyCollectionChangedEventArgs.OldItems);
                Register(notifyCollectionChangedEventArgs.NewItems);
            }
            else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                Unregister(notifyCollectionChangedEventArgs.OldItems);
            }
            base.OnCollectionChanged(notifyCollectionChangedEventArgs);
        }

        /// <summary>
        /// An item called PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// List is cleared; remove all events
        /// </summary>
        protected override void ClearItems()
        {
            Unregister(this);
            base.ClearItems();
        }

        /// <summary>
        /// Register item events
        /// </summary>
        /// <param name="items"></param>
        private void Register(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(ItemPropertyChanged);
                }
            }
        }

        /// <summary>
        /// Unregister item events
        /// </summary>
        /// <param name="items"></param>
        private void Unregister(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= new PropertyChangedEventHandler(ItemPropertyChanged);
                }
            }
        }
    }
}
