using System.Web.Mvc;
using FamilyTree.Models.FileSystem.AzureStorage;
using FamilyTree.Models.FileSystem.LocalDevice;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem(Controller controller)
		{
			var localDevice = new LocalDeviceFileSystem(s => controller.Server.MapPath(s));
			return new RelayingFileSystem(
				localDevice,
				new AzureStorageFileSystem());
		}
	}
}