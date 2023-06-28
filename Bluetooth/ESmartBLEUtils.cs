using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using OSCLock;
using OSCLock.Bluetooth;
using OSCLock.Bluetooth.Objects;

namespace OSCLock2 {
    public static class ESmartBLEUtils {

        public static byte[] shortToByteArray_Little(short s)
        {
            return new byte[] {(byte)s, (byte)(s >> 8)};
        }
        
        public static byte[] intToByteArray_Little(int i)
        {
            return new byte[] {(byte)i, (byte)(i >> 8), (byte)(i >> 16), (byte)(i >> 24)};
        }
        
        public const byte FRM_STATE_UNKOWN = unchecked((byte) -1);
        public static short byteArrayToShort_Little(byte[] bArr, int i)
        {
            return (short)(((bArr[i + 1] & FRM_STATE_UNKOWN) << 8) | (bArr[i] & FRM_STATE_UNKOWN));
        }
        public static int byteArrayToInt_Little(byte[] bArr, int i)
        {
            return ((bArr[i + 3] & FRM_STATE_UNKOWN) << 24) | (bArr[i] & FRM_STATE_UNKOWN) | ((bArr[i + 1] & FRM_STATE_UNKOWN) << 8) | ((bArr[i + 2] & FRM_STATE_UNKOWN) << 16);
        }

        public static IBuffer PackageUnlockPassword(int token, string pwd, byte pwdLength) {
            byte[] barr = new byte[20];
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Array.Copy(shortToByteArray_Little(18), 0, barr, 0, 2);
            Array.Copy(shortToByteArray_Little(4), 0, barr, 2, 2);
            Array.Copy(intToByteArray_Little(token), 0, barr, 4, 4);
            Array.Copy(intToByteArray_Little((int) (((int) milliseconds / 1000))), 0, barr, 8, 4);

            byte[] pwdBytes = new ASCIIEncoding().GetBytes(pwd);
            //     byte[] pwdBytes = BleAesCrypt.UTF8.GetBytes(pwd);
            
            Console.WriteLine(pwdLength);
            try {
                if (pwdLength > 6)
                    pwdLength = 6;
                Array.Copy(pwdBytes, 0, barr, 12, pwdLength);
            }
            catch (Exception e) {
                Console.WriteLine("Expected crash: "+ e);
            }

            barr[18] = pwdLength;
            var writer = new DataWriter();
            writer.WriteBytes(barr);

            return EncryptMessage(writer.DetachBuffer());
        }
        
        public static BleHeadInfo ParseHead(byte[] data) {
            byte[] tmp = new byte[2];
            Array.Copy(data, 0, tmp, 0, 2);
            short len = byteArrayToShort_Little(tmp, 0);
            Array.Copy(data, 2, tmp, 0, 2);
            short apiID = byteArrayToShort_Little(tmp, 0);

            return new BleHeadInfo(len, apiID);
        }
        
        public static IBuffer PackLogin() {
            DataWriter writer = new DataWriter();

            byte[] a = shortToByteArray_Little(6);
            writer.WriteByte(a[0]);
            writer.WriteByte(a[1]);
            a = shortToByteArray_Little(1);
            writer.WriteByte(a[0]);
            writer.WriteByte(a[1]);
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            a = intToByteArray_Little((int) (milliseconds / 1000));
            writer.WriteBytes(a);
            return EncryptMessage(writer.DetachBuffer());
        }
        
        public static IBuffer EncryptMessage(IBuffer buffer) {
            var encrypt = BleAesCrypt.Encrypt(buffer.ToArray());
            DataWriter writer = new DataWriter();
            var a = shortToByteArray_Little((short)encrypt.Length);
            writer.WriteByte(a[0]);
            writer.WriteByte(a[1]);
            writer.WriteBytes(encrypt);
            return writer.DetachBuffer();
        }
    }
}