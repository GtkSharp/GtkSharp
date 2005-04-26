// mcs -pkg:gtkhtml-sharp -pkg:gtk-sharp gtk-html-sample.cs
using Gtk;
using System;
using System.IO;

class HTMLSample {
	static int Main (string [] args)
	{
		HTML html;
		Window win;
		Application.Init ();
		html = new HTML ();
		win = new Window ("Test");
		ScrolledWindow sw = new ScrolledWindow ();
		win.Add (sw);
		sw.Add (html);
		HTMLStream s = html.Begin ("text/html");

		if (args.Length > 0) {
			using (StreamReader r = File.OpenText (args [0]))
				s.Write (r.ReadToEnd ());
		} else {
			s.Write ("<html><body>");
			s.Write ("Hello world!");
		}
		
		html.End (s, HTMLStreamStatus.Ok);
		win.ShowAll ();
		Application.Run ();
		return 0;
	}
}

