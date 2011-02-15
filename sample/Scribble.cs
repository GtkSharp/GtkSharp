// Copyright (c) 2011 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using Gtk;
using Gdk;
using System;

namespace GtkSamples {

	public class ScribbleArea : DrawingArea {

		public static int Main (string[] args)
		{
			Application.Init ();
			Gtk.Window win = new Gtk.Window ("Scribble");
			win.DeleteEvent += delegate { Application.Quit (); };
			win.BorderWidth = 8;
			Frame frm = new Frame (null);
			frm.ShadowType = ShadowType.In;
			frm.Add (new ScribbleArea ());
			win.Add (frm);
			win.ShowAll ();
			Application.Run ();
			return 0;
		}

		Cairo.Surface surface;

		public ScribbleArea ()
		{
			SetSizeRequest (200, 200);
			Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask | EventMask.PointerMotionHintMask;
		}

		void ClearSurface ()
		{
			using (Cairo.Context ctx = new Cairo.Context (surface)) {
				ctx.SetSourceRGB (1, 1, 1);
				ctx.Paint ();
			}
		}

		void DrawBrush (double x, double y)
		{
			using (Cairo.Context ctx = new Cairo.Context (surface)) {
				ctx.Rectangle ((int) x - 3, (int) y - 3, 6, 6);
				ctx.Fill ();
			}

			QueueDrawArea ((int) x - 3, (int) y - 3, 6, 6);
		}
	
		protected override bool OnButtonPressEvent (EventButton ev)
		{
			if (surface == null)
				return false;

			switch (ev.Button) {
			case 1:
				DrawBrush (ev.X, ev.Y);
				break;
			case 3:
				ClearSurface ();
				QueueDraw ();
				break;
			default:
				break;
			}
			return true;
		}
		
		protected override bool OnConfigureEvent (EventConfigure ev)
		{
			surface = ev.Window.CreateSimilarSurface (Cairo.Content.Color, AllocatedWidth, AllocatedHeight);
			ClearSurface ();
			return true;
		}

		protected override bool OnDrawn (Cairo.Context ctx)
		{
			ctx.SetSourceSurface (surface, 0, 0);
			ctx.Paint ();
			return false;
		}
		
		protected override bool OnMotionNotifyEvent (EventMotion ev)
		{
			if (surface == null)
				return false;

			int x, y;
			Gdk.ModifierType state;
			ev.Window.GetPointer (out x, out y, out state);
			if ((state & Gdk.ModifierType.Button1Mask) != 0)
				DrawBrush (x, y);

			return true;
		}
	}
}

