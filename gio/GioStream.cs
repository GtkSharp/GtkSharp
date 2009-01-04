/*
 * GioStream.cs: provide a System.IO.Stream api to [Input|Output]Streams
 *
 * Author(s):
 *	Stephane Delcroix  (stephane@delcroix.org)
 *
 * Copyright (c) 2008 Novell, Inc.
 *
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */
using System;

namespace GLib
{
	public class GioStream : System.IO.Stream
	{
		object stream;
		bool can_read;
		bool can_seek;
		bool can_write;
		bool is_disposed;

		public GioStream (Uri uri, System.IO.FileMode mode)
		{
			throw new NotImplementedException ();
		}

		public GioStream (string filename, System.IO.FileMode mode)
		{
			throw new NotImplementedException ();
		}

		public GioStream (InputStream stream)
		{
			this.stream = stream;
			can_read = true;
			can_seek = stream is Seekable && (stream as Seekable).CanSeek;
		}

		public GioStream (OutputStream stream)
		{
			this.stream = stream;
			can_write = true;
			can_seek = stream is Seekable && (stream as Seekable).CanSeek;
		}

		public override bool CanSeek {
			get { return can_seek; }
		}

		public override bool CanRead {
			get { return can_read; }
		}

		public override bool CanWrite {
			get { return can_write; }
		}

		public override long Length {
			get {
				if (!CanSeek)
					throw new NotSupportedException ("This stream doesn't support seeking");
				if (is_disposed)
					throw new ObjectDisposedException ("The stream is closed");
				
				if (stream is FileInputStream) {
					FileInfo info = (stream as FileInputStream).QueryInfo ("standard::size", null);
					return info.Size;
				}
				if (stream is FileOutputStream) {
					FileInfo info = (stream as FileOutputStream).QueryInfo ("standard::size", null);
					return info.Size;
				}
				throw new NotImplementedException (String.Format ("not implemented for {0} streams", stream.GetType()));
			}
		}

		public override long Position {
			get {
				if (!CanSeek)
					throw new NotSupportedException ("This stream doesn't support seeking");
				if (is_disposed)
					throw new ObjectDisposedException ("The stream is closed");
				return (stream as Seekable).Position;
			}
			set {
				Seek (value, System.IO.SeekOrigin.Begin);
			}
		}

		public override void Flush ()
		{
			if (is_disposed)
				throw new ObjectDisposedException ("The stream is closed");	
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset + count - 1 > buffer.Length)
				throw new ArgumentException ("(offset + count - 1) is greater than the length of buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");
			if (!CanRead)
				throw new NotSupportedException ("The stream does not support reading");
			if (is_disposed)
				throw new ObjectDisposedException ("The stream is closed");
			InputStream input_stream = stream as InputStream;
			if (input_stream == null)
				throw new System.Exception ("this shouldn't happen");

			if (offset == 0)
				return (int)input_stream.Read (buffer, (ulong)count, null);
			else {
				byte[] buf = new byte[count];
				int ret = (int)input_stream.Read (buf, (ulong)count, null);
				buf.CopyTo (buffer, offset);
				return ret;
			}
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset + count  > buffer.Length)
				throw new ArgumentException ("(offset + count) is greater than the length of buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");
			if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
			if (is_disposed)
				throw new ObjectDisposedException ("The stream is closed");
			OutputStream output_stream = stream as OutputStream;
			if (output_stream == null)
				throw new System.Exception ("this shouldn't happen");
			if (offset == 0) {
				output_stream.Write (buffer, (ulong)count, null);
				return;
			} else {
				byte[] buf = new byte[count];
				Array.Copy (buffer, offset, buf, 0, count);
				output_stream.Write (buf, (ulong)count, null);
				return;
			}
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException ("This stream doesn't support seeking");
			if (is_disposed)
				throw new ObjectDisposedException ("The stream is closed");
			Seekable seekable = stream as Seekable;

			SeekType seek_type;
			switch (origin) {
			case System.IO.SeekOrigin.Current:
				seek_type = SeekType.Cur;
				break;
			case System.IO.SeekOrigin.End:
				seek_type = SeekType.End;
				break;
			case System.IO.SeekOrigin.Begin:
			default:
				seek_type = SeekType.Set;
				break;
			}
			seekable.Seek (offset, seek_type, null);
			return Position;
		}

		public override void SetLength (long value)
		{
			if (!CanSeek || !CanWrite)
				throw new NotSupportedException ("This stream doesn't support seeking");
			if (is_disposed)
				throw new ObjectDisposedException ("The stream is closed");
			throw new NotImplementedException ();
		}

		public override void Close ()
		{
			if (stream is InputStream)
				(stream as InputStream).Close (null);
			if (stream is OutputStream)
				(stream as OutputStream).Close (null);
			is_disposed = true;
		}
	}
}
