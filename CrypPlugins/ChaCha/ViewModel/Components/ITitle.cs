namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface for page view models which have a title.
    /// </summary>
    internal interface ITitle
    {
        /// <summary>
        /// Page title. Will be shown on the page in the title row.
        /// </summary>
        string Title { get; set; }
    }
}