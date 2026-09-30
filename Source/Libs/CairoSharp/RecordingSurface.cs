//
// Cairo.RecordingSurface.cs
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
using System.Runtime.InteropServices;

namespace Cairo {

	/// <summary>
	/// A surface that records the operations played into it instead of
	/// rasterising them, so they can be replayed later at any scale or onto any
	/// other surface.
	/// </summary>
	public class RecordingSurface : Surface
	{
		internal RecordingSurface (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		/// <summary>Creates an unbounded recording surface.</summary>
		public RecordingSurface (Content content)
			: base (NativeMethods.cairo_recording_surface_create (content, IntPtr.Zero), true)
		{
		}

		/// <summary>
		/// Creates a recording surface bounded by <paramref name="extents"/>;
		/// anything drawn outside them is clipped away at replay.
		/// </summary>
		public RecordingSurface (Content content, Rectangle extents)
			: base (Create (content, extents), true)
		{
		}

		static IntPtr Create (Content content, Rectangle extents)
		{
			// cairo_recording_surface_create takes a pointer to a
			// cairo_rectangle_t, and reads it during the call only.
			IntPtr native = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (Rectangle)));
			try {
				Marshal.StructureToPtr (extents, native, false);
				return NativeMethods.cairo_recording_surface_create (content, native);
			} finally {
				Marshal.FreeHGlobal (native);
			}
		}

		/// <summary>
		/// The bounding box of everything drawn into the surface so far, in
		/// its own user space. An empty recording has a zero-area box.
		/// </summary>
		public Rectangle InkExtents {
			get {
				CheckDisposed ();
				double x, y, width, height;
				NativeMethods.cairo_recording_surface_ink_extents (Handle, out x, out y, out width, out height);
				return new Rectangle (x, y, width, height);
			}
		}

		/// <summary>
		/// The extents the surface was created with. Returns false — leaving
		/// <paramref name="extents"/> at its default — for an unbounded
		/// surface.
		/// </summary>
		public bool GetExtents (out Rectangle extents)
		{
			CheckDisposed ();
			return NativeMethods.cairo_recording_surface_get_extents (Handle, out extents);
		}
	}
}
