// GtkSharp.SignalArgs.cs - Signal argument class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp {
	using System;
	using System.Collections;

	/// <summary>
	///	SignalArgs Class
	/// </summary>
	///
	/// <remarks>
	///	Arguments and return value for signals.
	/// </remarks>

	public class SignalArgs : EventArgs {

		private object _ret;
		private object[] _args;

		/// <summary>
		///	SignalArgs Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SignalArgs object with no return value and 
		///	no arguments.
		/// </remarks>

		public SignalArgs()
		{
			_ret = null;
			_args = null;
		}

		/// <summary>
		///	SignalArgs Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SignalArgs object with a return value and 
		///	no arguments.
		/// </remarks>

		public SignalArgs(object retval)
		{
			_ret = retval;
			_args = null;
		}

		/// <summary>
		///	SignalArgs Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SignalArgs object with a return value and 
		///	a list of arguments.
		/// </remarks>

		public SignalArgs(object retval, object[] args)
		{
			_ret = retval;
			_args = args;
		}

		/// <summary>
		///	Args Property
		/// </summary>
		///
		/// <remarks>
		///	A list of arguments.
		/// </remarks>

		public object[] Args {
			get {
				return _args;
			}
			set {
				_args = value;
			}
		}

		/// <summary>
		///	RetVal Property
		/// </summary>
		///
		/// <remarks>
		///	The return value.
		/// </remarks>

		public object RetVal {
			get {
				return _ret;
			}
			set {
				_ret = value;
			}
		}
	}
}
