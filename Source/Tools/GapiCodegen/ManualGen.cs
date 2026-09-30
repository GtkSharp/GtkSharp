// GtkSharp.Generation.ManualGen.cs - Ungenerated handle type Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2003 Mike Kestner
// Copyright (c) 2004 Novell, Inc.
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

	public class ManualGen : SimpleBase {
		
		string from_fmt;
		string abi_type;

		/// <summary>
		/// True when a NULL pointer of this type means "there is no value", so
		/// the binding has to hand back null rather than a wrapper around
		/// IntPtr.Zero.
		/// </summary>
		/// <remarks>
		/// It is opt-in because it is not true of every manual type: a NULL
		/// <c>GList *</c> IS the empty list, and turning that into null would
		/// break every caller that iterates the result. It is true of GVariant,
		/// where g_settings_get_user_value returns NULL to say the key has never
		/// been written -- and the wrapper built over IntPtr.Zero looked like a
		/// value, compared non-null, and made g_variant_ref_sink log a CRITICAL
		/// on the way in.
		/// </remarks>
		public bool NullIsNull { get; set; }

		public ManualGen (string ctype, string type) : base (ctype, type, "null")
		{
			from_fmt = "new " + QualifiedName + "({0})";
		}

		public ManualGen (string ctype, string type, string from_fmt) : base (ctype, type, "null")
		{
			this.from_fmt = from_fmt;
		}

		public ManualGen (string ctype, string type, string from_fmt, string abi_type) : base (ctype, type, "null")
		{
			this.from_fmt = from_fmt;
			this.abi_type = abi_type;
		}

		public override string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public string AbiType {
			get {
				return abi_type;
			}
		}

		public override string CallByName (string var_name)
		{
			return var_name + " == null ? IntPtr.Zero : " + var_name + ".Handle";
		}
		
		public override string FromNative(string var)
		{
			string expr = String.Format (from_fmt, var);

			// The guard names the source twice, so it is only safe when the
			// source is a plain identifier. Every path that matters -- a return
			// value, a signal argument, an out-parameter's scratch variable --
			// passes one; FieldBase and DefaultSignalHandler pass a call
			// expression, and those keep the unguarded form.
			if (!NullIsNull || !IsIdentifier (var))
				return expr;

			return "(" + var + " == IntPtr.Zero ? null : " + expr + ")";
		}

		static bool IsIdentifier (string s)
		{
			if (String.IsNullOrEmpty (s) || !(Char.IsLetter (s [0]) || s [0] == '_'))
				return false;

			foreach (char c in s)
				if (!Char.IsLetterOrDigit (c) && c != '_')
					return false;

			return true;
		}

		public override string GenerateGetSizeOf () {
			return "(uint) " + GenerationInfo.GetSizeOfExpression(abi_type);
		}
	}
}

