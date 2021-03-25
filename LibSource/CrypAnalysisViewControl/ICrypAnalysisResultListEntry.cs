namespace CrypTool.CrypAnalysisViewControl
{
    public interface ICrypAnalysisResultListEntry
    {
        /// <summary>
        /// String of the entry's main value to be copied into the clipboard.
        /// </summary>
        string ClipboardValue { get; }

        /// <summary>
        /// String of the entry's key value to be copied into the clipboard.
        /// </summary>
        string ClipboardKey { get; }

        /// <summary>
        /// String of the entry's text value to be copied into the clipboard.
        /// </summary>
        string ClipboardText { get; }

        /// <summary>
        /// String of the whole result entry to be copied into the clipboard.
        /// </summary>
        string ClipboardEntry { get; }
    }
}
