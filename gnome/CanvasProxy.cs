//
// CanvasProxy.cs - For subclassing CanvasItems
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;

namespace Gnome {

	public class CanvasProxy : Gnome.CanvasItem
	{
		public CanvasProxy (IntPtr raw)
			: base (raw)
		{
		}

		protected CanvasProxy () : base ()
		{
		}

		private Hashtable Signals = new Hashtable ();

		public event GtkSharp.UpdateHandler Update {
			add {
				if (EventList["update"] == null)
					Signals["update"] = new GtkSharp.Gnome.voidObjectAffineSVPintSignal(this, Handle, "update", value, System.Type.GetType("EventArgs"));
				EventList.AddHandler("update", value);
			}

			remove {
				EventList.RemoveHandler ("update", value);
				if (EventList ["update"] == null)
					Signals.Remove ("update");
			}
		}

		public event EventHandler Realize {
			add {
				if (EventList["realize"] == null)
					Signals["realize"] = new GtkSharp.Gtk.voidObjectSignal(this, Handle, "realize", value, System.Type.GetType("EventArgs"));
				EventList.AddHandler("realize", value);
			}
			remove {
				EventList.RemoveHandler("realize", value);
				if (EventList["realize"] == null)
					Signals.Remove("realize");
			}
		}

		public event EventHandler Unrealize {
			add {
				if (EventList["unrealize"] == null)
					Signals["unrealize"] = new GtkSharp.Gtk.voidObjectSignal(this, Handle, "unrealize", value, System.Type.GetType("EventArgs"));
				EventList.AddHandler("unrealize", value);
			}
			remove {
				EventList.RemoveHandler("unrealize", value);
				if (EventList["unrealize"] == null)
					Signals.Remove("unrealize");
			}
		}

		public event EventHandler Map {
			add {
				if (EventList["map"] == null)
					Signals["map"] = new GtkSharp.Gtk.voidObjectSignal(this, Handle, "map", value, System.Type.GetType("EventArgs"));
				EventList.AddHandler("map", value);
			}
			remove {
				EventList.RemoveHandler("map", value);
				if (EventList["map"] == null)
					Signals.Remove("map");
			}
		}

		public event EventHandler Unmap {
			add {
				if (EventList["unmap"] == null)
					Signals["unmap"] = new GtkSharp.Gtk.voidObjectSignal(this, Handle, "unmap", value, System.Type.GetType("EventArgs"));
				EventList.AddHandler("unmap", value);
			}
			remove {
				EventList.RemoveHandler("unmap", value);
				if (EventList["unmap"] == null)
					Signals.Remove("unmap");
			}
		}

		public event EventHandler Coverage {
			add {
				throw new NotImplementedException ();
			}

			remove {
				EventList.RemoveHandler ("coverage", value);
				if (EventList ["coverage"] == null)
					Signals.Remove ("coverage");
			}
		}


		public event GtkSharp.DrawHandler Draw {
			add {
				throw new NotImplementedException ();
			}

			remove {
				EventList.RemoveHandler ("draw", value);
				if (EventList ["draw"] == null)
					Signals.Remove ("draw");
			}
		}

		public event GtkSharp.RenderHandler Render {
			add {
				throw new NotImplementedException ();
			}

			remove {
				EventList.RemoveHandler ("render", value);
				if (EventList ["render"] == null)
					Signals.Remove ("render");
			}
		}

		public event GtkSharp.PointHandler Point {
			add {
				throw new NotImplementedException ();
			}

			remove {
				EventList.RemoveHandler ("point", value);
				if (EventList ["point"] == null)
					Signals.Remove ("point");
			}
		}

		public event GtkSharp.BoundsHandler Bounds {
			add {
				throw new NotImplementedException ();
			}

			remove {
				EventList.RemoveHandler ("bounds", value);
				if (EventList ["bounds"] == null)
					Signals.Remove ("bounds");
			}
		}
	}
}
