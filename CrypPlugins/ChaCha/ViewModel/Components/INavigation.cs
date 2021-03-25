namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Interface for page view models which are navigable to by the page navigation.
    /// </summary>
    internal interface INavigation
    {
        /// <summary>
        /// Page name. Will be used as content in page navigation button.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Function which should be called if user enters this page.
        /// </summary>
        void Setup();

        /// <summary>
        /// Function which should be called if user leaves this page.
        /// </summary>
        void Teardown();
    }
}