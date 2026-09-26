/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace CrypTool.Alphabets
{
    public class AlphabetSettings : ISettings
    {
        public AlphabetSettings()
        {
            data = SerializeData<Data>(Default);
        }

        public Data Default = new Data()
        {
            OutputOrderData = new List<OutputOrder>(new OutputOrder[]
            {
                new OutputOrder() { Caption = Properties.Resources.Upper, OutputType = OutputType.Upper },
                new OutputOrder() { Caption = Properties.Resources.Lower, OutputType = OutputType.Lower },
                new OutputOrder() { Caption = Properties.Resources.Numeric, OutputType = OutputType.Numeric },
                new OutputOrder() { Caption = Properties.Resources.Special, OutputType = OutputType.Special }
            }),

            AlphabetData = new List<AlphabetItemData>(CreateAlphabetItems())
        };

        private static List<AlphabetItemData> CreateAlphabetItems()
        {
            List<AlphabetItemData> alphabetItemDatas = new List<AlphabetItemData>();
            foreach(BasicAlphabet basicAlphabet in BasicAlphabet.BasicAlphabets)
            {
                alphabetItemDatas.Add(new AlphabetItem(basicAlphabet, basicAlphabet.Name, true).Data);
            }            
            return alphabetItemDatas;
        }

        private string data;
        public string Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }

        public static string SerializeData<T>(T items)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, items);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        public static T DeserializeData<T>(string serItems)
        {
            if (string.IsNullOrWhiteSpace(serItems))
            {
                return default(T);
            }

            try
            {
                byte[] buffer = Convert.FromBase64String(serItems);
                using (MemoryStream stream = new MemoryStream(buffer, false))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (FormatException)
            {
                return default(T);
            }
            catch (InvalidOperationException)
            {
                return default(T);
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
