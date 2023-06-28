using System;
using Tomlet.Attributes;

namespace OSCLock.Configs {
    public class TimerMode {
        [TomlProperty("max")]
        //[TomlInlineComment("Max minutes. How much sand can the hourglass hold? ")]
        public int maxTime { get; set; }

        [TomlProperty("absolute_min")]
        //[TomlInlineComment("Miniumum time that must pass before the system can unlock. Minimum Sand")]
        public int absMin { get; set; }

        [TomlProperty("absolute_max")]
        //[TomlInlineComment("If the total time has reached this, it can not increase. How much total sand is there?")]
        public int absMax { get; set; }

        [TomlProperty("StartingTime")]
        public DefaultTime StartTime { get; set; }

        [TomlPrecedingComment("\n--- Incoming OSC Parameters ---")]
        //[TomlInlineComment("When this Bool is true, it should increase the timer once by inc_step.")]
        public string inc_parameter { get; set; }

        //[TomlInlineComment("Time in whole minutes to add when inc_parameter true recieved")]
        public int inc_step { get; set; }

        //[TomlInlineComment("When this Bool is true, it should decrease the timer by dec_step.")]
        public string dec_parameter { get; set; }

        //[TomlInlineComment("Time in whole minutes to remove when dec_parameter true recieved")]
        public int dec_step { get; set; }


        [TomlPrecedingComment("\n-- Outgoing OSC Parameters ---")]
        public int readout_mode { get; set; }
        public string readout_parameter { get; set; }
        public string readout_parameter2 { get; set; }



        //[TomlInlineComment("Time In miliseconds between messages")]
        public int readout_interval { get; set; }

    }
}