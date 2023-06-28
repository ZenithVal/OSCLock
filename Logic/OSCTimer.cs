using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using OSCLock.Configs;
using SharpOSC;

namespace OSCLock.Logic {
    public static class OSCTimer {
        private static DateTime StartTime;
        private static DateTime EndTime;
        private static Timer _timer;
        private static string readout_param;
        private static int maxAccumulated;
        private static int inc_step;
        private static int dec_step;
        private static int absolute_max;
        private static DateTime lastAdded = DateTime.Now;
        public static async Task OnIncParam(OscMessage message) {
            var shouldAdd = (bool) message.Arguments[0];
            if (shouldAdd)
                shouldAdd = DateTime.Now >= lastAdded;
            else Console.WriteLine($"Time adding cooldown: " + lastAdded);
            if (shouldAdd) {
                AddTime(inc_step);
                lastAdded = DateTime.Now.AddSeconds(1.5);
            }
        }

        public static async Task OnDecParam(OscMessage message) {
            var shouldDec = (bool) message.Arguments[0];
            if (shouldDec)
                AddTime(dec_step);
        }

        public static void Setup() {
            var timerConfig = ConfigManager.ApplicationConfig.TimerConfig;
            VRChatConnector.AddHandler(timerConfig.dec_parameter, OnDecParam);
            VRChatConnector.AddHandler(timerConfig.inc_parameter, OnIncParam);

            absolute_max = timerConfig.absMax;
            maxAccumulated = timerConfig.maxTime;
            dec_step = -timerConfig.dec_step;
            inc_step = timerConfig.inc_step;
            _timer = new Timer();

            var callbackInterval = timerConfig.readout_interval;
            if (callbackInterval > 0 && !String.IsNullOrEmpty(timerConfig.readout_parameter)) {
                readout_param = timerConfig.readout_parameter;
                _timer.Elapsed += OnProgress;
            }
            else callbackInterval = 1000;

            _timer.Interval = callbackInterval;
            _timer.Elapsed += CheckIfUnlockable;
            _timer.AutoReset = true;

            var doesFilesExist = File.Exists("timer.start") && File.Exists("timer.end");

            if (doesFilesExist) {
                try {
                    StartTime = DateTime.ParseExact(File.ReadAllText("timer.start"), "O", CultureInfo.InvariantCulture);
                    EndTime = DateTime.ParseExact(File.ReadAllText("timer.end"), "O", CultureInfo.InvariantCulture);
                    Program.isAllowedToUnlock = false;
                    _timer.Start();
                }
                catch (Exception e) {
                    Console.WriteLine("Failed to restore timer, start a new one at wish, lock has been made unlockable");
                    StartTime = DateTime.MinValue;
                    EndTime = DateTime.MinValue;
                    Program.isAllowedToUnlock = true;
                }
            }
            else {
                StartTime = DateTime.MinValue;
                EndTime = DateTime.MinValue;
                Program.isAllowedToUnlock = true;
            }

            //Load previous time and check if timer is already running
        }

        public static void AddTime(double minutesToAdd) {
            var maxTime = ConfigManager.ApplicationConfig.TimerConfig.maxTime;
            if (maxTime > 0 && minutesToAdd > 0) {
                var newTimeSpan = (int) (EndTime.AddSeconds(minutesToAdd) - DateTime.Now).TotalMinutes;
                if (newTimeSpan > maxTime) {
                    Console.WriteLine("Reached max allowed accomulated time");
                    minutesToAdd -= (newTimeSpan - maxTime);
                }
            }
            
            var newEndTime = EndTime.AddSeconds(minutesToAdd);
            if (absolute_max > 0) {
                //Checking if going past absolute max
                if (newEndTime > StartTime.AddMinutes(absolute_max))
                    newEndTime = StartTime.AddMinutes(absolute_max);
            }
            
            
            if (newEndTime < DateTime.Now)
                newEndTime = DateTime.Now;

            
            
            EndTime = newEndTime;
            try {
                File.WriteAllText("timer.end", EndTime.ToString("O"));
            }
            catch (Exception e) {
                Console.WriteLine("Failed to update endtime file" + e.Message);
            }
        }

        public static Double GetTimeLeftTotal() {
            var currentTime = DateTime.Now;
            if (EndTime < currentTime) { //Endtime in past
                var earliestPossible = StartTime.AddMinutes(ConfigManager.ApplicationConfig.TimerConfig.absMin);

                if (earliestPossible > currentTime) { //Endtime in past, but minimum time has not passed
                    Console.Write("Absolute minimum time has not yet elapsed, ");
                    var timeDiff = (earliestPossible - currentTime).TotalMinutes;
                    Console.Write($"atleast {timeDiff} minutes must pass, adding ");
                    if (timeDiff > 5) {
                        timeDiff = 5;
                    }

                    Console.WriteLine($"{timeDiff} minutes for now");


                    AddTime(timeDiff);
                    return timeDiff;
                }

                //Time has elapsed
                return 0;
            }

            return (EndTime - currentTime).TotalMinutes;
        }


        public static bool HasTimeElapsed() {
            return GetTimeLeftTotal() <= 0;
        }


        public static async Task Start() {
            if (!HasTimeElapsed()) {
                Console.WriteLine("Cannot start a new timer, you still have " + (int) GetTimeLeftTotal() + " minutes left of current timer.");
                return;
            }

            _timer.Stop();


            Console.WriteLine("YOU ARE ABOUT TO START A NEW TIMER ARE YOU SURE?");
            Console.WriteLine("You wont be able to unlock the lock if you proceed till the timer has reached 0");
            //Random disclaimer
            var defaultTimeConfig = ConfigManager.ApplicationConfig.TimerConfig.defaultTime;
            var startingTime = defaultTimeConfig.startingValue;
            if (startingTime <= 0) {
                Console.WriteLine($"You have configured for a random starting time between {defaultTimeConfig.randomMin} to {defaultTimeConfig.randomMax} minutes!");
            }

            var minimumTime = ConfigManager.ApplicationConfig.TimerConfig.absMin;
            if (minimumTime > 0) {
                Console.WriteLine("Note that you have configured an absolute minimum time of " + minimumTime + " minutes, meaning that time will be added as you approach 0 if you have not spent a total of " + minimumTime + " minutes yet.");
            }
            
            Console.Write("If you want to proceed press the key 'y', to quit press any other key");
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.Y) {
                Console.Clear();
                await Program.PrintHelp();
                return;
            }

            Console.Clear();
            
            Console.Write("New timer started with ");



            if (startingTime < 0) {
                var randomTime = new Random().Next(defaultTimeConfig.randomMin, defaultTimeConfig.randomMax);
                startingTime = randomTime;
                Console.Write("randomly rolled starting time ");
            }
            else Console.Write("configured starting time ");

            if (startingTime > ConfigManager.ApplicationConfig.TimerConfig.maxTime && ConfigManager.ApplicationConfig.TimerConfig.maxTime > 0) {
                startingTime = ConfigManager.ApplicationConfig.TimerConfig.maxTime;
                Console.Write("capped by maxtime to ");
            }
            else Console.Write("of ");
            
            Console.WriteLine($"{startingTime} minutes.... for now :)");

            StartTime = DateTime.Now;
            File.WriteAllText("timer.start", StartTime.ToString("O"));
            EndTime = StartTime.AddMinutes(startingTime);
            File.WriteAllText("timer.end", EndTime.ToString("O"));

            Program.isAllowedToUnlock = false;


            _timer.Start();
        }

        private static async void CheckIfUnlockable(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (HasTimeElapsed()) {
                Console.WriteLine("Time is up!!!, Marking as unlockable and stopping timer");
                _timer.Stop();
                Program.isAllowedToUnlock = true;
            }
        }

        private static async void OnProgress(object sender, ElapsedEventArgs elapsedEventArgs) {
            try {
                int date = (int) ((EndTime - DateTime.Now)).TotalSeconds;
                int d4 = date % 10;
                int d3 = (date % 60 - d4) / 10;
                int mins = date / 60;
                int d2 = mins % 10;
                int d1 = (mins % 100 - d2) / 10;

                Console.WriteLine($"{d1}{d2}:{d3}{d4}");
                
                var d1Msg = new OscMessage($"{readout_param}1", d1/10.0f);
                var d2Msg = new OscMessage($"{readout_param}2", d2/10.0f);
                var d3Msg = new OscMessage($"{readout_param}3", d3/10.0f);
                var d4Msg = new OscMessage($"{readout_param}4", d4/10.0f);
                
                
                VRChatConnector.SendToVRChat(d1Msg);
                VRChatConnector.SendToVRChat(d2Msg);
                VRChatConnector.SendToVRChat(d3Msg);
                VRChatConnector.SendToVRChat(d4Msg);
                /*
                Console.WriteLine($"{d1}{d2}:{d3}{d4}");
                var floatValue = (float) ((EndTime - DateTime.Now).TotalMinutes) / maxAccumulated;
                var message = new OscMessage(readout_param, floatValue);
                VRChatConnector.SendToVRChat(message);
                */
            }
            catch (Exception e) {
                Console.WriteLine("Failed to write vrchat readout parameter" + e.Message);
            }
        }
    }
}