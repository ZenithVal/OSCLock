using Tomlet.Attributes;

namespace OSCLock.Configs
{
	[TomlDoNotInlineObject]
	public class BasicMode
	{
		[TomlProperty("parameter")]
		public string parameter { get; set; }
	}
}