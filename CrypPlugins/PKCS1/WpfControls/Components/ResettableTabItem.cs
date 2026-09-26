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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace PKCS1.WpfControls.Components
{
    [ContentProperty(nameof(TabContentTemplate))]
    public class ResettableTabItem : TabItem
    {
        public delegate void TabContentChangedHandler(object content);
        public event TabContentChangedHandler OnTabContentChanged;

        public DataTemplate TabContentTemplate { get; set; }

        public ICommand Reset { get; }

        public ResettableTabItem()
        {
            Reset = new ResetCommand(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Content == null)
            {
                //Initial creation
                CreateTabContent();
            }
        }

        private class ResetCommand : ICommand
        {
            private readonly ResettableTabItem resettableTabItem;

            public ResetCommand(ResettableTabItem resettableTabItem)
            {
                this.resettableTabItem = resettableTabItem;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return resettableTabItem.TabContentTemplate != null;
            }

            public void Execute(object parameter)
            {
                resettableTabItem.CreateTabContent();
            }
        }

        private void CreateTabContent()
        {
            if (TabContentTemplate != null)
            {
                Content = TabContentTemplate.LoadContent();
                OnTabContentChanged?.Invoke(Content);
            }
        }
    }
}
