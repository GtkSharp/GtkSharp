using System;
using System.Threading.Tasks;

namespace WebKit
{

	public partial class WebView
	{

		public Task<JavascriptResult> RunJavascriptAsync(string script)
		{
			var tcs = new TaskCompletionSource<JavascriptResult>();
			IntPtr native_script = GLib.Marshaller.StringToPtrGStrdup(script);

			void Callback(IntPtr sourceObject, IntPtr res, IntPtr userData)
			{
				var jsResult = webkit_web_view_run_javascript_finish(sourceObject, res, out var error);
				WebKit.JavascriptResult ret = WebKit.JavascriptResult.New(jsResult);

				if (error != IntPtr.Zero) throw new GLib.GException(error);

				tcs.SetResult(ret);
			}

			webkit_web_view_run_javascript(Handle, native_script, IntPtr.Zero, Callback, IntPtr.Zero);

			return tcs.Task;
		}

	}

}