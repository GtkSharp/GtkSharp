// DelegateWrapper.cs - Delegate wrapper implementation
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GLib {

	using System;
	using System.Collections;

	/// <summary>
	///	DelegateWrapper Class
	/// </summary>
	///
	/// <remarks>
	///	Wrapper class for delegates.
	/// </remarks>

	public class DelegateWrapper {
		static ArrayList _instances = new ArrayList ();

		protected DelegateWrapper () {
		}
	}
}
