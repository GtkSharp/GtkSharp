// adopted from: https://github.com/mono/xwt/blob/master/Xwt.XamMac/Xwt.Mac/ImageHandler.cs

using System;
using Gtk;
using Gdk;
using Cairo;

namespace Samples
{
	// This is a completely pointless widget, but its for testing subclassing
	// Widget.OnSnapshot.
	//
	// Gtk 3 drew by overriding OnDrawn and receiving a cairo_t. Gtk 4 renders
	// through GtkSnapshot, which builds a tree of GskRenderNodes rather than
	// issuing immediate drawing commands. Cairo is still available inside it:
	// Snapshot.AppendCairo returns a context backed by a cairo node, which is
	// what lets the pixbuf drawing below survive unchanged.
	public class GtkDrawingArea : Gtk.DrawingArea { }

	public class ImageBox : GtkDrawingArea
	{
		Pixbuf image;
		float yalign = 0.5f, xalign = 0.5f;

		public ImageBox(Pixbuf img) : this()
		{
			Image = img;
		}

		public ImageBox()
		{
			// HasWindow and AppPaintable are gone: Gtk 4 widgets do not own a
			// GdkWindow, and every widget draws its own content.
		}

		public Pixbuf Image {
			get { return image; }
			set {
				image = value;
				SetSizeRequest((int) image.Width, (int) image.Height);
				QueueDraw();
			}
		}

		public float Yalign {
			get { return yalign; }
			set {
				yalign = value;
				QueueDraw();
			}
		}

		public float Xalign {
			get { return xalign; }
			set {
				xalign = value;
				QueueDraw();
			}
		}

		void DrawPixbuf(Cairo.Context ctx, Gdk.Pixbuf img, double x, double y, int width, int height)
		{
			ctx.Save();
			ctx.Translate(x, y);

			ctx.Scale(width / (double) img.Width, height / (double) img.Height);
			Gdk.CairoHelper.SetSourcePixbuf(ctx, img, 0, 0);

#pragma warning disable 618
			using (var p = ctx.Source) {
				if (p is SurfacePattern pattern) {
					if (width > img.Width || height > img.Height) {
						// Fixes blur issue when rendering on an image surface
						pattern.Filter = Cairo.Filter.Fast;
					} else
						pattern.Filter = Cairo.Filter.Good;
				}
			}
#pragma warning restore 618

			ctx.Paint();

			ctx.Restore();
		}

		protected override void OnSnapshot(Gtk.Snapshot snapshot)
		{
			base.OnSnapshot(snapshot);

			if (image == null)
				return;

			// Gtk 4 reports the widget's own size directly, in widget-local
			// coordinates. The Gtk 3 version had to guard against a bogus
			// 1x1-at-(-1,-1) allocation arriving mid-reallocation; snapshot is
			// only called with a valid size, so the guard is just a size check.
			int width = Width;
			int height = Height;
			if (width <= 0 || height <= 0)
				return;

			var bounds = Graphene.Rect.Alloc();
			bounds.Init(0, 0, width, height);

			using (var cr = snapshot.AppendCairo(bounds)) {
				var x = (int) ((width - (float) image.Width) * xalign);
				var y = (int) ((height - (float) image.Height) * yalign);
				if (x < 0) x = 0;
				if (y < 0) y = 0;
				DrawPixbuf(cr, image, x, y, width, height);
			}
		}
	}
}
