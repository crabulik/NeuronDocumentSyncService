namespace NeuronDocumentSync.Cypher
{
    public interface IRSACypher
    {
        void CreateKeys(out string publicKey);
        string GetCurrentPublicKey();
        string EncryptData(string data2Encrypt, string publickey = null);
        string DecryptData(string data2Decrypt, string privatekey = null);
        string EncryptAndSerializeData<T>(T data, string publickey = null);
        T DecryptAndDeserializeData<T>(string data2Decrypt, string privatekey = null);
    }
}