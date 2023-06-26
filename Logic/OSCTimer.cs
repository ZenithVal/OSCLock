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
        private static DateTime AbsoluteEndTime;
        private static DateTime EarlietEndTime;
        private static Timer _timer;

        private static int maxAccumulated;

        private static int absolute_min;
        private static int absolute_max;

        private static int inc_step;
        private static int dec_step;

        private static int readout_mode;
        private static string readout_param;
        private static string readout_param2;
        
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
                Console.WriteLine($"Param recieved - Attempting to remove {dec_step} minute(s)");
                AddTime(dec_step);
        }

        public static void Setup() {
            var timerConfig = ConfigManager.ApplicationConfig.TimerConfig;
            VRChatConnector.AddHandler(timerConfig.dec_parameter, OnDecParam);
            VRChatConnector.AddHandler(timerConfig.inc_parameter, OnIncParam);

            maxAccumulated = timerConfig.maxTime;

            absolute_min = timerConfig.absMin;
            absolute_max = timerConfig.absMax;

            inc_step = timerConfig.inc_step;
            dec_step = -timerConfig.dec_step;

            readout_mode = timerConfig.readout_mode;

        _timer = new Timer();

            var callbackInterval = timerConfig.readout_interval;
            if (callbackInterval > 0 && !String.IsNullOrEmpty(timerConfig.readout_parameter)) {
                readout_param = timerConfig.readout_parameter;
                readout_param2 = timerConfig.readout_parameter2;
                _timer.Elapsed += OnProgress;
            }
            else callbackInterval = 1000;

            _timer.Interval = callbackInterval;
            _timer.Elapsed += CheckIfUnlockable;
            _timer.AutoReset = true;

            var doesFilesExist = File.Exists("timer.start") && File.Exists("timer.end");

            if (doesFilesExist) {
                try {
                    if (Program.isEncrypted)
                    {
                        StartTime = DateTime.ParseExact(Encryption.Read("timer.start", Program.appPassword), "O", CultureInfo.InvariantCulture);
                        EndTime = DateTime.ParseExact(Encryption.Read("timer.end", Program.appPassword), "O", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        StartTime = DateTime.ParseExact(File.ReadAllText("timer.start"), "O", CultureInfo.InvariantCulture);
                        EndTime = DateTime.ParseExact(File.ReadAllText("timer.end"), "O", CultureInfo.InvariantCulture);
                    }
                    
                    AbsoluteEndTime = StartTime.AddMinutes(absolute_max);
                    EarlietEndTime = StartTime.AddMinutes(absolute_min);
                    Program.isAllowedToUnlock = false;
                    _timer.Start();
                }
                catch (Exception e) {
                    Console.WriteLine("Failed to restore timer, start a new one at wish, lock has been made unlockable");
                    StartTime = DateTime.MinValue;
                    EndTime = DateTime.MinValue;
                    AbsoluteEndTime = DateTime.MinValue;
                    EarlietEndTime = DateTime.MinValue;
                    Program.isAllowedToUnlock = true;

                    Console.WriteLine("Failed to restore timer, start a new one.");
                }
            }
            else {
                StartTime = DateTime.MinValue;
                EndTime = DateTime.MinValue;
                AbsoluteEndTime = DateTime.MinValue;
                EarlietEndTime = DateTime.MinValue;
                Program.isAllowedToUnlock = false;
            }

            //Load previous time and check if timer is already running
        }

        public static void AddTime(double minutesToAdd) {
            var maxTime = ConfigManager.ApplicationConfig.TimerConfig.maxTime;

            var newTimeSpan = (int)(EndTime.AddMinutes(minutesToAdd) - DateTime.Now).TotalMinutes;

            if (newTimeSpan > maxTime) {
                Console.WriteLine("Reached timer device cap");
                //If the new time span is greater than the max time, we need to remove the difference from the minutes to add
                minutesToAdd -= (newTimeSpan - maxTime);
             }
            
            var newEndTime = EndTime.AddMinutes(minutesToAdd);

            if (absolute_max > 0 && minutesToAdd > 0) {
                //Checking if going past absolute max
                if (newEndTime > AbsoluteEndTime) {
                    newEndTime = AbsoluteEndTime;
                    Console.WriteLine("Reached overall maximum time limit");
                }
            }
            
            if (newEndTime < DateTime.Now)
                newEndTime = DateTime.Now;

            EndTime = newEndTime;
            try {
                if (Program.isEncrypted) {
                    Encryption.Write("timer.end", EndTime.ToString("O"), Program.appPassword);
                }
                else {
                    File.WriteAllText("timer.end", EndTime.ToString("O"));
                }
                
            }
            catch (Exception e) {
                Console.WriteLine("Failed to update endtime file" + e.Message);
            }
        }

        public static Double GetTimeLeftTotal() {
            var currentTime = DateTime.Now;

            if (EndTime < currentTime) { //Endtime in past

                    if (absolute_min > 0 && EarlietEndTime > currentTime)
                    { //Endtime in past, but minimum time has not passed
                        Console.WriteLine("Absolute minimum time has not yet elapsed, ");
                        var timeDiff = Math.Round((EarlietEndTime - currentTime).TotalMinutes);
                        Console.WriteLine($"atleast {timeDiff} more minutes must pass, ");
                        if (timeDiff > maxAccumulated)
                        {
                            timeDiff = maxAccumulated;
                        }

                        Console.WriteLine($"adding {timeDiff} minute(s)");

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


            Console.WriteLine("You are about to start a new timer.");
            Console.WriteLine("Unlock will be disabled until the timer reaches 0.");
            Console.WriteLine("If encrypted, the key can be used as a failsafe.\n");
            //Random disclaimer
            var TimerConfig = ConfigManager.ApplicationConfig.TimerConfig;
            var startingTime = TimerConfig.startingValue;
            if (startingTime <= 0) {
                Console.WriteLine($"The time is set to random between {TimerConfig.randomMin} and {TimerConfig.randomMax} minutes.");
            }

            var minimumTime = ConfigManager.ApplicationConfig.TimerConfig.absMin;
            if (minimumTime > 0) {
                Console.WriteLine("There is a minimum time of " + minimumTime + " minutes set.");
            }
            
            Console.Write("Press 'y', to proceed or any other key to quit");
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.Y) {
                Console.Clear();
                await Program.PrintHelp();
                return;
            }

            Console.Clear();
            
            Console.Write("New timer started with ");

            if (startingTime < 0) {
                var randomTime = new Random().Next(TimerConfig.randomMin, TimerConfig.randomMax);
                startingTime = randomTime;
                Console.Write("randomly rolled starting time ");
            }
            else Console.Write("configured starting time ");

            if (startingTime > ConfigManager.ApplicationConfig.TimerConfig.maxTime && ConfigManager.ApplicationConfig.TimerConfig.maxTime > 0) {
                startingTime = ConfigManager.ApplicationConfig.TimerConfig.maxTime;
                Console.Write("capped by maxtime to ");
            }
            else Console.Write("of ");
            
            Console.WriteLine($"{startingTime} minutes.\n");

            StartTime = DateTime.Now;
            EndTime = StartTime.AddMinutes(startingTime);
            AbsoluteEndTime = StartTime.AddMinutes(absolute_max);
            EarlietEndTime = StartTime.AddMinutes(absolute_min);

            if (Program.isEncrypted)
            {
                Encryption.Write("timer.start", StartTime.ToString("O"), Program.appPassword);
                Encryption.Write("timer.end", EndTime.ToString("O"), Program.appPassword);
            }
            else
            {
                File.WriteAllText("timer.start", StartTime.ToString("O"));
                File.WriteAllText("timer.end", EndTime.ToString("O"));
            }

            Program.isAllowedToUnlock = false;


            _timer.Start();

            await Program.PrintHelp();
        }

        private static async void CheckIfUnlockable(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (HasTimeElapsed()) {
                Console.Clear();
                Console.WriteLine("Time is up, Marking as unlockable and stopping timer");

                _timer.Stop();

                var message = new OscMessage(readout_param, 0.0);
                var message2 = new OscMessage(readout_param2, 0.0);
                VRChatConnector.SendToVRChat(message);
                VRChatConnector.SendToVRChat(message2);
                //Makes sure the VRC param is set to 0.

                Program.isAllowedToUnlock = true;
                await Program.PrintHelp();
            }
        }

        public static async Task ForceEnd()
        {
                Console.Clear();
                Console.WriteLine("Ending Timer.");

                _timer.Stop();
                EndTime = DateTime.Now;

                var message = new OscMessage(readout_param, 0.0);
                var message2 = new OscMessage(readout_param2, 0.0);
                VRChatConnector.SendToVRChat(message);
                VRChatConnector.SendToVRChat(message2);
                //Makes sure the VRC param is set to 0.

            Program.isAllowedToUnlock = true;
                await Program.PrintHelp();
        }

        private static async void OnProgress(object sender, ElapsedEventArgs elapsedEventArgs) {
            //Readout mode 0: No functionality! We can skip sending data out to VRChat.
            //Readout mode 1: We'll output to readout_parameter the total time left as a float between 0 and 1.
            //Readout mode 2: We'll output to readout_parameter the total time left as a float between -1 and +1.
            //Readout mode 2: We'll output two ints to VRC. The first will be the minutes left, the second will be the seconds left.

            var remainingTime = ((EndTime - DateTime.Now).TotalMinutes);


            try
            {
                switch (readout_mode)
                {
                    case 1:
                        var Readout1 = (float)(remainingTime / maxAccumulated);
                        var message1 = new OscMessage(readout_param, Readout1);
                        VRChatConnector.SendToVRChat(message1);
                        break;
                    case 2:
                        var Readout2 = (float)((remainingTime / maxAccumulated * 2) - 1);
                        var message2 = new OscMessage(readout_param, Readout2);
                        VRChatConnector.SendToVRChat(message2);
                        break;
                    case 3:
                        var Readout3Minutes = (float)Math.Floor(remainingTime);
                        var Readout3Seconds = (float)Math.Floor(((remainingTime) - Readout3Minutes) * 60);

                        var message3Minutes = new OscMessage(readout_param, Readout3Minutes);
                        var message3Seconds = new OscMessage(readout_param2, Readout3Seconds);

                        VRChatConnector.SendToVRChat(message3Minutes);
                        VRChatConnector.SendToVRChat(message3Seconds);
                        break;
                    default:
                        //Invalid or no readout mode. Whatever!
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to write vrchat readout parameter" + e.Message);
            }
        }
    }
}