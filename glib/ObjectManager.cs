// Generated File.  Do not modify.
// <c> 2001-2002 Mike Kestner

namespace GtkSharp {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Text;

	public class ObjectManager {

		private static Hashtable types = new Hashtable ();

		[DllImport("gtksharpglue")]
		static extern string gtksharp_get_type_name (IntPtr raw);

		public static GLib.Object CreateObject (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
				return null;

			string typename = gtksharp_get_type_name (raw);
			string mangled;
			if (types.ContainsKey(typename)) 
				mangled = (string)types[typename];
			else
				mangled = GetExpected (typename);

			Type t = Type.GetType (mangled);
			if (t == null)
				return null;
			return (GLib.Object) Activator.CreateInstance (t, new object[] {raw});
		}

		public static void RegisterType (string native_name, string managed_name, string assembly)
		{
			types.Add(native_name, managed_name + "," + assembly);
		}

		public static void RegisterType (string native_name, string mangled)
		{
			types.Add(native_name, mangled);
		}

		static string GetExpected (string cname)
		{
			StringBuilder expected = new StringBuilder ();
			string ns = "";
			bool needs_dot = true;
			for (int i = 0; i < cname.Length; i++)
			{
				if (needs_dot && i > 0 && Char.IsUpper (cname[i])) {
					ns = expected.ToString ().ToLower (); 
					expected.Append ('.');
					needs_dot = false;
				}
				expected.Append (cname[i]);
			}
			expected.AppendFormat (",{0}-sharp", ns);
			return expected.ToString ();
		}
	}
}
