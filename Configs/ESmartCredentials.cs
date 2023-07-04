using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet.Attributes;

namespace OSCLock.Configs
{
    public class ESmartCredentials
    {
        [TomlPrecedingComment("If you have your lock bound to the app/cloud, we must get the device's password from the cloud for now\nFor this the username and password of an account the lock is shared with is required.")]
        [TomlProperty("esmart_username")]
        public string apiUsername { get; set; }

        [TomlProperty("esmart_password")]
        [TomlInlineComment("Leave blank if not using a cloud/app bound esmartlock")]
        public string apiPassword { get; set; }

        [TomlProperty("device_password")]
        [TomlInlineComment("The password to the device, gotten from the cloud after a successful login.")]
        public string DevicePassword { get; set; }
    }
}
