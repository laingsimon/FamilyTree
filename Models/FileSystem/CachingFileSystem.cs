using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace FamilyTree.Models.FileSystem
{
	public class CachingFileSystem : IFileSystem
	{
		private readonly Cache _cache;

		public static bool ShouldRefreshCache(HttpContext current)
		{
			if (current == null)
				return false;

			var headers = current.Request.Headers;
			return headers["Cache-Control"] == "no-cache"
				|| headers["max-age"] == "0";
		}

		private readonly IFileSystem _fileSystem;
		private readonly bool _refreshCache;
		private readonly HashSet<string> _recached = new HashSet<string>();

		public CachingFileSystem(IFileSystem fileSystem, Cache cache, bool refreshCache)
		{
			_fileSystem = fileSystem;
			_cache = cache;
			_refreshCache = refreshCache;
		}

		public IFile CreateFile(string path)
		{
			return _fileSystem.CreateFile(path);
		}

		public bool FileExists(string path)
		{
			var exists = _GetOrAdd(string.Format("FileExists:{0}", path), () => new _FileExists(_fileSystem.FileExists(path)));
			return exists.Exists;
		}

        public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			return _GetOrAdd(string.Format("GetDirectories:{0}", directory.Name), () => _fileSystem.GetDirectories(directory).Select(_InterceptedDirectory).ToArray());
		}

		public IDirectory GetDirectory(string path)
		{
			return _GetOrAdd(string.Format("GetDirectory:{0}", path), () => _InterceptedDirectory(_fileSystem.GetDirectory(path)));
		}

		public IFile GetFile(string path)
		{
			return _GetOrAdd(string.Format("GetFile:{0}", path), () => _InterceptedFile(_fileSystem.GetFile(path)));
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			return _GetOrAdd(string.Format("GetFiles:{0}/{1}", directory.Name, searchPattern), () => _fileSystem.GetFiles(directory, searchPattern).Select(_InterceptedFile).ToArray());
		}

		public Stream OpenRead(IFile file)
		{
			var repeatableReadStream = _GetOrAdd(string.Format("OpenRead:{0}", file.Name), () => new _RepeatableReadStream(_fileSystem.OpenRead(file)));
			return repeatableReadStream.NewStream();
		}

		public Stream OpenWrite(IFile file)
		{
			return _fileSystem.OpenWrite(file);
		}

		private IFile _InterceptedFile(IFile file)
		{
			if (file == File.Null || file == null)
				return File.Null;

			return new InterceptedFile(file, this);
		}

		private IDirectory _InterceptedDirectory(IDirectory directory)
		{
			if (directory == null || directory == Directory.Null)
				return Directory.Null;

			return new InterceptedDirectory(directory, this);
		}

		private T _GetOrAdd<T>(string key, Func<T> getValue)
			where T : class
		{
			var cachedValue = _cache.Get(key);
			if (cachedValue != null && !_ShouldBeRecache(key))
				return (T)cachedValue;

			Trace.TraceInformation("Getting value for {0}", key);

			var newValue = getValue();
			_recached.Add(key);
			_cache.Insert(key, newValue, null, DateTime.Now.AddHours(1), Cache.NoSlidingExpiration);
			return newValue;
		}

		private bool _ShouldBeRecache(string key)
		{
			if (!_refreshCache)
				return false;

			return !_recached.Contains(key);
		}

        private class _FileExists
		{
			private readonly bool _value;

			public _FileExists(bool value)
			{
				_value = value;
			}

			public bool Exists
			{
				[DebuggerStepThrough]
				get { return _value; }
			}
		}

		private class _RepeatableReadStream
		{
			private readonly byte[] _data;

			public _RepeatableReadStream(Stream coreStream)
			{
				_data = _ReadToEnd(coreStream).ToArray();
			}

			private static byte[] _ReadToEnd(Stream stream)
			{
				var buffer = new byte[stream.Length];
				var offset = 0;
				int read;

				while ((read = stream.Read(buffer, offset, Math.Min(1024, buffer.Length - offset))) > 0)
					offset += read;

				return buffer;
			}

			public Stream NewStream()
			{
				return new MemoryStream(_data);
			}
		}
	}
}