using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class FileWriteStream : Stream
	{
		private readonly ICloudBlob _blobRef;
		private readonly MemoryStream _memoryStream = new MemoryStream();

		public FileWriteStream(ICloudBlob blobRef)
		{
			_blobRef = blobRef;
		}

		#region read members
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}

		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			throw new NotSupportedException();
		}

		public override int ReadByte()
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
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

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}
		#endregion

		#region write members
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _memoryStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			_memoryStream.EndWrite(asyncResult);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_memoryStream.Write(buffer, offset, count);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return _memoryStream.WriteAsync(buffer, offset, count, cancellationToken);
		}

		public override void WriteByte(byte value)
		{
			_memoryStream.WriteByte(value);
		}

		public override int WriteTimeout
		{
			get { return _memoryStream.WriteTimeout; }
			set { _memoryStream.WriteTimeout = value; }
		}

		public override void SetLength(long value)
		{
			_memoryStream.SetLength(value);
		}
		#endregion

		#region other members
		public override bool CanTimeout
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override void Flush()
		{ }

		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			_memoryStream.Position = 0;
			_blobRef.UploadFromStream(_memoryStream);
		}
	}
}