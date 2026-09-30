// CustomCellRenderer.cs : C# implementation of an example custom cellrenderer
// from http://scentric.net/tutorial/sec-custom-cell-renderers.html
//
// Author: Todd Berman <tberman@sevenl.net>
//
// (c) 2004 Todd Berman

// adopted from: https://github.com/mono/gtk-sharp/commits/2.99.3/sample/CustomCellRenderer.cs

using System;
using Gtk;
using Gdk;
using GLib;

namespace Samples
{

	public class CustomCellRenderer : CellRenderer
	{

		private float percent;

		[GLib.Property ("percent")]
		public float Percentage {
			get { return percent; }
			set { percent = value; }
		}

		// Gtk 3 answered a single get_size covering both axes and the offsets
		// within a cell area. Gtk 4 removed that vfunc -- GtkCellRendererClass
		// has no get_size slot -- in favour of the height-for-width protocol,
		// one orientation per call, with alignment handled by the cell area.

		protected override void OnGetPreferredWidth (Widget widget, out int minimum_size, out int natural_size)
		{
			minimum_size = natural_size = (int) this.Xpad * 2 + 100;
		}

		protected override void OnGetPreferredHeight (Widget widget, out int minimum_size, out int natural_size)
		{
			minimum_size = natural_size = (int) this.Ypad * 2 + 10;
		}

		// Gtk 4 renders cells into a GtkSnapshot rather than onto a cairo_t.
		// gtk_render_background and friends became methods on the snapshot,
		// which is what keeps this a style-driven progress bar rather than a
		// hand-drawn rectangle.
		protected override void OnSnapshot (Gtk.Snapshot snapshot, Widget widget,
		                                    Rectangle background_area, Rectangle cell_area,
		                                    CellRendererState flags)
		{
			int x = (int) (cell_area.X + this.Xpad);
			int y = (int) (cell_area.Y + this.Ypad);
			int width = (int) (cell_area.Width - this.Xpad * 2);
			int height = (int) (cell_area.Height - this.Ypad * 2);

			var style = widget.StyleContext;

			style.Save ();
			style.AddClass ("trough");
			snapshot.RenderBackground (style, x, y, width, height);
			snapshot.RenderFrame (style, x, y, width, height);
			style.Restore ();

			// Gtk 3 inset the bar by the trough's CSS padding, read back through
			// StyleContext.GetPadding. Gtk 4 has no such getter -- padding is
			// applied by the theme when it renders the background -- so the bar
			// is drawn across the trough and the theme decides how it insets.
			style.Save ();
			style.AddClass ("progressbar");
			snapshot.RenderBackground (style, x, y, (int) (width * Percentage), height);
			style.Restore ();
		}
	}

}
