// GtkSharp.Generation.StructBase.cs - The Structure/Object Base Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;

	public class StructBase : ClassBase {
	
		ArrayList fields = new ArrayList ();
		uint bitfields;

		public StructBase (XmlElement ns, XmlElement elem) : base (ns, elem)
		{
			hasDefaultConstructor = false;

			foreach (XmlNode node in elem.ChildNodes) {

				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					fields.Add (member);
					break;

				case "callback":
					Statistics.IgnoreCount++;
					break;

				default:
					if (!IsNodeNameHandled (node.Name))
						Console.WriteLine ("Unexpected node " + node.Name + " in " + CName);
					break;
				}
			}
		}

		public override String MarshalType {
			get
			{
				return "ref " + QualifiedName;
			}
		}

		public override String MarshalReturnType {
			get
			{
				return "IntPtr";
			}
		}

		public override String CallByName (String var_name)
		{
			return "ref " + var_name;
		}

		public override String CallByName ()
		{
			return "ref this";
		}

		public override String AssignToName {
			get { return "raw"; }
		}

		public override String FromNative(String var)
		{
			return var;
		}
		
		public override String FromNativeReturn(String var)
		{
			return QualifiedName + ".New (" + var + ")";
		}

		public override String ToNativeReturn(String var)
		{
			// FIXME
			return var;
		}

		bool IsBit (XmlElement field)
		{
			return (field.HasAttribute("bits") && (field.GetAttribute("bits") == "1"));
		}

		bool IsPointer (XmlElement field)
		{
			string c_type = field.GetAttribute("type");
			return (c_type[c_type.Length - 1] == '*');
		}

		protected void GenFields (StreamWriter sw)
		{
			bitfields = 0;
			bool need_field = true;
			foreach (XmlElement field in fields) {
				if (IsBit (field)) {
					if (need_field)
						need_field = false;
					else
						continue;
				} else
					need_field = true;
				GenField (field, sw);	
			}
		}

		protected bool GetFieldInfo (XmlElement field, out string c_type, out string type, out string name)
		{
			name = "";
			c_type = field.GetAttribute ("type");
			type = SymbolTable.Table.GetCSType (c_type);
			if (IsBit (field)) {
				type = "uint";
			} else if ((IsPointer (field) || SymbolTable.Table.IsOpaque (c_type)) && type != "string") {
				type = "IntPtr";
				name = "_";
			} else if (SymbolTable.Table.IsCallback (c_type)) {
				type = "IntPtr";
			} else {
				if (type == "") {
					Console.WriteLine ("Field has unknown Type {0}", c_type);
					Statistics.ThrottledCount++;
					return false;
				}
			}
			
			// FIXME: marshalling not implemented here in mono 
			if (field.HasAttribute("array_len"))
				type = "IntPtr";

			if (IsBit (field))
				name = String.Format ("_bitfield{0}", bitfields++);
			else
				name += SymbolTable.Table.MangleName (field.GetAttribute ("cname"));

			return true;
		}

		protected bool GenField (XmlElement field, StreamWriter sw)
		{
			string c_type, type, name;
			if (!GetFieldInfo (field, out c_type, out type, out name))
				return false;
			sw.WriteLine ("\t\tpublic {0} {1};", type, SymbolTable.Table.MangleName (name));

			if (field.HasAttribute("array_len"))
				Console.WriteLine ("warning: array field {0}.{1} probably incorrectly generated", QualifiedName, name);
			SymbolTable table = SymbolTable.Table;

			string wrapped = table.GetCSType (c_type);
			string wrapped_name = SymbolTable.Table.MangleName (field.GetAttribute ("cname"));
			if (table.IsObject (c_type)) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { ");
				sw.WriteLine ("\t\t\t\tbool ref_owned = false;");
				sw.WriteLine ("\t\t\t\t" + wrapped + " ret = " + table.FromNativeReturn(c_type, name) + ";");
				sw.WriteLine ("\t\t\t\treturn ret;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t\tset { " + name + " = " + table.CallByName (c_type, "value") + "; }");
				sw.WriteLine ("\t\t}");
			} else if (table.IsOpaque (c_type)) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { ");
				sw.WriteLine ("\t\t\t\t" + wrapped + " ret = " + table.FromNativeReturn(c_type, name) + ";");
				sw.WriteLine ("\t\t\t\tif (ret == null) ret = new " + wrapped + "(" + name + ");");
				sw.WriteLine ("\t\t\t\treturn ret;");
				sw.WriteLine ("\t\t\t}");

				sw.WriteLine ("\t\t\tset { " + name + " = " + table.CallByName (c_type, "value") + "; }");
				sw.WriteLine ("\t\t}");
			} else if (IsPointer (field) && (table.IsStruct (c_type) || table.IsBoxed (c_type))) {
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic " + wrapped + " " + wrapped_name + " {");
				sw.WriteLine ("\t\t\tget { return " + table.FromNativeReturn (c_type, name) + "; }");
				sw.WriteLine ("\t\t}");
			}
			
			return true;
		}

		public virtual void Generate ()
		{
			StreamWriter sw = CreateWriter ();
			
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			
			sw.WriteLine ("#region Autogenerated code");
			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic struct " + Name + " {");
			sw.WriteLine ();

			GenFields (sw);
			sw.WriteLine ();
			GenCtors (sw);
			GenMethods (sw, null, null, true);

			sw.WriteLine ("#endregion");
			AppendCustom(sw);
			
			sw.WriteLine ("\t}");
			CloseWriter (sw);
		}
		
		protected override void GenCtors (StreamWriter sw)
		{
			sw.WriteLine ("\t\tpublic static {0} Zero = new {0} ();", QualifiedName);
			sw.WriteLine();
			sw.WriteLine ("\t\tpublic static " + QualifiedName + " New(IntPtr raw) {");
			sw.WriteLine ("\t\t\tif (raw == IntPtr.Zero) {");
			sw.WriteLine ("\t\t\t\treturn {0}.Zero;", QualifiedName);
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t\t{0} self = new {0}();", QualifiedName);
			sw.WriteLine ("\t\t\tself = ({0}) Marshal.PtrToStructure (raw, self.GetType ());", QualifiedName);
			sw.WriteLine ("\t\t\treturn self;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static bool operator == ({0} a, {0} b)", QualifiedName);
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\treturn a.Equals (b);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static bool operator != ({0} a, {0} b)", QualifiedName);
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\treturn ! a.Equals (b);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine();

			foreach (Ctor ctor in Ctors) {
				ctor.ForceStatic = true;
				if (ctor.Params != null)
					ctor.Params.Static = true;
			}

			base.GenCtors (sw);
		}

	}
}

