// CoverageExclusion.cs - marking generated code so coverage tools skip it
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace GtkSharp.Generation {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Decides where <c>[ExcludeFromCodeCoverage]</c> goes in the generated code.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>// &lt;auto-generated /&gt;</c> tells Roslyn to skip analyser
	/// diagnostics, and a runsettings file tells coverlet to skip a directory.
	/// Neither helps dotCover, or any other tool that reads attributes rather
	/// than paths. This attribute is the portable answer: it is
	/// <c>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage</c>, and
	/// dotCover, coverlet and Visual Studio all honour it.
	/// </para>
	/// <para>
	/// The catch is partial classes. An attribute on <em>any</em> part of a
	/// partial type applies to the <em>whole</em> type, and hand-written partial
	/// classes are this binding's main customisation mechanism: 143 types have
	/// one. Putting the attribute on the generated half of
	/// <c>Gtk.Button</c> would silently stop measuring
	/// <c>Source/Libs/GtkSharp/Button.cs</c> as well, which is the code most
	/// worth measuring -- every defect this suite has found has lived in a
	/// hand-written file.
	/// </para>
	/// <para>
	/// So the rule is:
	/// </para>
	/// <list type="bullet">
	/// <item>a type nobody has extended by hand gets the attribute on the
	/// <em>type</em>, which also covers the static field initialisers that run in
	/// its implicit class constructor;</item>
	/// <item>a type with a hand-written partial gets it on each generated
	/// <em>member</em> instead, leaving the hand-written members measured.</item>
	/// </list>
	/// <para>
	/// Which is which is worked out from the tree rather than configured: the
	/// hand-written partials live in the parent of the output directory, so
	/// adding one later flips that type to per-member marking on the next build
	/// with nothing to remember.
	/// </para>
	/// <para>
	/// One thing this cannot separate: a hand-extended type's implicit class
	/// constructor runs the static field initialisers from <em>both</em> halves,
	/// and no attribute can attribute half a method. Those few lines per type
	/// stay measured. See Docs/coverage.md.
	/// </para>
	/// </remarks>
	public static class CoverageExclusion {

		public const string Attribute = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

		static readonly Regex PartialDeclaration = new Regex (
			@"\bpartial\s+(?:class|struct|interface)\s+([A-Za-z_]\w*)",
			RegexOptions.Compiled);

		static readonly Dictionary<string, HashSet<string>> cache =
			new Dictionary<string, HashSet<string>> ();

		// Not a legal C# identifier, so it can never collide with a real type.
		const string Unknown = "?unknown";

		/// <summary>
		/// True when a hand-written file beside the output directory declares a
		/// partial of <paramref name="name"/>, so the attribute must go on the
		/// generated members rather than the type.
		/// </summary>
		public static bool IsHandExtended (GenerationInfo gen_info, string name)
		{
			if (gen_info == null || String.IsNullOrEmpty (name))
				return false;

			HashSet<string> partials = HandWrittenPartials (gen_info.Dir);

			// Unknown means "could not read the tree", and the safe answer there
			// is per-member: that is always correct, where a type-level attribute
			// can silently swallow a hand-written half.
			return partials.Contains (Unknown) || partials.Contains (name);
		}

		/// <summary>The attribute to put on a generated type, or null.</summary>
		public static string ForType (GenerationInfo gen_info, string name)
		{
			return IsHandExtended (gen_info, name) ? null : Attribute;
		}

		/// <summary>The attribute to put on a generated member, or null.</summary>
		/// <remarks>Only needed where the type could not take it.</remarks>
		public static string ForMemberOf (GenerationInfo gen_info, string name)
		{
			return IsHandExtended (gen_info, name) ? Attribute : null;
		}

		static HashSet<string> HandWrittenPartials (string outdir)
		{
			if (String.IsNullOrEmpty (outdir))
				return new HashSet<string> ();

			HashSet<string> names;
			if (cache.TryGetValue (outdir, out names))
				return names;

			names = new HashSet<string> ();

			try {
				// Generated/ sits inside the assembly's own directory, and that
				// is where the hand-written partial classes are.
				string assembly_dir = Path.GetDirectoryName (Path.GetFullPath (outdir));

				if (assembly_dir != null && Directory.Exists (assembly_dir)) {
					foreach (string file in Directory.GetFiles (assembly_dir, "*.cs")) {
						foreach (Match match in PartialDeclaration.Matches (File.ReadAllText (file)))
							names.Add (match.Groups [1].Value);
					}
				}
			} catch (IOException) {
				// Unreadable tree: fall back to per-member marking for
				// everything. That is always correct; it is the type-level
				// attribute that can over-reach, by swallowing a hand-written
				// half that was never seen.
				names.Add (Unknown);
			}

			cache [outdir] = names;
			return names;
		}
	}
}
