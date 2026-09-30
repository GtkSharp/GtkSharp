// ClosureExpression.cs - Gtk ClosureExpression class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	/// The plumbing shared by <see cref="ClosureExpression"/> and
	/// <see cref="CClosureExpression"/>: turning a managed delegate into
	/// something GObject will invoke.
	/// </summary>
	/// <remarks>
	/// Both use g_cclosure_marshal_generic, GObject's libffi marshaller, which
	/// calls the callback with the C signature the GValues describe:
	/// <c>(this_object, param1 .. paramN, user_data)</c>. The delegate handed in
	/// is kept alive by a GCHandle passed as that user_data and released by the
	/// closure's destroy notify, so it survives exactly as long as the closure
	/// does - a delegate rooted only by the caller's local would otherwise be
	/// collected while GTK still held its function pointer.
	/// </remarks>
	internal static class ExpressionClosure {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void GClosureNotify(IntPtr data, IntPtr closure);

		static readonly GClosureNotify release = ReleaseCallback;

		static void ReleaseCallback (IntPtr data, IntPtr closure)
		{
			if (data == IntPtr.Zero)
				return;
			GCHandle gch = (GCHandle) data;
			if (gch.IsAllocated)
				gch.Free ();
		}

		internal static IntPtr Marshaller {
			get { return FuncLoader.GetProcAddress (GLibrary.Load (Library.GObject), "g_cclosure_marshal_generic"); }
		}

		internal static IntPtr ReleaseNotify {
			get { return Marshal.GetFunctionPointerForDelegate (release); }
		}

		/// <summary>Roots <paramref name="callback"/> and returns the handle to pass as user_data.</summary>
		internal static IntPtr Root (Delegate callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");
			return (IntPtr) GCHandle.Alloc (callback);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_cclosure_new(IntPtr callback_func, IntPtr user_data, IntPtr destroy_data);
		static d_g_cclosure_new g_cclosure_new = FuncLoader.LoadFunction<d_g_cclosure_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_cclosure_new"));

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_closure_set_marshal(IntPtr closure, IntPtr marshal);
		static d_g_closure_set_marshal g_closure_set_marshal = FuncLoader.LoadFunction<d_g_closure_set_marshal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_closure_set_marshal"));

		/// <summary>A floating GClosure around <paramref name="callback"/>.</summary>
		internal static IntPtr New (Delegate callback)
		{
			IntPtr closure = g_cclosure_new (Marshal.GetFunctionPointerForDelegate (callback),
			                                 Root (callback), ReleaseNotify);
			g_closure_set_marshal (closure, Marshaller);
			return closure;
		}
	}

	public partial class ClosureExpression {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_closure_expression_new(IntPtr value_type, IntPtr closure, uint n_params, IntPtr parms);
		static d_gtk_closure_expression_new gtk_closure_expression_new = FuncLoader.LoadFunction<d_gtk_closure_expression_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_closure_expression_new"));

		/// <summary>
		/// An expression whose value is what <paramref name="closure"/> returns
		/// when invoked with the this-object and the values of
		/// <paramref name="parms"/>.
		/// </summary>
		/// <remarks>
		/// The api.xml says "GtkExpression** params, guint n_params", a shape
		/// codegen has no rule for, so the generated constructor took a single
		/// Expression and GTK read that expression's own first machine word -
		/// its vtable pointer - as element zero of the array.
		/// </remarks>
		public ClosureExpression (GLib.GType value_type, IntPtr closure, params Gtk.Expression[] parms)
			: base (IntPtr.Zero)
		{
			Owned = true;
			uint n_params = parms == null ? 0 : (uint) parms.Length;
			IntPtr native_parms = Gtk.Expression.MarshalExpressionArray (parms);
			try {
				Raw = gtk_closure_expression_new (value_type.Val, closure, n_params, native_parms);
			} finally {
				if (native_parms != IntPtr.Zero)
					Marshal.FreeHGlobal (native_parms);
			}
		}

		/// <summary>
		/// The same, over a managed delegate. GObject's generic marshaller calls
		/// it as <c>(this_object, param1 .. paramN, user_data)</c>, returning the
		/// C type <paramref name="value_type"/> describes.
		/// </summary>
		public ClosureExpression (GLib.GType value_type, Delegate callback, params Gtk.Expression[] parms)
			: this (value_type, ExpressionClosure.New (callback), parms)
		{
		}
	}
}
