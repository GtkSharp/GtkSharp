// GtkSharp.Generation.StructField.cs - The Structure Field generation
// Class.
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

	public class StructField : FieldBase {

		public static int bitfields;

		public StructField (XmlElement elem, ClassBase container_type) : base (elem, container_type) {}

		protected override string DefaultAccess {
			get {
				return "public";
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

		public new string CSType {
			get {
				string type = base.CSType;
				if (IsArray)
					type += "[]";
				else if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && type != "string")
					type = "IntPtr";
				else if (SymbolTable.Table.IsCallback (CType))
					type = "IntPtr";

				return type;
			}
		}

		public bool IsPadding {
			get {
				return (CName.StartsWith ("dummy") || CName.StartsWith ("padding"));
			}
		}

		public bool IsPointer {
			get {
				return (CType.EndsWith ("*") || CType.EndsWith ("pointer"));
			}
		}

		public new string Name {
			get {
				string result = "";
				if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && CSType != "string")
					result = "_";
				result += SymbolTable.Table.MangleName (CName);

				return result;
			}
		}

		public string StudlyName {
			get {
				string studly = base.Name;
				if (studly != "")
					return studly;

				// FIXME: this is backward compatibility for API files
				// output by older versions of the parser. It can go
				// away at some point.
				string name = CName;
				string[] segs = name.Split('_');
				foreach (string s in segs) {
					if (s.Trim () == "")
						continue;
					studly += (s.Substring(0,1).ToUpper() + s.Substring(1));
				}
				return studly;
			}
		}

		public override void Generate (GenerationInfo gen_info, string indent)
		{
			if (Hidden)
				return;

			StreamWriter sw = gen_info.Writer;
			SymbolTable table = SymbolTable.Table;

			if (IsArray) 
				sw.WriteLine (indent + "[MarshalAs (UnmanagedType.ByValArray, SizeConst=" + ArrayLength + ")]");

			string wrapped = table.GetCSType (CType);
			string wrapped_name = SymbolTable.Table.MangleName (CName);
			IGeneratable gen = table [CType];

			if (IsArray) {
				sw.WriteLine (indent + "{0} {1} {2};", Access, CSType, StudlyName);
			} else if (IsPadding) {
				sw.WriteLine (indent + "private {0} {1};", CSType, Name);
			} else if (IsBitfield) {
				base.Generate (gen_info, indent);
			} else if (table.IsCallback (CType)) {
				// FIXME
				sw.WriteLine (indent + "private {0} {1};", CSType, Name);
			} else if (gen is LPGen || gen is LPUGen) {
				sw.WriteLine (indent + "private " + gen.MarshalType + " " + Name + ";");
				sw.WriteLine (indent + "public " + CSType + " " + StudlyName + " {");
				sw.WriteLine (indent + "\tget {");
				sw.WriteLine (indent + "\t\treturn " + gen.FromNative (Name) + ";");
				sw.WriteLine (indent + "\t}");
				sw.WriteLine (indent + "\tset {");
				sw.WriteLine (indent + "\t\t" + Name + " = " + gen.CallByName ("value") + ";");
				sw.WriteLine (indent + "\t}");
				sw.WriteLine (indent + "}");
			} else if (table.IsObject (CType) || table.IsOpaque (CType)) {
				sw.WriteLine (indent + "private {0} {1};", CSType, Name);

				if (Access != "private") {
					sw.WriteLine (indent + Access + " " + wrapped + " " + wrapped_name + " {");
					sw.WriteLine (indent + "\tget { ");
					sw.WriteLine (indent + "\t\treturn " + table.FromNativeReturn(CType, Name) + ";");
					sw.WriteLine (indent + "\t}");

					sw.WriteLine (indent + "\tset { " + Name + " = " + table.CallByName (CType, "value") + "; }");
					sw.WriteLine (indent + "}");
				}
			} else if (IsPointer && (table.IsStruct (CType) || table.IsBoxed (CType))) {
				sw.WriteLine (indent + "private {0} {1};", CSType, Name);
				sw.WriteLine ();
				if (Access != "private") {
					sw.WriteLine (indent + Access + " " + wrapped + " " + wrapped_name + " {");
					sw.WriteLine (indent + "\tget { return " + table.FromNativeReturn (CType, Name) + "; }");
					sw.WriteLine (indent + "}");
				}
			} else if (IsPointer && CSType != "string") {
				// FIXME: probably some fields here which should be visible.
				sw.WriteLine (indent + "private {0} {1};", CSType, Name);
			} else if (Access != "public") {
				sw.WriteLine (indent + "{0} {1} {2};", Access, CSType, Name);
			} else {
				sw.WriteLine (indent + "public {0} {1};", CSType, StudlyName);
			}
		}
	}
}

