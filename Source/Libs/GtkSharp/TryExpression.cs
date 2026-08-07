// TryExpression.cs - Gtk TryExpression class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class TryExpression {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_try_expression_new(uint n_expressions, IntPtr expressions);
		static d_gtk_try_expression_new gtk_try_expression_new = FuncLoader.LoadFunction<d_gtk_try_expression_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_try_expression_new"));

		/// <summary>
		/// Evaluates each expression in turn and yields the first that succeeds;
		/// fails only when all of them do. Added in Gtk 4.22.
		/// </summary>
		/// <remarks>
		/// "GtkExpression **expressions plus a count" is the shape codegen has no
		/// rule for, so the generated constructor took a single Expression and
		/// GTK read that expression's own first machine word as element zero of
		/// the array.
		/// </remarks>
		public TryExpression (params Gtk.Expression[] expressions)
			: base (IntPtr.Zero)
		{
			Owned = true;
			uint n = expressions == null ? 0 : (uint) expressions.Length;
			IntPtr native = Gtk.Expression.MarshalExpressionArray (expressions);
			try {
				Raw = gtk_try_expression_new (n, native);
			} finally {
				if (native != IntPtr.Zero)
					Marshal.FreeHGlobal (native);
			}
		}
	}
}
