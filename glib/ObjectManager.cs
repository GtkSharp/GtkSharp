// GLib.ObjectManager.cs - GLib ObjectManager class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright <c> 2001-2002 Mike Kestner
// Copyright <c> 2004-2005 Novell, Inc.
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


namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System.Reflection;
	using System.Text;

	public class ObjectManager {

		private static Hashtable types = new Hashtable ();

		static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance;

		public static GLib.Object CreateObject (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
				return null;

			Type type = GetTypeOrParent (raw);

			if (type == null)
				return null;

			GLib.Object obj;
			try {
				obj = Activator.CreateInstance (type, flags, null, new object[] {raw}, null) as GLib.Object;
			} catch (MissingMethodException) {
				throw new GLib.MissingIntPtrCtorException ("GLib.Object subclass " + type + " must provide a protected or public IntPtr ctor to support wrapping of native object handles.");
			}
			return obj;
		}

		[Obsolete ("Use the (GType, Type) overload instead")]
		public static void RegisterType (string native_name, string managed_name, string assembly)
		{
			RegisterType (native_name, managed_name + "," + assembly);
		}

		[Obsolete ("Use the (GType, Type) overload instead")]
		public static void RegisterType (string native_name, string mangled)
		{
			types [native_name] = mangled;
		}

		public static void RegisterType (GType native_type, System.Type type)
		{
			types [native_type.Val] = type;
		}

		static string GetQualifiedName (string cname)
		{
			for (int i = 1; i < cname.Length; i++) {
				if (Char.IsUpper (cname[i])) {
					if (i == 1 && cname [0] == 'G')
						return "GLib." + cname.Substring (1);
					else
						return cname.Substring (0, i) + "." + cname.Substring (i);
				}
			}

			throw new ArgumentException ("cname is not in NamespaceType format. RegisterType should be called directly for " + cname);
		}

		static Type GetTypeOrParent (IntPtr obj)
		{
			IntPtr typeid = gtksharp_get_type_id (obj);

			Type result = LookupType (typeid);
			while (result == null) {
				typeid = gtksharp_get_parent_type (typeid);
				if (typeid == IntPtr.Zero)
					return null;
				result = LookupType (typeid);
			}
			return result;
		}

		static Type LookupType (IntPtr typeid)
		{
			if (types.Contains (typeid))
				return types [typeid] as Type;

			string native_name = Marshaller.Utf8PtrToString (gtksharp_get_type_name_for_id (typeid));
			string type_name = GetQualifiedName (native_name);
			Type result = null;
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ()) {
				result = asm.GetType (type_name);
				if (result != null)
					break;
			}

			types [typeid] = result;
			return result;
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_id (IntPtr raw);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_parent_type (IntPtr typ);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_name_for_id (IntPtr typ);
	}
}
