using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Orbitstrap.Models.APIs.Roblox;

namespace Orbitstrap.RobloxInterfaces;

public class ApplicationSettings
{
	private string _applicationName;

	private string _channelName;

	private bool _initialised;

	private Dictionary<string, string>? _flags;

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	private static Dictionary<string, Dictionary<string, ApplicationSettings>> _cache = new Dictionary<string, Dictionary<string, ApplicationSettings>>();

	public static ApplicationSettings PCDesktopClient => GetSettings("PCDesktopClient");

	public static ApplicationSettings PCClientBootstrapper => GetSettings("PCClientBootstrapper");

	private ApplicationSettings(string applicationName, string channelName)
	{
		_applicationName = applicationName;
		_channelName = channelName;
	}

	private async Task Fetch()
	{
		if (_initialised)
		{
			return;
		}
		await semaphoreSlim.WaitAsync();
		try
		{
			if (!_initialised)
			{
				string logIndent = "ApplicationSettings::Fetch." + _applicationName + "." + _channelName;
				App.Logger.WriteLine(logIndent, "Fetching fast flags");
				string path = "/v2/settings/application/" + _applicationName;
				if (_channelName != "production".ToLowerInvariant())
				{
					path = path + "/bucket/" + _channelName;
				}
				HttpResponseMessage response;
				try
				{
					response = await App.HttpClient.GetAsync("https://clientsettingscdn.roblox.com" + path);
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine(logIndent, "Failed to contact clientsettingscdn! Falling back to clientsettings...");
					App.Logger.WriteException(logIndent, ex);
					response = await App.HttpClient.GetAsync("https://clientsettings.roblox.com" + path);
				}
				string json = await response.Content.ReadAsStringAsync();
				response.EnsureSuccessStatusCode();
				ClientFlagSettings clientFlagSettings = JsonSerializer.Deserialize<ClientFlagSettings>(json);
				if (clientFlagSettings == null)
				{
					throw new Exception("Deserialised client settings is null!");
				}
				if (clientFlagSettings.ApplicationSettings == null)
				{
					throw new Exception("Deserialised application settings is null!");
				}
				_flags = clientFlagSettings.ApplicationSettings;
				_initialised = true;
			}
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	public async Task<T?> GetAsync<T>(string name)
	{
		await Fetch();
		if (!_flags.ContainsKey(name))
		{
			return default(T);
		}
		string text = _flags[name];
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter == null)
			{
				return default(T);
			}
			return (T)converter.ConvertFromString(text);
		}
		catch (NotSupportedException)
		{
			return default(T);
		}
	}

	public T? Get<T>(string name)
	{
		return GetAsync<T>(name).Result;
	}

	public static ApplicationSettings GetSettings(string applicationName, string channelName = "production", bool shouldCache = true)
	{
		channelName = channelName.ToLowerInvariant();
		lock (_cache)
		{
			if (_cache.ContainsKey(applicationName) && _cache[applicationName].ContainsKey(channelName))
			{
				return _cache[applicationName][channelName];
			}
			ApplicationSettings applicationSettings = new ApplicationSettings(applicationName, channelName);
			if (shouldCache)
			{
				if (!_cache.ContainsKey(applicationName))
				{
					_cache[applicationName] = new Dictionary<string, ApplicationSettings>();
				}
				_cache[applicationName][channelName] = applicationSettings;
			}
			return applicationSettings;
		}
	}
}
