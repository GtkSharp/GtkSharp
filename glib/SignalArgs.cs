// GLib.SignalArgs.cs - Signal argument class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GLib {
	using System;
	using System.Collections;

	public class SignalArgs : EventArgs {

		private object _ret;
		private object[] _args;

		public SignalArgs()
		{
			_ret = null;
			_args = null;
		}

		public SignalArgs(object retval)
		{
			_ret = retval;
			_args = null;
		}

		public SignalArgs(object retval, object[] args)
		{
			_ret = retval;
			_args = args;
		}

		public object[] Args {
			get {
				return _args;
			}
			set {
				_args = value;
			}
		}

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
