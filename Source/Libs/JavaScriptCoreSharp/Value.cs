// Value.cs - JavaScriptCore.Value customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace JavaScriptCore {

	using System;
	using System.Runtime.InteropServices;

	public partial class Value {

		// jsc_value_function_callv, _constructor_callv and _object_invoke_methodv
		// all take (guint n_parameters, JSCValue **parameters): an array whose
		// length is a sibling parameter, which codegen has no rule for. All three
		// came out as
		//
		//     Value FunctionCallv (uint n_parameters, Value parameters)
		//
		// passing one Value's handle where an array of handles belongs, so JSC
		// read that Value's own GObject header as parameters[0] and the count
		// came from the caller rather than from anything real. Calling a
		// JavaScript function with arguments was not possible: zero arguments
		// worked by accident, one was garbage, and more read past the end.
		//
		// Same family as gsk_container_node_new. Hidden in the metadata and
		// rebound here over Value[], where the count is the array's own length
		// and cannot disagree with it.

		static IntPtr[] Handles (Value[] parameters, string name)
		{
			if (parameters == null)
				return new IntPtr [0];

			var handles = new IntPtr [parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters [i] == null)
					throw new ArgumentException ("a JavaScript argument cannot be null; use a JSC null or undefined value", name);
				handles [i] = parameters [i].Handle;
			}

			return handles;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_jsc_value_function_callv(IntPtr raw, uint n_parameters, IntPtr[] parameters);
		static d_jsc_value_function_callv jsc_value_function_callv = FuncLoader.LoadFunction<d_jsc_value_function_callv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.JavaScriptCore), "jsc_value_function_callv"));

		/// <summary>Calls this value as a function, with the given arguments.</summary>
		public Value FunctionCall (params Value[] parameters)
		{
			IntPtr[] handles = Handles (parameters, "parameters");
			IntPtr raw_ret = jsc_value_function_callv (Handle, (uint) handles.Length, handles);

			GC.KeepAlive (parameters);
			return GLib.Object.GetObject (raw_ret, true) as Value;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_jsc_value_constructor_callv(IntPtr raw, uint n_parameters, IntPtr[] parameters);
		static d_jsc_value_constructor_callv jsc_value_constructor_callv = FuncLoader.LoadFunction<d_jsc_value_constructor_callv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.JavaScriptCore), "jsc_value_constructor_callv"));

		/// <summary>Calls this value as a constructor, as <c>new</c> would.</summary>
		public Value ConstructorCall (params Value[] parameters)
		{
			IntPtr[] handles = Handles (parameters, "parameters");
			IntPtr raw_ret = jsc_value_constructor_callv (Handle, (uint) handles.Length, handles);

			GC.KeepAlive (parameters);
			return GLib.Object.GetObject (raw_ret, true) as Value;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_jsc_value_object_invoke_methodv(IntPtr raw, IntPtr name, uint n_parameters, IntPtr[] parameters);
		static d_jsc_value_object_invoke_methodv jsc_value_object_invoke_methodv = FuncLoader.LoadFunction<d_jsc_value_object_invoke_methodv>(FuncLoader.GetProcAddress(GLibrary.Load(Library.JavaScriptCore), "jsc_value_object_invoke_methodv"));

		/// <summary>Invokes <paramref name="name"/> on this object, with
		/// <c>this</c> bound to it.</summary>
		public Value ObjectInvokeMethod (string name, params Value[] parameters)
		{
			IntPtr[] handles = Handles (parameters, "parameters");
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);

			try {
				IntPtr raw_ret = jsc_value_object_invoke_methodv (
					Handle, native_name, (uint) handles.Length, handles);

				GC.KeepAlive (parameters);
				return GLib.Object.GetObject (raw_ret, true) as Value;
			} finally {
				GLib.Marshaller.Free (native_name);
			}
		}
	}
}
