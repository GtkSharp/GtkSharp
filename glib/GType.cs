// GLib.Type.cs - GLib GType class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2003 Mike Kestner
// Copyright (c) 2003 Novell, Inc.
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
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct GType {

		IntPtr val;

		public GType (IntPtr val)
		{
			this.val = val;
		}

		public static GType FromName (string native_name)
		{
			return new GType (g_type_from_name (native_name));
		}
		
		public static readonly GType Invalid = new GType ((IntPtr) TypeFundamentals.TypeInvalid);
		public static readonly GType None = new GType ((IntPtr) TypeFundamentals.TypeNone);
		public static readonly GType Interface = new GType ((IntPtr) TypeFundamentals.TypeInterface);
		public static readonly GType Char = new GType ((IntPtr) TypeFundamentals.TypeChar);
		public static readonly GType UChar = new GType ((IntPtr) TypeFundamentals.TypeUChar);
		public static readonly GType Boolean = new GType ((IntPtr) TypeFundamentals.TypeBoolean);
		public static readonly GType Int = new GType ((IntPtr) TypeFundamentals.TypeInt);
		public static readonly GType UInt = new GType ((IntPtr) TypeFundamentals.TypeUInt);
		public static readonly GType Long = new GType ((IntPtr) TypeFundamentals.TypeLong);
		public static readonly GType ULong = new GType ((IntPtr) TypeFundamentals.TypeULong);
		public static readonly GType Int64 = new GType ((IntPtr) TypeFundamentals.TypeInt64);
		public static readonly GType UInt64 = new GType ((IntPtr) TypeFundamentals.TypeUInt64);
		public static readonly GType Enum = new GType ((IntPtr) TypeFundamentals.TypeEnum);
		public static readonly GType Flags = new GType ((IntPtr) TypeFundamentals.TypeFlags);
		public static readonly GType Float = new GType ((IntPtr) TypeFundamentals.TypeFloat);
		public static readonly GType Double = new GType ((IntPtr) TypeFundamentals.TypeDouble);
		public static readonly GType String = new GType ((IntPtr) TypeFundamentals.TypeString);
		public static readonly GType Pointer = new GType ((IntPtr) TypeFundamentals.TypePointer);
		public static readonly GType Boxed = new GType ((IntPtr) TypeFundamentals.TypeBoxed);
		public static readonly GType Param = new GType ((IntPtr) TypeFundamentals.TypeParam);
		public static readonly GType Object = new GType ((IntPtr) TypeFundamentals.TypeObject);

		static Hashtable types = new Hashtable ();
		static Hashtable gtypes = new Hashtable ();

		public static void Register (GType native_type, System.Type type)
		{
			if (native_type != GType.Pointer && native_type != GType.Boxed && native_type != ManagedValue.GType)
				types[native_type.Val] = type;
			if (type != null)
				gtypes[type] = native_type;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_type_init ();

		static GType ()
		{
			g_type_init ();

			Register (GType.Char, typeof (sbyte));
			Register (GType.UChar, typeof (byte));
			Register (GType.Boolean, typeof (bool));
			Register (GType.Int, typeof (int));
			Register (GType.UInt, typeof (uint));
			Register (GType.Int64, typeof (long));
			Register (GType.UInt64, typeof (ulong));
			Register (GType.Float, typeof (float));
			Register (GType.Double, typeof (double));
			Register (GType.String, typeof (string));
			Register (GType.Pointer, typeof (IntPtr));
			Register (GType.Object, typeof (GLib.Object));

			// One-way mapping
			gtypes[typeof (char)] = GType.UInt;
		}

		public static explicit operator GType (System.Type type)
		{
			GType gtype;

			if (gtypes.Contains (type))
				return (GType)gtypes[type];

			if (type.IsSubclassOf (typeof (GLib.Object))) {
				gtype = GLib.Object.LookupGType (type);
				Register (gtype, type);
				return gtype;
			}

			PropertyInfo pi = type.GetProperty ("GType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (pi != null)
				gtype = (GType) pi.GetValue (null, null);
			else if (type.IsDefined (typeof (GTypeAttribute), false)) {
				GTypeAttribute gattr = (GTypeAttribute)Attribute.GetCustomAttribute (type, typeof (GTypeAttribute), false);
				pi = gattr.WrapperType.GetProperty ("GType", BindingFlags.Public | BindingFlags.Static);
				gtype = (GType) pi.GetValue (null, null);
			} else if (type.IsSubclassOf (typeof (GLib.Opaque)))
				gtype = GType.Pointer;
			else
				gtype = ManagedValue.GType;

			Register (gtype, type);
			return gtype;
		}

		static string GetQualifiedName (string cname)
		{
			for (int i = 1; i < cname.Length; i++) {
				if (System.Char.IsUpper (cname[i])) {
					if (i == 1 && cname [0] == 'G')
						return "GLib." + cname.Substring (1);
					else
						return cname.Substring (0, i) + "." + cname.Substring (i);
				}
			}

			throw new ArgumentException ("cname is not in NamespaceType format. GType.Register should be called directly for " + cname);
		}

		public static explicit operator Type (GType gtype)
		{
			return LookupType (gtype.Val);
		}

		public static void Init ()
		{
			// cctor already calls g_type_init.
		}

		public static Type LookupType (IntPtr typeid)
		{
			if (types.Contains (typeid))
				return (Type)types[typeid];

			string native_name = Marshaller.Utf8PtrToString (g_type_name (typeid));
			string type_name = GetQualifiedName (native_name);
			Type result = null;
			Assembly[] assemblies = (Assembly[]) AppDomain.CurrentDomain.GetAssemblies ().Clone ();
			foreach (Assembly asm in assemblies) {
				result = asm.GetType (type_name);
				if (result != null)
					break;
			}

			if (result == null) {
				// Because of lazy loading of references, it's possible the type's assembly
				// needs to be loaded.  We will look for it by name in the references of
				// the currently loaded assemblies.  Hopefully a recursive traversal is
				// not needed. We avoid one for now because of problems experienced
				// in a patch from bug #400595, and a desire to keep memory usage low
				// by avoiding a complete loading of all dependent assemblies.
				string ns = type_name.Substring (0, type_name.LastIndexOf ('.'));
				string asm_name = ns.ToLower ().Replace ('.', '-') + "-sharp";
				foreach (Assembly asm in assemblies) {
					foreach (AssemblyName ref_name in asm.GetReferencedAssemblies ()) {
						if (ref_name.Name != asm_name)
							continue;
						string asm_dir = Path.GetDirectoryName (asm.Location);
						try {
							Assembly ref_asm;
							if (File.Exists (Path.Combine (asm_dir, ref_name.Name + ".dll")))
								ref_asm = Assembly.LoadFrom (Path.Combine (asm_dir, ref_name.Name + ".dll"));
							else
								ref_asm = Assembly.Load (ref_name);
							result = ref_asm.GetType (type_name);
							if (result != null)
								break;
						} catch (Exception) {
							/* Failure to load a referenced assembly is not an error */
						}
					}
					if (result != null)
						break;
				}
			}

			Register (new GType (typeid), result);
			return result;
		}

		public IntPtr Val {
			get {
				return val;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is GType))
				return false;

			return ((GType) o) == this;
		}

		public static bool operator == (GType a, GType b)
		{
			return a.Val == b.Val;
		}

		public static bool operator != (GType a, GType b)
		{
			return a.Val != b.Val;
		}

		public override int GetHashCode ()
		{
			return val.GetHashCode ();
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name (IntPtr raw);
		
		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		public override string ToString ()
		{
			return Marshaller.Utf8PtrToString (g_type_name (val));
		}
	}
}
