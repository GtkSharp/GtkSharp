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
		private bool preferred;
		private String clashName = null;
		private ClassBase container_type;
		private bool force_static;

		public bool Preferred {
			get { return preferred; }
			set { preferred = value; }
		}

		public bool ForceStatic {
			get { return force_static; }
			set { force_static = value; }
		}
	
		public Parameters Params {
			get { return parms; }
		}

		public Ctor (string libname, XmlElement elem, ClassBase container_type) {
			this.libname = libname;
			this.elem = elem;
			this.container_type = container_type;
			XmlElement parms_elem = elem ["parameters"];
			if (parms_elem != null)
				parms = new Parameters (parms_elem, container_type.NS);
			if (elem.HasAttribute ("preferred"))
				preferred = true;
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

		public void InitClashMap (Hashtable clash_map)
		{
			string sigtypes = (parms != null) ? parms.SignatureTypes : "";
			if (clash_map.ContainsKey (sigtypes)) {
				int num = (int) clash_map[sigtypes];
				clash_map[sigtypes] = ++num;
			}
			else
				clash_map[sigtypes] = 0;
		}

		public void Initialize (Hashtable clash_map)
		{
			string sig = "()";
			string sigtypes = "";
			if (parms != null) {
				sig = "(" + parms.Signature + ")";
				sigtypes = parms.SignatureTypes;
			}
			int clashes = (int) clash_map[sigtypes];
			string cname = elem.GetAttribute("cname");
			if (force_static || (clashes > 0 && !Preferred)) {
				String mname = cname.Substring(cname.IndexOf("new"));
				mname = mname.Substring(0,1).ToUpper() + mname.Substring(1);
				int idx;
				while ((idx = mname.IndexOf("_")) > 0) {
					mname = mname.Substring(0, idx) + mname.Substring(idx+1, 1).ToUpper() + mname.Substring(idx+2);
				}
				clashName = mname + sig;
			}
		}
		
		public void Generate (StreamWriter sw)
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
			
			sw.WriteLine("\t\t/// <summary> " + name + " Constructor </summary>");
			sw.WriteLine("\t\t/// <remarks> To be completed </remarks>");
			if (clashName != null) {
				string modifiers = "";
				Ctor dup = null;
				ObjectGen parent = (ObjectGen) container_type.Parent;
				while (dup == null && parent != null) {
					foreach (Ctor c in parent.Ctors) {
						if (c.clashName == clashName) {
							dup = c;
							modifiers = "new ";
							break;
						}
					}
					parent = (ObjectGen) parent.Parent;
				}
				
				sw.WriteLine("\t\tpublic static " + safety + modifiers +  name + " " + clashName);
				sw.WriteLine("\t\t{");

				if (parms != null)
					parms.Initialize(sw, false, false, ""); 

				sw.Write("\t\t\treturn ");
				if (container_type is StructBase)
					sw.Write ("{0}.New (", name);
				else
					sw.Write ("new {0} (", name);
				sw.WriteLine (cname + call + ");");
			} else {
				sw.WriteLine("\t\tpublic " + safety + name + sig);
				sw.WriteLine("\t\t{");

				if (parms != null)
					parms.Initialize(sw, false, false, ""); 
				sw.WriteLine("\t\t\t{0} = {1}{2};", container_type.AssignToName, cname, call);
				if (parms != null)
					parms.HandleException (sw, "");
			
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.CtorCount++;
		}
	}
}

