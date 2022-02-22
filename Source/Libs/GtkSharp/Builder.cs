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
			BindFields (handler);
			(new SignalConnector (handler)).ConnectSignals (this);
		}
		
		public void Autoconnect (Type handler_class)
		{
			BindFields (handler_class);
			(new SignalConnector (handler_class)).ConnectSignals (this);
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
		
		void BindFields (object target)
		{
			BindFields (target, target.GetType ());
		}
		
		void BindFields (Type type)
		{
			BindFields (null, type);
		}
		
		void BindFields (object target, Type type)
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
					GLib.Object gobject;
					if (attr.Specified)
						gobject = GetObject (attr.Name);
					else
						gobject = GetObject (field.Name);
		
					if (gobject != null)
						try {
							field.SetValue (target, gobject, flags, null, null);
						} catch (Exception e) {
							Console.WriteLine ("Unable to set value for field " + field.Name);
							throw e;
						}
				}
				type = type.BaseType;
			}
			while (type != typeof(object) && type != null);
		}
	}
}

