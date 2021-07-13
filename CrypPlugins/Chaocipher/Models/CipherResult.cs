using System.Collections.Generic;

namespace CrypTool.Chaocipher.Models
{
    public class CipherResult
    {
        public CipherResult(string initLeftDisk, string initRightDisk, string resultString,
            List<PresentationState> presentationStates)
        {
            InitLeftDisk = initLeftDisk;
            InitRightDisk = initRightDisk;
            ResultString = resultString;
            PresentationStates = presentationStates;
        }

        public List<PresentationState> PresentationStates { get; }
        public string ResultString { get; }
        public string InitLeftDisk { get; }
        public string InitRightDisk { get; }
    }
}