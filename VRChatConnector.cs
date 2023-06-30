﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSCLock.Configs;
using OSCLock.Logic;
using SharpOSC;


namespace OSCLock {
    public static class VRChatConnector {

        //public static bool isVRChatRunning() {
        //    var instances = System.Diagnostics.Process.GetProcessesByName("VRChat");
        //    return instances.Length > 0;
        //}

        private static UDPListener oscListener;
        private static UDPSender oscSender;
        private static bool debugging = ConfigManager.ApplicationConfig.debugging;
        private static int listener_port = ConfigManager.ApplicationConfig.listener_port;
        private static int write_port = ConfigManager.ApplicationConfig.write_port;
        private static string ipAddress = ConfigManager.ApplicationConfig.ipAddress ?? "127.0.0.1";

        public delegate Task AddressHandler(OscMessage address);

        public static Dictionary<string, AddressHandler> addressHandlers = new Dictionary<string, AddressHandler>();

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

            Console.WriteLine($"mode: {ConfigManager.ApplicationConfig.mode}");
            Console.WriteLine($"debugging: {debugging}");

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
            }
            

            //todo: add one for avatar change
        }


        private static async void OnOscMessage(OscPacket packet) {
            if (debugging) Console.WriteLine("Package recieved: " + packet);
            try {
                if (packet is OscMessage message) {
                    AddressHandler handler;
                    if (debugging) Console.WriteLine($"{message.Address}" + $"({message.Arguments[0]})");
                    if (addressHandlers.TryGetValue(message.Address, out handler)) {
                        if (message.Arguments[0] is true) {
                            await handler(message);
                        }
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