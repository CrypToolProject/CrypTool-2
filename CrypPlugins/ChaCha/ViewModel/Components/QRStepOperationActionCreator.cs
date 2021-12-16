using System;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    /// <summary>
    /// A helper class which creates the step operation actions for the quarterround visualization.
    /// This class is responsible for correctly retrieving the results
    /// by mapping the given round, quarterround and step index to the array index
    /// for a particular step operation (addition, XOR or shift).
    ///
    /// For example, if we want to have the addition result of round 2,
    /// quarterround 2, step 2 we need to access the index 6 of the QRStep array
    /// and get the addition result from that QRStep instance.
    /// This is so because the array is in this format:
    ///
    /// QR Step Array: [ Step0, Step1, Step2, Step3, Step0, Step1, Step2, Step3, ... ]
    /// Index:           0      1      2      3      4      5      6      7
    /// Quarterround:    QR 1                        QR 2
    /// Round:           Round 1
    /// </summary>
    internal class QRStepOperationActionCreator : ChaChaHashActionCreator
    {
        protected enum QRStepOperation
        {
            ADD, XOR, SHIFT
        }

        /// <summary>
        /// Create an quarterround action creator instance
        /// for the given round.
        /// </summary>
        /// <param name="round">Zero-based round index.</param>
        protected QRStepOperationActionCreator(ChaChaHashViewModel viewModel, QRStepOperation operation) : base(viewModel)
        {
            Operation = operation;
        }

        private QRStepOperation Operation { get; set; }

        /// <summary>
        /// Action which marks the input paths and boxes for the step operation.
        /// </summary>
        /// <param name="qrStep">Zero-based quarterround step index.</param>
        public Action MarkInputs(int qrStep)
        {
            AssertQRStepInput(qrStep);
            if (Operation == QRStepOperation.ADD)
            {
                return () => VM.QRStep[qrStep].Add.MarkInput = true;
            }
            else if (Operation == QRStepOperation.XOR)
            {
                return () => VM.QRStep[qrStep].XOR.MarkInput = true;
            }
            else if (Operation == QRStepOperation.SHIFT)
            {
                return () => VM.QRStep[qrStep].Shift.MarkInput = true;
            }

            throw new InvalidOperationException("Could not find a matching QRStepOperation.");
        }

        /// <summary>
        /// Action which marks the box for the step operation result.
        /// </summary>
        /// <param name="qrStep">Zero-based quarterround step index.</param>
        public Action Mark(int qrStep)
        {
            AssertQRStepInput(qrStep);
            if (Operation == QRStepOperation.ADD)
            {
                return () => VM.QRStep[qrStep].Add.Mark = true;
            }
            else if (Operation == QRStepOperation.XOR)
            {
                return () => VM.QRStep[qrStep].XOR.Mark = true;
            }
            else if (Operation == QRStepOperation.SHIFT)
            {
                return () => VM.QRStep[qrStep].Shift.Mark = true;
            }

            throw new InvalidOperationException("Could not find a matching QRStepOperation.");
        }

        /// <summary>
        /// Action which inserts the result for the step operation.
        /// </summary>
        /// <param name="keystreamBlock">Zero-based keystream block index.</param>
        /// <param name="round">Zero-based round index.</param>
        /// <param name="qr">Zero-based quarterround index.</param>
        /// <param name="qrStep">Zero-based quarterround step index.</param>
        public Action Insert(int keystreamBlock, int round, int qr, int qrStep)
        {
            int arrayIndex = MapIndex(keystreamBlock, round, qr, qrStep);
            if (Operation == QRStepOperation.ADD)
            {
                return () =>
                {
                    VM.QRStep[qrStep].Add.Value = VM.ChaCha.QRStep[arrayIndex].Add;
                    if (VM.DiffusionActive)
                    {
                        VM.DiffusionQRStep[qrStep].Add.Value = VM.ChaCha.QRStepDiffusion[arrayIndex].Add;
                    }
                };
            }
            else if (Operation == QRStepOperation.XOR)
            {
                return () =>
                {
                    VM.QRStep[qrStep].XOR.Value = VM.ChaCha.QRStep[arrayIndex].XOR;
                    if (VM.DiffusionActive)
                    {
                        VM.DiffusionQRStep[qrStep].XOR.Value = VM.ChaCha.QRStepDiffusion[arrayIndex].XOR;
                    }
                };
            }
            else if (Operation == QRStepOperation.SHIFT)
            {
                return () =>
                {
                    VM.QRStep[qrStep].Shift.Value = VM.ChaCha.QRStep[arrayIndex].Shift;
                    if (VM.DiffusionActive)
                    {
                        VM.DiffusionQRStep[qrStep].Shift.Value = VM.ChaCha.QRStepDiffusion[arrayIndex].Shift;
                    }
                };
            }

            throw new InvalidOperationException("Could not find a matching QRStepOperation.");
        }
    }

    /// <summary>
    /// A helper class which creates the actions for the addition operation during the quarterround visualization.
    /// </summary>
    internal class QRAdditionActionCreator : QRStepOperationActionCreator
    {
        public QRAdditionActionCreator(ChaChaHashViewModel viewModel) : base(viewModel, QRStepOperation.ADD)
        {
        }
    }

    /// <summary>
    /// A helper class which creates the actions for the XOR operation during the quarterround visualization.
    /// </summary>
    internal class QRXORActionCreator : QRStepOperationActionCreator
    {
        public QRXORActionCreator(ChaChaHashViewModel viewModel) : base(viewModel, QRStepOperation.XOR)
        {
        }
    }

    /// <summary>
    /// A helper class which creates the actions for the shift operation during the quarterround visualization.
    /// </summary>
    internal class QRShiftActionCreator : QRStepOperationActionCreator
    {
        public QRShiftActionCreator(ChaChaHashViewModel viewModel) : base(viewModel, QRStepOperation.SHIFT)
        {
        }
    }
}