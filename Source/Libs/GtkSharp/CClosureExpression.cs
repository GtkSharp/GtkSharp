// CClosureExpression.cs - Gtk CClosureExpression class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class CClosureExpression {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_cclosure_expression_new(IntPtr value_type, IntPtr marshal, uint n_params, IntPtr parms, IntPtr callback_func, IntPtr user_data, IntPtr user_destroy);
		static d_gtk_cclosure_expression_new gtk_cclosure_expression_new = FuncLoader.LoadFunction<d_gtk_cclosure_expression_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_cclosure_expression_new"));

		/// <summary>
		/// An expression that computes its value by calling
		/// <paramref name="callback"/> with the this-object and the values of
		/// <paramref name="parms"/>.
		/// </summary>
		/// <remarks>
		/// Codegen emitted <b>no constructor at all</b> for this type:
		/// gtk_cclosure_expression_new takes a GClosureMarshal, a GCallback and a
		/// GClosureNotify, none of which SymbolTable maps, so the whole
		/// constructor was dropped and the class was left with nothing but its
		/// GType. It is the type a Gtk 4 list view reaches for to derive a
		/// display value from an item, so "unreachable" meant that job could not
		/// be done through the binding at all.
		///
		/// The marshaller is GObject's libffi one, so <paramref name="callback"/>
		/// is called with the C signature
		/// <c>(this_object, param1 .. paramN, user_data)</c> and must carry
		/// <c>[UnmanagedFunctionPointer (CallingConvention.Cdecl)]</c>.
		/// </remarks>
		public CClosureExpression (GLib.GType value_type, Delegate callback, params Gtk.Expression[] parms)
			: base (IntPtr.Zero)
		{
			Owned = true;
			uint n_params = parms == null ? 0 : (uint) parms.Length;
			IntPtr native_parms = Gtk.Expression.MarshalExpressionArray (parms);
			try {
				Raw = gtk_cclosure_expression_new (value_type.Val,
					ExpressionClosure.Marshaller,
					n_params, native_parms,
					Marshal.GetFunctionPointerForDelegate (callback),
					ExpressionClosure.Root (callback),
					ExpressionClosure.ReleaseNotify);
			} finally {
				if (native_parms != IntPtr.Zero)
					Marshal.FreeHGlobal (native_parms);
			}
		}
	}
}
