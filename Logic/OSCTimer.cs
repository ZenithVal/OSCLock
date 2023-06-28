using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using OSCLock.Configs;
using SharpOSC;
using Windows.Foundation.Metadata;

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

        private static string inc_parameter;
        private static int inc_step;
        private static string dec_parameter;
        private static int dec_step;

        private static int readout_mode;
        private static string readout_param;
        private static string readout_param2;
        
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

            VRChatConnector.AddHandler(timerConfig.inc_parameter, OnIncParam);

            //Bumper
            Console.WriteLine("");

            maxAccumulated = timerConfig.maxTime;
            Console.WriteLine($"max: {maxAccumulated}");

            absolute_min = timerConfig.absMin;
            absolute_max = timerConfig.absMax;
            Console.WriteLine($"absolute_min: {absolute_min}");
            Console.WriteLine($"absolute_min: {absolute_max}\n");

            inc_parameter = timerConfig.inc_parameter;
            inc_step = timerConfig.inc_step;

            //If inc_parameter is NOT null, then add a handler and print the added parameter.
            if (inc_parameter != "") {
                VRChatConnector.AddHandler(inc_parameter, OnDecParam);
                Console.WriteLine($"inc_parameter: {inc_parameter}");
                Console.WriteLine($"inc_step: {inc_step}");
            }
            else {
                Console.WriteLine("inc_parameter not defined.");
            }

            dec_parameter = timerConfig.dec_parameter;
            dec_step = -timerConfig.dec_step;

            //If dec_parameter is NOT null, then add a handler and print the added parameter.
            if (dec_parameter != "") {
                VRChatConnector.AddHandler(dec_parameter, OnDecParam);
                Console.WriteLine($"dec_parameter: {dec_parameter}");
                Console.WriteLine($"dec_step: {dec_step}");
            }
            else {
                Console.WriteLine("dec_parameter not defined.");
            }

            readout_mode = timerConfig.readout_mode;
            Console.WriteLine($"\nreadout_mode: {readout_mode}\n");


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
                    
                    StartTime = DateTime.MinValue;
                    EndTime = DateTime.MinValue;
                    AbsoluteEndTime = DateTime.MinValue;
                    EarlietEndTime = DateTime.MinValue;

                    //If the app is encrypted, require the user to start a new timer to enable unlocking again.
                    if (!Program.isEncrypted) 
                    {
                        Program.isAllowedToUnlock = true;
                        Console.WriteLine("Failed to restore timer.\n");
                    }
                    else
                    {
                        Program.isAllowedToUnlock = false;
                        Console.WriteLine("Failed to restore timer.\nEncryption prevents timer file tampering.");
                    }
                }
            }
            else {
                StartTime = DateTime.MinValue;
                EndTime = DateTime.MinValue;
                AbsoluteEndTime = DateTime.MinValue;
                EarlietEndTime = DateTime.MinValue;

                if (!Program.isEncrypted)
                {
                    Program.isAllowedToUnlock = true;
                    Console.WriteLine("No timer files found.\n");
                }
                else
                {
                    Program.isAllowedToUnlock = false;
                    Console.WriteLine("No timer files found.\nEncryption prevents timer file tampering.");
                }
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
            var startTimeConfig = TimerConfig.StartTime;
            int startingTime = startTimeConfig.startingValue;
            if (startTimeConfig.startingValue < 0) {
                Console.WriteLine($"The time is set to random between {startTimeConfig.randomMin} and {startTimeConfig.randomMax} minutes.");
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
                var randomTime = new Random().Next(startTimeConfig.randomMin, startTimeConfig.randomMax);
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
                Console.WriteLine("\nTime is up, Marking as unlockable and stopping timer");

                _timer.Stop();

                var message = new OscMessage(readout_param, -1.0);
                var message2 = new OscMessage(readout_param2, -1.0);
                VRChatConnector.SendToVRChat(message);
                VRChatConnector.SendToVRChat(message2);
                //Makes sure the VRC param is set to lowest possible value. 
                //Negative number is fine, it'll be clamped if they're using bools or ints.

                Program.isAllowedToUnlock = true;
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

            var remainingTime = ((EndTime - DateTime.Now).TotalMinutes);


            try
            {
                switch (readout_mode)
                {
                    case 1: //Single Float readout 0 to +1
                        var Readout1 = (float)(remainingTime / maxAccumulated);
                        var message1 = new OscMessage(readout_param, Readout1);
                        VRChatConnector.SendToVRChat(message1);
                        break;

                    case 2: //Single Float readout -1 to +1
                        var Readout2 = (float)((remainingTime / maxAccumulated * 2) - 1);
                        var message2 = new OscMessage(readout_param, Readout2);
                        VRChatConnector.SendToVRChat(message2);
                        break;

                    case 3: //Double Float readout -1 to +1 Float 1 is mintues while Float 2 is seconds
                        var Readout3Minutes = (float)((remainingTime / maxAccumulated * 2) - 1);
                        var Readout3Seconds = (float)(((remainingTime - Math.Floor(remainingTime))) * 2) - 1;

                        var message3Minutes = new OscMessage(readout_param, Readout3Minutes);
                        var message3Seconds = new OscMessage(readout_param2, Readout3Seconds);

                        VRChatConnector.SendToVRChat(message3Minutes);
                        VRChatConnector.SendToVRChat(message3Seconds);
                        break;

                    case 4: //Float -1 to +1 for minutes, Rounded down int for seconds
                        var Readout4Minutes = (float)((remainingTime / maxAccumulated * 2) - 1);
                        var Readout4Seconds = (float)Math.Floor((remainingTime - Math.Floor(remainingTime)) * 60);

                        var message4Minutes = new OscMessage(readout_param, Readout4Minutes);
                        var message4Seconds = new OscMessage(readout_param2, Readout4Seconds);

                        VRChatConnector.SendToVRChat(message4Minutes);
                        VRChatConnector.SendToVRChat(message4Seconds);
                        break;

                    case 5: //Double int readout, straight translation of minutes and seconds (rounded down)
                        var Readout5Minutes = (float)Math.Floor(remainingTime);
                        var Readout5Seconds = (float)Math.Floor((remainingTime - Readout5Minutes) * 60);

                        var message5Minutes = new OscMessage(readout_param, Readout5Minutes);
                        var message5Seconds = new OscMessage(readout_param2, Readout5Seconds);

                        VRChatConnector.SendToVRChat(message5Minutes);
                        VRChatConnector.SendToVRChat(message5Seconds);
                        break;

                    case 6: //Signle int rounded down and a boolean to tell if it's sending minutes or seconds.
                        var Readout6Minutes = (float)Math.Floor(remainingTime);
                        var Readout6Seconds = (float)Math.Floor((remainingTime - Readout6Minutes) * 60);

                        var message6Minutes = new OscMessage(readout_param, Readout6Minutes);
                        var message6BoolFalse = new OscMessage(readout_param2, false);

                        VRChatConnector.SendToVRChat(message6BoolFalse);
                        VRChatConnector.SendToVRChat(message6Minutes);


                        var message6Seconds = new OscMessage(readout_param, Readout6Seconds);
                        var message6BoolTrue = new OscMessage(readout_param2, true);

                        VRChatConnector.SendToVRChat(message6BoolTrue);
                        VRChatConnector.SendToVRChat(message6Seconds);
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