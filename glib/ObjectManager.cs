// GLib.ObjectManager.cs - GLib ObjectManager class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright <c> 2001-2002 Mike Kestner
// Copyright <c> 2004 Novell, Inc.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Text;

	public class ObjectManager {

		private static Hashtable types = new Hashtable ();

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_get_type_name (IntPtr raw);

		public static GLib.Object CreateObject (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
				return null;

			string typename = Marshal.PtrToStringAnsi (gtksharp_get_type_name (raw));
			string mangled;
			if (types.ContainsKey(typename)) 
				mangled = (string)types[typename];
			else
				mangled = GetExpected (typename);
			Type t = Type.GetType (mangled);

			// if null, try to get a parent type
			if (t == null)
				t = GetValidParentType (raw);

			if (t == null) {
				// should never get reached since everything should end up at
				// GObject. Perhaps throw an exception here instead?
				Console.WriteLine ("*** Warning: No C# equivalent for class '" + typename +
						   "' found, returning null");
				return null;
			}

			GLib.Object obj;
			try {
				obj = (GLib.Object) Activator.CreateInstance (t, new object[] {raw});
			} catch (MissingMethodException) {
				throw new GLib.MissingIntPtrCtorException ("GLib.Object subclass " + t + " must provide a protected or public IntPtr ctor to support wrapping of native object handles.");
			}
			return obj;
		}

		public static void RegisterType (string native_name, string managed_name, string assembly)
		{
			RegisterType (native_name, managed_name + "," + assembly);
		}

		public static void RegisterType (string native_name, string mangled)
		{
			types [native_name] = mangled;
		}

		static string GetExpected (string cname)
		{
			StringBuilder expected = new StringBuilder ();
			string ns = "";
			bool needs_dot = true;
			for (int i = 0; i < cname.Length; i++)
			{
				if (needs_dot && i > 0 && Char.IsUpper (cname[i])) {
					// check for initial "G" and mangle to "GLib" if so
					// really only necessary for GObject
					if (expected.Length == 1 && expected[0] == 'G') {
						ns = "glib";
						expected = new StringBuilder ("GLib.");
					} else {
						ns = expected.ToString ().ToLower (); 
						expected.Append ('.');
					}
					needs_dot = false;
				}
				expected.Append (cname[i]);
			}
			expected.AppendFormat (",{0}-sharp", ns);

			string expected_string = expected.ToString ();
			RegisterType (cname, expected_string);
			return expected_string;
		}

		[DllImport("glibsharpglue")]
		static extern int gtksharp_get_type_id (IntPtr raw);

		[DllImport("glibsharpglue")]
		static extern int gtksharp_get_parent_type (int typ);

		[DllImport("glibsharpglue")]
		static extern IntPtr gtksharp_get_type_name_for_id (int typ);

		static Type GetValidParentType (IntPtr raw)
		{
			int type_id = gtksharp_get_type_id (raw);
			string typename;
			string mangled;
			Type t;
			// We will always end up at GObject and will break this loop
			while (true) {
				type_id = gtksharp_get_parent_type (type_id);
				typename = Marshal.PtrToStringAnsi (gtksharp_get_type_name_for_id (type_id));
				if (types.ContainsKey (typename))
					mangled = (string)types[typename];
				else
					mangled = GetExpected (typename);
				t = Type.GetType (mangled);
				if (t != null) {
					return t;
				}
			}
		}
	}
}
