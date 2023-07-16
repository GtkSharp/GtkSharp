// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[StructLayout(LayoutKind.Sequential)]
	public partial struct MarkupParser : IEquatable<MarkupParser> {

		private readonly IntPtr _start_element;
		private readonly IntPtr _end_element;
		private readonly IntPtr _text;
		private readonly IntPtr _passthrough;
		private readonly IntPtr _error;

		public static GLib.MarkupParser Zero = new GLib.MarkupParser ();

		public static GLib.MarkupParser New(IntPtr raw) {
			if (raw == IntPtr.Zero)
				return GLib.MarkupParser.Zero;
			return (GLib.MarkupParser) Marshal.PtrToStructure (raw, typeof (GLib.MarkupParser));
		}

		public bool Equals (MarkupParser other)
		{
			return true && _start_element.Equals (other._start_element) && _end_element.Equals (other._end_element) && _text.Equals (other._text) && _passthrough.Equals (other._passthrough) && _error.Equals (other._error);
		}

		public override bool Equals (object other)
		{
			return other is MarkupParser && Equals ((MarkupParser) other);
		}

		public override int GetHashCode ()
		{
			return this.GetType ().FullName.GetHashCode () ^ _start_element.GetHashCode () ^ _end_element.GetHashCode () ^ _text.GetHashCode () ^ _passthrough.GetHashCode () ^ _error.GetHashCode ();
		}

		private static GLib.GType GType {
			get { return GLib.GType.Pointer; }
		}
#endregion
	}
}

