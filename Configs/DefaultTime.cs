using System;
using Tomlet.Attributes;

namespace OSCLock.Configs {
    public class DefaultTime {
        [TomlProperty("starting_value")]
        [TomlInlineComment("Default duration of timer, -1, a random value between the below will be used.")]
        public int startingValue { get; set; }
        [TomlProperty("random_min")]
        public int randomMin { get; set; }
        [TomlProperty("random_max")]
        public int randomMax { get; set; }

        public int GetStartingValue() {
            if (startingValue < 0)
                return new Random().Next(randomMin, randomMax);
            return startingValue;
        }
    }
}