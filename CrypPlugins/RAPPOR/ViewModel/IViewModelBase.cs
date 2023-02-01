using System;

namespace CrypTool.Plugins.RAPPOR.ViewModel
{
    /// <summary>
    /// Base class for all view models.
    /// <remarks>
    /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern#viewmodelbase-class
    /// </remarks>
    /// </summary>
    public interface IViewModelBase
    {
        /// <summary>
        /// Interface method for DrawCanvas which has to be be implemented by every view model class.
        /// </summary>
        void DrawCanvas();
        //void Activate();
        //void Deactivate();
        void ChangeButton(Boolean ru);
        void CreateHeatMapViewText(int it);

    }
}