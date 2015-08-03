using System;
using System.IO;
using System.Linq;

namespace FamilyTree.Models.FileSystem
{
	public class MulticastStream : Stream
	{
		private readonly Stream[] _streams;

		public MulticastStream(params Stream[] streams)
		{
			_streams = streams;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			foreach (var stream in _streams)
				stream.Write(buffer, offset, count);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long length)
		{
			foreach (var stream in _streams)
				stream.SetLength(length);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			foreach (var stream in _streams)
				stream.Flush();
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return _streams.All(s => s.CanWrite); }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override void Close()
		{
			foreach (var stream in _streams)
				stream.Close();
		}

		protected override void Dispose(bool disposing)
		{
			foreach (var stream in _streams)
				stream.Dispose();
		}
	}
}