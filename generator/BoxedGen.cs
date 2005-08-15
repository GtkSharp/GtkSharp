// GtkSharp.Generation.BoxedGen.cs - The Boxed Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2003-2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class BoxedGen : StructBase {
		
		public BoxedGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public override void Generate (GenerationInfo gen_info)
		{
			Method copy = methods["Copy"] as Method;
			methods.Remove ("Copy");
			methods.Remove ("Free");

			gen_info.CurrentType = Name;

			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (Name);
			base.Generate (gen_info);
			sw.WriteLine ("\t\t[DllImport(\"glibsharpglue-2\")]");
			sw.WriteLine ("\t\tstatic extern IntPtr glibsharp_value_get_boxed (ref GLib.Value val);");
			sw.WriteLine ();
			sw.WriteLine ("\t\t[DllImport(\"glibsharpglue-2\")]");
			sw.WriteLine ("\t\tstatic extern void glibsharp_value_set_boxed (ref GLib.Value val, ref " + QualifiedName + " boxed);");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static explicit operator GLib.Value (" + QualifiedName + " boxed)");
			sw.WriteLine ("\t\t{");

			sw.WriteLine ("\t\t\tGLib.Value val = GLib.Value.Empty;");
			sw.WriteLine ("\t\t\tval.Init (" + QualifiedName + ".GType);");
			sw.WriteLine ("\t\t\tglibsharp_value_set_boxed (ref val, ref boxed);");
			sw.WriteLine ("\t\t\treturn val;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static explicit operator " + QualifiedName + " (GLib.Value val)");
			sw.WriteLine ("\t\t{");

			sw.WriteLine ("\t\t\tIntPtr boxed_ptr = glibsharp_value_get_boxed (ref val);");
			sw.WriteLine ("\t\t\treturn New (boxed_ptr);");
			sw.WriteLine ("\t\t}");

			if (copy != null && copy.IsDeprecated) {
				sw.WriteLine ();
				sw.WriteLine ("\t\t[Obsolete(\"This is a no-op\")]");
				sw.WriteLine ("\t\tpublic " + QualifiedName + " Copy() {");
				sw.WriteLine ("\t\t\treturn this;");
				sw.WriteLine ("\t\t}");
			}

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

