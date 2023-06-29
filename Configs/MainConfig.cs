using Tomlet.Attributes;
using Tomlet.Models;

namespace OSCLock.Configs {
    public class MainConfig {
        [TomlProperty("ip")]
        public string ipAddress { get; set; }

        [TomlProperty("listener_port")]
        public int listener_port { get; set; }

        [TomlProperty("write_port")]
        public int write_port { get; set; }

        [TomlProperty("Mode")]
        public ApplicationMode Mode { get; set; }

        [TomlProperty("Credentials for ESmartLock")]
        public ESmartCredentials ESmartConfig { get; set; }
        
        [TomlProperty("Basic")]
        public BasicMode BasicConfig { get; set; }
        
        [TomlProperty("Timer")]
        public TimerMode TimerConfig { get; set; }
    }
}