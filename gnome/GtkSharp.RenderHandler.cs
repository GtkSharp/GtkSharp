//
// Gnome.RenderHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace Gnome {

	using System;

	/// <summary> RenderHandler Delegate </summary>
	/// <remarks>
	///	Delegate signature for Render Event handlers
	/// </remarks>

	public delegate void RenderHandler(object o, RenderArgs args);

	/// <summary> RenderArgs Class </summary>
	/// <remarks>
	///	Arguments for Render Event handlers
	/// </remarks>

	public class RenderArgs : GtkSharp.SignalArgs {

		public Gnome.CanvasBuf Buf {
			get {
				return (Gnome.CanvasBuf) Args [0];
			}
		}
	}
}
