//
// GtkSharp.DrawHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace GtkSharp {

	using System;

	/// <summary> DrawHandler Delegate </summary>
	/// <remarks>
	///	Delegate signature for Draw Event handlers
	/// </remarks>

	public delegate void DrawHandler(object o, DrawArgs args);

	/// <summary> DrawArgs Class </summary>
	/// <remarks>
	///	Arguments for Draw Event handlers
	/// </remarks>

	public class DrawArgs : GtkSharp.SignalArgs {

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
