using System;
using System.Collections.Generic;
using System.IO;

namespace Orbitstrap.Models.Manifest;

public class PackageManifest : List<Package>
{
	public PackageManifest(string data)
	{
		using StringReader stringReader = new StringReader(data);
		string text = stringReader.ReadLine();
		if (text != "v0")
		{
			throw new NotSupportedException("Unexpected package manifest version: " + text + " (expected v0!)");
		}
		while (true)
		{
			string text2 = stringReader.ReadLine();
			string text3 = stringReader.ReadLine();
			string text4 = stringReader.ReadLine();
			string text5 = stringReader.ReadLine();
			if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4) || string.IsNullOrEmpty(text5) || text2 == "RobloxPlayerLauncher.exe")
			{
				break;
			}
			int packedSize = int.Parse(text4);
			int size = int.Parse(text5);
			Add(new Package
			{
				Name = text2,
				Signature = text3,
				PackedSize = packedSize,
				Size = size
			});
		}
	}
}
