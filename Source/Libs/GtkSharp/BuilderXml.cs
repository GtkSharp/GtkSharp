// Gtk.BuilderXml.cs - inspection of GtkBuilder documents
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System.IO;
	using System.Xml;

	/// <summary>
	/// Shared inspection of GtkBuilder XML, used by both <see cref="Builder"/>
	/// and the <c>[Template]</c> path in <see cref="Widget"/>.
	/// </summary>
	static class BuilderXml {

		/// <summary>
		/// True when the document asks for at least one signal handler.
		/// </summary>
		/// <remarks>
		/// Gtk 4 connects builder signals through GtkBuilderScope, which is not
		/// bound, so attempting it throws. Only documents that actually declare
		/// a signal need to fail; binding fields works either way.
		///
		/// This parses rather than searching for "&lt;signal", because that text
		/// also occurs in comments -- including comments explaining that a
		/// document deliberately has no signals, which is exactly the case that
		/// must not be misread.
		/// </remarks>
		public static bool DeclaresSignals (string xml)
		{
			try {
				var settings = new XmlReaderSettings {
					DtdProcessing = DtdProcessing.Ignore,
					IgnoreComments = true,
					IgnoreWhitespace = true,
					XmlResolver = null,
				};

				using (var reader = XmlReader.Create (new StringReader (xml), settings)) {
					while (reader.Read ()) {
						if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "signal")
							return true;
					}
				}
			} catch (XmlException) {
				// Malformed XML is GtkBuilder's problem to report, and it will
				// do so with a better message than anything available here.
				// Assume signals so the failure is loud rather than silent.
				return true;
			}

			return false;
		}
	}
}
