namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    internal interface IActionTag
    {
        /// <summary>
        /// Saves action indices under a "tag" for later retrieval.
        /// This implements "action tagging". We can mark actions with a string
        /// and then retrieve their action index using that string.
        /// One must use this function during action creation and call
        /// it with the index of the action we want to tag.
        /// </summary>
        void TagAction(string tag, int actionIndex);

        /// <summary>
        /// Return the action index of the given action tag.
        /// </summary>
        int GetTaggedActionIndex(string tag);
    }
}