// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

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
				AddItem(($"{nameof(WebKit.WebView)}",new Label($"{typeof(WebView).Namespace} is not suported on your OS")));
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

			webView.LoadHtml($"This is a <b>{nameof(WebView)}</b> showing html text");
			return ($"{nameof(WebView)} show html text:", webView);
		}

		public (string, Widget) ShowUri()
		{
			var webView = new WebView {
					Vexpand = true,
					Hexpand = true,
			};

			webView.LoadUri("https://github.com/GtkSharp/GtkSharp#readme");

			return ($"{nameof(WebView)} show uri:", webView);
		}

	}

}