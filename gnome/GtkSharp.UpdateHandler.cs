//
// Gnome.CanvasUpdateHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	public delegate void UpdateHandler (object o, UpdateArgs args);

	public class UpdateArgs : GLib.SignalArgs {

		public double [] Affine {
			get {
				return (double []) Args [0];
			}
		}
		
		public Art.SVP ClipPath {
			get {
				return (Art.SVP) Args [1];
			}
		}

		public int Flags {
			get {
				return (int) Args [2];
			}
		}
	}
}
