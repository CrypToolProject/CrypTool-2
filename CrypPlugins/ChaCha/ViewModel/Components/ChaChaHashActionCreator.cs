using System;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// Base class for all ChaCha Hash action creators.
    /// Implements the MapIndex method and argument validation.
    /// </summary>
    internal abstract class ChaChaHashActionCreator
    {
        protected ChaChaHashActionCreator(ChaChaHashViewModel viewModel)
        {
            VM = viewModel;
        }

        protected ChaChaHashViewModel VM { get; private set; }

        protected void AssertKeystreamBlockInput(int keystreamBlock)
        {
            int maxKeystreamBlock = VM.ChaCha.TotalKeystreamBlocks - 1;
            if (keystreamBlock < 0 || keystreamBlock > maxKeystreamBlock)
            {
                throw new ArgumentOutOfRangeException("keystreamBlock", $"keystreamBlock must be between 0 and {maxKeystreamBlock}. Received {keystreamBlock}.");
            }
        }

        protected void AssertRoundInput(int round)
        {
            int maxRoundIndex = VM.Settings.Rounds - 1;
            if (round < 0 || round > maxRoundIndex)
            {
                throw new ArgumentOutOfRangeException("round", $"round must be between 0 and {maxRoundIndex}. Received {round}.");
            }
        }

        protected void AssertQRInput(int qr)
        {
            if (qr < 0 || qr > 3)
            {
                throw new ArgumentOutOfRangeException("qr", $"qr must be between 0 and 3. Received {qr}");
            }
        }

        protected void AssertQRStepInput(int qrStep)
        {
            if (qrStep < 0 || qrStep > 3)
            {
                throw new ArgumentOutOfRangeException("qrStep", $"qrStep must be between 0 and 3. Received {qrStep}");
            }
        }

        /// <summary>
        /// The "big brain" method.
        /// Return the array index to access the correct QRStep instance.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        /// <param name="qrStep">Zero-based quarterround step index.</param>
        protected int MapIndex(int keystreamBlock, int round, int qr, int qrStep)
        {
            AssertKeystreamBlockInput(keystreamBlock);
            AssertRoundInput(qr);
            AssertQRInput(qr);
            AssertQRStepInput(qrStep);
            // For every round, we need to skip 16 * Settings.Rounds steps
            // For every round, we need to skip 16 steps.
            // For every quarterround, we need to skip 4 steps.
            return
                keystreamBlock * VM.Settings.Rounds * 16
                + round * 16
                + qr * 4
                + qrStep;
        }

        /// <summary>
        /// The "big brain" method.
        /// Return the array index to access the correct QRInput/QROutput instance.
        /// </summary
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        protected int MapIndex(int keystreamBlock, int round, int qr)
        {
            AssertRoundInput(qr);
            AssertQRInput(qr);
            // For every keystream block, we need to skip 4 * Settings.Rounds quarterrounds
            // For every round, we need to skip 4 quarterrounds.
            return
                keystreamBlock * VM.Settings.Rounds * 4
                + round * 4
                + qr;
        }
    }
}