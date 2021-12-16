using System;

namespace CrypTool.PluginBase.Control
{

    public interface IControlSigabaEncryption : IControl, IDisposable
    {
        string Decrypt(string ciphertext);
        void setInternalConfig();
        void changeSettings(string setting, object value);

        byte[] DecryptFast(byte[] ciphertext, int[] a, byte[] positions);

        void setCipherRotors(int i, byte a);

        void setControlRotors(byte i, byte b);

        void setIndexRotors(byte i, byte c);

        void setIndexMaze();

        void setIndexMaze(int[] indexmaze);

        void setIndexMaze2(int[] indexmaze);

        void setBool(byte ix, byte i, bool rev);

        void setPositionsControl(byte ix, byte i, byte position);

        void setPositionsIndex(byte ix, byte i, byte position);

        string preFormatInput(string text);
        string postFormatOutput(string text);
    }

}
