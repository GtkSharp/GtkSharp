// GtkSharp.Generation.BoxedGen.cs - The Boxed Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class BoxedGen : StructBase, IGeneratable  {
		
		public BoxedGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public void Generate ()
		{
			GenerationInfo gen_info = new GenerationInfo (NSElem);
			Generate (gen_info);
		}		
		
		public override void Generate (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (Name);
			base.Generate (gen_info);
			sw.WriteLine ("\t\t[DllImport(\"gtksharpglue\")]");
			sw.WriteLine ("\t\tstatic extern IntPtr gtksharp_value_create (GLib.GType gtype);");
			sw.WriteLine ();
			sw.WriteLine ("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.WriteLine ("\t\tstatic extern IntPtr g_value_get_boxed (IntPtr handle);");
			sw.WriteLine ();
			sw.WriteLine ("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.WriteLine ("\t\tstatic extern void g_value_set_boxed (IntPtr handle, ref " + QualifiedName + " boxed);");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static explicit operator GLib.Value (" + QualifiedName + " boxed)");
			sw.WriteLine ("\t\t{");

			sw.WriteLine ("\t\t\tIntPtr handle = gtksharp_value_create (" + QualifiedName + ".GType);");
			sw.WriteLine ("\t\t\tg_value_set_boxed (handle, ref boxed);");
			sw.WriteLine ("\t\t\treturn new GLib.Value (handle, IntPtr.Zero);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static explicit operator " + QualifiedName + " (GLib.Value val)");
			sw.WriteLine ("\t\t{");

			sw.WriteLine ("\t\t\tIntPtr boxed_ptr = g_value_get_boxed (val.Handle);");
			sw.WriteLine ("\t\t\treturn New (boxed_ptr);");
			sw.WriteLine ("\t\t}");

			sw.WriteLine ("#endregion");
                        AppendCustom(sw, gen_info.CustomDir);
                        sw.WriteLine ("\t}");
                        sw.WriteLine ("}");
			sw.Close ();
			gen_info.Writer = null;
			Statistics.BoxedCount++;
		}		
	}
}

