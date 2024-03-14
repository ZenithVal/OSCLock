using System;
using Tomlet.Attributes;

namespace OSCLock.Configs {
    public class TimerMode {
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

        //Should make this a float and allow seconds to be added.
        [TomlInlineComment("Seconds added per dec_step")]
        public int inc_step { get; set; }

        [TomlInlineComment("When this bool is true, decrease time once by dec_step")]
        public string dec_parameter { get; set; }

        //Should make this a float and allow seconds to be added.
        [TomlInlineComment("Seconds removed per dec_step")]
        public int dec_step { get; set; }

        [TomlInlineComment("Cooldown (miliseconds) between allowed inputs, 0 to disable.")]
        public int input_cooldown { get; set; }


        [TomlPrecedingComment("-- Outgoing OSC Parameters ---")]
        public int readout_mode { get; set; }
        public string readout_parameter { get; set; }
        public string readout_parameter2 { get; set; }


        [TomlInlineComment("Time (miliseconds) between outgoing messages")]
        public int readout_interval { get; set; }

    }
}