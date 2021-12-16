/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System;

namespace CrypToolStoreLib.Tools
{
    /// <summary>
    /// Exception that is thrown when something goes wrong during serialization of data
    /// </summary>
    internal class SerializationException : Exception
    {
        public SerializationException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// Exception that is thrown when something goes wrong during deserialization of data
    /// </summary>
    internal class DeserializationException : Exception
    {
        public DeserializationException(string message) : base(message)
        {

        }
    }
}
