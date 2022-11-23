/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.Runtime.CompilerServices;

public class TriviumAlgorithm
{
    private readonly byte[] _state = new byte[36];

    /// <summary>
    /// Creates a Trivium cipher using the given key, iv, and round number
    /// </summary>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    public TriviumAlgorithm(byte[] key, byte[] iv, int initRounds = 1152)
    {
        //copy key and iv, since we have to rotate them
        byte[] key_copy = new byte[key.Length];
        byte[] iv_copy = new byte[iv.Length];
        Array.Copy(key, key_copy, key.Length);
        Array.Copy(iv, iv_copy, key.Length);

        //rotate key and iv
        Array.Reverse(key_copy);
        Array.Reverse(iv_copy);

        //copy key to state
        Array.Copy(key_copy, 0, _state, 0, key_copy.Length);

        //copy iv to state
        for (int i = 0; i < iv_copy.Length * 8; i++)
        {
            byte bit = GetBit(iv_copy, i);
            SetBit(_state, i + 93, bit);
        }

        //set last byte to constant value
        _state[35] = 0b00000111;

        //rotate state 4 complete times
        for (int i = 0; i < initRounds; i++)
        {
            Rotate();
        }
    }

    /// <summary>
    /// Get next byte of generated key stream
    /// </summary>
    /// <returns></returns>
    public byte GenerateNextByte()
    {
        byte result = 0;

        for (int i = 0; i < 8; i++)
        {
            byte t1 = (byte)(GetBit(_state, 065) ^ GetBit(_state, 092));
            byte t2 = (byte)(GetBit(_state, 161) ^ GetBit(_state, 176));
            byte t3 = (byte)(GetBit(_state, 242) ^ GetBit(_state, 287));

            //funnily, the bits are used from LSB to MSB instead of MSB to LSB...
            result = (byte)((result) ^ ((t1 ^ t2 ^ t3) << i));

            t1 ^= (byte)((GetBit(_state, 090) & GetBit(_state, 091)) ^ GetBit(_state, 170));
            t2 ^= (byte)((GetBit(_state, 174) & GetBit(_state, 175)) ^ GetBit(_state, 263));
            t3 ^= (byte)((GetBit(_state, 285) & GetBit(_state, 286)) ^ GetBit(_state, 068));

            RightShift(_state);

            SetBit(_state, 000, t3);
            SetBit(_state, 093, t1);
            SetBit(_state, 177, t2);
        }
        return result;
    }

    /// <summary>
    /// Rotates the state one time
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rotate()
    {
        byte t1 = (byte)(GetBit(_state, 065) ^ GetBit(_state, 092));
        byte t2 = (byte)(GetBit(_state, 161) ^ GetBit(_state, 176));
        byte t3 = (byte)(GetBit(_state, 242) ^ GetBit(_state, 287));

        t1 ^= (byte)((GetBit(_state, 090) & GetBit(_state, 091)) ^ GetBit(_state, 170));
        t2 ^= (byte)((GetBit(_state, 174) & GetBit(_state, 175)) ^ GetBit(_state, 263));
        t3 ^= (byte)((GetBit(_state, 285) & GetBit(_state, 286)) ^ GetBit(_state, 068));

        RightShift(_state);

        SetBit(_state, 000, t3);
        SetBit(_state, 093, t1);
        SetBit(_state, 177, t2);
    }

    /// <summary>
    /// Get a bit value (in LSB of returned byte) of given byte array
    /// </summary>
    /// <param name="array"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetBit(byte[] array, int i)
    {
        return (byte)((array[i / 8] >> (7 - i % 8)) & 0x1);
    }

    /// <summary>
    /// Set a bit value (using LSB of given byte) in given byte array
    /// </summary>
    /// <param name="array"></param>
    /// <param name="i"></param>
    /// <param name="b"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBit(byte[] array, int i, byte b)
    {
        array[i / 8] = (byte)(array[i / 8] & ~(1 << (7 - i % 8)) | (b << (7 - i % 8)));
    }

    /// <summary>
    /// Performs an in-place right shift one bit of given byte array
    /// </summary>
    /// <param name="array"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RightShift(byte[] array)
    {
        for (int j = array.Length - 1; j > 0; j--)
        {
            array[j] = (byte)(array[j] >> 1 | (array[j - 1] << 7));
        }
        array[0] = (byte)(array[0] >> 1);
    }
}