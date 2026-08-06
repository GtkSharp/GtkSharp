// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			AddItem(ShowJavaScript());
			AddItem(ShowUri());

		}

		public (string, Widget) ShowHtml()
		{
			var webView = new WebView {
				HeightRequest = 100,
				WidthRequest = 400,
				Hexpand = true
			};

			webView.LoadHtml($"This is a <b>{nameof(WebView)}</b> showing html text", null);

			return ($"{nameof(WebView)} show html text:", webView);
		}

		public (string, Widget) ShowJavaScript()
		{
			var webView = new WebView {
				HeightRequest = 100,
				WidthRequest = 400,
				Hexpand = true
			};

			webView.Settings.EnableDeveloperExtras = true;
			var userContentManager = webView.UserContentManager;

			var messageHandlerName = "gtksharp";

			var script = new UserScript(
				source: $"function testFunc() {{\n" +
				        $"window.webkit.messageHandlers.{messageHandlerName}.postMessage(\"postMessage\");\n" +
				        $"return 'Success' }};\n",
				UserContentInjectedFrames.AllFrames,
				UserScriptInjectionTime.Start, null, null);

			userContentManager.AddScript(script);
			
			var buttonClickPostMessage = $"var button = document.getElementById(\"clickMeButton\");\n" +
			                  $"button.addEventListener(\"click\", " +
			                  $"function() {{varmessageToPost = {{'ButtonId':'clickMeButton'}};\n" +
			                  $"window.webkit.messageHandlers.{messageHandlerName}.postMessage(\"clickMeButton clicked\");\n}},false);";
			
			var script2 = new UserScript(
				source: buttonClickPostMessage,
				UserContentInjectedFrames.AllFrames,
				UserScriptInjectionTime.End, null, null);
			
			
			userContentManager.AddScript(script2);
			
			userContentManager.RegisterScriptMessageHandler(messageHandlerName, null);

			userContentManager.ScriptMessageReceived += (o, args) => {
				// WebKit 6 delivers a JSCValue rather than a WebKitJavascriptResult.
				// JavaScriptCore has its own gir and is not one of the bound
				// assemblies, so the value arrives as an opaque handle and cannot
				// be decoded here; that the message was delivered at all is what
				// this demonstrates.
				ApplicationOutput.WriteLine(
					$"{nameof(userContentManager.ScriptMessageReceived)}:\tvalue handle {args.Value}");
			};

			webView.LoadHtml($"This is a <b>{nameof(WebView)}</b> with {nameof(UserScript)}" +
			                 "<br/>Send message <input id=\"clickMeButton\" type=\"button\" value=\"Submit\" class=\"button\" onclick=\"\">",
			                 null);

			webView.LoadChanged += (s, e) => {
				ApplicationOutput.WriteLine(s, $"{e.LoadEvent}");

				if (e.LoadEvent != LoadEvent.Finished)
					return;

				// run_javascript became evaluate_javascript, which also takes the
				// world name and a source URI for attributing errors.
				webView.EvaluateJavascript("testFunc()", null, null, null, HandleJavaScriptResult);

			};

			void HandleJavaScriptResult(GLib.Object source_object, GLib.IAsyncResult res, IntPtr data)
			{
				if (source_object is not WebView view) return;

				try {
					// WebKitJavascriptResult is gone: the finish call returns the
					// JSCValue itself. Unbound, so it is an opaque handle here --
					// a non-zero handle means the script ran and produced a value,
					// and a failure still arrives as an exception.
					IntPtr js_value = view.EvaluateJavascriptFinish(res);

					ApplicationOutput.WriteLine(
						$"{nameof(view.EvaluateJavascriptFinish)}:\tvalue handle {js_value}");

				} catch (Exception exception) {
					ApplicationOutput.WriteLine($"{nameof(view.EvaluateJavascriptFinish)} throws:\n{exception.Message}");
				}
			}

			return ($"{nameof(WebView)} with {nameof(UserScript)}:", webView);
		}

		public (string, Widget) ShowUri()
		{
			var webView = new WebView {
				WidthRequest = 400,

				Vexpand = true,
				Hexpand = true,
			};

			webView.LoadUri("https://github.com/GtkSharp/GtkSharp#readme");

			return ($"{nameof(WebView)} show uri:", webView);
		}

	}

}