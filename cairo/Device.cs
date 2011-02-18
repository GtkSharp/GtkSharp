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

	public enum DeviceType {
		Drm,
		GL,
		Script,
		Xcb,
		Xlib,
		Xml,
	}

	public class Device : IDisposable
	{

		IntPtr handle;

		internal Device (IntPtr handle)
		{
			this.handle = NativeMethods.cairo_device_reference (handle);
		}

		public Status Acquire ()
		{
			return NativeMethods.cairo_device_acquire (handle);
		}

		public void Dispose ()
		{
			if (handle != IntPtr.Zero)
				NativeMethods.cairo_device_destroy (handle);
			handle = IntPtr.Zero;
			GC.SuppressFinalize (this);
		}

		public void Finish ()
		{
			NativeMethods.cairo_device_finish (handle);
		}

		public void Flush ()
		{
			NativeMethods.cairo_device_flush (handle);
		}

		public void Release ()
		{
			NativeMethods.cairo_device_release (handle);
		}

		public Status Status {
			get { return NativeMethods.cairo_device_status (handle); }
		}

		public DeviceType Type {
			get { return NativeMethods.cairo_device_get_type (handle); }
		}

	}
}

