// GladeViewer.cs - Silly tests for LibGlade in C#
//
// Author: Ricardo Fernández Pascual <ric@users.sourceforge.net>
//
// (c) 2002 Ricardo Fernández Pascual

namespace GladeSamples {
	using System;
	using System.Runtime.InteropServices;

	using Gtk;
	using Gnome;
	using Glade;

	public class GladeDemo {

		// temporary hack until GList is wrapped
		[DllImport("glib-2.0")]
		static extern IntPtr g_list_nth_data (IntPtr l, int i);
		[DllImport("glib-2.0")]
		static extern int g_list_length (IntPtr l);

		public static void Main (string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("Use: mono ./glade-viewer.exe \"fname\" \"root\"");
				return;
			}

			Program viewer = new Program ("GladeViewer", "0.1", Modules.UI, args);

			string fname = args [0];
			string root = args [1];
			
			Glade.XML gxml = new Glade.XML (fname, root, null);
			Widget wid = gxml [root];
			wid.Show ();
			
			Console.WriteLine ("The filename: {0}", gxml.Filename);
			Console.WriteLine ("A relative filename: {0}", gxml.RelativeFile ("image.png"));

			Console.WriteLine ("The name of the root widget: {0}", Glade.XML.GetWidgetName (wid));
			Console.WriteLine ("BTW, it's {0} that it was created using from a Glade.XML object",
					   Glade.XML.GetWidgetTree (wid) != null);

			Console.WriteLine ("\nList of created widgets:");
			// this is a hack, until GList is wrapped
			IntPtr l = gxml.GetWidgetPrefix ("");
			int len = g_list_length (l);
			for (int i = 0; i < len; ++i) {
				Widget w = GLib.Object.GetObject (g_list_nth_data (l, i)) as Widget;
				Console.WriteLine ("{0} {1}", 
						   w.GetType (),
						   Glade.XML.GetWidgetName (w));
			}
			
			viewer.Run ();
		}
		
	}
}
