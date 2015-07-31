using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace NeuronDocumentSync.Cypher
{
    

    public class RSACypher : IRSACypher
    {
        private const string ServiceContainerName = "NeuronDocumentExport";
        private const int ProviderRsaFull = 1;

        private CspParameters GetCspParameters()
        {
            var cspParams = new CspParameters(ProviderRsaFull)
            {
                KeyContainerName = ServiceContainerName,
                Flags = CspProviderFlags.UseMachineKeyStore,
                ProviderName = "Microsoft Strong Cryptographic Provider"
            };
            return cspParams;
        }

        public void CreateKeys(out string publicKey)
        {
            var cp = GetCspParameters();

            var rsa = new RSACryptoServiceProvider(cp);
            rsa.PersistKeyInCsp = false;
            rsa.Clear();
            rsa = new RSACryptoServiceProvider(cp);
            publicKey = rsa.ToXmlString(false);
        }

        public string GetCurrentPublicKey()
        {
            var rsa = new RSACryptoServiceProvider(GetCspParameters());
            return (rsa.ToXmlString(false));
        }

        public string EncryptData(string data2Encrypt, string publickey = null)
        {

            // Create instance of Rijndael for
            // symetric encryption of the data.
            var rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;
            ICryptoTransform transform = rjndl.CreateEncryptor();

            var rsa = new RSACryptoServiceProvider(GetCspParameters());

            if (publickey != null)
                rsa.FromXmlString(publickey);

            byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenK = BitConverter.GetBytes(lKey);
            int lIV = rjndl.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);

            byte[] cipherbytes = null;
            using (var outMs = new MemoryStream())
            {

                outMs.Write(LenK, 0, 4);
                outMs.Write(LenIV, 0, 4);
                outMs.Write(keyEncrypted, 0, lKey);
                outMs.Write(rjndl.IV, 0, lIV);

                using (var outStreamEncrypted = new CryptoStream(outMs, transform, CryptoStreamMode.Write))
                {
                    byte[] plainbytes = System.Text.Encoding.UTF8.GetBytes(data2Encrypt);
                    outStreamEncrypted.Write(plainbytes, 0, plainbytes.Length);


                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }
                cipherbytes = outMs.ToArray();
                outMs.Close();
            }

            return Convert.ToBase64String(cipherbytes);
        }

        public string DecryptData(string data2Decrypt, string privatekey = null)
        {

            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;

            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            byte[] getpassword = Convert.FromBase64String(data2Decrypt);
            byte[] plain = null;

            using (var inMs = new MemoryStream(getpassword))
            {

                inMs.Seek(0, SeekOrigin.Begin);
                inMs.Seek(0, SeekOrigin.Begin);
                inMs.Read(LenK, 0, 3);
                inMs.Seek(4, SeekOrigin.Begin);
                inMs.Read(LenIV, 0, 3);

                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                int startC = lenK + lenIV + 8;
                int lenC = (int)inMs.Length - startC;

                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];


                inMs.Seek(8, SeekOrigin.Begin);
                inMs.Read(KeyEncrypted, 0, lenK);
                inMs.Seek(8 + lenK, SeekOrigin.Begin);
                inMs.Read(IV, 0, lenIV);

                var rsa = new RSACryptoServiceProvider(GetCspParameters());
                if (privatekey != null)
                    rsa.FromXmlString(privatekey);

                byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

                ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

                using (var outMs = new MemoryStream())
                {


                    inMs.Seek(startC, SeekOrigin.Begin);
                    using (var outStreamDecrypted = new CryptoStream(outMs, transform, CryptoStreamMode.Write))
                    {
                        var cData = new byte[lenC];
                        inMs.Read(cData, 0, cData.Length);
                        outStreamDecrypted.Write(cData, 0, lenC);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }
                    plain = outMs.ToArray();
                    outMs.Close();
                }
                inMs.Close();
            }
            string data = System.Text.Encoding.UTF8.GetString(plain);
            return data;
        }

        public string EncryptAndSerializeData<T>(T data, string publickey = null)
        {
            var formater = new XmlSerializer(data.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                formater.Serialize(textWriter, data);
                return EncryptData(textWriter.ToString(), publickey);
            }
        }

        public T DecryptAndDeserializeData<T>(string data2Decrypt, string privatekey = null)
        {
            var decryptedData = DecryptData(data2Decrypt, privatekey);
            var formater = new XmlSerializer(typeof(T));
            using (StringReader textReader = new StringReader(decryptedData))
            {
                return (T)formater.Deserialize(textReader);
            }
        }
    }
}