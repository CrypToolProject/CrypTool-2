namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface for page view models which need an instance of ChaCha, ChaChaVisualization or ChaChaSettings
    /// </summary>
    internal interface IChaCha
    {
        ChaChaPresentationViewModel PresentationViewModel { get; }

        ChaCha ChaCha { get; }

        ChaChaSettings Settings { get; }
    }
}