// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScsCrypto.cs" company="Swisscom AG">
//   Copyright (c) 2025
// </copyright>
// <summary>
//   ScsCrypto class to encrypt and decrypt text using
//   a specific key. The key will be transformed into
//   a SHA256 hash and afterwards in a MD5 hash
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Util
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class ScsCrypto
    {
        /// <summary>
        /// The default salt value used if no custom salt is provided.
        /// </summary>
        private static readonly string _defaultSalt = "c395641a-ea04-4b6f-8d2d-b07a5b3e08be";

        /// <summary>
        /// The encryption key derived from the provided key and salt.
        /// </summary>
        private readonly byte[] _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsCrypto"/> class with a key.
        /// Uses the default salt.
        /// </summary>
        /// <param name="key">The key for encryption.</param>
        public ScsCrypto(string key) : this(key, _defaultSalt) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsCrypto"/> class with a key and custom salt.
        /// </summary>
        /// <param name="key">The key for encryption.</param>
        /// <param name="salt">The custom salt value to use.</param>
        public ScsCrypto(string key, string salt)
        {
            var password = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(salt), 10000);
            this._key = password.GetBytes(32);
            password.Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsCrypto"/> class.
        /// </summary>
        public ScsCrypto()
        {
            this._key = CreateKey(Environment.MachineName);
        }

        /// <summary>
        /// Creates a SHA 512 hash from a given text.
        /// </summary>
        /// <param name="text">The text to encrypt.</param>
        /// <param name="rounds">The rounds.</param>
        /// <returns>The hashed text.</returns>
        public static string CreateSha512Hash(string text, int rounds)
        {
            var shaHasher = new SHA512Managed();
            shaHasher.Initialize();
            var hashedText = text;
            for (var i = 0; i <= rounds; i++)
            {
                hashedText = new StringBuilder(_defaultSalt + hashedText).ToString();
                var hashedBytes = shaHasher.ComputeHash(Encoding.UTF8.GetBytes(hashedText));
                hashedText = Convert.ToBase64String(hashedBytes);
            }

            return hashedText;
        }

        /// <summary>
        /// Encrypts text using the key set by the
        /// constructor.
        /// </summary>
        /// <param name="text">Text to encrypt.</param>
        /// <returns>The encrypted text.</returns>
        public string Encrypt(string text)
        {
            string encryptedText;
            var rijndael = new RijndaelManaged();
            rijndael.Key = this._key;
            rijndael.GenerateIV();
            var byteIv = rijndael.IV;
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteIv, 0, byteIv.Length);
                using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(Encoding.UTF8.GetBytes(text), 0, Encoding.UTF8.GetBytes(text).Length);
                    cryptoStream.FlushFinalBlock();
                    var output = memoryStream.ToArray();
                    encryptedText = Convert.ToBase64String(output);
                }
            }

            rijndael.Clear();
            return encryptedText;
        }

        /// <summary>
        /// Decrypts text using the key set by the
        /// constructor.
        /// </summary>
        /// <param name="text">Text to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        public string Decrypt(string text)
        {
            string decryptedText;
            var rijndael = new RijndaelManaged();

            var encryptedBytes = Convert.FromBase64String(text);

            var iv = new byte[16];
            Array.Copy(encryptedBytes, 0, iv, 0, 16);

            var cipherTextLength = encryptedBytes.Length - 16;
            var cipherText = new byte[encryptedBytes.Length - 16];
            Array.Copy(encryptedBytes, 16, cipherText, 0, cipherTextLength);

            rijndael.IV = iv;
            rijndael.Key = this._key;
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;
            using (var decryptor = rijndael.CreateDecryptor())
            using (var memoryStream = new MemoryStream(cipherText))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                {
                    decryptedText = streamReader.ReadToEnd();
                }
            }

            rijndael.Clear();
            return decryptedText;
        }

        /// <summary>
        /// Creates a SHA256 hash from the key and
        /// combines it with the SALT value.
        /// </summary>
        /// <param name="key">The key for encryption.</param>
        /// <returns>The key as a byte array.</returns>
        private static byte[] CreateKey(string key)
        {
            var shaHasher = new SHA256Managed();
            var salt = Encoding.UTF8.GetBytes(_defaultSalt.ToString());
            var byteKey = Encoding.UTF8.GetBytes(key.ToUpper());
            for (var i = 0; i <= 10000; i++)
            {
                var byteTemporaryKey = new byte[byteKey.Length + salt.Length];
                Buffer.BlockCopy(byteKey, 0, byteTemporaryKey, 0, byteKey.Length);
                Buffer.BlockCopy(salt, 0, byteTemporaryKey, byteKey.Length, salt.Length);
                byteKey = shaHasher.ComputeHash(byteTemporaryKey);
            }

            shaHasher.Clear();
            return byteKey;
        }
    }
}
