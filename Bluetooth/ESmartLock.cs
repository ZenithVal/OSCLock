using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Animation;
using OSCLock.Configs;
using OSCLock2;

namespace OSCLock.Bluetooth {
    public class ESmartLock {
        
        public BluetoothLEDevice BleDevice { get; }
        public GattDeviceService Service { get; }
        public GattCharacteristic WriteChar { get; }
        public GattCharacteristic NotifyChar { get; }
        public string Id { get; }

        private static Guid NotifyGUID = Guid.Parse("00002ade-0000-1000-8000-00805f9b34fb");
        private static Guid WriteGUID = Guid.Parse("00002add-0000-1000-8000-00805f9b34fb");
        
        public int DeviceToken { get; set; }

        private ESmartLock(BluetoothLEDevice bleDevice, GattDeviceService service, GattCharacteristic writeChar, GattCharacteristic notifyChar, string id) {
            BleDevice = bleDevice;
            Service = service;
            WriteChar = writeChar;
            NotifyChar = notifyChar;
            Id = id;
            DeviceToken = 0;
        }

        /*
         * In order to open the lock a password is needed, this function gets the password.
         *
         * Note that the password is available through the API if its bound to the cloud,
         * else its the default password of 123456.
         *
         * Passwords seem to always be numeric, and consists of the numbers from 1-6, never 7, 8, 9 or 0.
         * Infact the offical app rejects passwords containing 7, 8, 9 and 0 as invalid.aa
         */
        private async Task<string> GetDevicePassword() {
            if (isCloudSetup) {
                if (string.IsNullOrEmpty(ConfigManager.ApplicationConfig.DevicePassword)) {
                    Console.WriteLine("Device password unknown, grabbing from api... logging in...");
                    var loginToken = await ESmartLockAPI.Login();
                    if (loginToken.StartsWith("ERROR"))
                        return "ERROR_NO_PASS";
                    var devicePassword = await ESmartLockAPI.GetDevicePassword(Id, loginToken);
                    if (loginToken.StartsWith("ERROR"))
                        return "ERROR_NO_PASS";
                    
                    Console.WriteLine("Retrived device password from cloud, saving it to config");
                    ConfigManager.ApplicationConfig.DevicePassword = devicePassword;
                    ConfigManager.Save();
                }

                return ConfigManager.ApplicationConfig.DevicePassword;
            }
            else return "123456";
        }
        
        public async Task Unlock() {
            Console.WriteLine("Unlocking lock " + Id);
            if (DeviceToken == 0)
                await AuthenticateToDevice();
            Console.WriteLine("Device token is" + DeviceToken);
            var password = await GetDevicePassword();
            
            Console.WriteLine($"Attempting unlock with deviceToken {DeviceToken}, and password {password}");
            await SendToDevice(ESmartBLEUtils.PackageUnlockPassword(DeviceToken, password, (byte) password.Length));
        }

        private TaskCompletionSource<BleLoginInfo> authenticationCompletion;

        private bool isCloudSetup = false;
        private async Task AuthenticateToDevice() {
            //Subscribe to notify
            GattCommunicationStatus status = await NotifyChar.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (status != GattCommunicationStatus.Success) Console.WriteLine("Failed to notify :c");
            else NotifyChar.ValueChanged += FromDevice;

            Console.WriteLine("Login in to device");
            authenticationCompletion = new TaskCompletionSource<BleLoginInfo>();
            await SendToDevice(ESmartBLEUtils.PackLogin());

            var result = await authenticationCompletion.Task;
            
            Console.Write("Logged into device");
            byte[] dtokenBuffer = new byte[4];
            Array.Copy(BleAesCrypt.Encrypt(result.random), 0, dtokenBuffer, 0, 4);
            DeviceToken = ESmartBLEUtils.byteArrayToInt_Little(dtokenBuffer, 0);
            Console.WriteLine($", calculated deviceToken " + DeviceToken);

            Console.Write("Checking if device is bound to cloud");
            isCloudSetup = result.bindCloud != 0;
            if (isCloudSetup)
                Console.WriteLine(", bound to cloud");
            else Console.WriteLine(", unbound");
        }
        
        private static Queue<byte[]> splitByte(byte[] bArr, int BUFFER_SIZE) {
            int NUM_CHUNKS;
            byte[] bArr2;
            if (BUFFER_SIZE > 20) {
                Console.WriteLine("Split count beyond 20! ensure MTU higher then 23");
            }

            if (bArr.Length % BUFFER_SIZE == 0) {
                NUM_CHUNKS = bArr.Length / BUFFER_SIZE;
            }
            else {
                NUM_CHUNKS = (int) (((float) (bArr.Length) / BUFFER_SIZE) + 1);
            }

            Console.WriteLine("i2: " + NUM_CHUNKS);
            Queue<byte[]> queue = new Queue<byte[]>();
            if (NUM_CHUNKS > 0) {
                for (int CHUNK_INDEX = 0; CHUNK_INDEX < NUM_CHUNKS; CHUNK_INDEX++) {
                    if (NUM_CHUNKS == 1 || CHUNK_INDEX == NUM_CHUNKS - 1) {
                        int length = bArr.Length % BUFFER_SIZE == 0 ? BUFFER_SIZE : bArr.Length % BUFFER_SIZE;
                        byte[] bArr3 = new byte[length];
                        Array.Copy(bArr, CHUNK_INDEX * BUFFER_SIZE, bArr3, 0, length);
                        bArr2 = bArr3;
                    }
                    else {
                        bArr2 = new byte[BUFFER_SIZE];
                        Array.Copy(bArr, CHUNK_INDEX * BUFFER_SIZE, bArr2, 0, BUFFER_SIZE);
                    }
                    queue.Enqueue(bArr2);
                }
            }

            return queue;
        }

        public static int SPLIT_WRITE_NUM = 20;

        private async Task<GattCommunicationStatus> SendToDevice(IBuffer data) {
            GattCommunicationStatus lastStatus = GattCommunicationStatus.Unreachable;
            byte[] bArr = data.ToArray();
            
            if (bArr != null) {
                int i = SPLIT_WRITE_NUM;
                if (i >= 1) {
                    Console.WriteLine("Splitting payload");
                    var splitDatas = splitByte(bArr, i);
                    Console.Write("Data has been split,");
                    var totalNumOfRequests = splitDatas.Count;
                    Console.WriteLine("into " + totalNumOfRequests);

                    for (int x = 0; x < totalNumOfRequests; x++) {
                        Console.Write("Sending req nr: " + x);
                        var buffer = splitDatas.Dequeue();
                        DataWriter writer = new DataWriter();
                        writer.WriteBytes(buffer);
                        lastStatus = await WriteChar.WriteValueAsync(writer.DetachBuffer());
                        Console.WriteLine(", result " + lastStatus);
                    }
                }
            }

            return lastStatus;
        }

        public static short CurrentPacket_TOTALSIZE = 0;
        public static byte[] CurrentPacket_DATA;
        public static int DATA_LENGTH = 0;
        
        private async void FromDevice(GattCharacteristic sender, GattValueChangedEventArgs args) {
            try {

                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                var data = reader.DetachBuffer().ToArray();

                Console.WriteLine("Got message from lock!!!");
                if (CurrentPacket_TOTALSIZE == 0) {
                    byte[] sizeBuffer = new byte[2];
                    Array.Copy(data, 0, sizeBuffer, 0, 2);
                    CurrentPacket_TOTALSIZE = ESmartBLEUtils.byteArrayToShort_Little(sizeBuffer, 0);
                    Console.WriteLine("Read short: " + CurrentPacket_TOTALSIZE);
                    reader.ByteOrder = ByteOrder.LittleEndian;

                    CurrentPacket_DATA = new byte[CurrentPacket_TOTALSIZE];
                    if (CurrentPacket_TOTALSIZE >= reader.UnconsumedBufferLength) {
                        Console.WriteLine("Read bytes into data");
                        Console.WriteLine(data.Length);
                        Console.WriteLine(CurrentPacket_TOTALSIZE);
                        Console.WriteLine("AAAA");
                        Array.Copy(data, 2, CurrentPacket_DATA, DATA_LENGTH, data.Length - 2);
                        DATA_LENGTH = data.Length - 2;
                    }
                    else {
                        Console.WriteLine("Failed so we set stuff to 0");
                        CurrentPacket_TOTALSIZE = 0;
                        CurrentPacket_DATA = null;
                        DATA_LENGTH = 0;
                    }
                }
                else {
                    Array.Copy(data, 0, CurrentPacket_DATA, DATA_LENGTH, data.Length);
                    DATA_LENGTH += data.Length;
                    Console.WriteLine("Reading rest");

                }

                Console.WriteLine($"Total size: {CurrentPacket_TOTALSIZE}, data Length {DATA_LENGTH}");

                if (CurrentPacket_TOTALSIZE != 0 && DATA_LENGTH >= CurrentPacket_TOTALSIZE) {
                    var x = BleAesCrypt.Decrypt(CurrentPacket_DATA);
                    await onDeviceDataMessage(x);
                    Console.WriteLine("Handling packet, decrypted data is of size: " + x.Length);

                    CurrentPacket_TOTALSIZE = 0;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error while reading from device " + e, e);
            }
        }
        
        private async Task HandleLoginResponse(byte[] bytes) {
            BleLoginInfo loginInfo = BleLoginInfo.Parse(bytes);
            authenticationCompletion.SetResult(loginInfo);
            /*
            Console.WriteLine("Got login info, protocol version: " + loginInfo.protocolVersion);
            Console.WriteLine(loginInfo.ToString());
            try {
                Console.WriteLine("Trying to parse token");
                byte[] dtokenBuffer = new byte[4];
                Array.Copy(BleAesCrypt.Encrypt(loginInfo.random), 0, dtokenBuffer, 0, 4);
                DeviceToken = ESmartBLEUtils.byteArrayToInt_Little(dtokenBuffer, 0);
                Console.WriteLine("Recieved device token by device: " + dtokenBuffer);
            }
            catch (Exception e) {
                Console.WriteLine("AA: " + e);
            }
            */
        }
        
        private async Task onDeviceDataMessage(byte[] bytes) {
            var head = ESmartBLEUtils.ParseHead(bytes);

            switch (head.apiID) {
                case 1:
                    await HandleLoginResponse(bytes);
                    break;
                default:
                    Console.WriteLine("UNHANDLED_API_ID: " + head.apiID);
                    break;
            }
        }

        public static async Task<ESmartLock> TryConstructLock(BluetoothLEDevice bleDevice, GattDeviceService service, string bluetoothAddress) {
            try {
                var characteristicsResult = await service.GetCharacteristicsAsync();
                if (characteristicsResult.Status != GattCommunicationStatus.Success) throw new Exception("Failed to get characteristics, reason: " + characteristicsResult.Status);

                var writeChar = characteristicsResult.Characteristics.FirstOrDefault(chr => chr.Uuid == WriteGUID) ?? throw new Exception("Failed to find write characteristic");
                var notifyChar = characteristicsResult.Characteristics.FirstOrDefault(chr => chr.Uuid == NotifyGUID) ?? throw new Exception("Failed to find read characteristic");

                var loc = new ESmartLock(bleDevice, service, writeChar, notifyChar, bluetoothAddress);
                return loc;
            }
            catch (Exception e) {
                Console.WriteLine("Failed to create device: " + e.Message);
                return null;
            }
        }
    }
}