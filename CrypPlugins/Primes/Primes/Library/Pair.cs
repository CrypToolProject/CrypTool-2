namespace Primes.Library
{
    public class Pair<T, U>
    {
        public Pair(T m1, U m2)
        {
            this.m_Member1 = m1;
            this.m_Member2 = m2;
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj != null && obj.GetType() == typeof(Pair<T, U>))
            {
                result = (obj as Pair<T, U>).m_Member1.Equals(this.m_Member1) && (obj as Pair<T, U>).m_Member2.Equals(this.m_Member2);
            }

            return result;
        }

        public override int GetHashCode()
        {
            return (this.m_Member1.GetHashCode() + this.m_Member2.GetHashCode()) % int.MaxValue;
        }

        private T m_Member1;

        public T Member1
        {
            get { return m_Member1; }
            set { m_Member1 = value; }
        }

        private U m_Member2;

        public U Member2
        {
            get { return m_Member2; }
            set { m_Member2 = value; }
        }
    }
}
