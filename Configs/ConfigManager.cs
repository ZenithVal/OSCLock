using System;
using System.IO;
using System.Threading;
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


            try 
            {
                //Check if isEncrypted is true, if so we need to decrypt it before attempting to parse it.
                if (Program.isEncrypted)
                {
                    string decryptedConfig = (Encryption.Read(CONFIG_FILE, Program.appPassword));
                    TomlParser parser = new TomlParser();

                    //Now that it's decrypted, lets parse it with Toml it
                    lastDocument = parser.Parse(decryptedConfig);

                }
                else
                {
                    lastDocument = TomlParser.ParseFile(CONFIG_FILE);
                }

                //Print the decrypted config file to the console
                //Console.WriteLine(lastDocument.SerializedValue);
                //Thread.Sleep(5000);

                ApplicationConfig = TomletMain.To<MainConfig>(lastDocument);
            }
            catch (Exception e) {
                Console.WriteLine("FAILED TO READ CONFIG FILE!" + e, e);
                Thread.Sleep(5000);
            }


        }

        public static void Save() {
            try {
                var configData = TomletMain.DocumentFrom(ApplicationConfig);
                if (Program.isEncrypted)
                {
                    Encryption.Write(CONFIG_FILE, configData.SerializedValue, Program.appPassword);
                }
                else
                {
                    File.WriteAllText(CONFIG_FILE, configData.SerializedValue);
                }
                    
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
            File.Delete("app.pass");
            File.WriteAllText(CONFIG_FILE, TomletMain.TomlStringFrom(new MainConfig {

                vrchatAddress = "127.0.0.1",
                port = 9001,
                vrchatPort = 9000,

                Mode = ApplicationMode.Testing,

                ESmartConfig = new ESmartCredentials
                {
                    apiUsername = "",
                    apiPassword = "",
                    DevicePassword = "",
                },

                
                BasicConfig = new BasicMode {
                    parameter = "/avatar/parameters/unlock"
                },

                TimerConfig = new TimerMode {
                    maxTime = 60,
                    absMin = 0,
                    StartTime = new DefaultTime {
                        startingValue = -1,
                        randomMin = 10,
                        randomMax = 20
                    },
                    inc_parameter = "/avatar/parameters/timer_inc",
                    inc_step = 1,
                    dec_parameter = "/avatar/parameters/timer_dec",
                    dec_step = 5,

                    readout_mode = 0,
                    readout_parameter = "/avatar/parameters/timer_readout",
                    readout_parameter2 = "",

                    readout_interval = 500
                }
            }));
        }
    }
}