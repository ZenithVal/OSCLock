using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSCLock.Bluetooth;
using OSCLock.Configs;
using OSCLock.Logic;

namespace OSCLock {
    internal class Program {
        public static bool isAllowedToUnlock = false;
        

        public static async Task Main(string[] args) {
            VRChatConnector.Start();
            Console.WriteLine("Going to home screen...");
            Thread.Sleep(1500);

            //while (true) {}
            
            await CmdPrompt();
        }


        public static async Task PrintHelp() {
            Console.WriteLine("=== HELP SCREEN ===");
            Console.WriteLine("h -- prints this menu");
            Console.WriteLine("t -- starts a new timer");
            Console.WriteLine("s -- prints status of the app and lock");
            Console.WriteLine("u -- Unlock device");
            Console.WriteLine("q -- quits the application");
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

            Console.WriteLine("Not to Unlock");
            if (connectedLock == null) {
                Console.WriteLine("You are not connected to a lock yet, starting bluetooth scanner\n");
                connectedLock = await BleScanner.FindESmartLock();
            }
            
            Console.WriteLine("Found device");
            
            //todo: Attempt unlock here
            await connectedLock.Unlock();
            
            Console.WriteLine("Removing lock as it turned of by now");
            connectedLock = null;
        }

        private static async Task StartTimer() {
            if (ConfigManager.ApplicationConfig.Mode == ApplicationMode.Timer) {
                await OSCTimer.Start();
            } else Console.WriteLine("Not operating in timer mode, change mode in config and restart");
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
                    default:
                        Console.WriteLine($"Unknown command {Key}, for help press 'h'");
                        break;
                }

                Console.Write(">");
            } while ((Key = Console.ReadKey().KeyChar.ToString().ToLower().ToCharArray().First()) != 'q');
        }
    }
}