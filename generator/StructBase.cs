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

		protected bool GetFieldInfo (XmlElement field, out string type, out string name)
		{
			name = "";
			if (IsBit (field))
				type = "uint";
			else if (IsPointer (field)) {
				type = "IntPtr";
				name = "_";
			} else {
				string c_type = field.GetAttribute ("type");
				type = SymbolTable.GetCSType (c_type);
				if (type == "") {
					Console.WriteLine ("Field has unknown Type {0}", c_type);
					Statistics.ThrottledCount++;
					return false;
				}
			}
			
			if (field.HasAttribute("array_len"))
				type += "[]";

			if (IsBit (field))
				name = String.Format ("_bitfield{0}", bitfields++);
			else
				name += MangleName (field.GetAttribute ("cname"));

			return true;
		}
		
		protected bool GenField (XmlElement field, StreamWriter sw)
		{
			string type, name;
			if (!GetFieldInfo (field, out type, out name))
				return false;
			sw.WriteLine ("\t\tpublic {0} {1};", type, name);
			return true;
		}

		private String MangleName(String name)
		{
			if (name == "string") {
				return "str1ng";
			} else if (name == "event") {
				return "evnt";
			} else if (name == "object") {
				return "objekt";
			} else if (name == "in") {
				return "inn";
			} else {
				return name;
			}
		}

		public virtual void Generate ()
		{
			StreamWriter sw = CreateWriter ();
			
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			
			sw.WriteLine("\t\t/// <summary> " + Name + " Struct </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			sw.WriteLine("\t\t/// </remarks>");

			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic struct " + Name + " {");
			sw.WriteLine ();

			GenFields (sw);
			sw.WriteLine ();
			GenCtors (sw);
			GenMethods (sw, null, null, true);
			AppendCustom(sw);
			
			sw.WriteLine ("\t}");
			CloseWriter (sw);
		}
		
		protected override void GenCtors (StreamWriter sw)
		{
			sw.WriteLine ("\t\tbool _is_null;");
			sw.WriteLine ("\t\tpublic bool IsNull {");
			sw.WriteLine ("\t\t\tget { return _is_null; }");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ("\t\tpublic void _Initialize () {");
			sw.WriteLine ("\t\t\t_is_null = false;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine();
			sw.WriteLine ("\t\tpublic static " + QualifiedName + " New(IntPtr raw) {");
			sw.WriteLine ("\t\t\t{0} self = new {0}();", QualifiedName);
			sw.WriteLine ("\t\t\tif (raw == IntPtr.Zero) {");
			sw.WriteLine ("\t\t\t\tself._is_null = true;");
			sw.WriteLine ("\t\t\t} else {");
   		sw.WriteLine ("\t\t\t\tself = ({0}) Marshal.PtrToStructure (raw, self.GetType ());", QualifiedName);
			sw.WriteLine ("\t\t\t\tself._is_null = false;");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t\treturn self;");
			sw.WriteLine ("\t\t}");
			sw.WriteLine();

			foreach (Ctor ctor in Ctors) {
				ctor.ForceStatic = true;
			}
			base.GenCtors (sw);
		}

	}
}

