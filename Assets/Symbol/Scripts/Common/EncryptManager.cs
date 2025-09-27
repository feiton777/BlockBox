using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SB
{
    public static class EncryptManager
    {
        private static RijndaelManaged CreateAes( string key, string salt )
        {
            RijndaelManaged aes = new RijndaelManaged();

            using(Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes( key, Encoding.UTF8.GetBytes( salt ), 1000 ))
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Key = rfc2898DeriveBytes.GetBytes( aes.KeySize / 8 );
                aes.IV = rfc2898DeriveBytes.GetBytes( aes.BlockSize / 8 );

                return aes;
            }
        }
        public static byte[] Encrypt( byte[] plainTextBytes, string key, string salt )
        {
            using(RijndaelManaged aes = CreateAes( key, salt ))
            {
                using(MemoryStream memoryStream = new MemoryStream())
                using(ICryptoTransform transform = aes.CreateEncryptor())
                using(CryptoStream cryptoStream = new CryptoStream( memoryStream, transform, CryptoStreamMode.Write ))
                {
                    using(BinaryWriter binaryWriter = new BinaryWriter( cryptoStream ))
                    {
                        binaryWriter.Write( plainTextBytes );
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] Decrypt( byte[] cipherTextBytes, string key, string salt )
        {
            using(RijndaelManaged aes = CreateAes( key, salt ))
            using(MemoryStream memoryStream = new MemoryStream( cipherTextBytes ))
            using(ICryptoTransform transform = aes.CreateDecryptor())
            using(CryptoStream cryptoStream = new CryptoStream( memoryStream, transform, CryptoStreamMode.Read ))
            using(BinaryReader binaryReader = new BinaryReader( cryptoStream ))
            {
                byte[] array = new byte[ cipherTextBytes.Length ];
                int newSize = binaryReader.Read( array, 0, array.Length );
                Array.Resize( ref array, newSize );
                return array;
            }
        }
    }
}
