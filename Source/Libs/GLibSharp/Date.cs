// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public partial class Date : GLib.Opaque {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_date_get_type();
		static readonly d_g_date_get_type g_date_get_type = FuncLoader.LoadFunction<d_g_date_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_type"));

		public static GLib.GType GType { 
			get {
				IntPtr raw_ret = g_date_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_add_days(IntPtr raw, uint n_days);
		static readonly d_g_date_add_days g_date_add_days = FuncLoader.LoadFunction<d_g_date_add_days>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_add_days"));

		public void AddDays(uint n_days) {
			g_date_add_days(Handle, n_days);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_add_months(IntPtr raw, uint n_months);
		static readonly d_g_date_add_months g_date_add_months = FuncLoader.LoadFunction<d_g_date_add_months>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_add_months"));

		public void AddMonths(uint n_months) {
			g_date_add_months(Handle, n_months);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_add_years(IntPtr raw, uint n_years);
		static readonly d_g_date_add_years g_date_add_years = FuncLoader.LoadFunction<d_g_date_add_years>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_add_years"));

		public void AddYears(uint n_years) {
			g_date_add_years(Handle, n_years);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_clamp(IntPtr raw, IntPtr min_date, IntPtr max_date);
		static readonly d_g_date_clamp g_date_clamp = FuncLoader.LoadFunction<d_g_date_clamp>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_clamp"));

		public void Clamp(GLib.Date min_date, GLib.Date max_date) {
			g_date_clamp(Handle, min_date == null ? IntPtr.Zero : min_date.Handle, max_date == null ? IntPtr.Zero : max_date.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_clear(IntPtr raw, uint n_dates);
		static readonly d_g_date_clear g_date_clear = FuncLoader.LoadFunction<d_g_date_clear>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_clear"));

		public void Clear(uint n_dates) {
			g_date_clear(Handle, n_dates);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_date_compare(IntPtr raw, IntPtr rhs);
		static readonly d_g_date_compare g_date_compare = FuncLoader.LoadFunction<d_g_date_compare>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_compare"));

		public int Compare(GLib.Date rhs) {
			int raw_ret = g_date_compare(Handle, rhs == null ? IntPtr.Zero : rhs.Handle);
			int ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_date_days_between(IntPtr raw, IntPtr date2);
		static readonly d_g_date_days_between g_date_days_between = FuncLoader.LoadFunction<d_g_date_days_between>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_days_between"));

		public int DaysBetween(GLib.Date date2) {
			int raw_ret = g_date_days_between(Handle, date2 == null ? IntPtr.Zero : date2.Handle);
			int ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_date_get_day(IntPtr raw);
		static readonly d_g_date_get_day g_date_get_day = FuncLoader.LoadFunction<d_g_date_get_day>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_day"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_day(IntPtr raw, byte day);
		static readonly d_g_date_set_day g_date_set_day = FuncLoader.LoadFunction<d_g_date_set_day>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_day"));

		public byte Day { 
			get {
				byte raw_ret = g_date_get_day(Handle);
				byte ret = raw_ret;
				return ret;
			}
			set {
				g_date_set_day(Handle, value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_date_get_day_of_year(IntPtr raw);
		static readonly d_g_date_get_day_of_year g_date_get_day_of_year = FuncLoader.LoadFunction<d_g_date_get_day_of_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_day_of_year"));

		public uint DayOfYear { 
			get {
				uint raw_ret = g_date_get_day_of_year(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_date_get_iso8601_week_of_year(IntPtr raw);
		static readonly d_g_date_get_iso8601_week_of_year g_date_get_iso8601_week_of_year = FuncLoader.LoadFunction<d_g_date_get_iso8601_week_of_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_iso8601_week_of_year"));

		public uint Iso8601WeekOfYear { 
			get {
				uint raw_ret = g_date_get_iso8601_week_of_year(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_date_get_julian(IntPtr raw);
		static readonly d_g_date_get_julian g_date_get_julian = FuncLoader.LoadFunction<d_g_date_get_julian>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_julian"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_julian(IntPtr raw, uint julian_date);
		static readonly d_g_date_set_julian g_date_set_julian = FuncLoader.LoadFunction<d_g_date_set_julian>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_julian"));

		public uint Julian { 
			get {
				uint raw_ret = g_date_get_julian(Handle);
				uint ret = raw_ret;
				return ret;
			}
			set {
				g_date_set_julian(Handle, value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_date_get_monday_week_of_year(IntPtr raw);
		static readonly d_g_date_get_monday_week_of_year g_date_get_monday_week_of_year = FuncLoader.LoadFunction<d_g_date_get_monday_week_of_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_monday_week_of_year"));

		public uint MondayWeekOfYear { 
			get {
				uint raw_ret = g_date_get_monday_week_of_year(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_date_get_month(IntPtr raw);
		static readonly d_g_date_get_month g_date_get_month = FuncLoader.LoadFunction<d_g_date_get_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_month"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_month(IntPtr raw, int month);
		static readonly d_g_date_set_month g_date_set_month = FuncLoader.LoadFunction<d_g_date_set_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_month"));

		public int Month { 
			get {
				int raw_ret = g_date_get_month(Handle);
				int ret = raw_ret;
				return ret;
			}
			set {
				g_date_set_month(Handle, value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_date_get_sunday_week_of_year(IntPtr raw);
		static readonly d_g_date_get_sunday_week_of_year g_date_get_sunday_week_of_year = FuncLoader.LoadFunction<d_g_date_get_sunday_week_of_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_sunday_week_of_year"));

		public uint SundayWeekOfYear { 
			get {
				uint raw_ret = g_date_get_sunday_week_of_year(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_date_get_weekday(IntPtr raw);
		static readonly d_g_date_get_weekday g_date_get_weekday = FuncLoader.LoadFunction<d_g_date_get_weekday>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_weekday"));

		public int Weekday { 
			get {
				int raw_ret = g_date_get_weekday(Handle);
				int ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate ushort d_g_date_get_year(IntPtr raw);
		static readonly d_g_date_get_year g_date_get_year = FuncLoader.LoadFunction<d_g_date_get_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_year"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_year(IntPtr raw, ushort year);
		static readonly d_g_date_set_year g_date_set_year = FuncLoader.LoadFunction<d_g_date_set_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_year"));

		public ushort Year { 
			get {
				ushort raw_ret = g_date_get_year(Handle);
				ushort ret = raw_ret;
				return ret;
			}
			set {
				g_date_set_year(Handle, value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_is_first_of_month(IntPtr raw);
		static readonly d_g_date_is_first_of_month g_date_is_first_of_month = FuncLoader.LoadFunction<d_g_date_is_first_of_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_is_first_of_month"));

		public bool IsFirstOfMonth { 
			get {
				bool raw_ret = g_date_is_first_of_month(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_is_last_of_month(IntPtr raw);
		static readonly d_g_date_is_last_of_month g_date_is_last_of_month = FuncLoader.LoadFunction<d_g_date_is_last_of_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_is_last_of_month"));

		public bool IsLastOfMonth { 
			get {
				bool raw_ret = g_date_is_last_of_month(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_order(IntPtr raw, IntPtr date2);
		static readonly d_g_date_order g_date_order = FuncLoader.LoadFunction<d_g_date_order>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_order"));

		public void Order(GLib.Date date2) {
			g_date_order(Handle, date2 == null ? IntPtr.Zero : date2.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_dmy(IntPtr raw, byte day, int month, ushort y);
		static readonly d_g_date_set_dmy g_date_set_dmy = FuncLoader.LoadFunction<d_g_date_set_dmy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_dmy"));

		public void SetDmy(byte day, int month, ushort y) {
			g_date_set_dmy(Handle, day, month, y);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_parse(IntPtr raw, IntPtr str);
		static readonly d_g_date_set_parse g_date_set_parse = FuncLoader.LoadFunction<d_g_date_set_parse>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_parse"));

		public string Parse { 
			set {
				IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
				g_date_set_parse(Handle, native_value);
				GLib.Marshaller.Free (native_value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_time(IntPtr raw, int time_);
		static readonly d_g_date_set_time g_date_set_time = FuncLoader.LoadFunction<d_g_date_set_time>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_time"));

		[Obsolete]
		public int Time { 
			set {
				g_date_set_time(Handle, value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_time_t(IntPtr raw, IntPtr timet);
		static readonly d_g_date_set_time_t g_date_set_time_t = FuncLoader.LoadFunction<d_g_date_set_time_t>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_time_t"));

		public long TimeT { 
			set {
				g_date_set_time_t(Handle, new IntPtr (value));
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_set_time_val(IntPtr raw, IntPtr value);
		static readonly d_g_date_set_time_val g_date_set_time_val = FuncLoader.LoadFunction<d_g_date_set_time_val>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_set_time_val"));

		public GLib.TimeVal TimeVal { 
			set {
				IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
				g_date_set_time_val(Handle, native_value);
				value = GLib.TimeVal.New (native_value);
				Marshal.FreeHGlobal (native_value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_subtract_days(IntPtr raw, uint n_days);
		static readonly d_g_date_subtract_days g_date_subtract_days = FuncLoader.LoadFunction<d_g_date_subtract_days>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_subtract_days"));

		public void SubtractDays(uint n_days) {
			g_date_subtract_days(Handle, n_days);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_subtract_months(IntPtr raw, uint n_months);
		static readonly d_g_date_subtract_months g_date_subtract_months = FuncLoader.LoadFunction<d_g_date_subtract_months>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_subtract_months"));

		public void SubtractMonths(uint n_months) {
			g_date_subtract_months(Handle, n_months);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_subtract_years(IntPtr raw, uint n_years);
		static readonly d_g_date_subtract_years g_date_subtract_years = FuncLoader.LoadFunction<d_g_date_subtract_years>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_subtract_years"));

		public void SubtractYears(uint n_years) {
			g_date_subtract_years(Handle, n_years);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_to_struct_tm(IntPtr raw, IntPtr tm);
		static readonly d_g_date_to_struct_tm g_date_to_struct_tm = FuncLoader.LoadFunction<d_g_date_to_struct_tm>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_to_struct_tm"));

		public void ToStructTm(IntPtr tm) {
			g_date_to_struct_tm(Handle, tm);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid(IntPtr raw);
		static readonly d_g_date_valid g_date_valid = FuncLoader.LoadFunction<d_g_date_valid>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid"));

		public bool Valid() {
			bool raw_ret = g_date_valid(Handle);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_date_get_days_in_month(int month, ushort year);
		static readonly d_g_date_get_days_in_month g_date_get_days_in_month = FuncLoader.LoadFunction<d_g_date_get_days_in_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_days_in_month"));

		public static byte GetDaysInMonth(int month, ushort year) {
			byte raw_ret = g_date_get_days_in_month(month, year);
			byte ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_date_get_monday_weeks_in_year(ushort year);
		static readonly d_g_date_get_monday_weeks_in_year g_date_get_monday_weeks_in_year = FuncLoader.LoadFunction<d_g_date_get_monday_weeks_in_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_monday_weeks_in_year"));

		public static byte GetMondayWeeksInYear(ushort year) {
			byte raw_ret = g_date_get_monday_weeks_in_year(year);
			byte ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate byte d_g_date_get_sunday_weeks_in_year(ushort year);
		static readonly d_g_date_get_sunday_weeks_in_year g_date_get_sunday_weeks_in_year = FuncLoader.LoadFunction<d_g_date_get_sunday_weeks_in_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_get_sunday_weeks_in_year"));

		public static byte GetSundayWeeksInYear(ushort year) {
			byte raw_ret = g_date_get_sunday_weeks_in_year(year);
			byte ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_is_leap_year(ushort year);
		static readonly d_g_date_is_leap_year g_date_is_leap_year = FuncLoader.LoadFunction<d_g_date_is_leap_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_is_leap_year"));

		public static bool IsLeapYear(ushort year) {
			bool raw_ret = g_date_is_leap_year(year);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate UIntPtr d_g_date_strftime(IntPtr s, UIntPtr slen, IntPtr format, IntPtr date);
		static readonly d_g_date_strftime g_date_strftime = FuncLoader.LoadFunction<d_g_date_strftime>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_strftime"));

		public static ulong Strftime(string s, string format, GLib.Date date) {
			IntPtr native_s = GLib.Marshaller.StringToPtrGStrdup (s);
			IntPtr native_format = GLib.Marshaller.StringToPtrGStrdup (format);
			UIntPtr raw_ret = g_date_strftime(native_s, new UIntPtr ((ulong) System.Text.Encoding.UTF8.GetByteCount (s)), native_format, date == null ? IntPtr.Zero : date.Handle);
			ulong ret = (ulong) raw_ret;
			GLib.Marshaller.Free (native_s);
			GLib.Marshaller.Free (native_format);
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_day(byte day);
		static readonly d_g_date_valid_day g_date_valid_day = FuncLoader.LoadFunction<d_g_date_valid_day>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_day"));

		public static bool ValidDay(byte day) {
			bool raw_ret = g_date_valid_day(day);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_dmy(byte day, int month, ushort year);
		static readonly d_g_date_valid_dmy g_date_valid_dmy = FuncLoader.LoadFunction<d_g_date_valid_dmy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_dmy"));

		public static bool ValidDmy(byte day, int month, ushort year) {
			bool raw_ret = g_date_valid_dmy(day, month, year);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_julian(uint julian_date);
		static readonly d_g_date_valid_julian g_date_valid_julian = FuncLoader.LoadFunction<d_g_date_valid_julian>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_julian"));

		public static bool ValidJulian(uint julian_date) {
			bool raw_ret = g_date_valid_julian(julian_date);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_month(int month);
		static readonly d_g_date_valid_month g_date_valid_month = FuncLoader.LoadFunction<d_g_date_valid_month>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_month"));

		public static bool ValidMonth(int month) {
			bool raw_ret = g_date_valid_month(month);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_weekday(int weekday);
		static readonly d_g_date_valid_weekday g_date_valid_weekday = FuncLoader.LoadFunction<d_g_date_valid_weekday>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_weekday"));

		public static bool ValidWeekday(int weekday) {
			bool raw_ret = g_date_valid_weekday(weekday);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_date_valid_year(ushort year);
		static readonly d_g_date_valid_year g_date_valid_year = FuncLoader.LoadFunction<d_g_date_valid_year>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_valid_year"));

		public static bool ValidYear(ushort year) {
			bool raw_ret = g_date_valid_year(year);
			bool ret = raw_ret;
			return ret;
		}

		public Date(IntPtr raw) : base(raw) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_date_new();
		static readonly d_g_date_new g_date_new = FuncLoader.LoadFunction<d_g_date_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_new"));

		public Date () 
		{
			Raw = g_date_new();
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_date_new_dmy(byte day, int month, ushort year);
		static readonly d_g_date_new_dmy g_date_new_dmy = FuncLoader.LoadFunction<d_g_date_new_dmy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_new_dmy"));

		public Date (byte day, int month, ushort year) 
		{
			Raw = g_date_new_dmy(day, month, year);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_date_new_julian(uint julian_day);
		static readonly d_g_date_new_julian g_date_new_julian = FuncLoader.LoadFunction<d_g_date_new_julian>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_new_julian"));

		public Date (uint julian_day) 
		{
			Raw = g_date_new_julian(julian_day);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_date_free(IntPtr raw);
		static readonly d_g_date_free g_date_free = FuncLoader.LoadFunction<d_g_date_free>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_date_free"));

		protected override void Free (IntPtr raw)
		{
			g_date_free (raw);
		}

		class FinalizerInfo {
            readonly IntPtr handle;

			public FinalizerInfo (IntPtr handle)
			{
				this.handle = handle;
			}

			public bool Handler ()
			{
				g_date_free (handle);
				return false;
			}
		}

		~Date ()
		{
			if (!Owned)
				return;
			FinalizerInfo info = new FinalizerInfo (Handle);
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (info.Handler));
		}

#endregion
	}
}

