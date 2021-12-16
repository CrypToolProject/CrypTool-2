namespace CrypTool.KasiskiTest
{
    public class CollectionElement
    {



        private int m_amount;
        private int m_factor;
        private double m_height;


        public CollectionElement(int factor, int amount, double height)
        {
            m_amount = amount;
            m_factor = factor;
            m_height = height;
        }


        public int Factor
        {
            get => m_factor;
            set => m_factor = value;
        }
        public int Amount
        {
            get => m_amount;
            set => m_amount = value;
        }
        public double Height
        {
            get => m_height;
            set => m_height = value;
        }
    }
}
