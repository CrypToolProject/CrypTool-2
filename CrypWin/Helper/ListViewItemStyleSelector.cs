/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Windows;
using System.Windows.Controls;

namespace CrypWin.Helper
{
    public class ListViewItemStyleSelector : StyleSelector
    {
        private int i = 0;
        public override Style SelectStyle(object item, DependencyObject container)
        {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(container);
            int test = ic.Items.IndexOf(item);

            string styleKey;
            if (test % 2 == 0)
            {
                styleKey = "ListViewItemStyle1";
            }
            else
            {
                styleKey = "ListViewItemStyle2";
            }

            i++;
            return (Style)(ic.FindResource(styleKey));
        }
    }
}
