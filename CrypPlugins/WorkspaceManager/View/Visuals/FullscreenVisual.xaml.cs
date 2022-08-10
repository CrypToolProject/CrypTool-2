/*                              
   Copyright 2010-2022 Nils Kopal, Viktor M.

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinFullscreenVisual.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class FullscreenVisual : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Close;
        public event EventHandler Open;
        #endregion

        #region Properties

        public UIElement ActivePresentation
        {
            get
            {
                if (ActiveComponent == null)
                {
                    return null;
                }

                return ActiveComponent.GetPresentationElement(ActiveComponent.FullScreenState);
            }
        }



        public bool HasComponentPresentation
        {
            get
            {
                if (ActiveComponent == null)
                {
                    return false;
                }

                return ActiveComponent.IsPresentationElementAvailable(BinComponentState.Presentation);
            }
        }

        public bool HasComponentSetting
        {
            get
            {
                if (ActiveComponent == null)
                {
                    return false;
                }

                return ActiveComponent.IsPresentationElementAvailable(BinComponentState.Setting);
            }
        }

        private ComponentVisual lastActiveComponent;
        public ComponentVisual LastActiveComponent
        {
            set => lastActiveComponent = value;
            get => lastActiveComponent;
        }

        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty ComponentCollectionProperty = DependencyProperty.Register("ComponentCollection",
            typeof(ObservableCollection<ComponentVisual>), typeof(FullscreenVisual), new FrameworkPropertyMetadata(null, null));

        public ObservableCollection<ComponentVisual> ComponentCollection
        {
            get => (ObservableCollection<ComponentVisual>)base.GetValue(ComponentCollectionProperty);
            set => base.SetValue(ComponentCollectionProperty, value);
        }

        public static readonly DependencyProperty ActiveComponentProperty = DependencyProperty.Register("ActiveComponent",
            typeof(ComponentVisual), typeof(FullscreenVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnActiveComponentChanged)));

        public ComponentVisual ActiveComponent
        {
            get => (ComponentVisual)base.GetValue(ActiveComponentProperty);
            set => base.SetValue(ActiveComponentProperty, value);
        }

        public static readonly DependencyProperty IsFullscreenOpenProperty = DependencyProperty.Register("IsFullscreenOpen",
            typeof(bool), typeof(FullscreenVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFullscreenOpenChanged)));


        public bool IsFullscreenOpen
        {
            get => (bool)base.GetValue(IsFullscreenOpenProperty);
            set => base.SetValue(IsFullscreenOpenProperty, value);
        }
        #endregion

        #region Constructors
        public FullscreenVisual()
        {
            InitializeComponent();
        }
        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region EventHandler

        private void CloseClickHandler(object sender, RoutedEventArgs e)
        {
            IsFullscreenOpen = false;
            if (Close != null)
            {
                Close.Invoke(this, new EventArgs());
            }
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
            {
                return;
            }

            if (b.Content is BinComponentState && ActiveComponent != null)
            {
                ActiveComponent.FullScreenState = (BinComponentState)b.Content;
                OnPropertyChanged("ActivePresentation");
                return;
            }

            e.Handled = true;
        }

        private static void OnActiveComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FullscreenVisual f = (FullscreenVisual)d;
            ComponentVisual newBin = (ComponentVisual)e.NewValue;
            ComponentVisual oldBin = (ComponentVisual)e.OldValue;
            if (newBin != null)
            {
                newBin.IsActive = true;
                newBin.IsFullscreen = true;
            }

            if (oldBin != null)
            {
                f.LastActiveComponent = oldBin;
                if (oldBin != newBin)
                {
                    oldBin.IsFullscreen = false;
                    oldBin.IsActive = false;
                }
            }

            f.OnPropertyChanged("HasComponentPresentation");
            f.OnPropertyChanged("HasComponentSetting");
            f.OnPropertyChanged("ActivePresentation");
        }

        private static void OnIsFullscreenOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FullscreenVisual f = (FullscreenVisual)d;
            if ((bool)e.NewValue)
            {
                if (f.LastActiveComponent != null)
                {
                    f.LastActiveComponent.IsFullscreen = true;
                }
            }
            else
            {
                if (f.LastActiveComponent != null)
                {
                    f.LastActiveComponent.IsFullscreen = false;
                }

                f.ActiveComponent = null;
            }

            if (f.Open != null)
            {
                f.Open.Invoke(f, new EventArgs());
            }

            f.OnPropertyChanged("ActivePresentation");
        }
        #endregion
    }
}
