using CrypTool.Plugins.ChaCha.Model;
using System;
using System.Linq;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// A helper class which creates the input and output actions for the quarterround visualization
    /// for the given round.
    /// </summary>
    internal class QRIOActionCreator : ChaChaHashActionCreator
    {
        /// <summary>
        /// Convenience list to write cleaner code which modifies all input values.
        /// </summary>
        private readonly QRValue[] qrInValues;

        private readonly QRValue[] diffusionQrInValues;

        /// <summary>
        /// Convenience list to write cleaner code which modifies all output values.
        /// </summary>
        private readonly QRValue[] qrOutValues;

        private readonly QRValue[] diffusionQrOutValues;

        /// <summary>
        /// Creates an instance to help with quarterround input action creation for the given round.
        /// </summary>
        /// <param name="round">Zero-based round index.</param>
        public QRIOActionCreator(ChaChaHashViewModel viewModel) : base(viewModel)
        {
            qrInValues = new QRValue[] { VM.QRInA, VM.QRInB, VM.QRInC, VM.QRInD };
            qrOutValues = new QRValue[] { VM.QROutA, VM.QROutB, VM.QROutC, VM.QROutD };
            diffusionQrInValues = new QRValue[] { VM.DiffusionQRInA, VM.DiffusionQRInB, VM.DiffusionQRInC, VM.DiffusionQRInD };
            diffusionQrOutValues = new QRValue[] { VM.DiffusionQROutA, VM.DiffusionQROutB, VM.DiffusionQROutC, VM.DiffusionQROutD };
        }

        /// <summary>
        /// Clear the values and background of each cell in the quarterround visualzation.
        /// </summary>
        public Action ResetQuarterroundValues => () =>
                                                               {
                                                                   foreach (QRValue v in qrInValues.Concat(qrOutValues))
                                                                   {
                                                                       v.Reset();
                                                                   }
                                                                   foreach (VisualQRStep qrStep in VM.QRStep)
                                                                   {
                                                                       qrStep.Reset();
                                                                   }
                                                                   if (VM.DiffusionActive)
                                                                   {
                                                                       foreach (QRValue v in diffusionQrInValues.Concat(diffusionQrOutValues))
                                                                       {
                                                                           v.Reset();
                                                                       }
                                                                       foreach (VisualQRStep qrStep in VM.DiffusionQRStep)
                                                                       {
                                                                           qrStep.Reset();
                                                                       }
                                                                   }
                                                               };

        /// <summary>
        /// Mark the state boxes which will be the qr inputs of the given round.
        /// </summary>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        public Action MarkState(int round, int qr)
        {
            return () =>
            {
                (int i, int j, int k, int l) = GetStateIndices(round, qr);
                VM.StateValues[i].Mark = true;
                VM.StateValues[j].Mark = true;
                VM.StateValues[k].Mark = true;
                VM.StateValues[l].Mark = true;
            };
        }

        /// <summary>
        /// Get the state indices which depend on the round and quarterround.
        /// </summary>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        private (int, int, int, int) GetStateIndices(int round, int qr)
        {
            AssertRoundInput(round);
            AssertQRInput(qr);
            if (round % 2 == 0)
            {
                // Column rounds
                switch (qr)
                {
                    case 0: return (0, 4, 8, 12);
                    case 1: return (1, 5, 9, 13);
                    case 2: return (2, 6, 10, 14);
                    case 3: return (3, 7, 11, 15);
                }
            }
            else
            {
                // Diagonal rounds
                switch (qr)
                {
                    case 0: return (0, 5, 10, 15);
                    case 1: return (1, 6, 11, 12);
                    case 2: return (2, 7, 8, 13);
                    case 3: return (3, 4, 9, 14);
                }
            }
            throw new InvalidOperationException($"No matching state indices found for given quarterround index {qr}");
        }

        /// <summary>
        /// Action which marks the qr input boxes.
        /// </summary>
        public Action MarkQRInputs => () =>
                                                    {
                                                        VM.QRInA.Mark = true;
                                                        VM.QRInB.Mark = true;
                                                        VM.QRInC.Mark = true;
                                                        VM.QRInD.Mark = true;
                                                    };

        /// <summary>
        /// Action which inserts the QR input values into the QR input boxes.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        public Action InsertQRInputs(int keystreamBlock, int round, int qr)
        {
            int arrayIndex = MapIndex(keystreamBlock, round, qr);
            return () =>
            {
                (VM.QRInA.Value, VM.QRInB.Value, VM.QRInC.Value, VM.QRInD.Value) = VM.ChaCha.QRInput[arrayIndex];
                if (VM.DiffusionActive)
                {
                    (VM.DiffusionQRInA.Value, VM.DiffusionQRInB.Value, VM.DiffusionQRInC.Value, VM.DiffusionQRInD.Value) = VM.ChaCha.QRInputDiffusion[arrayIndex];
                }
            };
        }

        /// <summary>
        /// Action which marks the qr output paths.
        /// </summary>
        public Action MarkQROutputPaths => () =>
                                                         {
                                                             VM.QROutA.MarkInput = true;
                                                             VM.QROutB.MarkInput = true;
                                                             VM.QROutC.MarkInput = true;
                                                             VM.QROutD.MarkInput = true;
                                                         };

        /// <summary>
        /// Action which marks the qr output boxes.
        /// </summary>
        public Action MarkQROutputs => () =>
                                                     {
                                                         VM.QROutA.Mark = true;
                                                         VM.QROutB.Mark = true;
                                                         VM.QROutC.Mark = true;
                                                         VM.QROutD.Mark = true;
                                                     };

        /// <summary>
        /// Action which inserts the QR output values into the QR output boxes.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        public Action InsertQROutputs(int keystreamBlock, int round, int qr)
        {
            int arrayIndex = MapIndex(keystreamBlock, round, qr);
            return () =>
            {
                (VM.QROutA.Value, VM.QROutB.Value, VM.QROutC.Value, VM.QROutD.Value) = VM.ChaCha.QROutput[arrayIndex];
                if (VM.DiffusionActive)
                {
                    (VM.DiffusionQROutA.Value, VM.DiffusionQROutB.Value, VM.DiffusionQROutC.Value, VM.DiffusionQROutD.Value) = VM.ChaCha.QROutputDiffusion[arrayIndex];
                }
            };
        }

        /// <summary>
        /// Update the state values with the result from the quarterround of the round.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        public Action UpdateState(int keystreamBlock, int round, int qr)
        {
            int arrayIndex = MapIndex(keystreamBlock, round, qr);
            return () =>
            {
                (uint a, uint b, uint c, uint d) = VM.ChaCha.QROutput[arrayIndex];
                (int i, int j, int k, int l) = GetStateIndices(round, qr);
                VM.StateValues[i].Value = a;
                VM.StateValues[j].Value = b;
                VM.StateValues[k].Value = c;
                VM.StateValues[l].Value = d;
                if (VM.DiffusionActive)
                {
                    (uint dA, uint dB, uint dC, uint dD) = VM.ChaCha.QROutputDiffusion[arrayIndex];
                    VM.DiffusionStateValues[i].Value = dA;
                    VM.DiffusionStateValues[j].Value = dB;
                    VM.DiffusionStateValues[k].Value = dC;
                    VM.DiffusionStateValues[l].Value = dD;
                    VM.OnPropertyChanged("DiffusionFlippedBits");
                    VM.OnPropertyChanged("DiffusionFlippedBitsPercentage");
                }
            };
        }
    }
}