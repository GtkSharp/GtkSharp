// GtkSharp.Generation.ImportSignature.cs - The ImportSignature Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2003 Mike Kestner, (c) 2003 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;

	public class ImportSignature {

		Parameters parameters;
		string impl_ns;

		public ImportSignature (Parameters parms, string impl_ns) 
		{
			parameters = parms;
			this.impl_ns = impl_ns;
		}

		private bool UsesHandle (IGeneratable igen) 
		{
			return igen is ManualGen || igen is ObjectGen || igen is InterfaceGen || igen is OpaqueGen;
		}

		public override string ToString ()
		{
			if (parameters == null)
				return "";

			string[] parms = new string [parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];
				string m_type = p.MarshalType;
				if (p.Generatable is CallbackGen)
					m_type = impl_ns + "Sharp" + p.MarshalType.Substring(p.MarshalType.IndexOf("."));

				parms [i] = "";
				if (p.CType == "GError**")
					parms [i] += "out ";
				else if (p.PassAs != "" && (!m_type.EndsWith ("IntPtr") || UsesHandle (p.Generatable)))
					parms [i] += p.PassAs + " ";
				parms [i] += m_type + " " + p.Name;
			}

			string import_sig = String.Join (", ", parms);
			import_sig = import_sig.Replace ("out ref", "out");
			import_sig = import_sig.Replace ("ref ref", "ref");
			return import_sig;
		}
	}
}

