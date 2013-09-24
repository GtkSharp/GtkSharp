// Copyright (c) 2008-2011 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Runtime.InteropServices;

namespace GLib {

	public enum KeyFileError {
		UnknownEncoding,
		Parse,
		NotFound,
		KeyNotFound,
		GroupNotFound,
		InvalidValue,
	}

	[Flags]
	public enum KeyFileFlags {
		None,
		KeepComments = 1 << 0,
		KeepTranslations = 1 << 1,
	}

	public class KeyFile {

		const string dll = "libglib-2.0-0.dll";

		IntPtr handle;
		bool owned;

		class FinalizerInfo {
			IntPtr handle;

			public FinalizerInfo (IntPtr handle)
			{
				this.handle = handle;
			}

			public bool Handler ()
			{
				g_key_file_free (handle);
				return false;
			}
		}

		~KeyFile ()
		{
			if (!owned)
				return;
			FinalizerInfo info = new FinalizerInfo (Handle);
			Timeout.Add (50, new TimeoutHandler (info.Handler));
		}

		public KeyFile (IntPtr handle) : this (handle, false) {}

		public KeyFile (IntPtr handle, bool owned)
		{
			this.handle = handle;
			this.owned = owned;
		}

		public KeyFile () : this (g_key_file_new (), true) {}

		public KeyFile (string file) : this (file, KeyFileFlags.KeepComments) {}

		public KeyFile (string file, KeyFileFlags flags) : this ()
		{
			LoadFromFile (file, flags);
		}

		public string[] Groups {
			get { return Marshaller.NullTermPtrToStringArray (g_key_file_get_groups (Handle, IntPtr.Zero), true); }
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public string StartGroup { 
			get { return Marshaller.PtrToStringGFree (g_key_file_get_start_group (Handle)); }
		}

		public bool GetBoolean (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			bool ret = g_key_file_get_boolean (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool[] GetBooleanList (string group_name, string key)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			UIntPtr native_length;
			IntPtr error;
			IntPtr raw_ret = g_key_file_get_boolean_list (Handle, native_group_name, native_key, out native_length, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			int length = (int) (ulong) native_length;
			int[] ints = new int [length];
			Marshal.Copy (raw_ret, ints, 0, length);
			Marshaller.Free (raw_ret);
			bool[] ret = new bool [length];
			for (int i = 0; i < length; i++)
				ret [i] = ints [i] != 0;
			return ret;
		}

		public string GetComment (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			string ret = Marshaller.PtrToStringGFree (g_key_file_get_comment (Handle, native_group_name, native_key, out error));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public double GetDouble (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			double ret = g_key_file_get_double (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public double[] GetDoubleList (string group_name, string key)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			UIntPtr native_length;
			IntPtr error;
			IntPtr raw_ret = g_key_file_get_double_list(Handle, native_group_name, native_key, out native_length, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			int length = (int) (ulong)native_length;
			double[] ret = new double [length];
			Marshal.Copy (raw_ret, ret, 0, length);
			Marshaller.Free (raw_ret);
			return ret;
		}
	
		public int GetInteger (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			int ret = g_key_file_get_integer (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public int[] GetIntegerList (string group_name, string key)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			UIntPtr native_length;
			IntPtr error;
			IntPtr raw_ret = g_key_file_get_integer_list(Handle, native_group_name, native_key, out native_length, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			int length = (int) (ulong) native_length;
			int[] ret = new int [length];
			Marshal.Copy (raw_ret, ret, 0, length);
			Marshaller.Free (raw_ret);
			return ret;
		}

		public string[] GetKeys (string group_name)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			string[] ret = Marshaller.NullTermPtrToStringArray (g_key_file_get_keys (Handle, native_group_name, IntPtr.Zero, out error), true);
			Marshaller.Free (native_group_name);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public string GetLocaleString (string group_name, string key, string locale)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_locale = Marshaller.StringToPtrGStrdup (locale);
			string ret = Marshaller.PtrToStringGFree (g_key_file_get_locale_string (Handle, native_group_name, native_key, native_locale, out error));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_locale);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public string[] GetLocaleStringList (string group_name, string key, string locale)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_locale = Marshaller.StringToPtrGStrdup (locale);
			string[] ret = Marshaller.NullTermPtrToStringArray (g_key_file_get_locale_string_list (Handle, native_group_name, native_key, native_locale, IntPtr.Zero, out error), true);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_locale);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public string GetString (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			string ret = Marshaller.PtrToStringGFree (g_key_file_get_string (Handle, native_group_name, native_key, out error));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public string[] GetStringList (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			string[] ret = Marshaller.NullTermPtrToStringArray (g_key_file_get_string_list (Handle, native_group_name, native_key, IntPtr.Zero, out error), true);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public string GetValue (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			string ret = Marshaller.PtrToStringGFree (g_key_file_get_value (Handle, native_group_name, native_key, out error));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool HasGroup (string group_name)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			bool ret = g_key_file_has_group(Handle, native_group_name);
			Marshaller.Free (native_group_name);
			return ret;
		}

		public bool HasKey (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			bool ret = g_key_file_has_key (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool LoadFromData (byte[] data, KeyFileFlags flags)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			IntPtr error;
			bool ret = g_key_file_load_from_data (Handle, data, new UIntPtr ((ulong) data.Length), (int) flags, out error);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool LoadFromDataDirs (string file, out string full_path, KeyFileFlags flags)
		{
			IntPtr error;
			IntPtr native_full_path;
			IntPtr native_file = Marshaller.StringToPtrGStrdup (file);
			bool ret = g_key_file_load_from_data_dirs(Handle, native_file, out native_full_path, (int) flags, out error);
			Marshaller.Free (native_file);
			if (error != IntPtr.Zero) throw new GException (error);
			full_path = Marshaller.PtrToStringGFree (native_full_path);
			return ret;
		}

		public bool LoadFromDirs (string file, string[] search_dirs, out string full_path, KeyFileFlags flags)
		{
			IntPtr error;
			IntPtr native_full_path;
			IntPtr native_file = Marshaller.StringToPtrGStrdup (file);
			IntPtr native_search_dirs = Marshaller.StringArrayToStrvPtr (search_dirs);
			bool ret = g_key_file_load_from_dirs (Handle, native_file, native_search_dirs, out native_full_path, (int) flags, out error);
			Marshaller.Free (native_file);
			Marshaller.StrFreeV (native_search_dirs);
			if (error != IntPtr.Zero) throw new GException (error);
			full_path = Marshaller.PtrToStringGFree (native_full_path);
			return ret;
		}

		public bool LoadFromFile (string file, KeyFileFlags flags)
		{
			IntPtr error;
			IntPtr native_file = Marshaller.StringToFilenamePtr (file);
			bool ret = g_key_file_load_from_file (Handle, native_file, (int) flags, out error);
			Marshaller.Free (native_file);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool RemoveComment (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			bool ret = g_key_file_remove_comment (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool RemoveGroup (string group_name)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			bool ret = g_key_file_remove_group (Handle, native_group_name, out error);
			Marshaller.Free (native_group_name);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public bool RemoveKey (string group_name, string key)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			bool ret = g_key_file_remove_key (Handle, native_group_name, native_key, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public void Save (string filename)
		{
			byte [] content = ToData ();
			System.IO.FileStream stream = System.IO.File.Create (filename);
			stream.Write (content, 0, content.Length);
			stream.Close ();
		}
	
		public void SetBoolean (string group_name, string key, bool value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_boolean (Handle, native_group_name, native_key, value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public void SetBooleanList (string group_name, string key, bool[] list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_boolean_list (Handle, native_group_name, native_key, list, new UIntPtr ((ulong) (list.Length)));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public bool SetComment (string group_name, string key, string comment)
		{
			IntPtr error;
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_comment = Marshaller.StringToPtrGStrdup (comment);
			bool ret = g_key_file_set_comment (Handle, native_group_name, native_key, native_comment, out error);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_comment);
			if (error != IntPtr.Zero) throw new GException (error);
			return ret;
		}

		public void SetDouble (string group_name, string key, double value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_double (Handle, native_group_name, native_key, value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public void SetDoubleList (string group_name, string key, double[] list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_double_list (Handle, native_group_name, native_key, list, new UIntPtr ((ulong) (list.Length)));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public void SetInteger (string group_name, string key, int value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_integer (Handle, native_group_name, native_key, value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public void SetIntegerList (string group_name, string key, int[] list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			g_key_file_set_integer_list (Handle, native_group_name, native_key, list, new UIntPtr ((ulong) list.Length));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
		}

		public void SetListSeparator (char value)
		{ 
			g_key_file_set_list_separator (Handle, (byte) value);
		}

		public void SetLocaleString (string group_name, string key, string locale, string value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_locale = Marshaller.StringToPtrGStrdup (locale);
			IntPtr native_value = Marshaller.StringToPtrGStrdup (value);
			g_key_file_set_locale_string (Handle, native_group_name, native_key, native_locale, native_value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_locale);
			Marshaller.Free (native_value);
		}

		public void SetLocaleStringList (string group_name, string key, string locale, string[] list)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (locale == null)
				throw new ArgumentNullException ("locale");
			if (list == null)
				throw new ArgumentNullException ("list");
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_locale = Marshaller.StringToPtrGStrdup (locale);
			IntPtr native_list = Marshaller.StringArrayToStrvPtr (list);
			g_key_file_set_locale_string_list (Handle, native_group_name, native_key, native_locale, native_list, new UIntPtr ((ulong)list.Length));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_locale);
			Marshaller.StrFreeV (native_list);
		}

		public void SetString (string group_name, string key, string value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_value = Marshaller.StringToPtrGStrdup (value);
			g_key_file_set_string (Handle, native_group_name, native_key, native_value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_value);
		}

		public void SetStringList (string group_name, string key, string[] list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_list = Marshaller.StringArrayToStrvPtr (list);
			g_key_file_set_string_list (Handle, native_group_name, native_key, native_list, new UIntPtr ((ulong)list.Length));
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.StrFreeV (native_list);
		}

		public void SetValue (string group_name, string key, string value)
		{
			IntPtr native_group_name = Marshaller.StringToPtrGStrdup (group_name);
			IntPtr native_key = Marshaller.StringToPtrGStrdup (key);
			IntPtr native_value = Marshaller.StringToPtrGStrdup (value);
			g_key_file_set_value (Handle, native_group_name, native_key, native_value);
			Marshaller.Free (native_group_name);
			Marshaller.Free (native_key);
			Marshaller.Free (native_value);
		}

		public byte[] ToData ()
		{
			UIntPtr native_length;
			IntPtr raw_ret = g_key_file_to_data(Handle, out native_length, IntPtr.Zero);
			byte[] ret = new byte [(int) native_length];
			Marshal.Copy (raw_ret, ret, 0, (int) native_length);
			Marshaller.Free (raw_ret);
			return ret;
		}

		[DllImport (dll)]
		static extern void g_key_file_free (IntPtr raw);

		[DllImport (dll)]
		static extern bool g_key_file_get_boolean (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_boolean_list (IntPtr raw, IntPtr group_name, IntPtr key, out UIntPtr length, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_comment (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern double g_key_file_get_double (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_double_list (IntPtr raw, IntPtr group_name, IntPtr key, out UIntPtr length, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_groups (IntPtr raw, IntPtr dummy);

		[DllImport (dll)]
		static extern int g_key_file_get_integer (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_integer_list (IntPtr raw, IntPtr group_name, IntPtr key, out UIntPtr length, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_keys (IntPtr raw, IntPtr group_name, IntPtr dummy, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_locale_string (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr locale, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_locale_string_list (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr locale, IntPtr dummy, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_start_group (IntPtr raw);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_string (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_string_list (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr dummy, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_get_value (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_has_group (IntPtr raw, IntPtr group_name);

		[DllImport (dll)]
		static extern bool g_key_file_has_key (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_load_from_data (IntPtr raw, byte[] data, UIntPtr length, int flags, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_load_from_data_dirs (IntPtr raw, IntPtr file, out IntPtr full_path, int flags, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_load_from_dirs (IntPtr raw, IntPtr file, IntPtr search_dirs, out IntPtr full_path, int flags, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_load_from_file (IntPtr raw, IntPtr file, int flags, out IntPtr error);

		[DllImport (dll)]
		static extern IntPtr g_key_file_new ();

		[DllImport (dll)]
		static extern bool g_key_file_remove_comment (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_remove_group (IntPtr raw, IntPtr group_name, out IntPtr error);

		[DllImport (dll)]
		static extern bool g_key_file_remove_key (IntPtr raw, IntPtr group_name, IntPtr key, out IntPtr error);

		[DllImport (dll)]
		static extern void g_key_file_set_boolean (IntPtr raw, IntPtr group_name, IntPtr key, bool value);

		[DllImport (dll)]
		static extern void g_key_file_set_boolean_list (IntPtr raw, IntPtr group_name, IntPtr key, bool[] list, UIntPtr n_list);

		[DllImport (dll)]
		static extern bool g_key_file_set_comment (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr comment, out IntPtr error);

		[DllImport (dll)]
		static extern void g_key_file_set_double (IntPtr raw, IntPtr group_name, IntPtr key, double value);

		[DllImport (dll)]
		static extern void g_key_file_set_double_list (IntPtr raw, IntPtr group_name, IntPtr key, double[] list, UIntPtr n_list);

		[DllImport (dll)]
		static extern void g_key_file_set_integer (IntPtr raw, IntPtr group_name, IntPtr key, int value);

		[DllImport (dll)]
		static extern void g_key_file_set_integer_list (IntPtr raw, IntPtr group_name, IntPtr key, int[] list, UIntPtr n_list);

		[DllImport (dll)]
		static extern void g_key_file_set_list_separator (IntPtr raw, byte separator);

		[DllImport (dll)]
		static extern void g_key_file_set_locale_string (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr locale, IntPtr value);

		[DllImport (dll)]
		static extern void g_key_file_set_locale_string_list (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr locale, IntPtr list, UIntPtr length);

		[DllImport (dll)]
		static extern void g_key_file_set_string (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr value);

		[DllImport (dll)]
		static extern void g_key_file_set_string_list (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr list, UIntPtr n_list);

		[DllImport (dll)]
		static extern void g_key_file_set_value (IntPtr raw, IntPtr group_name, IntPtr key, IntPtr value);

		[DllImport (dll)]
		static extern IntPtr g_key_file_to_data (IntPtr raw, out UIntPtr length, IntPtr dummy);

	}
}
