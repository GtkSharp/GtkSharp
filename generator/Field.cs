// GtkSharp.Generation.Field.cs - The Field generation Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
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

	public class Field {

		public static int bitfields;

		XmlElement elem;

		public Field (XmlElement elem)
		{
			this.elem = elem;
		}

		public string Access {
			get {
				return elem.HasAttribute ("access") ? elem.GetAttribute ("access") : "public";
			}
		}

		public int ArrayLength {
			get {
				if (!IsArray)
					return 0;
				
				int result;
				try {
					result = Int32.Parse (elem.GetAttribute("array_len"));
				} catch (Exception e) {
					Console.Write ("Non-numeric array_len: " + elem.GetAttribute("array_len"));
					Console.WriteLine (" warning: array field {0} incorrectly generated", Name);
					result = 0;
				}
				return result;
			}
		}

		public string CSType {
			get {
				string type = SymbolTable.Table.GetCSType (CType);
				if (IsArray)
					type += "[]";
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

		public bool Hidden {
			get {
				return elem.HasAttribute("hidden");
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
				return (c_name.StartsWith ("dummy") || c_name.StartsWith ("padding"));
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

		public string StudlyName {
			get {
				string studly = elem.GetAttribute ("name");
				if (studly != "")
					return studly;

				// FIXME: this is backward compatibility for API files
				// output by older versions of the parser. It can go
				// away at some point.
				string name = elem.GetAttribute ("cname");
				string[] segs = name.Split('_');
				foreach (string s in segs) {
					if (s.Trim () == "")
						continue;
					studly += (s.Substring(0,1).ToUpper() + s.Substring(1));
				}
				return studly;
			}
		}

		public bool Validate ()
		{
			if (CSType == "" && !Hidden) {
				Console.Write ("Field has unknown Type {0} ", CType);
				Statistics.ThrottledCount++;
				return false;
			}
			return true;
		}

		public void Generate (StreamWriter sw)
		{
			if (Hidden)
				return;

			SymbolTable table = SymbolTable.Table;

			if (IsArray) 
				sw.WriteLine ("\t\t[MarshalAs (UnmanagedType.ByValArray, SizeConst=" + ArrayLength + ")]");

			string wrapped = table.GetCSType (CType);
			string wrapped_name = SymbolTable.Table.MangleName (elem.GetAttribute ("cname"));
			IGeneratable gen = table [CType];

			if (IsArray) {
				sw.WriteLine ("\t\t{0} {1} {2};", Access, CSType, StudlyName);
			} else if (IsPadding) {
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);
			} else if (IsBit) {
				// FIXME
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);
			} else if (table.IsCallback (CType)) {
				// FIXME
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);
			} else if (gen is LPGen || gen is LPUGen) {
				sw.WriteLine ("\t\tprivate " + gen.MarshalType + " " + Name + ";");
				sw.WriteLine ("\t\tpublic " + CSType + " " + StudlyName + " {");
				sw.WriteLine ("\t\t\tget {");
				sw.WriteLine ("\t\t\t\treturn " + gen.FromNative (Name) + ";");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t\tset {");
				sw.WriteLine ("\t\t\t\t" + Name + " = " + gen.CallByName ("value") + ";");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
			} else if (table.IsObject (CType) || table.IsOpaque (CType)) {
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);

				if (Access != "private") {
					sw.WriteLine ("\t\t" + Access + " " + wrapped + " " + wrapped_name + " {");
					sw.WriteLine ("\t\t\tget { ");
					sw.WriteLine ("\t\t\t\t" + wrapped + " ret = " + table.FromNativeReturn(CType, Name) + ";");
					if (table.IsOpaque (CType))
						sw.WriteLine ("\t\t\t\tif (ret == null) ret = new " + wrapped + "(" + Name + ");");
					sw.WriteLine ("\t\t\t\treturn ret;");
					sw.WriteLine ("\t\t\t}");

					sw.WriteLine ("\t\t\tset { " + Name + " = " + table.CallByName (CType, "value") + "; }");
					sw.WriteLine ("\t\t}");
				}
			} else if (IsPointer && (table.IsStruct (CType) || table.IsBoxed (CType))) {
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);
				sw.WriteLine ();
				if (Access != "private") {
					sw.WriteLine ("\t\t" + Access + " " + wrapped + " " + wrapped_name + " {");
					sw.WriteLine ("\t\t\tget { return " + table.FromNativeReturn (CType, Name) + "; }");
					sw.WriteLine ("\t\t}");
				}
			} else if (IsPointer && CSType != "string") {
				// FIXME: probably some fields here which should be visible.
				sw.WriteLine ("\t\tprivate {0} {1};", CSType, Name);
			} else if (Access != "public") {
				sw.WriteLine ("\t\t{0} {1} {2};", Access, CSType, Name);
			} else {
				sw.WriteLine ("\t\tpublic {0} {1};", CSType, StudlyName);
			}
		}
	}
}

