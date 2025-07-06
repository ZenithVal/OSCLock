using Tomlet.Attributes;

namespace OSCLock.Configs
{
	public class TimerMode
	{
		[TomlProperty("max")]
		[TomlInlineComment("Max minutes at a given moment. How much sand can the hourglass hold at a time?")]
		public int maxTime { get; set; }

		[TomlProperty("absolute_min")]
		[TomlInlineComment("(Minutes). Time will be added if it total time is below this. 0 disables.")]
		public int absMin { get; set; }

		[TomlProperty("absolute_max")]
		[TomlInlineComment("(Minutes) If overall time reaches this, inc_step wont work. 0 disables.")]
		public int absMax { get; set; }

		//Time Section
		[TomlProperty("startingTime")]
		public DefaultTime StartTime { get; set; }


		[TomlPrecedingComment("--- Incoming OSC Parameters ---")]
		[TomlInlineComment("When this Bool is true, it should increase the timer once by inc_step.")]
		public string inc_parameter { get; set; }

		[TomlInlineComment("Seconds added per inc_step")]
		public int inc_step { get; set; }

		[TomlInlineComment("Cooldown (miliseconds) between allowed inputs, 0 to disable.")]
		public int inc_cooldown { get; set; }

		[TomlInlineComment("If an int is passed to this address, add/subtract the minute ammount from timer. Ignores cooldown.")]
		public string manual_parameter { get; set; }


		[TomlPrecedingComment("-- Outgoing OSC Parameters ---")]
		public int readout_mode { get; set; }
		public string readout_parameter1 { get; set; }
		public string readout_parameter2 { get; set; }
		public string cooldown_parameter { get; set; }
		public string capacity_max_parameter { get; set; }
		public string absolute_max_parameter { get; set; }


		[TomlInlineComment("Time (miliseconds) between outgoing messages")]
		public int readout_interval { get; set; }

	}
}