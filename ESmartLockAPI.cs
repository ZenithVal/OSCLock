using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OSCLock.Configs;
using Windows.Data.Json;

namespace OSCLock
{
	public static class ESmartLockAPI
	{
		private static HttpClient client = new HttpClient();

		static ESmartLockAPI()
		{
			client.DefaultRequestHeaders.Add("User-Agent", "android");
		}

		public static async Task<string> GetDevicePassword(string deviceAddress, string loginToken)
		{
			var deviceInfoResponse = await client.PostAsync("http://web.iloveismarthome.com?m=lock&a=getLockInfoByMac", new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("user_name", ConfigManager.ApplicationConfig.ESmartConfig.apiUsername),
				new KeyValuePair<string, string>("loginToken", loginToken),
				new KeyValuePair<string, string>("mac", deviceAddress)
			}));

			var responseString = await deviceInfoResponse.Content.ReadAsStringAsync();

			JsonObject response;

			if (!JsonObject.TryParse(responseString, out response))
			{
				Console.WriteLine("Failed to parse response");
				Console.WriteLine(responseString);
				return "ERROR_PARSE";
			}

			if (deviceInfoResponse.StatusCode == HttpStatusCode.OK)
			{
				IJsonValue devicePassword;
				if (response.TryGetValue("data", out devicePassword) && devicePassword.ValueType == JsonValueType.Object)
				{
					var dataObject = devicePassword.GetObject();
					if (dataObject.TryGetValue("password", out devicePassword) && devicePassword.ValueType == JsonValueType.String)
					{
						return devicePassword.GetString();
					}
				}

				Console.WriteLine(response);
				return "ERROR_DATA";
			}
			else
			{
				Console.WriteLine("Failed to login to eSmartHome's cloud");
				Console.WriteLine(response);
				Console.WriteLine(responseString);


				return "ERROR";
			}
		}

		public static async Task<string> Login()
		{
			var loginResponse = await client.PostAsync("http://web.iloveismarthome.com?m=user&a=login", new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("user_name", ConfigManager.ApplicationConfig.ESmartConfig.apiUsername),
				new KeyValuePair<string, string>("user_pwd", ConfigManager.ApplicationConfig.ESmartConfig.apiPassword),
				new KeyValuePair<string, string>("type", "2"),
				new KeyValuePair<string, string>("way", "0")
			}));

			var responseString = await loginResponse.Content.ReadAsStringAsync();
			JsonObject response;

			if (!JsonObject.TryParse(responseString, out response))
			{
				Console.WriteLine("Failed to parse response");
				Console.WriteLine(responseString);
				return "ERROR_PARSE";
			}

			if (loginResponse.StatusCode == HttpStatusCode.OK)
			{
				IJsonValue loginToken;
				if (response.TryGetValue("loginToken", out loginToken))
				{
					return loginToken.GetString();
				}
				else if (response.TryGetValue("type", out loginToken) && loginToken.ValueType == JsonValueType.Number && ((int)loginToken.GetNumber()) is int errorType)
				{
					switch (errorType)
					{
						case 1:
							Console.WriteLine("Wrong username or password configured, see config file");
							return "ERROR_WRONGCREDS";
							//Wrong password
							break;
						case 6:
							Console.WriteLine("No configured credentials");
							return "ERROR_NOCONFIG";
							//No username or password
							break;
						default:
							Console.WriteLine("Unknown error code " + errorType);
							Console.WriteLine(response);
							break;
					}

					return "ERROR_" + errorType;
				}
				else
				{
					Console.WriteLine("Got success login but failed to find login token");
					Console.WriteLine(response);
					Console.WriteLine(response.ToString());
					Console.WriteLine(responseString);
					return "ERROR_NOTOKEN";
				}
			}
			else
			{
				Console.WriteLine("Failed to login to eSmartHome's cloud");
				Console.WriteLine(loginResponse);
				Console.WriteLine(loginResponse.Content);
				Console.WriteLine(response);
				Console.WriteLine(responseString);


				return "ERROR";
			}
		}
	}
}