// GtkSharp.Generation.Method.cs - The Method Generatable.
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
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Method : MethodBase  {
		
		private ReturnValue retval;

		private bool initialized = false;
		private string call;
		private string name;
		private string protection = "public";
		private bool is_get, is_set;
		private bool deprecated = false;

		public Method (XmlElement elem, ClassBase container_type) : base (elem, container_type)
		{
			this.retval = new ReturnValue (elem["return-type"]);
			if (!container_type.IsDeprecated && elem.HasAttribute ("deprecated"))
				deprecated = elem.GetAttribute ("deprecated") == "1";
			this.name = elem.GetAttribute("name");
			if (name == "GetType")
				name = "GetGType";
		}

		public bool IsDeprecated {
			get {
				return deprecated;
			}
		}

		public bool IsGetter {
			get {
				return is_get;
			}
		}

		public bool IsSetter {
			get {
				return is_set;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public string Protection {
			get {
				return protection;
			}
			set {
				protection = value;
			}
		}

		public string ReturnType {
			get {
				return retval.CSType;
			}
		}

		public override bool Equals (object o)
		{
			if (!(o is Method))
				return false;
			Method a = this;
			Method b = (Method) o;

			if (a.Name != b.Name)
				return false;

			if (a.Signature == null)
				return b.Signature == null;

			if (b.Signature == null)
				return false;

			return (a.Signature.Types == b.Signature.Types);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode () ^ (Signature == null ? 0 : Signature.Types.GetHashCode ());
		}

		private bool Initialize ()
		{
			if (initialized)
				return true;

			Parameters parms = Parameters;
			is_get = ((((parms.IsAccessor && retval.CSType == "void") || (parms.Count == 0 && retval.CSType != "void")) || (parms.Count == 0 && retval.CSType != "void")) && Name.Length > 3 && (Name.StartsWith ("Get") || Name.StartsWith ("Is") || Name.StartsWith ("Has")));
			is_set = ((parms.IsAccessor || (parms.Count == 1 && retval.CSType == "void")) && (Name.Length > 3 && Name.Substring(0, 3) == "Set"));
			
			call = "(" + (IsStatic ? "" : container_type.CallByName () + (parms.Count > 0 ? ", " : "")) + Body.GetCallString (is_set) + ")";

			initialized = true;
			return true;
		}
		
		public override bool Validate ()
		{
			if (!Initialize () || !base.Validate ())
				return false;

			if (!retval.Validate ()) {
				Console.Write(" in method " + Name + " ");
				return false;
			}

			return true;
		}
		
		private Method GetComplement ()
		{
			char complement;
			if (is_get)
				complement = 'S';
			else
				complement = 'G';
			
			return container_type.GetMethod (complement + name.Substring (1));
		}
		
		public string Declaration {
			get {
				return retval.CSType + " " + Name + " (" + (Signature != null ? Signature.ToString() : "") + ");";
			}
		}

		private void GenerateDeclCommon (StreamWriter sw, ClassBase implementor)
		{
			if (IsStatic)
				sw.Write("static ");
			sw.Write (Safety);
			Method dup = null;
			if (container_type != null)
				dup = container_type.GetMethodRecursively (Name);
			if (implementor != null)
				dup = implementor.GetMethodRecursively (Name);

			if (Name == "ToString" && Parameters.Count == 0)
				sw.Write("override ");
			else if (Name == "GetGType" && container_type is ObjectGen)
				sw.Write("new ");
			else if (Modifiers == "new " || (dup != null && dup.Initialize () && ((dup.Signature != null && Signature != null && dup.Signature.ToString() == Signature.ToString()) || (dup.Signature == null && Signature == null))))
				sw.Write("new ");

			if (is_get || is_set) {
				if (retval.CSType == "void")
					sw.Write (Parameters.AccessorReturnType);
				else
					sw.Write(retval.CSType);
				sw.Write(" ");
				if (Name.StartsWith ("Get") || Name.StartsWith ("Set"))
					sw.Write (Name.Substring (3));
				else
					sw.Write (Name);
				sw.WriteLine(" { ");
			} else if (IsAccessor) {
				sw.Write (Signature.AccessorType + " " + Name + "(" + Signature.AsAccessor + ")");
			} else {
				sw.Write(retval.CSType + " " + Name + "(" + (Signature != null ? Signature.ToString() : "") + ")");
			}
		}

		public void GenerateDecl (StreamWriter sw)
		{
			if (!Initialize ()) 
				return;

			if (IsStatic)
				return;

			if (is_get || is_set)
			{
				Method comp = GetComplement ();
				if (comp != null && comp.Validate () && is_set)
					return;
			
				sw.Write("\t\t");
				GenerateDeclCommon (sw, null);

				sw.Write("\t\t\t");
				sw.Write ((is_get) ? "get;" : "set;");

				if (comp != null && comp.is_set)
					sw.WriteLine (" set;");
				else
					sw.WriteLine ();

				sw.WriteLine ("\t\t}");
			}
			else
			{
				sw.Write("\t\t");
				GenerateDeclCommon (sw, null);
				sw.WriteLine (";");
			}

			Statistics.MethodCount++;
		}

		public void GenerateImport (StreamWriter sw)
		{
			string import_sig = IsStatic ? "" : container_type.MarshalType + " raw";
			import_sig += !IsStatic && Parameters.Count > 0 ? ", " : "";
			import_sig += ImportSignature.ToString();
			sw.WriteLine("\t\t[DllImport(\"" + LibraryName + "\")]");
			if (retval.MarshalType.StartsWith ("[return:"))
				sw.WriteLine("\t\t" + retval.MarshalType + " static extern " + Safety + retval.CSType + " " + CName + "(" + import_sig + ");");
			else
				sw.WriteLine("\t\tstatic extern " + Safety + retval.MarshalType + " " + CName + "(" + import_sig + ");");
			sw.WriteLine();
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			Method comp = null;

			if (!Initialize ()) 
				return;

			if (implementor != null && IsStatic)
				return;

			/* we are generated by the get Method, if there is one */
			if (is_set || is_get)
			{
				if (Modifiers != "new " && container_type.GetPropertyRecursively (Name.Substring (3)) != null)
					return;
				comp = GetComplement ();
				if (comp != null && comp.Validate () && is_set && Parameters.AccessorReturnType == comp.ReturnType)
					return;
				if (comp != null && is_set && Parameters.AccessorReturnType != comp.ReturnType)
				{
					is_set = false;
					call = "(Handle, " + Body.GetCallString (false) + ")";
					comp = null;
				}
				/* some setters take more than one arg */
				if (comp != null && !comp.is_set)
					comp = null;
			}
			
			GenerateImport (gen_info.Writer);
			if (comp != null && retval.CSType == comp.Parameters.AccessorReturnType)
				comp.GenerateImport (gen_info.Writer);

			if (IsDeprecated)
				gen_info.Writer.WriteLine("\t\t[Obsolete]");
			gen_info.Writer.Write("\t\t");
			if (protection != "")
				gen_info.Writer.Write("{0} ", protection);
			GenerateDeclCommon (gen_info.Writer, implementor);

			if (is_get || is_set)
			{
				gen_info.Writer.Write ("\t\t\t");
				gen_info.Writer.Write ((is_get) ? "get" : "set");
				GenerateBody (gen_info, "\t");
			}
			else
				GenerateBody (gen_info, "");
			
			if (is_get || is_set)
			{
				if (comp != null && retval.CSType == comp.Parameters.AccessorReturnType)
				{
					gen_info.Writer.WriteLine ();
					gen_info.Writer.Write ("\t\t\tset");
					comp.GenerateBody (gen_info, "\t");
				}
				gen_info.Writer.WriteLine ();
				gen_info.Writer.WriteLine ("\t\t}");
			}
			else
				gen_info.Writer.WriteLine();
			
			gen_info.Writer.WriteLine();

			Statistics.MethodCount++;
		}

		public void GenerateBody (GenerationInfo gen_info, string indent)
		{
			StreamWriter sw = gen_info.Writer;
			sw.WriteLine(" {");
			if (IsAccessor)
				Body.InitAccessor (sw, Signature, indent);
			Body.Initialize(gen_info, is_get, is_set, indent);

			SymbolTable table = SymbolTable.Table;
			IGeneratable ret_igen = table [retval.CType];

			sw.Write(indent + "\t\t\t");
			if (retval.MarshalType == "void") {
				sw.WriteLine(CName + call + ";");
			} else {
				sw.WriteLine(retval.MarshalType + " raw_ret = " + CName + call + ";");
				sw.Write(indent + "\t\t\t");
				string raw_parms = "raw_ret";
				if (retval.ElementType != String.Empty)
					raw_parms += ", typeof (" + retval.ElementType + ")";
				else if (retval.Owned)
					raw_parms += ", true";
				sw.WriteLine(retval.CSType + " ret = " + table.FromNativeReturn(retval.CType, raw_parms) + ";");
			}

			Body.Finish (sw, indent);
			Body.HandleException (sw, indent);

			if (is_get && Parameters.Count > 0) 
				sw.WriteLine (indent + "\t\t\treturn " + Parameters.AccessorName + ";");
			else if (retval.MarshalType != "void")
				sw.WriteLine (indent + "\t\t\treturn ret;");
			else if (IsAccessor)
				Body.FinishAccessor (sw, Signature, indent);

			sw.Write(indent + "\t\t}");
		}

		bool IsAccessor { 
			get { 
				return retval.CSType == "void" && Signature.IsAccessor; 
			} 
		}
	}
}

