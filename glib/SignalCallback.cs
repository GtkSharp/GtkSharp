// GtkSharp.SignalCallback.cs - Signal callback base class implementation
//
// Authors: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp {
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLib;

	/// <summary>
	///	SignalCallback Class
	/// </summary>
	///
	/// <remarks>
	///	Base Class for GSignal to C# event marshalling.
	/// </remarks>

	public abstract class SignalCallback : IDisposable {

		// A counter used to produce unique keys for instances.
		protected static int _NextKey = 0;

		// Hashtable containing refs to all current instances.
		protected static Hashtable _Instances = new Hashtable ();

		// protected instance members
		protected GLib.Object _obj;
		protected Delegate _handler;
		protected int _key;
		protected System.Type _argstype;

		/// <summary>
		///	SignalCallback Constructor
		/// </summary>
		///
		/// <remarks>
		///	Initializes instance data.
		/// </remarks>

		public SignalCallback (GLib.Object obj, Delegate eh, System.Type argstype)
		{
			_key = _NextKey++;
			_obj = obj;
			_handler = eh;
			_argstype = argstype;
			_Instances [_key] = this;
		}

		public void AddDelegate (Delegate d)
		{
			_handler = Delegate.Combine (_handler, d);
		}

		public void RemoveDelegate (Delegate d)
		{
			_handler = Delegate.Remove (_handler, d);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				_obj = null;
				_handler = null;
				_argstype = null;
			}
		}

		~SignalCallback ()
		{
			Dispose (false);
		}
	}
}

