// GtkSharp.Generation.CallbackGen.cs - The Callback Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class CallbackGen : GenBase, IGeneratable  {

		private Parameters parms;

		public CallbackGen (XmlElement ns, XmlElement elem) : base (ns, elem) 
		{
			if (elem ["parameters"] != null)
				parms = new Parameters (elem ["parameters"]);
		}

		public String MarshalType {
			get
			{
				return NS + "Sharp." + Name + "Native";
			}
		}

		public String MarshalReturnType {
			get
			{
				return MarshalType;
			}
		}

		public String CallByName (String var_name)
		{
			return var_name + ".NativeDelegate";
		}

		public String FromNative(String var)
		{
			return var;
		}

		public String FromNativeReturn(String var)
		{
			return FromNative (var);
		}

		private void GenWrapper (string s_ret, string sig)
		{
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + NS.ToLower() + sep + "generated";

			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			string wrapper = Name + "Native";

			string filename = dir + sep + NS + "Sharp." + wrapper + ".cs";

			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			sw.WriteLine ("namespace " + NS + "Sharp {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ();
			
			string import_sig;
			if (parms != null)
			{
				parms.CreateSignature (false);
				import_sig = parms.ImportSig;
			} else
				import_sig = "";

			XmlElement ret_elem = Elem["return-type"];
			string rettype = ret_elem.GetAttribute("type");
			string m_ret = SymbolTable.GetMarshalReturnType (rettype);

			sw.WriteLine ("\tpublic delegate " + m_ret + " " + wrapper + "(" + import_sig + ");");
			sw.WriteLine ();
			
			sw.WriteLine ("\tpublic class " + Name + "Wrapper : GLib.DelegateWrapper {");
			sw.WriteLine ();

			sw.WriteLine ("\t\tpublic " + m_ret + " NativeCallback (" + import_sig + ")");
			sw.WriteLine ("\t\t{");
			int count = (parms != null) ? parms.Count : 0;
			if (count > 0)
				sw.WriteLine ("\t\t\tobject[] _args = new object[{0}];", count);
			int idx = 0;
			bool need_sep = false;
			string call_str = "";
			for (int i = 0; i < count; i++)
			{
				string parm_name = parms[i].Name;
				string ctype = parms[i].CType;
				if ((i == count - 1) && ctype == "gpointer" && (parm_name.EndsWith ("data") || parm_name.EndsWith ("data_or_owner"))) 
					continue;
				string cstype = parms[i].CSType;
				// FIXME: Too much code copy/pasted here. Refactor?
				ClassBase parm_wrapper = SymbolTable.GetClassGen (ctype);
				sw.WriteLine("\t\t\t_args[" + idx + "] = " + SymbolTable.FromNative (ctype, parm_name) + ";");
				if ((parm_wrapper != null && ((parm_wrapper is OpaqueGen))) || SymbolTable.IsManuallyWrapped (ctype)) {
					sw.WriteLine("\t\t\tif (_args[" + idx + "] == null)");
					sw.WriteLine("\t\t\t\t_args[{0}] = new {1}({2});", idx, cstype, parm_name);
				}
				if (need_sep)
					call_str += ", ";
				else
					need_sep = true;
				call_str += String.Format ("({0}) _args[{1}]", cstype, idx);
				idx++;
			}

			sw.Write ("\t\t\t");
			string invoke = "_managed (" + call_str + ")";
			if (m_ret != "void") {
					ClassBase parm_wrapper = SymbolTable.GetClassGen (rettype);
					if (parm_wrapper != null && (parm_wrapper is ObjectGen || parm_wrapper is OpaqueGen))
						sw.WriteLine ("return (({0}) {1}).Handle;", s_ret, invoke);
					else if (SymbolTable.IsStruct (rettype) || SymbolTable.IsBoxed (rettype)) {
						// Shoot. I have no idea what to do here.
						sw.WriteLine ("return IntPtr.Zero;"); 
					}
					else if (SymbolTable.IsEnum (rettype))
						sw.WriteLine ("return (int) {0};", invoke);
					else
						sw.WriteLine ("return ({0}) {1};", s_ret, invoke);
			}
			else
				sw.WriteLine (invoke + ";");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();

			sw.WriteLine ("\t\tpublic {0} NativeDelegate;", wrapper);
			sw.WriteLine ("\t\tprotected {0} _managed;", NS + "." + Name);
			sw.WriteLine ();

			sw.WriteLine ("\t\tpublic {0} ({1} managed) : base ()", Name + "Wrapper", NS + "." + Name);
			sw.WriteLine ("\t\t{");

			sw.WriteLine ("\t\t\tNativeDelegate = new {0} (NativeCallback);", wrapper);
			sw.WriteLine ("\t\t\t_managed = managed;");
			sw.WriteLine ("\t\t}");

			sw.WriteLine ("\t}");
			
			CloseWriter (sw);
		}
		
		public void Generate ()
		{
			if (!DoGenerate)
				return;

			XmlElement ret_elem = Elem["return-type"];
			if (ret_elem == null) {
				Console.WriteLine("No return type in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}

			string rettype = ret_elem.GetAttribute("type");
			string s_ret = SymbolTable.GetCSType (rettype);
			if (s_ret == "") {
				Console.WriteLine("rettype: " + rettype + " in callback " + CName);
				Statistics.ThrottledCount++;
				return;
			}

			if ((parms != null) && !parms.Validate ()) {
				Console.WriteLine(" in callback " + CName + " **** Stubbing it out ****");
				Statistics.ThrottledCount++;
				parms = null;
			}

			StreamWriter sw = CreateWriter ();

			string sig = "";
			if (parms != null) {
				parms.HideData = true;
				parms.CreateSignature (false);
				sig = parms.Signature;
			}

			sw.WriteLine ("\tpublic delegate " + s_ret + " " + Name + "(" + sig + ");");

			CloseWriter (sw);
			
			GenWrapper (s_ret, sig);

			Statistics.CBCount++;
		}
	}
}

