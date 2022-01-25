namespace JavaScriptCore {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using static GLib.AbiStructExtension;

	public partial class Context
	{

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void d_jsc_context_push_exception_handler(IntPtr raw, JavaScriptCoreSharp.ExceptionHandlerNative handler, IntPtr user_data, GLib.DestroyNotify destroy_notify);
		static d_jsc_context_push_exception_handler jsc_context_push_exception_handler = FuncLoader.LoadFunction<d_jsc_context_push_exception_handler>(FuncLoader.GetProcAddress(GLibrary.Load(Library.JavaScriptCore), "jsc_context_push_exception_handler"));

		public void PushExceptionHandler(JavaScriptCore.ExceptionHandler handler) {
			JavaScriptCoreSharp.ExceptionHandlerWrapper handler_wrapper = new JavaScriptCoreSharp.ExceptionHandlerWrapper (handler);
			IntPtr user_data;
			GLib.DestroyNotify destroy_notify;
			if (handler == null) {
				user_data = IntPtr.Zero;
				destroy_notify = null;
			} else {
				user_data = (IntPtr) GCHandle.Alloc (handler_wrapper);
				destroy_notify = GLib.DestroyHelper.NotifyHandler;
			}
			jsc_context_push_exception_handler(Handle, handler_wrapper.NativeDelegate, user_data, destroy_notify);
		}

	}

}