// GtkSharp.Generation.VMSignature.cs - The Virtual Method Signature Generation Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// (c) 2003 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.Xml;

	public class VMSignature  {
		
		private ArrayList parms = new ArrayList ();

		public VMSignature (Parameters parms) 
		{
			if (parms == null)
				return;

			bool has_cb = parms.HideData;
			for (int i = 1; i < parms.Count; i++) {
				Parameter p = parms [i];

				if (i > 1 && p.IsLength && parms [i - 1].IsString)
					continue;

				if (p.IsCount && ((i > 1 && parms [i - 1].IsArray) || (i < parms.Count - 1 && parms [i + 1].IsArray)))
					continue;

				has_cb = has_cb || p.Generatable is CallbackGen;
				if (p.IsUserData && has_cb && (i == parms.Count - 1)) 
					continue;

				if (p.CType == "GError**")
					continue;

				this.parms.Add (p);
			}
		}

		public override string ToString ()
		{
			if (parms.Count == 0)
				return "";

			string[] result = new string [parms.Count];
			int i = 0;

			foreach (Parameter p in parms) {
				result [i] = p.PassAs != "" ? p.PassAs + " " : "";
				if (p.Generatable is StructGen)
					result [i] += "ref ";
				result [i++] += p.CSType + " " + p.Name;
			}

			return String.Join (", ", result);
		}
	}
}

