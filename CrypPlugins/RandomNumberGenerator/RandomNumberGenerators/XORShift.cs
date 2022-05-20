/*
   Copyright 2022 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Nils Kopal
   Some code adapted from: https://en.wikipedia.org/wiki/Xorshift

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
using System.Numerics;

namespace CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerators
{
    internal class XORShift : RandomGenerator
    {
        private readonly XORShiftType _xorShiftType;
        private ulong _state;

        public XORShift(BigInteger seed, int outputLength, XORShiftType xorShiftType)
        {
            _xorShiftType = xorShiftType;
            OutputLength = outputLength;

            switch (_xorShiftType)
            {
                case XORShiftType.XOR_Shift8:
                    _state = (byte)seed;
                    break;
                case XORShiftType.XOR_Shift16:
                    _state = (ushort)seed;
                    break;
                case XORShiftType.XOR_Shift32:
                    _state = (uint)seed;
                    break;
                case XORShiftType.XOR_Shift64:
                    _state = (ulong)seed;
                    break;
                default:
                    throw new Exception(string.Format("XORShiftType {0} not implemented", _xorShiftType));
            }
        }

        public override byte[] GenerateRandomByteArray()
        {
            byte[] result = new byte[OutputLength];
            int resultoffset = 0;
            while (resultoffset < result.Length)
            {
                ComputeNextRandomNumber();
                byte[] array = ConvertCurrentNumberToByteArray();
                for (int arrayoffset = 0; arrayoffset < array.Length; arrayoffset++)
                {
                    if (resultoffset + arrayoffset == result.Length)
                    {
                        break;
                    }
                    result[resultoffset + arrayoffset] = array[arrayoffset];
                }
                resultoffset = resultoffset + array.Length;
            }
            return result;
        }

        private byte[] ConvertCurrentNumberToByteArray()
        {
            switch (_xorShiftType)
            {
                case XORShiftType.XOR_Shift8:
                    return new byte[] { (byte)_state };
                case XORShiftType.XOR_Shift16:
                    return BitConverter.GetBytes((ushort)_state);
                case XORShiftType.XOR_Shift32:
                    return BitConverter.GetBytes((uint)_state);
                case XORShiftType.XOR_Shift64:
                    return BitConverter.GetBytes(_state);
                default:
                    throw new Exception(string.Format("XORShiftType {0} not implemented", _xorShiftType));
            }
        }

        public override void ComputeNextRandomNumber()
        {
            switch (_xorShiftType)
            {
                case XORShiftType.XOR_Shift8:
                    _state = (byte)(_state ^ (_state << 5));
                    _state = (byte)(_state ^ (_state >> 3));
                    _state = (byte)(_state ^ (_state << 7));
                    break;
                case XORShiftType.XOR_Shift16:
                    _state = (ushort)(_state ^ (_state << 7));
                    _state = (ushort)(_state ^ (_state >> 9));
                    _state = (ushort)(_state ^ (_state << 8));
                    break;
                case XORShiftType.XOR_Shift32:
                    _state = (uint)(_state ^ (_state << 13));
                    _state = (uint)(_state ^ (_state >> 17));
                    _state = (uint)(_state ^ (_state << 5));
                    break;
                case XORShiftType.XOR_Shift64:
                    _state = _state ^ (_state << 13);
                    _state = _state ^ (_state >> 17);
                    _state = _state ^ (_state << 5);
                    break;
                default:
                    throw new Exception(string.Format("XORShiftType {0} not implemented", _xorShiftType));
            }
            RandNo = _state;
        }
    }
}