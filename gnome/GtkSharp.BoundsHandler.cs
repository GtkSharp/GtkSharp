//
// GtkSharp.BoundsHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace GtkSharp {

	using System;

	/// <summary> BoundsHandler Delegate </summary>
	/// <remarks>
	///	Delegate signature for Bounds Event handlers
	/// </remarks>

	public delegate void BoundsHandler(object o, BoundsArgs args);

	/// <summary> BoundsArgs Class </summary>
	/// <remarks>
	///	Arguments for Bounds Event handlers
	/// </remarks>

	public class BoundsArgs : GtkSharp.SignalArgs {

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
