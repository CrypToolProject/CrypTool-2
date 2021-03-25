using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkspaceManager.View.Visuals;
using WorkspaceManager.View.Base;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for ZoomScrollViewer.xaml
    /// </summary>
    public partial class ZoomScrollViewer : ScrollViewer
    {
        private double min = CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale,
                       max = CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale;

        private const double delta = 0.15;

        public ZoomScrollViewer()
        {
            InitializeComponent();
        }

        private void IncZoom(object sender, RoutedEventArgs e)
        {
            EditorVisual editor = (EditorVisual)Util.TryFindParent<EditorVisual>(this);
            editor.ZoomLevel = Math.Min(editor.ZoomLevel + delta, max);
        }

        private void DecZoom(object sender, RoutedEventArgs e)
        {
            EditorVisual editor = (EditorVisual)Util.TryFindParent<EditorVisual>(this);
            editor.ZoomLevel = Math.Max(editor.ZoomLevel - delta, min);
        }

        private void TextBlockMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EditorVisual editor = (EditorVisual)Util.TryFindParent<EditorVisual>(this);
            editor.ZoomLevel = 1.0;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}