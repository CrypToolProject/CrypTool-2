namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface for pages which display diffusion content.
    /// </summary>
    internal interface IDiffusion
    {
        /// <summary>
        /// Property to which the visibility of the toggle button to show the diffusion XOR values can be bound.
        /// </summary>
        bool ShowToggleButton { get; }
    }
}