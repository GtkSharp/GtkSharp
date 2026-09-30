// ExpressionWatch.cs - Gtk ExpressionWatch class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ExpressionWatch {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_expression_watch_evaluate(IntPtr raw, IntPtr value);
		static d_gtk_expression_watch_evaluate gtk_expression_watch_evaluate = FuncLoader.LoadFunction<d_gtk_expression_watch_evaluate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_expression_watch_evaluate"));

		/// <summary>
		/// Re-evaluates the watched expression against the same this-object it
		/// was created with. Returns false, leaving <paramref name="value"/>
		/// empty, once the expression can no longer be evaluated.
		/// </summary>
		/// <remarks>
		/// Caller-allocates, exactly like <see cref="Expression.Evaluate"/>: the
		/// generated binding copied a Value in and freed it again without
		/// reading it back.
		/// </remarks>
		public bool Evaluate (out GLib.Value value)
		{
			value = default (GLib.Value);
			IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
			bool ret = gtk_expression_watch_evaluate (Handle, native_value);
			value = (GLib.Value) Marshal.PtrToStructure (native_value, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_value);
			return ret;
		}
	}
}
