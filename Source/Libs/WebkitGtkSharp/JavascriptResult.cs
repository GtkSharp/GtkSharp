using System;
using System.Runtime.InteropServices;
using JavaScript;

namespace WebKit
{

	public partial class JavascriptResult
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_webkit_javascript_result_get_js_value(IntPtr raw);
		static d_webkit_javascript_result_get_js_value webkit_javascript_result_get_js_value = FuncLoader.LoadFunction<d_webkit_javascript_result_get_js_value>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_javascript_result_get_js_value"));

		public Value JsValue { 
			get {
				IntPtr raw_ret = webkit_javascript_result_get_js_value(Handle);
				Value ret = new Value(raw_ret);
				return ret;
			}
		}
		
		static bool initialized = false;
		static JavascriptResult()
		{
			GtkSharp.WebkitGtkSharp.ObjectManager.Initialize();
			
			if (initialized)
				return;
			
			GLib.GType.Register (JavascriptResult.GType, typeof (JavascriptResult));

			initialized = true;
		}

	}

}