// GtkSharp.Generation.InterfaceGen.cs - The Interface Generatable.
//
// Authors:
//   Mike Kestner <mkestner@speakeasy.net>
//   Andres G. Aragoneses <knocte@gmail.com>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004, 2007 Novell, Inc.
// Copyright (c) 2013 Andres G. Aragoneses
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
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	public class InterfaceGen : ObjectBase {

		bool consume_only;

		public InterfaceGen (XmlElement ns, XmlElement elem) : base (ns, elem, true)
		{
			consume_only = elem.GetAttributeAsBoolean ("consume_only");
			foreach (XmlNode node in elem.ChildNodes) {
				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;

				switch (member.Name) {
				case "signal":
					object sig = sigs [member.GetAttribute ("name")];
					if (sig == null)
						sig = new Signal (node as XmlElement, this);
					break;
				default:
					if (!base.IsNodeNameHandled (node.Name))
						new LogWriter (QualifiedName).Warn ("Unexpected node " + node.Name);
					break;
				}
			}
		}

		public bool IsConsumeOnly {
			get {
				return consume_only;
			}
		}

		public string AdapterName {
			get {
				return base.Name + "Adapter";
			}
		}

		public string QualifiedAdapterName {
			get {
				return NS + "." + AdapterName;
			}
		}

		public string ImplementorName {
			get {
				return Name + "Implementor";
			}
		}

		public override string Name {
			get {
				return "I" + base.Name;
			}
		}

		public override string CallByName (string var, bool owned)
		{
			return String.Format (
				"{0} == null ? IntPtr.Zero : (({0} is GLib.Object) ? ({0} as GLib.Object).{1} : ({0} as {2}).{1})",
				var, owned ? "OwnedHandle" : "Handle", QualifiedAdapterName);
		}

		public override string FromNative (string var, bool owned)
		{
			return QualifiedAdapterName + ".GetObject (" + var + ", " + (owned ? "true" : "false") + ")";
		}

		public override bool ValidateForSubclass ()
		{
			if (!base.ValidateForSubclass ())
				return false;

			LogWriter log = new LogWriter (QualifiedName);
			var invalids = new List<Method> ();
			foreach (Method method in Methods.Values) {
				if (!method.Validate (log))
					invalids.Add (method);
			}
			foreach (Method method in invalids)
				Methods.Remove (method.Name);
			invalids.Clear ();

			return true;
		}

		void GenerateStaticCtor (StreamWriter sw)
		{
			sw.WriteLine ("\t\tstatic {0} iface;", class_struct_name);
			sw.WriteLine ();
			sw.WriteLine ("\t\tstatic " + AdapterName + " ()");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tGLib.GType.Register (_gtype, typeof ({0}));", AdapterName);
			foreach (InterfaceVM vm in interface_vms) {
				if (vm.Validate (new LogWriter (QualifiedName)))
					sw.WriteLine ("\t\t\tiface.{0} = new {0}NativeDelegate ({0}_cb);", vm.Name);
			}
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateInitialize (StreamWriter sw)
		{
			if (interface_vms.Count > 0) {
				sw.WriteLine ("\t\tstatic int class_offset = 2 * IntPtr.Size;"); // Class size of GTypeInterface struct
				sw.WriteLine ();
			}
			sw.WriteLine ("\t\tstatic void Initialize (IntPtr ptr, IntPtr data)");
			sw.WriteLine ("\t\t{");
			if (interface_vms.Count > 0) {
				sw.WriteLine ("\t\t\tIntPtr ifaceptr = new IntPtr (ptr.ToInt64 () + class_offset);");
				sw.WriteLine ("\t\t\t{0} native_iface = ({0}) Marshal.PtrToStructure (ifaceptr, typeof ({0}));", class_struct_name);
				foreach (InterfaceVM vm in interface_vms) {
					sw.WriteLine ("\t\t\tnative_iface." + vm.Name + " = iface." + vm.Name + ";");
				}
				sw.WriteLine ("\t\t\tMarshal.StructureToPtr (native_iface, ifaceptr, false);");
			}
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateCallbacks (StreamWriter sw)
		{
			foreach (InterfaceVM vm in interface_vms) {
				vm.GenerateCallback (sw, null);
				}
			}

		void GenerateCtors (StreamWriter sw)
		{
			// Native GObjects do not implement the *Implementor interfaces
			sw.WriteLine ("\t\tGLib.Object implementor;");
			sw.WriteLine ();

			if (!IsConsumeOnly) {
				sw.WriteLine ("\t\tpublic " + AdapterName + " ()");
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tInitHandler = new GLib.GInterfaceInitHandler (Initialize);");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic {0} ({1} implementor)", AdapterName, ImplementorName);
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tif (implementor == null)");
				sw.WriteLine ("\t\t\t\tthrow new ArgumentNullException (\"implementor\");");
				sw.WriteLine ("\t\t\telse if (!(implementor is GLib.Object))");
				sw.WriteLine ("\t\t\t\tthrow new ArgumentException (\"implementor must be a subclass of GLib.Object\");");
				sw.WriteLine ("\t\t\tthis.implementor = implementor as GLib.Object;");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
			}

			sw.WriteLine ("\t\tpublic " + AdapterName + " (IntPtr handle)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (!_gtype.IsInstance (handle))");
			sw.WriteLine ("\t\t\t\tthrow new ArgumentException (\"The gobject doesn't implement the GInterface of this adapter\", \"handle\");");
			sw.WriteLine ("\t\t\timplementor = GLib.Object.GetObject (handle);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateGType (StreamWriter sw)
		{
			Method m = GetMethod ("GetType");
			if (m == null)
				throw new Exception ("Interface " + QualifiedName + " missing GetType method.");
			m.GenerateImport (sw);
			sw.WriteLine ("\t\tprivate static GLib.GType _gtype = new GLib.GType ({0} ());", m.CName);
			sw.WriteLine ();

			// by convention, all GTypes generated have a static GType property
			sw.WriteLine ("\t\tpublic static GLib.GType GType {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn _gtype;");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();

			// we need same property but non-static, because it is being accessed via a GInterfaceAdapter instance
			sw.WriteLine ("\t\tpublic override GLib.GType GInterfaceGType {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn _gtype;");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");

			sw.WriteLine ();
		}

		void GenerateHandleProp (StreamWriter sw)
		{
			sw.WriteLine ("\t\tpublic override IntPtr Handle {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn implementor.Handle;");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic IntPtr OwnedHandle {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn implementor.OwnedHandle;");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateGetObject (StreamWriter sw)
		{
			sw.WriteLine ("\t\tpublic static " + Name + " GetObject (IntPtr handle, bool owned)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tGLib.Object obj = GLib.Object.GetObject (handle, owned);");
			sw.WriteLine ("\t\t\treturn GetObject (obj);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static " + Name + " GetObject (GLib.Object obj)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (obj == null)");
			sw.WriteLine ("\t\t\t\treturn null;");
			if (!IsConsumeOnly) {
				sw.WriteLine ("\t\t\telse if (obj is " + ImplementorName + ")");
				sw.WriteLine ("\t\t\t\treturn new {0} (obj as {1});", AdapterName, ImplementorName);
			}
			sw.WriteLine ("\t\t\telse if (obj as " + Name + " == null)");
			sw.WriteLine ("\t\t\t\treturn new {0} (obj.Handle);", AdapterName);
			sw.WriteLine ("\t\t\telse");
			sw.WriteLine ("\t\t\t\treturn obj as {0};", Name);
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateImplementorProp (StreamWriter sw)
		{
			sw.WriteLine ("\t\tpublic " + ImplementorName + " Implementor {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn implementor as {0};", ImplementorName);
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
		}

		void GenerateAdapter (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (AdapterName, NS);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			sw.WriteLine ("#region Autogenerated code");
			sw.WriteLine ("\tpublic partial class " + AdapterName + " : GLib.GInterfaceAdapter, " + QualifiedName + " {");
			sw.WriteLine ();

			if (!IsConsumeOnly) {
				GenerateClassStruct (gen_info);
				GenerateStaticCtor (sw);
				GenerateCallbacks (sw);
				GenerateInitialize (sw);
			}
			GenerateCtors (sw);
			GenerateGType (sw);
			GenerateHandleProp (sw);
			GenerateGetObject (sw);
			if (!IsConsumeOnly)
				GenerateImplementorProp (sw);

			GenProperties (gen_info, null);

			foreach (Signal sig in sigs.Values)
				sig.GenEvent (sw, null, "GLib.Object.GetObject (Handle)");

			Method temp = GetMethod ("GetType");
			if (temp != null)
				Methods.Remove ("GetType");
			GenMethods (gen_info, null, this);
			if (temp != null)
				Methods ["GetType"] = temp;

			sw.WriteLine ("#endregion");

			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Close ();
			gen_info.Writer = null;
		}

		void GenerateImplementorIface (GenerationInfo gen_info)
		{
			if (IsConsumeOnly)
				return;

			StreamWriter sw = gen_info.Writer;
			sw.WriteLine ();
			sw.WriteLine ("\t[GLib.GInterface (typeof (" + AdapterName + "))]");
			string access = IsInternal ? "internal" : "public";
			sw.WriteLine ("\t" + access + " partial interface " + ImplementorName + " : GLib.IWrapper {");
			sw.WriteLine ();
			var vm_table = new Dictionary<string, InterfaceVM> ();
			foreach (InterfaceVM vm in interface_vms) {
				vm_table [vm.Name] = vm;
			}
			foreach (InterfaceVM vm in interface_vms) {
				if (!vm_table.ContainsKey (vm.Name)) {
					continue;
				} else if (!vm.Validate (new LogWriter (QualifiedName))) {
					vm_table.Remove (vm.Name);
					continue;
				} else if (vm.IsGetter || vm.IsSetter) {
					string cmp_name = (vm.IsGetter ? "Set" : "Get") + vm.Name.Substring (3);
					InterfaceVM cmp = null;
					if (vm_table.TryGetValue (cmp_name, out cmp) && (cmp.IsGetter || cmp.IsSetter)) {
						if (vm.IsSetter)
							cmp.GenerateDeclaration (sw, vm);
						else
							vm.GenerateDeclaration (sw, cmp);
						vm_table.Remove (cmp.Name);
					} else
						vm.GenerateDeclaration (sw, null);
					vm_table.Remove (vm.Name);
				} else {
					vm.GenerateDeclaration (sw, null);
					vm_table.Remove (vm.Name);
				}
			}
			foreach (Property prop in Properties.Values) {
				sw.WriteLine ("\t\t[GLib.Property (\"" + prop.CName + "\")]");
				prop.GenerateDecl (sw, "\t\t");
			}

			sw.WriteLine ("\t}");
		}

		public override void Generate (GenerationInfo gen_info)
		{
			GenerateAdapter (gen_info);
			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (Name, NS);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ();
			sw.WriteLine ("#region Autogenerated code");
			string access = IsInternal ? "internal" : "public";
			sw.WriteLine ("\t" + access + " partial interface " + Name + " : GLib.IWrapper {");
			sw.WriteLine ();
			
			foreach (Signal sig in sigs.Values) {
				sig.GenerateDecl (sw);
				sig.GenEventHandler (gen_info);
			}

			foreach (Method method in Methods.Values) {
				if (IgnoreMethod (method, this))
					continue;
				method.GenerateDecl (sw);
			}

			foreach (Property prop in Properties.Values)
				prop.GenerateDecl (sw, "\t\t");

			sw.WriteLine ("\t}");
			GenerateImplementorIface (gen_info);
			sw.WriteLine ("#endregion");
			sw.WriteLine ("}");
			sw.Close ();
			gen_info.Writer = null;
			Statistics.IFaceCount++;
		}
	}
}

