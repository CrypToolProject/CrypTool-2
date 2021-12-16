using CrypTool.PluginBase;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;

namespace CrypTool.WEPAttacks
{
    /// <summary>
    /// Interaktionslogik für WEPAttacksPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WEPAttacks.Properties.Resources")]
    public partial class WEPAttacksPresentation : UserControl
    {
        public WEPAttacksPresentation()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
        }

        #region Public methods for text settings
        /// <summary>
        /// Sets text within label "attack". Indicates which attack is currently running.
        /// </summary>
        /// <param name="attack">The number for the attack. 0 = no attack, 1 = FMS, 2 = KoreK, 3 = PTW.</param>
        public void setKindOfAttack(int attackNumber)
        {
            switch (attackNumber)
            {
                case 0:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                        {
                            labelAttack.Content = typeof(WEPAttacks).GetPluginStringResource("Attack_None");
                        }, attackNumber);
                    break;
                case 1:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = typeof(WEPAttacks).GetPluginStringResource("Attack_FMS");
                    }, attackNumber);
                    break;
                case 2:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = typeof(WEPAttacks).GetPluginStringResource("Attack_KoreK");
                    }, attackNumber);
                    break;
                case 3:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = typeof(WEPAttacks).GetPluginStringResource("Attack_PTW");
                    }, attackNumber);
                    break;
                default:
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                    {
                        labelAttack.Content = typeof(WEPAttacks).GetPluginStringResource("Attack_None");
                    }, attackNumber);
                    break;
            }
        }

        /// <summary>
        /// Sets the text within label "collectedPackets". Indicates how many packets have been sniffed up to now.
        /// </summary>
        /// <param name="counter">The number of sniffed packets.</param>
        public void setNumberOfSniffedPackages(int counter)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    labelCollectedPackets.Content = typeof(WEPAttacks).GetPluginStringResource("Sniffed_packets") + counter.ToString("#,#", CultureInfo.InstalledUICulture);
                }, counter);
        }

        /// <summary>
        /// Sets the text within label "usedIVs". Indicates how many packets has been used for crypto analysis.
        /// </summary>
        /// <param name="usedIVs">The up to now used packets.</param>
        public void setUsedIVs(int usedIVs)
        {
            if (usedIVs == int.MaxValue)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (SendOrPostCallback)delegate
                    {
                        labelUsedIVs.Content = typeof(WEPAttacks).GetPluginStringResource("Used_packets");
                    }, usedIVs);
            }
            else
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (SendOrPostCallback)delegate
                    {
                        labelUsedIVs.Content = typeof(WEPAttacks).GetPluginStringResource("Used_packets") + usedIVs.ToString("#,#", CultureInfo.InstalledUICulture);
                    }, usedIVs);
            }
        }

        /// <summary>
        /// Clears the text box.
        /// </summary>
        public void resetTextBox(string text)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text = text;
                }, text);
        }

        /// <summary>
        /// Sets the text within the text box. Key, key ranking and so on.
        /// </summary>
        /// <param name="votes">The votes table.</param>
        /// <param name="success">Indicates if the attack was successful.</param>
        /// <param name="keySize">Indicates the key size (40 bit or 104 bit).</param>
        /// <param name="counter">Indicates number of used packets.</param>
        /// <param name="duration">The total timespan for the attack.</param>
        /// <param name="inputMode">"file" or "plugin".</param>
        /// <param name="kindOfAttack">"FMS" or "KoreK".</param>
        /// <param name="stop">Stop button from the outer world.</param>
        public void setTextBox(int[,] votes, bool success, int keySize, int counter, TimeSpan duration, string inputMode, string kindOfAttack, bool stop)
        {
            int firstKeyByteMaxVoted = indexOfMaxVoted(votes, 0);
            int firstKeyByteMaxVotedVotes = votes[0, firstKeyByteMaxVoted];
            int firstKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 0, firstKeyByteMaxVotedVotes, firstKeyByteMaxVoted);
            int firstKeyByteSecondMostVotedVotes = votes[0, firstKeyByteSecondMostVoted];
            int firstKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 0, firstKeyByteSecondMostVotedVotes, firstKeyByteSecondMostVoted);
            int firstKeyByteThirdMostVotedVotes = votes[0, firstKeyByteThirdMostVoted];

            int secondKeyByteMaxVoted = indexOfMaxVoted(votes, 1);
            int secondKeyByteMaxVotedVotes = votes[1, secondKeyByteMaxVoted];
            int secondKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 1, secondKeyByteMaxVotedVotes, secondKeyByteMaxVoted);
            int secondKeyByteSecondMostVotedVotes = votes[1, secondKeyByteSecondMostVoted];
            int secondKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 1, secondKeyByteSecondMostVotedVotes, secondKeyByteSecondMostVoted);
            int secondKeyByteThirdMostVotedVotes = votes[1, secondKeyByteThirdMostVoted];

            int thirdKeyByteMaxVoted = indexOfMaxVoted(votes, 2);
            int thirdKeyByteMaxVotedVotes = votes[2, thirdKeyByteMaxVoted];
            int thirdKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 2, thirdKeyByteMaxVotedVotes, thirdKeyByteMaxVoted);
            int thirdKeyByteSecondMostVotedVotes = votes[2, thirdKeyByteSecondMostVoted];
            int thirdKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 2, thirdKeyByteSecondMostVotedVotes, thirdKeyByteSecondMostVoted);
            int thirdKeyByteThirdMostVotedVotes = votes[2, thirdKeyByteThirdMostVoted];

            int fourthKeyByteMaxVoted = indexOfMaxVoted(votes, 3);
            int fourthKeyByteMaxVotedVotes = votes[3, fourthKeyByteMaxVoted];
            int fourthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 3, fourthKeyByteMaxVotedVotes, fourthKeyByteMaxVoted);
            int fourthKeyByteSecondMostVotedVotes = votes[3, fourthKeyByteSecondMostVoted];
            int fourthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 3, fourthKeyByteSecondMostVotedVotes, fourthKeyByteSecondMostVoted);
            int fourthKeyByteThirdMostVotedVotes = votes[3, fourthKeyByteThirdMostVoted];

            int fifthKeyByteMaxVoted = indexOfMaxVoted(votes, 4);
            int fifthKeyByteMaxVotedVotes = votes[4, fifthKeyByteMaxVoted];
            int fifthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 4, fifthKeyByteMaxVotedVotes, fifthKeyByteMaxVoted);
            int fifthKeyByteSecondMostVotedVotes = votes[4, fifthKeyByteSecondMostVoted];
            int fifthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 4, fifthKeyByteSecondMostVotedVotes, fifthKeyByteSecondMostVoted);
            int fifthKeyByteThirdMostVotedVotes = votes[4, fifthKeyByteThirdMostVoted];

            int sixthKeyByteMaxVoted = indexOfMaxVoted(votes, 5);
            int sixthKeyByteMaxVotedVotes = votes[5, sixthKeyByteMaxVoted];
            int sixthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 5, sixthKeyByteMaxVotedVotes, sixthKeyByteMaxVoted);
            int sixthKeyByteSecondMostVotedVotes = votes[5, sixthKeyByteSecondMostVoted];
            int sixthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 5, sixthKeyByteSecondMostVotedVotes, sixthKeyByteSecondMostVoted);
            int sixthKeyByteThirdMostVotedVotes = votes[5, sixthKeyByteThirdMostVoted];

            int seventhKeyByteMaxVoted = indexOfMaxVoted(votes, 6);
            int seventhKeyByteMaxVotedVotes = votes[6, seventhKeyByteMaxVoted];
            int seventhKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 6, seventhKeyByteMaxVotedVotes, seventhKeyByteMaxVoted);
            int seventhKeyByteSecondMostVotedVotes = votes[6, seventhKeyByteSecondMostVoted];
            int seventhKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 6, seventhKeyByteSecondMostVotedVotes, seventhKeyByteSecondMostVoted);
            int seventhKeyByteThirdMostVotedVotes = votes[6, seventhKeyByteThirdMostVoted];

            int eighthKeyByteMaxVoted = indexOfMaxVoted(votes, 7);
            int eighthKeyByteMaxVotedVotes = votes[7, eighthKeyByteMaxVoted];
            int eighthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 7, eighthKeyByteMaxVotedVotes, eighthKeyByteMaxVoted);
            int eighthKeyByteSecondMostVotedVotes = votes[7, eighthKeyByteSecondMostVoted];
            int eighthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 7, eighthKeyByteSecondMostVotedVotes, eighthKeyByteSecondMostVoted);
            int eighthKeyByteThirdMostVotedVotes = votes[7, eighthKeyByteThirdMostVoted];

            int ninthKeyByteMaxVoted = indexOfMaxVoted(votes, 8);
            int ninthKeyByteMaxVotedVotes = votes[8, ninthKeyByteMaxVoted];
            int ninthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 8, ninthKeyByteMaxVotedVotes, ninthKeyByteMaxVoted);
            int ninthKeyByteSecondMostVotedVotes = votes[8, ninthKeyByteSecondMostVoted];
            int ninthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 8, ninthKeyByteSecondMostVotedVotes, ninthKeyByteSecondMostVoted);
            int ninthKeyByteThirdMostVotedVotes = votes[8, ninthKeyByteThirdMostVoted];

            int tenthKeyByteMaxVoted = indexOfMaxVoted(votes, 9);
            int tenthKeyByteMaxVotedVotes = votes[9, tenthKeyByteMaxVoted];
            int tenthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 9, tenthKeyByteMaxVotedVotes, tenthKeyByteMaxVoted);
            int tenthKeyByteSecondMostVotedVotes = votes[9, tenthKeyByteSecondMostVoted];
            int tenthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 9, tenthKeyByteSecondMostVotedVotes, tenthKeyByteSecondMostVoted);
            int tenthKeyByteThirdMostVotedVotes = votes[9, tenthKeyByteThirdMostVoted];

            int eleventhKeyByteMaxVoted = indexOfMaxVoted(votes, 10);
            int eleventhKeyByteMaxVotedVotes = votes[10, eleventhKeyByteMaxVoted];
            int eleventhKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 10, eleventhKeyByteMaxVotedVotes, eleventhKeyByteMaxVoted);
            int eleventhKeyByteSecondMostVotedVotes = votes[10, eleventhKeyByteSecondMostVoted];
            int eleventhKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 10, eleventhKeyByteSecondMostVotedVotes, eleventhKeyByteSecondMostVoted);
            int eleventhKeyByteThirdMostVotedVotes = votes[10, eleventhKeyByteThirdMostVoted];

            int twelfthKeyByteMaxVoted = indexOfMaxVoted(votes, 11);
            int twelfthKeyByteMaxVotedVotes = votes[11, twelfthKeyByteMaxVoted];
            int twelfthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 11, twelfthKeyByteMaxVotedVotes, twelfthKeyByteMaxVoted);
            int twelfthKeyByteSecondMostVotedVotes = votes[11, twelfthKeyByteSecondMostVoted];
            int twelfthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 11, twelfthKeyByteSecondMostVotedVotes, twelfthKeyByteSecondMostVoted);
            int twelfthKeyByteThirdMostVotedVotes = votes[11, twelfthKeyByteThirdMostVoted];

            int thirteenthKeyByteMaxVoted = indexOfMaxVoted(votes, 12);
            int thirteenthKeyByteMaxVotedVotes = votes[12, thirteenthKeyByteMaxVoted];
            int thirteenthKeyByteSecondMostVoted = indexOfSpecificVoted(votes, 12, thirteenthKeyByteMaxVotedVotes, thirteenthKeyByteMaxVoted);
            int thirteenthKeyByteSecondMostVotedVotes = votes[12, thirteenthKeyByteSecondMostVoted];
            int thirteenthKeyByteThirdMostVoted = indexOfSpecificVoted(votes, 12, thirteenthKeyByteSecondMostVotedVotes, thirteenthKeyByteSecondMostVoted);
            int thirteenthKeyByteThirdMostVotedVotes = votes[12, thirteenthKeyByteThirdMostVoted];

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + string.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + string.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + string.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + string.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + string.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + string.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + string.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + string.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + string.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + string.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + string.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"

                        ;
                }, votes);
            if (success && (keySize == 40))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + string.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + string.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + string.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + string.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + string.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + string.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "\n\n"

                        + string.Format(typeof(WEPAttacks).GetPluginStringResource("Possible_key_found_after_using_0_packets"), counter.ToString("#,#", CultureInfo.InstalledUICulture)) + "\n["
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "]"

                        + "\n(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + ")\n\n"


                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + "."
                        ;
                }, votes);
            }

            if (success && (keySize == 104))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + string.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + string.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + string.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + string.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + string.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + string.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + string.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + string.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + string.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + string.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + string.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"


                        + "\n\n"

                        + string.Format(typeof(WEPAttacks).GetPluginStringResource("Possible_key_found_after_using_0_packets"), counter.ToString("#,#", CultureInfo.InstalledUICulture)) + "\n["
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", seventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "]"

                        + "\n(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + (char)sixthKeyByteMaxVoted
                        + (char)seventhKeyByteMaxVoted
                        + (char)eighthKeyByteMaxVoted
                        + (char)ninthKeyByteMaxVoted
                        + (char)tenthKeyByteMaxVoted
                        + (char)eleventhKeyByteMaxVoted
                        + (char)twelfthKeyByteMaxVoted
                        + (char)thirteenthKeyByteMaxVoted
                        + ")\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + "."
                        ;
                }, votes);
            }

            if (stop)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + string.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + string.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + string.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + string.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + string.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + string.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + string.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + string.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + string.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + string.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + string.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Aborted_after") + duration + "."
                        ;
                }, votes);
            }

            if (!success && inputMode.Equals("file"))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        "KB\tbyte(vote)\n"
                        + "00\t" + string.Format("{0:X2}", firstKeyByteMaxVoted) + "(" + string.Format("{0:D4}", firstKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", firstKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", firstKeyByteThirdMostVotedVotes) + ")\n"

                        + "01\t" + string.Format("{0:X2}", secondKeyByteMaxVoted) + "(" + string.Format("{0:D4}", secondKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", secondKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", secondKeyByteThirdMostVotedVotes) + ")\n"

                        + "02\t" + string.Format("{0:X2}", thirdKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirdKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirdKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirdKeyByteThirdMostVotedVotes) + ")\n"

                        + "03\t" + string.Format("{0:X2}", fourthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fourthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fourthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fourthKeyByteThirdMostVotedVotes) + ")\n"

                        + "04\t" + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", fifthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", fifthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", fifthKeyByteThirdMostVotedVotes) + ")\n"

                        + "05\t" + string.Format("{0:X2}", sixthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", sixthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", sixthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", sixthKeyByteThirdMostVotedVotes) + ")\n"

                        + "06\t" + string.Format("{0:X2}", seventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", seventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", seventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", seventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "07\t" + string.Format("{0:X2}", eighthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eighthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eighthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eighthKeyByteThirdMostVotedVotes) + ")\n"

                        + "08\t" + string.Format("{0:X2}", ninthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", ninthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", ninthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", ninthKeyByteThirdMostVotedVotes) + ")\n"

                        + "09\t" + string.Format("{0:X2}", tenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", tenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", tenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", tenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "10\t" + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", eleventhKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", eleventhKeyByteThirdMostVotedVotes) + ")\n"

                        + "11\t" + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", twelfthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", twelfthKeyByteThirdMostVotedVotes) + ")\n"

                        + "12\t" + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteMaxVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteSecondMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteSecondMostVotedVotes) + ") "
                        + string.Format("{0:X2}", thirteenthKeyByteThirdMostVoted) + "(" + string.Format("{0:D4}", thirteenthKeyByteThirdMostVotedVotes) + ")\n"

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Could_not_recover_key")
                        + "\n"
                        + typeof(WEPAttacks).GetPluginStringResource("Maybe_you_need_more_packets");

                    if (kindOfAttack.Equals("FMS"))
                    {
                        textBox.AppendText(typeof(WEPAttacks).GetPluginStringResource("FMS_more_packets"));
                    }
                    if (kindOfAttack.Equals("KoreK"))
                    {
                        textBox.AppendText(typeof(WEPAttacks).GetPluginStringResource("KoreK_more_packets"));
                    }

                    textBox.AppendText("\n\n"
                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + ".");
                }, votes);
            }
        }


        /// <summary>
        /// Sets the text within the text box. Key, key ranking and so on.
        /// </summary>
        /// <param name="votes">The votes table.</param>
        /// <param name="success">Indicates if the attack was successful.</param>
        /// <param name="keySize">Indicates the key size (40 bit or 104 bit).</param>
        /// <param name="counter">Indicates number of used packets.</param>
        public void setTextBoxPTW(int[,] votes, bool success, int keySize, int counter, TimeSpan duration, string inputMode, bool stop)
        {
            int firstKeyByteMaxVoted = indexOfMaxVoted(votes, 0);
            int secondKeyByteMaxVoted = (indexOfMaxVoted(votes, 1) - indexOfMaxVoted(votes, 0)) & 0xFF;
            int thirdKeyByteMaxVoted = (indexOfMaxVoted(votes, 2) - indexOfMaxVoted(votes, 1)) & 0xFF;
            int fourthKeyByteMaxVoted = (indexOfMaxVoted(votes, 3) - indexOfMaxVoted(votes, 2)) & 0xFF;
            int fifthKeyByteMaxVoted = (indexOfMaxVoted(votes, 4) - indexOfMaxVoted(votes, 3)) & 0xFF;
            int sixthKeyByteMaxVoted = (indexOfMaxVoted(votes, 5) - indexOfMaxVoted(votes, 4)) & 0xFF;
            int seventhKeyByteMaxVoted = (indexOfMaxVoted(votes, 6) - indexOfMaxVoted(votes, 5)) & 0xFF;
            int eighthKeyByteMaxVoted = (indexOfMaxVoted(votes, 7) - indexOfMaxVoted(votes, 6)) & 0xFF;
            int ninthKeyByteMaxVoted = (indexOfMaxVoted(votes, 8) - indexOfMaxVoted(votes, 7)) & 0xFF;
            int tenthKeyByteMaxVoted = (indexOfMaxVoted(votes, 9) - indexOfMaxVoted(votes, 8)) & 0xFF;
            int eleventhKeyByteMaxVoted = (indexOfMaxVoted(votes, 10) - indexOfMaxVoted(votes, 9)) & 0xFF;
            int twelfthKeyByteMaxVoted = (indexOfMaxVoted(votes, 11) - indexOfMaxVoted(votes, 10)) & 0xFF;
            int thirteenthKeyByteMaxVoted = (indexOfMaxVoted(votes, 12) - indexOfMaxVoted(votes, 11)) & 0xFF;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        typeof(WEPAttacks).GetPluginStringResource("Possible_key") + ":\n"
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", seventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirteenthKeyByteMaxVoted);
                }, votes);
            if (success && (keySize == 40))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        string.Format(typeof(WEPAttacks).GetPluginStringResource("Possible_key_found_after_using_0_packets"), counter.ToString("#,#", CultureInfo.InstalledUICulture)) + "\n["
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + "]"
                        + "\n\n"

                        + "(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + ")\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + "."
                        ;
                }, votes);
            }

            if (success && (keySize == 104))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        string.Format(typeof(WEPAttacks).GetPluginStringResource("Possible_key_found_after_using_0_packets"), counter.ToString("#,#", CultureInfo.InstalledUICulture)) + "\n["
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirteenthKeyByteMaxVoted) + "]"

                        + "\n\n"

                        + "(ASCII: "
                        + (char)firstKeyByteMaxVoted
                        + (char)secondKeyByteMaxVoted
                        + (char)thirdKeyByteMaxVoted
                        + (char)fourthKeyByteMaxVoted
                        + (char)fifthKeyByteMaxVoted
                        + (char)sixthKeyByteMaxVoted
                        + (char)seventhKeyByteMaxVoted
                        + (char)eighthKeyByteMaxVoted
                        + (char)ninthKeyByteMaxVoted
                        + (char)tenthKeyByteMaxVoted
                        + (char)eleventhKeyByteMaxVoted
                        + (char)twelfthKeyByteMaxVoted
                        + (char)thirteenthKeyByteMaxVoted
                        + ")\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + "."
                        ;
                }, votes);
            }

            if ((!success) && (keySize == 900))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        typeof(WEPAttacks).GetPluginStringResource("Possible_key") + ":\n"
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirteenthKeyByteMaxVoted)

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Could_not_recover_key")
                        + "\n"
                        + typeof(WEPAttacks).GetPluginStringResource("Probably_it_is_a_strong_key")

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + ".";
                }, votes);
            }

            if ((!success) && (inputMode.Equals("file")))
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    textBox.Text =
                        typeof(WEPAttacks).GetPluginStringResource("Possible_key") + ":\n"
                        + string.Format("{0:X2}", firstKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", secondKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirdKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fourthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", fifthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", sixthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eighthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", ninthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", tenthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", eleventhKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", twelfthKeyByteMaxVoted) + ":"
                        + string.Format("{0:X2}", thirteenthKeyByteMaxVoted)

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Could_not_recover_key")
                        + "\n"
                        + typeof(WEPAttacks).GetPluginStringResource("Maybe_you_need_more_packets_104")

                        + "\n\n"

                        + typeof(WEPAttacks).GetPluginStringResource("Time_used") + duration + ".";
                }, votes);
            }
        }
        #endregion

        #region Private help methods

        /// <summary>
        /// Finds the index of the highest value in a two dimensional array in the given dimension.
        /// </summary>
        /// <param name="votes">The two dimensional array with the key byte votes in it.</param>
        /// <param name="dimenstion">Dimension, in which the max are searched.</param>
        /// <returns>The position of the hightes value and in this way the most voted key byte.</returns>
        public int indexOfMaxVoted(int[,] votes, int dimension)
        {
            int temp = 0;
            int index = 0;
            for (int i = 0; i < 256; i++)
            {
                if (votes[dimension, i] > temp)
                {
                    temp = votes[dimension, i];
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Searches for a specific voted key byte. Returns NOT the most voted, it returns the most voted
        /// up to the given limit.
        /// Example: Most voted key byte is 0x47. Then this method searches for the scond most voted key byte
        /// under 0x47.
        /// </summary>
        /// <param name="votes">The two dimensional array with the key byte votes in it.</param>
        /// <param name="dimension">Dimension in which the key byte is searched.</param>
        /// <param name="limit">The upper limit of votes.</param>
        /// <param name="indexOfGreaterKeyByte">Index of keybyte value, which is in actual context more voted.
        /// Needed if two different key bytes have same voting.</param>
        /// <returns>The most voted key byte under the given limit.</returns>
        private int indexOfSpecificVoted(int[,] votes, int dimension, int limit, int indexOfGreaterKeyByte)
        {
            int index = 0;
            int temp = 0;
            for (int i = 0; i < 256; i++)
            {
                if (votes[dimension, i] > temp)
                {
                    temp = votes[dimension, i];

                    if ((temp <= limit) && (i != indexOfGreaterKeyByte))
                    {
                        index = i;
                    }
                }
            }
            return index;
        }

        #endregion
    }
}
