namespace OSCLock.Bluetooth.Objects {
    public class BleHeadInfo {
        public short apiID;
        public short len;

        public BleHeadInfo(short len, short apiID) {
            this.len = len;
            this.apiID = apiID;
        }

    }
}