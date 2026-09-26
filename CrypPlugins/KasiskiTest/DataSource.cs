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
using System.Collections.ObjectModel;


namespace CrypTool.KasiskiTest
{
    public class DataSource
    {


        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get => valueCollection;
            set => valueCollection = value;
        }


        public DataSource()
        {

            valueCollection = new ObservableCollection<CollectionElement>();
            //  CollectionElement z= new CollectionElement(30,30,30);
            // valueCollection.Add(z);
            // CollectionElement y = new CollectionElement(30, 40, 50);
            // CollectionElement s = new CollectionElement(30, 200, 180);
            // valueCollection.Add(s);
            // valueCollection.Add(y);
            // CollectionElement q = new CollectionElement(30, 30, 150);
            // valueCollection.Add(q);

        }
    }
}
