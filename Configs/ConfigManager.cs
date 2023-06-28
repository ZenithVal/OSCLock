using System;
using System.IO;
using Tomlet;
using Tomlet.Models;

namespace OSCLock.Configs {
    public static class ConfigManager {
        private static string CONFIG_FILE = "config.toml";
        public static MainConfig ApplicationConfig;
        private static TomlDocument lastDocument;


        static ConfigManager() {
            if (!File.Exists(CONFIG_FILE))
                Reset();
            InitConfig();
        }

        private static void InitConfig() {
            try {
                lastDocument = TomlParser.ParseFile(CONFIG_FILE);
                ApplicationConfig = TomletMain.To<MainConfig>(lastDocument);
            }
            catch (Exception e) {
                Console.WriteLine("FAILED TO READ CONFIG FILE!!!" + e, e);
            }
        }

        public static void Save() {
            try {
                var value = TomletMain.DocumentFrom(ApplicationConfig);
                File.WriteAllText(CONFIG_FILE, value.SerializedValue);
                InitConfig();                
            }
            catch (Exception e) {
                Console.WriteLine("Failed to save config reason: " + e, e);
                Console.Write("Trying to restore old config...");
                try {
                    File.WriteAllText(CONFIG_FILE, lastDocument.SerializedValue);
                    InitConfig();
                    Console.WriteLine("SUCCESS");
                } catch (Exception w) {
                    Console.WriteLine("FAILED!!" + e, e);
                }
            }
        }

        public static void Reset() {
            File.Delete(CONFIG_FILE);
            File.WriteAllText(CONFIG_FILE, TomletMain.TomlStringFrom(new MainConfig {
                port = 9001,
                vrchatPort = 9000,
                vrchatAddress = "127.0.0.1",
                apiUsername = "",
                apiPassword = "",
                Mode = ApplicationMode.Testing,
                BasicConfig = new BasicMode {
                    parameter = "/avatar/parameters/unlock"
                },
                TimerConfig = new TimerMode {
                    maxTime = 240,
                    absMin = 20,
                    defaultTime = new DefaultTime {
                        startingValue = -1,
                        randomMin = 10,
                        randomMax = 20
                    },
                    inc_parameter = "/avatar/parameters/timer_inc",
                    inc_step = 20,
                    dec_parameter = "/avatar/parameters/timer_dec",
                    dec_step = 10,
                    readout_parameter = "/avatar/parameters/timer_value",
                    readout_interval = 500
                }
            }));
        }
    }
}