using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FluentColorConsole;
using OSCLock.Logic;

//This is mostly just fun security theater. Strings are not protected in memory, and you have access to the source code anyway.

namespace OSCLock {

    public static class Encryption
    {
        public static string Read(string filePath, string encryptionKey)
        {
            //Read incoming data and decrypt it
            string EncryptedData = File.ReadAllText(filePath);
            string DecryptedData = Decrypt(EncryptedData, encryptionKey);

            return DecryptedData;
        }


        public static void Write(string filePath, string data, string encryptionKey)
        {
            //Encrypt incoming data with the encryption key and write it to the file
            string encryptedData = Encrypt(data, encryptionKey);

            try
            {
                File.WriteAllText(filePath, encryptedData);
                //Console.WriteLine("Successfully Encrypted File");
            }
            catch (Exception e)
            {
                ColorConsole.WithRedText.WriteLine("Failed to write encrypted file" + e.Message);
            }
        }

        // Decrypt a string using AES
        public static string Decrypt(string encryptedData, string encryptionKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] ivBytes = new byte[16];

            // Adjust the key size if necessary
            if (keyBytes.Length < 32)
            {
                Array.Resize(ref keyBytes, 32);
            }
            else if (keyBytes.Length > 32)
            {
                Array.Resize(ref keyBytes, 32);
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }

                    byte[] decryptedBytes = ms.ToArray();
                    string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                    return decryptedText;
                }
            }
        }

        // Encrypt using AES. 
        public static string Encrypt(string decryptedData, string encryptionKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] ivBytes = new byte[16];

            if (keyBytes.Length < 32)
            {
                Array.Resize(ref keyBytes, 32);
            }
            else if (keyBytes.Length > 32)
            {
                Array.Resize(ref keyBytes, 32);
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(decryptedData);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }

                    byte[] encryptedBytes = ms.ToArray();
                    string encryptedText = Convert.ToBase64String(encryptedBytes);

                    return encryptedText;
                }
            }
        }

        public static void EncryptApp()
            {
            //Request the user type a password
            Console.WriteLine("Enter a password to encrypt configurations with.");
            Console.WriteLine("Pasting from the clipboard works. Maybe ask a friend for a code?");
            string password = Console.ReadLine();

            //Require the user to type it again, just in case.
            Console.WriteLine("Please confirm the password.");
            string passwordConfirm = Console.ReadLine();

            //If the passwords do not match, ask the user to try again
            if (password != passwordConfirm)
            {
                Console.WriteLine("Passwords did not match.");
                EncryptApp();
            }

            //Require the user to Press Y  N to CONFIRM the password
            Console.WriteLine("Press Y to Confirm or N to Cancel");
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.Y)
            {
                return;
            }

            Console.WriteLine("\nEncrypting...");
            //Create a app.pass file with the password and encrypt it using a generic key.
            Program.appPassword = password;
            string appPassPath = "app.pass";
            Encryption.Write(appPassPath, password, "7b7079bb6379001dce"); //Really secret, I know.

            //Encrypt the config.toml file with the password
            string configPath = "config.toml";
            string configData = File.ReadAllText(configPath);
            Encryption.Write(configPath, configData, password);

            //Encrypt the existing timer.end and timer.start files, IF THEY EXIST.
            var timersExist = File.Exists("timer.start") && File.Exists("timer.end");
            if (timersExist)
            {
                string timerStartPath = "timer.start";
                string timerStartData = File.ReadAllText(timerStartPath);
                Encryption.Write(timerStartPath, timerStartData, password);

                string timerEndPath = "timer.end";
                string timerEndData = File.ReadAllText(timerEndPath);
                Encryption.Write(timerEndPath, timerEndData, password);
            }

            Thread.Sleep(500);

            Console.WriteLine("Encryption Complete\n");
            Program.isEncrypted = true;

            Thread.Sleep(500);
        }


        public static void DecryptApp()
        {
            //if a timer is already running, allow the user to use the key as a failsafe.
            if (!OSCTimer.HasTimeElapsed())
            {
                Console.WriteLine("A timer is already running, decrypting will terminate it.\n");
                Thread.Sleep(500);

            }

            Console.WriteLine("Password: ");
            string password = Console.ReadLine();

            //If the passwords do not match, boot the to the main screen.
            if (password != Program.appPassword) //LEAST SECURE WAY TO DO THIS. GIVE ZENI AN AWARD.
            {
                Console.WriteLine("Passwords did not match.");
                Thread.Sleep(800);
                return;
            }

            Console.WriteLine("Decrypting...");
            //Delete the app.pass file
            string appPassPath = "app.pass";
            File.Delete(appPassPath);

            //Decrypt the config.toml file with the password
            string configPath = "config.toml";
            string configData = File.ReadAllText(configPath);
            string decryptedConfig = Decrypt(configData, password);
            File.WriteAllText(configPath, decryptedConfig);

            Thread.Sleep(500);

            Console.WriteLine("Decryption Complete\n");
            Program.isEncrypted = false;

            if (!OSCTimer.HasTimeElapsed())
            {
               OSCTimer.ForceEnd();
            }

            //Delete existing encrypted timer files.
            string timerStartPath = "timer.start";
            string timerEndPath = "timer.end";

            var timersExist = File.Exists(timerStartPath) && File.Exists(timerEndPath);
            if (timersExist)
            {
                File.Delete(timerStartPath);
                File.Delete(timerEndPath);
            }

            Thread.Sleep(1500);
        }

    }
    }
