//
// GtkSharp.CanvasUpdateHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace GtkSharp {

	using System;

	/// <summary> CanvasUpdateHandler Delegate </summary>
	/// <remarks>
	///	Delegate signature for CanvasUpdate Event handlers
	/// </remarks>

	public delegate void UpdateHandler (object o, UpdateArgs args);

	/// <summary> CanvasUpdateArgs Class </summary>
	/// <remarks>
	///	Arguments for CanvasUpdate Event handlers
	/// </remarks>

	public class UpdateArgs : GtkSharp.SignalArgs {

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
