//
// Gnome.RenderHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	public delegate void RenderHandler(object o, RenderArgs args);

	public class RenderArgs : GLib.SignalArgs {

		public Gnome.CanvasBuf Buf {
			get {
				return (Gnome.CanvasBuf) Args [0];
			}
		}
	}
}
