// GtkSharp.Generation.CBoolGen.cs - C99 `bool` as a Generatable.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.Generation {

	using System;

	/// <summary>
	/// C99's <c>bool</c> (<c>_Bool</c>), which is <b>not</b> <c>gboolean</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// GLib's <c>gboolean</c> is a <c>gint</c> - four bytes - and the symbol
	/// table maps it onto C#'s <c>bool</c>, whose default marshalling in a
	/// P/Invoke signature is the four-byte Win32 <c>BOOL</c>. That pairing is
	/// correct, and it is why almost every gir in this tree needs nothing more.
	/// </para>
	/// <para>
	/// Graphene is the exception: its headers include &lt;stdbool.h&gt; and its
	/// girs say <c>&lt;type name="gboolean" c:type="bool"/&gt;</c>. GirToGapi
	/// writes the C type through, so the api.xml says <c>bool</c> - which the
	/// symbol table did not know at all, so <b>every</b> predicate in graphene
	/// was silently dropped by codegen: 49 methods, among them
	/// <c>graphene_matrix_inverse</c>, <c>graphene_rect_contains_point</c>,
	/// <c>graphene_rect_intersection</c> and every <c>_equal</c>/<c>_near</c>.
	/// </para>
	/// <para>
	/// <c>_Bool</c> occupies one byte, and only the low byte of the return
	/// register is architecturally defined, so it is marshalled as a
	/// <c>byte</c> and compared against zero here rather than being handed to
	/// the runtime as a <c>bool</c>. Reading four bytes would depend on the
	/// compiler that built graphene having zero-extended them - which gcc and
	/// clang do and MSVC does not promise.
	/// </para>
	/// </remarks>
	public class CBoolGen : SimpleGen {

		public CBoolGen (string ctype) : base (ctype, "bool", "false") {}

		public override string MarshalType {
			get {
				return "byte";
			}
		}

		public override string CallByName (string var_name)
		{
			return "(" + var_name + " ? (byte) 1 : (byte) 0)";
		}

		public override string FromNative (string var)
		{
			return "(" + var + " != 0)";
		}
	}
}
