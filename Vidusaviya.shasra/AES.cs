using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Vidusaviya.shasra
{


    public class AesRij
    {
        public RijndaelManaged rijndael = new RijndaelManaged();

        private readonly int CHUNK_SIZE = 128;

        private void InitializeRijndael()
        {
        }

        public AesRij()
        {
            InitializeRijndael();

            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.KeySize = CHUNK_SIZE;
            rijndael.BlockSize = CHUNK_SIZE;

            rijndael.GenerateKey();
            rijndael.GenerateIV();
        }



        public AesRij(String base64key, String base64iv, int chunkSize = 128, CipherMode mode = CipherMode.ECB)
        {
            CHUNK_SIZE = chunkSize;

            rijndael.Mode = mode;
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.Key = Convert.FromBase64String(base64key);
            rijndael.IV = Convert.FromBase64String(base64iv);
        }


        /// <summary>
        /// Must be 16 bytes
        /// </summary>
        public AesRij(byte[] key, byte[] iv, int chunkSize = 128, CipherMode mode = CipherMode.ECB)
        {
            CHUNK_SIZE = chunkSize;

            rijndael.Mode = mode;
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.Key = key;
            rijndael.IV = iv;
        }


        /// <summary>
        /// passwordBytes can be any size
        /// </summary>
        public AesRij(byte[] passwordBytes, byte[] saltBytes, int iterations = 100, int chunkSize = 128, CipherMode mode = CipherMode.ECB)
        {
            CHUNK_SIZE = chunkSize;

            rijndael.Mode = mode;
            rijndael.Padding = PaddingMode.PKCS7;

            using var keygen = new Rfc2898DeriveBytes(passwordBytes, saltBytes, iterations);
            rijndael.Key = keygen.GetBytes(CHUNK_SIZE / 8);
            rijndael.IV = new byte[CHUNK_SIZE / 8];
        }


        public AesRij(string password, int chunkSize = 128, CipherMode mode = CipherMode.ECB)
        {
            byte[] PasswordBytes = Encoding.Unicode.GetBytes(password);
            byte[] Salt = new byte[PasswordBytes.Length];
            for (int i = 0; i < PasswordBytes.Length; i++)
            {
                Salt[i] = (byte)((PasswordBytes[i] + i) % 256);
            }


            CHUNK_SIZE = chunkSize;

            rijndael.Mode = mode;
            rijndael.Padding = PaddingMode.PKCS7;


            using var keygen = new Rfc2898DeriveBytes(PasswordBytes, Salt, 100);
            rijndael.Key = keygen.GetBytes(CHUNK_SIZE / 8);
            rijndael.IV = new byte[CHUNK_SIZE / 8];


        }

        public static byte[] AESKeyFromString(string password, int chunkSize = 128)
        {
            byte[] PasswordBytes = Encoding.Unicode.GetBytes(password);
            byte[] Salt = new byte[PasswordBytes.Length];
            for (int i = 0; i < PasswordBytes.Length; i++)
            {
                Salt[i] = (byte)((PasswordBytes[i] + i) % 256);
            }

            using var keygen = new Rfc2898DeriveBytes(PasswordBytes, Salt, 100);
            return keygen.GetBytes(chunkSize / 8);

        }


        public byte[] Decrypt(byte[] cipher)
        {
            using ICryptoTransform transform = rijndael.CreateDecryptor();
            return transform.TransformFinalBlock(cipher, 0, cipher.Length);

        }

        public byte[] Encrypt(byte[] cipher)
        {
            using ICryptoTransform encryptor = rijndael.CreateEncryptor();
            return encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        }



        public string DecryptBase64String(string base64cipher)
        {
            return Convert.ToBase64String(DecryptFromBase64String(base64cipher));
        }
        public string EncryptBase64String(string plain)
        {
            return Convert.ToBase64String(EncryptFromBase64String(plain));
        }


        public byte[] DecryptFromBase64String(string base64cipher)
        {
            return Decrypt(Convert.FromBase64String(base64cipher));
        }
        public string DecryptToBase64String(byte[] cipher)
        {
            return Convert.ToBase64String(Decrypt(cipher));
        }


        public byte[] EncryptFromBase64String(string plain)
        {
            return Encrypt(Convert.FromBase64String(plain));
        }
        public string EncryptToBase64String(byte[] plain)
        {
            return Convert.ToBase64String(Encrypt(plain));
        }




        public string GetKey()
        {
            return Convert.ToBase64String(rijndael.Key);
        }

        public string GetIV()
        {
            return Convert.ToBase64String(rijndael.IV);
        }

        public override string ToString()
        {
            return "KEY:" + GetKey() + Environment.NewLine + "IV:" + GetIV();
        }
    }
}
