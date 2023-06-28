using Tomlet.Attributes;
using Tomlet.Models;

namespace OSCLock.Configs {
    public class MainConfig {
        [TomlProperty("ip")]
        public string vrchatAddress { get; set; }

        [TomlProperty("listener_port")]
        public int port { get; set; }

        [TomlProperty("write_port")]
        public int vrchatPort { get; set; }

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