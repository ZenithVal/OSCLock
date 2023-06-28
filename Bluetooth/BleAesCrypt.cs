using System;
using System.Security.Cryptography;
using System.Text;

namespace OSCLock.Bluetooth {
    public static class BleAesCrypt {
        public static ICryptoTransform encryptor;
        public static ICryptoTransform decryptor;
        public static UTF8Encoding UTF8 = new UTF8Encoding();
        public static string KEY_STRING = "7b7079bb69001dce";

        static BleAesCrypt() {
            AesManaged aes = new AesManaged();
            aes.Key = UTF8.GetBytes(KEY_STRING);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7; //Should be PKCS5 hopefully this works
            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
        }
        
        public static byte[] Encrypt(byte[] plaintext) {
            return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
        }

        public static byte[] Decrypt(byte[] encrypted) {
            return decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        }
    }
}