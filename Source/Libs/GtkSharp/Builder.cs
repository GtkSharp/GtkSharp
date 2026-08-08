// Builder.cs - customizations to Gtk.Builder
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
// The biggest part of this code is adapted from glade#, by
//	Ricardo Fernández Pascual <ric@users.sourceforge.net>
//	Rachel Hestilow <hestilow@ximian.com>
//
// Copyright (c) 2002 Ricardo Fernández Pascual
// Copyright (c) 2003 Rachel Hestilow
// Copyright (c) 2008 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk {

	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Text;

	public partial class Builder {
		
		[AttributeUsage (AttributeTargets.Field)]
		public class ObjectAttribute : Attribute
		{
			private string name;
			private bool specified;
		
			public ObjectAttribute (string name)
			{
				specified = true;
				this.name = name;
			}
		
			public ObjectAttribute ()
			{
				specified = false;
			}
		
			public string Name
			{
				get { return name; }
			}
		
			public bool Specified
			{
				get { return specified; }
			}
		}
		
		public IntPtr GetRawObject(string name) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			IntPtr raw_ret = gtk_builder_get_object(Handle, native_name);
			GLib.Marshaller.Free (native_name);
			return raw_ret;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_ref(IntPtr raw);
		static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));

		public IntPtr GetRawOwnedObject(string name) {
			IntPtr raw_ret = GetRawObject (name);
			g_object_ref (raw_ret);
			return raw_ret;
		}
		
		public Builder (System.IO.Stream s) : this (s, null)
		{
		}
		
		public Builder (System.IO.Stream s, string translation_domain)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
		
			AddFromStream (s);
			TranslationDomain = translation_domain;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Builder (string resource_name) : this (Assembly.GetCallingAssembly (), resource_name, null)
		{
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Builder (string resource_name, string translation_domain)
			: this (Assembly.GetCallingAssembly (), resource_name, translation_domain)
		{
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Builder (Assembly assembly, string resource_name, string translation_domain) : this ()
		{
			if (GetType() != typeof (Builder))
				throw new InvalidOperationException ("Cannot chain to this constructor from subclasses.");
		
			if (assembly == null)
				assembly = Assembly.GetCallingAssembly ();
		
			System.IO.Stream s = assembly.GetManifestResourceStream (resource_name);
			if (s == null)
				throw new ArgumentException ("Cannot get resource file '" + resource_name + "'",
				                             "resource_name");
		
			AddFromStream (s);
			TranslationDomain = translation_domain;
		}
		
		public void Autoconnect (object handler)
		{
			Autoconnect (handler, false);
		}

		public void Autoconnect (object handler, bool throwOnUnknownObject)
		{
			BindFields (handler, handler.GetType (), throwOnUnknownObject);

			// Autoconnect does two independent jobs: binding [Object] fields,
			// which works, and connecting signals, which needs GtkBuilderScope in
			// Gtk 4 and throws.
			//
			// This second branch is hard to reach in practice, and that is worth
			// knowing rather than discovering: GtkBuilder resolves a <signal>
			// handler while parsing, so a document declaring one fails to load at
			// all -- see Explain below. The branch stays because a future
			// GtkBuilderScope implementation would make loading succeed while
			// leaving connection the open question.
			if (declares_signals)
				new SignalConnector (handler).ConnectSignals (this);
		}

		public void Autoconnect (Type handler_class)
		{
			Autoconnect (handler_class, false);
		}
		
		public void Autoconnect (Type handler_class, bool throwOnUnknownObject)
		{
			BindFields (null, handler_class, throwOnUnknownObject);

			if (declares_signals)
				new SignalConnector (handler_class).ConnectSignals (this);
		}
		
		void AddFromStream (Stream stream)
		{
			var size = (int)stream.Length;
			var buffer = new byte[size];
			stream.Read (buffer, 0, size);
			stream.Close ();

			// If buffer contains a BOM, omit it while reading, otherwise AddFromString(text) crashes
			var offset = 0;
			if (size >= 3 && buffer [0] == 0xEF && buffer [1] == 0xBB && buffer [2] == 0xBF) {
				offset = 3;
			}

			var text = Encoding.UTF8.GetString (buffer, offset, size - offset);

			AddFromString (text);
		}

		// gtk_builder_add_from_string/_file/_resource are hidden in
		// GtkSharp.metadata and rebound here to give one failure a usable
		// message.
		//
		// Gtk 4 resolves a <signal> element's handler through GtkBuilderScope, at
		// *parse* time. The default scope is GtkBuilderCScope, which looks the
		// name up as an exported C symbol -- so a managed handler is never found,
		// and the document does not merely load with its signals unconnected: it
		// does not load at all. What GtkBuilder reports is
		//
		//     GLib.GException: No function named `OnClicked`.
		//
		// which sends the reader hunting for a missing native symbol rather than
		// telling them that .ui signal handlers are not supported by this
		// binding. Autoconnect's own NotSupportedException never gets a chance to
		// say so, because the builder threw long before Autoconnect was called.
		//
		// So the document is inspected for signals *before* the native call, and
		// if the call then fails, that is the explanation offered -- with
		// GtkBuilder's own error kept as the inner exception. A document with no
		// signals is untouched: its errors stay GtkBuilder's to report.

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool d_gtk_builder_add_from_string(IntPtr raw, IntPtr buffer, IntPtr length, out IntPtr error);
		static d_gtk_builder_add_from_string gtk_builder_add_from_string = FuncLoader.LoadFunction<d_gtk_builder_add_from_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_builder_add_from_string"));

		/// <summary>Adds the objects described by <paramref name="buffer"/>.</summary>
		public bool AddFromString (string buffer)
		{
			bool signals = NoteSignals (buffer);

			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (buffer);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = gtk_builder_add_from_string (
				Handle, native,
				new IntPtr ((long) Encoding.UTF8.GetByteCount (buffer)), out error);
			GLib.Marshaller.Free (native);

			if (error != IntPtr.Zero)
				throw Explain (new GLib.GException (error), signals);

			return raw_ret;
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool d_gtk_builder_add_from_file(IntPtr raw, IntPtr filename, out IntPtr error);
		static d_gtk_builder_add_from_file gtk_builder_add_from_file = FuncLoader.LoadFunction<d_gtk_builder_add_from_file>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_builder_add_from_file"));

		/// <summary>Adds the objects described by the file at <paramref name="filename"/>.</summary>
		public bool AddFromFile (string filename)
		{
			// Read only to look for signals. A file GtkBuilder cannot open is
			// left to GtkBuilder to complain about, with the path in the message.
			bool signals = false;
			try {
				signals = NoteSignals (File.ReadAllText (filename));
			} catch (IOException) {
			} catch (UnauthorizedAccessException) {
			} catch (ArgumentException) {
			}

			IntPtr native = GLib.Marshaller.StringToFilenamePtr (filename);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = gtk_builder_add_from_file (Handle, native, out error);
			GLib.Marshaller.Free (native);

			if (error != IntPtr.Zero)
				throw Explain (new GLib.GException (error), signals);

			return raw_ret;
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate bool d_gtk_builder_add_from_resource(IntPtr raw, IntPtr resource_path, out IntPtr error);
		static d_gtk_builder_add_from_resource gtk_builder_add_from_resource = FuncLoader.LoadFunction<d_gtk_builder_add_from_resource>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_builder_add_from_resource"));

		/// <summary>Adds the objects described by the GResource at
		/// <paramref name="resource_path"/>.</summary>
		public bool AddFromResource (string resource_path)
		{
			bool signals = false;
			try {
				var bytes = GLib.Resources.LookupData (resource_path, GLib.ResourceLookupFlags.None);
				if (bytes != null)
					signals = NoteSignals (Encoding.UTF8.GetString (bytes.Data));
			} catch (GLib.GException) {
				// Not registered, or not readable. GtkBuilder says so next.
			}

			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (resource_path);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = gtk_builder_add_from_resource (Handle, native, out error);
			GLib.Marshaller.Free (native);

			if (error != IntPtr.Zero)
				throw Explain (new GLib.GException (error), signals);

			return raw_ret;
		}

		/// <summary>Records whether <paramref name="xml"/> asks for signal
		/// handlers, and answers the same. Never clears the flag: one document out
		/// of several is enough.</summary>
		bool NoteSignals (string xml)
		{
			if (xml == null || !BuilderXml.DeclaresSignals (xml))
				return false;

			declares_signals = true;
			return true;
		}

		/// <summary>
		/// Turns GtkBuilder's "No function named X" into an explanation, for a
		/// document that declared a signal handler.
		/// </summary>
		/// <remarks>
		/// Only for such a document. Every other error a builder can raise is
		/// GtkBuilder's to describe, and it does so better than anything invented
		/// here: an unknown widget class or a malformed property still arrives as
		/// the <see cref="GLib.GException"/> it always was.
		/// </remarks>
		static Exception Explain (GLib.GException error, bool declaredSignals)
		{
			if (!declaredSignals)
				return error;

			return new NotSupportedException (
				"This document declares a <signal> handler. Gtk 4 resolves builder signal " +
				"handlers through GtkBuilderScope at parse time, and the default scope " +
				"looks each name up as an exported C symbol -- so a managed handler is " +
				"never found and the document fails to load rather than merely loading " +
				"unconnected. GtkSharp does not implement GtkBuilderScope yet: remove the " +
				"<signal> elements and connect the handlers in C# after Autoconnect has " +
				"bound the fields. GtkBuilder reported: " + error.Message,
				error);
		}

		bool declares_signals;

		/// <summary>True when a document handed to this builder declared at least
		/// one signal handler.</summary>
		/// <remarks>Exposed because the alternative way to observe it is to call
		/// Autoconnect and catch the exception, which also binds fields.</remarks>
		public bool DeclaresSignals {
			get { return declares_signals; }
		}
		
		void BindFields (object target, Type type, bool throwOnUnknownObject)
		{
			System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
			if (target != null)
				flags |= System.Reflection.BindingFlags.Instance;
			else
				flags |= System.Reflection.BindingFlags.Static;
		
			do {
				System.Reflection.FieldInfo[] fields = type.GetFields (flags);
				if (fields == null)
					return;
		
				foreach (System.Reflection.FieldInfo field in fields)
				{
					object[] attrs = field.GetCustomAttributes (typeof (ObjectAttribute), false);
					if (attrs == null || attrs.Length == 0)
						continue;
					// The widget to field binding must be 1:1, so only check
					// the first attribute.
					ObjectAttribute attr = (ObjectAttribute) attrs[0];
					string name = attr.Specified ? attr.Name : field.Name;
					GLib.Object gobject = GetObject (name);
		
					if (gobject != null)
						try {
							field.SetValue (target, gobject, flags, null, null);
						} catch (Exception e) {
							Console.WriteLine ("Unable to set value for field " + field.Name);
							throw e;
						}
					else if (throwOnUnknownObject)
						throw new Exception ("Unknown object '" + name + "' to connect in type '" + type + "'");
				}
				type = type.BaseType;
			}
			while (type != typeof(object) && type != null);
		}
	}
}
