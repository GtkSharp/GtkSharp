// GtkSharp.Generation.SymbolTable.cs - The Symbol Table Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;

	public class SymbolTable {
		
		private Hashtable complex_types = new Hashtable ();
		private Hashtable simple_types;
		
		public SymbolTable ()
		{
			simple_types = new Hashtable ();
			simple_types.Add ("gboolean", "bool");
			simple_types.Add ("gint", "int");
			simple_types.Add ("guint", "uint");
			simple_types.Add ("glong", "long");
			simple_types.Add ("gshort", "short");
			simple_types.Add ("guint32", "uint");
			simple_types.Add ("const-gchar", "String");
			simple_types.Add ("gchar", "String");
			simple_types.Add ("GObject", "GLib.Object");
			simple_types.Add ("gfloat", "float");
			simple_types.Add ("gdouble", "double");
			simple_types.Add ("gint8", "byte");
			simple_types.Add ("guint8", "byte");
			simple_types.Add ("gint16", "short");
			simple_types.Add ("gint32", "int");
			simple_types.Add ("guint16", "ushort");
			simple_types.Add ("guint1", "bool");
			simple_types.Add ("gpointer", "IntPtr");
			simple_types.Add ("guchar", "byte");
			simple_types.Add ("GValue", "GLib.Value");
			simple_types.Add ("GtkType", "int");
			simple_types.Add ("long", "long");
			simple_types.Add ("gulong", "ulong");
			simple_types.Add ("GQuark", "int");
			simple_types.Add ("int", "int");
			simple_types.Add ("char", "char");
			simple_types.Add ("double", "double");
			simple_types.Add ("gunichar", "String");
			simple_types.Add ("uint1", "bool");
			
			// FIXME: These ought to be handled properly.
			simple_types.Add ("GList", "IntPtr");
			simple_types.Add ("GSList", "IntPtr");
			simple_types.Add ("GHashTable", "IntPtr");
			simple_types.Add ("va_list", "IntPtr");
			simple_types.Add ("GParamSpec", "IntPtr");
		}
		
		public void AddType (IGeneratable gen)
		{
			complex_types [gen.CName] = gen;
		}
		
		public int Count {
			get
			{
				return complex_types.Count;
			}
		}
		
		public IDictionaryEnumerator GetEnumerator ()
		{
			return complex_types.GetEnumerator();
		}
		
		public String GetCSType (String c_type)
		{
			if (simple_types.ContainsKey(c_type)) {
				return (String) simple_types [c_type];
			} else if (complex_types.ContainsKey(c_type)) {
				IGeneratable gen = (IGeneratable) complex_types[c_type];
				return gen.QualifiedName;
			} else {
				return "";
			}
		}
		
	}
}

