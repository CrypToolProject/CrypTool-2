using System;

namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// Internal class of the state view model, handling the ui logic of the start view.
    /// </summary>
    public class StartViewModel : IViewModelBase
    {
        /// <summary>
        /// Name of the start view model.
        /// </summary>
        private readonly string name;
        /// <summary>
        /// Constructor for the start view model.
        /// </summary>
        public StartViewModel()
        {
            name = "{Loc Start}";
        }
        /// <summary>
        /// Empty constructer class for drawing on the canvas. The canvas is handled by the start
        /// xaml.
        /// </summary>
        public void DrawCanvas()
        {
        }

        /// <summary>
        /// Getter for the name of the start view model.
        /// </summary>
        /// <returns>Returns the name of the start view model as an string.</returns>
        public string GetName()
        {
            return name;
        }
        public new void ChangeButton(Boolean ru)
        {
        }
        public void CreateHeatMapViewText(int a)
        {
        }
    }
}