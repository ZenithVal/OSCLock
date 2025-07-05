using Tomlet.Attributes;

namespace OSCLock.Configs
{
	public class MainConfig
	{
		[TomlProperty("OSCQuery")]
		[TomlInlineComment("Enable OSCQuery; Custom Port/IP settings will be ignored.")]
		public bool oscQuery { get; set; }

		[TomlProperty("ip")]
		public string ipAddress { get; set; }

		[TomlProperty("listener_port")]
		public int listener_port { get; set; }

		[TomlProperty("write_port")]
		public int write_port { get; set; }

		[TomlProperty("Mode")]
		public ApplicationMode mode { get; set; }

		[TomlProperty("Debugging")]
		public bool debugging { get; set; }

		[TomlProperty("Credentials for ESmartLock")]
		public ESmartCredentials ESmartConfig { get; set; }

		[TomlProperty("Basic")]
		public BasicMode BasicConfig { get; set; }

		[TomlProperty("Timer")]
		public TimerMode TimerConfig { get; set; }
	}
}