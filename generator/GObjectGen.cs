// GtkSharp.Generation.GObjectGen.cs - The GObject generatable
//
// Note: This generatable only handles GObject* values. GObject subclasses
// are handled by ObjectGen.
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//
// (c) 2004 Rachel Hestilow

namespace GtkSharp.Generation {

	public class GObjectGen : ManualGen {

		public GObjectGen () : base ("GObject", "GLib.Object") {}
		
		public override string FromNative(string var)
		{
			return "GLib.Object.GetObject (" + var + ")";
		}
	}
}

