// Gdk.Color.cs - the Gtk 3 GdkColor, re-provided over GdkRGBA
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gdk {

	using System;
	using System.Globalization;

	/// <summary>
	/// Stands in for GdkColor, which Gtk 4 removed outright in favour of
	/// <see cref="RGBA"/>.
	/// </summary>
	/// <remarks>
	/// <para>The channels are 16-bit, as GdkColor's were. That is not cosmetic: code written
	/// against Gtk 3 divides by 65535 to reach a 0..1 range, and a shim that quietly stored
	/// 8-bit channels would make every such conversion 257 times too dark while still
	/// compiling and still looking like a colour.</para>
	/// <para>There is no <c>Pixel</c> field. It was the index into a GdkColormap, colormaps
	/// went away in Gtk 3 already, and anything still reading it wants a different fix.</para>
	/// </remarks>
	public struct Color : IEquatable<Color> {

		public ushort Red;
		public ushort Green;
		public ushort Blue;

		/// <summary>Builds a colour from 8-bit channels, as the Gtk 3 constructor did.</summary>
		/// <remarks>
		/// Each byte is replicated into both halves of the 16-bit channel (n * 257) rather
		/// than shifted (n * 256), so 0xFF maps to 0xFFFF - full white - instead of 0xFF00.
		/// </remarks>
		public Color(byte red, byte green, byte blue)
		{
			Red = (ushort)(red * 257);
			Green = (ushort)(green * 257);
			Blue = (ushort)(blue * 257);
		}

		public Color(ushort red, ushort green, ushort blue)
		{
			Red = red;
			Green = green;
			Blue = blue;
		}

		/// <summary>
		/// Parses a colour specification, accepting everything gdk_rgba_parse does
		/// ("#rrggbb", "rgb(...)", CSS colour names).
		/// </summary>
		/// <remarks>
		/// Any alpha in the specification is dropped, because GdkColor had nowhere to put it -
		/// the same lossiness the Gtk 3 gdk_color_parse had.
		/// </remarks>
		public static bool Parse(string spec, ref Color color)
		{
			var rgba = new RGBA();

			if (!rgba.Parse(spec))
				return false;

			color = FromRGBA(rgba);
			return true;
		}

		public static Color FromRGBA(RGBA rgba)
		{
			return new Color(
				ToChannel(rgba.Red),
				ToChannel(rgba.Green),
				ToChannel(rgba.Blue));
		}

		/// <summary>The equivalent <see cref="RGBA"/>, fully opaque.</summary>
		public RGBA ToRGBA()
		{
			return new RGBA {
				Red = Red / (float)ushort.MaxValue,
				Green = Green / (float)ushort.MaxValue,
				Blue = Blue / (float)ushort.MaxValue,
				Alpha = 1f
			};
		}

		static ushort ToChannel(float component)
		{
			if (component <= 0)
				return 0;
			if (component >= 1)
				return ushort.MaxValue;

			return (ushort)Math.Round(component * ushort.MaxValue);
		}

		/// <summary>"#rrggbb", the form gdk_color_to_string produced without its 16-bit width.</summary>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}",
				Red >> 8, Green >> 8, Blue >> 8);
		}

		public bool Equals(Color other)
		{
			return Red == other.Red && Green == other.Green && Blue == other.Blue;
		}

		public override bool Equals(object o)
		{
			return o is Color && Equals((Color)o);
		}

		public override int GetHashCode()
		{
			return (Red << 16) ^ (Green << 8) ^ Blue;
		}

		public static bool operator ==(Color a, Color b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Color a, Color b)
		{
			return !a.Equals(b);
		}
	}
}
