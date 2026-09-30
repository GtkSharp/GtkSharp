// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
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
	using System.Collections.Generic;
	using System.Xml;

	public class ArrayParameter : Parameter {

		bool null_terminated;

		public ArrayParameter (XmlElement elem) : base (elem)
		{
			null_terminated = elem.GetAttributeAsBoolean ("null_term_array");
			if (elem.HasAttribute ("array_len"))
				FixedArrayLength = Int32.Parse (elem.GetAttribute ("array_len"));
		}

		public override string MarshalType {
			get {
				if (Generatable is StructBase)
					return CSType;
				else
					return base.MarshalType;
			}
		}

		bool NullTerminated {
			get {
				return null_terminated;
			}
		}

		public int? FixedArrayLength { get; private set; }

		string MarshalElement {
			get { return MarshalType.TrimEnd ('[', ']'); }
		}

		string CSElement {
			get { return CSType.TrimEnd ('[', ']'); }
		}

		/// <summary>
		/// A fixed-size array parameter is a buffer of N elements the C
		/// declaration sizes itself -- `float v[16]`, `GdkRGBA colour[4]`.
		/// </summary>
		/// <remarks>
		/// It is never passed as a C# <c>out</c> to the native call, whichever
		/// direction it has: the callee wants a pointer to N elements, and a
		/// pinned managed array already is one. `out T[]` would hand it a
		/// <c>T**</c>. The managed signature still says <c>out</c> for a buffer
		/// the callee fills, because the array is allocated here.
		/// </remarks>
		public override string NativeSignature {
			get {
				if (FixedArrayLength.HasValue)
					return MarshalType + " " + Name;

				return base.NativeSignature;
			}
		}

		public override string[] Prepare {
			get {
				if (FixedArrayLength.HasValue)
					return PrepareFixed ();

				if (CSType == MarshalType)
					return new string [0];

				var result = new List<string> ();

				result.Add (String.Format ("int cnt_{0} = {0} == null ? 0 : {0}.Length;", CallName));
				result.Add (String.Format ("{0}[] native_{1} = new {0} [cnt_{1}" + (NullTerminated ? " + 1" : "") + "];", MarshalType.TrimEnd('[', ']'), CallName));
				result.Add (String.Format ("for (int i = 0; i < cnt_{0}; i++)", CallName));
				IGeneratable gen = Generatable;
				if (gen is IManualMarshaler)
					result.Add (String.Format ("\tnative_{0} [i] = {1};", CallName, (gen as IManualMarshaler).AllocNative (CallName + "[i]")));
				else
					result.Add (String.Format ("\tnative_{0} [i] = {1};", CallName, gen.CallByName (CallName + "[i]")));

				if (NullTerminated)
					result.Add (String.Format ("native_{0} [cnt_{0}] = IntPtr.Zero;", CallName));
				return result.ToArray ();
			}
		}

		string[] PrepareFixed ()
		{
			var result = new List<string> ();
			int n = FixedArrayLength.Value;

			if (PassAs == "out") {
				// The callee fills the buffer, so this side sizes it. The C
				// declaration is the only thing that says how big, which is why
				// the length has to survive into the api.xml.
				if (CSType == MarshalType)
					return new string [] { String.Format ("{0} = new {1} [{2}];", CallName, MarshalElement, n) };

				return new string [] { String.Format ("{0}[] native_{1} = new {0} [{2}];", MarshalElement, CallName, n) };
			}

			// An input buffer: the callee reads exactly n elements and there is no
			// count argument to tell it otherwise, so a short array is an overrun
			// with nothing to catch it. C cannot check this; managed code can.
			result.Add (String.Format (
				"if ({0} == null || {0}.Length != {1}) throw new ArgumentException (\"must have exactly {1} elements\", \"{0}\");",
				CallName, n));

			if (CSType != MarshalType) {
				result.Add (String.Format ("{0}[] native_{1} = new {0} [{2}];", MarshalElement, CallName, n));
				result.Add (String.Format ("for (int i = 0; i < {0}; i++)", n));
				result.Add (String.Format ("\tnative_{0} [i] = {1};", CallName, Generatable.CallByName (CallName + "[i]")));
			}

			return result.ToArray ();
		}

		public override string CallString {
			get {
				if (FixedArrayLength.HasValue)
					return CSType == MarshalType ? CallName : "native_" + CallName;
				else if (CSType != MarshalType)
					return "native_" + CallName;
				else
					return CallName;
			}
		}

		public override string[] Finish {
			get {
				if (CSType == MarshalType)
					return new string [0];

				if (FixedArrayLength.HasValue) {
					if (PassAs != "out")
						return new string [0];

					int n = FixedArrayLength.Value;
					return new string [] {
						String.Format ("{0} = new {1} [{2}];", CallName, CSElement, n),
						String.Format ("for (int i = 0; i < {0}; i++)", n),
						String.Format ("\t{0} [i] = {1};", CallName, Generatable.FromNative ("native_" + CallName + "[i]"))
					};
				}

				IGeneratable gen = Generatable;
				if (gen is IManualMarshaler) {
					string [] result = new string [4];
					result [0] = "for (int i = 0; i < native_" + CallName + ".Length" + (NullTerminated ? " - 1" : "") + "; i++) {";
					result [1] = "\t" + CallName + " [i] = " + Generatable.FromNative ("native_" + CallName + "[i]") + ";";
					result [2] = "\t" + (gen as IManualMarshaler).ReleaseNative ("native_" + CallName + "[i]") + ";";
					result [3] = "}";
					return result;
				}

				return new string [0];
			}
		}
	}

	public class ArrayCountPair : ArrayParameter {

		XmlElement count_elem;
		bool invert;

		public ArrayCountPair (XmlElement array_elem, XmlElement count_elem, bool invert) : base (array_elem)
		{
			this.count_elem = count_elem;
			this.invert = invert;
		}

		string CountNativeType {
			get {
				return SymbolTable.Table.GetMarshalType(count_elem.GetAttribute("type"));
			}
		}

		string CountType {
			get {
				return SymbolTable.Table.GetCSType(count_elem.GetAttribute("type"));
			}
		}

		string CountCast {
			get {
				if (CountType == "int")
					return String.Empty;
				else
					return "(" + CountType + ") ";
			}
		}

		string CountName {
			get {
				return SymbolTable.Table.MangleName (count_elem.GetAttribute("name"));
			}
		}

		string CallCount (string name)
		{
			string result = CountCast + "(" + name + " == null ? 0 : " + name + ".Length)";
			IGeneratable gen = SymbolTable.Table[count_elem.GetAttribute("type")];
			return gen.CallByName (result);
		}

		public override string CallString {
			get {
				if (invert)
					return CallCount (CallName) + ", " + base.CallString;
				else
					return base.CallString + ", " + CallCount (CallName);
			}
		}

		public override string NativeSignature {
			get {
				if (invert)
					return CountNativeType + " " + CountName + ", " + MarshalType + " " + Name;
				else
					return MarshalType + " " + Name + ", " + CountNativeType + " " + CountName;
			}
		}
	}
}
