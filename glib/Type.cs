// GLib.Type.cs - GLib Type class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2003 Mike Kestner

namespace GLib {

	using System;

	/// <summary> 
	///	Type Class
	/// </summary>
	///
	/// <remarks>
	///	An arbitrary data type similar to a CORBA Any which is used
	///	to get and set properties on Objects.
	/// </remarks>

	public class Type {

		uint val;

		/// <summary>
		///	Type Constructor
		/// </summary>
		///
		/// <remarks>
		///	Constructs a new Type from a native GType value.
		/// </remarks>

		public Type (uint val) {
			this.val = val;
		}

		/// <summary>
		///	Value Property
		/// </summary>
		/// 
		/// <remarks>
		///	Gets the native value of a Type object.
		/// </remarks>

		public uint Value {
			get {
				return val;
			}
		}
	}
}
