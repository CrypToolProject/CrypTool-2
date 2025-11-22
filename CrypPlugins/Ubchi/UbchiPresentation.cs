using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.Ubchi
{
    /// <summary>
    /// Presentation control for the Übchi cipher plugin
    /// Displays step-by-step visualization of the encryption/decryption process
    /// Shows transposition grids, permutations, and intermediate results
    /// </summary>
    public class UbchiPresentation : UserControl
    {
        private TextBox displayTextBox;
        private ScrollViewer scrollViewer;

        /// <summary>
        /// Constructor - initializes the WPF controls
        /// </summary>
        public UbchiPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes WPF user interface components
        /// Creates a read-only textbox with monospace font inside a scrollviewer
        /// </summary>
        private void InitializeComponent()
        {
            displayTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                Background = System.Windows.Media.Brushes.White,
                Padding = new Thickness(10),
                BorderThickness = new Thickness(1),
                BorderBrush = System.Windows.Media.Brushes.Gray
            };

            scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = displayTextBox
            };

            Content = scrollViewer;
            Background = System.Windows.Media.Brushes.White;
        }

        /// <summary>
        /// Updates the presentation display with cipher process information
        /// This method is thread-safe - it handles calls from non-UI threads
        /// </summary>
        /// <param name="inputText">Original input text</param>
        /// <param name="key">Key(s) used for encryption/decryption</param>
        /// <param name="isEncryption">True for encryption, false for decryption</param>
        /// <param name="permutationText">String representation of the permutation array</param>
        /// <param name="firstGridText">Grid visualization for first transposition</param>
        /// <param name="firstResult">Result after first transposition</param>
        /// <param name="withNulls">Text with null characters added/before removal</param>
        /// <param name="secondGridText">Grid visualization for second transposition</param>
        /// <param name="finalResult">Final encryption/decryption result</param>
        /// <param name="nullCharsInfo">Information about which null characters were used</param>
        public void UpdatePresentation(string inputText, string key, bool isEncryption,
            string permutationText, string firstGridText, string firstResult,
            string withNulls, string secondGridText, string finalResult, string nullCharsInfo = "")
        {
            // Check if we're on the UI thread
            if (Dispatcher.CheckAccess())
            {
                UpdatePresentationInternal(inputText, key, isEncryption, permutationText,
                    firstGridText, firstResult, withNulls, secondGridText, finalResult, nullCharsInfo);
            }
            else
            {
                // If not on UI thread, invoke on UI thread
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdatePresentationInternal(inputText, key, isEncryption, permutationText,
                        firstGridText, firstResult, withNulls, secondGridText, finalResult, nullCharsInfo);
                }));
            }
        }

        /// <summary>
        /// Internal method that actually updates the presentation
        /// Builds a formatted text display showing all steps of the cipher process
        /// Must be called on the UI thread
        /// </summary>
        private void UpdatePresentationInternal(string inputText, string key, bool isEncryption,
            string permutationText, string firstGridText, string firstResult,
            string withNulls, string secondGridText, string finalResult, string nullCharsInfo)
        {
            try
            {
                StringBuilder display = new StringBuilder();

                // Header
                display.AppendLine("═══════════════════════════════════════════════════");
                display.AppendLine("                 UBCHI CIPHER STEPS");
                display.AppendLine("═══════════════════════════════════════════════════");
                display.AppendLine();

                // Input information section
                display.AppendLine(" INPUT INFORMATION:");
                display.AppendLine("   Input: " + (inputText.Length > 80 ? inputText.Substring(0, 80) + "..." : inputText));
                display.AppendLine("   Key: " + key);
                display.AppendLine("   Action: " + (isEncryption ? "Encryption" : "Decryption"));
                display.AppendLine("   Nulls: " + CountWords(key) + " (based on words in key)");
                display.AppendLine();

                // Permutation section
                display.AppendLine(" PERMUTATION:");
                display.AppendLine("   " + permutationText);
                display.AppendLine();

                // Step 1: First or reverse second transposition
                string step1Title = isEncryption ? " STEP 1: FIRST TRANSPOSITION" : " STEP 1: REVERSE SECOND TRANSPOSITION";
                display.AppendLine(step1Title);
                display.AppendLine("   Fill by " + (isEncryption ? "rows, read by columns:" : "columns, read by rows:"));
                display.AppendLine();

                if (!string.IsNullOrEmpty(firstGridText))
                {
                    DisplayFormattedGrid(display, firstGridText);
                }
                display.AppendLine();
                display.AppendLine("   Result: " + FormatLongText(firstResult, 70));
                display.AppendLine();

                // Step 2: Add or remove nulls
                if (isEncryption)
                {
                    display.AppendLine(" STEP 2: ADD NULLS");
                    display.AppendLine("   Adding " + CountWords(key) + " frequency-based null characters...");
                    if (!string.IsNullOrEmpty(nullCharsInfo))
                    {
                        display.AppendLine("   " + nullCharsInfo);
                    }
                    display.AppendLine("   With nulls: " + FormatLongText(withNulls, 70));
                    display.AppendLine();
                }
                else
                {
                    display.AppendLine(" STEP 2: REMOVE NULLS");
                    display.AppendLine("   Removing " + CountWords(key) + " null characters...");
                    if (!string.IsNullOrEmpty(nullCharsInfo))
                    {
                        display.AppendLine("   " + nullCharsInfo);
                    }
                    display.AppendLine("   Without nulls: " + FormatLongText(withNulls, 70));
                    display.AppendLine();
                }

                // Step 3: Second or reverse first transposition
                string step3Title = isEncryption ? " STEP 3: SECOND TRANSPOSITION" : " STEP 3: REVERSE FIRST TRANSPOSITION";
                display.AppendLine(step3Title);
                display.AppendLine("   Fill by " + (isEncryption ? "rows, read by columns:" : "columns, read by rows:"));
                display.AppendLine();

                if (!string.IsNullOrEmpty(secondGridText))
                {
                    DisplayFormattedGrid(display, secondGridText);
                }
                display.AppendLine();
                display.AppendLine("   Result: " + FormatLongText(finalResult, 70));
                display.AppendLine();

                // Final result section
                display.AppendLine("═══════════════════════════════════════════════════");
                display.AppendLine(" FINAL RESULT (formatted in groups of 5):");
                display.AppendLine("   " + FormatInGroupsOfFive(finalResult));
                display.AppendLine("═══════════════════════════════════════════════════");

                // Update the display and scroll to top
                displayTextBox.Text = display.ToString();
                scrollViewer.ScrollToTop();
            }
            catch (Exception ex)
            {
                displayTextBox.Text = "Error updating presentation: " + ex.Message;
            }
        }

        /// <summary>
        /// Displays a formatted grid with proper indentation
        /// Permutation row has extra space for alignment
        /// Skips separator lines (containing "--")
        /// </summary>
        /// <param name="display">StringBuilder to append grid to</param>
        /// <param name="gridText">Grid text to format and display</param>
        private void DisplayFormattedGrid(StringBuilder display, string gridText)
        {
            string[] gridLines = gridText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineCount = 0;

            foreach (string line in gridLines)
            {
                if (string.IsNullOrEmpty(line.Trim())) continue;

                // Skip separator lines
                if (line.Contains("--")) continue;

                if (lineCount == 0)
                {
                    // Permutation row: 4 spaces (aligned with row numbers having 3 spaces)
                    display.AppendLine("     " + line.Trim());  
                }
                else
                {
                    // Data rows: 3 spaces
                    display.AppendLine("   " + line.Trim());
                }

                lineCount++;
            }
        }

        /// <summary>
        /// Formats text in groups of 5 characters for readability
        /// Example: "ABCDEFGHIJK" -> "ABCDE FGHIJK"
        /// </summary>
        private string FormatInGroupsOfFive(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder formatted = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 5 == 0) formatted.Append(' ');
                formatted.Append(text[i]);
            }
            return formatted.ToString();
        }

        /// <summary>
        /// Truncates long text to specified maximum length
        /// Adds "..." if text was truncated
        /// </summary>
        private string FormatLongText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Counts the number of words in a key string
        /// Words are separated by spaces
        /// </summary>
        private int CountWords(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            return key.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Clears the presentation and shows a ready message
        /// Thread-safe - can be called from any thread
        /// </summary>
        public void ClearPresentation()
        {
            if (Dispatcher.CheckAccess())
            {
                displayTextBox.Text = "UBCHI Cipher - Ready for input...";
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    displayTextBox.Text = "UBCHI Cipher - Ready for input...";
                }));
            }
        }
    }
}
