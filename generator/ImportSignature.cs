// GtkSharp.Generation.ImportSignature.cs - The ImportSignature Generation Class.
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

