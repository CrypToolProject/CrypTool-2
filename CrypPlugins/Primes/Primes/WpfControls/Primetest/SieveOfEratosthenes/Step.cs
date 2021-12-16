using Primes.Bignum;

namespace Primes.WpfControls.Primetest.SieveOfEratosthenes
{
    public enum StepResult { SUCCESS, FAILED, END }

    public class Step
    {
        private PrimesBigInteger m_Current;

        public PrimesBigInteger Current => m_Current;

        private PrimesBigInteger m_Expected;

        public PrimesBigInteger Expected => m_Expected;

        private readonly PrimesBigInteger m_MaxValue;

        private readonly Numbergrid.Numbergrid m_Numbergrid;

        public Step(Numbergrid.Numbergrid numbergrid, PrimesBigInteger maxValue)
        {
            m_Expected = m_Current = PrimesBigInteger.Two;
            m_Numbergrid = numbergrid;
            m_MaxValue = maxValue;
        }

        public StepResult DoStep(PrimesBigInteger value)
        {
            if (m_Expected.CompareTo(value) == 0)
            {
                m_Numbergrid.RemoveMulipleOf(value);
                m_Expected = m_Expected.NextProbablePrime();
                m_Current = value;
                if (m_Current.Pow(2).CompareTo(m_MaxValue) >= 0)
                {
                    return StepResult.END;
                }

                return StepResult.SUCCESS;
            }
            else
            {
                return StepResult.FAILED;
            }
        }

        public void Reset()
        {
            m_Expected = m_Current = PrimesBigInteger.Two;
        }
    }
}