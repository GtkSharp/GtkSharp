// GtkSharp.Generation.SymbolTable.cs - The Symbol Table Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;

	public class SymbolTable {
		
		private static Hashtable alias = new Hashtable ();
		private static Hashtable complex_types = new Hashtable ();
		private static Hashtable simple_types;
		private static Hashtable manually_wrapped_types;
		private static Hashtable dlls;
		
		static SymbolTable ()
		{
			simple_types = new Hashtable ();
			simple_types.Add ("void", "void");
			simple_types.Add ("gboolean", "bool");
			simple_types.Add ("gint", "int");
			simple_types.Add ("guint", "uint");
			simple_types.Add ("glong", "long");
			simple_types.Add ("gshort", "short");
			simple_types.Add ("gushort", "ushort");
			simple_types.Add ("guint32", "uint");
			simple_types.Add ("const-gchar", "string");
			simple_types.Add ("const-char", "string");
			simple_types.Add ("gchar", "string");
			simple_types.Add ("GObject", "GLib.Object");
			simple_types.Add ("gfloat", "float");
			simple_types.Add ("gdouble", "double");
			simple_types.Add ("gint8", "byte");
			simple_types.Add ("guint8", "byte");
			simple_types.Add ("gint16", "short");
			simple_types.Add ("gint32", "int");
			simple_types.Add ("guint16", "ushort");
			simple_types.Add ("guint1", "bool");
			simple_types.Add ("gpointer", "System.IntPtr");
			simple_types.Add ("guchar", "byte");
			simple_types.Add ("GValue", "GLib.Value");
			simple_types.Add ("GtkType", "int");
			simple_types.Add ("long", "long");
			simple_types.Add ("gulong", "ulong");
			simple_types.Add ("GQuark", "int");
			simple_types.Add ("int", "int");
			simple_types.Add ("char", "string");
			simple_types.Add ("double", "double");
			simple_types.Add ("float", "float");
			simple_types.Add ("gunichar", "string");
			simple_types.Add ("uint1", "bool");
			simple_types.Add ("GPtrArray", "System.IntPtr[]");
			simple_types.Add ("GType", "int");
			simple_types.Add ("GError", "IntPtr");
			// gsize is a system-specific typedef in glibconfig.h,
			// but this should work for now
			simple_types.Add ("gsize", "uint");
			simple_types.Add ("gssize", "int");
			
			// FIXME: These ought to be handled properly.
			simple_types.Add ("GList", "System.IntPtr");
			simple_types.Add ("GMemChunk", "System.IntPtr");
			simple_types.Add ("GTimeVal", "System.IntPtr");
			simple_types.Add ("GClosure", "System.IntPtr");
			simple_types.Add ("GArray", "System.IntPtr");
			simple_types.Add ("GData", "System.IntPtr");
			simple_types.Add ("GTypeModule", "GLib.Object");
			simple_types.Add ("GHashTable", "System.IntPtr");
			simple_types.Add ("va_list", "System.IntPtr");
			simple_types.Add ("GParamSpec", "System.IntPtr");
			simple_types.Add ("GdkAtom", "System.IntPtr");
			simple_types.Add ("gconstpointer", "System.IntPtr");

			manually_wrapped_types = new Hashtable ();
			manually_wrapped_types.Add ("GdkEvent", "Gdk.Event");
			manually_wrapped_types.Add ("GSList", "GLib.SList");

			dlls = new Hashtable();
			dlls.Add("Pango", "pango-1.0");
			dlls.Add("Atk", "atk-1.0");
			dlls.Add("Gdk", "gdk-x11-2.0");
			dlls.Add("Gdk.Imaging", "gdk_pixbuf-2.0");
			dlls.Add("Gtk", "gtk-x11-2.0");
		}
		
		public static void AddAlias (string name, string type)
		{
			type = type.TrimEnd(' ', '\t');
			alias [name] = type;
		}
		
		public static void AddType (IGeneratable gen)
		{
			complex_types [gen.CName] = gen;
		}
		
		public static int Count {
			get
			{
				return complex_types.Count;
			}
		}
		
		public static IEnumerable Generatables {
			get {
				return complex_types.Values;
			}
		}
		
		private static string Trim(string type)
		{
			string trim_type = type.TrimEnd('*');
			if (trim_type.StartsWith("const-")) return trim_type.Substring(6);
			return trim_type;
		}

		private static string DeAlias (string type)
		{
			while (alias.ContainsKey(type))
				type = (string) alias[type];

			return type;
		}

		public static string FromNativeReturn(string c_type, string val)
		{
			return FromNative (c_type, val, true);
		}

		public static string FromNative(string c_type, string val)
		{
			return FromNative (c_type, val, false);
		}

		public static string FromNative(string c_type, string val, bool ret)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (simple_types.ContainsKey(c_type)) {
				return val;
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (ret)
					return gen.FromNativeReturn(val);
				else
					return gen.FromNative(val);
			} else if (manually_wrapped_types.ContainsKey(c_type)) {
				// FIXME: better way of handling this?
				if (c_type == "GSList") {
					return "new GLib.SList (" + val + ")";
				} else {
					return "(" + GetCSType (c_type) + ") GLib.Object.GetObject(" + val + ")";
				}
			} else {
				return "";
			}
		}
		
		public static string GetCSType(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (simple_types.ContainsKey(c_type)) {
				return (string) simple_types[c_type];
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				return gen.QualifiedName;
			} else if (manually_wrapped_types.ContainsKey(c_type)) {
				return (string) manually_wrapped_types[c_type];
			} else {
				return "";
			}
		}
		
		public static string GetName(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (simple_types.ContainsKey(c_type) || manually_wrapped_types.ContainsKey(c_type)) {
				string stype;
				if (simple_types.ContainsKey(c_type))
					stype = (string) simple_types[c_type];
				else
					stype = (string) manually_wrapped_types[c_type];
				int dotidx = stype.IndexOf(".");
				if (dotidx == -1) {
					return stype;
				} else {
					return stype.Substring(dotidx+1);
				}
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				return gen.Name;
			} else {
				return "";
			}
		}
		
		public static string GetMarshalReturnType(string c_type)
		{
			return GetMarshalType (c_type, true);
		}
		
		public static string GetMarshalType(string c_type)
		{
			return GetMarshalType (c_type, false);
		}
		
		public static string GetMarshalType(string c_type, bool ret)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (simple_types.ContainsKey(c_type)) {
				return (string) simple_types[c_type];
			} else if (manually_wrapped_types.ContainsKey(c_type)) {
				return "IntPtr";
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (ret)
					return gen.MarshalReturnType;
				else
					return gen.MarshalType;
			} else {
				return "";
			}
		}
		
		public static string CallByName(string c_type, string var_name)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (simple_types.ContainsKey(c_type)) {
				return var_name;
			} else if (manually_wrapped_types.ContainsKey(c_type)) {
				return var_name + ".Handle";
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				return gen.CallByName(var_name);
			} else {
				return "";
			}
		}
	
		public static bool IsOpaque(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is OpaqueGen) {
					return true;
				}
			}
			return false;
		}
	
		public static bool IsBoxed(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is BoxedGen) {
					return true;
				}
			}
			return false;
		}
		
		public static bool IsStruct(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is StructGen) {
					return true;
				}
			}
			return false;
		}
	
		public static bool IsEnum(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is EnumGen) {
					return true;
				}
			}
			return false;
		}
		
		public static bool IsInterface(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is InterfaceGen) {
					return true;
				}
			}
			return false;
		}
		
		public static ClassBase GetClassGen(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			return (complex_types[c_type] as ClassBase);
		}
			
		public static bool IsObject(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				if (gen is ObjectGen) {
					return true;
				}
			}
			return false;
		}

		public static bool IsManuallyWrapped(string c_type)
		{
			c_type = Trim(c_type);
			c_type = DeAlias(c_type);
			return manually_wrapped_types.ContainsKey(c_type);
		}

	}
}

