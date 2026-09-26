/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
namespace Primes.Library
{
    public class Pair<T, U>
    {
        public Pair(T m1, U m2)
        {
            m_Member1 = m1;
            m_Member2 = m2;
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj != null && obj.GetType() == typeof(Pair<T, U>))
            {
                result = (obj as Pair<T, U>).m_Member1.Equals(m_Member1) && (obj as Pair<T, U>).m_Member2.Equals(m_Member2);
            }

            return result;
        }

        public override int GetHashCode()
        {
            return (m_Member1.GetHashCode() + m_Member2.GetHashCode()) % int.MaxValue;
        }

        private T m_Member1;

        public T Member1
        {
            get => m_Member1;
            set => m_Member1 = value;
        }

        private U m_Member2;

        public U Member2
        {
            get => m_Member2;
            set => m_Member2 = value;
        }
    }
}
