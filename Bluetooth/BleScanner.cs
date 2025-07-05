using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace OSCLock.Bluetooth
{
	public static class BleScanner
	{
		private static DeviceWatcher deviceWatcher;

		private static string[] requestedProperties = {
			"System.Devices.Aep.DeviceAddress",
		};

		private static TaskCompletionSource<ESmartLock> ELookResult;


		public static Task<ESmartLock> FindESmartLock()
		{
			Console.WriteLine("No ble address configured, scanning for ESmartLock device instead");
			ELookResult = new TaskCompletionSource<ESmartLock>();
			deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false), requestedProperties, DeviceInformationKind.AssociationEndpointService);
			deviceWatcher.Added += OnDeviceDiscovered;
			deviceWatcher.Removed += OnDeviceUndiscovered;
			deviceWatcher.Updated += OnDeviceUpdate;
			deviceWatcher.Start();
			return ELookResult.Task;
		}

		private static async void OnDeviceUpdate(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			try
			{
				object deviceAddress;
				if (args.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out deviceAddress) && deviceAddress.ToString().StartsWith(ESMARTLOCK_ADDR))
				{
					var bleDevice = await BluetoothLEDevice.FromIdAsync(args.Id);
					var serviceResult = await bleDevice.GetGattServicesAsync();

					if (serviceResult.Status == GattCommunicationStatus.Success)
					{
						GattDeviceService lockService = serviceResult.Services.FirstOrDefault(service => service.Uuid == SERVICE_UUID);
						if (lockService != null)
						{
							var smartLock = await ESmartLock.TryConstructLock(bleDevice, lockService, deviceAddress.ToString());
							if (smartLock != null)
							{
								deviceWatcher.Stop();
								Console.Clear();
								Console.WriteLine("Found smartlock!");
								ELookResult.SetResult(smartLock);
								return;
							}
						} //Device does not have the right service
					}
					else
					{
						var reason = "";
						switch (serviceResult.Status)
						{
							case GattCommunicationStatus.ProtocolError:
								reason = "because an protocol error occured, error: " + serviceResult.ProtocolError;
								break;
							case GattCommunicationStatus.AccessDenied:
								reason = "due to missing permissions, try running the app as admin and/or restarting the app and lock.";
								break;
							case GattCommunicationStatus.Unreachable:
								reason = "because the device is currently unreachable, try going closer or restarting the device.";
								break;
						}

						Console.WriteLine($"Unable to determine if an eSmartDevice with address {deviceAddress} is a lock, {reason}");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed on discovery " + e);
			}
		}

		private static async void OnDeviceUndiscovered(DeviceWatcher sender, DeviceInformationUpdate args)
		{
		}

		private static Guid SERVICE_UUID = Guid.Parse("00001828-0000-1000-8000-00805f9b34fb");
		private static string ESMARTLOCK_ADDR = "a4:c1:38";

		private static async void OnDeviceDiscovered(DeviceWatcher sender, DeviceInformation args)
		{
			try
			{
				object deviceAddress;
				if (args.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out deviceAddress) && deviceAddress.ToString().StartsWith(ESMARTLOCK_ADDR))
				{
					Console.WriteLine(deviceAddress);
					Console.WriteLine(args.Name);
					var bleDevice = await BluetoothLEDevice.FromIdAsync(args.Id);
					var serviceResult = await bleDevice.GetGattServicesAsync();

					if (serviceResult.Status == GattCommunicationStatus.Success)
					{
						GattDeviceService lockService = serviceResult.Services.FirstOrDefault(service => service.Uuid == SERVICE_UUID);
						if (lockService != null)
						{
							var smartLock = await ESmartLock.TryConstructLock(bleDevice, lockService, deviceAddress.ToString());
							if (smartLock != null)
							{
								deviceWatcher.Stop();
								Console.Clear();
								Console.WriteLine("Found smartlock!");
								ELookResult.SetResult(smartLock);
								return;
							}
						} //Device does not have the right service
					}
					else
					{
						var reason = "";
						switch (serviceResult.Status)
						{
							case GattCommunicationStatus.ProtocolError:
								reason = "because an protocol error occured, error: " + serviceResult.ProtocolError;
								break;
							case GattCommunicationStatus.AccessDenied:
								reason = "due to missing permissions, try running the app as admin and/or restarting the app and lock.";
								break;
							case GattCommunicationStatus.Unreachable:
								reason = "because the device is currently unreachable, try going closer or restarting the device.";
								break;
						}

						Console.WriteLine($"Unable to determine if an eSmartDevice{(args.Name.Length > 0 ? $" named {args.Name}, and" : "")} with address {deviceAddress} is a lock, {reason}");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed on discovery " + e);
			}
		}
	}
}