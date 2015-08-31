using Newtonsoft.Json;
using System;
using System.IO;

namespace FamilyTree.Models.FileSystem
{
	public class FileJsonConverter : JsonConverter
	{
		private readonly IFileSystem _fileSystem;

		public FileJsonConverter(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IFile);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var intermediary = serializer.Deserialize<_DeserialisableFile>(reader);

			return new File(
				intermediary.Name,
				intermediary.Directory,
				intermediary.Size,
				intermediary.LastWriteTimeUtc,
				_fileSystem);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotSupportedException("Only supports deserialise");
		}

		private class _DeserialisableFile : IFile
		{
			public IDirectory Directory { get; set; }
			public DateTime LastWriteTimeUtc { get; set; }
			public string Name { get; set; }
			public long Size { get; set; }

			public Stream OpenRead()
			{
				throw new NotSupportedException();
			}

			public Stream OpenWrite()
			{
				throw new NotSupportedException();
			}
		}
	}
}