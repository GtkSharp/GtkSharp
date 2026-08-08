//
// Cairo.ScriptSurface.cs
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

namespace Cairo {

	/// <summary>
	/// The device half of cairo's script backend: it owns the output file, and
	/// every surface created against it appends to that one script.
	/// </summary>
	public class Script : Device
	{
		/// <summary>Opens <paramref name="filename"/> for the script text.</summary>
		public Script (string filename)
			: base (NativeMethods.cairo_script_create (filename), true)
		{
		}

		/// <summary>
		/// Whether cairo was built with the script backend. Without it the
		/// symbol is absent and <c>FuncLoader</c> leaves a null delegate
		/// behind, which is a <c>NullReferenceException</c> at the call rather
		/// than anything that names the symbol.
		/// </summary>
		public static bool IsSupported {
			get { return NativeMethods.cairo_script_create != null && NativeMethods.cairo_script_surface_create != null; }
		}
	}

	/// <summary>A surface that writes what is drawn into it to a
	/// <see cref="Script"/> device as cairo script text.</summary>
	public class ScriptSurface : Surface
	{
		internal ScriptSurface (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public ScriptSurface (Script script, Content content, double width, double height)
			: base (NativeMethods.cairo_script_surface_create (DeviceHandle (script), content, width, height), true)
		{
		}

		static IntPtr DeviceHandle (Script script)
		{
			if (script == null)
				throw new ArgumentNullException ("script");
			return script.Handle;
		}
	}
}
