// GtkSharp.Generation.Field.cs - The Field generation Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// (c) 2004 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class Field {

		public static int bitfields;

		XmlElement elem;

		public Field (XmlElement elem)
		{
			this.elem = elem;
		}

		public string CSType {
			get {
				string type = SymbolTable.Table.GetCSType (CType);
				if (IsArray)
					// FIXME
					type = "IntPtr";
				else if (IsBit)
					type = "uint";
				else if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && type != "string")
					type = "IntPtr";
				else if (SymbolTable.Table.IsCallback (CType))
					type = "IntPtr";

				return type;
			}
		}

		public string CType {
			get {
				return elem.GetAttribute ("type");
			}
		}

		public bool IsArray {
			get {
				return elem.HasAttribute("array_len");
			}
		}

		public bool IsBit {
			get {
				return (elem.HasAttribute("bits") && (elem.GetAttribute("bits") == "1"));
			}
		}

		public bool IsPadding {
			get {
				string c_name = elem.GetAttribute ("cname");
				return (c_name.StartsWith ("dummy"));
			}
		}

		public bool IsPointer {
			get {
				return (CType.EndsWith ("*") || CType.EndsWith ("pointer"));
			}
		}

		public string Name {
			get {
				string result = "";
				if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && CSType != "string")
					result = "_";

				if (IsBit)
					result = String.Format ("_bitfield{0}", bitfields++);
				else
					result += SymbolTable.Table.MangleName (elem.GetAttribute ("cname"));

				return result;
			}
		}

		public string Protection {
			get {
				if (IsArray)
					// FIXME
					return "private";
				else if (IsBit || IsPadding || SymbolTable.Table.IsCallback (CType))
					return "private";
				else if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && CSType != "string")
					return "private";
				else
					return "public";
			}
		}

		public bool Generate (StreamWriter sw)
		{
			if (CSType == "") {
				Console.WriteLine ("Field has unknown Type {0}", CType);
				Statistics.ThrottledCount++;
				return false;
			}

			SymbolTable table = SymbolTable.Table;

			// FIXME
			if (IsArray)
				Console.WriteLine ("warning: array field {0} probably incorrectly generated", Name);
			sw.WriteLine ("\t\t{0} {1} {2};", Protection, CSType, table.MangleName (Name));


			string wrapped = table.GetCSType (CType);
			string wrapped_name = SymbolTable.Table.MangleName (elem.GetAttribute ("cname"));
			if (table.IsObject (CType)) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { ");
				sw.WriteLine ("\t\t\t\t" + wrapped + " ret = " + table.FromNativeReturn(CType, Name) + ";");
				sw.WriteLine ("\t\t\t\treturn ret;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t\tset { " + Name + " = " + table.CallByName (CType, "value") + "; }");
				sw.WriteLine ("\t\t}");
			} else if (table.IsOpaque (CType)) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { ");
				sw.WriteLine ("\t\t\t\t" + wrapped + " ret = " + table.FromNativeReturn(CType, Name) + ";");
				sw.WriteLine ("\t\t\t\tif (ret == null) ret = new " + wrapped + "(" + Name + ");");
				sw.WriteLine ("\t\t\t\treturn ret;");
				sw.WriteLine ("\t\t\t}");

				sw.WriteLine ("\t\t\tset { " + Name + " = " + table.CallByName (CType, "value") + "; }");
				sw.WriteLine ("\t\t}");
			} else if (IsPointer && (table.IsStruct (CType) || table.IsBoxed (CType))) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { return " + table.FromNativeReturn (CType, Name) + "; }");
				sw.WriteLine ("\t\t}");
			}
		
			return true;
		}
	}
}

