// GLibSharp.Marshaller.cs : Marshalling utils 
//
// Author: Rachel Hestilow <rachel@nullenvoid.com>
//         Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2002, 2003 Rachel Hestilow
// Copyright (c) 2004 Novell, Inc.
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


namespace GLib {
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	
	public class Marshaller {

		private Marshaller () {}
		
		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern void g_free (IntPtr mem);

		public static void Free (IntPtr ptr)
		{
			g_free (ptr);
		}

		public static void Free (IntPtr[] ptrs)
		{
			if (ptrs == null)
				return;

			for (int i = 0; i < ptrs.Length; i++)
				g_free (ptrs [i]);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_filename_to_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		[DllImport (Global.GLibNativeDll)]
		static extern IntPtr g_filename_to_utf8_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		public static string FilenamePtrToString (IntPtr ptr) 
		{
			if (ptr == IntPtr.Zero) return null;
			
			IntPtr dummy, error;
			IntPtr utf8;

			if (Global.IsWindowsPlatform)
				utf8 = g_filename_to_utf8_utf8 (ptr, -1, IntPtr.Zero, out dummy, out error);
			else
				utf8 = g_filename_to_utf8 (ptr, -1, IntPtr.Zero, out dummy, out error);

			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			return Utf8PtrToString (utf8);
		}

		public static string FilenamePtrToStringGFree (IntPtr ptr) 
		{
			string ret = FilenamePtrToString (ptr);
			g_free (ptr);
			return ret;
		}

		static unsafe ulong strlen (IntPtr s)
		{
			ulong cnt = 0;
			byte *b = (byte *)s;
			while (*b != 0) {
				b++;
				cnt++;
			}
			return cnt;
		}

		public static string Utf8PtrToString (IntPtr ptr) 
		{
			if (ptr == IntPtr.Zero)
				return null;

			int len = (int) (uint) strlen (ptr);
			byte[] bytes = new byte [len];
			Marshal.Copy (ptr, bytes, 0, len);
			return System.Text.Encoding.UTF8.GetString (bytes);
		}

		public static string[] Utf8PtrToString (IntPtr[] ptrs) {
			// The last pointer is a null terminator.
			string[] ret = new string[ptrs.Length - 1];
			for (int i = 0; i < ret.Length; i++)
				ret[i] = Utf8PtrToString (ptrs[i]);
			return ret;
		}

		public static string PtrToStringGFree (IntPtr ptr) 
		{
			string ret = Utf8PtrToString (ptr);
			g_free (ptr);
			return ret;
		}

		public static string[] PtrToStringGFree (IntPtr[] ptrs) {
			// The last pointer is a null terminator.
			string[] ret = new string[ptrs.Length - 1];
			for (int i = 0; i < ret.Length; i++) {
				ret[i] = Utf8PtrToString (ptrs[i]);
				g_free (ptrs[i]);
			}
			return ret;
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_filename_from_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		[DllImport (Global.GLibNativeDll)]
		static extern IntPtr g_filename_from_utf8_utf8 (IntPtr mem, int len, IntPtr read, out IntPtr written, out IntPtr error);

		public static IntPtr StringToFilenamePtr (string str) 
		{
			if (str == null)
				return IntPtr.Zero;

			IntPtr dummy, error;
			IntPtr utf8 = StringToPtrGStrdup (str);

			IntPtr result;

			if (Global.IsWindowsPlatform)
				result = g_filename_from_utf8_utf8 (utf8, -1, IntPtr.Zero, out dummy, out error);
			else
				result = g_filename_from_utf8 (utf8, -1, IntPtr.Zero, out dummy, out error);

			g_free (utf8);
			if (error != IntPtr.Zero)
				throw new GException (error);

			return result;
		}

		public static IntPtr StringToPtrGStrdup (string str) {
			if (str == null)
				return IntPtr.Zero;
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (str);
			IntPtr result = g_malloc (new UIntPtr ((ulong)bytes.Length + 1));
			Marshal.Copy (bytes, 0, result, bytes.Length);
			Marshal.WriteByte (result, bytes.Length, 0);
			return result;
		}

		public static string StringFormat (string format, params object[] args) {
			string ret = String.Format (format, args);
			if (ret.IndexOf ('%') == -1)
				return ret;
			else
				return ret.Replace ("%", "%%");
		}

		public static IntPtr StringArrayToStrvPtr (string[] strs)
		{
			IntPtr[] ptrs = StringArrayToNullTermPointer (strs);
			IntPtr ret = g_malloc (new UIntPtr ((ulong) (ptrs.Length * IntPtr.Size)));
			Marshal.Copy (ptrs, 0, ret, ptrs.Length);
			return ret;
		}

		public static IntPtr StringArrayToNullTermStrvPointer (string[] strs)
		{
			return StringArrayToStrvPtr (strs);
		}

		public static IntPtr[] StringArrayToNullTermPointer (string[] strs)
		{
			if (strs == null)
				return null;
			IntPtr[] result = new IntPtr [strs.Length + 1];
			for (int i = 0; i < strs.Length; i++)
				result [i] = StringToPtrGStrdup (strs [i]);
			result [strs.Length] = IntPtr.Zero;
			return result;
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern void g_strfreev (IntPtr mem);

		public static void StrFreeV (IntPtr null_term_array)
		{
			g_strfreev (null_term_array);
		}

		public static string[] NullTermPtrToStringArray (IntPtr null_term_array, bool owned)
		{
			if (null_term_array == IntPtr.Zero)
				return new string [0];

			int count = 0;
			var result = new List<string> ();
			IntPtr s = Marshal.ReadIntPtr (null_term_array, count++ * IntPtr.Size);
			while (s != IntPtr.Zero) {
				result.Add (Utf8PtrToString (s));
				s = Marshal.ReadIntPtr (null_term_array, count++ * IntPtr.Size);
			}

			if (owned)
				g_strfreev (null_term_array);

			return result.ToArray ();
		}

		public static string[] PtrToStringArrayGFree (IntPtr string_array)
		{
			if (string_array == IntPtr.Zero)
				return new string [0];
	
			int count = 0;
			while (Marshal.ReadIntPtr (string_array, count*IntPtr.Size) != IntPtr.Zero)
				++count;
	
			string[] members = new string[count];
			for (int i = 0; i < count; ++i) {
				IntPtr s = Marshal.ReadIntPtr (string_array, i * IntPtr.Size);
				members[i] = PtrToStringGFree (s);
			}
			Free (string_array);
			return members;
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_malloc(UIntPtr size);

		public static IntPtr Malloc (ulong size)
		{
			return g_malloc (new UIntPtr (size));
		}

		static System.DateTime local_epoch = new System.DateTime (1970, 1, 1, 0, 0, 0);
		static int utc_offset = (int) (System.TimeZone.CurrentTimeZone.GetUtcOffset (System.DateTime.Now)).TotalSeconds;

		public static IntPtr DateTimeTotime_t (System.DateTime time)
		{
			return new IntPtr (((long)time.Subtract (local_epoch).TotalSeconds) - utc_offset);
		}

		public static System.DateTime time_tToDateTime (IntPtr time_t)
		{
			return local_epoch.AddSeconds (time_t.ToInt64 () + utc_offset);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_malloc0 (UIntPtr size);

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern int g_unichar_to_utf8 (uint c, IntPtr buf);

		public static char GUnicharToChar (uint ucs4_char)
		{ 
			if (ucs4_char == 0)
				return (char) 0;

			string ret = GUnicharToString (ucs4_char);
			if (ret.Length != 1)
				throw new ArgumentOutOfRangeException ("ucs4char is not representable by a char.");

			return ret [0];
		}

		public static string GUnicharToString (uint ucs4_char)
		{ 
			if (ucs4_char == 0)
				return String.Empty;

			IntPtr buf = g_malloc0 (new UIntPtr (7));
			g_unichar_to_utf8 (ucs4_char, buf);
			return PtrToStringGFree (buf);
		}

		[DllImport (Global.GLibNativeDll, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_utf16_to_ucs4 (ref ushort c, IntPtr len, IntPtr d1, IntPtr d2, IntPtr d3);

		public static uint CharToGUnichar (char c)
		{
			ushort val = (ushort) c;
			IntPtr ucs4_str = g_utf16_to_ucs4 (ref val, new IntPtr (1), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			uint result = (uint) Marshal.ReadInt32 (ucs4_str);
			g_free (ucs4_str);
			return result;
		}

		public static IntPtr StructureToPtrAlloc (object o)
		{
			IntPtr result = Marshal.AllocHGlobal (Marshal.SizeOf (o));
			Marshal.StructureToPtr (o, result, false);
			return result;
		}

		public static IntPtr ArrayToArrayPtr (byte[] array)
		{
			IntPtr ret = Malloc ((ulong) array.Length);
			Marshal.Copy (array, 0, ret, array.Length);
			return ret;
		}

		public static Array ArrayPtrToArray (IntPtr array_ptr, Type element_type, int length, bool owned)
		{
			Array result = null;
			if (element_type == typeof (byte)) {
				byte[] ret = new byte [length];
				Marshal.Copy (array_ptr, ret, 0, length);
				result = ret;
			} else {
				throw new InvalidOperationException ("Marshaling of " + element_type + " arrays is not supported");
			}
			if (owned)
				Free (array_ptr);
			return result;
		}

		public static Array ListPtrToArray (IntPtr list_ptr, Type list_type, bool owned, bool elements_owned, Type elem_type)
		{
			Type array_type = elem_type == typeof (ListBase.FilenameString) ? typeof (string) : elem_type;
			ListBase list;
			if (list_type == typeof(GLib.List))
				list = new GLib.List (list_ptr, elem_type, owned, elements_owned);
			else
				list = new GLib.SList (list_ptr, elem_type, owned, elements_owned);

			using (list)
				return ListToArray (list, array_type);
		}

		public static Array PtrArrayToArray (IntPtr list_ptr, bool owned, bool elements_owned, Type elem_type)
		{
			GLib.PtrArray array = new GLib.PtrArray (list_ptr, elem_type, owned, elements_owned);
			Array ret = Array.CreateInstance (elem_type, array.Count);
			array.CopyTo (ret, 0);
			array.Dispose ();
			return ret;
		}

		public static Array ListToArray (ListBase list, System.Type type)
		{
			Array result = Array.CreateInstance (type, list.Count);
			if (list.Count > 0)
				list.CopyTo (result, 0);

			if (type.IsSubclassOf (typeof (GLib.Opaque)))
				list.elements_owned = false;

			return result;
		}

		public static T[] StructArrayFromNullTerminatedIntPtr<T> (IntPtr array)
		{
			var res = new List<T> ();
			IntPtr current = array;
			T currentStruct = default(T);

			while (current != IntPtr.Zero) {
				Marshal.PtrToStructure (current, currentStruct);
				res.Add (currentStruct);
				current = (IntPtr) ((long)current + Marshal.SizeOf (typeof (T)));
			}

			return res.ToArray ();
		}

		public static IntPtr StructArrayToNullTerminatedStructArrayIntPtr<T> (T[] InputArray)
		{
			int intPtrSize = Marshal.SizeOf (typeof (IntPtr));
			IntPtr mem = Marshal.AllocHGlobal ((InputArray.Length + 1) * intPtrSize);

			for (int i = 0; i < InputArray.Length; i++) {
				IntPtr structPtr = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (T)));
				Marshal.StructureToPtr (InputArray[i], structPtr, false);
				// jump to next pointer
				Marshal.WriteIntPtr (mem, structPtr);
				mem = (IntPtr) ((long)mem + intPtrSize);
			}
			// null terminate
			Marshal.WriteIntPtr (mem, IntPtr.Zero);

			return mem;
		}
	}
}

