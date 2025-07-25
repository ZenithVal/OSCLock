﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentColorConsole;
using OSCLock.Bluetooth;
using OSCLock.Configs;
using OSCLock.Logic;

namespace OSCLock
{
	internal class Program
	{
		public static bool isAllowedToUnlock = false;
		public static bool isEncrypted = false;
		public static string appPassword = "";

		public static async Task Main(string[] args)
		{

			Console.WriteLine("OSCLock Version v1.0\n");

			if (File.Exists("app.pass"))
			{
				isEncrypted = true;
				//VERY Secure app password, I know. It's not meant to be real, this is just fun security theater. 
				appPassword = Encryption.Read("app.pass", "7b7079bb6379001dce");
				Console.WriteLine("Application Config is encrypted.\n");
				//Console.WriteLine(appPassword);
			}

			VRChatConnector.Start();
			//Console.WriteLine("Going to home screen...");
			//Thread.Sleep(500);

			//Delay for reading the config readout if in debug mode.
			if (VRChatConnector.debugging) Thread.Sleep(1500);

			await CmdPrompt();
		}


		public static async Task PrintHelp()
		{
			Console.WriteLine("■■■■■■■ HELP SCREEN ■■■■■■■");


			Console.WriteLine("  H -- Prints this menu");
			Console.WriteLine("  T -- Starts a new timer");
			Console.WriteLine("  S -- Status of app & lock");
			Console.WriteLine("  U -- Unlock device");
			Console.WriteLine("  Q -- Quits the app");
			if (isEncrypted) Console.WriteLine("  } -- Decrypts files");

			//Decided to not inform the user about accessing encryption, since it's documented in the readme.
			//if (!isEncrypted)  Console.WriteLine("{ -- Encrypts Config & Timer files\n");
			//else Console.WriteLine("} -- Decrypts Config & Timer filesu\n");

			//Bumper
			Console.WriteLine("");
		}

		private static async Task PrintStatus()
		{
			var appConfig = ConfigManager.ApplicationConfig;
			Console.WriteLine($"Operating in {appConfig.mode} mode.");

			if (appConfig.mode == ApplicationMode.Timer)
			{
				Console.WriteLine("Time left: " + (int)OSCTimer.GetTimeLeftTotal() + " minutes \n");
			}

			await PrintHelp();

			//Console.WriteLine("Allowed to unlock: " + isAllowedToUnlock + "\n");
		}

		private static ESmartLock connectedLock;
		public static async Task UnlockDevice()
		{
			if (!isAllowedToUnlock)
			{
				ColorConsole.WithYellowText.WriteLine("You are not allowed to unlock yet!\n");

				if (isEncrypted && OSCTimer.HasTimeElapsed())
				{
					ColorConsole.WithYellowText.WriteLine("Encryption requires a complete timer to allow unlocking.");
					Thread.Sleep(1500);
					Console.Clear();
					await PrintHelp();
				}

				else await PrintStatus();
				return;
			}

			if (connectedLock == null)
			{
				Console.WriteLine("You are not connected to a lock yet, starting bluetooth scanner\n");
				connectedLock = await BleScanner.FindESmartLock();
			}

			Console.WriteLine("Found device");

			//todo: Attempt unlock here
			await connectedLock.Unlock();

			Console.WriteLine("Removing lock as it turned of by now\n");
			connectedLock = null;

			Thread.Sleep(1500);
			Console.Clear();
			await PrintHelp();
		}

		private static async Task StartTimer()
		{
			if (ConfigManager.ApplicationConfig.mode == ApplicationMode.Timer)
			{
				await OSCTimer.Start();
			}
			else ColorConsole.WithYellowText.WriteLine("Not operating in timer mode, change mode in config and restart");
		}

		private static async Task EncryptApp()
		{

			//if OSCTimer.HasTimeElapsed is false, don't allow encryption
			if (!OSCTimer.HasTimeElapsed())
			{
				ColorConsole.WithYellowText.WriteLine("A timer is already running, you need to finish that before attempting encryption.\n");
				await PrintHelp();
				return;
			}

			if (isEncrypted)
			{
				ColorConsole.WithYellowText.WriteLine("Application is already encrypted!\n");
				await PrintHelp();
				return;
			}

			Encryption.EncryptApp();

			Console.Clear();
			await PrintHelp();
		}

		private static async Task DecryptApp()
		{
			if (!isEncrypted)
			{
				ColorConsole.WithYellowText.WriteLine("Application is already decrypted!\n");
				await PrintHelp();
				return;
			}

			Encryption.DecryptApp();

			Console.Clear();
			await PrintHelp();
		}

		private static async Task CmdPrompt()
		{
			char Key = 'h';

			do
			{
				Console.Clear();
				//Switch statements for the possible UI options

				switch (Key)
				{
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
					case '{':
						await EncryptApp();
						break;
					case '}':
						await DecryptApp();
						break;
					default:
						ColorConsole.WithRedText.WriteLine($"Unknown command {Key}");
						await PrintHelp();
						break;
				}

				Console.Write(">");
			} while ((Key = Console.ReadKey().KeyChar.ToString().ToLower().ToCharArray().First()) != 'q');
		}

		private static void Exit()
		{
			VRChatConnector.oscQueryDispose();
		}
	}
}