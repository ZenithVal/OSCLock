using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OSCLock.Configs;
using OSCLock.Logic;
using SharpOSC;


namespace OSCLock {
    public static class VRChatConnector {

        //App can run without VRChat so this doesn't really serve a purpose.
        //public static bool isVRChatRunning() {
        //    var instances = System.Diagnostics.Process.GetProcessesByName("VRChat");
        //    return instances.Length > 0;
        //}

        private static UDPListener oscListener;
        private static UDPSender oscSender;
        private static bool debugging;
        private static int listener_port;
        private static int write_port;
        private static string ipAddress;

        public delegate Task AddressHandler(OscMessage address);

        public static Dictionary<string, AddressHandler> addressHandlers = new Dictionary<string, AddressHandler>();

        public static void Start() {
            if (oscListener != null) {
                oscListener.Close();
                addressHandlers.Clear();
                Program.isAllowedToUnlock = false;
            }

            //Load VRchat Connector settings.
            try {
                listener_port = ConfigManager.ApplicationConfig.listener_port;
                write_port = ConfigManager.ApplicationConfig.write_port;
                ipAddress = ConfigManager.ApplicationConfig.ipAddress ?? "127.0.0.1";
                debugging = ConfigManager.ApplicationConfig.debugging;

                if (ipAddress == "127.0.0.1") Console.WriteLine("ip: LocalHost");
                //If debugging, display the whole IP.
                else if (debugging) Console.WriteLine("ip: " + ipAddress);
                //If not localhost, partially hide the IP. Just in case.
                else Console.WriteLine("ip: " + ipAddress.Substring(0, 3) + "###.###." + ipAddress.Substring(ipAddress.Length - 3, 3));

                Console.WriteLine("listener_port: " + listener_port);
                Console.WriteLine("write_port: " + write_port);

                Console.WriteLine($"mode: {ConfigManager.ApplicationConfig.mode}");
                Console.WriteLine($"debugging: {debugging}");
            }
            catch (Exception e) {
                Console.WriteLine($"Connector config load failed: {e.Message}\n\nPlease check your config file and reboot.");
                Task.Delay(5000).Wait();
                Environment.Exit(0);
            }

            //Boot OSC Listener and Sender
            try {
                oscListener = new UDPListener(listener_port, OnOscMessage);
                oscSender = new UDPSender(ipAddress, write_port);

            }
            //90% of the time this will fail because the port they attempted to use is already occupied.
            catch (Exception e) {
                Console.WriteLine($"\nUDPListener failed: {e.Message}\n\nMake sure you're not attempting to run two apps on the same port.");
                Task.Delay(5000).Wait();
                Environment.Exit(0);
            }

            //Select mode
            switch (ConfigManager.ApplicationConfig.mode) {
                case ApplicationMode.Testing:
                    Program.isAllowedToUnlock = true;
                    break;
                case ApplicationMode.Basic:
                    OSCBasic.Setup();
                    break;
                case ApplicationMode.Timer:
                    OSCTimer.Setup();
                    break;

                //Not implemented yet
                //case ApplicationMode.Counter: 
                //    OSCCounter.Setup();
                //break;

                //case for invalid mode
                default:
                    Console.WriteLine("Invalid mode selected, please check your config file.");
                    Task.Delay(5000).Wait();
                    Environment.Exit(0);
                    break;


            }
            

            //todo: add one for avatar change' - Neet
            //Actually, what would be the purpose of that? - Zeni
        }


        private static async void OnOscMessage(OscPacket packet) {
            if (debugging) Console.WriteLine("Package recieved: " + packet);
            try {
                if (packet is OscMessage message) {
                    AddressHandler handler;
                    if (debugging) Console.WriteLine($"{message.Address}" + $"({message.Arguments[0]})");
                    if (addressHandlers.TryGetValue(message.Address, out handler)) {
                        //Better to do the bool true/false check in the handler since basic mode also listens for false.
                        //if (message.Arguments[0] is true) {
                            await handler(message);
                        //}
                    }
                }
            }
            catch (InvalidCastException e) {
                Console.WriteLine("Wrong parameter type written!" + e, e);
            }
            catch (Exception e) {
                Console.WriteLine("Failed to handle osc message: " + e, e);
            }
        }

        public static void AddHandler(string addr, AddressHandler handler) {
            addressHandlers[addr] = handler;
            if (debugging) Console.WriteLine($"Installed OSC handler for address {addr}");
        }

        //Might need a queue here, as it might be possible to send too many messages at once.
        public static void SendToVRChat(OscMessage message) {
            try {
                oscSender.Send(message);
            }
            catch (Exception e) {
                Console.WriteLine("Failed to send message to vrchat " + e.Message);
            }
        }
    }
}