// GLib.Type.cs - GLib GType class implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//          Andres G. Aragoneses <knocte@gmail.com>
//
// Copyright (c) 2003 Mike Kestner
// Copyright (c) 2003 Novell, Inc.
// Copyright (c) 2013 Andres G. Aragoneses
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
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;

	public delegate System.Type TypeResolutionHandler (GLib.GType gtype, string gtype_name);

	[StructLayout(LayoutKind.Sequential)]
	public struct GType {

		IntPtr val;

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void ClassInitDelegate (IntPtr gobject_class_handle);

		struct GTypeInfo {
			public ushort class_size;
			public IntPtr base_init;
			public IntPtr base_finalize;
			public ClassInitDelegate class_init;
			public IntPtr class_finalize;
			public IntPtr class_data;
			public ushort instance_size;
			public ushort n_preallocs;
			public IntPtr instance_init;
			public IntPtr value_table;
		}

		struct GTypeQuery {
			public IntPtr type;
			public IntPtr type_name;
			public uint class_size;
			public uint instance_size;
		}

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
		public static readonly GType Variant = new GType ((IntPtr) TypeFundamentals.TypeVariant);


		static HashSet<GType> managedTypes = new HashSet<GType> ();
		static IDictionary<IntPtr, Type> types = new Dictionary<IntPtr, Type> ();
		static IDictionary<Type, GType> gtypes = new Dictionary<Type, GType> ();

		public static void Register (GType native_type, System.Type type)
		{
			Register (native_type, type, false);
		}

		public static void Register (GType native_type, System.Type type, bool managed)
		{
			lock (types) {
				if (native_type != GType.Pointer && native_type != GType.Boxed && native_type != ManagedValue.GType)
					types[native_type.Val] = type;
				if (type != null)
					gtypes[type] = native_type;
				if (managed)
					managedTypes.Add(native_type);
			}
		}

		public static bool IsManaged (GType gtype)
		{
			return managedTypes.Contains(gtype);
		}

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
			Register (GType.Pointer, typeof (IntPtr));
			Register (GType.Variant, typeof (GLib.Variant));

			// One-way mapping
			gtypes[typeof (char)] = GType.UInt;
		}

		public static explicit operator GType (System.Type type)
		{
			GType gtype;

			lock (types) {
				if (gtypes.ContainsKey (type))
					return gtypes[type];
			}

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
			if (string.IsNullOrEmpty (cname))
				return null;

			for (int i = 1; i < cname.Length; i++) {
				if (System.Char.IsUpper (cname[i])) {
					if (i == 1 && cname [0] == 'G')
						return "GLib." + cname.Substring (1);
					else
						return cname.Substring (0, i) + "." + cname.Substring (i);
				}
			}

			return null;
		}

		public static explicit operator Type (GType gtype)
		{
			return LookupType (gtype.Val);
		}

		public static void Init ()
		{
			// cctor already calls g_type_init.
		}

		public static event TypeResolutionHandler ResolveType;

		public static Type LookupType (IntPtr typeid)
		{
			lock (types) {
				if (types.ContainsKey (typeid))
					return types[typeid];
			}

			string native_name = Marshaller.Utf8PtrToString (g_type_name (typeid));

			if (ResolveType != null) {
				GLib.GType gt = new GLib.GType (typeid);

				Delegate[] invocation_list = ResolveType.GetInvocationList ();
				foreach (Delegate d in invocation_list) {
					TypeResolutionHandler handler = (TypeResolutionHandler) d;
					System.Type tmp = handler (gt, native_name);
					if (tmp != null) {
						Register (gt, tmp);
						return tmp;
					}
				}
			}

			string type_name = GetQualifiedName (native_name);
			if (type_name == null)
				return null;
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
						try {
							string asm_dir = Path.GetDirectoryName (asm.Location);
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
			get { return val; }
		}

		public static bool operator == (GType a, GType b)
		{
			return a.Val == b.Val;
		}

		public static bool operator != (GType a, GType b)
		{
			return a.Val != b.Val;
		}

		public override bool Equals (object o)
		{
			if (!(o is GType))
				return false;

			return ((GType) o) == this;
		}

		public override int GetHashCode ()
		{
			return val.GetHashCode ();
		}

		public override string ToString ()
		{
			return Marshaller.Utf8PtrToString (g_type_name (val));
		}

		public IntPtr GetClassPtr ()
		{
			IntPtr klass = g_type_class_peek (val);
			if (klass == IntPtr.Zero)
				klass = g_type_class_ref (val);
			return klass;
		}

		public IntPtr GetDefaultInterfacePtr ()
		{
			IntPtr klass = g_type_default_interface_peek (val);
			if (klass == IntPtr.Zero)
				klass = g_type_default_interface_ref (val);
			return klass;
		}

		public GType GetBaseType ()
		{
			IntPtr parent = g_type_parent (this.Val);
			return parent == IntPtr.Zero ? GType.None : new GType (parent);
		}

		public GType GetThresholdType ()
		{
			GType curr_type = this;
			while (IsManaged (curr_type))
				curr_type = curr_type.GetBaseType ();
			return curr_type;
		}

		public uint GetClassSize ()
		{
			GTypeQuery query;
			g_type_query (this.Val, out query);
			return query.class_size;
		}

		internal void EnsureClass ()
		{
			if (g_type_class_peek (val) == IntPtr.Zero)
				g_type_class_ref (val);
		}

		static int type_uid;
		static string BuildEscapedName (System.Type t)
		{
			string qn = t.FullName;
			// Just a random guess
			StringBuilder sb = new StringBuilder (20 + qn.Length);
			sb.Append ("__gtksharp_");
			sb.Append (type_uid++);
			sb.Append ("_");
			foreach (char c in qn) {
				if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
					sb.Append (c);
				else if (c == '.')
					sb.Append ('_');
				else if ((uint) c <= byte.MaxValue) {
					sb.Append ('+');
					sb.Append (((byte) c).ToString ("x2"));
				} else {
					sb.Append ('-');
					sb.Append (((uint) c).ToString ("x4"));
				}
			}
			return sb.ToString ();
		}

		internal static GType RegisterGObjectType (Object.ClassInitializer gobject_class_initializer)
		{
			GType parent_gtype = LookupGObjectType (gobject_class_initializer.Type.BaseType);

			TypeNameAttribute nattr = (TypeNameAttribute)Attribute.GetCustomAttribute (gobject_class_initializer.Type, typeof (TypeNameAttribute), false);
			string name = nattr != null ? nattr.Name : BuildEscapedName (gobject_class_initializer.Type);

			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			GTypeQuery query;
			g_type_query (parent_gtype.Val, out query);
			GTypeInfo info = new GTypeInfo ();
			info.class_size = (ushort) query.class_size;
			info.instance_size = (ushort) query.instance_size;
			info.class_init = gobject_class_initializer.ClassInitManagedDelegate;

			GType gtype = new GType (g_type_register_static (parent_gtype.Val, native_name, ref info, 0));
			GLib.Marshaller.Free (native_name);
			Register (gtype, gobject_class_initializer.Type, true);

			return gtype;
		}

		internal static GType LookupGObjectType (System.Type t)
		{
			lock (types) {
				if (gtypes.ContainsKey (t))
					return gtypes [t];
			}

			PropertyInfo pi = t.GetProperty ("GType", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			if (pi != null)
				return (GType) pi.GetValue (null, null);
			
			return GLib.Object.RegisterGType (t);
		}

		internal static IntPtr ValFromInstancePtr (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return IntPtr.Zero;

			// First field of instance is a GTypeClass*.  
			IntPtr klass = Marshal.ReadIntPtr (handle);
			// First field of GTypeClass is a GType.
			return Marshal.ReadIntPtr (klass);
		}

		internal static bool Is (IntPtr type, GType is_a_type)
		{
			return g_type_is_a (type, is_a_type.Val);
		}

		public bool IsInstance (IntPtr raw)
		{
			return GType.Is (ValFromInstancePtr (raw), this);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_class_peek(IntPtr gtype);
		static d_g_type_class_peek g_type_class_peek = FuncLoader.LoadFunction<d_g_type_class_peek>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_class_peek"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_class_ref(IntPtr gtype);
		static d_g_type_class_ref g_type_class_ref = FuncLoader.LoadFunction<d_g_type_class_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_class_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_default_interface_peek(IntPtr gtype);
		static d_g_type_default_interface_peek g_type_default_interface_peek = FuncLoader.LoadFunction<d_g_type_default_interface_peek>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_default_interface_peek"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_default_interface_ref(IntPtr gtype);
		static d_g_type_default_interface_ref g_type_default_interface_ref = FuncLoader.LoadFunction<d_g_type_default_interface_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_default_interface_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_from_name(string name);
		static d_g_type_from_name g_type_from_name = FuncLoader.LoadFunction<d_g_type_from_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_from_name"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_type_init();
		static d_g_type_init g_type_init = FuncLoader.LoadFunction<d_g_type_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_init"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_name(IntPtr raw);
		static d_g_type_name g_type_name = FuncLoader.LoadFunction<d_g_type_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_name"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_parent(IntPtr type);
		static d_g_type_parent g_type_parent = FuncLoader.LoadFunction<d_g_type_parent>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_parent"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_type_query(IntPtr type, out GTypeQuery query);
		static d_g_type_query g_type_query = FuncLoader.LoadFunction<d_g_type_query>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_query"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_type_register_static(IntPtr parent, IntPtr name, ref GTypeInfo info, int flags);
		static d_g_type_register_static g_type_register_static = FuncLoader.LoadFunction<d_g_type_register_static>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_register_static"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_type_is_a(IntPtr type, IntPtr is_a_type);
		static d_g_type_is_a g_type_is_a = FuncLoader.LoadFunction<d_g_type_is_a>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_type_is_a"));
	}
}

