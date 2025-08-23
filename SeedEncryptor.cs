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
        // IV (Initialization Vector) yang sama persis dengan di kode C++ (NT_KOREA)
        private static readonly byte[] iv = new byte[] 
        {
            56, 170, 255, 3, 4, 78, 6, 54, 8, 222, 10, 123, 19, 88, 14, 1
        };

        // DATA_LEN dan SEED_USER_KEY_LEN dari C++, biasanya 16 atau 32. Kita asumsikan 16 untuk key.
        private const int SEED_USER_KEY_LEN = 16;
        private const int DATA_LEN = 256; // Ukuran buffer tetap seperti di C++

        public static string Encode15(string plainText, string userKey)
        {
            // Konversi input string ke byte array menggunakan encoding standar
            byte[] keyBytes = Encoding.ASCII.GetBytes(userKey);
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            if (plainBytes.Length >= DATA_LEN)
            {
                // Meniru pengecekan batas panjang dari C++
                throw new ArgumentException("Plaintext is too long.", nameof(plainText));
            }

            if (keyBytes.Length > SEED_USER_KEY_LEN)
            {
                throw new ArgumentException("UserKey is too long.", nameof(userKey));
            }

            // Kunci harus memiliki panjang tepat 16 byte (128 bit) untuk SEED
            // Jika lebih pendek, kita pad dengan nol, meniru perilaku C++
            byte[] finalKeyBytes = new byte[SEED_USER_KEY_LEN];
            Array.Copy(keyBytes, finalKeyBytes, keyBytes.Length);

            // Membuat cipher SEED dengan mode CFB (Cipher Feedback) dan tanpa padding,
            // sama persis seperti di C++. Ukuran blok feedback adalah 128 bit.
            var seedEngine = new SeedEngine();
            var cfbCipher = new CfbBlockCipher(seedEngine, 128); 
            IBufferedCipher cipher = new BufferedBlockCipher(cfbCipher);

            // Siapkan parameter kunci dan IV
            var keyParam = new KeyParameter(finalKeyBytes);
            var parameters = new ParametersWithIV(keyParam, iv);

            // Inisialisasi cipher untuk enkripsi
            cipher.Init(true, parameters);

            // =========================================================================
            // BAGIAN PENTING: Meniru perilaku C++ yang menggunakan buffer ukuran tetap
            // 1. Buat buffer dengan ukuran DATA_LEN
            byte[] bufferToEncrypt = new byte[DATA_LEN];
            // 2. Salin plaintext ke dalam buffer tersebut
            Array.Copy(plainBytes, bufferToEncrypt, plainBytes.Length);
            // 3. Enkripsi seluruh buffer
            byte[] encryptedBuffer = cipher.DoFinal(bufferToEncrypt);
            // =========================================================================

            // Konversi hasil enkripsi (byte array) ke string heksadesimal (lowercase)
            // PENTING: Kode C++ hanya mengambil sebagian dari hasil enkripsi,
            // yaitu sepanjang plaintext aslinya. Kita harus menirunya.
            var hexString = new StringBuilder(plainBytes.Length * 2);
            for (int i = 0; i < plainBytes.Length; i++)
            {
                hexString.Append(encryptedBuffer[i].ToString("x2"));
            }

            return hexString.ToString();
        }
    }
}