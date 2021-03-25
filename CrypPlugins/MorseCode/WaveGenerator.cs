/*
   Copyright 2013 Nils Kopal <Nils.Kopal@Uni-Kassel.de>

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
using System.IO;

namespace MorseCode
{
    public class RiffHeader
    {
        public char[] ChunkId = { 'R', 'I', 'F', 'F' };
        public UInt32 ChunkSize = 0;
        public char[] RiffType = { 'W', 'A', 'V', 'E' };
    }//size 12

    public class FormatChunk
    {
        public char[] HeaderSignature = { 'f', 'm', 't', ' ' };
        public UInt32 WFormatLength = 16;                        
        public UInt16 WFormatTag = 1;                           //PCM = 1
        public UInt16 WChannels = 2;                            //Channels = 2 (Stereo)
        public UInt32 DwSamplesPerSec = 44100;                  //SampleRate = 44100
        public UInt32 DwAvgBytesPerSec = 88200;                 //WChannels * WSamplesPerSecond * WBitsPerSample / 8 
        public UInt16 WBlockAlign = 2;                          //WChannels * WBitsPerSample / 8 
        public UInt16 WBitsPerSample = 8;                       //Datenbits pro Kanal = 8
    }//size 24

    public class DataChunk
    {
        public char[] HeaderSignature = { 'd', 'a', 't', 'a' };
        public UInt32 DwLength = 0;
        public byte[] Data;
    }

    public class Wave
    {
        public RiffHeader RiffHeader = new RiffHeader();
        public FormatChunk FormatChunk = new FormatChunk();
        public DataChunk DataChunk = new DataChunk();

        /// <summary>
        /// Writes this Wave to a given stream
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            //Write RiffHeader to stream
            writer.Write(RiffHeader.ChunkId);
            writer.Write(RiffHeader.ChunkSize);
            writer.Write(RiffHeader.RiffType);
            //Write FormatChunk to stream
            writer.Write(FormatChunk.HeaderSignature);
            writer.Write(FormatChunk.WFormatLength);
            writer.Write(FormatChunk.WFormatTag);
            writer.Write(FormatChunk.WChannels);
            writer.Write(FormatChunk.DwSamplesPerSec);
            writer.Write(FormatChunk.DwAvgBytesPerSec);
            writer.Write(FormatChunk.WBlockAlign);
            writer.Write(FormatChunk.WBitsPerSample);
            //Write DataChunk to Stream
            writer.Write(DataChunk.HeaderSignature);
            writer.Write(DataChunk.DwLength);
            writer.Write(DataChunk.Data, 0, DataChunk.Data.Length);
            writer.Flush();
        }
    }


    public class Tone
    {
        private byte[] data;

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(data, 0, data.Length);
            writer.Flush();
        }

        /// <summary>
        /// Generates a wave with the given amplitude, frequency, and duration in miliseconds
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="duration"></param>
        public void GenerateSound(double amplitude, double frequency, double duration)
        {
            int size = (int)((duration / 1000.0) * 44100.0 * 2.0);
            if (size % 2 == 1)
            {
                size++;
            }
            data =  new byte[size];
            for (int i = 0; i < size; i = i + 2)
            {
                double currentAmplitude = 0;
                double percentageValue = (i / (double)size) * 100;
                //attack phase 10%
                if(percentageValue < 10)
                {
                    currentAmplitude = amplitude * (percentageValue / 10);
                }               
                //sustain phase 80%
                else if(percentageValue >= 10 && percentageValue < 90)
                {
                    currentAmplitude = amplitude;
                }
                //release phase 10%
                else if(percentageValue >= 90)                
                {
                    percentageValue = 100 - percentageValue;
                    currentAmplitude = amplitude * ((double)percentageValue / 10);
                }                
                int value = (int)Math.Sign(currentAmplitude * Math.Sin(2.0 * Math.PI * (((double)i) / 88200.0 * frequency)));
                data[i] = (byte)(value);
                data[i + 1] = (byte)(value);
            }            
        }
    }
}
