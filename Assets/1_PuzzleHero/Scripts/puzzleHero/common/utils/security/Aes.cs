using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace phongvan.common.utils.security
{
    class Aes
    {
        private const int CHUNK_SIZE = 0x80;
        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly RijndaelManaged _rijndael;

        public Aes()
        {
            this._rijndael = new RijndaelManaged();
            var key = new byte[] { 3, 5, 0x1f, 2, 5, 6, 3, 0x15, 6, 0x17, 5, 0x20, 0x2a, 0x1f, 0x56, 0x20 };
            this._create(key, key);
        }


        private void _create(byte[] key, byte[] iv)
        {
            this.InitializeRijndael();
            this._rijndael.Key = key;
            this._rijndael.IV = iv;
        }

        private void InitializeRijndael()
        {
            this._rijndael.Mode = CipherMode.CBC;
            this._rijndael.Padding = PaddingMode.PKCS7;
            this._rijndael.KeySize = CHUNK_SIZE;
            this._rijndael.BlockSize = CHUNK_SIZE;
        }

        public Aes(string base64Key, string base64Iv)
        {
            this._rijndael = new RijndaelManaged();
            this._create(Convert.FromBase64String(base64Key), Convert.FromBase64String(base64Iv));
        }

        public Aes(byte[] key, byte[] iv)
        {
            this._rijndael = new RijndaelManaged();
            this._create(key, iv);
        }


        public string Decrypt(string encryptedString)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                return null;
            }
            ICryptoTransform transform = this._rijndael.CreateDecryptor();
            try
            {
                byte[] inputBuffer = Convert.FromBase64String(encryptedString);
                byte[] bytes = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                return encoding.GetString(bytes);
            }
            catch (Exception)
            {
                return encryptedString;
            }
        }

        public string Encrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }
            ICryptoTransform transform = this._rijndael.CreateEncryptor();
            try
            {
                byte[] bytes = encoding.GetBytes(cipherText);
                return Convert.ToBase64String(transform.TransformFinalBlock(bytes, 0, bytes.Length));
            }
            catch (Exception)
            {
                return cipherText;
            }
        }

    }
}
