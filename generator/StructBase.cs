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
		
		public StructBase (XmlElement ns, XmlElement elem) : base (ns, elem) {}

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

		public override String FromNative(String var)
		{
			return var;
		}
		
		public override String FromNativeReturn(String var)
		{
			return "new " + QualifiedName + " (" + var + ")";
		}

		protected bool GenField (XmlElement field, StreamWriter sw)
		{
			String c_type;
			
			if (field.HasAttribute("bits") && (field.GetAttribute("bits") == "1")) {
				c_type = "gboolean";
			} else {
				c_type = field.GetAttribute("type");
			}
			char[] ast = {'*'};
			c_type = c_type.TrimEnd(ast);
			String m_type = SymbolTable.GetMarshalType(c_type);
			
			if (m_type == "") {
				Console.WriteLine ("Field has unknown Type {0}", c_type);
				Statistics.ThrottledCount++;
				return false;
			}
			
			sw.Write ("\t\t public " + m_type);
			if (field.HasAttribute("array_len")) {
				sw.Write ("[]");
			}
			sw.WriteLine (" " + MangleName(field.GetAttribute("cname")) + ";");
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

			GenCtors (sw);
			AppendCustom(sw);
			
			sw.WriteLine ("\t}");
			CloseWriter (sw);
		}
		
		protected override void GenCtors (StreamWriter sw)
		{
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) {}");
			sw.WriteLine();
			//base.GenCtors (sw);
		}

	}
}

