//
// GtkSharp.BoundsHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	public delegate void BoundsHandler(object o, BoundsArgs args);

	public class BoundsArgs : GLib.SignalArgs {

		public double [] X1 {
			get {
				return (double []) Args [0];
			}
		}
		
		public double [] Y1 {
			get {
				return (double []) Args [1];
			}
		}

		public double [] X2 {
			get {
				return (double []) Args [2];
			}
		}
		
		public double [] Y2 {
			get {
				return (double []) Args [3];
			}
		}
	}
}
