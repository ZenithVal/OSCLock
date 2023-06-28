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
        [TomlProperty("write_address")]
        public string vrchatAddress { get; set; }
        
        [TomlProperty("device_password_from_cloud")]
        [TomlPrecedingComment("The password to the device, gotten from the cloud")]
        public string DevicePassword { get; set; }

        [TomlPrecedingComment("\n--- Eseesmart lock Login ---")]
        [TomlPrecedingComment("If you have your lock bound to the app/cloud, we must get the device's password from the cloud for now\n\nFor this the username and password of an account the lock is shared with is required.")]
        [TomlProperty("esmart_username")]
        public string apiUsername { get; set; }

        [TomlProperty("esmart_password")]
        [TomlInlineComment("Leave blank if not using a cloud/app bound esmartlock")]
        public string apiPassword { get; set; }

        [TomlProperty("device_password")]
        [TomlInlineComment("The password to the device, gotten from the cloud after a successful login.")]
        public string DevicePassword { get; set; }

        [TomlPrecedingComment("")]
        [TomlProperty("Basic")]
        public BasicMode BasicConfig { get; set; }
        
        [TomlPrecedingComment("")]
        [TomlProperty("Timer")]
        public TimerMode TimerConfig { get; set; }
    }
}