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
		private Signature sig = null;
		private ImportSignature isig = null;
		private MethodBody body = null;
		private bool preferred;
		private string clashName = null;
		private ClassBase container_type;
		private bool force_static;
		private bool needs_chaining = false;

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
			if (container_type is ObjectGen)
				needs_chaining = true;
		}

		public bool Validate ()
		{
			if (parms != null) {
				if (!parms.Validate ()) {
					Console.Write("in ctor ");
					Statistics.ThrottledCount++;
					return false;
				}
			}
			sig = new Signature (parms);
			isig = new ImportSignature (parms, container_type.NS);
			body = new MethodBody (parms, container_type.NS);

			return true;
		}

		public void InitClashMap (Hashtable clash_map)
		{
			if (clash_map.ContainsKey (sig.Types)) {
				int num = (int) clash_map[sig.Types];
				clash_map[sig.Types] = ++num;
			}
			else
				clash_map[sig.Types] = 0;
		}

		public void Initialize (Hashtable clash_map)
		{
			int clashes = (int) clash_map[sig.Types];
			string cname = elem.GetAttribute("cname");
			if (force_static || (clashes > 0 && !Preferred)) {
				string mname = cname.Substring(cname.IndexOf("new"));
				mname = mname.Substring(0,1).ToUpper() + mname.Substring(1);
				int idx;
				while ((idx = mname.IndexOf("_")) > 0) {
					mname = mname.Substring(0, idx) + mname.Substring(idx+1, 1).ToUpper() + mname.Substring(idx+2);
				}
				clashName = mname + "(" + sig.ToString () + ")";
			}
		}
		
		public void Generate (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer;

			string cname = elem.GetAttribute("cname");
			string name = ((XmlElement)elem.ParentNode).GetAttribute("name");
			string safety;
			if (body.ThrowsException)
				safety = "unsafe ";
			else
				safety = "";

			SymbolTable table = SymbolTable.Table;

			sw.WriteLine("\t\t[DllImport(\"" + libname + "\")]");
			sw.WriteLine("\t\tstatic extern " + safety + "IntPtr " + cname + "(" + isig.ToString () + ");");
			sw.WriteLine();
			
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

				body.Initialize(gen_info, false, false, ""); 

				sw.Write("\t\t\treturn ");
				if (container_type is StructBase)
					sw.Write ("{0}.New (", name);
				else
					sw.Write ("new {0} (", name);
				sw.WriteLine (cname + "(" + body.GetCallString (false) + "));");
			} else {
				sw.WriteLine("\t\tpublic {0}{1} ({2}) {3}", safety, name, sig.ToString(), needs_chaining ? ": base (IntPtr.Zero)" : "");
				sw.WriteLine("\t\t{");

				if (needs_chaining) {
					sw.WriteLine ("\t\t\tif (GetType () != typeof (" + name + ")) {");
					
					if (Params == null || Params.Count == 0) {
						sw.WriteLine ("\t\t\t\tCreateNativeObject (new string [0], new GLib.Value[0]);");
						sw.WriteLine ("\t\t\t\treturn;");
					} else {
						ArrayList names = new ArrayList ();
						ArrayList values = new ArrayList ();
						for (int i = 0; i < Params.Count; i++) {
							Parameter p = Params[i];
							if (container_type.GetPropertyRecursively (p.StudlyName) != null) {
								names.Add (p.Name);
								values.Add (p.Name);
							} else if (p.PropertyName != String.Empty) {
								names.Add (p.PropertyName);
								values.Add (p.Name);
							}
						}

						if (names.Count == Params.Count) {
							sw.WriteLine ("\t\t\t\tArrayList vals = new ArrayList();");
							sw.WriteLine ("\t\t\t\tArrayList names = new ArrayList();");
							for (int i = 0; i < names.Count; i++) {
								Parameter p = Params [i];
								string indent = "\t\t\t\t";
								if (p.NullOk && p.Generatable is ObjectGen) {
									sw.WriteLine (indent + "if (" + p.Name + " != null) {");
									indent += "\t";
								}
								sw.WriteLine (indent + "names.Add (\"" + names [i] + "\");");
								sw.Write (indent + "vals.Add (");

								if (table.IsEnum (p.CType))
									sw.WriteLine ("new GLib.Value (this, \"" + names[i] + "\", new GLib.EnumWrapper ((int)" + values[i] + ", " + (table.IsEnumFlags (p.CType) ? "true" : "false") + ")));");
								else
									sw.WriteLine ("new GLib.Value (" + values[i] + "));");

								if (p.NullOk && p.Generatable is ObjectGen)
									sw.WriteLine ("\t\t\t\t}");
							}

							sw.WriteLine ("\t\t\t\tCreateNativeObject ((string[])names.ToArray (typeof (string)), (GLib.Value[])vals.ToArray (typeof (GLib.Value)));");
							sw.WriteLine ("\t\t\t\treturn;");
						} else
							sw.WriteLine ("\t\t\t\tthrow new InvalidOperationException (\"Can't override this constructor.\");");
					}
					
					sw.WriteLine ("\t\t\t}");
				}
	
				body.Initialize(gen_info, false, false, ""); 
				sw.WriteLine("\t\t\t{0} = {1}({2});", container_type.AssignToName, cname, body.GetCallString (false));
				body.HandleException (sw, "");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.CtorCount++;
		}
	}
}

