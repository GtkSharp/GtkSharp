// GtkSharp.Generation.Ctor.cs - The Constructor Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Ctor  {

		private string libname;
		private XmlElement elem;
		private Parameters parms;

		public Ctor (string libname, XmlElement elem) {
			this.libname = libname;
			this.elem = elem;
			XmlElement parms_elem = elem ["parameters"];
			if (parms_elem != null)
				parms = new Parameters (parms_elem);
		}

		public bool Validate ()
		{
			if (parms != null) {
				if (!parms.Validate ()) {
					Console.Write("ctor ");
					Statistics.ThrottledCount++;
					return false;
				}
				parms.CreateSignature (false);
			}

			return true;
		}

		public void Generate (StreamWriter sw, Hashtable clash_map)
		{
			string sigtypes = "";
			string sig = "()";
			string call = "()";
			string isig = "();";
			if (parms != null) {
				call = "(" + parms.CallString + ")";
				sig = "(" + parms.Signature + ")";
				isig = "(" + parms.ImportSig + ");";
				sigtypes = parms.SignatureTypes;
			}

			bool clash = false;
			if (clash_map.ContainsKey(sigtypes)) {
				clash = true;
			} else {
				clash_map[sigtypes] = elem;
			}			
			
			string cname = elem.GetAttribute("cname");
			string name = ((XmlElement)elem.ParentNode).GetAttribute("name");
			string safety;
			if (parms != null && parms.ThrowsException)
				safety = "unsafe ";
			else
				safety = "";

			sw.WriteLine("\t\t[DllImport(\"" + libname + "\")]");
			sw.WriteLine("\t\tstatic extern " + safety + "IntPtr " + cname + isig);
			sw.WriteLine();
			
			if (clash) {
				String mname = cname.Substring(cname.IndexOf("new"));
				mname = mname.Substring(0,1).ToUpper() + mname.Substring(1);
				int idx;
				while ((idx = mname.IndexOf("_")) > 0) {
					mname = mname.Substring(0, idx) + mname.Substring(idx+1, 1).ToUpper() + mname.Substring(idx+2);
				}
				
				sw.WriteLine("\t\tpublic static " + safety + name + " " + mname + sig);
				sw.WriteLine("\t\t{");
				if (parms != null)
					parms.Initialize(sw, false, "");
				sw.WriteLine("\t\t\tIntPtr ret = " + cname + call + ";");
				if (parms != null)
					parms.HandleException (sw, "");
				sw.WriteLine("\t\t\treturn new " + name + "(ret);");
			} else {
				sw.WriteLine("\t\tpublic " + safety + name + sig);
				sw.WriteLine("\t\t{");
				if (parms != null)
					parms.Initialize(sw, false, ""); 
				sw.WriteLine("\t\t\tRaw = " + cname + call + ";");
				if (parms != null)
					parms.HandleException (sw, "");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.CtorCount++;
		}
	}
}

