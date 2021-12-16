using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkspaceManager.View.Base;
using WorkspaceManager.View.Visuals;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for ZoomScrollViewer.xaml
    /// </summary>
    public partial class ZoomScrollViewer : ScrollViewer
    {
        private readonly double min = CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale,
                       max = CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale;

        private const double delta = 0.15;

        public ZoomScrollViewer()
        {
            InitializeComponent();
        }

        private void IncZoom(object sender, RoutedEventArgs e)
        {
            EditorVisual editor = (this).TryFindParent<EditorVisual>();
            editor.ZoomLevel = Math.Min(editor.ZoomLevel + delta, max);
        }

        private void DecZoom(object sender, RoutedEventArgs e)
        {
            EditorVisual editor = (this).TryFindParent<EditorVisual>();
            editor.ZoomLevel = Math.Max(editor.ZoomLevel - delta, min);
        }

        private void TextBlockMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditorVisual editor = (this).TryFindParent<EditorVisual>();
            editor.ZoomLevel = 1.0;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}