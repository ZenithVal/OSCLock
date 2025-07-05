using System;
using System.Threading.Tasks;
using OSCLock.Configs;
using SharpOSC;

namespace OSCLock.Logic
{
	public static class OSCBasic
	{
		public static async Task onUnlockParameter(OscMessage message)
		{
			try
			{
				bool writtenBool = (bool)message.Arguments[0];
				Program.isAllowedToUnlock = writtenBool;
				Console.WriteLine($"Unlock Parameter Recieved: {writtenBool}");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error reading parameter: {e}");
			}
		}

		public static void Setup()
		{
			var config = ConfigManager.ApplicationConfig.BasicConfig;
			VRChatConnector.AddHandler(config.parameter, onUnlockParameter);
		}
	}
}