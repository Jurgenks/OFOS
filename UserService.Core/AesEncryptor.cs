using System.Security.Cryptography;
using System.Text;

namespace UserService.Core
{
    public static class AesEncryptor
    {
        // The key and initialization vector used for encryption and decryption
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("16characterkey!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("16characterIV!!");

        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            // Create a new Aes object to perform symmetric encryption
            using var aesAlg = Aes.Create();

            // Set the key and initialization vector
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor object to perform the stream transform
            using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create a memory stream to hold the encrypted data
            using var msEncrypt = new MemoryStream();

            // Create a crypto stream that transforms the data as it is written to and read from the memory stream
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                // Convert the plaintext string to a byte array
                var plainBytes = Encoding.UTF8.GetBytes(plainText);

                // Write the plaintext bytes to the crypto stream
                csEncrypt.Write(plainBytes, 0, plainBytes.Length);

                // Flush the crypto stream to ensure all plaintext bytes have been encrypted
                csEncrypt.FlushFinalBlock();
            }

            // Convert the encrypted bytes to a base64-encoded string and return it
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            if (cipherText == null)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            // Create a new Aes object to perform symmetric encryption
            using var aesAlg = Aes.Create();

            // Set the key and initialization vector
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor object to perform the stream transform
            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Convert the base64-encoded cipher text to a byte array
            var cipherBytes = Convert.FromBase64String(cipherText);

            // Create a memory stream to hold the decrypted data
            using var msDecrypt = new MemoryStream(cipherBytes);

            // Create a crypto stream that transforms the data as it is written to and read from the memory stream
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            // Create a byte array to hold the plaintext data
            using var msPlain = new MemoryStream();
            var buffer = new byte[1024];
            var bytesRead = 0;

            // Read the decrypted data from the crypto stream and write it to the plaintext memory stream
            while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
            {
                msPlain.Write(buffer, 0, bytesRead);
            }

            // Convert the plaintext bytes to a string and return it
            return Encoding.UTF8.GetString(msPlain.ToArray());
        }
    }

}
