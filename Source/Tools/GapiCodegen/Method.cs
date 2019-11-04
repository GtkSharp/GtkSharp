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

		private string call;
		private bool is_get, is_set;
		private bool deprecated = false;

		public Method (XmlElement elem, ClassBase container_type) : base (elem, container_type)
		{
			this.retval = new ReturnValue (elem["return-type"]);
			
			if (!container_type.IsDeprecated) {
				deprecated = elem.GetAttributeAsBoolean ("deprecated");
			}
			
			if (Name == "GetType")
				Name = "GetGType";
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

		public string ReturnType {
			get {
				return retval.CSType;
			}
		}

		public override bool Validate (LogWriter log)
		{
			log.Member = Name;
			if (!retval.Validate (log) || !base.Validate (log))
				return false;

			if (Name == String.Empty || CName == String.Empty) {
				log.Warn ("Method has no name or cname.");
				return false;
			}

			Parameters parms = Parameters;
			is_get = ((parms.IsAccessor && retval.IsVoid) || (parms.Count == 0 && !retval.IsVoid)) && HasGetterName;
			is_set = ((parms.IsAccessor || (parms.VisibleCount == 1 && retval.IsVoid)) && HasSetterName);

			call = "(" + (IsStatic ? "" : container_type.CallByName () + (parms.Count > 0 ? ", " : "")) + Body.GetCallString (is_set) + ")";

			return true;
		}
		
		private Method GetComplement ()
		{
			char complement;
			if (is_get)
				complement = 'S';
			else
				complement = 'G';
			
			return container_type.GetMethod (complement + BaseName.Substring (1));
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

			if (Name == "ToString" && Parameters.Count == 0 && (!(container_type is InterfaceGen)|| implementor != null))
				sw.Write("override ");
			else if (Name == "GetGType" && (container_type is ObjectGen || (container_type.Parent != null && container_type.Parent.Methods.ContainsKey ("GetType"))))
				sw.Write("new ");
			else if (Modifiers == "new " || (dup != null && ((dup.Signature != null && Signature != null && dup.Signature.ToString() == Signature.ToString()) || (dup.Signature == null && Signature == null))))
				sw.Write("new ");

			if (Name.StartsWith (container_type.Name))
				Name = Name.Substring (container_type.Name.Length);

			if (is_get || is_set) {
				if (retval.IsVoid)
					sw.Write (Parameters.AccessorReturnType);
				else
					sw.Write(retval.CSType);
				sw.Write(" ");
				if (Name.StartsWith ("Get") || Name.StartsWith ("Set"))
					sw.Write (Name.Substring (3));
				else {
					int dot = Name.LastIndexOf ('.');
					if (dot != -1 && (Name.Substring (dot + 1, 3) == "Get" || Name.Substring (dot + 1, 3) == "Set"))
						sw.Write (Name.Substring (0, dot + 1) + Name.Substring (dot + 4));
					else
						sw.Write (Name);
				}
				sw.WriteLine(" { ");
			} else if (IsAccessor) {
				sw.Write (Signature.AccessorType + " " + Name + "(" + Signature.AsAccessor + ")");
			} else {
				sw.Write(retval.CSType + " " + Name + "(" + (Signature != null ? Signature.ToString() : "") + ")");
			}
		}

		public void GenerateDecl (StreamWriter sw)
		{
			if (IsStatic)
				return;

			if (is_get || is_set)
			{
				Method comp = GetComplement ();
				if (comp != null && is_set)
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
			import_sig += Parameters.ImportSignature.ToString();

            sw.WriteLine("\t\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");

            if (retval.MarshalType.StartsWith("[return:"))
				sw.WriteLine("\t\tdelegate " + retval.CSType + " d_" + CName + "(" + import_sig + ");");
			else
                sw.WriteLine("\t\tdelegate " + retval.MarshalType + " d_" + CName + "(" + import_sig + ");");
			sw.WriteLine("\t\tstatic d_" + CName + " " + CName + " = FuncLoader.LoadFunction<d_" + CName + ">(FuncLoader.GetProcAddress(GLibrary.Load(" + LibraryName + "), \"" + CName + "\"));");
			sw.WriteLine();
		}

		public void GenerateOverloads (StreamWriter sw)
		{
			sw.WriteLine ();
			sw.Write ("\t\tpublic ");
			if (IsStatic)
				sw.Write ("static ");
			sw.WriteLine (retval.CSType + " " + Name + "(" + (Signature != null ? Signature.WithoutOptional () : "") + ") {");
			sw.WriteLine ("\t\t\t{0}{1} ({2});", !retval.IsVoid ? "return " : String.Empty, Name, Signature.CallWithoutOptionals ());
			sw.WriteLine ("\t\t}");
		}

		public void Generate (GenerationInfo gen_info, ClassBase implementor)
		{
			Method comp = null;

			gen_info.CurrentMember = Name;

			/* we are generated by the get Method, if there is one */
			if (is_set || is_get)
			{
				if (Modifiers != "new " && container_type.GetPropertyRecursively (Name.Substring (3)) != null)
					return;
				comp = GetComplement ();
				if (comp != null && is_set) {
					if (Parameters.AccessorReturnType == comp.ReturnType)
						return;
					else {
						is_set = false;
						call = "(" + (IsStatic ? "" : container_type.CallByName () + (parms.Count > 0 ? ", " : "")) + Body.GetCallString (false) + ")";
						comp = null;
					}
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
			if (Protection != "")
				gen_info.Writer.Write("{0} ", Protection);
			GenerateDeclCommon (gen_info.Writer, implementor);

			if (is_get || is_set)
			{
				gen_info.Writer.Write ("\t\t\t");
				gen_info.Writer.Write ((is_get) ? "get" : "set");
				GenerateBody (gen_info, implementor, "\t");
			}
			else
				GenerateBody (gen_info, implementor, "");
			
			if (is_get || is_set)
			{
				if (comp != null && retval.CSType == comp.Parameters.AccessorReturnType)
				{
					gen_info.Writer.WriteLine ();
					gen_info.Writer.Write ("\t\t\tset");
					comp.GenerateBody (gen_info, implementor, "\t");
				}
				gen_info.Writer.WriteLine ();
				gen_info.Writer.WriteLine ("\t\t}");
			}
			else
				gen_info.Writer.WriteLine();

			if (Parameters.HasOptional && !(is_get || is_set))
				GenerateOverloads (gen_info.Writer);
			
			gen_info.Writer.WriteLine();

			Statistics.MethodCount++;
		}

		public void GenerateBody (GenerationInfo gen_info, ClassBase implementor, string indent)
		{
			StreamWriter sw = gen_info.Writer;
			sw.WriteLine(" {");
			if (!IsStatic && implementor != null)
				implementor.Prepare (sw, indent + "\t\t\t");
			if (IsAccessor)
				Body.InitAccessor (sw, Signature, indent);
			Body.Initialize(gen_info, is_get, is_set, indent);

			sw.Write(indent + "\t\t\t");
			if (retval.IsVoid)
				sw.WriteLine(CName + call + ";");
			else {
				sw.WriteLine(retval.MarshalType + " raw_ret = " + CName + call + ";");
				sw.WriteLine(indent + "\t\t\t" + retval.CSType + " ret = " + retval.FromNative ("raw_ret") + ";");
			}
			
			if (!IsStatic && implementor != null)
				implementor.Finish (sw, indent + "\t\t\t");
			Body.Finish (sw, indent);
			Body.HandleException (sw, indent);

			if (is_get && Parameters.Count > 0)
				sw.WriteLine (indent + "\t\t\treturn " + Parameters.AccessorName + ";");
			else if (!retval.IsVoid)
				sw.WriteLine (indent + "\t\t\treturn ret;");
			else if (IsAccessor)
				Body.FinishAccessor (sw, Signature, indent);

			sw.Write(indent + "\t\t}");
		}

		bool IsAccessor {
			get {
				return retval.IsVoid && Signature.IsAccessor;
			}
		}
	}
}

