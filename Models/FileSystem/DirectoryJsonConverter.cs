using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FamilyTree.Models.FileSystem
{
	public class DirectoryJsonConverter : JsonConverter
	{
		private readonly IFileSystem _fileSystem;

		public DirectoryJsonConverter(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IDirectory);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var intermediary = serializer.Deserialize<_DeserialisableDirectory>(reader);

			return new Directory(
				intermediary.Name,
				intermediary.Parent,
				_fileSystem);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotSupportedException("Only supports deserialise");
		}

		private class _DeserialisableDirectory : IDirectory
		{
			public string Name { get; set; }
			public IDirectory Parent { get; set; }
			
			public IEnumerable<IDirectory> GetDirectories()
			{
				throw new NotSupportedException();
			}

			public IEnumerable<IFile> GetFiles(string searchPattern)
			{
				throw new NotSupportedException();
			}
		}
	}
}