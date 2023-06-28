using Tomlet.Attributes;

namespace OSCLock.Configs {
    public class TimerMode {
        [TomlProperty("max")]
        public int maxTime { get; set; }

        [TomlProperty("absolute_min")]
        [TomlPrecedingComment("Miniumum time that must pass before you can unlock, note that this time cannot be reduced")]
        public int absMin { get; set; }
        [TomlProperty("absolute_max")]
        public int absMax { get; set; }

        [TomlProperty("StartingTime")]
        public DefaultTime defaultTime { get; set; }
        
        public string inc_parameter { get; set; }
        public int inc_step { get; set; }
        public string dec_parameter { get; set; }
        public int dec_step { get; set; }
        [TomlPrecedingComment("Parameter to write to for vrchat to get remaining time")]
        public string readout_parameter { get; set; }
        [TomlInlineComment("NB! In miliseconds, 500 = 0.5s, 1000s = 1s, 1500 = 1.5s ect.")]
        public int readout_interval { get; set; }
    }
}