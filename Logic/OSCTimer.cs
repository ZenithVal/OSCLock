using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using FluentColorConsole;
using OSCLock.Configs;
using SharpOSC;
using VRC.OSCQuery;

namespace OSCLock.Logic
{
	public static class OSCTimer
	{
		private static DateTime StartTime;
		private static DateTime EndTime;
		private static DateTime AbsoluteEndTime;
		private static DateTime EarlietEndTime;
		private static TimeSpan TimeRemaining;
		private static Timer _timer;

		private static int maxAccumulatedMinutes;
		private static TimeSpan maxAccumulatedTime;
		private static int absolute_min;
		private static int absolute_max;

		private static string inc_parameter;
		private static int inc_step;
		private static string manual_parameter;

		private static float inc_cooldown;
		private static bool cooldown_state = false;
		private static string cooldown_parameter;

		private static DateTime cooldownEndTime = DateTime.Now;

		private static string capacity_max_parameter;
		private static bool capacity_max_state = false;
		private static string absolute_max_parameter;
		private static bool absolute_max_state = false;

		private static int readout_mode;
		private static string readout_parameter1;
		private static string readout_parameter2;

		private static int starting_time;
		private static int random_min;
		private static int random_max;

		public static void Setup()
		{
			var timerConfig = ConfigManager.ApplicationConfig.TimerConfig;

			try
			{
				maxAccumulatedMinutes = timerConfig.maxTime;
				Console.WriteLine($"\nmax: {maxAccumulatedMinutes}");
				maxAccumulatedTime = TimeSpan.FromMinutes(maxAccumulatedMinutes);

				absolute_min = timerConfig.absMin;
				absolute_max = timerConfig.absMax;
				Console.WriteLine($"absolute_min: {absolute_min}");
				Console.WriteLine($"absolute_max: {absolute_max}\n");

				starting_time = timerConfig.StartTime.startingValue;
				random_min = timerConfig.StartTime.randomMin;
				random_max = timerConfig.StartTime.randomMax;
				Console.WriteLine($"starting_time: {starting_time}");
				Console.WriteLine($"random_min: {random_min}");
				Console.WriteLine($"random_max: {random_max}\n");

				inc_parameter = timerConfig.inc_parameter;
				inc_step = timerConfig.inc_step;

				if (inc_parameter != "")
				{
					//Add oscQuery endpoint
					if (ConfigManager.ApplicationConfig.oscQuery)
					{
						VRChatConnector.ModifyEndPoint(true, inc_parameter, "b", Attributes.AccessValues.WriteOnly, "OSCLock Inc Param");
					}
					VRChatConnector.AddHandler(inc_parameter, OnIncParam);
					Console.WriteLine($"inc_parameter: {inc_parameter}");
					Console.WriteLine($"inc_step: {inc_step}\n");
				}
				else
				{
					Console.WriteLine("inc_parameter not defined.\n");
				}

				manual_parameter = timerConfig.manual_parameter;

				if (manual_parameter != "")
				{
					//Add oscQuery endpoint
					if (ConfigManager.ApplicationConfig.oscQuery)
					{
						VRChatConnector.ModifyEndPoint(true, manual_parameter, "b", Attributes.AccessValues.WriteOnly, "OSCLock Manual Param");
					}
					VRChatConnector.AddHandler(manual_parameter, OnManualParam);
					Console.WriteLine($"manual_parameter: {manual_parameter}");
				}
				else
				{
					Console.WriteLine("manual_parameter not defined.\n");
				}

				//print the addressHandlers dictionary
				foreach (var handler in VRChatConnector.addressHandlers)
				{
					Console.WriteLine($"Address: {handler.Key} | Value: {handler.Value}");
				}

				inc_cooldown = timerConfig.inc_cooldown;
				Console.WriteLine($"inc_cooldoown: {inc_cooldown}");
				Console.WriteLine();

				readout_mode = timerConfig.readout_mode;
				readout_parameter1 = timerConfig.readout_parameter1;
				readout_parameter2 = timerConfig.readout_parameter2;

				cooldown_parameter = timerConfig.cooldown_parameter;
				capacity_max_parameter = timerConfig.capacity_max_parameter;
				absolute_max_parameter = timerConfig.absolute_max_parameter;

				Console.WriteLine($"readout_mode: {readout_mode}");
				Console.WriteLine($"readout_parameter1: {readout_parameter1}");
				Console.WriteLine($"readout_parameter2: {readout_parameter2}");
				Console.WriteLine($"cooldown_parameter: {cooldown_parameter}");
				Console.WriteLine($"capacity_max_parameter: {capacity_max_parameter}");
				Console.WriteLine($"absolute_max_parameter: {absolute_max_parameter}");

				Console.WriteLine();

				if (readout_parameter1 != "" && ConfigManager.ApplicationConfig.oscQuery)
				{
					VRChatConnector.ModifyEndPoint(true, readout_parameter1, "f", Attributes.AccessValues.ReadOnly, "OSCLock Readout Param 1");
				}
				if (readout_parameter2 != "" && ConfigManager.ApplicationConfig.oscQuery)
				{
					VRChatConnector.ModifyEndPoint(true, readout_parameter2, "f", Attributes.AccessValues.ReadOnly, "OSCLock Readout Param 2");
				}

				_timer = new Timer();

				var callbackInterval = timerConfig.readout_interval;
				if (callbackInterval > 0 && !String.IsNullOrEmpty(timerConfig.readout_parameter1))
				{
					_timer.Elapsed += OnProgress;
				}
				else callbackInterval = 1000;

				_timer.Interval = callbackInterval;
				_timer.Elapsed += CheckIfUnlockable;
				_timer.AutoReset = true;

				var doTimerFilesExist = File.Exists("timer.start") && File.Exists("timer.end");

				if (doTimerFilesExist)
				{
					try
					{
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
					catch (Exception e)
					{

						StartTime = DateTime.MinValue;
						EndTime = DateTime.MinValue;
						AbsoluteEndTime = DateTime.MinValue;
						EarlietEndTime = DateTime.MinValue;

						//If the app is encrypted, require the user to start a new timer to enable unlocking again.
						if (!Program.isEncrypted)
						{
							Program.isAllowedToUnlock = true;
							ColorConsole.WithYellowText.WriteLine("Failed to restore timer.\n");
						}
						else
						{
							Program.isAllowedToUnlock = false;
							ColorConsole.WithRedText.WriteLine("Failed to restore timer.\nEncryption prevents timer tampering.");
						}
					}
				}
				else
				{
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
						ColorConsole.WithRedText.WriteLine("No timer files found.\nEncryption prevents timer file tampering.");
					}
				}
			}
			catch (Exception e)
			{
				ColorConsole.WithRedText.WriteLine($"Timer config load failed: {e.Message}");
				Console.WriteLine("\n\nPlease check your config file and reboot.");
				Task.Delay(5000).Wait();
				Environment.Exit(0);
			}

			//Load previous time and check if timer is already running
		}

		public static async Task OnIncParam(OscMessage message)
		{
			var shouldAdd = (bool)message.Arguments[0];
			if (shouldAdd)
			{
				Console.WriteLine($"Inc Param recieved - Attempting to add {inc_step} seconds(s)");

				if (cooldown_state)
				{
					ColorConsole.WithYellowText.WriteLine($"Input is on cooldown");
					CheckCooldown(true);
				}
				else if (absolute_max_state)
				{
					ColorConsole.WithYellowText.WriteLine($"Reached absolute maximum time limit of {absolute_max} minutes");
					CheckAbsMax(true);
				}
				else
				{
					AddTime(inc_step);
					cooldownEndTime = DateTime.Now.AddMilliseconds(inc_cooldown);
				}
			}
		}

		public static async Task OnManualParam(OscMessage message)
		{
			try
			{
				var inputSeconds = (int)message.Arguments[0];
				if (inputSeconds == 0) return;
				Console.WriteLine($"Manual parameter recieved - Adding {inputSeconds} minute(s) to timer.");
				AddTime(inputSeconds);
			}
			catch (Exception e)
			{
				ColorConsole.WithRedText.WriteLine("Failed to parse manual parameter: " + e.Message);
				return;
			}
		}


		public static void AddTime(double timeToAdd)
		{
			var newEndTime = EndTime.AddSeconds(timeToAdd);
			TimeSpan newTimeSpan = newEndTime - DateTime.Now;

			//Device capacity
			if (newTimeSpan > maxAccumulatedTime)
			{
				ColorConsole.WithYellowText.WriteLine($"Reached timer device max of {maxAccumulatedMinutes} minutes.");
				//If the new time span is greater than the max time, we need to remove the difference from the minutes to add
				var timeOverflow = newTimeSpan - maxAccumulatedTime;
				timeToAdd -= timeOverflow.TotalSeconds;
				newEndTime = EndTime.AddSeconds(timeToAdd);
				CheckCapacityMax(true);
			}

			//Min-Max
			if (absolute_max > 0 && timeToAdd > 0)
			{
				if (newEndTime > AbsoluteEndTime)
				{
					newEndTime = AbsoluteEndTime;
					ColorConsole.WithYellowText.WriteLine($"Reached overall maximum time limit of {absolute_max} minutes.");
					absolute_max_state = true;
					CheckAbsMax(true);
				}
				if (newEndTime < EarlietEndTime)
				{
					newEndTime = EarlietEndTime;
					ColorConsole.WithYellowText.WriteLine($"Timer can't go below {absolute_min} minutes.");
				}
			}
			else if (absolute_max_state && timeToAdd < 0)
			{

			}

			//Is timer over?
			if (newEndTime < DateTime.Now)
			{
				newEndTime = DateTime.Now;
			}

			EndTime = newEndTime;
			newTimeSpan = EndTime - DateTime.Now;
			Console.WriteLine($"{newTimeSpan.ToString(@"dd\.hh\:mm\:ss")} remaining");

			try
			{
				if (Program.isEncrypted)
				{
					Encryption.Write("timer.end", EndTime.ToString("O"), Program.appPassword);
				}
				else
				{
					File.WriteAllText("timer.end", EndTime.ToString("O"));
				}
			}
			catch (Exception e)
			{
				ColorConsole.WithRedText.WriteLine("Failed to update endtime file" + e.Message);
			}
		}

		public static Double GetTimeLeftTotal()
		{
			var currentTime = DateTime.Now;

			if (EndTime < currentTime)
			{ //Endtime in past

				if (absolute_min > 0 && EarlietEndTime > currentTime)
				{ //Endtime in past, but minimum time has not passed
					ColorConsole.WithYellowText.WriteLine("Absolute minimum time has not yet elapsed");

					//Going to use ceiling instead of floor to prevent the timer from having to fire minimum warning twice in rare cases.
					var timeDiff = Math.Ceiling((EarlietEndTime - currentTime).TotalSeconds);
					var timeDiffMinutes = Math.Round(timeDiff / 60.0);

					if (timeDiff < 60)
					{
						ColorConsole.WithYellowText.WriteLine($"atleast {timeDiff} more seconds must pass, ");
					}
					else
					{
						ColorConsole.WithYellowText.WriteLine($"atleast {timeDiffMinutes} more minute(s) must pass, ");
					}

					//Makes sure we don't go past the max accumulated time. 
					//EG: If their max timer is 30 minutes but their minimum time is 40, it should only add a total of 30 minutes.
					if (timeDiff > maxAccumulatedMinutes * 60)
					{
						timeDiff = maxAccumulatedMinutes * 60;
					}

					Console.WriteLine($"adding remaining time.");

					AddTime(timeDiff);
					return timeDiff;
				}

				//Time has elapsed
				return 0;
			}

			return (EndTime - currentTime).TotalMinutes;
		}


		public static bool HasTimeElapsed()
		{
			return GetTimeLeftTotal() <= 0;
		}

		public static async Task Start()
		{
			if (!HasTimeElapsed())
			{
				ColorConsole.WithRedText.WriteLine("Cannot start a new timer, you still have " + (int)GetTimeLeftTotal() + " minutes left of current timer.");
				return;
			}

			_timer.Stop();

			ColorConsole.WithRedText.WriteLine("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
			ColorConsole.WithRedText.WriteLine("      You are about to start a new timer");
			ColorConsole.WithRedText.WriteLine(" Unlock will be disabled until the timer reaches 0");
			ColorConsole.WithRedText.WriteLine("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\n");

			if (Program.isEncrypted) Console.WriteLine("Your decrypt key can be used as a failsafe to end the timer.\n");

			//Random disclaimer
			var TimerConfig = ConfigManager.ApplicationConfig.TimerConfig;

			//Need to re-initialize this variable because it gets changed by the randomize values.
			starting_time = TimerConfig.StartTime.startingValue;


			if (starting_time < 0)
			{
				ColorConsole.WithYellowText.WriteLine($"The time is set to random between {random_min} and {random_max} minutes.");
			}

			if (absolute_min > 0)
			{
				ColorConsole.WithYellowText.WriteLine("There is a minimum time of " + absolute_min + " minutes set.");
			}


			Console.Write("  Press 'y', to proceed or any other key to quit");
			var key = Console.ReadKey().Key;
			if (key != ConsoleKey.Y)
			{
				Console.Clear();
				await Program.PrintHelp();
				return;
			}

			Console.Clear();

			Console.Write("New timer started with ");

			if (starting_time < 0)
			{
				var randomTime = new Random().Next(random_min, random_max);
				starting_time = randomTime;
				Console.Write("randomly rolled starting time ");
			}
			else Console.Write("configured starting time ");

			//Minimum check
			if (starting_time < absolute_min && absolute_min > 0)
			{
				starting_time = absolute_min;
				ColorConsole.WithYellowText.Write("capped by minimum time to ");
			}
			//Maximum check
			else if (starting_time > maxAccumulatedMinutes && maxAccumulatedMinutes > 0)
			{
				starting_time = maxAccumulatedMinutes;

				ColorConsole.WithYellowText.Write("capped by maxtime to ");
			}
			//Absolute max check. This should never really happen... but just in case someone really fumbles the config:
			else if (starting_time > absolute_max && absolute_max > 0)
			{
				starting_time = absolute_max;
				ColorConsole.WithYellowText.Write("capped by maxtime to ");
			}
			else Console.Write("of ");

			Console.WriteLine($"{starting_time} minutes.\n");

			StartTime = DateTime.Now;
			EndTime = StartTime.AddMinutes(starting_time);
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

		private static async void CheckIfUnlockable(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (HasTimeElapsed())
			{
				ColorConsole.WithGreenText.WriteLine("\nTime is up, Marking as unlockable and stopping timer");

				_timer.Stop();

				var readout1 = new OscMessage(readout_parameter1, (float)-1.0);
				var readout2 = new OscMessage(readout_parameter2, (float)-1.0);
				var cooldown = new OscMessage(cooldown_parameter, false);
				var capacityMax = new OscMessage(capacity_max_parameter, false);
				var absoluteMax = new OscMessage(absolute_max_parameter, false);
				VRChatConnector.SendToVRChat(readout1);
				VRChatConnector.SendToVRChat(readout2);
				VRChatConnector.SendToVRChat(cooldown);
				VRChatConnector.SendToVRChat(capacityMax);
				VRChatConnector.SendToVRChat(absoluteMax);

				Program.isAllowedToUnlock = true;
			}
		}

		public static async Task ForceEnd()
		{
			Console.Clear();
			ColorConsole.WithGreenText.WriteLine("Ending Timer.");

			_timer.Stop();
			EndTime = DateTime.Now;

			var message = new OscMessage(readout_parameter1, (float)-1.0);
			var message2 = new OscMessage(readout_parameter2, (float)-1.0);
			VRChatConnector.SendToVRChat(message);
			VRChatConnector.SendToVRChat(message2);
			//Makes sure the VRC param is set to the minimum.

			Program.isAllowedToUnlock = true;
			await Program.PrintHelp();
		}

		private static bool lastSentCooldownstate = false;
		private static async void CheckCooldown(bool forceUpdate)
		{
			if (inc_cooldown <= 0 || !cooldown_state && !forceUpdate)
			{
				return;
			}

			if (DateTime.Now < cooldownEndTime)
			{ 
				cooldown_state = true;
			}
			else
			{
				cooldown_state = false;
			}

			if (cooldown_state != lastSentCooldownstate || forceUpdate)
			{
				SendOSCBool(cooldown_parameter, cooldown_state);
				lastSentCooldownstate = cooldown_state;
			}
		}

		private static bool lastSentCapacityState = false;
		private static async void CheckCapacityMax(bool forceUpdate)
		{
			if (!capacity_max_state && !forceUpdate)
			{
				return;
			}

			if (TimeRemaining.Minutes + inc_step*60 > maxAccumulatedMinutes)
			{
				capacity_max_state = true;
			}
			else
			{
				capacity_max_state = false;
			}

			if (capacity_max_state != lastSentCapacityState || forceUpdate)
			{
				SendOSCBool(capacity_max_parameter, capacity_max_state);
				lastSentCapacityState = capacity_max_state;
			}
		}

		private static bool lastSentAbsMaxState = false;
		private static async void CheckAbsMax(bool forceUpdate)
		{
			if (!absolute_max_state && !forceUpdate)
			{
				return;
			}

			if (absolute_max_state != lastSentAbsMaxState || forceUpdate)
			{
				SendOSCBool(absolute_max_parameter, absolute_max_state);
				lastSentCapacityState = capacity_max_state;
			}
		}

		private static async void SendOSCBool(string address, bool value)
		{
			try
			{
				if (string.IsNullOrEmpty(address)) return;
				var message = new OscMessage(capacity_max_parameter, false);
				VRChatConnector.SendToVRChat(message);
			}
			catch (Exception e)
			{
				ColorConsole.WithRedText.WriteLine($"Failed to write OSC bool parameter for {address}: " + e.Message);
			}
		}

		private static async void OnProgress(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			TimeRemaining = EndTime - DateTime.Now;
			var remainingTimeMinutes = TimeRemaining.TotalMinutes;

			CheckCooldown(false);
			CheckCapacityMax(false);

			try
			{
				float readout1 = 0.0f;
				float readout2 = 0.0f;

				switch (readout_mode)
				{
					case 1: //Single Float readout 0 to +1
						readout1 = (float)(remainingTimeMinutes / maxAccumulatedMinutes);
						break;

					case 2: //Single Float readout -1 to +1
						readout1 = (float)((remainingTimeMinutes / maxAccumulatedMinutes * 2) - 1);
						break;

					case 3: //Double Float readout -1 to +1 Float 1 is mintues while Float #2 is seconds
						readout1 = TimeRemaining.Minutes / 60;
						readout2 = TimeRemaining.Seconds / 60;

						//Convert to -1 to +1
						readout1 = (readout1 * 2) - 1;
						readout2 = (readout2 * 2) - 1;
						break;

					case 4: // Double int readout, automatically formatting mm:ss, hh:mm, or dd:hh
						if (remainingTimeMinutes > 1440) //dd:hh 
						{
							readout1 = TimeRemaining.Days;
							readout2 = TimeRemaining.Hours;
						}
						else if (remainingTimeMinutes > 60) //hh:mm
						{
							readout1 = TimeRemaining.Hours;
							readout2 = TimeRemaining.Minutes;
						}
						else // mm:ss
						{
							readout1 = TimeRemaining.Minutes;
							readout2 = TimeRemaining.Seconds;
						}
						break;

					default:
						//If this happens, it's user error.
						break;
				}

				var message1 = new OscMessage(readout_parameter1, readout1);
				VRChatConnector.SendToVRChat(message1);
				if (readout_mode > 2)
				{
					var message2 = new OscMessage(readout_parameter2, readout2);
					VRChatConnector.SendToVRChat(message2);
				}

			}
			catch (Exception e)
			{
				ColorConsole.WithRedText.WriteLine("Failed to write vrchat readout parameter" + e.Message);
			}
		}
	}
}