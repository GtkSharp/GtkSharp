// GtkSharp.Generation.FieldBase.cs - base class for struct and object
// fields
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
	using System.Collections;
	using System.IO;
	using System.Xml;

	public abstract class FieldBase : PropertyBase {
		public FieldBase abi_field = null;
		string getterName, setterName;
		protected string getOffsetName, offsetName;

		public FieldBase (XmlElement elem, ClassBase container_type) : base (elem, container_type) {}
		public FieldBase (XmlElement elem, ClassBase container_type, FieldBase abi_field) : base (elem, container_type) {
			abi_field = abi_field;
		}


		public virtual bool Validate (LogWriter log)
		{
			log.Member = Name;
			if (!Ignored && !Hidden && CSType == "") {
				if (Name == "Priv")
					return false;
				log.Warn ("field has unknown type: " + CType);
				Statistics.ThrottledCount++;
				return false;
			}

			return true;
		}

		internal virtual bool Readable {
			get {
				if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) <= 2)
					return elem.GetAttribute ("readable") != "false";
				return elem.HasAttribute ("readable") && elem.GetAttributeAsBoolean ("readable");
			}
		}

		internal virtual bool Writable {
			get {
				if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) <= 2)
					return elem.GetAttribute ("writeable") != "false";
				return elem.HasAttribute ("writeable") && elem.GetAttributeAsBoolean ("writeable");
			}
		}

		protected abstract string DefaultAccess { get; }

		protected virtual string Access {
			get {
				return elem.HasAttribute ("access") ? elem.GetAttribute ("access") : DefaultAccess;
			}
		}

		public bool IsArray {
			get {
				return elem.HasAttribute ("array_len") || elem.GetAttributeAsBoolean ("array");
			}
		}

		public bool IsBitfield {
			get {
				return elem.HasAttribute("bits");
			}
		}

		public bool Ignored {
			get {
				if (container_type.GetProperty (Name) != null)
					return true;
				if (IsArray)
					return true;
				if (Access == "private" && (Getter == null) && (Setter == null))
					return true;
				return false;
			}
		}

		private bool UseABIStruct(GenerationInfo gen_info) {
			if (!container_type.CanGenerateABIStruct(new LogWriter(container_type.CName)))
				return false;

			return (abi_field != null && abi_field.getOffsetName != null &&
						gen_info.GlueWriter == null);
		}

		void CheckGlue (GenerationInfo gen_info)
		{
			getterName = setterName = getOffsetName = null;
			if (Access != "public")
				return;

			if (UseABIStruct(gen_info)) {
				getOffsetName = abi_field.getOffsetName;
				offsetName = ((StructABIField) abi_field).abi_info_name + ".GetFieldOffset(\"" + ((StructField)abi_field).CName + "\")";

				return;
			}

			if (gen_info.GlueWriter == null)
				return;

			string prefix = (container_type.NS + "Sharp_" + container_type.NS + "_" + container_type.Name).Replace(".", "__").ToLower ();

			if (IsBitfield) {
				if (Readable && Getter == null)
					getterName = prefix + "_get_" + CName;
				if (Writable && Setter == null)
					setterName = prefix + "_set_" + CName;
			} else {
				if ((Readable && Getter == null) || (Writable && Setter == null)) {
					offsetName = CName + "_offset";
					getOffsetName = prefix + "_get_" + offsetName;
				}
			}
		}

		protected override void GenerateImports (GenerationInfo gen_info, string indent)
		{
			StreamWriter sw = gen_info.Writer;
			SymbolTable table = SymbolTable.Table;

			if (gen_info.GlueWriter == null) {
				base.GenerateImports(gen_info, indent);
				return;
			}

			if (getterName != null) {
				sw.WriteLine (indent + "[DllImport (\"{0}\")]", gen_info.GluelibName);
				sw.WriteLine (indent + "extern static {0} {1} ({2} raw);",
						  table.GetMarshalType (CType), getterName,
						  container_type.MarshalType);
			}

			if (setterName != null) {
				sw.WriteLine (indent + "[DllImport (\"{0}\")]", gen_info.GluelibName);
				sw.WriteLine (indent + "extern static void {0} ({1} raw, {2} value);",
						  setterName, container_type.MarshalType, table.GetMarshalType (CType));
			}

			if (getOffsetName != null) {
				sw.WriteLine (indent + "[DllImport (\"{0}\")]", gen_info.GluelibName);
				sw.WriteLine (indent + "extern static uint {0} ();", getOffsetName);
				sw.WriteLine ();
				sw.WriteLine (indent + "static uint " + offsetName + " = " + getOffsetName + " ();");
			}

			base.GenerateImports (gen_info, indent);
		}

		public virtual void Generate (GenerationInfo gen_info, string indent)
		{
			if (Ignored || Hidden)
				return;

			CheckGlue (gen_info);

			GenerateImports (gen_info, indent);

			if (Getter == null && getterName == null && offsetName == null &&
					Setter == null && setterName == null) {
				return;
			}


			SymbolTable table = SymbolTable.Table;
			IGeneratable gen = table [CType];
			StreamWriter sw = gen_info.Writer;
			string modifiers = elem.GetAttributeAsBoolean ("new_flag") ? "new " : "";

			sw.WriteLine (indent + "public " + modifiers + CSType + " " + Name + " {");

			if (Getter != null) {
				sw.Write (indent + "\tget ");
				Getter.GenerateBody (gen_info, container_type, "\t");
				sw.WriteLine ("");
			} else if (getterName != null) {
				sw.WriteLine (indent + "\tget {");
				container_type.Prepare (sw, indent + "\t\t");
				sw.WriteLine (indent + "\t\t" + CSType + " result = " + table.FromNative (ctype, getterName + " (" + container_type.CallByName () + ")") + ";");
				container_type.Finish (sw, indent + "\t\t");
				sw.WriteLine (indent + "\t\treturn result;");
				sw.WriteLine (indent + "\t}");
			} else if (Readable && offsetName != null) {
				sw.WriteLine (indent + "\tget {");
				sw.WriteLine (indent + "\t\tunsafe {");
				if (gen is CallbackGen) {
					sw.WriteLine (indent + "\t\t\tIntPtr* raw_ptr = (IntPtr*)(((byte*)" + container_type.CallByName () + ") + " + offsetName + ");");
					sw.WriteLine (indent + "\t\t\t {0} del = ({0})Marshal.GetDelegateForFunctionPointer(*raw_ptr, typeof({0}));", table.GetMarshalType (CType));
					sw.WriteLine (indent + "\t\t\treturn " + table.FromNative (ctype, "(del)") + ";");
				}
				else {
					sw.WriteLine (indent + "\t\t\t" + table.GetMarshalType (CType) + "* raw_ptr = (" + table.GetMarshalType (CType) + "*)(((byte*)" + container_type.CallByName () + ") + " + offsetName + ");");
					sw.WriteLine (indent + "\t\t\treturn " + table.FromNative (ctype, "(*raw_ptr)") + ";");
				}
				sw.WriteLine (indent + "\t\t}");
				sw.WriteLine (indent + "\t}");
			}

			string to_native = (gen is IManualMarshaler) ? (gen as IManualMarshaler).AllocNative ("value") : gen.CallByName ("value");

			if (Setter != null) {
				sw.Write (indent + "\tset ");
				Setter.GenerateBody (gen_info, container_type, "\t");
				sw.WriteLine ("");
			} else if (setterName != null) {
				sw.WriteLine (indent + "\tset {");
				container_type.Prepare (sw, indent + "\t\t");
				sw.WriteLine (indent + "\t\t" + setterName + " (" + container_type.CallByName () + ", " + to_native + ");");
				container_type.Finish (sw, indent + "\t\t");
				sw.WriteLine (indent + "\t}");
			} else if (Writable && offsetName != null) {
				sw.WriteLine (indent + "\tset {");
				sw.WriteLine (indent + "\t\tunsafe {");
				if (gen is CallbackGen) {
					sw.WriteLine (indent + "\t\t\t{0} wrapper = new {0} (value);", ((CallbackGen)gen).WrapperName);
					sw.WriteLine (indent + "\t\t\tIntPtr* raw_ptr = (IntPtr*)(((byte*)" + container_type.CallByName () + ") + " + offsetName + ");");
					sw.WriteLine (indent + "\t\t\t*raw_ptr = Marshal.GetFunctionPointerForDelegate (wrapper.NativeDelegate);");
				}
				else {
					sw.WriteLine (indent + "\t\t\t" + table.GetMarshalType (CType) + "* raw_ptr = (" + table.GetMarshalType (CType) + "*)(((byte*)" + container_type.CallByName () + ") + " + offsetName + ");");
					sw.WriteLine (indent + "\t\t\t*raw_ptr = " + to_native + ";");
				}
				sw.WriteLine (indent + "\t\t}");
				sw.WriteLine (indent + "\t}");
			}

			sw.WriteLine (indent + "}");
			sw.WriteLine ("");

			if ((getterName != null || setterName != null || getOffsetName != null) && gen_info.GlueWriter != null)
				GenerateGlue (gen_info);
		}

		protected void GenerateGlue (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.GlueWriter;
			SymbolTable table = SymbolTable.Table;

			string FieldCType = CType.Replace ("-", " ");
			bool byref = table[CType] is ByRefGen || table[CType] is StructGen;
			string GlueCType = byref ? FieldCType + " *" : FieldCType;
			string ContainerCType = container_type.CName;
			string ContainerCName = container_type.Name.ToLower ();

			if (getterName != null) {
				sw.WriteLine ("{0} {1} ({2} *{3});",
					      GlueCType, getterName, ContainerCType, ContainerCName);
			}
			if (setterName != null) {
				sw.WriteLine ("void {0} ({1} *{2}, {3} value);",
					      setterName, ContainerCType, ContainerCName, GlueCType);
			}
			if (getOffsetName != null)
				sw.WriteLine ("guint {0} (void);", getOffsetName);
			sw.WriteLine ("");

			if (getterName != null) {
				sw.WriteLine (GlueCType);
				sw.WriteLine ("{0} ({1} *{2})", getterName, ContainerCType, ContainerCName);
				sw.WriteLine ("{");
				sw.WriteLine ("\treturn ({0}){1}{2}->{3};", GlueCType,
					      byref ? "&" : "", ContainerCName, CName);
				sw.WriteLine ("}");
				sw.WriteLine ("");
			}
			if (setterName != null) {
				sw.WriteLine ("void");
				sw.WriteLine ("{0} ({1} *{2}, {3} value)",
					      setterName, ContainerCType, ContainerCName, GlueCType);
				sw.WriteLine ("{");
				sw.WriteLine ("\t{0}->{1} = ({2}){3}value;", ContainerCName, CName,
					      FieldCType, byref ? "*" : "");
				sw.WriteLine ("}");
				sw.WriteLine ("");
			}
			if (getOffsetName != null) {
				sw.WriteLine ("guint");
				sw.WriteLine ("{0} (void)", getOffsetName);
				sw.WriteLine ("{");
				sw.WriteLine ("\treturn (guint)G_STRUCT_OFFSET ({0}, {1});",
					      ContainerCType, CName);
				sw.WriteLine ("}");
				sw.WriteLine ("");
			}
		}
	}
}

