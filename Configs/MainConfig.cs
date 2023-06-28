using Tomlet.Attributes;
using Tomlet.Models;

namespace OSCLock.Configs {
    public class MainConfig {
        [TomlProperty("listener_port")]
        public int port { get; set; }
        [TomlProperty("write_port")]
        public int vrchatPort { get; set; }
        [TomlProperty("write_address")]
        public string vrchatAddress { get; set; }
        
        [TomlProperty("device_password_from_cloud")]
        [TomlPrecedingComment("The password to the device, gotten from the cloud")]
        public string DevicePassword { get; set; }

        [TomlPrecedingComment("If you have your lock bound to the app/cloud, we must get the device's password from the cloud for now\n\nFor this the username and password of an account the lock is shared with is required.")]
        [TomlProperty("esmart_username")]
        public string apiUsername { get; set; }
        [TomlProperty("esmart_password")]
        [TomlInlineComment("Leave blank if not using a cloud/app bound esmartlock")]
        public string apiPassword { get; set; }
        
        [TomlProperty("mode")]
        public ApplicationMode Mode { get; set; }

        [TomlProperty("Basic")]
        public BasicMode BasicConfig { get; set; }
        
        [TomlProperty("Timer")]
        public TimerMode TimerConfig { get; set; }
    }
}