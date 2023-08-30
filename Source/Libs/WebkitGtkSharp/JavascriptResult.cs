using System;
using System.Runtime.InteropServices;

namespace WebKit
{

	public partial class JavascriptResult
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr d_webkit_javascript_result_get_js_value(IntPtr raw);
		static d_webkit_javascript_result_get_js_value webkit_javascript_result_get_js_value = FuncLoader.LoadFunction<d_webkit_javascript_result_get_js_value>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_javascript_result_get_js_value"));

		public JavaScriptValue JsValue { 
			get {
				IntPtr raw_ret = webkit_javascript_result_get_js_value(Handle);
				JavaScriptValue ret = new JavaScriptValue(raw_ret);
				return ret;
			}
		}
		

	}

}