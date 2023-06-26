using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSCLock.Bluetooth;
using OSCLock.Configs;
using OSCLock.Logic;
using System.IO;
using Bluetooth.GATT;

namespace OSCLock {
    internal class Program {
        public static bool isAllowedToUnlock = false;
        public static bool isEncrypted = false;
        public static string appPassword = "";

        public static async Task Main(string[] args) {

            if (File.Exists("app.pass"))
            {
                isEncrypted = true;
                //Idk how to really hide this. Too lazy to hide it in RAM either. Very secure, I know.
                appPassword = Encryption.Read("app.pass", "7b7079bb6379001dce"); 
                Console.WriteLine("Application Config is encrypted.");
                //Console.WriteLine(appPassword);
            }

            VRChatConnector.Start();
            Console.WriteLine("Going to home screen...");
            Thread.Sleep(1500);

            //while (true) {}
            
            await CmdPrompt();
        }


        public static async Task PrintHelp() {
            Console.WriteLine("=== HELP SCREEN ===");
            Console.WriteLine("H -- prints this menu");
            Console.WriteLine("T -- starts a new timer");
            Console.WriteLine("S -- prints status of the app and lock");
            Console.WriteLine("U -- Unlock device");
            Console.WriteLine("Q -- quits the application");
            Console.WriteLine("F7 -- Encrypts config & Timer files");
            Console.WriteLine("F8 -- Decrypts config & Timer files");
        }

        private static async Task PrintStatus() {
            var appConfig = ConfigManager.ApplicationConfig;
            Console.WriteLine($"Operating in {appConfig.Mode} mode.");
            if (appConfig.Mode == ApplicationMode.Timer) {
                Console.WriteLine("Time left: " + (int)OSCTimer.GetTimeLeftTotal() + " minutes");
            }
            Console.WriteLine("Allowed to unlock: " + isAllowedToUnlock);
        }

        private static ESmartLock connectedLock;
        public static async Task UnlockDevice() {
            if (!isAllowedToUnlock) {
                Console.WriteLine("You are not allowed to unlock yet!" + isAllowedToUnlock);
                await PrintStatus();
                return;
            }

            if (connectedLock == null) {
                Console.WriteLine("You are not connected to a lock yet, starting bluetooth scanner\n");
                connectedLock = await BleScanner.FindESmartLock();
            }
            
            Console.WriteLine("Found device");
            
            //todo: Attempt unlock here
            await connectedLock.Unlock();
            
            Console.WriteLine("Removing lock as it turned of by now");
            connectedLock = null;
            Thread.Sleep(1000);
            Console.Clear();
            await PrintHelp();
        }

        private static async Task StartTimer() {
            if (ConfigManager.ApplicationConfig.Mode == ApplicationMode.Timer) {
                await OSCTimer.Start();
            } else Console.WriteLine("Not operating in timer mode, change mode in config and restart");
        }

        private static async Task EncryptApp() { 

            //if OSCTimer.HasTimeElapsed is false, don't allow encryption
            if (!OSCTimer.HasTimeElapsed()) {
                Console.WriteLine("A timer is already running, you need to finish that before attempting encryption.");
                await PrintStatus();
                return;
            }

            if (isEncrypted)
            {
                Console.WriteLine("Application is already encrypted!");
                await PrintStatus();
                return;
            }
            
            Encryption.EncryptApp();
            
            Console.WriteLine("Encryption Complete");
            isEncrypted = true;
            Thread.Sleep(1000);
            Console.Clear();
            await PrintHelp();
        }

        private static async Task DecryptApp()
        {
            //if OSCTimer.HasTimeElapsed is false, don't allow encryption
            if (!OSCTimer.HasTimeElapsed())
            {
                Console.WriteLine("A timer is already running, you need to finish that before attempting decryption.");
                await PrintStatus();
                return;
            }

            if (!isEncrypted)
            {
                Console.WriteLine("Application is already decrypted!");
                await PrintStatus();
                return;
            }
            
            Encryption.DecryptApp();
            
            Console.WriteLine("Decryption Complete");
            isEncrypted = false;
            Thread.Sleep(1000);
            Console.Clear();
            await PrintHelp();
        }

        private static async Task CmdPrompt() {
            char Key = 'h';

            do {
                Console.Clear();
                switch (Key) {
                    case 's':
                        await PrintStatus();
                        break;
                    case 'h':
                        await PrintHelp();
                        break;
                    case 't':
                        await StartTimer();
                        break;
                    case 'u':
                        await UnlockDevice();
                        break;
                    case ']':
                        await EncryptApp();
                        break;
                    case '[':
                        await DecryptApp();
                        break;
                    default:
                        Console.WriteLine($"Unknown command {Key}");
                        await PrintHelp();
                        break;
                }

                Console.Write(">");
            } while ((Key = Console.ReadKey().KeyChar.ToString().ToLower().ToCharArray().First()) != 'q');
        }
    }
}