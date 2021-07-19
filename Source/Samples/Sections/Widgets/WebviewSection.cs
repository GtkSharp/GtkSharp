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
			AddItem(ShowHtml());
			AddItem(ShowUri());

		}

		public (string, Widget) ShowHtml()
		{
			var webView = new WebView {
				HeightRequest = 100,
				Hexpand = true
			};

			webView.LoadString(
				$"This is a <b>{nameof(WebView)}</b> showing html text",
				"text/html", "UTF-8", null
			);

			return ($"{nameof(WebView)} show html text:", webView);
		}

		public (string, Widget) ShowUri()
		{
			var webView = new WebView {ViewMode = WebViewViewMode.Floating};

			var scroll = new ScrolledWindow {
				Child = webView,
				Vexpand = true,
				Hexpand = true,
				PropagateNaturalHeight = true,
				PropagateNaturalWidth = true
			};

			webView.LoadUri("https://github.com/GtkSharp/GtkSharp#readme");

			return ($"{nameof(WebView)} show uri:", scroll);
		}

	}

}