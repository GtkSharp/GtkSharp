//
// Cairo.cs - a simplistic binding of the Cairo API to C#.
//
// Authors: Duncan Mak (duncan@ximian.com)
//          Hisham Mardam Bey (hisham.mardambey@gmail.com)
//          John Luke (john.luke@gmail.com)
//          Alp Toker (alp@atoker.com)
//
// (C) Ximian, Inc. 2003
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 John Luke
// Copyright (C) 2006 Alp Toker
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
//

using System;
using System.Runtime.InteropServices;

namespace Cairo
{
	// sort the functions like in the following page so it is easier to find what is missing
	// http://cairographics.org/manual/index-all.html

	internal static class NativeMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_append_path(IntPtr cr, IntPtr path);
		internal static d_cairo_append_path cairo_append_path = FuncLoader.LoadFunction<d_cairo_append_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_append_path"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_arc(IntPtr cr, double xc, double yc, double radius, double angle1, double angle2);
		internal static d_cairo_arc cairo_arc = FuncLoader.LoadFunction<d_cairo_arc>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_arc"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_arc_negative(IntPtr cr, double xc, double yc, double radius, double angle1, double angle2);
		internal static d_cairo_arc_negative cairo_arc_negative = FuncLoader.LoadFunction<d_cairo_arc_negative>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_arc_negative"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_atsui_font_face_create_for_atsu_font_id(IntPtr font_id);
		internal static d_cairo_atsui_font_face_create_for_atsu_font_id cairo_atsui_font_face_create_for_atsu_font_id = FuncLoader.LoadFunction<d_cairo_atsui_font_face_create_for_atsu_font_id>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_atsui_font_face_create_for_atsu_font_id"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_clip(IntPtr cr);
		internal static d_cairo_clip cairo_clip = FuncLoader.LoadFunction<d_cairo_clip>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_clip"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_clip_extents(IntPtr cr, out double x1, out double y1, out double x2, out double y2);
		internal static d_cairo_clip_extents cairo_clip_extents = FuncLoader.LoadFunction<d_cairo_clip_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_clip_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_clip_preserve(IntPtr cr);
		internal static d_cairo_clip_preserve cairo_clip_preserve = FuncLoader.LoadFunction<d_cairo_clip_preserve>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_clip_preserve"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_close_path(IntPtr cr);
		internal static d_cairo_close_path cairo_close_path = FuncLoader.LoadFunction<d_cairo_close_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_close_path"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_copy_clip_rectangle_list(IntPtr cr);
		internal static d_cairo_copy_clip_rectangle_list cairo_copy_clip_rectangle_list = FuncLoader.LoadFunction<d_cairo_copy_clip_rectangle_list>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_copy_clip_rectangle_list"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_copy_page(IntPtr cr);
		internal static d_cairo_copy_page cairo_copy_page = FuncLoader.LoadFunction<d_cairo_copy_page>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_copy_page"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_copy_path(IntPtr cr);
		internal static d_cairo_copy_path cairo_copy_path = FuncLoader.LoadFunction<d_cairo_copy_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_copy_path"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_copy_path_flat(IntPtr cr);
		internal static d_cairo_copy_path_flat cairo_copy_path_flat = FuncLoader.LoadFunction<d_cairo_copy_path_flat>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_copy_path_flat"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_create(IntPtr target);
		internal static d_cairo_create cairo_create = FuncLoader.LoadFunction<d_cairo_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_create"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_curve_to(IntPtr cr, double x1, double y1, double x2, double y2, double x3, double y3);
		internal static d_cairo_curve_to cairo_curve_to = FuncLoader.LoadFunction<d_cairo_curve_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_curve_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_debug_reset_static_data();
		internal static d_cairo_debug_reset_static_data cairo_debug_reset_static_data = FuncLoader.LoadFunction<d_cairo_debug_reset_static_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_debug_reset_static_data"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_destroy(IntPtr cr);
		internal static d_cairo_destroy cairo_destroy = FuncLoader.LoadFunction<d_cairo_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_device_acquire(IntPtr device);
		internal static d_cairo_device_acquire cairo_device_acquire = FuncLoader.LoadFunction<d_cairo_device_acquire>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_acquire"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_destroy(IntPtr device);
		internal static d_cairo_device_destroy cairo_device_destroy = FuncLoader.LoadFunction<d_cairo_device_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_finish(IntPtr device);
		internal static d_cairo_device_finish cairo_device_finish = FuncLoader.LoadFunction<d_cairo_device_finish>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_finish"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_flush(IntPtr device);
		internal static d_cairo_device_flush cairo_device_flush = FuncLoader.LoadFunction<d_cairo_device_flush>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_flush"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern uint cairo_device_get_reference_count (IntPtr device);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate DeviceType d_cairo_device_get_type(IntPtr device);
		internal static d_cairo_device_get_type cairo_device_get_type = FuncLoader.LoadFunction<d_cairo_device_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_get_type"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_device_get_user_data (IntPtr device, IntPtr key);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_device_reference(IntPtr device);
		internal static d_cairo_device_reference cairo_device_reference = FuncLoader.LoadFunction<d_cairo_device_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_reference"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_release(IntPtr device);
		internal static d_cairo_device_release cairo_device_release = FuncLoader.LoadFunction<d_cairo_device_release>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_release"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern int cairo_device_set_user_data (IntPtr device, IntPtr key, IntPtr user_data, IntPtr destroy);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_device_status(IntPtr device);
		internal static d_cairo_device_status cairo_device_status = FuncLoader.LoadFunction<d_cairo_device_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_status"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_to_user(IntPtr cr, ref double x, ref double y);
		internal static d_cairo_device_to_user cairo_device_to_user = FuncLoader.LoadFunction<d_cairo_device_to_user>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_to_user"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_device_to_user_distance(IntPtr cr, ref double dx, ref double dy);
		internal static d_cairo_device_to_user_distance cairo_device_to_user_distance = FuncLoader.LoadFunction<d_cairo_device_to_user_distance>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_device_to_user_distance"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_fill(IntPtr cr);
		internal static d_cairo_fill cairo_fill = FuncLoader.LoadFunction<d_cairo_fill>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_fill"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_fill_extents(IntPtr cr, out double x1, out double y1, out double x2, out double y2);
		internal static d_cairo_fill_extents cairo_fill_extents = FuncLoader.LoadFunction<d_cairo_fill_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_fill_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_fill_preserve(IntPtr cr);
		internal static d_cairo_fill_preserve cairo_fill_preserve = FuncLoader.LoadFunction<d_cairo_fill_preserve>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_fill_preserve"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_extents(IntPtr cr, out FontExtents extents);
		internal static d_cairo_font_extents cairo_font_extents = FuncLoader.LoadFunction<d_cairo_font_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_face_destroy(IntPtr font_face);
		internal static d_cairo_font_face_destroy cairo_font_face_destroy = FuncLoader.LoadFunction<d_cairo_font_face_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_face_destroy"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint d_cairo_font_face_get_reference_count(IntPtr font_face);
		internal static d_cairo_font_face_get_reference_count cairo_font_face_get_reference_count = FuncLoader.LoadFunction<d_cairo_font_face_get_reference_count>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_face_get_reference_count"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate FontType d_cairo_font_face_get_type(IntPtr font_face);
		internal static d_cairo_font_face_get_type cairo_font_face_get_type = FuncLoader.LoadFunction<d_cairo_font_face_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_face_get_type"));

		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_font_face_get_user_data (IntPtr font_face, IntPtr key);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_font_face_reference(IntPtr font_face);
		internal static d_cairo_font_face_reference cairo_font_face_reference = FuncLoader.LoadFunction<d_cairo_font_face_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_face_reference"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_font_face_set_user_data (IntPtr font_face, IntPtr key, IntPtr user_data, DestroyFunc destroy);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_font_face_status(IntPtr font_face);
		internal static d_cairo_font_face_status cairo_font_face_status = FuncLoader.LoadFunction<d_cairo_font_face_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_face_status"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_font_options_copy(IntPtr original);
		internal static d_cairo_font_options_copy cairo_font_options_copy = FuncLoader.LoadFunction<d_cairo_font_options_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_copy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_font_options_create();
		internal static d_cairo_font_options_create cairo_font_options_create = FuncLoader.LoadFunction<d_cairo_font_options_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_create"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_destroy(IntPtr options);
		internal static d_cairo_font_options_destroy cairo_font_options_destroy = FuncLoader.LoadFunction<d_cairo_font_options_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_font_options_equal(IntPtr options, IntPtr other);
		internal static d_cairo_font_options_equal cairo_font_options_equal = FuncLoader.LoadFunction<d_cairo_font_options_equal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_equal"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Antialias d_cairo_font_options_get_antialias(IntPtr options);
		internal static d_cairo_font_options_get_antialias cairo_font_options_get_antialias = FuncLoader.LoadFunction<d_cairo_font_options_get_antialias>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_get_antialias"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate HintMetrics d_cairo_font_options_get_hint_metrics(IntPtr options);
		internal static d_cairo_font_options_get_hint_metrics cairo_font_options_get_hint_metrics = FuncLoader.LoadFunction<d_cairo_font_options_get_hint_metrics>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_get_hint_metrics"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate HintStyle d_cairo_font_options_get_hint_style(IntPtr options);
		internal static d_cairo_font_options_get_hint_style cairo_font_options_get_hint_style = FuncLoader.LoadFunction<d_cairo_font_options_get_hint_style>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_get_hint_style"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SubpixelOrder d_cairo_font_options_get_subpixel_order(IntPtr options);
		internal static d_cairo_font_options_get_subpixel_order cairo_font_options_get_subpixel_order = FuncLoader.LoadFunction<d_cairo_font_options_get_subpixel_order>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_get_subpixel_order"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate long d_cairo_font_options_hash(IntPtr options);
		internal static d_cairo_font_options_hash cairo_font_options_hash = FuncLoader.LoadFunction<d_cairo_font_options_hash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_hash"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_merge(IntPtr options, IntPtr other);
		internal static d_cairo_font_options_merge cairo_font_options_merge = FuncLoader.LoadFunction<d_cairo_font_options_merge>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_merge"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_set_antialias(IntPtr options, Antialias aa);
		internal static d_cairo_font_options_set_antialias cairo_font_options_set_antialias = FuncLoader.LoadFunction<d_cairo_font_options_set_antialias>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_set_antialias"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_set_hint_metrics(IntPtr options, HintMetrics metrics);
		internal static d_cairo_font_options_set_hint_metrics cairo_font_options_set_hint_metrics = FuncLoader.LoadFunction<d_cairo_font_options_set_hint_metrics>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_set_hint_metrics"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_set_hint_style(IntPtr options, HintStyle style);
		internal static d_cairo_font_options_set_hint_style cairo_font_options_set_hint_style = FuncLoader.LoadFunction<d_cairo_font_options_set_hint_style>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_set_hint_style"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_font_options_set_subpixel_order(IntPtr options, SubpixelOrder order);
		internal static d_cairo_font_options_set_subpixel_order cairo_font_options_set_subpixel_order = FuncLoader.LoadFunction<d_cairo_font_options_set_subpixel_order>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_set_subpixel_order"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_font_options_status(IntPtr options);
		internal static d_cairo_font_options_status cairo_font_options_status = FuncLoader.LoadFunction<d_cairo_font_options_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_font_options_status"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_format_stride_for_width(Format format, int width);
		internal static d_cairo_format_stride_for_width cairo_format_stride_for_width = FuncLoader.LoadFunction<d_cairo_format_stride_for_width>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_format_stride_for_width"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_ft_font_face_create_for_ft_face(IntPtr face, int load_flags);
		internal static d_cairo_ft_font_face_create_for_ft_face cairo_ft_font_face_create_for_ft_face = FuncLoader.LoadFunction<d_cairo_ft_font_face_create_for_ft_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ft_font_face_create_for_ft_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_ft_font_face_create_for_pattern(IntPtr fc_pattern);
		internal static d_cairo_ft_font_face_create_for_pattern cairo_ft_font_face_create_for_pattern = FuncLoader.LoadFunction<d_cairo_ft_font_face_create_for_pattern>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ft_font_face_create_for_pattern"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ft_font_options_substitute(IntPtr options, IntPtr pattern);
		internal static d_cairo_ft_font_options_substitute cairo_ft_font_options_substitute = FuncLoader.LoadFunction<d_cairo_ft_font_options_substitute>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ft_font_options_substitute"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_ft_scaled_font_lock_face(IntPtr scaled_font);
		internal static d_cairo_ft_scaled_font_lock_face cairo_ft_scaled_font_lock_face = FuncLoader.LoadFunction<d_cairo_ft_scaled_font_lock_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ft_scaled_font_lock_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ft_scaled_font_unlock_face(IntPtr scaled_font);
		internal static d_cairo_ft_scaled_font_unlock_face cairo_ft_scaled_font_unlock_face = FuncLoader.LoadFunction<d_cairo_ft_scaled_font_unlock_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ft_scaled_font_unlock_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Antialias d_cairo_get_antialias(IntPtr cr);
		internal static d_cairo_get_antialias cairo_get_antialias = FuncLoader.LoadFunction<d_cairo_get_antialias>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_antialias"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_get_current_point(IntPtr cr, out double x, out double y);
		internal static d_cairo_get_current_point cairo_get_current_point = FuncLoader.LoadFunction<d_cairo_get_current_point>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_current_point"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_get_dash(IntPtr cr, IntPtr dashes, out double offset);
		internal static d_cairo_get_dash cairo_get_dash = FuncLoader.LoadFunction<d_cairo_get_dash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_dash"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_get_dash_count(IntPtr cr);
		internal static d_cairo_get_dash_count cairo_get_dash_count = FuncLoader.LoadFunction<d_cairo_get_dash_count>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_dash_count"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate FillRule d_cairo_get_fill_rule(IntPtr cr);
		internal static d_cairo_get_fill_rule cairo_get_fill_rule = FuncLoader.LoadFunction<d_cairo_get_fill_rule>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_fill_rule"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_font_face(IntPtr cr);
		internal static d_cairo_get_font_face cairo_get_font_face = FuncLoader.LoadFunction<d_cairo_get_font_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_font_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_get_font_matrix(IntPtr cr, out Matrix matrix);
		internal static d_cairo_get_font_matrix cairo_get_font_matrix = FuncLoader.LoadFunction<d_cairo_get_font_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_font_matrix"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_get_font_options(IntPtr cr, IntPtr options);
		internal static d_cairo_get_font_options cairo_get_font_options = FuncLoader.LoadFunction<d_cairo_get_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_font_options"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_group_target(IntPtr cr);
		internal static d_cairo_get_group_target cairo_get_group_target = FuncLoader.LoadFunction<d_cairo_get_group_target>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_group_target"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate LineCap d_cairo_get_line_cap(IntPtr cr);
		internal static d_cairo_get_line_cap cairo_get_line_cap = FuncLoader.LoadFunction<d_cairo_get_line_cap>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_line_cap"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate LineJoin d_cairo_get_line_join(IntPtr cr);
		internal static d_cairo_get_line_join cairo_get_line_join = FuncLoader.LoadFunction<d_cairo_get_line_join>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_line_join"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate double d_cairo_get_line_width(IntPtr cr);
		internal static d_cairo_get_line_width cairo_get_line_width = FuncLoader.LoadFunction<d_cairo_get_line_width>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_line_width"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_get_matrix(IntPtr cr, Matrix matrix);
		internal static d_cairo_get_matrix cairo_get_matrix = FuncLoader.LoadFunction<d_cairo_get_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_matrix"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate double d_cairo_get_miter_limit(IntPtr cr);
		internal static d_cairo_get_miter_limit cairo_get_miter_limit = FuncLoader.LoadFunction<d_cairo_get_miter_limit>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_miter_limit"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Operator d_cairo_get_operator(IntPtr cr);
		internal static d_cairo_get_operator cairo_get_operator = FuncLoader.LoadFunction<d_cairo_get_operator>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_operator"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint d_cairo_get_reference_count(IntPtr surface);
		internal static d_cairo_get_reference_count cairo_get_reference_count = FuncLoader.LoadFunction<d_cairo_get_reference_count>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_reference_count"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_scaled_font(IntPtr cr);
		internal static d_cairo_get_scaled_font cairo_get_scaled_font = FuncLoader.LoadFunction<d_cairo_get_scaled_font>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_scaled_font"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_source(IntPtr cr);
		internal static d_cairo_get_source cairo_get_source = FuncLoader.LoadFunction<d_cairo_get_source>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_source"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_target(IntPtr cr);
		internal static d_cairo_get_target cairo_get_target = FuncLoader.LoadFunction<d_cairo_get_target>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_target"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate double d_cairo_get_tolerance(IntPtr cr);
		internal static d_cairo_get_tolerance cairo_get_tolerance = FuncLoader.LoadFunction<d_cairo_get_tolerance>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_tolerance"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_get_user_data(IntPtr cr, IntPtr key);
		internal static d_cairo_get_user_data cairo_get_user_data = FuncLoader.LoadFunction<d_cairo_get_user_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_get_user_data"));
		
		// this isn't in the 1.10 doc index
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_glitz_surface_create(IntPtr surface);
		internal static d_cairo_glitz_surface_create cairo_glitz_surface_create = FuncLoader.LoadFunction<d_cairo_glitz_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_glitz_surface_create"));

		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_glyph_allocate (int num_glyphs);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_glyph_extents(IntPtr cr, IntPtr glyphs, int num_glyphs, out TextExtents extents);
		internal static d_cairo_glyph_extents cairo_glyph_extents = FuncLoader.LoadFunction<d_cairo_glyph_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_glyph_extents"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_glyph_free (IntPtr glyphs);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_glyph_path(IntPtr cr, IntPtr glyphs, int num_glyphs);
		internal static d_cairo_glyph_path cairo_glyph_path = FuncLoader.LoadFunction<d_cairo_glyph_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_glyph_path"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_has_current_point(IntPtr cr);
		internal static d_cairo_has_current_point cairo_has_current_point = FuncLoader.LoadFunction<d_cairo_has_current_point>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_has_current_point"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_identity_matrix(IntPtr cr);
		internal static d_cairo_identity_matrix cairo_identity_matrix = FuncLoader.LoadFunction<d_cairo_identity_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_identity_matrix"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_create(Cairo.Format format, int width, int height);
		internal static d_cairo_image_surface_create cairo_image_surface_create = FuncLoader.LoadFunction<d_cairo_image_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_create"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_create_for_data(byte[] data, Cairo.Format format, int width, int height, int stride);
		internal static d_cairo_image_surface_create_for_data cairo_image_surface_create_for_data = FuncLoader.LoadFunction<d_cairo_image_surface_create_for_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_create_for_data"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_create_for_data2(IntPtr data, Cairo.Format format, int width, int height, int stride);
		internal static d_cairo_image_surface_create_for_data2 cairo_image_surface_create_for_data2 = FuncLoader.LoadFunction<d_cairo_image_surface_create_for_data2>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_create_for_data"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_create_from_png(string filename);
		internal static d_cairo_image_surface_create_from_png cairo_image_surface_create_from_png = FuncLoader.LoadFunction<d_cairo_image_surface_create_from_png>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_create_from_png"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_image_surface_create_from_png_stream  (string filename);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_get_data(IntPtr surface);
		internal static d_cairo_image_surface_get_data cairo_image_surface_get_data = FuncLoader.LoadFunction<d_cairo_image_surface_get_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_get_data"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Format d_cairo_image_surface_get_format(IntPtr surface);
		internal static d_cairo_image_surface_get_format cairo_image_surface_get_format = FuncLoader.LoadFunction<d_cairo_image_surface_get_format>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_get_format"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_image_surface_get_height(IntPtr surface);
		internal static d_cairo_image_surface_get_height cairo_image_surface_get_height = FuncLoader.LoadFunction<d_cairo_image_surface_get_height>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_get_height"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_image_surface_get_stride(IntPtr surface);
		internal static d_cairo_image_surface_get_stride cairo_image_surface_get_stride = FuncLoader.LoadFunction<d_cairo_image_surface_get_stride>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_get_stride"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_image_surface_get_width(IntPtr surface);
		internal static d_cairo_image_surface_get_width cairo_image_surface_get_width = FuncLoader.LoadFunction<d_cairo_image_surface_get_width>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_image_surface_get_width"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_in_clip(IntPtr cr, double x, double y);
		internal static d_cairo_in_clip cairo_in_clip = FuncLoader.LoadFunction<d_cairo_in_clip>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_in_clip"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_in_fill(IntPtr cr, double x, double y);
		internal static d_cairo_in_fill cairo_in_fill = FuncLoader.LoadFunction<d_cairo_in_fill>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_in_fill"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_in_stroke(IntPtr cr, double x, double y);
		internal static d_cairo_in_stroke cairo_in_stroke = FuncLoader.LoadFunction<d_cairo_in_stroke>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_in_stroke"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_line_to(IntPtr cr, double x, double y);
		internal static d_cairo_line_to cairo_line_to = FuncLoader.LoadFunction<d_cairo_line_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_line_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_mask(IntPtr cr, IntPtr pattern);
		internal static d_cairo_mask cairo_mask = FuncLoader.LoadFunction<d_cairo_mask>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_mask"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_mask_surface(IntPtr cr, IntPtr surface, double x, double y);
		internal static d_cairo_mask_surface cairo_mask_surface = FuncLoader.LoadFunction<d_cairo_mask_surface>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_mask_surface"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_init(Matrix matrix, double xx, double yx, double xy, double yy, double x0, double y0);
		internal static d_cairo_matrix_init cairo_matrix_init = FuncLoader.LoadFunction<d_cairo_matrix_init>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_init"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_init_identity(Matrix matrix);
		internal static d_cairo_matrix_init_identity cairo_matrix_init_identity = FuncLoader.LoadFunction<d_cairo_matrix_init_identity>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_init_identity"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_init_rotate(Matrix matrix, double radians);
		internal static d_cairo_matrix_init_rotate cairo_matrix_init_rotate = FuncLoader.LoadFunction<d_cairo_matrix_init_rotate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_init_rotate"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_init_scale(Matrix matrix, double sx, double sy);
		internal static d_cairo_matrix_init_scale cairo_matrix_init_scale = FuncLoader.LoadFunction<d_cairo_matrix_init_scale>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_init_scale"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_init_translate(Matrix matrix, double tx, double ty);
		internal static d_cairo_matrix_init_translate cairo_matrix_init_translate = FuncLoader.LoadFunction<d_cairo_matrix_init_translate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_init_translate"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_matrix_invert(Matrix matrix);
		internal static d_cairo_matrix_invert cairo_matrix_invert = FuncLoader.LoadFunction<d_cairo_matrix_invert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_invert"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_multiply(Matrix result, Matrix a, Matrix b);
		internal static d_cairo_matrix_multiply cairo_matrix_multiply = FuncLoader.LoadFunction<d_cairo_matrix_multiply>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_multiply"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_rotate(Matrix matrix, double radians);
		internal static d_cairo_matrix_rotate cairo_matrix_rotate = FuncLoader.LoadFunction<d_cairo_matrix_rotate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_rotate"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_scale(Matrix matrix, double sx, double sy);
		internal static d_cairo_matrix_scale cairo_matrix_scale = FuncLoader.LoadFunction<d_cairo_matrix_scale>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_scale"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_transform_distance(Matrix matrix, ref double dx, ref double dy);
		internal static d_cairo_matrix_transform_distance cairo_matrix_transform_distance = FuncLoader.LoadFunction<d_cairo_matrix_transform_distance>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_transform_distance"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_transform_point(Matrix matrix, ref double x, ref double y);
		internal static d_cairo_matrix_transform_point cairo_matrix_transform_point = FuncLoader.LoadFunction<d_cairo_matrix_transform_point>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_transform_point"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_matrix_translate(Matrix matrix, double tx, double ty);
		internal static d_cairo_matrix_translate cairo_matrix_translate = FuncLoader.LoadFunction<d_cairo_matrix_translate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_matrix_translate"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_move_to(IntPtr cr, double x, double y);
		internal static d_cairo_move_to cairo_move_to = FuncLoader.LoadFunction<d_cairo_move_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_move_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_new_path(IntPtr cr);
		internal static d_cairo_new_path cairo_new_path = FuncLoader.LoadFunction<d_cairo_new_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_new_path"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_new_sub_path(IntPtr cr);
		internal static d_cairo_new_sub_path cairo_new_sub_path = FuncLoader.LoadFunction<d_cairo_new_sub_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_new_sub_path"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_paint(IntPtr cr);
		internal static d_cairo_paint cairo_paint = FuncLoader.LoadFunction<d_cairo_paint>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_paint"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_paint_with_alpha(IntPtr cr, double alpha);
		internal static d_cairo_paint_with_alpha cairo_paint_with_alpha = FuncLoader.LoadFunction<d_cairo_paint_with_alpha>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_paint_with_alpha"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_path_destroy(IntPtr path);
		internal static d_cairo_path_destroy cairo_path_destroy = FuncLoader.LoadFunction<d_cairo_path_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_path_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_path_extents(IntPtr cr, out double x1, out double y1, out double x2, out double y2);
		internal static d_cairo_path_extents cairo_path_extents = FuncLoader.LoadFunction<d_cairo_path_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_path_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_add_color_stop_rgb(IntPtr pattern, double offset, double red, double green, double blue);
		internal static d_cairo_pattern_add_color_stop_rgb cairo_pattern_add_color_stop_rgb = FuncLoader.LoadFunction<d_cairo_pattern_add_color_stop_rgb>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_add_color_stop_rgb"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_add_color_stop_rgba(IntPtr pattern, double offset, double red, double green, double blue, double alpha);
		internal static d_cairo_pattern_add_color_stop_rgba cairo_pattern_add_color_stop_rgba = FuncLoader.LoadFunction<d_cairo_pattern_add_color_stop_rgba>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_add_color_stop_rgba"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_create_for_surface(IntPtr surface);
		internal static d_cairo_pattern_create_for_surface cairo_pattern_create_for_surface = FuncLoader.LoadFunction<d_cairo_pattern_create_for_surface>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_create_for_surface"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_create_linear(double x0, double y0, double x1, double y1);
		internal static d_cairo_pattern_create_linear cairo_pattern_create_linear = FuncLoader.LoadFunction<d_cairo_pattern_create_linear>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_create_linear"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_create_radial(double cx0, double cy0, double radius0, double cx1, double cy1, double radius1);
		internal static d_cairo_pattern_create_radial cairo_pattern_create_radial = FuncLoader.LoadFunction<d_cairo_pattern_create_radial>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_create_radial"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_create_rgb(double r, double g, double b);
		internal static d_cairo_pattern_create_rgb cairo_pattern_create_rgb = FuncLoader.LoadFunction<d_cairo_pattern_create_rgb>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_create_rgb"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_create_rgba(double r, double g, double b, double a);
		internal static d_cairo_pattern_create_rgba cairo_pattern_create_rgba = FuncLoader.LoadFunction<d_cairo_pattern_create_rgba>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_create_rgba"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_destroy(IntPtr pattern);
		internal static d_cairo_pattern_destroy cairo_pattern_destroy = FuncLoader.LoadFunction<d_cairo_pattern_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_color_stop_count(IntPtr pattern, out int count);
		internal static d_cairo_pattern_get_color_stop_count cairo_pattern_get_color_stop_count = FuncLoader.LoadFunction<d_cairo_pattern_get_color_stop_count>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_color_stop_count"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_color_stop_rgba(IntPtr pattern, int index, out double offset, out double red, out double green, out double blue, out double alpha);
		internal static d_cairo_pattern_get_color_stop_rgba cairo_pattern_get_color_stop_rgba = FuncLoader.LoadFunction<d_cairo_pattern_get_color_stop_rgba>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_color_stop_rgba"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Extend d_cairo_pattern_get_extend(IntPtr pattern);
		internal static d_cairo_pattern_get_extend cairo_pattern_get_extend = FuncLoader.LoadFunction<d_cairo_pattern_get_extend>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_extend"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Filter d_cairo_pattern_get_filter(IntPtr pattern);
		internal static d_cairo_pattern_get_filter cairo_pattern_get_filter = FuncLoader.LoadFunction<d_cairo_pattern_get_filter>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_filter"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_linear_points(IntPtr pattern, out double x0, out double y0, out double x1, out double y1);
		internal static d_cairo_pattern_get_linear_points cairo_pattern_get_linear_points = FuncLoader.LoadFunction<d_cairo_pattern_get_linear_points>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_linear_points"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_get_matrix(IntPtr pattern, Matrix matrix);
		internal static d_cairo_pattern_get_matrix cairo_pattern_get_matrix = FuncLoader.LoadFunction<d_cairo_pattern_get_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_matrix"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_radial_circles(IntPtr pattern, out double x0, out double y0, out double r0, out double x1, out double y1, out double r1);
		internal static d_cairo_pattern_get_radial_circles cairo_pattern_get_radial_circles = FuncLoader.LoadFunction<d_cairo_pattern_get_radial_circles>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_radial_circles"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern uint cairo_pattern_get_reference_count (IntPtr pattern);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_rgba(IntPtr pattern, out double red, out double green, out double blue, out double alpha);
		internal static d_cairo_pattern_get_rgba cairo_pattern_get_rgba = FuncLoader.LoadFunction<d_cairo_pattern_get_rgba>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_rgba"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_get_surface(IntPtr pattern, out IntPtr surface);
		internal static d_cairo_pattern_get_surface cairo_pattern_get_surface = FuncLoader.LoadFunction<d_cairo_pattern_get_surface>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_surface"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate PatternType d_cairo_pattern_get_type(IntPtr pattern);
		internal static d_cairo_pattern_get_type cairo_pattern_get_type = FuncLoader.LoadFunction<d_cairo_pattern_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_get_type"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_pattern_get_user_data (IntPtr pattern, IntPtr key);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pattern_reference(IntPtr pattern);
		internal static d_cairo_pattern_reference cairo_pattern_reference = FuncLoader.LoadFunction<d_cairo_pattern_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_reference"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_set_extend(IntPtr pattern, Extend extend);
		internal static d_cairo_pattern_set_extend cairo_pattern_set_extend = FuncLoader.LoadFunction<d_cairo_pattern_set_extend>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_set_extend"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_set_filter(IntPtr pattern, Filter filter);
		internal static d_cairo_pattern_set_filter cairo_pattern_set_filter = FuncLoader.LoadFunction<d_cairo_pattern_set_filter>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_set_filter"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pattern_set_matrix(IntPtr pattern, Matrix matrix);
		internal static d_cairo_pattern_set_matrix cairo_pattern_set_matrix = FuncLoader.LoadFunction<d_cairo_pattern_set_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_set_matrix"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_pattern_set_user_data (IntPtr pattern, IntPtr key, IntPtr user_data, DestroyFunc destroy);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_pattern_status(IntPtr pattern);
		internal static d_cairo_pattern_status cairo_pattern_status = FuncLoader.LoadFunction<d_cairo_pattern_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pattern_status"));
	
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_pdf_get_versions (IntPtr versions, out int num_versions);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pdf_surface_create(string filename, double width, double height);
		internal static d_cairo_pdf_surface_create cairo_pdf_surface_create = FuncLoader.LoadFunction<d_cairo_pdf_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pdf_surface_create"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_pdf_surface_create_for_stream (string filename, double width, double height);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_pdf_surface_restrict_to_version (IntPtr surface, PdfVersion version);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pdf_surface_set_size(IntPtr surface, double x, double y);
		internal static d_cairo_pdf_surface_set_size cairo_pdf_surface_set_size = FuncLoader.LoadFunction<d_cairo_pdf_surface_set_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pdf_surface_set_size"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_pdf_version_to_string (PdfVersion version);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_pop_group(IntPtr cr);
		internal static d_cairo_pop_group cairo_pop_group = FuncLoader.LoadFunction<d_cairo_pop_group>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pop_group"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_pop_group_to_source(IntPtr cr);
		internal static d_cairo_pop_group_to_source cairo_pop_group_to_source = FuncLoader.LoadFunction<d_cairo_pop_group_to_source>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_pop_group_to_source"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_ps_get_levels (IntPtr levels, out int num_levels);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_ps_level_to_string (PSLevel version);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_ps_surface_create(string filename, double width, double height);
		internal static d_cairo_ps_surface_create cairo_ps_surface_create = FuncLoader.LoadFunction<d_cairo_ps_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ps_surface_create"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_ps_surface_create_for_stream (string filename, double width, double height);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ps_surface_dsc_begin_page_setup(IntPtr surface);
		internal static d_cairo_ps_surface_dsc_begin_page_setup cairo_ps_surface_dsc_begin_page_setup = FuncLoader.LoadFunction<d_cairo_ps_surface_dsc_begin_page_setup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ps_surface_dsc_begin_page_setup"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ps_surface_dsc_begin_setup(IntPtr surface);
		internal static d_cairo_ps_surface_dsc_begin_setup cairo_ps_surface_dsc_begin_setup = FuncLoader.LoadFunction<d_cairo_ps_surface_dsc_begin_setup>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ps_surface_dsc_begin_setup"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ps_surface_dsc_comment(IntPtr surface, string comment);
		internal static d_cairo_ps_surface_dsc_comment cairo_ps_surface_dsc_comment = FuncLoader.LoadFunction<d_cairo_ps_surface_dsc_comment>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ps_surface_dsc_comment"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//[return: MarshalAs (UnmanagedType.U1)]
		//internal static extern bool cairo_ps_surface_get_eps (IntPtr surface);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_ps_surface_restrict_to_level (IntPtr surface, PSLevel level);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_ps_surface_set_eps (IntPtr surface, bool eps);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_ps_surface_set_size(IntPtr surface, double x, double y);
		internal static d_cairo_ps_surface_set_size cairo_ps_surface_set_size = FuncLoader.LoadFunction<d_cairo_ps_surface_set_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_ps_surface_set_size"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_push_group(IntPtr cr);
		internal static d_cairo_push_group cairo_push_group = FuncLoader.LoadFunction<d_cairo_push_group>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_push_group"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_push_group_with_content(IntPtr cr, Content content);
		internal static d_cairo_push_group_with_content cairo_push_group_with_content = FuncLoader.LoadFunction<d_cairo_push_group_with_content>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_push_group_with_content"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_quartz_font_face_create_for_atsui_font_id (int font_id);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_quartz_font_face_create_for_cgfont (IntPtr font);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_quartz_surface_create(IntPtr context, bool flipped, int width, int height);
		internal static d_cairo_quartz_surface_create cairo_quartz_surface_create = FuncLoader.LoadFunction<d_cairo_quartz_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_quartz_surface_create"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_quartz_surface_create_for_cg_context (IntPtr context, uint width, uint height);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_quartz_surface_get_cg_context (IntPtr surface);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_recording_surface_create (Content content, IntPtr extents);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_recording_surface_ink_extents (IntPtr surface, out double x, out double y, out double width, out double height);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rectangle(IntPtr cr, double x, double y, double width, double height);
		internal static d_cairo_rectangle cairo_rectangle = FuncLoader.LoadFunction<d_cairo_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rectangle"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rectangle_list_destroy(IntPtr rectangle_list);
		internal static d_cairo_rectangle_list_destroy cairo_rectangle_list_destroy = FuncLoader.LoadFunction<d_cairo_rectangle_list_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rectangle_list_destroy"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_reference(IntPtr cr);
		internal static d_cairo_reference cairo_reference = FuncLoader.LoadFunction<d_cairo_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_reference"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_region_contains_point(IntPtr region, int x, int y);
		internal static d_cairo_region_contains_point cairo_region_contains_point = FuncLoader.LoadFunction<d_cairo_region_contains_point>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_contains_point"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate RegionOverlap d_cairo_region_contains_rectangle(IntPtr region, ref RectangleInt rectangle);
		internal static d_cairo_region_contains_rectangle cairo_region_contains_rectangle = FuncLoader.LoadFunction<d_cairo_region_contains_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_contains_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_region_copy(IntPtr original);
		internal static d_cairo_region_copy cairo_region_copy = FuncLoader.LoadFunction<d_cairo_region_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_copy"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_region_create();
		internal static d_cairo_region_create cairo_region_create = FuncLoader.LoadFunction<d_cairo_region_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_create"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_region_create_rectangle(ref RectangleInt rect);
		internal static d_cairo_region_create_rectangle cairo_region_create_rectangle = FuncLoader.LoadFunction<d_cairo_region_create_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_create_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_region_create_rectangles(RectangleInt[] rects, int count);
		internal static d_cairo_region_create_rectangles cairo_region_create_rectangles = FuncLoader.LoadFunction<d_cairo_region_create_rectangles>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_create_rectangles"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_region_destroy(IntPtr region);
		internal static d_cairo_region_destroy cairo_region_destroy = FuncLoader.LoadFunction<d_cairo_region_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_destroy"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_region_equal(IntPtr a, IntPtr b);
		internal static d_cairo_region_equal cairo_region_equal = FuncLoader.LoadFunction<d_cairo_region_equal>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_equal"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_region_get_extents(IntPtr region, out RectangleInt extents);
		internal static d_cairo_region_get_extents cairo_region_get_extents = FuncLoader.LoadFunction<d_cairo_region_get_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_get_extents"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_region_get_rectangle(IntPtr region, int nth, out RectangleInt rectangle);
		internal static d_cairo_region_get_rectangle cairo_region_get_rectangle = FuncLoader.LoadFunction<d_cairo_region_get_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_get_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_intersect(IntPtr dst, IntPtr other);
		internal static d_cairo_region_intersect cairo_region_intersect = FuncLoader.LoadFunction<d_cairo_region_intersect>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_intersect"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_intersect_rectangle(IntPtr dst, ref RectangleInt rectangle);
		internal static d_cairo_region_intersect_rectangle cairo_region_intersect_rectangle = FuncLoader.LoadFunction<d_cairo_region_intersect_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_intersect_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool d_cairo_region_is_empty(IntPtr region);
		internal static d_cairo_region_is_empty cairo_region_is_empty = FuncLoader.LoadFunction<d_cairo_region_is_empty>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_is_empty"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_region_num_rectangles(IntPtr region);
		internal static d_cairo_region_num_rectangles cairo_region_num_rectangles = FuncLoader.LoadFunction<d_cairo_region_num_rectangles>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_num_rectangles"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_region_reference(IntPtr region);
		internal static d_cairo_region_reference cairo_region_reference = FuncLoader.LoadFunction<d_cairo_region_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_reference"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_status(IntPtr region);
		internal static d_cairo_region_status cairo_region_status = FuncLoader.LoadFunction<d_cairo_region_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_status"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_subtract(IntPtr dst, IntPtr other);
		internal static d_cairo_region_subtract cairo_region_subtract = FuncLoader.LoadFunction<d_cairo_region_subtract>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_subtract"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_subtract_rectangle(IntPtr dst, ref RectangleInt rectangle);
		internal static d_cairo_region_subtract_rectangle cairo_region_subtract_rectangle = FuncLoader.LoadFunction<d_cairo_region_subtract_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_subtract_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_region_translate(IntPtr region, int dx, int dy);
		internal static d_cairo_region_translate cairo_region_translate = FuncLoader.LoadFunction<d_cairo_region_translate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_translate"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_union(IntPtr dst, IntPtr other);
		internal static d_cairo_region_union cairo_region_union = FuncLoader.LoadFunction<d_cairo_region_union>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_union"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_union_rectangle(IntPtr dst, ref RectangleInt rectangle);
		internal static d_cairo_region_union_rectangle cairo_region_union_rectangle = FuncLoader.LoadFunction<d_cairo_region_union_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_union_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_xor(IntPtr dst, IntPtr other);
		internal static d_cairo_region_xor cairo_region_xor = FuncLoader.LoadFunction<d_cairo_region_xor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_xor"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_region_xor_rectangle(IntPtr dst, ref RectangleInt rectangle);
		internal static d_cairo_region_xor_rectangle cairo_region_xor_rectangle = FuncLoader.LoadFunction<d_cairo_region_xor_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_region_xor_rectangle"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rel_curve_to(IntPtr cr, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3);
		internal static d_cairo_rel_curve_to cairo_rel_curve_to = FuncLoader.LoadFunction<d_cairo_rel_curve_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rel_curve_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rel_line_to(IntPtr cr, double dx, double dy);
		internal static d_cairo_rel_line_to cairo_rel_line_to = FuncLoader.LoadFunction<d_cairo_rel_line_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rel_line_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rel_move_to(IntPtr cr, double dx, double dy);
		internal static d_cairo_rel_move_to cairo_rel_move_to = FuncLoader.LoadFunction<d_cairo_rel_move_to>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rel_move_to"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_reset_clip(IntPtr cr);
		internal static d_cairo_reset_clip cairo_reset_clip = FuncLoader.LoadFunction<d_cairo_reset_clip>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_reset_clip"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_restore(IntPtr cr);
		internal static d_cairo_restore cairo_restore = FuncLoader.LoadFunction<d_cairo_restore>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_restore"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_rotate(IntPtr cr, double angle);
		internal static d_cairo_rotate cairo_rotate = FuncLoader.LoadFunction<d_cairo_rotate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_rotate"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_save(IntPtr cr);
		internal static d_cairo_save cairo_save = FuncLoader.LoadFunction<d_cairo_save>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_save"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scale(IntPtr cr, double sx, double sy);
		internal static d_cairo_scale cairo_scale = FuncLoader.LoadFunction<d_cairo_scale>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scale"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_scaled_font_create(IntPtr fontFace, Matrix matrix, Matrix ctm, IntPtr options);
		internal static d_cairo_scaled_font_create cairo_scaled_font_create = FuncLoader.LoadFunction<d_cairo_scaled_font_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_create"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_scaled_font_destroy(IntPtr scaled_font);
		internal static d_cairo_scaled_font_destroy cairo_scaled_font_destroy = FuncLoader.LoadFunction<d_cairo_scaled_font_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scaled_font_extents(IntPtr scaled_font, out FontExtents extents);
		internal static d_cairo_scaled_font_extents cairo_scaled_font_extents = FuncLoader.LoadFunction<d_cairo_scaled_font_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_extents"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scaled_font_get_ctm(IntPtr scaled_font, out Matrix matrix);
		internal static d_cairo_scaled_font_get_ctm cairo_scaled_font_get_ctm = FuncLoader.LoadFunction<d_cairo_scaled_font_get_ctm>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_get_ctm"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_scaled_font_get_font_face(IntPtr scaled_font);
		internal static d_cairo_scaled_font_get_font_face cairo_scaled_font_get_font_face = FuncLoader.LoadFunction<d_cairo_scaled_font_get_font_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_get_font_face"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scaled_font_get_font_matrix(IntPtr scaled_font, out Matrix matrix);
		internal static d_cairo_scaled_font_get_font_matrix cairo_scaled_font_get_font_matrix = FuncLoader.LoadFunction<d_cairo_scaled_font_get_font_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_get_font_matrix"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_scaled_font_get_font_options(IntPtr scaled_font);
		internal static d_cairo_scaled_font_get_font_options cairo_scaled_font_get_font_options = FuncLoader.LoadFunction<d_cairo_scaled_font_get_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_get_font_options"));

		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern uint cairo_scaled_font_get_reference_count (IntPtr scaled_font);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_scaled_font_get_scale_matrix (IntPtr scaled_font, out Matrix matrix);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate FontType d_cairo_scaled_font_get_type(IntPtr scaled_font);
		internal static d_cairo_scaled_font_get_type cairo_scaled_font_get_type = FuncLoader.LoadFunction<d_cairo_scaled_font_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_get_type"));

		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_scaled_font_get_user_data (IntPtr scaled_font, IntPtr key);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scaled_font_glyph_extents(IntPtr scaled_font, IntPtr glyphs, int num_glyphs, out TextExtents extents);
		internal static d_cairo_scaled_font_glyph_extents cairo_scaled_font_glyph_extents = FuncLoader.LoadFunction<d_cairo_scaled_font_glyph_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_glyph_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_scaled_font_reference(IntPtr scaled_font);
		internal static d_cairo_scaled_font_reference cairo_scaled_font_reference = FuncLoader.LoadFunction<d_cairo_scaled_font_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_reference"));

		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_scaled_font_set_user_data (IntPtr scaled_font, IntPtr key, IntPtr user_data, DestroyFunc destroy);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_scaled_font_status(IntPtr scaled_font);
		internal static d_cairo_scaled_font_status cairo_scaled_font_status = FuncLoader.LoadFunction<d_cairo_scaled_font_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_status"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_scaled_font_text_extents(IntPtr scaled_font, string utf8, out TextExtents extents);
		internal static d_cairo_scaled_font_text_extents cairo_scaled_font_text_extents = FuncLoader.LoadFunction<d_cairo_scaled_font_text_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_scaled_font_text_extents"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_scaled_font_text_to_glyphs (IntPtr scaled_font, double x, double y, IntPtr utf8, IntPtr glyphs, out int num_glyphs, IntPtr clusters, out int num_clusters, IntPtr cluster_flags);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_select_font_face(IntPtr cr, string family, FontSlant slant, FontWeight weight);
		internal static d_cairo_select_font_face cairo_select_font_face = FuncLoader.LoadFunction<d_cairo_select_font_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_select_font_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_antialias(IntPtr cr, Antialias antialias);
		internal static d_cairo_set_antialias cairo_set_antialias = FuncLoader.LoadFunction<d_cairo_set_antialias>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_antialias"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_dash(IntPtr cr, double [] dashes, int ndash, double offset);
		internal static d_cairo_set_dash cairo_set_dash = FuncLoader.LoadFunction<d_cairo_set_dash>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_dash"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_fill_rule(IntPtr cr, Cairo.FillRule fill_rule);
		internal static d_cairo_set_fill_rule cairo_set_fill_rule = FuncLoader.LoadFunction<d_cairo_set_fill_rule>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_fill_rule"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_font_face(IntPtr cr, IntPtr fontFace);
		internal static d_cairo_set_font_face cairo_set_font_face = FuncLoader.LoadFunction<d_cairo_set_font_face>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_font_face"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_font_matrix(IntPtr cr, Matrix matrix);
		internal static d_cairo_set_font_matrix cairo_set_font_matrix = FuncLoader.LoadFunction<d_cairo_set_font_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_font_matrix"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_font_options(IntPtr cr, IntPtr options);
		internal static d_cairo_set_font_options cairo_set_font_options = FuncLoader.LoadFunction<d_cairo_set_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_font_options"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_font_size(IntPtr cr, double size);
		internal static d_cairo_set_font_size cairo_set_font_size = FuncLoader.LoadFunction<d_cairo_set_font_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_font_size"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_line_cap(IntPtr cr, LineCap line_cap);
		internal static d_cairo_set_line_cap cairo_set_line_cap = FuncLoader.LoadFunction<d_cairo_set_line_cap>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_line_cap"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_line_join(IntPtr cr, LineJoin line_join);
		internal static d_cairo_set_line_join cairo_set_line_join = FuncLoader.LoadFunction<d_cairo_set_line_join>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_line_join"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_line_width(IntPtr cr, double width);
		internal static d_cairo_set_line_width cairo_set_line_width = FuncLoader.LoadFunction<d_cairo_set_line_width>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_line_width"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_matrix(IntPtr cr, Matrix matrix);
		internal static d_cairo_set_matrix cairo_set_matrix = FuncLoader.LoadFunction<d_cairo_set_matrix>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_matrix"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_miter_limit(IntPtr cr, double limit);
		internal static d_cairo_set_miter_limit cairo_set_miter_limit = FuncLoader.LoadFunction<d_cairo_set_miter_limit>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_miter_limit"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_operator(IntPtr cr, Cairo.Operator op);
		internal static d_cairo_set_operator cairo_set_operator = FuncLoader.LoadFunction<d_cairo_set_operator>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_operator"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_scaled_font(IntPtr cr, IntPtr scaled_font);
		internal static d_cairo_set_scaled_font cairo_set_scaled_font = FuncLoader.LoadFunction<d_cairo_set_scaled_font>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_scaled_font"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_source(IntPtr cr, IntPtr pattern);
		internal static d_cairo_set_source cairo_set_source = FuncLoader.LoadFunction<d_cairo_set_source>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_source"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_source_rgb(IntPtr cr, double red, double green, double blue);
		internal static d_cairo_set_source_rgb cairo_set_source_rgb = FuncLoader.LoadFunction<d_cairo_set_source_rgb>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_source_rgb"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_source_rgba(IntPtr cr, double red, double green, double blue, double alpha);
		internal static d_cairo_set_source_rgba cairo_set_source_rgba = FuncLoader.LoadFunction<d_cairo_set_source_rgba>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_source_rgba"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_source_surface(IntPtr cr, IntPtr surface, double x, double y);
		internal static d_cairo_set_source_surface cairo_set_source_surface = FuncLoader.LoadFunction<d_cairo_set_source_surface>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_source_surface"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_set_tolerance(IntPtr cr, double tolerance);
		internal static d_cairo_set_tolerance cairo_set_tolerance = FuncLoader.LoadFunction<d_cairo_set_tolerance>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_set_tolerance"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_set_user_data (IntPtr cr, IntPtr key, IntPtr user_data, DestroyFunc destroy);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_show_glyphs(IntPtr ct, IntPtr glyphs, int num_glyphs);
		internal static d_cairo_show_glyphs cairo_show_glyphs = FuncLoader.LoadFunction<d_cairo_show_glyphs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_show_glyphs"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_show_page(IntPtr cr);
		internal static d_cairo_show_page cairo_show_page = FuncLoader.LoadFunction<d_cairo_show_page>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_show_page"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_show_text(IntPtr cr, byte[] utf8);
		internal static d_cairo_show_text cairo_show_text = FuncLoader.LoadFunction<d_cairo_show_text>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_show_text"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_show_text_glyphs (IntPtr cr, IntPtr utf8, int utf8_len, IntPtr glyphs, int num_glyphs, IntPtr clusters, int num_clusters, ClusterFlags cluster_flags);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_status(IntPtr cr);
		internal static d_cairo_status cairo_status = FuncLoader.LoadFunction<d_cairo_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_status"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_status_to_string(Status status);
		internal static d_cairo_status_to_string cairo_status_to_string = FuncLoader.LoadFunction<d_cairo_status_to_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_status_to_string"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_stroke(IntPtr cr);
		internal static d_cairo_stroke cairo_stroke = FuncLoader.LoadFunction<d_cairo_stroke>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_stroke"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_stroke_extents(IntPtr cr, out double x1, out double y1, out double x2, out double y2);
		internal static d_cairo_stroke_extents cairo_stroke_extents = FuncLoader.LoadFunction<d_cairo_stroke_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_stroke_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_stroke_preserve(IntPtr cr);
		internal static d_cairo_stroke_preserve cairo_stroke_preserve = FuncLoader.LoadFunction<d_cairo_stroke_preserve>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_stroke_preserve"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_surface_copy_page (IntPtr surface);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_surface_create_for_rectangle (IntPtr surface, double x, double y, double width, double height);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_surface_create_similar(IntPtr surface, Cairo.Content content, int width, int height);
		internal static d_cairo_surface_create_similar cairo_surface_create_similar = FuncLoader.LoadFunction<d_cairo_surface_create_similar>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_create_similar"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_destroy(IntPtr surface);
		internal static d_cairo_surface_destroy cairo_surface_destroy = FuncLoader.LoadFunction<d_cairo_surface_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_destroy"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_finish(IntPtr surface);
		internal static d_cairo_surface_finish cairo_surface_finish = FuncLoader.LoadFunction<d_cairo_surface_finish>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_finish"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_flush(IntPtr surface);
		internal static d_cairo_surface_flush cairo_surface_flush = FuncLoader.LoadFunction<d_cairo_surface_flush>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_flush"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Content d_cairo_surface_get_content(IntPtr surface);
		internal static d_cairo_surface_get_content cairo_surface_get_content = FuncLoader.LoadFunction<d_cairo_surface_get_content>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_content"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_surface_get_device(IntPtr surface);
		internal static d_cairo_surface_get_device cairo_surface_get_device = FuncLoader.LoadFunction<d_cairo_surface_get_device>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_device"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_get_device_offset(IntPtr surface, out double x, out double y);
		internal static d_cairo_surface_get_device_offset cairo_surface_get_device_offset = FuncLoader.LoadFunction<d_cairo_surface_get_device_offset>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_device_offset"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_surface_get_fallback_resolution (IntPtr surface, out double x_pixels_per_inch, out double y_pixels_per_inch);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_get_font_options(IntPtr surface, IntPtr FontOptions);
		internal static d_cairo_surface_get_font_options cairo_surface_get_font_options = FuncLoader.LoadFunction<d_cairo_surface_get_font_options>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_font_options"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_surface_get_mime_data (IntPtr surface, IntPtr mime_type, out IntPtr data, out IntPtr length);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint d_cairo_surface_get_reference_count(IntPtr surface);
		internal static d_cairo_surface_get_reference_count cairo_surface_get_reference_count = FuncLoader.LoadFunction<d_cairo_surface_get_reference_count>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_reference_count"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SurfaceType d_cairo_surface_get_type(IntPtr surface);
		internal static d_cairo_surface_get_type cairo_surface_get_type = FuncLoader.LoadFunction<d_cairo_surface_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_get_type"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_surface_get_user_data (IntPtr surface, IntPtr key);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//[return: MarshalAs (UnmanagedType.U1)]
		//internal static extern bool cairo_surface_has_show_text_glyphs (IntPtr surface);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_mark_dirty(IntPtr surface);
		internal static d_cairo_surface_mark_dirty cairo_surface_mark_dirty = FuncLoader.LoadFunction<d_cairo_surface_mark_dirty>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_mark_dirty"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_mark_dirty_rectangle(IntPtr surface, int x, int y, int width, int height);
		internal static d_cairo_surface_mark_dirty_rectangle cairo_surface_mark_dirty_rectangle = FuncLoader.LoadFunction<d_cairo_surface_mark_dirty_rectangle>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_mark_dirty_rectangle"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_surface_reference(IntPtr surface);
		internal static d_cairo_surface_reference cairo_surface_reference = FuncLoader.LoadFunction<d_cairo_surface_reference>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_reference"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_set_device_offset(IntPtr surface, double x, double y);
		internal static d_cairo_surface_set_device_offset cairo_surface_set_device_offset = FuncLoader.LoadFunction<d_cairo_surface_set_device_offset>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_set_device_offset"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_set_fallback_resolution(IntPtr surface, double x, double y);
		internal static d_cairo_surface_set_fallback_resolution cairo_surface_set_fallback_resolution = FuncLoader.LoadFunction<d_cairo_surface_set_fallback_resolution>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_set_fallback_resolution"));
		
		// DONTCARE
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern Status cairo_surface_set_user_data (IntPtr surface, IntPtr key, IntPtr user_data DestroyFunc destroy);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_surface_show_page (IntPtr surface);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_surface_status(IntPtr surface);
		internal static d_cairo_surface_status cairo_surface_status = FuncLoader.LoadFunction<d_cairo_surface_status>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_status"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_surface_write_to_png(IntPtr surface, string filename);
		internal static d_cairo_surface_write_to_png cairo_surface_write_to_png = FuncLoader.LoadFunction<d_cairo_surface_write_to_png>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_surface_write_to_png"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_surface_write_to_png_stream (IntPtr surface, WriteFunc writeFunc);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_svg_get_versions (out IntPtr versions, out int num_versions);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_svg_surface_create(string fileName, double width, double height);
		internal static d_cairo_svg_surface_create cairo_svg_surface_create = FuncLoader.LoadFunction<d_cairo_svg_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_svg_surface_create"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_svg_surface_create_for_stream (double width, double height);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_svg_surface_restrict_to_version(IntPtr surface, SvgVersion version);
		internal static d_cairo_svg_surface_restrict_to_version cairo_svg_surface_restrict_to_version = FuncLoader.LoadFunction<d_cairo_svg_surface_restrict_to_version>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_svg_surface_restrict_to_version"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_svg_version_to_string (SvgVersion version);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_text_cluster_allocate (int num_clusters);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_text_cluster_free (IntPtr clusters);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_text_extents(IntPtr cr, byte[] utf8, out TextExtents extents);
		internal static d_cairo_text_extents cairo_text_extents = FuncLoader.LoadFunction<d_cairo_text_extents>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_text_extents"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_text_path(IntPtr ct, byte[] utf8);
		internal static d_cairo_text_path cairo_text_path = FuncLoader.LoadFunction<d_cairo_text_path>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_text_path"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_toy_font_face_create (IntPtr face, FontSlant slant, FontWeight weight);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_toy_font_face_get_family (IntPtr face);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern FontSlant cairo_toy_font_face_get_slant (IntPtr face);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern FontWeight cairo_toy_font_face_get_weight (IntPtr face);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_transform(IntPtr cr, Matrix matrix);
		internal static d_cairo_transform cairo_transform = FuncLoader.LoadFunction<d_cairo_transform>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_transform"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_translate(IntPtr cr, double tx, double ty);
		internal static d_cairo_translate cairo_translate = FuncLoader.LoadFunction<d_cairo_translate>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_translate"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_user_font_face_create ();
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern ScaledFontInitFunc cairo_user_font_face_get_init_func (IntPtr face);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern ScaledFontRenderGlyphFunc cairo_user_font_face_get_render_glyph_func (IntPtr face);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_get_text_to_glyphs_func (blah)
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_get_unicode_to_glyph_func (blah)
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_set_init_func (blah)
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_set_render_glyph_func (blah)
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_set_text_to_glyphs_func (blah)
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern blah cairo_user_font_face_set_unicode_to_glyph_func (blah)
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_user_to_device(IntPtr cr, ref double x, ref double y);
		internal static d_cairo_user_to_device cairo_user_to_device = FuncLoader.LoadFunction<d_cairo_user_to_device>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_user_to_device"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_user_to_device_distance(IntPtr cr, ref double dx, ref double dy);
		internal static d_cairo_user_to_device_distance cairo_user_to_device_distance = FuncLoader.LoadFunction<d_cairo_user_to_device_distance>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_user_to_device_distance"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_version();
		internal static d_cairo_version cairo_version = FuncLoader.LoadFunction<d_cairo_version>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_version"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_version_string();
		internal static d_cairo_version_string cairo_version_string = FuncLoader.LoadFunction<d_cairo_version_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_version_string"));
		
		// not in the 1.10 doc index
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_directfb_surface_create(IntPtr dfb, IntPtr surface);
		internal static d_cairo_directfb_surface_create cairo_directfb_surface_create = FuncLoader.LoadFunction<d_cairo_directfb_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_directfb_surface_create"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_font_face_create_for_hfont (IntPtr hfont);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_win32_font_face_create_for_logfontw(IntPtr logfontw);
		internal static d_cairo_win32_font_face_create_for_logfontw cairo_win32_font_face_create_for_logfontw = FuncLoader.LoadFunction<d_cairo_win32_font_face_create_for_logfontw>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_font_face_create_for_logfontw"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_font_face_create_for_logfontw_hfont (IntPtr logfontw, IntPtr hfont);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_printing_surface_create (IntPtr hdc);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_win32_scaled_font_done_font(IntPtr scaled_font);
		internal static d_cairo_win32_scaled_font_done_font cairo_win32_scaled_font_done_font = FuncLoader.LoadFunction<d_cairo_win32_scaled_font_done_font>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_scaled_font_done_font"));
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_win32_scaled_font_get_logical_to_device (IntPtr scaled_font, out IntPtr matrix);
		
		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern void cairo_win32_scaled_font_get_device_to_logical (IntPtr scaled_font, out IntPtr matrix);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate double d_cairo_win32_scaled_font_get_metrics_factor(IntPtr scaled_font);
		internal static d_cairo_win32_scaled_font_get_metrics_factor cairo_win32_scaled_font_get_metrics_factor = FuncLoader.LoadFunction<d_cairo_win32_scaled_font_get_metrics_factor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_scaled_font_get_metrics_factor"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_win32_scaled_font_select_font(IntPtr scaled_font, IntPtr hdc);
		internal static d_cairo_win32_scaled_font_select_font cairo_win32_scaled_font_select_font = FuncLoader.LoadFunction<d_cairo_win32_scaled_font_select_font>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_scaled_font_select_font"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_win32_surface_create(IntPtr hdc);
		internal static d_cairo_win32_surface_create cairo_win32_surface_create = FuncLoader.LoadFunction<d_cairo_win32_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_surface_create"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_win32_surface_create_with_ddb(IntPtr hdc, Format format, int width, int height);
		internal static d_cairo_win32_surface_create_with_ddb cairo_win32_surface_create_with_ddb = FuncLoader.LoadFunction<d_cairo_win32_surface_create_with_ddb>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_win32_surface_create_with_ddb"));

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_surface_create_with_dib (Format format, int width, int height);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_surface_get_dc (IntPtr surface);

		//[DllImport (cairo, CallingConvention=CallingConvention.Cdecl)]
		//internal static extern IntPtr cairo_win32_surface_get_image (IntPtr surface);

		// not in the 1.10 doc index
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xcb_surface_create(IntPtr connection, uint drawable, IntPtr visual, int width, int height);
		internal static d_cairo_xcb_surface_create cairo_xcb_surface_create = FuncLoader.LoadFunction<d_cairo_xcb_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xcb_surface_create"));
		
		// not in the 1.10 doc index
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xcb_surface_create_for_bitmap(IntPtr connection, uint bitmap, IntPtr screen, int width, int height);
		internal static d_cairo_xcb_surface_create_for_bitmap cairo_xcb_surface_create_for_bitmap = FuncLoader.LoadFunction<d_cairo_xcb_surface_create_for_bitmap>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xcb_surface_create_for_bitmap"));
		
		// not in the 1.10 doc index
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_xcb_surface_set_size(IntPtr surface, int width, int height);
		internal static d_cairo_xcb_surface_set_size cairo_xcb_surface_set_size = FuncLoader.LoadFunction<d_cairo_xcb_surface_set_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xcb_surface_set_size"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_create(IntPtr display, IntPtr drawable, IntPtr visual, int width, int height);
		internal static d_cairo_xlib_surface_create cairo_xlib_surface_create = FuncLoader.LoadFunction<d_cairo_xlib_surface_create>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_create"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_create_for_bitmap(IntPtr display, IntPtr bitmap, IntPtr screen, int width, int height);
		internal static d_cairo_xlib_surface_create_for_bitmap cairo_xlib_surface_create_for_bitmap = FuncLoader.LoadFunction<d_cairo_xlib_surface_create_for_bitmap>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_create_for_bitmap"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_xlib_surface_get_depth(IntPtr surface);
		internal static d_cairo_xlib_surface_get_depth cairo_xlib_surface_get_depth = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_depth>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_depth"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_get_display(IntPtr surface);
		internal static d_cairo_xlib_surface_get_display cairo_xlib_surface_get_display = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_display>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_display"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_get_drawable(IntPtr surface);
		internal static d_cairo_xlib_surface_get_drawable cairo_xlib_surface_get_drawable = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_drawable>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_drawable"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_xlib_surface_get_height(IntPtr surface);
		internal static d_cairo_xlib_surface_get_height cairo_xlib_surface_get_height = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_height>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_height"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_get_screen(IntPtr surface);
		internal static d_cairo_xlib_surface_get_screen cairo_xlib_surface_get_screen = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_screen>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_screen"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_xlib_surface_get_visual(IntPtr surface);
		internal static d_cairo_xlib_surface_get_visual cairo_xlib_surface_get_visual = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_visual>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_visual"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int d_cairo_xlib_surface_get_width(IntPtr surface);
		internal static d_cairo_xlib_surface_get_width cairo_xlib_surface_get_width = FuncLoader.LoadFunction<d_cairo_xlib_surface_get_width>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_get_width"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_xlib_surface_set_drawable(IntPtr surface, IntPtr drawable, int width, int height);
		internal static d_cairo_xlib_surface_set_drawable cairo_xlib_surface_set_drawable = FuncLoader.LoadFunction<d_cairo_xlib_surface_set_drawable>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_set_drawable"));
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void d_cairo_xlib_surface_set_size(IntPtr surface, int width, int height);
		internal static d_cairo_xlib_surface_set_size cairo_xlib_surface_set_size = FuncLoader.LoadFunction<d_cairo_xlib_surface_set_size>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Cairo), "cairo_xlib_surface_set_size"));
	}
}

