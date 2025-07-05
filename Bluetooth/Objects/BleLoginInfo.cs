// Include namespace system
using System;

public class BleLoginInfo
{
	private byte alarmState;
	private byte alarmSup;
	private short apiId;
	private byte bFacState;
	private byte backAdv;
	private byte backAdvSup;
	public byte bindCloud;
	private byte bindCloudSup;
	private byte fgpPageSup;
	private byte fgpSup;
	private byte[] fwVersion;
	private short len;
	private byte power;
	private byte preLose;
	private byte preLoseSup;
	public int protocolVersion;
	public byte[] random;
	private byte state;
	public static short byteArrayToShort_Little(byte[] var0, int var1)
	{
		var var2 = var0[var1];
		return (short)((var0[var1 + 1] & 255) << 8 | var2 & 255);
	}
	public static BleLoginInfo Parse(byte[] var0)
	{
		byte[] len = new byte[2];
		Array.Copy(var0, 0, len, 0, 2);
		byte[] apiId = new byte[2];
		Array.Copy(var0, 2, apiId, 0, 2);
		byte[] random = new byte[4];
		Array.Copy(var0, 4, random, 0, 4);
		// Protocol version (Reuse fwversion)
		byte[] fwVersion = new byte[4];
		Array.Copy(var0, 8, fwVersion, 0, 4);
		var protocolVersion = BleLoginInfo.byteArrayToShort_Little(fwVersion, 0);
		// Fw version
		fwVersion = new byte[4];
		Array.Copy(var0, 12, fwVersion, 0, 4);
		// State and power
		byte state = var0[16];
		byte power = var0[17];
		// Prelose
		byte preLoseSup = 0;
		byte preLose = 0;
		if (protocolVersion >= 2)
		{
			preLoseSup = var0[18];
			if (preLoseSup == 1)
			{
				preLose = var0[19];
			}
			else
			{
				preLoseSup = 0;
			}
		}
		// BackAdv
		byte backAdvSup = 0;
		byte backAdv = 0;
		if (protocolVersion >= 3)
		{
			backAdvSup = var0[20];
			if (backAdvSup == 1)
			{
				backAdv = var0[21];
			}
			else
			{
				backAdvSup = 0;
			}
		}
		// Cloud featur
		byte bindCloudSup = 0;
		byte bindCloud = 0;
		if (protocolVersion >= 4)
		{
			bindCloudSup = var0[22];
			if (bindCloudSup == 1)
			{
				bindCloud = var0[23];
			}
			else
			{
				bindCloudSup = 0;
			}
		}
		// Another feature
		byte fgpSup = 0;
		byte bFacState = 0;
		if (protocolVersion >= 5)
		{
			fgpSup = var0[24];
			bFacState = var0[25];
		}
		byte alarmSup = 0;
		byte alarmState = 0;
		// Alarm
		if (protocolVersion >= 7)
		{
			Console.WriteLine("DEBUG: ");
			Console.WriteLine(string.Join(", ", var0));
			alarmSup = var0[38];
			if (alarmSup == 1)
			{
				// If alarms are supported
				alarmState = var0[39];
			}
			else
			{
				alarmSup = 0;
			}
		}
		// fingerprint
		byte fgpPageSup = 0;
		// Default value is 0
		if (protocolVersion >= 8)
		{
			Console.WriteLine("DEBUG: ");
			Console.WriteLine(string.Join(", ", var0));
			// supportsFingerPrints = var0[40];
			fgpPageSup = var0[40];
		}
		return new BleLoginInfo(BleLoginInfo.byteArrayToShort_Little(len, 0), BleLoginInfo.byteArrayToShort_Little(apiId, 0), random, protocolVersion, fwVersion, state, power, preLoseSup, preLose, backAdvSup, backAdv, bindCloudSup, bindCloud, fgpSup, bFacState, alarmSup, alarmState, fgpPageSup);
	}
	public BleLoginInfo(short var1, short var2, byte[] var3, int var4, byte[] var5, byte var6, byte var7, byte var8, byte var9, byte var10, byte var11, byte var12, byte var13, byte var14, byte var15, byte var16, byte var17, byte var18)
	{
		this.len = (short)var1;
		this.apiId = (short)var2;
		this.random = var3;
		this.protocolVersion = var4;
		this.fwVersion = var5;
		this.state = (byte)var6;
		this.power = (byte)var7;
		this.preLoseSup = (byte)var8;
		this.preLose = (byte)var9;
		this.backAdvSup = (byte)var10;
		this.backAdv = (byte)var11;
		this.bindCloudSup = (byte)var12;
		this.bindCloud = (byte)var13;
		this.fgpSup = (byte)var14;
		this.bFacState = (byte)var15;
		this.alarmSup = (byte)var16;
		this.alarmState = (byte)var17;
		this.fgpPageSup = (byte)var18;
	}

	public override string ToString()
	{
		return $"{nameof(alarmState)}: {alarmState},\n{nameof(alarmSup)}: {alarmSup},\n{nameof(apiId)}: {apiId},\n{nameof(bFacState)}: {bFacState},\n{nameof(backAdv)}: {backAdv},\n{nameof(backAdvSup)}: {backAdvSup},\n{nameof(bindCloud)}: {bindCloud},\n{nameof(bindCloudSup)}: {bindCloudSup},\n{nameof(fgpPageSup)}: {fgpPageSup},\n{nameof(fgpSup)}: {fgpSup},\n{nameof(fwVersion)}: {fwVersion},\n{nameof(len)}: {len},\n{nameof(power)}: {power},\n{nameof(preLose)}: {preLose},\n{nameof(preLoseSup)}: {preLoseSup},\n{nameof(protocolVersion)}: {protocolVersion},\n{nameof(random)}: {random},\n{nameof(state)}: {state}";
	}
}