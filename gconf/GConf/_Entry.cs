// Entry_.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GConf
{
	using System;
	using System.Runtime.InteropServices;
	
	internal struct _NativeEntry
	{
		public string key;
		public IntPtr value;
	}
	
	internal class _Entry
	{
		_NativeEntry native;
		
		public _Entry (IntPtr raw)
		{
			native = (_NativeEntry) Marshal.PtrToStructure (raw, typeof (_NativeEntry));
		}

		public string Key
		{
			get { return native.key; }
		}

		internal IntPtr ValuePtr
		{
			get { return native.value; }
		}
	}
}
