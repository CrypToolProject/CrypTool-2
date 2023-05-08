#define _DEBUG_

using Keccak.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.Keccak
{
    public class Keccak_f
    {
        #region variables

        private const int X = 5;
        private const int Y = 5;

        private readonly int z, l;             // 2^l = z = lane size
        private readonly int rounds;

        private readonly KeccakPres pres;
        private bool presActive;

        private byte[][][] columns;
        private byte[][][] rows;
        private byte[][][] lanes;

        private readonly Keccak plugin;

        private readonly StreamWriter DebugWriter = null;

        /* translation vectors for rho */
        private readonly int[][] translationVectors = new int[][]
        {
            new int[] { 0, 1, 190, 28, 91 },
            new int[] { 36, 300, 6, 55, 276 },
            new int[] { 3, 10, 171, 153, 231 },
            new int[] { 105, 45, 15, 21, 136 },
            new int[] { 210, 66, 253, 120, 78 }
        };

        /* iota round constants */
        private readonly byte[][] roundConstants = new byte[][]
        {
            new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x82, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8a, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x00, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x8b, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x81, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x09, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x8a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x09, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x0a, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8b, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x89, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x03, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x02, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x0a, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x0a, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x81, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x08, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
        };

        /* iota round constants for presentation */
        private readonly string[] roundConstantsPres = new string[]
        {
            "0x0000000000000001", "0x0000000000008082", "0x800000000000808A", "0x8000000080008000",
            "0x000000000000808B", "0x0000000080000001", "0x8000000080008081", "0x8000000000008009",
            "0x000000000000008A", "0x0000000000000088", "0x0000000080008009", "0x000000008000000A",
            "0x000000008000808B", "0x800000000000008B", "0x8000000000008089", "0x8000000000008003",
            "0x8000000000008002", "0x8000000000000080", "0x000000000000800A", "0x800000008000000A",
            "0x8000000080008081", "0x8000000000008080", "0x0000000080000001", "0x8000000080008008",
        };

        #endregion

        public Keccak_f(int stateSize, ref byte[] state, ref KeccakPres pres, Keccak plugin, StreamWriter writer)
        {
            Debug.Assert(stateSize % 25 == 0);
            z = stateSize / 25;                     // length of a lane
            l = (int)Math.Log(z, 2);        // parameter l

            Debug.Assert((int)Math.Pow(2, l) == z);
            rounds = 12 + 2 * l;
            this.pres = pres;
            presActive = false;
            this.plugin = plugin;
            DebugWriter = writer;
        }

        public void Permute(ref byte[] state, int progressionStepCounter, int progressionSteps)
        {
            /* the order of steps is taken from the pseudo-code description at http://keccak.noekeon.org/specs_summary.html (accessed on 2013-02-01) */

#if _DEBUG_
            DebugWriter.Write("#Keccak-f: start Keccak-f[{0}] with {1} rounds", z * 25, rounds);
            DebugWriter.Write("#Keccak-f: state before permutation:");
            KeccakHashFunction.PrintBits(state, z, DebugWriter);
            KeccakHashFunction.PrintBytes(state, z, DebugWriter);
#endif

            for (int i = 0; i < rounds; i++)
            {
#if _VERBOSE_
                OutWriter.WriteLine(Environment.NewLine + "Round {0}", i + 1);
                OutWriter.WriteLine("State before Keccak-f[{0}]", z * 25);
                KeccakHashFunction.PrintBits(state, z);
                KeccakHashFunction.PrintBytes(state, z);
#endif

                if (pres.IsVisible && !pres.skipPresentation && !pres.skipPermutation)
                {
                    string roundStr = (i + 1).ToString();
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Keccak-f";
                        pres.labelCurrentStep.Content = "Theta";
                        pres.imgStepTheta.Visibility = Visibility.Visible;
                        pres.stepCanvas.Visibility = Visibility.Visible;

                        pres.buttonSkip.IsEnabled = true;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;

                        pres.labelStepRounds.Content = string.Format(Resources.PresRoundOfRounds, roundStr, rounds);
                        pres.labelRound.Content = string.Format(Resources.PresRoundOfRounds, roundStr, rounds);
                    }, null);

                    /* wait button clicks */
                    if (!pres.skipPermutation)
                    {
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.stepCanvas.Visibility = Visibility.Hidden;
                        pres.imgStepTheta.Visibility = Visibility.Hidden;
                    }, null);
                }

                Theta(ref state);

                plugin.ProgressChanged(progressionStepCounter + (1.0 / 6.0) + (5.0 / 6.0) * ((i / (double)rounds) + (1.0 / 5.0) * (1.0 / rounds)), progressionSteps);

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);

                if (pres.IsVisible && !pres.skipPresentation && !pres.skipPermutation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Keccak-f";
                        pres.labelCurrentStep.Content = "Rho";
                        pres.imgStepRho.Visibility = Visibility.Visible;
                        pres.stepCanvas.Visibility = Visibility.Visible;

                        pres.buttonSkip.IsEnabled = true;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;
                    }, null);

                    /* force skipping rho presentation if state size is not supported */
                    #region force skip rho
                    if (z < 8)
                    {
                        /* disable next click button to force the user to skip the rho step */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.buttonNext.IsEnabled = false;
                            pres.textBlockStepPresentationNotAvailable.Text = string.Format(Resources.PresStepPresentationNotAvailable, "rho");
                            pres.textBlockStepPresentationNotAvailable.Visibility = Visibility.Visible;
                        }, null);
                    }
                    #endregion

                    /* wait button clicks */
                    if (!pres.skipPermutation)
                    {
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.stepCanvas.Visibility = Visibility.Hidden;
                        pres.imgStepRho.Visibility = Visibility.Hidden;
                    }, null);
                }

                Rho(ref state);

                plugin.ProgressChanged(progressionStepCounter + (1.0 / 6.0) + (5.0 / 6.0) * ((i / (double)rounds) + (2.0 / 5.0) * (1.0 / rounds)), progressionSteps);

                #region force skip rho
                if (z < 8)
                {
                    /* enable next button again */
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.buttonNext.IsEnabled = true;
                        pres.textBlockStepPresentationNotAvailable.Visibility = Visibility.Hidden;
                        pres.textBlockStepPresentationNotAvailable.Text = "";
                    }, null);
                }
                #endregion

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);

                if (pres.IsVisible && !pres.skipPresentation && !pres.skipPermutation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Keccak-f";
                        pres.labelCurrentStep.Content = "Pi";
                        pres.imgStepPi.Visibility = Visibility.Visible;
                        pres.stepCanvas.Visibility = Visibility.Visible;

                        pres.buttonSkip.IsEnabled = true;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;
                    }, null);

                    /* wait button clicks */
                    if (!pres.skipPermutation)
                    {
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.stepCanvas.Visibility = Visibility.Hidden;
                        pres.imgStepPi.Visibility = Visibility.Hidden;
                    }, null);
                }

                Pi(ref state);

                plugin.ProgressChanged(progressionStepCounter + (1.0 / 6.0) + (5.0 / 6.0) * ((i / (double)rounds) + (3.0 / 5.0) * (1.0 / rounds)), progressionSteps);

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);

                if (pres.IsVisible && !pres.skipPresentation && !pres.skipPermutation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Keccak-f";
                        pres.labelCurrentStep.Content = "Chi";
                        pres.imgStepChi.Visibility = Visibility.Visible;
                        pres.stepCanvas.Visibility = Visibility.Visible;

                        pres.buttonSkip.IsEnabled = true;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;
                    }, null);

                    /* wait button clicks */
                    if (!pres.skipPermutation)
                    {
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.stepCanvas.Visibility = Visibility.Hidden;
                        pres.imgStepChi.Visibility = Visibility.Hidden;
                    }, null);
                }

                Chi(ref state);

                plugin.ProgressChanged(progressionStepCounter + (1.0 / 6.0) + (5.0 / 6.0) * ((i / (double)rounds) + (4.0 / 5.0) * (1.0 / rounds)), progressionSteps);

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);

                if (pres.IsVisible && !pres.skipPresentation && !pres.skipPermutation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Keccak-f";
                        pres.labelCurrentStep.Content = "Iota";
                        pres.imgStepIota.Visibility = Visibility.Visible;
                        pres.stepCanvas.Visibility = Visibility.Visible;

                        pres.buttonSkip.IsEnabled = true;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;
                    }, null);

                    /* force skipping iota presentation if state size is not supported */
                    #region force skip iota
                    if (z < 8)
                    {
                        /* disable next click button to force the user to skip the rho step */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.buttonNext.IsEnabled = false;
                            pres.textBlockStepPresentationNotAvailable.Text = string.Format(Resources.PresStepPresentationNotAvailable, "iota");
                            pres.textBlockStepPresentationNotAvailable.Visibility = Visibility.Visible;
                        }, null);
                    }
                    #endregion

                    /* wait button clicks */
                    if (!pres.skipPermutation)
                    {
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.stepCanvas.Visibility = Visibility.Hidden;
                        pres.imgStepIota.Visibility = Visibility.Hidden;
                    }, null);
                }

                Iota(ref state, i);

                plugin.ProgressChanged(progressionStepCounter + (1.0 / 6.0) + (5.0 / 6.0) * ((i / (double)rounds) + (5.0 / 5.0) * (1.0 / rounds)), progressionSteps);

                #region force skip iota
                if (z < 8)
                {
                    /* enable next button again */
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.buttonNext.IsEnabled = true;
                        pres.textBlockStepPresentationNotAvailable.Visibility = Visibility.Hidden;
                        pres.textBlockStepPresentationNotAvailable.Text = "";
                    }, null);
                }
                #endregion

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);
            }

#if _DEBUG_
            DebugWriter.WriteLine(Environment.NewLine + "#Keccak-f: state after permutation");
            KeccakHashFunction.PrintBits(state, z, DebugWriter);
            KeccakHashFunction.PrintBytes(state, z, DebugWriter);
#endif

        }

        #region step mappings

        public void Theta(ref byte[] state)
        {
            byte parity1, parity2, parityResult;
            byte[][][] tmpColumns;

            GetColumnsFromState(ref state);

            // clone `columns` in `tmpColumns`
            tmpColumns = new byte[z][][];
            for (int i = 0; i < z; i++)
            {
                tmpColumns[i] = new byte[X][];
                for (int j = 0; j < X; j++)
                {
                    tmpColumns[i][j] = new byte[Y];
                    for (int k = 0; k < Y; k++)
                    {
                        tmpColumns[i][j][k] = columns[i][j][k];
                    }
                }
            }

            for (int i = 0; i < z; i++)          // iterate over slices
            {
                for (int j = 0; j < X; j++)      // iterate over columns of a slice
                {
                    /* xor the parities of two certain nearby columns */
                    parity1 = Parity(columns[i][(X + j - 1) % X]);              // add X because if j = 0 the reult would be negative
                    parity2 = Parity(columns[(z + i - 1) % z][(j + 1) % X]);    // same here with z
                    parityResult = (byte)(parity1 ^ parity2);

                    for (int k = 0; k < Y; k++)  // iterate over bits of a column
                    {
                        tmpColumns[i][j][k] ^= parityResult;
                    }

                    ThetaPres(columns[i][j], tmpColumns[i][j], columns[i][(X + j - 1) % X], columns[(z + i - 1) % z][(j + 1) % X], parity1, parity2, i, j);
                }
            }

            columns = tmpColumns;

            SetColumnsToState(ref state);

            pres.autostep = false;
            pres.skipStep = false;
        }

        public void Rho(ref byte[] state)
        {
            /* do nothing when lane size is 1 */
            if (z == 1)
            {
                return;
            }

            byte[] oldLane;
            int[][] translationVectorsPres = new int[5][];

            #region presentation translation vectors table

            if (pres.IsVisible && !pres.skipPresentation)
            {
                /* initialize translation vectors for presentation */
                for (int i = 0; i < 5; i++)
                {
                    translationVectorsPres[i] = new int[5];
                    for (int j = 0; j < 5; j++)
                    {
                        translationVectorsPres[i][j] = translationVectors[i][j] % z;
                    }
                }

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label139.Content = translationVectorsPres[0][0].ToString();
                    pres.label140.Content = translationVectorsPres[0][1].ToString();
                    pres.label141.Content = translationVectorsPres[0][2].ToString();
                    pres.label142.Content = translationVectorsPres[0][3].ToString();
                    pres.label143.Content = translationVectorsPres[0][4].ToString();

                    pres.label144.Content = translationVectorsPres[1][0].ToString();
                    pres.label145.Content = translationVectorsPres[1][1].ToString();
                    pres.label146.Content = translationVectorsPres[1][2].ToString();
                    pres.label147.Content = translationVectorsPres[1][3].ToString();
                    pres.label148.Content = translationVectorsPres[1][4].ToString();

                    pres.label149.Content = translationVectorsPres[2][0].ToString();
                    pres.label150.Content = translationVectorsPres[2][1].ToString();
                    pres.label151.Content = translationVectorsPres[2][2].ToString();
                    pres.label152.Content = translationVectorsPres[2][3].ToString();
                    pres.label153.Content = translationVectorsPres[2][4].ToString();

                    pres.label154.Content = translationVectorsPres[3][0].ToString();
                    pres.label155.Content = translationVectorsPres[3][1].ToString();
                    pres.label156.Content = translationVectorsPres[3][2].ToString();
                    pres.label157.Content = translationVectorsPres[3][3].ToString();
                    pres.label158.Content = translationVectorsPres[3][4].ToString();

                    pres.label159.Content = translationVectorsPres[4][0].ToString();
                    pres.label160.Content = translationVectorsPres[4][1].ToString();
                    pres.label161.Content = translationVectorsPres[4][2].ToString();
                    pres.label162.Content = translationVectorsPres[4][3].ToString();
                    pres.label163.Content = translationVectorsPres[4][4].ToString();
                }, null);
            }
            #endregion

            GetLanesFromState(ref state);

            /* rotate lanes by a certain value */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    oldLane = lanes[i][j];
                    lanes[i][j] = RotateByteArray(lanes[i][j], translationVectors[i][j] % z);

                    RhoPres(oldLane, lanes[i][j], i, j, translationVectors[i][j] % z);
                }
            }

            SetLanesToState(ref state);

            pres.autostep = false;
            pres.skipStep = false;
        }

        public void Pi(ref byte[] state)
        {
            byte[][][] tmpLanes;

            // init `tmpLanes`
            tmpLanes = new byte[Y][][];
            for (int i = 0; i < Y; i++)
            {
                tmpLanes[i] = new byte[X][];
                for (int j = 0; j < X; j++)
                {
                    tmpLanes[i][j] = new byte[z];
                }
            }

            GetLanesFromState(ref state);

            /* rearrange lanes in a certain pattern */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    tmpLanes[(2 * j + 3 * i) % X][i] = lanes[i][j];
                }
            }

            PiPres(tmpLanes);

            lanes = tmpLanes;
            SetLanesToState(ref state);

            pres.autostep = false;
            pres.skipStep = false;
        }

        public void Chi(ref byte[] state)
        {
            byte inv;
            byte[] oldRow = new byte[X];

            GetRowsFromState(ref state);

            for (int i = 0; i < z; i++)             // iterate over slices
            {
                for (int j = 0; j < Y; j++)         // iterate over rows of a slice
                {
                    for (int k = 0; k < X; k++)     // save old value of row
                    {
                        oldRow[k] = rows[i][j][k];
                    }
                    for (int k = 0; k < X; k++)     // iterate over bits of a row
                    {
                        /* the inverting has to be calculated manually. Since a byte represents a bit, the "~"-operator would lead to false results here. */
                        inv = oldRow[(k + 1) % X];
                        inv = (byte)(inv == 0x00 ? 0x01 : 0x00);

                        rows[i][j][k] = (byte)(oldRow[k] ^ (inv & oldRow[(k + 2) % X]));
                    }

                    ChiPres(oldRow, i, j);
                }
            }

            /* write back to state */
            SetRowsToState(ref state);

            pres.autostep = false;
            pres.skipStep = false;
        }

        public void Iota(ref byte[] state, int round)
        {
            /* map round constant bits to bytes */
            byte[] constant = KeccakHashFunction.ByteArrayToBitArray(roundConstants[round]);

            /* truncate constant to the size of z */
            byte[] truncatedConstant = KeccakHashFunction.SubArray(constant, 0, z);

            byte[] firstLane = GetFirstLaneFromState(ref state);
            byte[] firstLaneOld = GetFirstLaneFromState(ref state);

            /* xor round constant */
            for (int i = 0; i < z; i++)
            {
                firstLane[i] ^= truncatedConstant[i];
            }

            IotaPres(firstLane, firstLaneOld, truncatedConstant, round);

            SetFirstLaneToState(ref state, firstLane);

            pres.autostep = false;
            pres.skipStep = false;
        }

        #endregion

        #region presentation methods

        public void ThetaPres(byte[] columnOld, byte[] columnNew, byte[] columnLeft, byte[] columnRight, int parity1, int parity2, int slice, int column)
        {
            if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    /* show theta canvas in first iteration */
                    if (slice == 0 && column == 0)
                    {
                        pres.buttonAutostep.IsEnabled = true;
                        pres.autostepSpeedSlider.IsEnabled = true;

                        pres.textBlockCurrentPhase.Text = Resources.PresThetaPhaseText;
                        pres.textBlockCurrentStep.Text = Resources.PresThetaStepText;

                        pres.canvasStepDetailTheta.Visibility = Visibility.Visible;
                        pres.canvasCubeTheta.Visibility = Visibility.Visible;
                        pres.zCoordinates.Visibility = Visibility.Visible;
                        presActive = true;
                    }

                    #region pres cube

                    switch (z)
                    {
                        case 1:
                            /* TODO */
                            break;
                        case 2:
                            /* TODO */
                            break;
                        case 4:
                            /* TODO */
                            break;

                        #region lane size greater than 4

                        case 8:
                        case 16:
                        case 32:
                        case 64:

                            /* show cube */
                            if (slice == 0)
                            {
                                /* show default cube */
                                pres.imgCubeDefault.Visibility = Visibility.Visible;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice + 1);
                                pres.zCoordinate_2.Content = "z=" + (slice + 2);
                                pres.zCoordinate_3.Content = "z=" + (slice + 3);
                                pres.zCoordinate_4.Content = "z=" + (slice + 4);
                            }
                            else if (slice >= 3 && slice < z - 2)
                            {
                                if (slice == 3)
                                {
                                    /* show inner cube */
                                    pres.imgCubeDefault.Visibility = Visibility.Hidden;
                                    pres.imgCubeDefaultInner.Visibility = Visibility.Visible;
                                }

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice - 1);
                                pres.zCoordinate_2.Content = "z=" + (slice);
                                pres.zCoordinate_3.Content = "z=" + (slice + 1);
                                pres.zCoordinate_4.Content = "z=" + (slice + 2);
                            }
                            else if (slice == z - 2)
                            {
                                /* show bottom cube */
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultBottom.Visibility = Visibility.Visible;

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice - 1);
                                pres.zCoordinate_2.Content = "z=" + (slice);
                                pres.zCoordinate_3.Content = "z=" + (slice + 1);
                                pres.zCoordinate_4.Content = "z=" + (slice + 2);
                            }

                            /* move modified row */
                            if (slice == 0)
                            {
                                pres.imgThetaModifiedRowFront.SetValue(Canvas.LeftProperty, 7.0 + column * 26);
                                pres.imgThetaModifiedRowFront.Visibility = Visibility.Visible;
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 8.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 50.0);
                                pres.imgThetaModifiedRowTop.Visibility = Visibility.Visible;
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 51.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaModifiedRowFront.Visibility = Visibility.Hidden;
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 21.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 37.0);
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 38.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 24.0);
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 163.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 25.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26 + (slice - (z - 2)) * 13);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);

                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 163.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 25.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }

                            /* move left row */
                            if (slice == 0)
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowFront.SetValue(Canvas.LeftProperty, 111.0);
                                    pres.imgThetaLeftRowFront.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 112.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 51.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowFront.SetValue(Canvas.LeftProperty, 7.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowFront.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 8.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaLeftRowFront.Visibility = Visibility.Hidden;
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 125.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 38.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 21.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 37.0);
                                }
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 138.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 163.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 25.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 34.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0);
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 138.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 163.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 25.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 34.0 + (column - 1) * 26 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);
                                }
                            }

                            /* move right row */
                            if (slice == 0)
                            {
                                pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowFront.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowTop.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.LeftProperty, 46.0);
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.TopProperty, 11.0);
                                    pres.imgThetaRightRowTopFading.Visibility = Visibility.Visible;
                                    pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.LeftProperty, 72.0 + column * 26);
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.TopProperty, 11.0);
                                    pres.imgThetaRightRowTopFading.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSideFading.SetValue(Canvas.LeftProperty, 176.0);
                                        pres.imgThetaRightRowSideFading.SetValue(Canvas.TopProperty, 12.0);
                                        pres.imgThetaRightRowSideFading.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowTopFading.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowFront.SetValue(Canvas.LeftProperty, 7.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 8.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    pres.imgThetaRightRowFront.SetValue(Canvas.LeftProperty, 33.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;

                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 51.0);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }

                                pres.imgThetaRightRowFront.Visibility = Visibility.Visible;
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                pres.imgThetaRightRowFront.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 21.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 47.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 38.0);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 21.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 47.0 + column * 26 + (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 150.0 + (slice - (z - 2)) * 13);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 38.0 - (slice - (z - 2)) * 13);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }
                            }

                            break;

                        #endregion

                        default:
                            break;
                    }

                    #endregion

                    #region pres detailed

                    /* left and right column */
                    pres.label164.Content = columnLeft[0].ToString();
                    pres.label165.Content = columnLeft[1].ToString();
                    pres.label166.Content = columnLeft[2].ToString();
                    pres.label167.Content = columnLeft[3].ToString();
                    pres.label168.Content = columnLeft[4].ToString();

                    pres.label169.Content = columnRight[0].ToString();
                    pres.label170.Content = columnRight[1].ToString();
                    pres.label171.Content = columnRight[2].ToString();
                    pres.label172.Content = columnRight[3].ToString();
                    pres.label173.Content = columnRight[4].ToString();

                    /* parity bits */
                    pres.label174.Content = parity1.ToString();
                    pres.label175.Content = parity2.ToString();

                    /* old and new column */
                    pres.label176.Content = columnOld[0].ToString();
                    pres.label177.Content = columnOld[1].ToString();
                    pres.label178.Content = columnOld[2].ToString();
                    pres.label179.Content = columnOld[3].ToString();
                    pres.label180.Content = columnOld[4].ToString();

                    pres.label181.Content = columnNew[0].ToString();
                    pres.label182.Content = columnNew[1].ToString();
                    pres.label183.Content = columnNew[2].ToString();
                    pres.label184.Content = columnNew[3].ToString();
                    pres.label185.Content = columnNew[4].ToString();

                    #endregion

                }, null);

                /* wait for button clicks */
                if (!pres.autostep || (slice == (z - 1) && column == (X - 1)))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed);       // value adjustable by a slider
                }
            }

            /* hide theta canvas after last iteration */
            if (slice == (z - 1) && column == (X - 1))
            {
                if (presActive)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockCurrentPhase.Text = "";
                        pres.textBlockCurrentStep.Text = "";

                        pres.canvasStepDetailTheta.Visibility = Visibility.Hidden;
                        pres.canvasCubeTheta.Visibility = Visibility.Hidden;
                        pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                        pres.imgCubeDefaultBottom.Visibility = Visibility.Hidden;
                        pres.imgCubeDefault.Visibility = Visibility.Hidden;
                        pres.zCoordinates.Visibility = Visibility.Hidden;
                        presActive = false;
                    }, null);
                }
            }
        }

        public void RhoPres(byte[] oldLane, byte[] newLane, int plane, int lane, int rotationOffset)
        {
            if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
            {
                /* show rho canvas in first iteration */
                if (plane == 0 && lane == 0)
                {
                    presActive = true;

                    #region toggle visibility of rho images with respect to lane size

                    switch (z)
                    {
                        case 8:
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.imgStepDetailRho8.Visibility = Visibility.Visible;
                                pres.imgStepDetailRho16.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho32.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho64.Visibility = Visibility.Hidden;

                                pres.labelRhoBitPos1.Content = "";
                                pres.labelRhoBitPos2.Content = "";
                                pres.labelRhoBitPos3.Content = "";
                                pres.labelRhoBitPos4.Content = "";
                                pres.labelRhoBitPos5.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos6.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos7.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos8.Visibility = Visibility.Hidden;
                            }, null);
                            break;

                        case 16:
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.imgStepDetailRho8.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho16.Visibility = Visibility.Visible;
                                pres.imgStepDetailRho32.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho64.Visibility = Visibility.Hidden;

                                pres.labelRhoBitPos1.Content = "";
                                pres.labelRhoBitPos2.Content = "";
                                pres.labelRhoBitPos3.Content = "";
                                pres.labelRhoBitPos4.Content = "";
                                pres.labelRhoBitPos5.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos6.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos7.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos8.Visibility = Visibility.Hidden;
                            }, null);
                            break;

                        case 32:
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.imgStepDetailRho8.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho16.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho32.Visibility = Visibility.Visible;
                                pres.imgStepDetailRho64.Visibility = Visibility.Hidden;

                                pres.labelRhoBitPos1.Content = "";
                                pres.labelRhoBitPos2.Content = "";
                                pres.labelRhoBitPos3.Content = "0";
                                pres.labelRhoBitPos4.Content = "16";
                                pres.labelRhoBitPos5.Visibility = Visibility.Visible;
                                pres.labelRhoBitPos6.Visibility = Visibility.Visible;
                                pres.labelRhoBitPos7.Visibility = Visibility.Hidden;
                                pres.labelRhoBitPos8.Visibility = Visibility.Hidden;
                            }, null);
                            break;

                        case 64:
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.imgStepDetailRho8.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho16.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho32.Visibility = Visibility.Hidden;
                                pres.imgStepDetailRho64.Visibility = Visibility.Visible;

                                pres.labelRhoBitPos1.Content = "0";
                                pres.labelRhoBitPos2.Content = "16";
                                pres.labelRhoBitPos3.Content = "32";
                                pres.labelRhoBitPos4.Content = "48";
                                pres.labelRhoBitPos5.Visibility = Visibility.Visible;
                                pres.labelRhoBitPos6.Visibility = Visibility.Visible;
                                pres.labelRhoBitPos7.Visibility = Visibility.Visible;
                                pres.labelRhoBitPos8.Visibility = Visibility.Visible;
                            }, null);
                            break;

                        default: break;
                    }

                    #endregion

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.buttonAutostep.IsEnabled = true;
                        pres.autostepSpeedSlider.IsEnabled = true;

                        pres.textBlockCurrentPhase.Text = Resources.PresRhoPhaseText;
                        pres.textBlockCurrentStep.Text = Resources.PresRhoStepText;

                        pres.canvasStepDetailRho.Visibility = Visibility.Visible;
                        pres.canvasCubeRho.Visibility = Visibility.Visible;
                        pres.imgCubeDefault.Visibility = Visibility.Visible;
                        pres.xCoordinates.Visibility = Visibility.Visible;
                        pres.yCoordinates.Visibility = Visibility.Visible;
                    }, null);
                }

                /* set coordinates */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.xCoordinate_var.Content = "x=" + (lane + 1);
                    pres.xCoordinate_var.SetValue(Canvas.LeftProperty, 17.0 + 26 * lane);
                    pres.yCoordinate_var.Content = "y=" + (plane + 1);
                    pres.yCoordinate_var.SetValue(Canvas.TopProperty, 174.0 - 26 * plane);
                }, null);

                #region pres cube

                /* move modified lane */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgRhoModifiedLane.SetValue(Canvas.TopProperty, 167.0 - plane * 26);
                    pres.imgRhoModifiedLane.SetValue(Canvas.LeftProperty, 7.0 + lane * 26);

                    pres.imgRhoModifiedSideLane.Visibility = Visibility.Hidden;
                    pres.imgRhoModifiedTopLane.Visibility = Visibility.Hidden;

                    if (lane == 4)
                    {
                        pres.imgRhoModifiedSideLane.Visibility = Visibility.Visible;
                        pres.imgRhoModifiedSideLane.SetValue(Canvas.TopProperty, 116.0 - plane * 26);
                    }
                    if (plane == 4)
                    {
                        pres.imgRhoModifiedTopLane.SetValue(Canvas.LeftProperty, 8.0 + lane * 26);
                        pres.imgRhoModifiedTopLane.Visibility = Visibility.Visible;
                    }
                }, null);

                #endregion

                #region pres detailed

                /* move table marker */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgRhoTableMarker.SetValue(Canvas.TopProperty, 67.0 + plane * 23);
                    pres.imgRhoTableMarker.SetValue(Canvas.LeftProperty, 270.0 + lane * 23);
                    pres.labelRotationOffset.Content = rotationOffset.ToString();
                }, null);

                // 2023-05-08: kopal, esslinger: we changed the datatypes in the following lines from int
                //                               to double, since we got a runtime error, when the values
                //                               were assigned to Canvas.TopProperty and Canvas.LeftProperty
                double recVariableOffset = (rotationOffset % 16 <= 4) ? 0 : (rotationOffset % 16 <= 10) ? 1 : 2;
                double recLeft, recTop;
                recLeft = 8 + (rotationOffset % 16) * 14 + recVariableOffset;
                recTop = 122 + rotationOffset / 16 * 14;

                #region move rectangle which visualizes the rotation for different lane sizes

                switch (z)
                {
                    case 8:
                        /* move rectangle which visualizes the rotation */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.rectangleRhoOldLane.SetValue(Canvas.TopProperty, 73.0);
                            pres.rectangleRhoOldLane.SetValue(Canvas.LeftProperty, 8.0);
                            pres.rectangleRhoNewLane.SetValue(Canvas.TopProperty, recTop);
                            pres.rectangleRhoNewLane.SetValue(Canvas.LeftProperty, recLeft);
                        }, null);

                        #region fill labels of old lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label11.Content = "";
                            pres.label12.Content = "";
                            pres.label13.Content = "";
                            pres.label14.Content = "";
                            pres.label15.Content = "";
                            pres.label16.Content = "";
                            pres.label17.Content = "";
                            pres.label18.Content = "";
                            pres.label19.Content = "";
                            pres.label20.Content = "";
                            pres.label21.Content = "";
                            pres.label22.Content = "";
                            pres.label23.Content = "";
                            pres.label24.Content = "";
                            pres.label25.Content = "";
                            pres.label26.Content = "";

                            pres.label27.Content = "";
                            pres.label28.Content = "";
                            pres.label29.Content = "";
                            pres.label30.Content = "";
                            pres.label31.Content = "";
                            pres.label32.Content = "";
                            pres.label33.Content = "";
                            pres.label34.Content = "";
                            pres.label35.Content = "";
                            pres.label36.Content = "";
                            pres.label37.Content = "";
                            pres.label38.Content = "";
                            pres.label39.Content = "";
                            pres.label40.Content = "";
                            pres.label41.Content = "";
                            pres.label42.Content = "";

                            pres.label43.Content = "";
                            pres.label44.Content = "";
                            pres.label45.Content = "";
                            pres.label46.Content = "";
                            pres.label47.Content = "";
                            pres.label48.Content = "";
                            pres.label49.Content = "";
                            pres.label50.Content = "";
                            pres.label51.Content = "";
                            pres.label52.Content = "";
                            pres.label53.Content = "";
                            pres.label54.Content = "";
                            pres.label55.Content = "";
                            pres.label56.Content = "";
                            pres.label57.Content = "";
                            pres.label58.Content = "";

                            pres.label59.Content = oldLane[0].ToString();
                            pres.label60.Content = oldLane[1].ToString();
                            pres.label61.Content = oldLane[2].ToString();
                            pres.label62.Content = oldLane[3].ToString();
                            pres.label63.Content = oldLane[4].ToString();
                            pres.label64.Content = oldLane[5].ToString();
                            pres.label65.Content = oldLane[6].ToString();
                            pres.label66.Content = oldLane[7].ToString();
                            pres.label67.Content = "";
                            pres.label68.Content = "";
                            pres.label69.Content = "";
                            pres.label70.Content = "";
                            pres.label71.Content = "";
                            pres.label72.Content = "";
                            pres.label73.Content = "";
                            pres.label74.Content = "";
                        }, null);

                        #endregion

                        #region fill labels of new lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label75.Content = newLane[0].ToString();
                            pres.label76.Content = newLane[1].ToString();
                            pres.label77.Content = newLane[2].ToString();
                            pres.label78.Content = newLane[3].ToString();
                            pres.label79.Content = newLane[4].ToString();
                            pres.label80.Content = newLane[5].ToString();
                            pres.label81.Content = newLane[6].ToString();
                            pres.label82.Content = newLane[7].ToString();
                            pres.label83.Content = "";
                            pres.label84.Content = "";
                            pres.label85.Content = "";
                            pres.label86.Content = "";
                            pres.label87.Content = "";
                            pres.label88.Content = "";
                            pres.label89.Content = "";
                            pres.label90.Content = "";

                            pres.label91.Content = "";
                            pres.label92.Content = "";
                            pres.label93.Content = "";
                            pres.label94.Content = "";
                            pres.label95.Content = "";
                            pres.label96.Content = "";
                            pres.label97.Content = "";
                            pres.label98.Content = "";
                            pres.label99.Content = "";
                            pres.label100.Content = "";
                            pres.label101.Content = "";
                            pres.label102.Content = "";
                            pres.label103.Content = "";
                            pres.label104.Content = "";
                            pres.label105.Content = "";
                            pres.label106.Content = "";

                            pres.label107.Content = "";
                            pres.label108.Content = "";
                            pres.label109.Content = "";
                            pres.label110.Content = "";
                            pres.label111.Content = "";
                            pres.label112.Content = "";
                            pres.label113.Content = "";
                            pres.label114.Content = "";
                            pres.label115.Content = "";
                            pres.label116.Content = "";
                            pres.label117.Content = "";
                            pres.label118.Content = "";
                            pres.label119.Content = "";
                            pres.label120.Content = "";
                            pres.label121.Content = "";
                            pres.label122.Content = "";

                            pres.label123.Content = "";
                            pres.label124.Content = "";
                            pres.label125.Content = "";
                            pres.label126.Content = "";
                            pres.label127.Content = "";
                            pres.label128.Content = "";
                            pres.label129.Content = "";
                            pres.label130.Content = "";
                            pres.label131.Content = "";
                            pres.label132.Content = "";
                            pres.label133.Content = "";
                            pres.label134.Content = "";
                            pres.label135.Content = "";
                            pres.label136.Content = "";
                            pres.label137.Content = "";
                            pres.label138.Content = "";
                        }, null);

                        #endregion

                        break;

                    case 16:
                        /* move rectangle which visualizes the rotation */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.rectangleRhoOldLane.SetValue(Canvas.TopProperty, 73.0);
                            pres.rectangleRhoOldLane.SetValue(Canvas.LeftProperty, 8.0);
                            pres.rectangleRhoNewLane.SetValue(Canvas.TopProperty, recTop);
                            pres.rectangleRhoNewLane.SetValue(Canvas.LeftProperty, recLeft);
                        }, null);

                        #region fill labels of old lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label11.Content = "";
                            pres.label12.Content = "";
                            pres.label13.Content = "";
                            pres.label14.Content = "";
                            pres.label15.Content = "";
                            pres.label16.Content = "";
                            pres.label17.Content = "";
                            pres.label18.Content = "";
                            pres.label19.Content = "";
                            pres.label20.Content = "";
                            pres.label21.Content = "";
                            pres.label22.Content = "";
                            pres.label23.Content = "";
                            pres.label24.Content = "";
                            pres.label25.Content = "";
                            pres.label26.Content = "";

                            pres.label27.Content = "";
                            pres.label28.Content = "";
                            pres.label29.Content = "";
                            pres.label30.Content = "";
                            pres.label31.Content = "";
                            pres.label32.Content = "";
                            pres.label33.Content = "";
                            pres.label34.Content = "";
                            pres.label35.Content = "";
                            pres.label36.Content = "";
                            pres.label37.Content = "";
                            pres.label38.Content = "";
                            pres.label39.Content = "";
                            pres.label40.Content = "";
                            pres.label41.Content = "";
                            pres.label42.Content = "";

                            pres.label43.Content = "";
                            pres.label44.Content = "";
                            pres.label45.Content = "";
                            pres.label46.Content = "";
                            pres.label47.Content = "";
                            pres.label48.Content = "";
                            pres.label49.Content = "";
                            pres.label50.Content = "";
                            pres.label51.Content = "";
                            pres.label52.Content = "";
                            pres.label53.Content = "";
                            pres.label54.Content = "";
                            pres.label55.Content = "";
                            pres.label56.Content = "";
                            pres.label57.Content = "";
                            pres.label58.Content = "";

                            pres.label59.Content = oldLane[0].ToString();
                            pres.label60.Content = oldLane[1].ToString();
                            pres.label61.Content = oldLane[2].ToString();
                            pres.label62.Content = oldLane[3].ToString();
                            pres.label63.Content = oldLane[4].ToString();
                            pres.label64.Content = oldLane[5].ToString();
                            pres.label65.Content = oldLane[6].ToString();
                            pres.label66.Content = oldLane[7].ToString();
                            pres.label67.Content = oldLane[8].ToString();
                            pres.label68.Content = oldLane[9].ToString();
                            pres.label69.Content = oldLane[10].ToString();
                            pres.label70.Content = oldLane[11].ToString();
                            pres.label71.Content = oldLane[12].ToString();
                            pres.label72.Content = oldLane[13].ToString();
                            pres.label73.Content = oldLane[14].ToString();
                            pres.label74.Content = oldLane[15].ToString();
                        }, null);

                        #endregion

                        #region fill labels of new lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label75.Content = newLane[0].ToString();
                            pres.label76.Content = newLane[1].ToString();
                            pres.label77.Content = newLane[2].ToString();
                            pres.label78.Content = newLane[3].ToString();
                            pres.label79.Content = newLane[4].ToString();
                            pres.label80.Content = newLane[5].ToString();
                            pres.label81.Content = newLane[6].ToString();
                            pres.label82.Content = newLane[7].ToString();
                            pres.label83.Content = newLane[8].ToString();
                            pres.label84.Content = newLane[9].ToString();
                            pres.label85.Content = newLane[10].ToString();
                            pres.label86.Content = newLane[11].ToString();
                            pres.label87.Content = newLane[12].ToString();
                            pres.label88.Content = newLane[13].ToString();
                            pres.label89.Content = newLane[14].ToString();
                            pres.label90.Content = newLane[15].ToString();

                            pres.label91.Content = "";
                            pres.label92.Content = "";
                            pres.label93.Content = "";
                            pres.label94.Content = "";
                            pres.label95.Content = "";
                            pres.label96.Content = "";
                            pres.label97.Content = "";
                            pres.label98.Content = "";
                            pres.label99.Content = "";
                            pres.label100.Content = "";
                            pres.label101.Content = "";
                            pres.label102.Content = "";
                            pres.label103.Content = "";
                            pres.label104.Content = "";
                            pres.label105.Content = "";
                            pres.label106.Content = "";

                            pres.label107.Content = "";
                            pres.label108.Content = "";
                            pres.label109.Content = "";
                            pres.label110.Content = "";
                            pres.label111.Content = "";
                            pres.label112.Content = "";
                            pres.label113.Content = "";
                            pres.label114.Content = "";
                            pres.label115.Content = "";
                            pres.label116.Content = "";
                            pres.label117.Content = "";
                            pres.label118.Content = "";
                            pres.label119.Content = "";
                            pres.label120.Content = "";
                            pres.label121.Content = "";
                            pres.label122.Content = "";

                            pres.label123.Content = "";
                            pres.label124.Content = "";
                            pres.label125.Content = "";
                            pres.label126.Content = "";
                            pres.label127.Content = "";
                            pres.label128.Content = "";
                            pres.label129.Content = "";
                            pres.label130.Content = "";
                            pres.label131.Content = "";
                            pres.label132.Content = "";
                            pres.label133.Content = "";
                            pres.label134.Content = "";
                            pres.label135.Content = "";
                            pres.label136.Content = "";
                            pres.label137.Content = "";
                            pres.label138.Content = "";
                        }, null);

                        #endregion

                        break;

                    case 32:
                        /* move rectangle which visualizes the rotation */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.rectangleRhoOldLane.SetValue(Canvas.TopProperty, 59.0);
                            pres.rectangleRhoOldLane.SetValue(Canvas.LeftProperty, 8.0);
                            pres.rectangleRhoNewLane.SetValue(Canvas.TopProperty, recTop);
                            pres.rectangleRhoNewLane.SetValue(Canvas.LeftProperty, recLeft);
                        }, null);

                        #region fill labels of old lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label11.Content = "";
                            pres.label12.Content = "";
                            pres.label13.Content = "";
                            pres.label14.Content = "";
                            pres.label15.Content = "";
                            pres.label16.Content = "";
                            pres.label17.Content = "";
                            pres.label18.Content = "";
                            pres.label19.Content = "";
                            pres.label20.Content = "";
                            pres.label21.Content = "";
                            pres.label22.Content = "";
                            pres.label23.Content = "";
                            pres.label24.Content = "";
                            pres.label25.Content = "";
                            pres.label26.Content = "";

                            pres.label27.Content = "";
                            pres.label28.Content = "";
                            pres.label29.Content = "";
                            pres.label30.Content = "";
                            pres.label31.Content = "";
                            pres.label32.Content = "";
                            pres.label33.Content = "";
                            pres.label34.Content = "";
                            pres.label35.Content = "";
                            pres.label36.Content = "";
                            pres.label37.Content = "";
                            pres.label38.Content = "";
                            pres.label39.Content = "";
                            pres.label40.Content = "";
                            pres.label41.Content = "";
                            pres.label42.Content = "";

                            pres.label43.Content = oldLane[0].ToString();
                            pres.label44.Content = oldLane[1].ToString();
                            pres.label45.Content = oldLane[2].ToString();
                            pres.label46.Content = oldLane[3].ToString();
                            pres.label47.Content = oldLane[4].ToString();
                            pres.label48.Content = oldLane[5].ToString();
                            pres.label49.Content = oldLane[6].ToString();
                            pres.label50.Content = oldLane[7].ToString();
                            pres.label51.Content = oldLane[8].ToString();
                            pres.label52.Content = oldLane[9].ToString();
                            pres.label53.Content = oldLane[10].ToString();
                            pres.label54.Content = oldLane[11].ToString();
                            pres.label55.Content = oldLane[12].ToString();
                            pres.label56.Content = oldLane[13].ToString();
                            pres.label57.Content = oldLane[14].ToString();
                            pres.label58.Content = oldLane[15].ToString();

                            pres.label59.Content = oldLane[16].ToString();
                            pres.label60.Content = oldLane[17].ToString();
                            pres.label61.Content = oldLane[18].ToString();
                            pres.label62.Content = oldLane[19].ToString();
                            pres.label63.Content = oldLane[20].ToString();
                            pres.label64.Content = oldLane[21].ToString();
                            pres.label65.Content = oldLane[22].ToString();
                            pres.label66.Content = oldLane[23].ToString();
                            pres.label67.Content = oldLane[24].ToString();
                            pres.label68.Content = oldLane[25].ToString();
                            pres.label69.Content = oldLane[26].ToString();
                            pres.label70.Content = oldLane[27].ToString();
                            pres.label71.Content = oldLane[28].ToString();
                            pres.label72.Content = oldLane[29].ToString();
                            pres.label73.Content = oldLane[30].ToString();
                            pres.label74.Content = oldLane[31].ToString();
                        }, null);

                        #endregion                            

                        #region fill labels of new lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label75.Content = newLane[0].ToString();
                            pres.label76.Content = newLane[1].ToString();
                            pres.label77.Content = newLane[2].ToString();
                            pres.label78.Content = newLane[3].ToString();
                            pres.label79.Content = newLane[4].ToString();
                            pres.label80.Content = newLane[5].ToString();
                            pres.label81.Content = newLane[6].ToString();
                            pres.label82.Content = newLane[7].ToString();
                            pres.label83.Content = newLane[8].ToString();
                            pres.label84.Content = newLane[9].ToString();
                            pres.label85.Content = newLane[10].ToString();
                            pres.label86.Content = newLane[11].ToString();
                            pres.label87.Content = newLane[12].ToString();
                            pres.label88.Content = newLane[13].ToString();
                            pres.label89.Content = newLane[14].ToString();
                            pres.label90.Content = newLane[15].ToString();

                            pres.label91.Content = newLane[16].ToString();
                            pres.label92.Content = newLane[17].ToString();
                            pres.label93.Content = newLane[18].ToString();
                            pres.label94.Content = newLane[19].ToString();
                            pres.label95.Content = newLane[20].ToString();
                            pres.label96.Content = newLane[21].ToString();
                            pres.label97.Content = newLane[22].ToString();
                            pres.label98.Content = newLane[23].ToString();
                            pres.label99.Content = newLane[24].ToString();
                            pres.label100.Content = newLane[25].ToString();
                            pres.label101.Content = newLane[26].ToString();
                            pres.label102.Content = newLane[27].ToString();
                            pres.label103.Content = newLane[28].ToString();
                            pres.label104.Content = newLane[29].ToString();
                            pres.label105.Content = newLane[30].ToString();
                            pres.label106.Content = newLane[31].ToString();

                            pres.label107.Content = "";
                            pres.label108.Content = "";
                            pres.label109.Content = "";
                            pres.label110.Content = "";
                            pres.label111.Content = "";
                            pres.label112.Content = "";
                            pres.label113.Content = "";
                            pres.label114.Content = "";
                            pres.label115.Content = "";
                            pres.label116.Content = "";
                            pres.label117.Content = "";
                            pres.label118.Content = "";
                            pres.label119.Content = "";
                            pres.label120.Content = "";
                            pres.label121.Content = "";
                            pres.label122.Content = "";

                            pres.label123.Content = "";
                            pres.label124.Content = "";
                            pres.label125.Content = "";
                            pres.label126.Content = "";
                            pres.label127.Content = "";
                            pres.label128.Content = "";
                            pres.label129.Content = "";
                            pres.label130.Content = "";
                            pres.label131.Content = "";
                            pres.label132.Content = "";
                            pres.label133.Content = "";
                            pres.label134.Content = "";
                            pres.label135.Content = "";
                            pres.label136.Content = "";
                            pres.label137.Content = "";
                            pres.label138.Content = "";
                        }, null);

                        #endregion                        

                        break;

                    case 64:
                        /* move rectangle which visualizes the rotation */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.rectangleRhoOldLane.SetValue(Canvas.TopProperty, 31.0);
                            pres.rectangleRhoOldLane.SetValue(Canvas.LeftProperty, 8.0);
                            pres.rectangleRhoNewLane.SetValue(Canvas.TopProperty, recTop);
                            pres.rectangleRhoNewLane.SetValue(Canvas.LeftProperty, recLeft);
                        }, null);

                        #region fill labels of old lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label11.Content = oldLane[0].ToString();
                            pres.label12.Content = oldLane[1].ToString();
                            pres.label13.Content = oldLane[2].ToString();
                            pres.label14.Content = oldLane[3].ToString();
                            pres.label15.Content = oldLane[4].ToString();
                            pres.label16.Content = oldLane[5].ToString();
                            pres.label17.Content = oldLane[6].ToString();
                            pres.label18.Content = oldLane[7].ToString();
                            pres.label19.Content = oldLane[8].ToString();
                            pres.label20.Content = oldLane[9].ToString();
                            pres.label21.Content = oldLane[10].ToString();
                            pres.label22.Content = oldLane[11].ToString();
                            pres.label23.Content = oldLane[12].ToString();
                            pres.label24.Content = oldLane[13].ToString();
                            pres.label25.Content = oldLane[14].ToString();
                            pres.label26.Content = oldLane[15].ToString();

                            pres.label27.Content = oldLane[16].ToString();
                            pres.label28.Content = oldLane[17].ToString();
                            pres.label29.Content = oldLane[18].ToString();
                            pres.label30.Content = oldLane[19].ToString();
                            pres.label31.Content = oldLane[20].ToString();
                            pres.label32.Content = oldLane[21].ToString();
                            pres.label33.Content = oldLane[22].ToString();
                            pres.label34.Content = oldLane[23].ToString();
                            pres.label35.Content = oldLane[24].ToString();
                            pres.label36.Content = oldLane[25].ToString();
                            pres.label37.Content = oldLane[26].ToString();
                            pres.label38.Content = oldLane[27].ToString();
                            pres.label39.Content = oldLane[28].ToString();
                            pres.label40.Content = oldLane[29].ToString();
                            pres.label41.Content = oldLane[30].ToString();
                            pres.label42.Content = oldLane[31].ToString();

                            pres.label43.Content = oldLane[32].ToString();
                            pres.label44.Content = oldLane[33].ToString();
                            pres.label45.Content = oldLane[34].ToString();
                            pres.label46.Content = oldLane[35].ToString();
                            pres.label47.Content = oldLane[36].ToString();
                            pres.label48.Content = oldLane[37].ToString();
                            pres.label49.Content = oldLane[38].ToString();
                            pres.label50.Content = oldLane[39].ToString();
                            pres.label51.Content = oldLane[40].ToString();
                            pres.label52.Content = oldLane[41].ToString();
                            pres.label53.Content = oldLane[42].ToString();
                            pres.label54.Content = oldLane[43].ToString();
                            pres.label55.Content = oldLane[44].ToString();
                            pres.label56.Content = oldLane[45].ToString();
                            pres.label57.Content = oldLane[46].ToString();
                            pres.label58.Content = oldLane[47].ToString();

                            pres.label59.Content = oldLane[48].ToString();
                            pres.label60.Content = oldLane[49].ToString();
                            pres.label61.Content = oldLane[50].ToString();
                            pres.label62.Content = oldLane[51].ToString();
                            pres.label63.Content = oldLane[52].ToString();
                            pres.label64.Content = oldLane[53].ToString();
                            pres.label65.Content = oldLane[54].ToString();
                            pres.label66.Content = oldLane[55].ToString();
                            pres.label67.Content = oldLane[56].ToString();
                            pres.label68.Content = oldLane[57].ToString();
                            pres.label69.Content = oldLane[58].ToString();
                            pres.label70.Content = oldLane[59].ToString();
                            pres.label71.Content = oldLane[60].ToString();
                            pres.label72.Content = oldLane[61].ToString();
                            pres.label73.Content = oldLane[62].ToString();
                            pres.label74.Content = oldLane[63].ToString();
                        }, null);

                        #endregion                            

                        #region fill labels of new lane

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label75.Content = newLane[0].ToString();
                            pres.label76.Content = newLane[1].ToString();
                            pres.label77.Content = newLane[2].ToString();
                            pres.label78.Content = newLane[3].ToString();
                            pres.label79.Content = newLane[4].ToString();
                            pres.label80.Content = newLane[5].ToString();
                            pres.label81.Content = newLane[6].ToString();
                            pres.label82.Content = newLane[7].ToString();
                            pres.label83.Content = newLane[8].ToString();
                            pres.label84.Content = newLane[9].ToString();
                            pres.label85.Content = newLane[10].ToString();
                            pres.label86.Content = newLane[11].ToString();
                            pres.label87.Content = newLane[12].ToString();
                            pres.label88.Content = newLane[13].ToString();
                            pres.label89.Content = newLane[14].ToString();
                            pres.label90.Content = newLane[15].ToString();

                            pres.label91.Content = newLane[16].ToString();
                            pres.label92.Content = newLane[17].ToString();
                            pres.label93.Content = newLane[18].ToString();
                            pres.label94.Content = newLane[19].ToString();
                            pres.label95.Content = newLane[20].ToString();
                            pres.label96.Content = newLane[21].ToString();
                            pres.label97.Content = newLane[22].ToString();
                            pres.label98.Content = newLane[23].ToString();
                            pres.label99.Content = newLane[24].ToString();
                            pres.label100.Content = newLane[25].ToString();
                            pres.label101.Content = newLane[26].ToString();
                            pres.label102.Content = newLane[27].ToString();
                            pres.label103.Content = newLane[28].ToString();
                            pres.label104.Content = newLane[29].ToString();
                            pres.label105.Content = newLane[30].ToString();
                            pres.label106.Content = newLane[31].ToString();

                            pres.label107.Content = newLane[32].ToString();
                            pres.label108.Content = newLane[33].ToString();
                            pres.label109.Content = newLane[34].ToString();
                            pres.label110.Content = newLane[35].ToString();
                            pres.label111.Content = newLane[36].ToString();
                            pres.label112.Content = newLane[37].ToString();
                            pres.label113.Content = newLane[38].ToString();
                            pres.label114.Content = newLane[39].ToString();
                            pres.label115.Content = newLane[40].ToString();
                            pres.label116.Content = newLane[41].ToString();
                            pres.label117.Content = newLane[42].ToString();
                            pres.label118.Content = newLane[43].ToString();
                            pres.label119.Content = newLane[44].ToString();
                            pres.label120.Content = newLane[45].ToString();
                            pres.label121.Content = newLane[46].ToString();
                            pres.label122.Content = newLane[47].ToString();

                            pres.label123.Content = newLane[48].ToString();
                            pres.label124.Content = newLane[49].ToString();
                            pres.label125.Content = newLane[50].ToString();
                            pres.label126.Content = newLane[51].ToString();
                            pres.label127.Content = newLane[52].ToString();
                            pres.label128.Content = newLane[53].ToString();
                            pres.label129.Content = newLane[54].ToString();
                            pres.label130.Content = newLane[55].ToString();
                            pres.label131.Content = newLane[56].ToString();
                            pres.label132.Content = newLane[57].ToString();
                            pres.label133.Content = newLane[58].ToString();
                            pres.label134.Content = newLane[59].ToString();
                            pres.label135.Content = newLane[60].ToString();
                            pres.label136.Content = newLane[61].ToString();
                            pres.label137.Content = newLane[62].ToString();
                            pres.label138.Content = newLane[63].ToString();
                        }, null);

                        #endregion                        
                        break;

                    default: break;
                }

                #endregion

                #endregion

                /* wait for button clicks */
                if (!pres.autostep || (plane == Y - 1 && lane == X - 1))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed * 3);       // value adjustable by a slider (slower for rho, since it performs less steps than theta and chi)
                }
            }

            /* hide rho canvas after last iteration */
            if (plane == (Y - 1) && lane == (X - 1))
            {
                if (presActive) // only hide canvas if they are visible
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockCurrentPhase.Text = "";
                        pres.textBlockCurrentStep.Text = "";

                        pres.canvasStepDetailRho.Visibility = Visibility.Hidden;
                        pres.canvasCubeRho.Visibility = Visibility.Hidden;
                        pres.imgCubeDefault.Visibility = Visibility.Hidden;
                        presActive = false;

                        pres.xCoordinate_var.Content = "";
                        pres.yCoordinate_var.Content = "";
                    }, null);
                }
            }
        }

        public void PiPres(byte[][][] tmpLanes)
        {
            if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.buttonAutostep.IsEnabled = true;
                    pres.autostepSpeedSlider.IsEnabled = true;

                    pres.textBlockCurrentPhase.Text = Resources.PresPiPhaseText;
                    pres.textBlockCurrentStep.Text = Resources.PresPiStepText;
                }, null);
            }

            /* presentation is fixed to six rounds and performed after the step mapping of pi */
            for (int i = 0; i < 6; i++)
            {
                if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.xCoordinates.Visibility = Visibility.Visible;
                        pres.yCoordinates.Visibility = Visibility.Visible;
                        pres.PicanvasCubeCoordinates.Visibility = Visibility.Visible;

                        /* set coordinates*/
                        pres.xCoordinate_1.Content = "x=4";
                        pres.xCoordinate_2.Content = "x=5";
                        pres.xCoordinate_3.Content = "x=1";
                        pres.xCoordinate_4.Content = "x=2";
                        pres.xCoordinate_5.Content = "x=3";

                        pres.yCoordinate_1.Content = "y=4";
                        pres.yCoordinate_2.Content = "y=5";
                        pres.yCoordinate_3.Content = "y=1";
                        pres.yCoordinate_4.Content = "y=2";
                        pres.yCoordinate_5.Content = "y=3";

                        #region rounds

                        switch (i)
                        {
                            case 0:
                                pres.canvasCubePi_1.Visibility = Visibility.Visible;
                                pres.imgPiCube_1.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_1.Visibility = Visibility.Visible;
                                pres.imgPiDetailed_1.Visibility = Visibility.Visible;
                                presActive = true;
                                break;

                            case 1:
                                pres.canvasCubePi_1.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_1.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_2.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_2.Visibility = Visibility.Visible;
                                break;

                            case 2:
                                pres.canvasCubePi_2.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_2.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_3.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_3.Visibility = Visibility.Visible;
                                break;

                            case 3:
                                pres.canvasCubePi_3.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_3.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_4.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_4.Visibility = Visibility.Visible;
                                break;

                            case 4:
                                pres.canvasCubePi_4.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_4.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_5.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_5.Visibility = Visibility.Visible;
                                break;

                            case 5:
                                pres.canvasCubePi_5.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_5.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_6.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_6.Visibility = Visibility.Visible;
                                break;

                            default:
                                break;
                        }

                        #endregion

                    }, null);

                    /* wait for button clicks */
                    if (!pres.autostep || i == 5)
                    {
                        pres.autostep = false;
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                    /* sleep between steps, if autostep was clicked */
                    else
                    {
                        System.Threading.Thread.Sleep(pres.autostepSpeed * 8);       // value adjustable by a slider (slower for pi, since it performs only 6 steps)
                    }
                }
            }

            /* hide pi canvas after last iteration */
            if (presActive)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.textBlockCurrentPhase.Text = "";
                    pres.textBlockCurrentStep.Text = "";

                    pres.canvasCubePi_1.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_1.Visibility = Visibility.Hidden;
                    pres.canvasCubePi_2.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_2.Visibility = Visibility.Hidden;
                    pres.canvasCubePi_3.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_3.Visibility = Visibility.Hidden;
                    pres.canvasCubePi_4.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_4.Visibility = Visibility.Hidden;
                    pres.canvasCubePi_5.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_5.Visibility = Visibility.Hidden;
                    pres.canvasCubePi_6.Visibility = Visibility.Hidden;
                    pres.canvasStepDetailPi_6.Visibility = Visibility.Hidden;

                    pres.xCoordinates.Visibility = Visibility.Hidden;
                    pres.yCoordinates.Visibility = Visibility.Hidden;

                    pres.PicanvasCubeCoordinates.Visibility = Visibility.Hidden;

                    /* unset coordinates*/
                    pres.xCoordinate_1.Content = "";
                    pres.xCoordinate_2.Content = "";
                    pres.xCoordinate_3.Content = "";
                    pres.xCoordinate_4.Content = "";
                    pres.xCoordinate_5.Content = "";

                    pres.yCoordinate_1.Content = "";
                    pres.yCoordinate_2.Content = "";
                    pres.yCoordinate_3.Content = "";
                    pres.yCoordinate_4.Content = "";
                    pres.yCoordinate_5.Content = "";

                    presActive = false;
                }, null);
            }
        }

        public void ChiPres(byte[] oldRow, int slice, int row)
        {
            if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
            {
                /* show slice and row indexes */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.buttonAutostep.IsEnabled = true;
                    pres.autostepSpeedSlider.IsEnabled = true;

                    /* show chi canvas in first iteration */
                    if (slice == 0 && row == 0)
                    {
                        pres.textBlockCurrentPhase.Text = Resources.PresChiPhaseText;
                        pres.textBlockCurrentStep.Text = Resources.PresChiStepText;

                        pres.canvasStepDetailChi.Visibility = Visibility.Visible;
                        pres.canvasCubeChi.Visibility = Visibility.Visible;
                        pres.zCoordinates.Visibility = Visibility.Visible;
                        presActive = true;
                    }

                }, null);

                #region pres cube

                switch (z)
                {
                    case 1:
                        /* TODO */
                        break;
                    case 2:
                        /* TODO */
                        break;
                    #region lane size 4

                    case 4:
                        double modifiedRowTop = 155 - row * 26 - slice * 13;
                        double modifiedRowLeft = 137 + slice * 13;

                        double modifiedFirstRowTop = 167 - row * 26;

                        /* show slice and row indexes */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            /* move modified row */
                            pres.imgModifiedRow.SetValue(Canvas.TopProperty, modifiedRowTop);
                            pres.imgModifiedRow.SetValue(Canvas.LeftProperty, modifiedRowLeft);

                            /* move first row and toggle visibility*/
                            if (slice == 0)
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, modifiedFirstRowTop);
                            }
                            else
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;
                            }

                            /* toggle visibility top row */
                            if (row == 4)
                            {
                                pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, modifiedRowTop - 1);
                                pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, modifiedRowLeft - 130);
                                pres.imgModifiedTopRow.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pres.imgModifiedTopRow.Visibility = Visibility.Hidden;
                            }

                        }, null);

                        break;

                    #endregion

                    #region lane size greater than 4

                    case 8:
                    case 16:
                    case 32:
                    case 64:

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            /* show cube */
                            if (slice == 0)
                            {
                                /* show default cube */
                                pres.imgCubeDefault.Visibility = Visibility.Visible;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice + 1);
                                pres.zCoordinate_2.Content = "z=" + (slice + 2);
                                pres.zCoordinate_3.Content = "z=" + (slice + 3);
                                pres.zCoordinate_4.Content = "z=" + (slice + 4);
                            }
                            else if (slice >= 2 && slice < z - 3)
                            {
                                if (slice == 2)
                                {
                                    /* show inner cube */
                                    pres.imgCubeDefault.Visibility = Visibility.Hidden;
                                    pres.imgCubeDefaultInner.Visibility = Visibility.Visible;
                                }

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice);
                                pres.zCoordinate_2.Content = "z=" + (slice + 1);
                                pres.zCoordinate_3.Content = "z=" + (slice + 2);
                                pres.zCoordinate_4.Content = "z=" + (slice + 3);
                            }
                            else if (slice == z - 3)
                            {
                                /* show bottom cube */
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultBottom.Visibility = Visibility.Visible;

                                /* set coordinates */
                                pres.zCoordinate_1.Content = "z=" + (slice);
                                pres.zCoordinate_2.Content = "z=" + (slice + 1);
                                pres.zCoordinate_3.Content = "z=" + (slice + 2);
                                pres.zCoordinate_4.Content = "z=" + (slice + 3);
                            }

                            /* move first row and toggle visibility*/
                            if (slice == 0)
                            {
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, 167.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 155.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 137.0);
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                            }

                            /* move modified row only */
                            else if (slice > 0 && slice < z - 3)
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;

                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 142.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 150.0);
                            }
                            else // slice >= z - 3, last three slices
                            {
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 142.0 - row * 26 - (slice - (z - 3)) * 13);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 150.0 + (slice - (z - 3)) * 13);
                            }

                            /* toggle visibility top row */
                            if (row == 4)
                            {
                                if (slice == 0)
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 8.0);
                                }
                                else if (slice < z - 3)
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 21.0);
                                }
                                else /* last two slices */
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 3)) * 13);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 21.0 + (slice - (z - 3)) * 13);
                                }

                                pres.imgModifiedTopRow.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pres.imgModifiedTopRow.Visibility = Visibility.Hidden;
                            }

                        }, null);

                        break;

                    #endregion

                    default:
                        break;
                }

                #endregion

                #region pres detailed

                /* presentation detailed step */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label1.Content = oldRow[0].ToString();
                    pres.label2.Content = oldRow[1].ToString();
                    pres.label3.Content = oldRow[2].ToString();
                    pres.label4.Content = oldRow[3].ToString();
                    pres.label5.Content = oldRow[4].ToString();

                    pres.label6.Content = rows[slice][row][0].ToString();
                    pres.label7.Content = rows[slice][row][1].ToString();
                    pres.label8.Content = rows[slice][row][2].ToString();
                    pres.label9.Content = rows[slice][row][3].ToString();
                    pres.label10.Content = rows[slice][row][4].ToString();
                }, null);

                #endregion

                /* wait for button clicks */
                if (!pres.autostep || (slice == (z - 1) && row == (Y - 1)))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed);       // value adjustable by a slider
                }
            }

            /* hide chi canvas after last iteration */
            if (presActive)
            {
                if (slice == (z - 1) && row == (Y - 1))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockCurrentPhase.Text = "";
                        pres.textBlockCurrentStep.Text = "";

                        pres.canvasStepDetailChi.Visibility = Visibility.Hidden;
                        pres.canvasCubeChi.Visibility = Visibility.Hidden;
                        pres.imgCubeDefaultBottom.Visibility = Visibility.Hidden;
                        pres.imgCubeDefault.Visibility = Visibility.Hidden;
                        pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                        pres.zCoordinates.Visibility = Visibility.Hidden;
                        presActive = false;
                    }, null);
                }
            }
        }

        public void IotaPres(byte[] firstLane, byte[] firstLaneOld, byte[] truncatedConstant, int round)
        {
            if (pres.IsVisible && !pres.skipPresentation && !pres.skipStep && !pres.skipPermutation)
            {
                AutoResetEvent buttonNextClickedEvent;

                #region toggle visibility of rho images with respect to lane size

                switch (z)
                {
                    case 8:
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.imgStepDetailIota8.Visibility = Visibility.Visible;
                            pres.imgStepDetailIota16.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota32.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota64.Visibility = Visibility.Hidden;

                            pres.labelIotaBitPos1.Content = "";
                            pres.labelIotaBitPos2.Content = "";
                            pres.labelIotaBitPos3.Content = "";
                            pres.labelIotaBitPos4.Content = "";
                            pres.labelIotaBitPos5.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos6.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos7.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos8.Visibility = Visibility.Hidden;
                        }, null);
                        break;

                    case 16:
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.imgStepDetailIota8.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota16.Visibility = Visibility.Visible;
                            pres.imgStepDetailIota32.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota64.Visibility = Visibility.Hidden;

                            pres.labelIotaBitPos1.Content = "";
                            pres.labelIotaBitPos2.Content = "";
                            pres.labelIotaBitPos3.Content = "";
                            pres.labelIotaBitPos4.Content = "";
                            pres.labelIotaBitPos5.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos6.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos7.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos8.Visibility = Visibility.Hidden;
                        }, null);
                        break;

                    case 32:
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.imgStepDetailIota8.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota16.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota32.Visibility = Visibility.Visible;
                            pres.imgStepDetailIota64.Visibility = Visibility.Hidden;

                            pres.labelIotaBitPos1.Content = "";
                            pres.labelIotaBitPos2.Content = "";
                            pres.labelIotaBitPos3.Content = "0";
                            pres.labelIotaBitPos4.Content = "16";
                            pres.labelIotaBitPos5.Visibility = Visibility.Visible;
                            pres.labelIotaBitPos6.Visibility = Visibility.Visible;
                            pres.labelIotaBitPos7.Visibility = Visibility.Hidden;
                            pres.labelIotaBitPos8.Visibility = Visibility.Hidden;
                        }, null);
                        break;

                    case 64:
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.imgStepDetailIota8.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota16.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota32.Visibility = Visibility.Hidden;
                            pres.imgStepDetailIota64.Visibility = Visibility.Visible;

                            pres.labelIotaBitPos1.Content = "0";
                            pres.labelIotaBitPos2.Content = "16";
                            pres.labelIotaBitPos3.Content = "32";
                            pres.labelIotaBitPos4.Content = "48";
                            pres.labelIotaBitPos5.Visibility = Visibility.Visible;
                            pres.labelIotaBitPos6.Visibility = Visibility.Visible;
                            pres.labelIotaBitPos7.Visibility = Visibility.Visible;
                            pres.labelIotaBitPos8.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    default: break;

                }

                #endregion

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.buttonAutostep.IsEnabled = true;
                    pres.autostepSpeedSlider.IsEnabled = true;

                    pres.textBlockCurrentPhase.Text = Resources.PresIotaPhaseText;
                    pres.textBlockCurrentStep.Text = Resources.PresIotaStepText;

                    /* show iota canvas */
                    pres.imgIotaSelectedRC.Visibility = Visibility.Hidden;
                    pres.imgIotaXORedRC.Visibility = Visibility.Hidden;
                    pres.labelIotaXORedRC.Content = "";
                    pres.canvasStepDetailIota.Visibility = Visibility.Visible;
                    pres.canvasCubeIota.Visibility = Visibility.Visible;
                    pres.imgCubeDefault.Visibility = Visibility.Visible;

                    pres.xCoordinates.Visibility = Visibility.Visible;
                    pres.yCoordinates.Visibility = Visibility.Visible;

                    /* set coordinates*/
                    pres.xCoordinate_1.Content = "x=1";
                    pres.yCoordinate_1.Content = "y=1";

                    presActive = true;
                }, null);

                #region fill old lane labels for different lane sizes

                switch (z)
                {
                    case 1:
                        /* TODO */
                        break;
                    case 2:
                        /* TODO */
                        break;
                    case 4:
                        /* TODO */
                        break;
                    case 8:
                        #region fill labels of old lane
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label311.Content = "";
                            pres.label312.Content = "";
                            pres.label313.Content = "";
                            pres.label314.Content = "";
                            pres.label315.Content = "";
                            pres.label316.Content = "";
                            pres.label317.Content = "";
                            pres.label318.Content = "";
                            pres.label319.Content = "";
                            pres.label320.Content = "";
                            pres.label321.Content = "";
                            pres.label322.Content = "";
                            pres.label323.Content = "";
                            pres.label324.Content = "";
                            pres.label325.Content = "";
                            pres.label326.Content = "";

                            pres.label327.Content = "";
                            pres.label328.Content = "";
                            pres.label329.Content = "";
                            pres.label330.Content = "";
                            pres.label331.Content = "";
                            pres.label332.Content = "";
                            pres.label333.Content = "";
                            pres.label334.Content = "";
                            pres.label335.Content = "";
                            pres.label336.Content = "";
                            pres.label337.Content = "";
                            pres.label338.Content = "";
                            pres.label339.Content = "";
                            pres.label340.Content = "";
                            pres.label341.Content = "";
                            pres.label342.Content = "";

                            pres.label343.Content = "";
                            pres.label344.Content = "";
                            pres.label345.Content = "";
                            pres.label346.Content = "";
                            pres.label347.Content = "";
                            pres.label348.Content = "";
                            pres.label349.Content = "";
                            pres.label350.Content = "";
                            pres.label351.Content = "";
                            pres.label352.Content = "";
                            pres.label353.Content = "";
                            pres.label354.Content = "";
                            pres.label355.Content = "";
                            pres.label356.Content = "";
                            pres.label357.Content = "";
                            pres.label358.Content = "";

                            pres.label359.Content = "";
                            pres.label360.Content = "";
                            pres.label361.Content = "";
                            pres.label362.Content = "";
                            pres.label363.Content = firstLaneOld[0].ToString();
                            pres.label364.Content = firstLaneOld[1].ToString();
                            pres.label365.Content = firstLaneOld[2].ToString();
                            pres.label366.Content = firstLaneOld[3].ToString();
                            pres.label367.Content = firstLaneOld[4].ToString();
                            pres.label368.Content = firstLaneOld[5].ToString();
                            pres.label369.Content = firstLaneOld[6].ToString();
                            pres.label370.Content = firstLaneOld[7].ToString();
                            pres.label371.Content = "";
                            pres.label372.Content = "";
                            pres.label373.Content = "";
                            pres.label374.Content = "";
                        }, null);
                        #endregion    
                        break;
                    case 16:
                        #region fill labels of old lane
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label311.Content = "";
                            pres.label312.Content = "";
                            pres.label313.Content = "";
                            pres.label314.Content = "";
                            pres.label315.Content = "";
                            pres.label316.Content = "";
                            pres.label317.Content = "";
                            pres.label318.Content = "";
                            pres.label319.Content = "";
                            pres.label320.Content = "";
                            pres.label321.Content = "";
                            pres.label322.Content = "";
                            pres.label323.Content = "";
                            pres.label324.Content = "";
                            pres.label325.Content = "";
                            pres.label326.Content = "";

                            pres.label327.Content = "";
                            pres.label328.Content = "";
                            pres.label329.Content = "";
                            pres.label330.Content = "";
                            pres.label331.Content = "";
                            pres.label332.Content = "";
                            pres.label333.Content = "";
                            pres.label334.Content = "";
                            pres.label335.Content = "";
                            pres.label336.Content = "";
                            pres.label337.Content = "";
                            pres.label338.Content = "";
                            pres.label339.Content = "";
                            pres.label340.Content = "";
                            pres.label341.Content = "";
                            pres.label342.Content = "";

                            pres.label343.Content = "";
                            pres.label344.Content = "";
                            pres.label345.Content = "";
                            pres.label346.Content = "";
                            pres.label347.Content = "";
                            pres.label348.Content = "";
                            pres.label349.Content = "";
                            pres.label350.Content = "";
                            pres.label351.Content = "";
                            pres.label352.Content = "";
                            pres.label353.Content = "";
                            pres.label354.Content = "";
                            pres.label355.Content = "";
                            pres.label356.Content = "";
                            pres.label357.Content = "";
                            pres.label358.Content = "";

                            pres.label359.Content = firstLaneOld[0].ToString();
                            pres.label360.Content = firstLaneOld[1].ToString();
                            pres.label361.Content = firstLaneOld[2].ToString();
                            pres.label362.Content = firstLaneOld[3].ToString();
                            pres.label363.Content = firstLaneOld[4].ToString();
                            pres.label364.Content = firstLaneOld[5].ToString();
                            pres.label365.Content = firstLaneOld[6].ToString();
                            pres.label366.Content = firstLaneOld[7].ToString();
                            pres.label367.Content = firstLaneOld[8].ToString();
                            pres.label368.Content = firstLaneOld[9].ToString();
                            pres.label369.Content = firstLaneOld[10].ToString();
                            pres.label370.Content = firstLaneOld[11].ToString();
                            pres.label371.Content = firstLaneOld[12].ToString();
                            pres.label372.Content = firstLaneOld[13].ToString();
                            pres.label373.Content = firstLaneOld[14].ToString();
                            pres.label374.Content = firstLaneOld[15].ToString();
                        }, null);
                        #endregion    
                        break;
                    case 32:
                        #region fill labels of old lane
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label311.Content = "";
                            pres.label312.Content = "";
                            pres.label313.Content = "";
                            pres.label314.Content = "";
                            pres.label315.Content = "";
                            pres.label316.Content = "";
                            pres.label317.Content = "";
                            pres.label318.Content = "";
                            pres.label319.Content = "";
                            pres.label320.Content = "";
                            pres.label321.Content = "";
                            pres.label322.Content = "";
                            pres.label323.Content = "";
                            pres.label324.Content = "";
                            pres.label325.Content = "";
                            pres.label326.Content = "";

                            pres.label327.Content = "";
                            pres.label328.Content = "";
                            pres.label329.Content = "";
                            pres.label330.Content = "";
                            pres.label331.Content = "";
                            pres.label332.Content = "";
                            pres.label333.Content = "";
                            pres.label334.Content = "";
                            pres.label335.Content = "";
                            pres.label336.Content = "";
                            pres.label337.Content = "";
                            pres.label338.Content = "";
                            pres.label339.Content = "";
                            pres.label340.Content = "";
                            pres.label341.Content = "";
                            pres.label342.Content = "";

                            pres.label343.Content = firstLaneOld[0].ToString();
                            pres.label344.Content = firstLaneOld[1].ToString();
                            pres.label345.Content = firstLaneOld[2].ToString();
                            pres.label346.Content = firstLaneOld[3].ToString();
                            pres.label347.Content = firstLaneOld[4].ToString();
                            pres.label348.Content = firstLaneOld[5].ToString();
                            pres.label349.Content = firstLaneOld[6].ToString();
                            pres.label350.Content = firstLaneOld[7].ToString();
                            pres.label351.Content = firstLaneOld[8].ToString();
                            pres.label352.Content = firstLaneOld[9].ToString();
                            pres.label353.Content = firstLaneOld[10].ToString();
                            pres.label354.Content = firstLaneOld[11].ToString();
                            pres.label355.Content = firstLaneOld[12].ToString();
                            pres.label356.Content = firstLaneOld[13].ToString();
                            pres.label357.Content = firstLaneOld[14].ToString();
                            pres.label358.Content = firstLaneOld[15].ToString();

                            pres.label359.Content = firstLaneOld[16].ToString();
                            pres.label360.Content = firstLaneOld[17].ToString();
                            pres.label361.Content = firstLaneOld[18].ToString();
                            pres.label362.Content = firstLaneOld[19].ToString();
                            pres.label363.Content = firstLaneOld[20].ToString();
                            pres.label364.Content = firstLaneOld[21].ToString();
                            pres.label365.Content = firstLaneOld[22].ToString();
                            pres.label366.Content = firstLaneOld[23].ToString();
                            pres.label367.Content = firstLaneOld[24].ToString();
                            pres.label368.Content = firstLaneOld[25].ToString();
                            pres.label369.Content = firstLaneOld[26].ToString();
                            pres.label370.Content = firstLaneOld[27].ToString();
                            pres.label371.Content = firstLaneOld[28].ToString();
                            pres.label372.Content = firstLaneOld[29].ToString();
                            pres.label373.Content = firstLaneOld[30].ToString();
                            pres.label374.Content = firstLaneOld[31].ToString();
                        }, null);
                        #endregion    
                        break;
                    case 64:
                        #region fill labels of old lane
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.label311.Content = firstLaneOld[0].ToString();
                            pres.label312.Content = firstLaneOld[1].ToString();
                            pres.label313.Content = firstLaneOld[2].ToString();
                            pres.label314.Content = firstLaneOld[3].ToString();
                            pres.label315.Content = firstLaneOld[4].ToString();
                            pres.label316.Content = firstLaneOld[5].ToString();
                            pres.label317.Content = firstLaneOld[6].ToString();
                            pres.label318.Content = firstLaneOld[7].ToString();
                            pres.label319.Content = firstLaneOld[8].ToString();
                            pres.label320.Content = firstLaneOld[9].ToString();
                            pres.label321.Content = firstLaneOld[10].ToString();
                            pres.label322.Content = firstLaneOld[11].ToString();
                            pres.label323.Content = firstLaneOld[12].ToString();
                            pres.label324.Content = firstLaneOld[13].ToString();
                            pres.label325.Content = firstLaneOld[14].ToString();
                            pres.label326.Content = firstLaneOld[15].ToString();

                            pres.label327.Content = firstLaneOld[16].ToString();
                            pres.label328.Content = firstLaneOld[17].ToString();
                            pres.label329.Content = firstLaneOld[18].ToString();
                            pres.label330.Content = firstLaneOld[19].ToString();
                            pres.label331.Content = firstLaneOld[20].ToString();
                            pres.label332.Content = firstLaneOld[21].ToString();
                            pres.label333.Content = firstLaneOld[22].ToString();
                            pres.label334.Content = firstLaneOld[23].ToString();
                            pres.label335.Content = firstLaneOld[24].ToString();
                            pres.label336.Content = firstLaneOld[25].ToString();
                            pres.label337.Content = firstLaneOld[26].ToString();
                            pres.label338.Content = firstLaneOld[27].ToString();
                            pres.label339.Content = firstLaneOld[28].ToString();
                            pres.label340.Content = firstLaneOld[29].ToString();
                            pres.label341.Content = firstLaneOld[30].ToString();
                            pres.label342.Content = firstLaneOld[31].ToString();

                            pres.label343.Content = firstLaneOld[32].ToString();
                            pres.label344.Content = firstLaneOld[33].ToString();
                            pres.label345.Content = firstLaneOld[34].ToString();
                            pres.label346.Content = firstLaneOld[35].ToString();
                            pres.label347.Content = firstLaneOld[36].ToString();
                            pres.label348.Content = firstLaneOld[37].ToString();
                            pres.label349.Content = firstLaneOld[38].ToString();
                            pres.label350.Content = firstLaneOld[39].ToString();
                            pres.label351.Content = firstLaneOld[40].ToString();
                            pres.label352.Content = firstLaneOld[41].ToString();
                            pres.label353.Content = firstLaneOld[42].ToString();
                            pres.label354.Content = firstLaneOld[43].ToString();
                            pres.label355.Content = firstLaneOld[44].ToString();
                            pres.label356.Content = firstLaneOld[45].ToString();
                            pres.label357.Content = firstLaneOld[46].ToString();
                            pres.label358.Content = firstLaneOld[47].ToString();

                            pres.label359.Content = firstLaneOld[48].ToString();
                            pres.label360.Content = firstLaneOld[49].ToString();
                            pres.label361.Content = firstLaneOld[50].ToString();
                            pres.label362.Content = firstLaneOld[51].ToString();
                            pres.label363.Content = firstLaneOld[52].ToString();
                            pres.label364.Content = firstLaneOld[53].ToString();
                            pres.label365.Content = firstLaneOld[54].ToString();
                            pres.label366.Content = firstLaneOld[55].ToString();
                            pres.label367.Content = firstLaneOld[56].ToString();
                            pres.label368.Content = firstLaneOld[57].ToString();
                            pres.label369.Content = firstLaneOld[58].ToString();
                            pres.label370.Content = firstLaneOld[59].ToString();
                            pres.label371.Content = firstLaneOld[60].ToString();
                            pres.label372.Content = firstLaneOld[61].ToString();
                            pres.label373.Content = firstLaneOld[62].ToString();
                            pres.label374.Content = firstLaneOld[63].ToString();
                        }, null);
                        #endregion    
                        break;
                    default:
                        break;
                }
                #endregion

                #region clear new lane labels
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label375.Content = "";
                    pres.label376.Content = "";
                    pres.label377.Content = "";
                    pres.label378.Content = "";
                    pres.label379.Content = "";
                    pres.label380.Content = "";
                    pres.label381.Content = "";
                    pres.label382.Content = "";
                    pres.label383.Content = "";
                    pres.label384.Content = "";
                    pres.label385.Content = "";
                    pres.label386.Content = "";
                    pres.label387.Content = "";
                    pres.label388.Content = "";
                    pres.label389.Content = "";
                    pres.label390.Content = "";

                    pres.label391.Content = "";
                    pres.label392.Content = "";
                    pres.label393.Content = "";
                    pres.label394.Content = "";
                    pres.label395.Content = "";
                    pres.label396.Content = "";
                    pres.label397.Content = "";
                    pres.label398.Content = "";
                    pres.label399.Content = "";
                    pres.label400.Content = "";
                    pres.label401.Content = "";
                    pres.label402.Content = "";
                    pres.label403.Content = "";
                    pres.label404.Content = "";
                    pres.label405.Content = "";
                    pres.label406.Content = "";

                    pres.label407.Content = "";
                    pres.label408.Content = "";
                    pres.label409.Content = "";
                    pres.label410.Content = "";
                    pres.label411.Content = "";
                    pres.label412.Content = "";
                    pres.label413.Content = "";
                    pres.label414.Content = "";
                    pres.label415.Content = "";
                    pres.label416.Content = "";
                    pres.label417.Content = "";
                    pres.label418.Content = "";
                    pres.label419.Content = "";
                    pres.label420.Content = "";
                    pres.label421.Content = "";
                    pres.label422.Content = "";

                    pres.label423.Content = "";
                    pres.label424.Content = "";
                    pres.label425.Content = "";
                    pres.label426.Content = "";
                    pres.label427.Content = "";
                    pres.label428.Content = "";
                    pres.label429.Content = "";
                    pres.label430.Content = "";
                    pres.label431.Content = "";
                    pres.label432.Content = "";
                    pres.label433.Content = "";
                    pres.label434.Content = "";
                    pres.label435.Content = "";
                    pres.label436.Content = "";
                    pres.label437.Content = "";
                    pres.label438.Content = "";
                }, null);

                #endregion

                /* wait for button clicks */
                if (!pres.skipPresentation && !pres.skipStep)
                {
                    if (!pres.autostep)
                    {
                        pres.autostep = false;
                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                    /* sleep between steps, if autostep was clicked */
                    else
                    {
                        System.Threading.Thread.Sleep(pres.autostepSpeed * 8);       // value adjustable by a slider
                    }
                }

                double topProperty = 36.0 + (round % 12) * 13;
                double leftProperty = 262.0 + (round / 12) * 55;
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgIotaSelectedRC.SetValue(Canvas.TopProperty, topProperty);
                    pres.imgIotaSelectedRC.SetValue(Canvas.LeftProperty, leftProperty);
                    pres.imgIotaSelectedRC.Visibility = Visibility.Visible;

                }, null);

                /* wait for button clicks */
                if (!pres.skipPresentation && !pres.skipStep)
                {
                    if (!pres.autostep)
                    {
                        pres.autostep = false;
                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                    /* sleep between steps, if autostep was clicked */
                    else
                    {
                        System.Threading.Thread.Sleep(pres.autostepSpeed * 8);       // value adjustable by a slider
                    }

                    string roundConstantPres = roundConstantsPres[round];
                    string roundConstantPresTruncated = null;
                    double widthImgIotaXORedRC = 0.0;

                    #region truncate round constant string

                    switch (z)
                    {
                        case 8:
                            roundConstantPresTruncated = "0x" + roundConstantPres.Substring(2 + 8 + 4 + 2, 2);
                            widthImgIotaXORedRC = 35.0;
                            break;

                        case 16:
                            roundConstantPresTruncated = "0x" + roundConstantPres.Substring(2 + 8 + 4, 4);
                            widthImgIotaXORedRC = 46.0;
                            break;

                        case 32:
                            roundConstantPresTruncated = "0x" + roundConstantPres.Substring(2 + 8, 8);
                            widthImgIotaXORedRC = 70.0;
                            break;

                        case 64:
                            roundConstantPresTruncated = roundConstantPres;
                            widthImgIotaXORedRC = 118.0;
                            break;

                        default:
                            break;
                    }

                    #endregion

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIotaXORedRC.Width = widthImgIotaXORedRC;
                        pres.imgIotaXORedRC.Visibility = Visibility.Visible;
                        pres.labelIotaXORedRC.Content = roundConstantPresTruncated;
                    }, null);
                }

                /* wait for button clicks */
                if (!pres.skipPresentation && !pres.skipStep)
                {
                    if (!pres.autostep)
                    {
                        pres.autostep = false;
                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                    /* sleep between steps, if autostep was clicked */
                    else
                    {
                        System.Threading.Thread.Sleep(pres.autostepSpeed * 8);       // value adjustable by a slider
                    }

                    #region fill new lane labels for different lane sizes

                    switch (z)
                    {
                        case 1:
                            /* TODO */
                            break;
                        case 2:
                            /* TODO */
                            break;
                        case 4:
                            /* TODO */
                            break;
                        case 8:
                            #region fill labels of new lane
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.label375.Content = "";
                                pres.label376.Content = "";
                                pres.label377.Content = "";
                                pres.label378.Content = "";
                                pres.label379.Content = firstLane[0].ToString();
                                pres.label380.Content = firstLane[1].ToString();
                                pres.label381.Content = firstLane[2].ToString();
                                pres.label382.Content = firstLane[3].ToString();
                                pres.label383.Content = firstLane[4].ToString();
                                pres.label384.Content = firstLane[5].ToString();
                                pres.label385.Content = firstLane[6].ToString();
                                pres.label386.Content = firstLane[7].ToString();
                                pres.label387.Content = "";
                                pres.label388.Content = "";
                                pres.label389.Content = "";
                                pres.label390.Content = "";

                                pres.label391.Content = "";
                                pres.label392.Content = "";
                                pres.label393.Content = "";
                                pres.label394.Content = "";
                                pres.label395.Content = "";
                                pres.label396.Content = "";
                                pres.label397.Content = "";
                                pres.label398.Content = "";
                                pres.label399.Content = "";
                                pres.label400.Content = "";
                                pres.label401.Content = "";
                                pres.label402.Content = "";
                                pres.label403.Content = "";
                                pres.label404.Content = "";
                                pres.label405.Content = "";
                                pres.label406.Content = "";

                                pres.label407.Content = "";
                                pres.label408.Content = "";
                                pres.label409.Content = "";
                                pres.label410.Content = "";
                                pres.label411.Content = "";
                                pres.label412.Content = "";
                                pres.label413.Content = "";
                                pres.label414.Content = "";
                                pres.label415.Content = "";
                                pres.label416.Content = "";
                                pres.label417.Content = "";
                                pres.label418.Content = "";
                                pres.label419.Content = "";
                                pres.label420.Content = "";
                                pres.label421.Content = "";
                                pres.label422.Content = "";

                                pres.label423.Content = "";
                                pres.label424.Content = "";
                                pres.label425.Content = "";
                                pres.label426.Content = "";
                                pres.label427.Content = "";
                                pres.label428.Content = "";
                                pres.label429.Content = "";
                                pres.label430.Content = "";
                                pres.label431.Content = "";
                                pres.label432.Content = "";
                                pres.label433.Content = "";
                                pres.label434.Content = "";
                                pres.label435.Content = "";
                                pres.label436.Content = "";
                                pres.label437.Content = "";
                                pres.label438.Content = "";
                            }, null);

                            #endregion
                            break;
                        case 16:
                            #region fill labels of new lane
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.label375.Content = firstLane[0].ToString();
                                pres.label376.Content = firstLane[1].ToString();
                                pres.label377.Content = firstLane[2].ToString();
                                pres.label378.Content = firstLane[3].ToString();
                                pres.label379.Content = firstLane[4].ToString();
                                pres.label380.Content = firstLane[5].ToString();
                                pres.label381.Content = firstLane[6].ToString();
                                pres.label382.Content = firstLane[7].ToString();
                                pres.label383.Content = firstLane[8].ToString();
                                pres.label384.Content = firstLane[9].ToString();
                                pres.label385.Content = firstLane[10].ToString();
                                pres.label386.Content = firstLane[11].ToString();
                                pres.label387.Content = firstLane[12].ToString();
                                pres.label388.Content = firstLane[13].ToString();
                                pres.label389.Content = firstLane[14].ToString();
                                pres.label390.Content = firstLane[15].ToString();

                                pres.label391.Content = "";
                                pres.label392.Content = "";
                                pres.label393.Content = "";
                                pres.label394.Content = "";
                                pres.label395.Content = "";
                                pres.label396.Content = "";
                                pres.label397.Content = "";
                                pres.label398.Content = "";
                                pres.label399.Content = "";
                                pres.label400.Content = "";
                                pres.label401.Content = "";
                                pres.label402.Content = "";
                                pres.label403.Content = "";
                                pres.label404.Content = "";
                                pres.label405.Content = "";
                                pres.label406.Content = "";

                                pres.label407.Content = "";
                                pres.label408.Content = "";
                                pres.label409.Content = "";
                                pres.label410.Content = "";
                                pres.label411.Content = "";
                                pres.label412.Content = "";
                                pres.label413.Content = "";
                                pres.label414.Content = "";
                                pres.label415.Content = "";
                                pres.label416.Content = "";
                                pres.label417.Content = "";
                                pres.label418.Content = "";
                                pres.label419.Content = "";
                                pres.label420.Content = "";
                                pres.label421.Content = "";
                                pres.label422.Content = "";

                                pres.label423.Content = "";
                                pres.label424.Content = "";
                                pres.label425.Content = "";
                                pres.label426.Content = "";
                                pres.label427.Content = "";
                                pres.label428.Content = "";
                                pres.label429.Content = "";
                                pres.label430.Content = "";
                                pres.label431.Content = "";
                                pres.label432.Content = "";
                                pres.label433.Content = "";
                                pres.label434.Content = "";
                                pres.label435.Content = "";
                                pres.label436.Content = "";
                                pres.label437.Content = "";
                                pres.label438.Content = "";
                            }, null);

                            #endregion
                            break;
                        case 32:
                            #region fill labels of new lane
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.label375.Content = firstLane[0].ToString();
                                pres.label376.Content = firstLane[1].ToString();
                                pres.label377.Content = firstLane[2].ToString();
                                pres.label378.Content = firstLane[3].ToString();
                                pres.label379.Content = firstLane[4].ToString();
                                pres.label380.Content = firstLane[5].ToString();
                                pres.label381.Content = firstLane[6].ToString();
                                pres.label382.Content = firstLane[7].ToString();
                                pres.label383.Content = firstLane[8].ToString();
                                pres.label384.Content = firstLane[9].ToString();
                                pres.label385.Content = firstLane[10].ToString();
                                pres.label386.Content = firstLane[11].ToString();
                                pres.label387.Content = firstLane[12].ToString();
                                pres.label388.Content = firstLane[13].ToString();
                                pres.label389.Content = firstLane[14].ToString();
                                pres.label390.Content = firstLane[15].ToString();

                                pres.label391.Content = firstLane[16].ToString();
                                pres.label392.Content = firstLane[17].ToString();
                                pres.label393.Content = firstLane[18].ToString();
                                pres.label394.Content = firstLane[19].ToString();
                                pres.label395.Content = firstLane[20].ToString();
                                pres.label396.Content = firstLane[21].ToString();
                                pres.label397.Content = firstLane[22].ToString();
                                pres.label398.Content = firstLane[23].ToString();
                                pres.label399.Content = firstLane[24].ToString();
                                pres.label400.Content = firstLane[25].ToString();
                                pres.label401.Content = firstLane[26].ToString();
                                pres.label402.Content = firstLane[27].ToString();
                                pres.label403.Content = firstLane[28].ToString();
                                pres.label404.Content = firstLane[29].ToString();
                                pres.label405.Content = firstLane[30].ToString();
                                pres.label406.Content = firstLane[31].ToString();

                                pres.label407.Content = "";
                                pres.label408.Content = "";
                                pres.label409.Content = "";
                                pres.label410.Content = "";
                                pres.label411.Content = "";
                                pres.label412.Content = "";
                                pres.label413.Content = "";
                                pres.label414.Content = "";
                                pres.label415.Content = "";
                                pres.label416.Content = "";
                                pres.label417.Content = "";
                                pres.label418.Content = "";
                                pres.label419.Content = "";
                                pres.label420.Content = "";
                                pres.label421.Content = "";
                                pres.label422.Content = "";

                                pres.label423.Content = "";
                                pres.label424.Content = "";
                                pres.label425.Content = "";
                                pres.label426.Content = "";
                                pres.label427.Content = "";
                                pres.label428.Content = "";
                                pres.label429.Content = "";
                                pres.label430.Content = "";
                                pres.label431.Content = "";
                                pres.label432.Content = "";
                                pres.label433.Content = "";
                                pres.label434.Content = "";
                                pres.label435.Content = "";
                                pres.label436.Content = "";
                                pres.label437.Content = "";
                                pres.label438.Content = "";
                            }, null);

                            #endregion
                            break;
                        case 64:
                            #region fill labels of new lane
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.label375.Content = firstLane[0].ToString();
                                pres.label376.Content = firstLane[1].ToString();
                                pres.label377.Content = firstLane[2].ToString();
                                pres.label378.Content = firstLane[3].ToString();
                                pres.label379.Content = firstLane[4].ToString();
                                pres.label380.Content = firstLane[5].ToString();
                                pres.label381.Content = firstLane[6].ToString();
                                pres.label382.Content = firstLane[7].ToString();
                                pres.label383.Content = firstLane[8].ToString();
                                pres.label384.Content = firstLane[9].ToString();
                                pres.label385.Content = firstLane[10].ToString();
                                pres.label386.Content = firstLane[11].ToString();
                                pres.label387.Content = firstLane[12].ToString();
                                pres.label388.Content = firstLane[13].ToString();
                                pres.label389.Content = firstLane[14].ToString();
                                pres.label390.Content = firstLane[15].ToString();

                                pres.label391.Content = firstLane[16].ToString();
                                pres.label392.Content = firstLane[17].ToString();
                                pres.label393.Content = firstLane[18].ToString();
                                pres.label394.Content = firstLane[19].ToString();
                                pres.label395.Content = firstLane[20].ToString();
                                pres.label396.Content = firstLane[21].ToString();
                                pres.label397.Content = firstLane[22].ToString();
                                pres.label398.Content = firstLane[23].ToString();
                                pres.label399.Content = firstLane[24].ToString();
                                pres.label400.Content = firstLane[25].ToString();
                                pres.label401.Content = firstLane[26].ToString();
                                pres.label402.Content = firstLane[27].ToString();
                                pres.label403.Content = firstLane[28].ToString();
                                pres.label404.Content = firstLane[29].ToString();
                                pres.label405.Content = firstLane[30].ToString();
                                pres.label406.Content = firstLane[31].ToString();

                                pres.label407.Content = firstLane[32].ToString();
                                pres.label408.Content = firstLane[33].ToString();
                                pres.label409.Content = firstLane[34].ToString();
                                pres.label410.Content = firstLane[35].ToString();
                                pres.label411.Content = firstLane[36].ToString();
                                pres.label412.Content = firstLane[37].ToString();
                                pres.label413.Content = firstLane[38].ToString();
                                pres.label414.Content = firstLane[39].ToString();
                                pres.label415.Content = firstLane[40].ToString();
                                pres.label416.Content = firstLane[41].ToString();
                                pres.label417.Content = firstLane[42].ToString();
                                pres.label418.Content = firstLane[43].ToString();
                                pres.label419.Content = firstLane[44].ToString();
                                pres.label420.Content = firstLane[45].ToString();
                                pres.label421.Content = firstLane[46].ToString();
                                pres.label422.Content = firstLane[47].ToString();

                                pres.label423.Content = firstLane[48].ToString();
                                pres.label424.Content = firstLane[49].ToString();
                                pres.label425.Content = firstLane[50].ToString();
                                pres.label426.Content = firstLane[51].ToString();
                                pres.label427.Content = firstLane[52].ToString();
                                pres.label428.Content = firstLane[53].ToString();
                                pres.label429.Content = firstLane[54].ToString();
                                pres.label430.Content = firstLane[55].ToString();
                                pres.label431.Content = firstLane[56].ToString();
                                pres.label432.Content = firstLane[57].ToString();
                                pres.label433.Content = firstLane[58].ToString();
                                pres.label434.Content = firstLane[59].ToString();
                                pres.label435.Content = firstLane[60].ToString();
                                pres.label436.Content = firstLane[61].ToString();
                                pres.label437.Content = firstLane[62].ToString();
                                pres.label438.Content = firstLane[63].ToString();
                            }, null);

                            #endregion
                            break;
                        default:
                            break;
                    }
                    #endregion

                    if (!pres.skipPresentation && !pres.skipStep)
                    {
                        /* wait for button clicks */
                        pres.autostep = false;
                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                }
            }

            /* hide iota canvas after last iteration */
            if (presActive)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.textBlockCurrentPhase.Text = "";
                    pres.textBlockCurrentStep.Text = "";

                    pres.canvasStepDetailIota.Visibility = Visibility.Hidden;
                    pres.canvasCubeIota.Visibility = Visibility.Hidden;
                    pres.imgCubeDefault.Visibility = Visibility.Hidden;

                    pres.xCoordinates.Visibility = Visibility.Hidden;
                    pres.yCoordinates.Visibility = Visibility.Hidden;

                    /* unset coordinates*/
                    pres.xCoordinate_1.Content = "";
                    pres.yCoordinate_1.Content = "";

                    presActive = false;
                }, null);
            }
        }

        #endregion

        #region helper methods

        /* returns the first lane from state (needed by iota step) */
        public byte[] GetFirstLaneFromState(ref byte[] state)
        {
            return KeccakHashFunction.SubArray(state, 0, z);
        }

        /* sets the first lane of the state (needed by iota step) */
        public void SetFirstLaneToState(ref byte[] state, byte[] firstLane)
        {
            Array.Copy(firstLane, 0, state, 0, z);
        }

        /**
        * transforms the state to a lane-wise representation of the state 
        * */
        public void GetLanesFromState(ref byte[] state)
        {
            lanes = new byte[Y][][];
            for (int i = 0; i < Y; i++)         // iterate over y coordinate
            {
                lanes[i] = new byte[X][];
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    lanes[i][j] = new byte[z];
                    lanes[i][j] = KeccakHashFunction.SubArray(state, (i * 5 + j) * z, z);
                }
            }
        }

        /**
            * sets the state from the lane-wise representation of the state
            * */
        public void SetLanesToState(ref byte[] state)
        {
            for (int i = 0; i < Y; i++)         // iterate over y coordinate
            {
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    Array.Copy(lanes[i][j], 0, state, (i * 5 + j) * z, z);
                }
            }
        }

        /**
            * transforms the state to a column-wise representation of the state 
            * */
        public void GetColumnsFromState(ref byte[] state)
        {
            columns = new byte[z][][];
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                columns[i] = new byte[X][];
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    columns[i][j] = new byte[Y];
                    for (int k = 0; k < Y; k++) // iterate over y coordinate
                    {
                        columns[i][j][k] = state[(j * z) + k * (X * z) + i];
                    }
                }
            }
        }

        /**
            * sets the state from the column-wise representation of the state
            * */
        public void SetColumnsToState(ref byte[] state)
        {
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    for (int k = 0; k < Y; k++) // iterate over y coordinate
                    {
                        state[(j * z) + k * (X * z) + i] = columns[i][j][k];
                    }
                }
            }
        }

        /**
            * transforms the state to a row-wise representation of the state 
            * */
        public void GetRowsFromState(ref byte[] state)
        {
            rows = new byte[z][][];
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                rows[i] = new byte[Y][];
                for (int j = 0; j < Y; j++)     // iterate over y coordinate
                {
                    rows[i][j] = new byte[X];
                    for (int k = 0; k < X; k++) // iterate over x coordinate
                    {
                        rows[i][j][k] = state[(k * z) + j * (X * z) + i];
                    }
                }
            }
        }

        /**
            * sets the state from the row-wise representation of the state
            * */
        public void SetRowsToState(ref byte[] state)
        {
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                for (int j = 0; j < Y; j++)     // iterate over y coordinate
                {
                    for (int k = 0; k < X; k++) // iterate over x coordinate
                    {
                        state[(k * z) + j * (X * z) + i] = rows[i][j][k];
                    }
                }
            }
        }

        /**
            * rotates an array of bytes by a given value (used by Rho)
            * */
        public byte[] RotateByteArray(byte[] lane, int value)
        {
            byte[] tmpLane = new byte[z];

            Array.Copy(lane, 0, tmpLane, value, lane.Length - value);
            Array.Copy(lane, lane.Length - value, tmpLane, 0, value);

            return tmpLane;
        }


        /**
            * computes the parity bit of a byte array
            * */
        public byte Parity(byte[] column)
        {
            byte parity = 0x00;

            for (int i = 0; i < Y; i++)
            {
                parity ^= column[i];
            }

            return parity;
        }

        #endregion
    }
}
