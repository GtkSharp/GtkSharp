// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Atk;
using Gdk;
using Gtk;
using WebKit;
using IAsyncResult = GLib.IAsyncResult;
using Object = GLib.Object;

namespace Samples
{

	[Section(ContentType = typeof(WebView), Category = Category.Widgets)]
	class WebviewSection : ListSection
	{

		public WebviewSection()
		{
			if (!WebKit.Global.IsSupported) {
				AddItem(($"{nameof(WebKit.WebView)}", new Label($"{typeof(WebView).Namespace} is not suported on your OS")));

				return;
			}

			AddItem(ShowHtml());
			AddItem(ShowUri());

		}

		public (string, Widget) ShowHtml()
		{
			var webView = new WebView {
				HeightRequest = 100,
				Hexpand = true
			};

			var ucm = webView.UserContentManager;

			if (false) {
				var script = UserScript.New(
					source: "function testFunc() { return 'Success' }",
					UserContentInjectedFrames.AllFrames,
					UserScriptInjectionTime.End, new string[0], new string [0]);

				// this crashes:
				ucm.AddScript(script);
			}

			void HandleJavaScriptResult(Object o, IAsyncResult res)
			{
				if (o is not WebView view) return;

				try {
					JavascriptResult js_result = view.RunJavascriptFinish(res);

					if (js_result.JsValue != null) {
						var r = js_result.JsValue;
					}

				} catch (Exception exception) {
					ApplicationOutput.WriteLine($"{nameof(webView.RunJavascriptFinish)} throws:\n {exception.Message}");
				}
			}

			webView.LoadHtml($"<!DOCTYPE html>" +
			                 "<script>function testFunc() { return 'Success' }</script>" +
			                 $"<html><span id='sometext'>This is a <b>{nameof(WebView)}</b> showing html text</span>" +
			                 $"</html>");

			webView.LoadChanged += (s, e) => {
				ApplicationOutput.WriteLine(s, $"{e.LoadEvent}");

				if (e.LoadEvent != LoadEvent.Finished)
					return;

				if (JavaScriptCore.Global.IsSupported) {
					webView.RunJavascript("testFunc()", null, HandleJavaScriptResult);
					// alert('Hello') : works, shows alert box
					// document.getElementById(\"sometext\") : throws exception
				}
			};

			return ($"{nameof(WebView)} show html text:", webView);
		}

		public (string, Widget) ShowUri()
		{
			var webView = new WebView {
				Vexpand = true,
				Hexpand = true,
			};

			webView.Settings.EnableJavascript = true;
			webView.LoadUri("https://github.com/GtkSharp/GtkSharp#readme");

			return ($"{nameof(WebView)} show uri:", webView);
		}

	}

}