using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Primes.WpfControls.Components
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
