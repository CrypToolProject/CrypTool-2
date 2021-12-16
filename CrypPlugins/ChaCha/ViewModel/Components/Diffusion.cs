using CrypTool.Plugins.ChaCha.Helper;
using System;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CrypTool.Plugins.ChaCha.ViewModel.Components
{
    internal static class Diffusion
    {
        /// <summary>
        /// Set the Document of the RichTextBox with the diffusion value as hex string; using byte array for comparison; marking differences red.
        /// </summary>
        public static void InitDiffusionValue(RichTextBox rtb, byte[] diffusion, byte[] primary)
        {
            string dHex = Formatter.HexString(diffusion);
            string pHex = Formatter.HexString(primary);
            InitDiffusionValue(rtb, dHex, pHex);
        }

        /// <summary>
        /// Set the Document of the RichTextBox with the diffusion value as hex string; using byte array for comparison; marking differences red.
        /// </summary>
        public static void InitDiffusionValue(RichTextBox rtb, uint diffusion, uint primary)
        {
            string dHex = Formatter.HexString(diffusion);
            string pHex = Formatter.HexString(primary);
            InitDiffusionValue(rtb, dHex, pHex);
        }

        /// <summary>
        /// Set the document of the RichTextBox with the diffusion value as hex string; using strings for comparison; marking differences red.
        /// </summary>
        public static void InitDiffusionValue(RichTextBox rtb, string dHex, string pHex)
        {
            if (dHex.Length != pHex.Length)
            {
                throw new ArgumentException("Diffusion value must be of same length as primary value.");
            }

            if (dHex.Replace(" ", "").Length % 2 != 0)
            {
                throw new ArgumentException("Length must be even");
            }

            if ((Paragraph)rtb.Document.Blocks.LastBlock == null)
            {
                rtb.Document.Blocks.Add(new Paragraph());
            }
            else
            {
                ((Paragraph)rtb.Document.Blocks.LastBlock).Inlines.Clear();
            }

            for (int i = 0; i < dHex.Length; ++i)
            {
                char dChar1 = dHex[i];
                char pChar1 = pHex[i];
                ((Paragraph)rtb.Document.Blocks.LastBlock).Inlines.Add(RedIfDifferent(dChar1, pChar1));
            }
        }

        /// <summary>
        /// Set the document of the RichTextBox with the xor value as hex string; marking every non-zero character red.
        /// </summary>
        public static void InitXORValue(RichTextBox rtb, string dHex, string pHex)
        {
            byte[] d = Formatter.Bytes(dHex);
            byte[] p = Formatter.Bytes(pHex);
            byte[] xor = ByteUtil.XOR(d, p);
            string xorHex = Formatter.HexString(xor);
            InitDiffusionValue(rtb, xorHex, string.Concat(Enumerable.Repeat("0", xorHex.Length)));
        }

        /// <summary>
        /// Set the document of the RichTextBox with the xor value as hex string; marking every non-zero character red.
        /// This function preserves whitespaces in the input strings.
        /// </summary>
        public static void InitXORChunkValue(RichTextBox rtb, string chunkDHex, string chunkPHex)
        {
            int chunkSize = chunkDHex.IndexOf(' ');
            byte[] d = Formatter.Bytes(chunkDHex.Replace(" ", ""));
            byte[] p = Formatter.Bytes(chunkPHex.Replace(" ", ""));
            byte[] xor = ByteUtil.XOR(d, p);
            string xorHex = Formatter.HexString(xor);
            string xorHexCunks = Formatter.Chunkify(xorHex, chunkSize);
            string zeroes = Formatter.Chunkify(string.Concat(Enumerable.Repeat("0", xorHex.Length)), chunkSize);
            InitDiffusionValue(rtb, xorHexCunks, zeroes);
        }

        /// <summary>
        /// Set the document of the RichTextBox with the xor value of the byte arrays as hex string; marking every non-zero character red.
        /// </summary>
        public static void InitXORValue(RichTextBox rtb, byte[] diffusion, byte[] primary)
        {
            string dHex = Formatter.HexString(diffusion);
            string pHex = Formatter.HexString(primary);
            InitXORValue(rtb, dHex, pHex);
        }

        /// <summary>
        /// Set the Document of the RichTextBox with the xor value as hex string.
        /// </summary>
        public static void InitXORValue(RichTextBox rtb, uint diffusion, uint primary)
        {
            string dHex = Formatter.HexString(diffusion);
            string pHex = Formatter.HexString(primary);
            InitXORValue(rtb, dHex, pHex);
        }

        /// <summary>
        /// Returns a Run element with the character d in red if d != v else black.
        /// </summary>
        private static Run RedIfDifferent(char d, char v)
        {
            return new Run() { Text = d.ToString(), Foreground = d != v ? Brushes.Red : Brushes.Black };
        }

        /// <summary>
        /// Set the Document of the RichTextBox with the diffusion value; marking differences red.
        /// Version is used to determine counter size.
        /// </summary>
        public static void InitDiffusionValue(RichTextBox rtb, BigInteger diffusion, BigInteger primary, Version version)
        {
            if (version.CounterBits == 64)
            {
                byte[] diffusionBytes = ByteUtil.GetBytesBE((ulong)diffusion);
                byte[] primaryBytes = ByteUtil.GetBytesBE((ulong)primary);
                InitDiffusionValue(rtb, diffusionBytes, primaryBytes);
            }
            else
            {
                byte[] diffusionBytes = ByteUtil.GetBytesBE((uint)diffusion);
                byte[] primaryBytes = ByteUtil.GetBytesBE((uint)primary);
                InitDiffusionValue(rtb, diffusionBytes, primaryBytes);
            }
        }

        /// <summary>
        /// Set the Document of the RichTextBox with the xor value; marking non-zero characters red.
        /// Version is used to determine counter size.
        /// </summary>
        public static void InitXORValue(RichTextBox rtb, BigInteger diffusion, BigInteger primary, Version version)
        {
            if (version.CounterBits == 64)
            {
                byte[] diffusionBytes = ByteUtil.GetBytesBE((ulong)diffusion);
                byte[] primaryBytes = ByteUtil.GetBytesBE((ulong)primary);
                InitXORValue(rtb, diffusionBytes, primaryBytes);
            }
            else
            {
                byte[] diffusionBytes = ByteUtil.GetBytesBE((uint)diffusion);
                byte[] primaryBytes = ByteUtil.GetBytesBE((uint)primary);
                InitXORValue(rtb, diffusionBytes, primaryBytes);
            }
        }
    }
}