using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Orbitstrap.Extensions;

internal static class HttpClientEx
{
	public static async Task<HttpResponseMessage> GetWithRetriesAsync(this HttpClient client, string url, int retries, CancellationToken token)
	{
		HttpResponseMessage response = null;
		for (int i = 1; i <= retries; i++)
		{
			try
			{
				response = await client.GetAsync(url, token);
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				App.Logger.WriteException("HttpClientEx::GetWithRetriesAsync", ex2);
				if (i == retries)
				{
					throw;
				}
			}
		}
		return response;
	}

	public static async Task<HttpResponseMessage> PostWithRetriesAsync(this HttpClient client, string url, HttpContent? content, int retries, CancellationToken token)
	{
		HttpResponseMessage response = null;
		for (int i = 1; i <= retries; i++)
		{
			try
			{
				response = await client.PostAsync(url, content, token);
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				App.Logger.WriteException("HttpClientEx::PostWithRetriesAsync", ex2);
				if (i == retries)
				{
					throw;
				}
			}
		}
		return response;
	}

	public static async Task<T?> GetFromJsonWithRetriesAsync<T>(this HttpClient client, string url, int retries, CancellationToken token) where T : class
	{
		HttpResponseMessage obj = await client.GetWithRetriesAsync(url, retries, token);
		obj.EnsureSuccessStatusCode();
		using Stream stream = await obj.Content.ReadAsStreamAsync(token);
		return await JsonSerializer.DeserializeAsync<T>(stream, (JsonSerializerOptions?)null, token);
	}

	public static async Task<T?> PostFromJsonWithRetriesAsync<T>(this HttpClient client, string url, HttpContent? content, int retries, CancellationToken token) where T : class
	{
		HttpResponseMessage obj = await client.PostWithRetriesAsync(url, content, retries, token);
		obj.EnsureSuccessStatusCode();
		using Stream stream = await obj.Content.ReadAsStreamAsync(token);
		return await JsonSerializer.DeserializeAsync<T>(stream, (JsonSerializerOptions?)null, token);
	}
}
