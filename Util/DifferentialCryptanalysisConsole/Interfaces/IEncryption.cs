using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    /// <summary>
    /// Interface for encryption class
    /// </summary>
    public interface IEncryption
    {
        int EncryptBlock(int data);
        int SBox(int data);
        int PBox(int data);
        int KeyMix(int data, int key);
        void GenerateRandomKeys();
        string PrintKeys();
        string PrintKeyBits(int keyNum);
    }
}
