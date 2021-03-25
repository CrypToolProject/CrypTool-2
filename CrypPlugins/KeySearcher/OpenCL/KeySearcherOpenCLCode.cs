using System;
using CrypTool.PluginBase.Control;
using KeySearcher.Properties;
using OpenCLNet;

namespace KeySearcher
{
    /// <summary>
    /// This class constructs the OpenCL bruteforce code out of the single components (encryption plugin, cost function, key pattern, ...)
    /// that should be used by the KeySearcher.
    /// The created kernel (which uses the generated code) can directly be used by the KeySearcher for bruteforcing.
    /// </summary>
    class KeySearcherOpenCLCode
    {
        private readonly KeySearcher keySearcher;
        private byte[] encryptedData;
        private readonly byte[] iv;
        private IControlCost controlCost;
        private IControlEncryption encryptionController;
        private int approximateNumberOfKeys;

        private IKeyTranslator keyTranslatorOfCode = null;
        private string openCLCode = null;
        private Kernel openCLKernel = null;

        /// <summary>
        /// This constructor is used to setup the parameters used for code generation.
        /// </summary>
        /// <param name="keySearcher">The KeySearcher instance (only used for GuiLogMessages).</param>
        /// <param name="encryptedData">The byte array which contains the data which should be encrypted.</param>
        /// <param name="iv">The IV vector</param>
        /// <param name="encryptionController">The IControlEncryption instance of the encryption plugin to use.</param>
        /// <param name="controlCost">The IControlCost instance of the cost function plugin to use.</param>
        /// <param name="approximateNumberOfKeys">A maximum bound which indicates on how many key bruteforcing at once the OpenCL code should be layed out.</param>
        public KeySearcherOpenCLCode(KeySearcher keySearcher, byte[] encryptedData, byte[] iv, IControlEncryption encryptionController, IControlCost controlCost, int approximateNumberOfKeys)
        {
            this.keySearcher = keySearcher;
            this.encryptedData = encryptedData;
            this.iv = iv;
            this.encryptionController = encryptionController;
            this.controlCost = controlCost;
            this.approximateNumberOfKeys = approximateNumberOfKeys;
        }

        /// <summary>
        /// Generates the OpenCL code and returns it as a string.
        /// </summary>
        /// <param name="keyTranslator">The KeyTranslator to use. This is important for mapping the key movements to code.</param>
        /// <returns>The OpenCL code</returns>
        public string CreateOpenCLBruteForceCode(IKeyTranslator keyTranslator)
        {
            if (keyTranslatorOfCode == keyTranslator)
            {
                return openCLCode;
            }

            int bytesUsed = controlCost.GetBytesToUse();
            if (encryptedData.Length < bytesUsed)
                bytesUsed = encryptedData.Length;

            string code = encryptionController.GetOpenCLCode(bytesUsed, iv);
            if (code == null)
                throw new Exception("OpenCL not supported in this configuration!");

            //put cost function stuff into code:
            code = controlCost.ModifyOpenCLCode(code);

            //put input to be bruteforced into code:
            string inputarray = string.Format("__constant unsigned char inn[{0}] = {{ \n", bytesUsed);
            for (int i = 0; i < bytesUsed; i++)
            {
                inputarray += String.Format("0x{0:X2}, ", this.encryptedData[i]);
            }
            inputarray = inputarray.Substring(0, inputarray.Length - 2);
            inputarray += "}; \n";
            code = code.Replace("$$INPUTARRAY$$", inputarray);

            //put key movement of pattern into code:
            code = keyTranslator.ModifyOpenCLCode(code, approximateNumberOfKeys);

            keyTranslatorOfCode = keyTranslator;
            this.openCLCode = code;
            
            return code;
        }

        /// <summary>
        /// Generates the OpenCL code and creates the kernel out of it.
        /// </summary>
        /// <param name="oclManager">The OpenCL manager to use.</param>
        /// <param name="keyTranslator">The KeyTranslator to use. This is important for mapping the key movements to code.</param>
        /// <returns>The Kernel</returns>
        public Kernel GetBruteforceKernel(OpenCLManager oclManager, IKeyTranslator keyTranslator)
        {
            //caching:
            if (keyTranslatorOfCode == keyTranslator)
            {
                return openCLKernel;
            }

            try
            {
                var program = oclManager.CompileSource(CreateOpenCLBruteForceCode(keyTranslator));
                //keySearcher.GuiLogMessage(string.Format("Using OpenCL with (virtually) {0} threads.", keyTranslator.GetOpenCLBatchSize()), NotificationLevel.Info);
                openCLKernel = program.CreateKernel("bruteforceKernel");
                return openCLKernel;
            }
            catch (Exception ex)
            {
                throw new Exception(Resources.An_error_occured_when_trying_to_compile_OpenCL_code__ + ex.Message);
            }
        }
    }
}
