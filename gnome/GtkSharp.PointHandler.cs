//
// Gnome.PointHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	public delegate void PointHandler(object o, PointArgs args);

	public class PointArgs : GLib.SignalArgs {

		public double X {
			get {
				return (double) Args [0];
			}
		}
		
		public double Y {
			get {
				return (double) Args [1];
			}
		}

		public int CX {
			get {
				return (int) Args [2];
			}
		}

		public int CY {
			get {
				return (int) Args [3];
			}
		}

		public Gnome.CanvasItem [] ActualItem {
			get {
				return (Gnome.CanvasItem []) Args [4];
			}
		}
	}
}
