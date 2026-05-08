using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Orbitstrap.Utility;

public static class MD5Hash
{
	public static string FromBytes(byte[] data)
	{
		using MD5 mD = MD5.Create();
		return Stringify(mD.ComputeHash(data));
	}

	public static string FromStream(Stream stream)
	{
		stream.Seek(0L, SeekOrigin.Begin);
		using MD5 mD = MD5.Create();
		return Stringify(mD.ComputeHash(stream));
	}

	public static string FromFile(string filename)
	{
		using (MD5.Create())
		{
			using FileStream stream = File.OpenRead(filename);
			return FromStream(stream);
		}
	}

	public static string FromString(string str)
	{
		return FromBytes(Encoding.UTF8.GetBytes(str));
	}

	public static string Stringify(byte[] hash)
	{
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}
}
