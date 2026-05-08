using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orbitstrap.Exceptions;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.Entities;

public class UniverseDetails
{
	private static List<UniverseDetails> _cache { get; set; } = new List<UniverseDetails>();

	public GameDetailResponse Data { get; set; }

	public ThumbnailResponse Thumbnail { get; set; }

	public static UniverseDetails? LoadFromCache(long id)
	{
		IEnumerable<UniverseDetails> source = _cache.Where(delegate(UniverseDetails x)
		{
			GameDetailResponse data = x.Data;
			return data != null && data.Id == id;
		});
		if (source.Any())
		{
			return source.FirstOrDefault();
		}
		return null;
	}

	public static Task FetchSingle(long id)
	{
		return FetchBulk(id.ToString());
	}

	public static async Task FetchBulk(string ids)
	{
		ApiArrayResponse<GameDetailResponse> gameDetailResponse = await Http.GetJson<ApiArrayResponse<GameDetailResponse>>("https://games.roblox.com/v1/games?universeIds=" + ids);
		if (!gameDetailResponse.Data.Any())
		{
			throw new InvalidHTTPResponseException("Roblox API for Game Details returned invalid data");
		}
		ApiArrayResponse<ThumbnailResponse> apiArrayResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>("https://thumbnails.roblox.com/v1/games/icons?universeIds=" + ids + "&returnPolicy=PlaceHolder&size=128x128&format=Png&isCircular=false");
		if (!apiArrayResponse.Data.Any())
		{
			throw new InvalidHTTPResponseException("Roblox API for Game Thumbnails returned invalid data");
		}
		string[] array = ids.Split(',');
		foreach (string s in array)
		{
			long id = long.Parse(s);
			_cache.Add(new UniverseDetails
			{
				Data = gameDetailResponse.Data.Where((GameDetailResponse x) => x.Id == id).FirstOrDefault(),
				Thumbnail = apiArrayResponse.Data.Where((ThumbnailResponse x) => x.TargetId == id).FirstOrDefault()
			});
		}
	}
}
