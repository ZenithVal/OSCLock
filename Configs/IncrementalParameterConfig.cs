using Tomlet.Attributes;

namespace OSCLock.Configs
{
	public class IncrementalParameterConfig
	{
		[TomlProperty("parameter")]
		public string parameterName { get; set; }

		[TomlProperty("step")]
		public int stepping { get; set; }
	}
}