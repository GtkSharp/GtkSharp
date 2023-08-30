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

			webView.LoadHtml($"This is a <b>{nameof(WebView)}</b> showing html text");

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
			
			userContentManager.RegisterScriptMessageHandler(messageHandlerName);

			userContentManager.ScriptMessageReceived += (o, args) => {
				var value = args.JsResult?.JsValue;

				if (value is { IsString: true } v)
					ApplicationOutput.WriteLine($"{nameof(userContentManager.ScriptMessageReceived)}:\t{nameof(JavascriptResult.JsValue)}\t{v?.ToString()}");

			};

			webView.LoadHtml($"This is a <b>{nameof(WebView)}</b> with {nameof(UserScript)}" +
			                 "<br/>Send message <input id=\"clickMeButton\" type=\"button\" value=\"Submit\" class=\"button\" onclick=\"\">");

			webView.LoadChanged += (s, e) => {
				ApplicationOutput.WriteLine(s, $"{e.LoadEvent}");

				if (e.LoadEvent != LoadEvent.Finished)
					return;

				webView.RunJavascript("testFunc()", null, HandleJavaScriptResult);

			};

			void HandleJavaScriptResult(object source_object, IAsyncResult res)
			{
				if (source_object is not WebView view) return;

				try {
					JavascriptResult js_result = view.RunJavascriptFinish(res);

					if (js_result.JsValue is { } jsValue) {
						if (jsValue.IsString) {
							ApplicationOutput.WriteLine($"{nameof(webView.RunJavascriptFinish)}:\t{nameof(JavascriptResult.JsValue)}\t{jsValue.ToString()}");
						}
					}

				} catch (Exception exception) {
					ApplicationOutput.WriteLine($"{nameof(webView.RunJavascriptFinish)} throws:\n{exception.Message}");
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