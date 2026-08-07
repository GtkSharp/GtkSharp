// Expression.cs - Gtk Expression class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class Expression {

		// gtk_expression_bind takes its receiver as (transfer full): the watch
		// it creates stores the expression and owns that reference. An api.xml
		// <method> describes the ownership of its PARAMETERS and has no way to
		// describe the instance's, so codegen passed Handle and the wrapper
		// went on owning a reference the callee had already eaten. Both then
		// unref, and the second one is a free of freed memory deferred onto the
		// main loop by the generated finalizer - so it lands wherever the GC
		// happens to run, which is why it looks like an unrelated flake. Same
		// shape as the twelve GskTransform builders; taking the reference here
		// keeps a method from destroying the object it was called on.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_expression_ref_transfer(IntPtr raw);
		static d_gtk_expression_ref_transfer gtk_expression_ref_transfer = FuncLoader.LoadFunction<d_gtk_expression_ref_transfer>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_expression_ref"));

		IntPtr Consumed {
			get {
				gtk_expression_ref_transfer (Handle);
				return Handle;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_expression_bind(IntPtr raw, IntPtr target, IntPtr property, IntPtr this_);
		static d_gtk_expression_bind gtk_expression_bind = FuncLoader.LoadFunction<d_gtk_expression_bind>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_expression_bind"));

		/// <summary>
		/// Keeps <paramref name="target"/>'s <paramref name="property"/> equal to
		/// what this expression evaluates to, re-running whenever anything the
		/// expression depends on changes. The watch dies with the target.
		/// </summary>
		public Gtk.ExpressionWatch Bind (GLib.Object target, string property, GLib.Object this_)
		{
			IntPtr native_property = GLib.Marshaller.StringToPtrGStrdup (property);
			IntPtr raw_ret = gtk_expression_bind (Consumed,
				target == null ? IntPtr.Zero : target.Handle,
				native_property,
				this_ == null ? IntPtr.Zero : this_.Handle);
			GLib.Marshaller.Free (native_property);
			return raw_ret == IntPtr.Zero ? null
				: (Gtk.ExpressionWatch) GLib.Opaque.GetOpaque (raw_ret, typeof (Gtk.ExpressionWatch), false);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_expression_evaluate(IntPtr raw, IntPtr this_, IntPtr value);
		static d_gtk_expression_evaluate gtk_expression_evaluate = FuncLoader.LoadFunction<d_gtk_expression_evaluate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_expression_evaluate"));

		/// <summary>
		/// Evaluates the expression against <paramref name="this_"/>. Returns
		/// false - leaving <paramref name="value"/> empty - when some object on
		/// the way to the answer is null.
		/// </summary>
		/// <remarks>
		/// gtk_expression_evaluate writes into a GValue the CALLER provides, and
		/// requires it to be zeroed (G_VALUE_INIT) because it g_value_inits it
		/// to <see cref="ValueType"/> itself. Codegen bound it as a by-value
		/// parameter and freed the unmanaged copy without reading it back, so
		/// the result was discarded on every call.
		/// </remarks>
		public bool Evaluate (GLib.Object this_, out GLib.Value value)
		{
			// default(GLib.Value) is a zeroed GValue, which is G_VALUE_INIT.
			value = default (GLib.Value);
			IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
			bool ret = gtk_expression_evaluate (Handle,
				this_ == null ? IntPtr.Zero : this_.Handle, native_value);
			value = (GLib.Value) Marshal.PtrToStructure (native_value, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_value);
			return ret;
		}

		/// <summary>
		/// Calls <paramref name="notify"/> whenever the value this expression
		/// evaluates to against <paramref name="this_"/> may have changed.
		/// </summary>
		public Gtk.ExpressionWatch Watch (GLib.Object this_, Gtk.ExpressionNotify notify)
		{
			return Watch (this_ == null ? IntPtr.Zero : this_.Handle, notify);
		}

		// Builds a GtkExpression** for the "array plus count" constructors,
		// taking the reference each of them eats. Callers free the block.
		internal static IntPtr MarshalExpressionArray (Gtk.Expression[] expressions)
		{
			if (expressions == null || expressions.Length == 0)
				return IntPtr.Zero;

			IntPtr array = Marshal.AllocHGlobal (IntPtr.Size * expressions.Length);
			for (int i = 0; i < expressions.Length; i++) {
				if (expressions [i] == null)
					throw new ArgumentNullException ("expressions[" + i + "]");
				// OwnedCopy is gtk_expression_ref + hand the pointer over.
				Marshal.WriteIntPtr (array, i * IntPtr.Size, expressions [i].OwnedCopy);
			}
			return array;
		}
	}
}
