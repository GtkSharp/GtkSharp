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

		public event UpdateHandler Update {
			add {
				if (AfterHandlers["update"] == null)
					AfterSignals["update"] = new GtkSharp.voidObjectAffineSVPintSignal(this, Handle, "update", value, System.Type.GetType("EventArgs"));
				AfterHandlers.AddHandler("update", value);
			}

			remove {
				AfterHandlers.RemoveHandler ("update", value);
				if (AfterHandlers ["update"] == null)
					AfterSignals.Remove ("update");
			}
		}

		public event EventHandler Realize {
			add {
				if (AfterHandlers["realize"] == null)
					AfterSignals["realize"] = new GnomeSharp.voidObjectSignal(this, Handle, "realize", value, System.Type.GetType("EventArgs"), 1);
				AfterHandlers.AddHandler("realize", value);
			}
			remove {
				AfterHandlers.RemoveHandler("realize", value);
				if (AfterHandlers["realize"] == null)
					AfterSignals.Remove("realize");
			}
		}

		public event EventHandler Unrealize {
			add {
				if (AfterHandlers["unrealize"] == null)
					AfterSignals["unrealize"] = new GnomeSharp.voidObjectSignal(this, Handle, "unrealize", value, System.Type.GetType("EventArgs"), 1);
				AfterHandlers.AddHandler("unrealize", value);
			}
			remove {
				AfterHandlers.RemoveHandler("unrealize", value);
				if (AfterHandlers["unrealize"] == null)
					AfterSignals.Remove("unrealize");
			}
		}

		public event EventHandler Map {
			add {
				if (AfterHandlers["map"] == null)
					AfterSignals["map"] = new GnomeSharp.voidObjectSignal(this, Handle, "map", value, System.Type.GetType("EventArgs"), 1);
				AfterHandlers.AddHandler("map", value);
			}
			remove {
				AfterHandlers.RemoveHandler("map", value);
				if (AfterHandlers["map"] == null)
					AfterSignals.Remove("map");
			}
		}

		public event EventHandler Unmap {
			add {
				if (AfterHandlers["unmap"] == null)
					AfterSignals["unmap"] = new GnomeSharp.voidObjectSignal(this, Handle, "unmap", value, System.Type.GetType("EventArgs"), 1);
				AfterHandlers.AddHandler("unmap", value);
			}
			remove {
				AfterHandlers.RemoveHandler("unmap", value);
				if (AfterHandlers["unmap"] == null)
					AfterSignals.Remove("unmap");
			}
		}

		public event EventHandler Coverage {
			add {
				throw new NotImplementedException ();
			}

			remove {
				AfterHandlers.RemoveHandler ("coverage", value);
				if (AfterHandlers ["coverage"] == null)
					AfterSignals.Remove ("coverage");
			}
		}


		public event DrawHandler Draw {
			add {
				throw new NotImplementedException ();
			}

			remove {
				AfterHandlers.RemoveHandler ("draw", value);
				if (AfterHandlers ["draw"] == null)
					AfterSignals.Remove ("draw");
			}
		}

		public event RenderHandler Render {
			add {
				throw new NotImplementedException ();
			}

			remove {
				AfterHandlers.RemoveHandler ("render", value);
				if (AfterHandlers ["render"] == null)
					AfterSignals.Remove ("render");
			}
		}

		public event PointHandler Point {
			add {
				throw new NotImplementedException ();
			}

			remove {
				AfterHandlers.RemoveHandler ("point", value);
				if (AfterHandlers ["point"] == null)
					AfterSignals.Remove ("point");
			}
		}

		public event BoundsHandler Bounds {
			add {
				throw new NotImplementedException ();
			}

			remove {
				AfterHandlers.RemoveHandler ("bounds", value);
				if (AfterHandlers ["bounds"] == null)
					AfterSignals.Remove ("bounds");
			}
		}
	}
}
