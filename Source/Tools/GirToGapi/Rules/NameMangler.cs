// NameMangler.cs - gapi name derivation.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Rules {

	using System.Text.RegularExpressions;

	/// <summary>
	/// Derives the gapi <c>name</c> attribute the way gapi2xml.pl did.
	/// </summary>
	/// <remarks>
	/// Every XPath rule in the .metadata files that selects by @name depends on
	/// this being bug-compatible with the Perl original, so StudlyCaps is a
	/// statement-for-statement port of gapi2xml.pl:StudlyCaps rather than a
	/// tidied-up equivalent.
	///
	/// The prefix arithmetic the Perl script did (strip the type's C prefix off
	/// the cname, then StudlyCaps the remainder) is NOT reproduced, because GIR
	/// already reports the prefix-stripped identifier in its own @name. Measured
	/// against GTK 3, StudlyCaps(gir @name) reproduces gapi2xml.pl's @name for
	/// 5987 of 6204 members; every difference is a case where GIR attributes a
	/// function to a different owner than the Perl script's C-header heuristic
	/// did, and GIR is the more accurate of the two. See scripts/name-parity.py
	/// and Docs/gir-gapi-coverage.md.
	/// </remarks>
	public static class NameMangler {

		public static string StudlyCaps (string symbol)
		{
			if (string.IsNullOrEmpty (symbol))
				return symbol;

			// s/^([a-z])/\u\1/
			symbol = Regex.Replace (symbol, "^([a-z])", m => m.Groups [1].Value.ToUpperInvariant ());
			// s/^(\d)/\1_/
			symbol = Regex.Replace (symbol, @"^(\d)", "$1_");
			// s/[-_]([a-z])/\u\1/g
			symbol = Regex.Replace (symbol, "[-_]([a-z])", m => m.Groups [1].Value.ToUpperInvariant ());
			// s/[-_](\d)/\1/g
			symbol = Regex.Replace (symbol, @"[-_](\d)", "$1");
			// s/^2/Two/ ; s/^3/Three/
			symbol = Regex.Replace (symbol, "^2", "Two");
			symbol = Regex.Replace (symbol, "^3", "Three");

			return symbol;
		}

		/// <summary>
		/// Enum members are lower-cased before StudlyCaps (gapi2xml.pl:295), so
		/// that GTK_ALIGN_FILL becomes Fill rather than FILL.
		/// </summary>
		public static string EnumMemberName (string girName)
		{
			return StudlyCaps (girName.ToLowerInvariant ());
		}
	}
}
