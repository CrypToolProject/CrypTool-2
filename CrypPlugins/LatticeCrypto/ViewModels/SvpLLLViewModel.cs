using LatticeCrypto.Utilities;

namespace LatticeCrypto.ViewModels
{
    public class SvpLLLViewModel : SvpGaussViewModel
    {
        public SvpLLLViewModel()
        {
            ReductionMethod = ReductionMethods.reduceLLL;
        }
    }
}
