namespace WebService
{
    public class WSSecurityTracer
    {
        public string signatureTrace = "\n Signature Validation Trace";
        public string decryptionTrace = "\n Decrypted Data Trace";






        public void appendReferenceValidation(string id, string hash)
        {
            signatureTrace += "\n Digest Value of Reference " + id + " " + hash;
        }

        public void appendDecryptedData(string id, string data)
        {
            decryptionTrace += "\n Decrypted Data with id " + id + " :" + data;
        }
    }
}
