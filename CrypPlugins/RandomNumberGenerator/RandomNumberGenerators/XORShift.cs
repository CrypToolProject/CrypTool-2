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
        private XORShiftType _xorShiftType;
        private ulong _state;

        public XORShift(BigInteger seed, BigInteger outputLength, XORShiftType xorShiftType)
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
            byte[] res = new byte[(int)OutputLength];

            for (int i = 0; i < res.Length; i++)
            {
                int curByte = 0;
                int tmp = 128;
                for (int j = 0; j < 8; j++)
                {
                    Randomize();
                    if (GenerateRandomBit())
                    {
                        curByte += tmp;
                    }
                    tmp /= 2;
                }
                res[i] = Convert.ToByte(curByte);
            }
            return res;
        }

        public override bool GenerateRandomBit()
        {
            return RandNo % 2 == 1;
        }

        public override void Randomize()
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
                    _state = (ulong)(_state ^ (_state << 13));
                    _state = (ulong)(_state ^ (_state >> 17));
                    _state = (ulong)(_state ^ (_state << 5));
                    break;
                default:
                    throw new Exception(string.Format("XORShiftType {0} not implemented", _xorShiftType));
            }
            RandNo = _state;
        }
    }
}