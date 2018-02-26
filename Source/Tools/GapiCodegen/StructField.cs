// GtkSharp.Generation.StructField.cs - The Structure Field generation
// Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2004-2005 Novell, Inc.
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
				if (IsPadding)
					return "private";

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
				} catch (Exception) {
					LogWriter log = new LogWriter (container_type.Name + "." + Name);

					log.Warn("Non-numeric array_len: \"" + elem.GetAttribute("array_len") +
							 "\" incorrectly generated");
					result = 0;
				}
				return result;
			}
		}

		public bool IsNullTermArray {
			get { return elem.GetAttributeAsBoolean ("null_term_array"); }
		}

		public new string CSType {
			get {
				string type = base.CSType;
				if (IsArray)
					type += "[]";
				else if ((IsPointer || SymbolTable.Table.IsOpaque (CType)) && type != "string")
					type = "IntPtr";

				return type;
			}
		}

		bool visible = false;
		internal bool Visible {
			get {
				return visible;
			}
		}

		public string EqualityName {
			get {
				SymbolTable table = SymbolTable.Table;
				string wrapped_name = SymbolTable.Table.MangleName (CName);
				IGeneratable gen = table [CType];

				if (IsArray || gen is IAccessor)
					return Access == "public" ? StudlyName : Name;
				else if (IsBitfield)
					return Name;
				else if (IsPointer && (gen is StructGen || gen is BoxedGen || gen is UnionGen))
					return Access != "private" ? wrapped_name : Name;
				else if (IsPointer && CSType != "string")
					return Name;
				else 
					return Access == "public" ? StudlyName : Name;
			}
		}

		bool IsFixedSizeArray() {
			return IsArray && !IsNullTermArray && ArrayLength != 0;
		}

		public virtual bool IsCPointer() {
			IGeneratable gen = SymbolTable.Table[CType];

			return (CType.EndsWith("*") ||
						CType.EndsWith ("pointer") ||
						gen is CallbackGen ||
						cstype == "string" ||
						(CType == "guint8" && (IsArray && IsNullTermArray)) ||
						elem.GetAttributeAsBoolean("is_callback"));

		}

		public virtual string GenerateGetSizeOf(string indent) {
			string cstype = SymbolTable.Table.GetCSType(CType, true);
			string res = "";
			IGeneratable gen = SymbolTable.Table[CType];
			var is_pointer = false;

			if (IsCPointer()) {
				is_pointer = true;
				cstype = "IntPtr";
			} else if (gen != null) {
				res = gen.GenerateGetSizeOf();
			}

			if (res != null && res != "") {
				if (IsFixedSizeArray())
					res += " * " + ArrayLength;

				return indent + res;
			}

			var _enum = gen as EnumGen;
			if (_enum != null && !is_pointer)
				res = "(uint) Marshal.SizeOf(System.Enum.GetUnderlyingType(typeof(" + cstype + ")))";
			else
				res = "(uint) Marshal.SizeOf(typeof(" + cstype + "))";

			if (IsFixedSizeArray())
				res += " * " + ArrayLength;

			return res;
		}

		public bool IsPadding {
			get {
				if (elem.GetAttributeAsBoolean ("is-padding"))
					return elem.GetAttributeAsBoolean ("is-padding");

				return (elem.GetAttribute ("access") == "private" && (
					CName.StartsWith ("dummy") || CName.StartsWith ("padding")));
			}
		}

		public bool IsPointer {
			get {
				return IsCPointer();
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

		public virtual string StudlyName {
			get {
				string studly = base.Name;
				if (studly == "")
					throw new Exception (CName + "API file must be regenerated with a current version of the GAPI parser. It is incompatible with this version of the GAPI code generator.");

				return studly;
			}
		}

		public override void Generate (GenerationInfo gen_info, string indent)
		{
			Generate(gen_info, indent, false, gen_info.Writer);
		}

		public void Generate (GenerationInfo gen_info, string indent, bool use_cnames,
				TextWriter sw)
		{
			if (Hidden && !use_cnames)
				return;

			visible = Access != "private";

			SymbolTable table = SymbolTable.Table;

			string wrapped = table.GetCSType (CType);

			string wrapped_name = SymbolTable.Table.MangleName (CName);
			string name = Name;
			string studly_name = StudlyName;
			string cstype = CSType;

			IGeneratable gen = table [CType];

			if (use_cnames) {
				name = studly_name = wrapped_name = SymbolTable.Table.MangleName (CName).Replace(".", "_");

				var mangen = gen as ManualGen;
				if (mangen != null) {
					if (mangen.AbiType != null)
						cstype = mangen.AbiType;
				}

				if (IsCPointer())
					cstype = "IntPtr";
			}

			if (IsArray && !IsNullTermArray) {
				sw.WriteLine (indent + "[MarshalAs (UnmanagedType.ByValArray, SizeConst=" + ArrayLength + ")]");
				sw.WriteLine (indent + "{0} {1} {2};", Access, cstype, studly_name);
			} else if (IsArray && IsNullTermArray) {
				sw.WriteLine (indent + "private {0} {1};", "IntPtr", studly_name+ "Ptr");
				if ((Readable || Writable) && Access == "public") {
					sw.WriteLine (indent + "public {0} {1} {{", cstype, studly_name);
					if (Readable)
						sw.WriteLine (indent + "\tget {{ return GLib.Marshaller.StructArrayFromNullTerminatedIntPtr<{0}> ({1}); }}",
									  base.CSType, studly_name + "Ptr");
					if (Writable)
						sw.WriteLine (indent + "\tset {{ {0} = GLib.Marshaller.StructArrayToNullTerminatedStructArrayIntPtr<{1}> (value); }}",
									  studly_name + "Ptr", base.CSType);
					sw.WriteLine (indent + "}");
				}
			} else if (IsBitfield) {
				base.Generate (gen_info, indent);
			} else if (gen is IAccessor) {
				sw.WriteLine (indent + "private {0} {1};", gen.MarshalType, name);

				if (Access != "private") {
					IAccessor acc = table [CType] as IAccessor;
					sw.WriteLine (indent + Access + " " + wrapped + " " + studly_name + " {");
					acc.WriteAccessors (sw, indent + "\t", name);
					sw.WriteLine (indent + "}");
				}
			} else if (IsPointer && (gen is StructGen || gen is BoxedGen || gen is UnionGen)) {
				sw.WriteLine (indent + "private {0} {1};", cstype, name);
				sw.WriteLine ();
				if (Access != "private") {
					sw.WriteLine (indent + Access + " " + wrapped + " " + wrapped_name + " {");
					sw.WriteLine (indent + "\tget { return " + table.FromNative (CType, name) + "; }");
					sw.WriteLine (indent + "}");
				}
			} else if (IsPointer && cstype != "string") {
				// FIXME: probably some fields here which should be visible.
				visible = false;
				sw.WriteLine (indent + "private {0} {1};", cstype, name);
			} else {
				sw.WriteLine (indent + "{0} {1} {2};", Access, cstype, Access == "public" ? studly_name : name);
			}
		}
	}
}



