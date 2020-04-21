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

		protected override void OnGetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			base.OnGetSize (widget, ref cell_area, out x_offset, out y_offset, out width, out height);

			int calc_width = (int) this.Xpad * 2 + 100;
			int calc_height = (int) this.Ypad * 2 + 10;

			width = calc_width;
			height = calc_height;

			x_offset = 0;
			y_offset = 0;
			if (!cell_area.Equals (Rectangle.Zero)) {
				x_offset = (int) (this.Xalign * (cell_area.Width - calc_width));
				x_offset = Math.Max (x_offset, 0);

				y_offset = (int) (this.Yalign * (cell_area.Height - calc_height));
				y_offset = Math.Max (y_offset, 0);
			}
		}

		protected override void OnRender (Cairo.Context cr, Widget widget, Rectangle background_area, Rectangle cell_area, CellRendererState flags)
		{
			int x = (int) (cell_area.X + this.Xpad);
			int y = (int) (cell_area.Y + this.Ypad);
			int width = (int) (cell_area.Width - this.Xpad * 2);
			int height = (int) (cell_area.Height - this.Ypad * 2);

			widget.StyleContext.Save ();
			widget.StyleContext.AddClass ("trough");
			widget.StyleContext.RenderBackground (cr, x, y, width, height);
			widget.StyleContext.RenderFrame (cr, x, y, width, height);

			Border padding = widget.StyleContext.GetPadding (StateFlags.Normal);
			x += padding.Left;
			y += padding.Top;
			width -= padding.Left + padding.Right;
			height -= padding.Top + padding.Bottom;

			widget.StyleContext.Restore ();

			widget.StyleContext.Save ();
			widget.StyleContext.AddClass ("progressbar");
			widget.StyleContext.RenderActivity (cr, x, y, (int) (width * Percentage), height);
			widget.StyleContext.Restore ();
		}
	}

}