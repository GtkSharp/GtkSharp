// Copyright (c) 2011 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Cairo
{

	/// <summary>
	/// Mirrors <c>cairo_device_type_t</c>; the values are what
	/// <c>cairo_device_get_type</c> returns.
	/// </summary>
	public enum DeviceType {
		Invalid = -1,
		Drm = 0,
		GL,
		Script,
		Xcb,
		Xlib,
		Xml,
		Cogl,
		Win32,
	}

	public class Device : IDisposable
	{

		IntPtr handle;

		internal Device (IntPtr handle) : this (handle, false)
		{
		}

		/// <param name="owner">
		/// True when the caller already holds a reference that this wrapper is
		/// to take over — <c>cairo_script_create</c> returns one. False for a
		/// borrowed pointer such as <c>cairo_surface_get_device</c>'s, which is
		/// then referenced here.
		/// </param>
		internal Device (IntPtr handle, bool owner)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle should not be NULL", "handle");

			this.handle = owner ? handle : NativeMethods.cairo_device_reference (handle);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public Status Acquire ()
		{
			CheckDisposed ();
			return NativeMethods.cairo_device_acquire (handle);
		}

		public void Dispose ()
		{
			if (handle != IntPtr.Zero)
				NativeMethods.cairo_device_destroy (handle);
			handle = IntPtr.Zero;
			GC.SuppressFinalize (this);
		}

		void CheckDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("Object has already been disposed");
		}

		public void Finish ()
		{
			CheckDisposed ();
			NativeMethods.cairo_device_finish (handle);
		}

		public void Flush ()
		{
			CheckDisposed ();
			NativeMethods.cairo_device_flush (handle);
		}

		public void Release ()
		{
			CheckDisposed ();
			NativeMethods.cairo_device_release (handle);
		}

		public Status Status {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_device_status (handle);
			}
		}

		public DeviceType Type {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_device_get_type (handle);
			}
		}

	}
}

