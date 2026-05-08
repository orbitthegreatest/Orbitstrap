using System.IO;
using System.Linq;

namespace Orbitstrap.Utility;

internal static class PathValidator
{
	public enum ValidationResult
	{
		Ok,
		IllegalCharacter,
		ReservedFileName,
		ReservedDirectoryName
	}

	private static readonly string[] _reservedNames = new string[22]
	{
		"CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6",
		"COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
		"LPT8", "LPT9"
	};

	private static readonly char[] _directorySeperatorDelimiters = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private static readonly char[] _invalidPathChars = GetInvalidPathChars();

	public static char[] GetInvalidPathChars()
	{
		char[] array = new char[9] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
		char[] invalidPathChars = Path.GetInvalidPathChars();
		char[] array2 = new char[array.Length + invalidPathChars.Length];
		array.CopyTo(array2, 0);
		invalidPathChars.CopyTo(array2, array.Length);
		return array2;
	}

	public static ValidationResult IsFileNameValid(string fileName)
	{
		if (fileName.IndexOfAny(_invalidPathChars) != -1)
		{
			return ValidationResult.IllegalCharacter;
		}
		string value = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
		if (_reservedNames.Contains(value))
		{
			return ValidationResult.ReservedFileName;
		}
		return ValidationResult.Ok;
	}

	public static ValidationResult IsPathValid(string path)
	{
		string pathRoot = Path.GetPathRoot(path);
		string text;
		if (pathRoot == null)
		{
			text = path;
		}
		else
		{
			int length = pathRoot.Length;
			text = path.Substring(length, path.Length - length);
		}
		string[] array = text.Split(_directorySeperatorDelimiters);
		foreach (string text2 in array)
		{
			if (text2.IndexOfAny(_invalidPathChars) != -1)
			{
				return ValidationResult.IllegalCharacter;
			}
			if (_reservedNames.Contains(text2))
			{
				return ValidationResult.ReservedDirectoryName;
			}
		}
		if (Path.GetFileName(path).IndexOfAny(_invalidPathChars) != -1)
		{
			return ValidationResult.IllegalCharacter;
		}
		string value = Path.GetFileNameWithoutExtension(path).ToUpperInvariant();
		if (_reservedNames.Contains(value))
		{
			return ValidationResult.ReservedFileName;
		}
		return ValidationResult.Ok;
	}
}
