// GLib.TypeFundamentals.cs : Standard Types enumeration 
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GLib {

	/// <summary>
	///	TypeFundamentals enumeration
	/// </summary>
	///
	/// <remarks>
	///	The built-in types available in GLib.
	/// </remarks>

	public enum TypeFundamentals {
		TypeInvalid	= 0 << 2,
		TypeNone	= 1 << 2,
		TypeInterface	= 2 << 2,
		TypeChar	= 3 << 2,
		TypeUChar	= 4 << 2,
		TypeBoolean	= 5 << 2,
		TypeInt		= 6 << 2,
		TypeUInt	= 7 << 2,
		TypeLong	= 8 << 2,
		TypeULong	= 9 << 2,
		TypeInt64	= 10 << 2,
		TypeUInt64	= 11 << 2,
		TypeEnum	= 12 << 2,
		TypeFlags	= 13 << 2,
		TypeFloat	= 14 << 2,
		TypeDouble	= 15 << 2,
		TypeString	= 16 << 2,
		TypePointer	= 17 << 2,
		TypeBoxed	= 18 << 2,
		TypeParam	= 19 << 2,
		TypeObject	= 20 << 2,
	}
}
