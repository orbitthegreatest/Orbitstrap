using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orbitstrap.Exceptions;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Models.RobloxApi;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.Entities;

public class UserDetails
{
	private static List<UserDetails> _cache { get; set; } = new List<UserDetails>();

	public GetUserResponse Data { get; set; }

	public ThumbnailResponse Thumbnail { get; set; }

	public static async Task<UserDetails> Fetch(long id)
	{
		IEnumerable<UserDetails> source = _cache.Where(delegate(UserDetails x)
		{
			GetUserResponse data = x.Data;
			return data != null && data.Id == id;
		});
		if (source.Any())
		{
			return source.FirstOrDefault();
		}
		GetUserResponse userResponse = await Http.GetJson<GetUserResponse>($"https://users.roblox.com/v1/users/{id}");
		if (userResponse == null)
		{
			throw new InvalidHTTPResponseException("Roblox API for User Details returned invalid data");
		}
		ApiArrayResponse<ThumbnailResponse> apiArrayResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={id}&size=180x180&format=Png&isCircular=false");
		if (apiArrayResponse == null || !apiArrayResponse.Data.Any())
		{
			throw new InvalidHTTPResponseException("Roblox API for Thumbnails returned invalid data");
		}
		UserDetails userDetails = new UserDetails
		{
			Data = userResponse,
			Thumbnail = apiArrayResponse.Data.FirstOrDefault()
		};
		_cache.Add(userDetails);
		return userDetails;
	}
}
