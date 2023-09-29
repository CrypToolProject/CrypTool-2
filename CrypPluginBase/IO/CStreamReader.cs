/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CrypTool.PluginBase.IO
{
    /// <summary>
    /// Read from a CStream. Use POSIX Read() or more convenient ReadFully() to retrieve data
    /// from CStream.
    /// 
    /// <para>You MAY seek in the CStream to re-use the reader or skip data (beware of seeking too far:
    /// will lead to EOF).</para>
    /// 
    /// <para>You SHOULD dispose the reader when you're done using it. If you don't, the GC will release
    /// your resources.</para>
    /// 
    /// <para>You SHOULD NOT pass the same reader instance to other components. Concurrent access on
    /// the same reader will lead to a probably unwanted behaviour. You MAY however use two different
    /// readers on the same CStream. Each reader maintains its own state.</para>
    /// </summary>
    public class CStreamReader : Stream, IDisposable
    {
        #region Private fields and constructors

        private readonly CStreamWriter _writer;

        private FileStream _readStream;
        private int _readPtr;

        private bool _disposed;

        /// <summary>
        /// Create a reader to read from the passed CStream.
        /// </summary>
        public CStreamReader(CStreamWriter writer)
        {
            _writer = writer;

            _writer.SwapEvent += swapHandler;

            if (_writer.IsSwapped)
            {
                swapHandler();
            }
        }

        #endregion

        #region Public properties

        public override bool CanRead => !_disposed;

        public override bool CanSeek => !_disposed;

        public override bool CanWrite => false;

        public bool IsSwapped => _readStream != null;

        /// <summary>
        /// Caveat: The length may grow while the writer has not closed the stream. If you rely on Length, you may want to call WaitEof() before.
        /// </summary>
        public override long Length =>
                // TODO: cache FileStream property
                _writer.Length;

        public override long Position
        {
            get
            {
                if (IsSwapped)
                {
                    // TODO: cache FileStream property
                    return _readStream.Position;
                }
                else
                {
                    return _readPtr;
                }
            }

            set => Seek(value, SeekOrigin.Begin);
        }

        #endregion

        #region Public methods

        public override void Close()
        {
            Dispose();
        }

        public new void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            base.Dispose();

            if (IsSwapped)
            {
                _readStream.Close();
                _readStream = null;
            }

            _writer.SwapEvent -= swapHandler;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Convenience method for Read(byte[] buf, 0, buf.Length)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Read POSIX-like 1 to count amount of bytes into given byte array.
        /// Blocks until at least 1 byte has been read or underlying stream has been closed.
        /// Does not guarantee to read the requested/available amount of data, can read less.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>amount of bytes that has been read into buffer or 0 if EOF</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            checkDisposal();

            lock (_writer.InternalMonitor)
            {
                int available;

                while ((available = availableRead()) < 1)
                {
                    // writer has been closed or reader has seeked beyond available length
                    if (_writer.IsClosed || available < 0)
                    {
                        return 0; // EOF
                    }

                    Monitor.Wait(_writer.InternalMonitor);
                }

                int readAttempt = Math.Min(available, count);

                if (IsSwapped)
                {
                    // MUST NOT block, otherwise we're potentially deadlocked
                    Debug.Assert(_writer.Length - _readStream.Position > 0);
                    return _readStream.Read(buffer, offset, readAttempt);
                }
                else
                {
                    Array.Copy(_writer.MemBuff, _readPtr, buffer, offset, readAttempt);
                    _readPtr += readAttempt;

                    return readAttempt;
                }
            }
        }

        /// <summary>
        /// Convenience method for Read: read and block until EOF occurs.
        /// 
        /// This method is inefficient for large data amounts. You should avoid it in production code.
        /// </summary>
        public byte[] ReadFully()
        {
            List<byte[]> list = new List<byte[]>();
            int overall = 0;

            { // read bunch of byte arrays
                byte[] buf;
                int read;
                do
                {
                    buf = new byte[4096];
                    read = ReadFully(buf);

                    if (read > 0)
                    {
                        if (read < buf.Length)
                        { // special case: read less bytes than buffer can hold
                            byte[] resizedBuf = new byte[read];
                            Array.Copy(buf, resizedBuf, read);
                            list.Add(resizedBuf);
                        }
                        else
                        { // default case
                            Debug.Assert(buf.Length == read);
                            list.Add(buf);
                        }

                        overall += read;
                    }
                } while (read == buf.Length); // not EOF
            }

            { // concat small buffers to bigbuffer
                byte[] bigbuffer = new byte[overall];
                int offset = 0;
                foreach (byte[] buf in list)
                {
                    Array.Copy(buf, 0, bigbuffer, offset, buf.Length);
                    offset += buf.Length;
                }

                Debug.Assert(offset == overall);

                return bigbuffer;
            }
        }

        /// <summary>
        /// Convenience method for Read: read and block until array is full or EOF occurs.
        /// </summary>
        public int ReadFully(byte[] buffer)
        {
            return ReadFully(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Convenience method for Read: read and block until required amount of data has
        /// been retrieved or EOF occurs.
        /// </summary>
        public int ReadFully(byte[] buffer, int offset, int count)
        {
            int readSum = 0;
            while (readSum < count)
            {
                int read = Read(buffer, offset, (count - readSum));

                if (read == 0) // EOF
                {
                    return readSum;
                }

                readSum += read;
            }

            return readSum;
        }

        /// <summary>
        /// Seek to another position. Seeking beyond the length of the stream is permitted
        /// (successive read will block until writer stream is closed).
        /// 
        /// Note: there is no boundary check and no comprehensive check for int wraparound
        /// (internal pointer on memory buffer is int32). Don't drink and seek.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            checkDisposal();

            if (IsSwapped)
            {
                return _readStream.Seek(offset, origin);
            }
            else
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _readPtr = (int)offset;
                        break;
                    case SeekOrigin.Current:
                        _readPtr += (int)offset;
                        break;
                    case SeekOrigin.End:
                        _readPtr = _writer.MemBuff.Length + (int)offset;
                        break;
                }

                if (_readPtr < 0)
                {
                    _readPtr = 0;
                }

                return _readPtr;
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Waits until the writer has stopped generating data and has closed the stream.
        /// This method is for lazy readers waiting to have full length available before starting any processing.
        /// It's usually less effective than trying to read continously.
        /// </summary>
        public void WaitEof()
        {
            checkDisposal();

            lock (_writer.InternalMonitor)
            {
                while (!_writer.IsClosed)
                {
                    Monitor.Wait(_writer.InternalMonitor);
                }
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Private/protected methods

        private int availableRead()
        {
            long avail = _writer.Position - (IsSwapped ? _readStream.Position : _readPtr);
            return (int)Math.Min(int.MaxValue, avail);
        }

        private void checkDisposal()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Reader is already disposed");
            }
        }

        /// <summary>
        /// Switch from membuff to swapfile
        /// </summary>
        private void swapHandler()
        {
            _readStream = new FileStream(_writer.FilePath, FileMode.Open, FileAccess.Read, (FileShare.ReadWrite | FileShare.Delete));
            if (_readPtr > 0)
            {
                Debug.Assert(_readPtr <= _writer.Length);
                _readStream.Seek(_readPtr, SeekOrigin.Begin);
            }
        }

        #endregion
    }
}
