using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace GameLauncher
{
    public static class SeedEncryptor
    {
        // IV (Initialization Vector) (NT_KOREA)
        private static readonly byte[] iv = new byte[] 
        {
            56, 170, 255, 3, 4, 78, 6, 54, 8, 222, 10, 123, 19, 88, 14, 1
        };

        // DATA_LEN & SEED_USER_KEY_LEN
        private const int SEED_USER_KEY_LEN = 16;
        private const int DATA_LEN = 256; // BUFFER

        public static string Encode15(string plainText, string userKey)
        {
           // ENCODING
            byte[] keyBytes = Encoding.ASCII.GetBytes(userKey);
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            if (plainBytes.Length >= DATA_LEN)
            {
                throw new ArgumentException("Plaintext is too long.", nameof(plainText));
            }

            if (keyBytes.Length > SEED_USER_KEY_LEN)
            {
                throw new ArgumentException("UserKey is too long.", nameof(userKey));
            }

            // KEY 128 BIT
            byte[] finalKeyBytes = new byte[SEED_USER_KEY_LEN];
            Array.Copy(keyBytes, finalKeyBytes, keyBytes.Length);

            // BLOCK CHIPER 128 BIT
            var seedEngine = new SeedEngine();
            var cfbCipher = new CfbBlockCipher(seedEngine, 128); 
            IBufferedCipher cipher = new BufferedBlockCipher(cfbCipher);

            // PARAMETER KEY
            var keyParam = new KeyParameter(finalKeyBytes);
            var parameters = new ParametersWithIV(keyParam, iv);

            cipher.Init(true, parameters);

            // =========================================================================
            // BUFFER SIZE DATA_LEN
            byte[] bufferToEncrypt = new byte[DATA_LEN];
            Array.Copy(plainBytes, bufferToEncrypt, plainBytes.Length);
            // ENCRYPT ALL BUFFER
            byte[] encryptedBuffer = cipher.DoFinal(bufferToEncrypt);
            // =========================================================================
            var hexString = new StringBuilder(plainBytes.Length * 2);
            for (int i = 0; i < plainBytes.Length; i++)
            {
                hexString.Append(encryptedBuffer[i].ToString("x2"));
            }

            return hexString.ToString();
        }
    }
}
