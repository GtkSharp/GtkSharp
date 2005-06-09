//  VfsStream.cs - System.IO.Stream wrapper around gnome-vfs.
//
//  Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
//  Copyright (c) 2004 Jeroen Zwartepoorte
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

using GLib;
using System;
using System.IO;

namespace Gnome.Vfs {
	class VfsStreamAsync {
		private Handle handle;
		private byte[] buffer;
		private int count;
		private int offset;
		private int bytesRemaining;
		private System.AsyncCallback cback;
		private object state;
		private VfsStreamAsyncResult asyncResult;
		
		public VfsStreamAsync (Handle handle, byte[] buffer, int offset,
				       int count, System.AsyncCallback cback, object state)
		{
			this.handle = handle;
			this.buffer = buffer;
			this.offset = offset;
			this.count = count;
			this.cback = cback;
			bytesRemaining = count;
		}
		
		public VfsStreamAsyncResult BeginRead ()
		{
			asyncResult = new VfsStreamAsyncResult (state);
			Async.Read (handle, out buffer[offset], (uint)count, new AsyncReadCallback (AsyncRead));
			return asyncResult;
		}
		
		public VfsStreamAsyncResult BeginWrite ()
		{
			asyncResult = new VfsStreamAsyncResult (state);
			Async.Write (handle, out buffer[offset], (uint)count, new AsyncWriteCallback (AsyncWrite));
			return asyncResult;
		}
		
		private void AsyncRead (Handle handle, Result result, byte[] buf,
					ulong bytesRequested, ulong bytesRead)
		{
			if (result == Result.Ok) {
				Array.Copy (buf, 0, buffer, offset + count - bytesRemaining, (int)bytesRead);
				bytesRemaining -= (int)bytesRead;
				if (bytesRemaining > 0) {
					buf = new byte[bytesRemaining];
					Async.Read (handle, out buf[0], (uint)bytesRemaining,
						    new AsyncReadCallback (AsyncRead));
				} else if (cback != null) {
					asyncResult.SetComplete (null, count);
					cback (asyncResult);
				}
			} else if (result == Result.ErrorEof) {
				Array.Copy (buf, 0, buffer, offset + count - bytesRemaining, (int)bytesRead);
				bytesRemaining -= (int)bytesRead;
				asyncResult.SetComplete (null, count - bytesRemaining);
				
				if (cback != null)
					cback (asyncResult);
			} else if (cback != null) {
				Exception e = new IOException (Vfs.ResultToString (result));
				asyncResult.SetComplete (e, -1);
				cback (asyncResult);
			}
		}
		
		private void AsyncWrite (Handle handle, Result result, byte[] buffer,
					 ulong bytesRequested, ulong bytesWritten)
		{
			if (result == Result.Ok) {
				bytesRemaining -= (int)bytesWritten;
				if (bytesRemaining > 0) {
					Async.Write (handle, out buffer[offset + count - bytesRemaining],
						     (uint)bytesRemaining, new AsyncWriteCallback (AsyncWrite));
				} else if (cback != null) {
					asyncResult.SetComplete (null, count);
					cback (asyncResult);
				}
			} else if (result == Result.ErrorEof) {
				bytesRemaining -= (int)bytesWritten;
				asyncResult.SetComplete (null, count - bytesRemaining);
				
				if (cback != null)
					cback (asyncResult);
			} else if (cback != null) {
				Exception e = new IOException (Vfs.ResultToString (result));
				asyncResult.SetComplete (e, -1);
				cback (asyncResult);
			}
		}
	}

	public class VfsStream : System.IO.Stream {
		private Gnome.Vfs.Uri uri;
		private Handle handle;
		private FileMode mode;
		private FileAccess access;
		private bool async;
		private bool canseek;
		
		// Async state variables.
		private AsyncCallback callback;
		private AsyncReadCallback readCallback;
		private AsyncWriteCallback writeCallback;
		private bool asyncCompleted = false;
		private Result asyncResult;
		private ulong asyncBytesRead;
		private ulong asyncBytesWritten;

		public VfsStream (string uri, FileMode mode)
			: this (uri, mode, false) { }
		
		public VfsStream (string text_uri, FileMode mode, bool async)
		{
			if (text_uri == null)
				throw new ArgumentNullException ("uri");
				
			if (text_uri == "")
				throw new ArgumentNullException ("Uri is empty");
			
			if (mode < FileMode.CreateNew || mode > FileMode.Append)
				throw new ArgumentOutOfRangeException ("mode");

			if (text_uri.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Uri has invalid chars");
			
			uri = new Gnome.Vfs.Uri (text_uri);

			if (mode == FileMode.Open && !uri.Exists)
				throw new FileNotFoundException ("Could not find uri \"" + text_uri + "\".");

			if (mode == FileMode.CreateNew) {
				string dname = uri.ExtractDirname ();
				if (dname != "" && !new Uri (dname).Exists)
					throw new DirectoryNotFoundException ("Could not find a part of " +
									      "the path \"" + dname + "\".");
			}
			
			if (async) {
				callback = new AsyncCallback (OnAsyncCallback);
				readCallback = new AsyncReadCallback (OnAsyncReadCallback);
				writeCallback = new AsyncWriteCallback (OnAsyncWriteCallback);
			}
			
			OpenMode om = OpenMode.None;
			switch (mode) {
				case FileMode.CreateNew:
				case FileMode.Create:
				case FileMode.Truncate:
				case FileMode.Append:
					om = OpenMode.Write;
					access = FileAccess.Write;
					break;
				case FileMode.OpenOrCreate:
					if (uri.Exists) {
						om = OpenMode.Read;
						access = FileAccess.Read;
					} else {
						om = OpenMode.Write;
						access = FileAccess.Write;
					}
					break;
				case FileMode.Open:
					om = OpenMode.Read;
					access = FileAccess.Read;
					break;
			}
			
			/* 644 */
			FilePermissions perms = FilePermissions.UserRead |
						FilePermissions.UserWrite |
						FilePermissions.GroupRead |
						FilePermissions.OtherRead;
			
			Result result;
			handle = null;
			switch (mode) {
				case FileMode.Append:
					if (uri.Exists) {
						if (async) {
							handle = Async.Open (uri, om,
									     (int)Async.Priority.Default,
									     callback);
							Wait ();
							Async.Seek (handle, SeekPosition.End, 0, callback);
							Wait ();
						} else {
							handle = Sync.Open (uri, om);
							result = Sync.Seek (handle, SeekPosition.End, 0);
							Vfs.ThrowException (uri, result);
						}
					} else {
						if (async) {
							handle = Async.Create (uri, om, true, perms,
									       (int)Async.Priority.Default,
									       callback);
							Wait ();
						} else {
							handle = Sync.Create (uri, om, true, perms);
						}
					}
					break;
				case FileMode.Create:
					if (uri.Exists) {
						if (async) {
							handle =  Async.Open (uri, om,
									      (int)Async.Priority.Default,
									      callback);
							Wait ();
						} else {
							handle = Sync.Open (uri, om);
							result = uri.Truncate (0);
							Vfs.ThrowException (uri, result);
						}
					} else {
						handle = Sync.Create (uri, om, true, perms);
					}
					break;
				case FileMode.CreateNew:
					if (uri.Exists) {
						throw new IOException ("Uri \"" + text_uri + "\" already exists.");
					} else {
						if (async) {
							handle = Async.Create (uri, om, true, perms,
									       (int)Async.Priority.Default,
									       callback);
							Wait ();
						} else {
							handle = Sync.Create (uri, om, true, perms);
						}
					}
					break;
				case FileMode.Open:
					if (uri.Exists) {
						if (async) {
							handle = Async.Open (uri, om,
									     (int)Async.Priority.Default,
									     callback);
							Wait ();
						} else {
							handle = Sync.Open (uri, om);
						}
					} else {
						throw new FileNotFoundException (text_uri);
					}
					break;
				case FileMode.OpenOrCreate:
					if (uri.Exists) {
						if (async) {
							handle = Async.Open (uri, om,
									     (int)Async.Priority.Default,
									     callback);
							Wait ();
						} else {
							handle = Sync.Open (uri, om);
						}
					} else {
						if (async) {
							handle = Async.Create (uri, om, true, perms,
									       (int)Async.Priority.Default,
									       callback);
							Wait ();
						} else {
							handle = Sync.Create (uri, om, true, perms);
						}
					}
					break;
				case FileMode.Truncate:
					if (uri.Exists) {
						result = uri.Truncate (0);
						if (async) {
							handle = Async.Open (uri, om,
									     (int)Async.Priority.Default,
									     callback);
							Wait ();
						} else {
							handle = Sync.Open (uri, om);
							Vfs.ThrowException (uri, result);
						}
					} else {
						throw new FileNotFoundException (text_uri);
					}
					break;
			}
			
			this.mode = mode;
			this.canseek = true;
			this.async = async;
		}

		public override bool CanRead {
			get {
				return access == FileAccess.Read ||
				       access == FileAccess.ReadWrite;
			}
		}

                public override bool CanWrite {
                        get {
				return access == FileAccess.Write ||
				       access == FileAccess.ReadWrite;
                        }
                }
		
		public override bool CanSeek {
                        get {
                                return canseek;
                        }
                }

		public virtual bool IsAsync {
			get {
				return async;
			}
		}

		public string Uri {
			get {
				return uri.ToString (); 
			}
		}

		public override long Length {
			get {
				FileInfo info = uri.GetFileInfo ();
				return info.Size;
			}
		}

		public override long Position {
			get {
				if (IsAsync)
					throw new NotSupportedException ("Cannot tell what the offset is in async mode");
				ulong pos;
				Result result = Sync.Tell (handle, out pos);
				Vfs.ThrowException (Uri, result);
				return (long)pos;
			}
			set {
				Seek (value, SeekOrigin.Begin);
			}
		}

		public override int ReadByte ()
		{
			if (!CanRead)
				throw new NotSupportedException ("The stream does not support reading");
			
			byte[] buffer = new byte[1];
			ulong bytesRead;
			Result result;
			if (async) {
				Async.Read (handle, out buffer[0], 1, readCallback);
				Wait ();
				result = asyncResult;
			} else {
				result = Sync.Read (handle, out buffer[0], 1UL, out bytesRead);
			}
			if (result == Result.ErrorEof)
				return -1;
				
			Vfs.ThrowException (Uri, result);
			return buffer[0];
		}

		public override void WriteByte (byte value)
		{
			if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
			
			byte[] buffer = new byte[1];
			buffer[0] = value;
			ulong bytesWritten;
			Result result;
			if (async) {
				Async.Write (handle, out buffer[0], 1, writeCallback);
				Wait ();
				result = asyncResult;
			} else {
				result = Sync.Write (handle, out buffer[0],  1UL, out bytesWritten);
			}
			Vfs.ThrowException (Uri, result);
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			else if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");
			else if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");
			else if (count > buffer.Length - offset)
				throw new ArgumentException ("Buffer too small, count/offset wrong");
			else if (!CanRead)
				throw new NotSupportedException ("The stream does not support reading");
			
			ulong bytesRead;
			Result result;
			if (async) {
				Async.Read (handle, out buffer[offset], (uint)count, readCallback);
				Wait ();
				result = asyncResult;
				bytesRead = asyncBytesRead;
			} else {
				result = Sync.Read (handle, out buffer[offset], (ulong)count, out bytesRead);
			}
			if (result == Result.ErrorEof)
				return 0;

			Vfs.ThrowException (Uri, result);
			return (int)bytesRead;
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count,
							System.AsyncCallback cback, object state)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			else if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");
			else if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");
			else if (count > buffer.Length - offset)
				throw new ArgumentException ("Buffer too small, count/offset wrong");
			else if (!CanRead)
				throw new NotSupportedException ("The stream does not support reading");

			if (!IsAsync)
				return base.BeginRead (buffer, offset, count, cback, state);

			VfsStreamAsync async = new VfsStreamAsync (handle, buffer, offset, count, cback, state);
			return async.BeginRead ();
		}

		public override int EndRead (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");

			if (!IsAsync)
				base.EndRead (result);

			if (!(result is VfsStreamAsyncResult))
				throw new ArgumentException ("Invalid IAsyncResult object", "result");

			VfsStreamAsyncResult asyncResult = (VfsStreamAsyncResult)result;
			if (asyncResult.Done)
				throw new InvalidOperationException ("EndRead already called");
			asyncResult.Done = true;
			
			while (!asyncResult.IsCompleted)
				MainContext.Iteration ();
			
			if (asyncResult.Exception != null)
				throw asyncResult.Exception;
			
			return asyncResult.NBytes;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			else if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");
			else if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");
			else if (count > buffer.Length - offset)
				throw new ArgumentException ("Buffer too small, count/offset wrong");
			else if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");

			ulong bytesWritten;
			Result result;
			if (async) {
				Async.Write (handle, out buffer[offset], (uint)count, writeCallback);
				Wait ();
				result = asyncResult;
				bytesWritten = asyncBytesWritten;
			} else {
				result = Sync.Write (handle, out buffer[offset], (ulong)count, out bytesWritten);
			}
			Vfs.ThrowException (Uri, result);
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							 System.AsyncCallback cback, object state)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			else if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");
			else if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");
			else if (count > buffer.Length - offset)
				throw new ArgumentException ("Buffer too small, count/offset wrong");
			else if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");

			if (!IsAsync)
				return base.BeginRead (buffer, offset, count, cback, state);

			VfsStreamAsync async = new VfsStreamAsync (handle, buffer, offset, count, cback, state);
			return async.BeginWrite ();
		}

		public override void EndWrite (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");

			if (!IsAsync)
				base.EndWrite (result);

			if (!(result is VfsStreamAsyncResult))
				throw new ArgumentException ("Invalid IAsyncResult object", "result");

			VfsStreamAsyncResult asyncResult = (VfsStreamAsyncResult)result;
			if (asyncResult.Done)
				throw new InvalidOperationException ("EndWrite already called");
			asyncResult.Done = true;
			
			while (!asyncResult.IsCompleted)
				MainContext.Iteration ();
			
			if (asyncResult.Exception != null)
				throw asyncResult.Exception;
		}
		
		public override long Seek (long offset, SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException ("The stream does not support seeking");
			if (IsAsync && origin == SeekOrigin.Current)
				throw new NotSupportedException ("Cannot tell what the offset is in async mode");

			SeekPosition seekPos = SeekPosition.Start;
			long newOffset = -1;
			switch (origin) {
				case SeekOrigin.Begin:
					seekPos = SeekPosition.Start;
					newOffset = offset;
					break;
				case SeekOrigin.Current:
					seekPos = SeekPosition.Current;
					break;
				case SeekOrigin.End:
					seekPos = SeekPosition.End;
					newOffset = Length + offset;
					break;
			}

			Result result;
			if (async) {
				Async.Seek (handle, seekPos, offset, callback);
				Wait ();
				result = asyncResult;
			} else {
				result = Sync.Seek (handle, seekPos, offset);
			}
			Vfs.ThrowException (Uri, result);
			return newOffset;
		}

		public override void SetLength (long length)
		{
			if (!CanSeek)
				throw new NotSupportedException ("The stream does not support seeking");
			else if (!CanWrite)
				throw new NotSupportedException ("The stream does not support writing");
			
			Result result = Sync.Truncate (handle, (ulong)length);
			Vfs.ThrowException (Uri, result);
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
			Result result = Sync.Close (handle);
			Vfs.ThrowException (Uri, result);
		}
		
		private void OnAsyncCallback (Handle handle, Result result)
		{
			asyncResult = result;
			asyncCompleted = true;
		}
		
		private void OnAsyncReadCallback (Handle handle, Result result,
						  byte[] buffer, ulong bytes_requested,
						  ulong bytes_read)
		{
			asyncResult = result;
			asyncBytesRead = bytes_read;
			asyncCompleted = true;
		}
		
		private void OnAsyncWriteCallback (Handle handle, Result result,
						   byte[] buffer, ulong bytes_requested,
						   ulong bytes_written)
		{
			asyncResult = result;
			asyncBytesWritten = bytes_written;
			asyncCompleted = true;
		}
		
		private void Wait ()
		{
			while (!asyncCompleted)
				MainContext.Iteration ();
			asyncCompleted = false;
		}
	}
}
