//
// TestToolTip.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;

namespace WidgetViewer {
	public class TestToolTip
	{
		static Window window = null;
		static Tooltips tooltips = null;
		
		public Gtk.Window Create ()
		{
			window = new Window ("Tooltips");
			window.Width = 200;
			tooltips = new Tooltips ();

			window.Data ("tooltips", tooltip);
			
			return window;
		}

		static void Window_Delete (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}

