//
// Gnome.DrawHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	public delegate void DrawHandler(object o, DrawArgs args);

	public class DrawArgs : GLib.SignalArgs {

		public Gdk.Drawable Drawable {
			get {
				return (Gdk.Drawable) Args [0];
			}
		}
		
		public int X {
			get {
				return (int) Args [1];
			}
		}

		public int Y {
			get {
				return (int) Args [2];
			}
		}

		public int Width {
			get {
				return (int) Args [3];
			}
		}

		public int Height {
			get {
				return (int) Args [4];
			}
		}
	}
}
