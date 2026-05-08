using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Orbitstrap.Exceptions;
using Orbitstrap.Extensions;
using Orbitstrap.Models.APIs.Roblox;

namespace Orbitstrap.Utility;

internal static class Thumbnails
{
	public static async Task<string?[]> GetThumbnailUrlsAsync(List<ThumbnailRequest> requests, CancellationToken token)
	{
		string?[] urls = new string[requests.Count];
		for (int i = 0; i < requests.Count; i++)
		{
			requests[i].RequestId = i.ToString();
		}
		StringContent payload = new StringContent(JsonSerializer.Serialize(requests));
		ThumbnailResponse[] response = null;
		for (int j = 1; j <= 5; j++)
		{
			ThumbnailBatchResponse thumbnailBatchResponse = await App.HttpClient.PostFromJsonWithRetriesAsync<ThumbnailBatchResponse>("https://thumbnails.roblox.com/v1/batch", payload, 3, token);
			if (thumbnailBatchResponse == null)
			{
				throw new InvalidHTTPResponseException("Deserialised ThumbnailBatchResponse is null");
			}
			response = thumbnailBatchResponse.Data;
			if (response.All((ThumbnailResponse x) => x.State != "Pending"))
			{
				break;
			}
			if (j == 5)
			{
				App.Logger.WriteLine("Thumbnails::GetThumbnailUrlsAsync", "Ran out of retries");
			}
			else
			{
				await Task.Delay(500 * j, token);
			}
		}
		ThumbnailResponse[] array = response;
		foreach (ThumbnailResponse thumbnailResponse in array)
		{
			if (thumbnailResponse.State == "Pending")
			{
				App.Logger.WriteLine("Thumbnails::GetThumbnailUrlsAsync", $"{thumbnailResponse.TargetId} is still pending");
			}
			else if (thumbnailResponse.State == "Error")
			{
				App.Logger.WriteLine("Thumbnails::GetThumbnailUrlsAsync", $"{thumbnailResponse.TargetId} got error code {thumbnailResponse.ErrorCode} ({thumbnailResponse.ErrorMessage})");
			}
			else if (thumbnailResponse.State != "Completed")
			{
				App.Logger.WriteLine("Thumbnails::GetThumbnailUrlsAsync", $"{thumbnailResponse.TargetId} got \"{thumbnailResponse.State}\"");
			}
			urls[int.Parse(thumbnailResponse.RequestId)] = thumbnailResponse.ImageUrl;
		}
		return urls;
	}

	public static async Task<string?> GetThumbnailUrlAsync(ThumbnailRequest request, CancellationToken token)
	{
		request.RequestId = "0";
		StringContent payload = new StringContent(JsonSerializer.Serialize(new ThumbnailRequest[1] { request }));
		ThumbnailResponse response = null;
		for (int i = 1; i <= 5; i++)
		{
			ThumbnailBatchResponse thumbnailBatchResponse = await App.HttpClient.PostFromJsonWithRetriesAsync<ThumbnailBatchResponse>("https://thumbnails.roblox.com/v1/batch", payload, 3, token);
			if (thumbnailBatchResponse == null)
			{
				throw new InvalidHTTPResponseException("Deserialised ThumbnailBatchResponse is null");
			}
			response = thumbnailBatchResponse.Data[0];
			if (response.State != "Pending")
			{
				break;
			}
			if (i == 5)
			{
				App.Logger.WriteLine("Thumbnails::GetThumbnailUrlAsync", "Ran out of retries");
			}
			else
			{
				await Task.Delay(500 * i, token);
			}
		}
		if (response.State == "Pending")
		{
			App.Logger.WriteLine("Thumbnails::GetThumbnailUrlAsync", $"{response.TargetId} is still pending");
		}
		else if (response.State == "Error")
		{
			App.Logger.WriteLine("Thumbnails::GetThumbnailUrlAsync", $"{response.TargetId} got error code {response.ErrorCode} ({response.ErrorMessage})");
		}
		else if (response.State != "Completed")
		{
			App.Logger.WriteLine("Thumbnails::GetThumbnailUrlAsync", $"{response.TargetId} got \"{response.State}\"");
		}
		return response.ImageUrl;
	}
}
