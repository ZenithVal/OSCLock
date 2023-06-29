using System;
using Tomlet.Attributes;

namespace OSCLock.Configs {
    public class TimerMode {
        [TomlProperty("max")]
        [TomlInlineComment("Max minutes at a given moment. How much sand can the hourglass hold?")]
        public int maxTime { get; set; }

        [TomlProperty("absolute_min")]
        [TomlInlineComment("Miniumum time that must pass before the system can unlock. Minimum Sand that must pass")]
        public int absMin { get; set; }


        [TomlProperty("absolute_max")]
        [TomlInlineComment("If total time >= this, ignore all time add requests")]
        public int absMax { get; set; }

        [TomlProperty("StartingTime")]
        public DefaultTime StartTime { get; set; }

        [TomlPrecedingComment("--- Incoming OSC Parameters ---")]
        [TomlInlineComment("When this Bool is true, it should increase the timer once by inc_step.")]
        public string inc_parameter { get; set; }

        //Should make this a float and allow seconds to be added.
        [TomlInlineComment("Time (whole minutes) added per dec_step")]
        public int inc_step { get; set; }

        [TomlInlineComment("Avatar parameter used.")]
        public string dec_parameter { get; set; }

        //Should make this a float and allow seconds to be added.
        [TomlInlineComment("Time (whole minutes) removed per dec_step")]
        public int dec_step { get; set; }

        [TomlInlineComment("Minimum Time (miliseconds) between allowed inputs")]
        public int input_delay { get; set; }


        [TomlPrecedingComment("-- Outgoing OSC Parameters ---")]
        public int readout_mode { get; set; }
        public string readout_parameter { get; set; }
        public string readout_parameter2 { get; set; }


        [TomlInlineComment("Time (miliseconds) between outgoing messages")]
        public int readout_interval { get; set; }

    }
}