//
// GtkSharp.PointHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace GtkSharp {

	using System;

	/// <summary> PointHandler Delegate </summary>
	/// <remarks>
	///	Delegate signature for Point Event handlers
	/// </remarks>

	public delegate void PointHandler(object o, PointArgs args);

	/// <summary> PointArgs Class </summary>
	/// <remarks>
	///	Arguments for Point Event handlers
	/// </remarks>

	public class PointArgs : GtkSharp.SignalArgs {

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
