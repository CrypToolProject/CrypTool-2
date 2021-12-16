/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

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

namespace KeySearcher.KeyPattern
{
    /// <summary>
    /// This class represents key movements which can be described by A*x+B
    /// </summary>
    public class LinearKeyMovement : IKeyMovement
    {
        public int A
        {
            get;
            private set;
        }

        public int B
        {
            get;
            private set;
        }

        public int UpperBound
        {
            get;
            private set;
        }

        public LinearKeyMovement(int a, int b, int upperBound)
        {
            A = a;
            B = b;
            UpperBound = upperBound;
        }

        public int Count()
        {
            return UpperBound;
        }
    }
}
