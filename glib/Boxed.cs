// GtkSharp.Boxed.cs - Base class for deriving marshallable structures.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp {

	using System;
	using System.Runtime.InteropServices;

	/// <summary> 
	///	Boxed Class
	/// </summary>
	///
	/// <remarks>
	///	An abstract base class to derive structures and marshal them.
	/// </remarks>

	public abstract class Boxed {

		IntPtr	_raw;

		// Destructor is required since we are allocating unmanaged
		// heap resources.

		~Boxed ()
		{
			Marshal.FreeHGlobal (_raw);
		}

		/// <summary>
		///	Raw Property
		/// </summary>
		/// 
		/// <remarks>
		///	Gets a marshallable IntPtr.
		/// </remarks>

		public IntPtr Raw {
			get {
				if (_raw == IntPtr.Zero) {
					// FIXME: Ugly hack.
					_raw = Marshal.AllocHGlobal (128);
					Marshal.StructureToPtr (this, _raw, true);
				}
				return _raw;
			}
		}

		/// <summary>
		///	GetBoxed Shared Method
		/// </summary>
		/// 
		/// <remarks>
		///	Gets a managed class representing a raw ref.
		/// </remarks>

		public static Boxed GetBoxed (IntPtr raw)
		{
			// FIXME: Use the type manager to box the raw ref.
			return null;
		}

	}
}
