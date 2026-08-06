// Pango.FontDescription - value equality for a value-like type
//
// This is free and unencumbered software released into the public domain.

namespace Pango
{

	public partial class FontDescription
	{

		// FontDescription inherited GLib.Opaque's equality, which compares
		// handles. Two descriptions built from the same string are equal by
		// every measure Pango offers -- Equal says so, and Hash returns the same
		// value -- but were unequal here, and hashed differently, so one could
		// not be used to look the other up in a dictionary or found in a set.
		//
		// Pango exposes both operations, so both are used rather than
		// reimplemented: the description's fields are Pango's business, not
		// ours, and it already knows which of them count.

		public override bool Equals (object o)
		{
			var other = o as FontDescription;

			if (other == null)
				return false;

			if (Handle == other.Handle)
				return true;

			return Equal (other);
		}

		public override int GetHashCode ()
		{
			return (int) Hash;
		}

	}

}
