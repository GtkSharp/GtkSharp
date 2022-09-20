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

			webView.LoadHtml($"<span id='sometext'>This is a <b>{nameof(WebView)}</b> showing html text</span>");



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
			webView.LoadChanged += (s, e) => {
				ApplicationOutput.WriteLine(s, $"{e.LoadEvent}");

				if (JavaScriptCore.Global.IsSupported) {
					webView.RunJavascript("window.document.getElementById('sometext')", null,
						(o, res) => {
							if (o is not WebView view)
								return;

							try {
								JavascriptResult js_result = view.RunJavascriptFinish(res);

								if (!js_result.Equals(JavascriptResult.Zero)) { // this is always false
			
								}
								if (js_result.JsValue != null) {
									var r = js_result.JsValue;
								}

							} catch (Exception exception) {
								ApplicationOutput.WriteLine(s, $"{e.LoadEvent}:\t{nameof(webView.RunJavascriptFinish)} throws {exception.Message}:{exception.StackTrace}");
							}
						}
					);
				}
			};
			return ($"{nameof(WebView)} show uri:", webView);
		}

	}

}