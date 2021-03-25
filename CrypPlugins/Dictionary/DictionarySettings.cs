/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

using CrypTool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Dictionary
{
  public class CrypToolDictionarySettings : ISettings
  {
    # region private_variables
    private int currentDictionary;
    private ObservableCollection<string> collection = new ObservableCollection<string>();
    # endregion private_variables

    public delegate void ExecuteCallback();

    [TaskPane("DictionaryCaption", "DictionaryTooltip", null, 0, true, ControlType.DynamicComboBox, new string[] { "Collection" })]
    public int Dictionary
    {
      get { return currentDictionary; }
      set
      {
        if (value != currentDictionary)
        {
            this.currentDictionary = value;
            OnPropertyChanged("Dictionary");
        }
      }
    }
      
    private string numberEntries = string.Empty;

    [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
    [TaskPane("NumberEntriesCaption", "NumberEntriesTooltip", null, 1, true, ControlType.TextBoxReadOnly)]
    public string NumberEntries
    {
        get { return numberEntries; }
        set
        {
            if (value != numberEntries)
            {
                numberEntries = value;
                OnPropertyChanged("NumberEntries");
            }
        }
    }

    // CrypWin requires this to be a collection of strings
    [DontSave]
    public ObservableCollection<string> Collection
    {
      get { return collection; }
      set
      {
        if (value != collection)
        {
          collection = value;
          OnPropertyChanged("Collection");
        }
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
      public void Initialize()
      {
          
      }

      protected void OnPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    #endregion
  
  }
}
