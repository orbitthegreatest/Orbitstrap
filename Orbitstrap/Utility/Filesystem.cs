using System.IO;

namespace Orbitstrap.Utility;

internal static class Filesystem
{
	internal static long GetFreeDiskSpace(string path)
	{
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			if (path.ToUpperInvariant().StartsWith(driveInfo.Name))
			{
				return driveInfo.AvailableFreeSpace;
			}
		}
		return -1L;
	}

	internal static void AssertReadOnly(string filePath)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		if (fileInfo.Exists && fileInfo.IsReadOnly)
		{
			fileInfo.IsReadOnly = false;
			App.Logger.WriteLine("Filesystem::AssertReadOnly", "The following file was set as read-only: " + filePath);
		}
	}

	internal static void AssertReadOnlyDirectory(string directoryPath)
	{
		FileSystemInfo[] fileSystemInfos = new DirectoryInfo(directoryPath)
		{
			Attributes = FileAttributes.Normal
		}.GetFileSystemInfos("*", SearchOption.AllDirectories);
		for (int i = 0; i < fileSystemInfos.Length; i++)
		{
			fileSystemInfos[i].Attributes = FileAttributes.Normal;
		}
		App.Logger.WriteLine("Filesystem::AssertReadOnlyDirectory", "The following directory was set as read-only: " + directoryPath);
	}
}
