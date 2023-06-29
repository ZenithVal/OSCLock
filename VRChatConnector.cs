using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OSCLock.Configs;
using OSCLock.Logic;
using SharpOSC;
using Windows.Security.Cryptography.Core;

namespace OSCLock {
    public static class VRChatConnector {

        //public static bool isVRChatRunning() {
        //    var instances = System.Diagnostics.Process.GetProcessesByName("VRChat");
        //    return instances.Length > 0;
        //}

        private static UDPListener oscListener;
        private static UDPSender oscSender;
        private static int listener_port = ConfigManager.ApplicationConfig.listener_port;
        private static int write_port = ConfigManager.ApplicationConfig.write_port;
        private static string ipAddress = ConfigManager.ApplicationConfig.ipAddress ?? "127.0.0.1";

        public delegate Task AddressHandler(OscMessage address);

        private static Dictionary<string, AddressHandler> addressHandlers = new Dictionary<string, AddressHandler>();

        public static void Start() {
            if (oscListener != null) {
                oscListener.Close();
                addressHandlers.Clear();
                Program.isAllowedToUnlock = false;
            }
            
            oscListener = new UDPListener(listener_port, OnOscMessage);
            Console.WriteLine("listener_port: " + listener_port);

            oscSender = new UDPSender(ipAddress, write_port);
            Console.WriteLine("write_port: " + write_port);

            //Disabled other modes, as they are not used.
            OSCTimer.Setup();

            //switch (ConfigManager.ApplicationConfig.Mode) {
            //    case ApplicationMode.Testing:
            //        Program.isAllowedToUnlock = true;
            //        break;
            //    case ApplicationMode.Basic:
            //        OSCBasic.Setup();
            //        break;
            //    case ApplicationMode.Timer:
            //        OSCTimer.Setup();
            //        break;
            //}

            //todo: add one for avatar change
        }


        private static async void OnOscMessage(OscPacket packet) {
            Console.WriteLine(packet);
            try {
                if (packet is OscMessage message) {
                    AddressHandler handler;
                    if (addressHandlers.TryGetValue(message.Address, out handler)) {
                        if (message.Arguments[0] is true) {
                            await handler(message);
                        }
                        //Checks if the message recieved (only bother accepting true messages.)
                    }

                    //Console.WriteLine($"{message.Address} Recieved");
                }
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine("Wrong parameter type written!" + e, e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to handle osc message: " + e, e);
            }
        }

        public static void AddHandler(string addr, AddressHandler handler) {
            addressHandlers[addr] = handler;
            Console.WriteLine($"Installed OSC handler for address {addr}");
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