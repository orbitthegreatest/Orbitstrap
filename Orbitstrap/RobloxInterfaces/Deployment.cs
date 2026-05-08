using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Orbitstrap.Enums;
using Orbitstrap.Exceptions;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Utility;

namespace Orbitstrap.RobloxInterfaces;

public static class Deployment
{
	public const string DefaultChannel = "production";

	private const string VersionStudioHash = "version-012732894899482c";

	public static string Channel = App.Settings.Prop.Channel;

	public static string BinaryType = "WindowsPlayer";

	public static readonly List<HttpStatusCode?> BadChannelCodes = new List<HttpStatusCode?>
	{
		HttpStatusCode.Unauthorized,
		HttpStatusCode.Forbidden,
		HttpStatusCode.NotFound
	};

	private static readonly Dictionary<string, ClientVersion> ClientVersionCache = new Dictionary<string, ClientVersion>();

	private static readonly Dictionary<string, int> BaseUrls = new Dictionary<string, int>
	{
		{ "https://setup.rbxcdn.com", 0 },
		{ "https://setup-aws.rbxcdn.com", 2 },
		{ "https://setup-ak.rbxcdn.com", 2 },
		{ "https://roblox-setup.cachefly.net", 2 },
		{ "https://s3.amazonaws.com/setup.roblox.com", 4 }
	};

	public static bool IsDefaultChannel
	{
		get
		{
			if (!Channel.Equals("production", StringComparison.OrdinalIgnoreCase))
			{
				return Channel.Equals("live", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}

	public static string BaseUrl { get; private set; } = null;

	private static async Task<string?> TestConnection(string url, int priority, CancellationToken token)
	{
		string LOG_IDENT = "Deployment::TestConnection<" + url + ">";
		await Task.Delay(priority * 1000, token);
		App.Logger.WriteLine(LOG_IDENT, "Connecting...");
		try
		{
			HttpResponseMessage obj = await App.HttpClient.GetAsync(url + "/versionStudio", token);
			obj.EnsureSuccessStatusCode();
			string text = await obj.Content.ReadAsStringAsync(token);
			if (text != "version-012732894899482c")
			{
				throw new InvalidHTTPResponseException($"versionStudio response does not match (expected \"{"version-012732894899482c"}\", got \"{text}\")");
			}
		}
		catch (TaskCanceledException)
		{
			App.Logger.WriteLine(LOG_IDENT, "Connectivity test cancelled.");
			throw;
		}
		catch (Exception ex2)
		{
			App.Logger.WriteException(LOG_IDENT, ex2);
			throw;
		}
		return url;
	}

	public static async Task<Exception?> InitializeConnectivity()
	{
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		List<Exception> exceptions = new List<Exception>();
		List<Task<string>> tasks = BaseUrls.Select<KeyValuePair<string, int>, Task<string>>(delegate(KeyValuePair<string, int> entry)
		{
			KeyValuePair<string, int> keyValuePair = entry;
			string key = keyValuePair.Key;
			keyValuePair = entry;
			return TestConnection(key, keyValuePair.Value, tokenSource.Token);
		}).ToList();
		App.Logger.WriteLine("Deployment::InitializeConnectivity", "Testing connectivity...");
		while (tasks.Any() && string.IsNullOrEmpty(BaseUrl))
		{
			Task<string> task = await Task.WhenAny(tasks);
			tasks.Remove(task);
			if (task.IsFaulted)
			{
				exceptions.Add(task.Exception.InnerException);
			}
			else if (!task.IsCanceled)
			{
				BaseUrl = task.Result;
			}
		}
		tokenSource.Cancel();
		if (string.IsNullOrEmpty(BaseUrl))
		{
			if (exceptions.Any())
			{
				return exceptions[0];
			}
			return new TaskCanceledException("All connection attempts timed out.");
		}
		App.Logger.WriteLine("Deployment::InitializeConnectivity", "Got " + BaseUrl + " as the optimal base URL");
		return null;
	}

	public static string GetLocation(string resource)
	{
		string text = BaseUrl;
		if (!IsDefaultChannel)
		{
			text += "/channel/common";
		}
		return text + resource;
	}

	public static async Task<ClientVersion> GetInfo(string? channel = null)
	{
		if (string.IsNullOrEmpty(channel))
		{
			channel = Channel;
		}
		bool isDefaultChannel = string.Compare(channel, "production", StringComparison.OrdinalIgnoreCase) == 0;
		App.Logger.WriteLine("Deployment::GetInfo", "Getting deploy info for channel " + channel);
		string cacheKey = channel + "-" + BinaryType;
		ClientVersion clientVersion;
		if (ClientVersionCache.ContainsKey(cacheKey))
		{
			App.Logger.WriteLine("Deployment::GetInfo", "Deploy information is cached");
			clientVersion = ClientVersionCache[cacheKey];
		}
		else
		{
			string path = "/v2/client-version/" + BinaryType;
			if (!isDefaultChannel)
			{
				path = "/v2/client-version/" + BinaryType + "/channel/" + channel;
			}
			try
			{
				clientVersion = await Http.GetJson<ClientVersion>("https://clientsettingscdn.roblox.com" + path);
			}
			catch (HttpRequestException ex) when (!isDefaultChannel && BadChannelCodes.Contains(ex.StatusCode))
			{
				throw new InvalidChannelException(ex.StatusCode);
			}
			catch (Exception ex2)
			{
				App.Logger.WriteLine("Deployment::GetInfo", "Failed to contact clientsettingscdn! Falling back to clientsettings...");
				App.Logger.WriteException("Deployment::GetInfo", ex2);
				try
				{
					clientVersion = await Http.GetJson<ClientVersion>("https://clientsettings.roblox.com" + path);
				}
				catch (HttpRequestException ex3) when (!isDefaultChannel && BadChannelCodes.Contains(ex3.StatusCode))
				{
					throw new InvalidChannelException(ex3.StatusCode);
				}
			}
			if (!isDefaultChannel)
			{
				ClientVersion clientVersion2 = await GetInfo("production");
				if (Utilities.CompareVersions(clientVersion.Version, clientVersion2.Version) == VersionComparison.LessThan)
				{
					clientVersion.IsBehindDefaultChannel = true;
				}
			}
			ClientVersionCache[cacheKey] = clientVersion;
		}
		return clientVersion;
	}
}
