using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orbitstrap.Utility;

internal static class Http
{
	public static async Task<T> GetJson<T>(string url)
	{
		HttpResponseMessage obj = await App.HttpClient.GetAsync(url);
		obj.EnsureSuccessStatusCode();
		return JsonSerializer.Deserialize<T>(await obj.Content.ReadAsStringAsync());
	}
}
