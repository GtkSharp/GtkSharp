using Gtk;
using GtkSharp;
using System;
// mcs -r gtkhtml-sharp -r gtk-sharp gtk-html-sample.cs

class HTMLSample {
	static int Main (string [] args)
	{
		HTML html;
		Window win;
		Application.Init ();
		html = new HTML ();
		win = new Window ("Test");
		win.Add (html);
		HTMLStream s = html.Begin ("text/html");
		s.Write ("<html><body>");
		s.Write ("Hello world!");
		html.End (s, HTMLStreamStatus.Ok);
		win.ShowAll ();
		Application.Run ();
		return 0;
	}
}

