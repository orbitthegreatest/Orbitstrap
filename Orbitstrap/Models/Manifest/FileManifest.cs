using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orbitstrap.RobloxInterfaces;

namespace Orbitstrap.Models.Manifest;

public class FileManifest : List<ManifestFile>
{
	private FileManifest(string data)
	{
		using StringReader stringReader = new StringReader(data);
		while (true)
		{
			string text = stringReader.ReadLine();
			string text2 = stringReader.ReadLine();
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
			{
				break;
			}
			Add(new ManifestFile
			{
				Name = text,
				Signature = text2
			});
		}
	}

	public static async Task<FileManifest> Get(string versionGuid)
	{
		string location = Deployment.GetLocation("/" + versionGuid + "-rbxManifest.txt");
		return new FileManifest(await App.HttpClient.GetStringAsync(location));
	}
}
