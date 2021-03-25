using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

/// <summary>
/// A wrapper class which provides platform independent methods
/// of sending and receiving primitives. Everything numeric type
/// is sent and received as big endian.
/// </summary>
public class PlatformIndependentWrapper
{
    private NetworkStream stream;

    public PlatformIndependentWrapper(TcpClient client)
    {
        this.stream = client.GetStream();
    }

    public String ReadString()
    {
        int strlen = ReadInt();
        if (strlen == 0)
            return string.Empty;
        byte[] str = ReadArray(strlen);
        return Encoding.ASCII.GetString(str);
    }

    public int ReadInt()
    {
        byte[] bytes = ReadArray(4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    public float ReadFloat()
    {
        byte[] bytes = ReadArray(4);
        return BitConverter.ToSingle(bytes, 0);
    }

    public byte[] ReadArray(int num)
    {
        byte[] buffer = new byte[num];
        int rec = 0;
        int offset = 0;
        do{
            rec = stream.Read(buffer, offset, num-offset);
            offset += rec;
        } while(rec != 0 && offset< num);

        if(rec == 0)
        {
            throw new IOException("Socket has been closed");
        }

        return buffer;
    }

    public void WriteString(String str)
    {
        if (str == null)
        {
            WriteInt(0);
        }
        else
        {
            byte[] byted = Encoding.ASCII.GetBytes(str);
            WriteInt(byted.Length);
            WriteBytes(byted);
        }
    }

    public void WriteInt(int i)
    {
        byte[] byted = BitConverter.GetBytes(i);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byted);
        }

        WriteBytes(byted);
    }

    public void WriteBytes(byte[] bytes)
    {
        stream.Write(bytes, 0, bytes.Length);
    }
}
