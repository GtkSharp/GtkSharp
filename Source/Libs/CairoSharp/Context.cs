//
// Mono.Cairo.Context.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Miguel de Icaza (miguel@novell.com)
//   Hisham Mardam Bey (hisham.mardambey@gmail.com)
//   Alp Toker (alp@atoker.com)
//
// (C) Ximian Inc, 2003.
// (C) Novell Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Text;
using Cairo;

namespace Cairo {

	[Obsolete ("Renamed Cairo.Context per suggestion from cairo binding guidelines.")]
	public class Graphics : Context {
		public Graphics (IntPtr state) : base (state) {}
		public Graphics (Surface surface) : base (surface) {}
	}

	public class Context : IDisposable
	{
		IntPtr handle = IntPtr.Zero;

		static int native_glyph_size, c_compiler_long_size;

		static Context ()
		{
			//
			// This is used to determine what kind of structure
			// we should use to marshal Glyphs, as the public
			// definition in Cairo uses `long', which can be
			// 32 bits or 64 bits depending on the platform.
			//
			// We assume that sizeof(long) == sizeof(void*)
			// except in the case of Win64 where sizeof(long)
			// is 32 bits
			//
			int ptr_size = Marshal.SizeOf (typeof (IntPtr));

			PlatformID platform = Environment.OSVersion.Platform;
			if (platform == PlatformID.Win32NT ||
			    platform == PlatformID.Win32S ||
			    platform == PlatformID.Win32Windows ||
			    platform == PlatformID.WinCE ||
			    ptr_size == 4){
				c_compiler_long_size = 4;
				native_glyph_size = Marshal.SizeOf (typeof (NativeGlyph_4byte_longs));
			} else {
				c_compiler_long_size = 8;
				native_glyph_size = Marshal.SizeOf (typeof (Glyph));
			}
		}

		public Context (Surface surface) : this (NativeMethods.cairo_create (surface.Handle), true)
		{
		}

		public Context (IntPtr handle, bool owner)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle should not be NULL", "handle");

			this.handle = handle;
			if (!owner)
				NativeMethods.cairo_reference (handle);
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		[Obsolete]
		public Context (IntPtr state) : this (state, true)
		{
		}

		~Context ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || CairoDebug.Enabled)
				CairoDebug.OnDisposed<Context> (handle, disposing);

			if (handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_destroy (handle);
			handle = IntPtr.Zero;
		}

		void CheckDisposed ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("Object has already been disposed");
		}

		public void Save ()
		{
			CheckDisposed ();
			NativeMethods.cairo_save (handle);
		}

		public void Restore ()
		{
			CheckDisposed ();
			NativeMethods.cairo_restore (handle);
		}

		public Antialias Antialias {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_antialias (handle);
			}
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_antialias (handle, value);
			}
		}

		public Cairo.Status Status {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_status (handle);
			}
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public Operator Operator {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_operator (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_operator (handle);
			}
		}

		[Obsolete ("Use SetSourceColor method")]
		public Color Color {
			set {
				SetSourceColor (value);
			}
		}

		[Obsolete ("Use SetSourceRGBA method")]
		public Cairo.Color ColorRgb {
			set {
				SetSourceRGBA (value.R, value.G, value.B, value.A);
			}
		}

		public double Tolerance {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_tolerance (handle);
			}

			set {
				CheckDisposed ();
				NativeMethods.cairo_set_tolerance (handle, value);
			}
		}

		public Cairo.FillRule FillRule {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_fill_rule (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_fill_rule (handle);
			}
		}

		public double LineWidth {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_line_width (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_line_width (handle);
			}
		}

		public Cairo.LineCap LineCap {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_line_cap (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_line_cap (handle);
			}
		}

		public Cairo.LineJoin LineJoin {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_line_join (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_line_join (handle);
			}
		}

		public void SetDash (double [] dashes, double offset)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_dash (handle, dashes, dashes.Length, offset);
		}

		[Obsolete("Use GetSource/SetSource")]
		public Pattern Pattern {
			set {
				SetSource (value);
			}
			get {
				return GetSource ();
			}
		}

		//This is obsolete because it wasn't obvious it needed to be disposed
		[Obsolete("Use GetSource/SetSource")]
		public Pattern Source {
			set {
				SetSource (value);
			}
			get {
				return GetSource ();
			}
		}

		public void SetSource (Pattern source)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source (handle, source.Handle);
		}

		public Pattern GetSource ()
		{
			CheckDisposed ();
			var ptr = NativeMethods.cairo_get_source (handle);
			return Cairo.Pattern.Lookup (ptr, false);
		}

		public double MiterLimit {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_miter_limit (handle, value);
			}

			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_miter_limit (handle);
			}
		}

		public PointD CurrentPoint {
			get {
				CheckDisposed ();
				double x, y;
				NativeMethods.cairo_get_current_point (handle, out x, out y);
				return new PointD (x, y);
			}
		}

		public bool HasCurrentPoint {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_has_current_point (handle);
			}
		}

		[Obsolete ("Use GetTarget/SetTarget")]
		public Cairo.Surface Target {
			set {
				SetTarget (value);
			}

			get {
				return GetTarget ();
			}
		}

		public Surface GetTarget ()
		{
			CheckDisposed ();
			return Surface.Lookup (NativeMethods.cairo_get_target (handle), false);
		}

		public void SetTarget (Surface target)
		{
			CheckDisposed ();
			if (handle != IntPtr.Zero)
				NativeMethods.cairo_destroy (handle);
			handle = NativeMethods.cairo_create (target.Handle);
		}

		[Obsolete("Use GetScaledFont/SetScaledFont")]
		public ScaledFont ScaledFont {
			set {
				SetScaledFont (value);
			}

			get {
				return GetScaledFont ();
			}
		}

		public ScaledFont GetScaledFont ()
		{
			CheckDisposed ();
			return new ScaledFont (NativeMethods.cairo_get_scaled_font (handle), false);
		}

		public void SetScaledFont (ScaledFont font)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_scaled_font (handle, font.Handle);
		}

		public uint ReferenceCount {
			get {
				CheckDisposed ();
				return NativeMethods.cairo_get_reference_count (handle);
			}
		}

		public void SetSourceColor (Color color)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_rgba (handle, color.R, color.G, color.B, color.A);
		}

		public void SetSourceRGB (double r, double g, double b)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_rgb (handle, r, g, b);
		}

		public void SetSourceRGBA (double r, double g, double b, double a)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_rgba (handle, r, g, b, a);
		}

		//[Obsolete ("Use SetSource method (with double parameters)")]
		public void SetSourceSurface (Surface source, int x, int y)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_surface (handle, source.Handle, x, y);
		}

		public void SetSource (Surface source, double x, double y)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_surface (handle, source.Handle, x, y);
		}

		public void SetSource (Surface source)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_source_surface (handle, source.Handle, 0, 0);
		}

#region Path methods

		public void NewPath ()
		{
			CheckDisposed ();
			NativeMethods.cairo_new_path (handle);
		}

		public void NewSubPath ()
		{
			CheckDisposed ();
			NativeMethods.cairo_new_sub_path (handle);
		}

		public void MoveTo (PointD p)
		{
			MoveTo (p.X, p.Y);
		}

		public void MoveTo (double x, double y)
		{
			CheckDisposed ();
			NativeMethods.cairo_move_to (handle, x, y);
		}

		public void LineTo (PointD p)
		{
			LineTo (p.X, p.Y);
		}

		public void LineTo (double x, double y)
		{
			CheckDisposed ();
			NativeMethods.cairo_line_to (handle, x, y);
		}

		public void CurveTo (PointD p1, PointD p2, PointD p3)
		{
			CurveTo (p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
		}

		public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
		{
			CheckDisposed ();
			NativeMethods.cairo_curve_to (handle, x1, y1, x2, y2, x3, y3);
		}

		public void RelMoveTo (Distance d)
		{
			RelMoveTo (d.Dx, d.Dy);
		}

		public void RelMoveTo (double dx, double dy)
		{
			CheckDisposed ();
			NativeMethods.cairo_rel_move_to (handle, dx, dy);
		}

		public void RelLineTo (Distance d)
		{
			RelLineTo (d.Dx, d.Dy);
		}

		public void RelLineTo (double dx, double dy)
		{
			CheckDisposed ();
			NativeMethods.cairo_rel_line_to (handle, dx, dy);
		}

		public void RelCurveTo (Distance d1, Distance d2, Distance d3)
		{
			RelCurveTo (d1.Dx, d1.Dy, d2.Dx, d2.Dy, d3.Dx, d3.Dy);
		}

		public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			CheckDisposed ();
			NativeMethods.cairo_rel_curve_to (handle, dx1, dy1, dx2, dy2, dx3, dy3);
		}

		public void Arc (double xc, double yc, double radius, double angle1, double angle2)
		{
			CheckDisposed ();
			NativeMethods.cairo_arc (handle, xc, yc, radius, angle1, angle2);
		}

		public void ArcNegative (double xc, double yc, double radius, double angle1, double angle2)
		{
			CheckDisposed ();
			NativeMethods.cairo_arc_negative (handle, xc, yc, radius, angle1, angle2);
		}

		public void Rectangle (Rectangle rectangle)
		{
			Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		public void Rectangle (PointD p, double width, double height)
		{
			Rectangle (p.X, p.Y, width, height);
		}

		public void Rectangle (double x, double y, double width, double height)
		{
			CheckDisposed ();
			NativeMethods.cairo_rectangle (handle, x, y, width, height);
		}

		public void ClosePath ()
		{
			CheckDisposed ();
			NativeMethods.cairo_close_path (handle);
		}

		public Path CopyPath ()
		{
			CheckDisposed ();
			return new Path (NativeMethods.cairo_copy_path (handle));
		}

		public Path CopyPathFlat ()
		{
			CheckDisposed ();
			return new Path (NativeMethods.cairo_copy_path_flat (handle));
		}

		public void AppendPath (Path path)
		{
			CheckDisposed ();
			NativeMethods.cairo_append_path (handle, path.Handle);
		}

		public Rectangle PathExtents ()
		{
			CheckDisposed ();
			double x1, y1, x2, y2;
			NativeMethods.cairo_path_extents (handle, out x1, out y1, out x2, out y2);
			return new Rectangle (x1, y1, x2 - x1, y2 - y1);
		}

#endregion

#region Painting Methods
		public void Paint ()
		{
			CheckDisposed ();
			NativeMethods.cairo_paint (handle);
		}

		public void PaintWithAlpha (double alpha)
		{
			CheckDisposed ();
			NativeMethods.cairo_paint_with_alpha (handle, alpha);
		}

		public void Mask (Pattern pattern)
		{
			CheckDisposed ();
			NativeMethods.cairo_mask (handle, pattern.Handle);
		}

		public void MaskSurface (Surface surface, double surface_x, double surface_y)
		{
			CheckDisposed ();
			NativeMethods.cairo_mask_surface (handle, surface.Handle, surface_x, surface_y);
		}

		public void Stroke ()
		{
			CheckDisposed ();
			NativeMethods.cairo_stroke (handle);
		}

		public void StrokePreserve ()
		{
			CheckDisposed ();
			NativeMethods.cairo_stroke_preserve (handle);
		}

		public Rectangle StrokeExtents ()
		{
			CheckDisposed ();
			double x1, y1, x2, y2;
			NativeMethods.cairo_stroke_extents (handle, out x1, out y1, out x2, out y2);
			return new Rectangle (x1, y1, x2 - x1, y2 - y1);
		}

		public void Fill ()
		{
			CheckDisposed ();
			NativeMethods.cairo_fill (handle);
		}

		public Rectangle FillExtents ()
		{
			CheckDisposed ();
			double x1, y1, x2, y2;
			NativeMethods.cairo_fill_extents (handle, out x1, out y1, out x2, out y2);
			return new Rectangle (x1, y1, x2 - x1, y2 - y1);
		}

		public void FillPreserve ()
		{
			CheckDisposed ();
			NativeMethods.cairo_fill_preserve (handle);
		}

#endregion

		public void Clip ()
		{
			CheckDisposed ();
			NativeMethods.cairo_clip (handle);
		}

		public Rectangle ClipExtents()
		{
			CheckDisposed();
			double x1, y1, x2, y2;
			NativeMethods.cairo_clip_extents(handle, out x1, out y1, out x2, out y2);
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		public void ClipPreserve ()
		{
			CheckDisposed ();
			NativeMethods.cairo_clip_preserve (handle);
		}

		public void ResetClip ()
		{
			CheckDisposed ();
			NativeMethods.cairo_reset_clip (handle);
		}

		public bool InStroke (double x, double y)
		{
			CheckDisposed ();
			return NativeMethods.cairo_in_stroke (handle, x, y);
		}

		public bool InClip (double x, double y)
		{
			CheckDisposed ();
			return NativeMethods.cairo_in_clip (handle, x, y);
		}

		public bool InFill (double x, double y)
		{
			CheckDisposed ();
			return NativeMethods.cairo_in_fill (handle, x, y);
		}

		public Pattern PopGroup ()
		{
			CheckDisposed ();
			return Pattern.Lookup (NativeMethods.cairo_pop_group (handle), true);
		}

		public void PopGroupToSource ()
		{
			CheckDisposed ();
			NativeMethods.cairo_pop_group_to_source (handle);
		}

		public void PushGroup ()
		{
			CheckDisposed ();
			NativeMethods.cairo_push_group (handle);
		}

		public void PushGroup (Content content)
		{
			CheckDisposed ();
			NativeMethods.cairo_push_group_with_content (handle, content);
		}

		[Obsolete ("Use GetGroupTarget()")]
		public Surface GroupTarget {
			get {
				return GetGroupTarget ();
			}
		}

		public Surface GetGroupTarget ()
		{
			CheckDisposed ();
			IntPtr surface = NativeMethods.cairo_get_group_target (handle);
			return Surface.Lookup (surface, false);
		}

		public void Rotate (double angle)
		{
			CheckDisposed ();
			NativeMethods.cairo_rotate (handle, angle);
		}

		public void Scale (double sx, double sy)
		{
			CheckDisposed ();
			NativeMethods.cairo_scale (handle, sx, sy);
		}

		public void Translate (double tx, double ty)
		{
			CheckDisposed ();
			NativeMethods.cairo_translate (handle, tx, ty);
		}

		public void Transform (Matrix m)
		{
			CheckDisposed ();
			NativeMethods.cairo_transform (handle, m);
		}

		[Obsolete("Use UserToDevice instead")]
		public void TransformPoint (ref double x, ref double y)
		{
			UserToDevice (ref x, ref y);
		}

		[Obsolete("Use UserToDeviceDistance instead")]
		public void TransformDistance (ref double dx, ref double dy)
		{
			UserToDevice (ref dx, ref dy);
		}

		[Obsolete("Use DeviceToUser instead")]
		public void InverseTransformPoint (ref double x, ref double y)
		{
			UserToDevice (ref x, ref y);
		}

		[Obsolete("Use DeviceToUserDistance instead")]
		public void InverseTransformDistance (ref double dx, ref double dy)
		{
			DeviceToUserDistance (ref dx, ref dy);
		}

		public void UserToDevice (ref double x, ref double y)
		{
			CheckDisposed ();
			NativeMethods.cairo_user_to_device (handle, ref x, ref y);
		}

		public void UserToDeviceDistance (ref double dx, ref double dy)
		{
			CheckDisposed ();
			NativeMethods.cairo_user_to_device_distance (handle, ref dx, ref dy);
		}

		public void DeviceToUser (ref double x, ref double y)
		{
			CheckDisposed ();
			NativeMethods.cairo_device_to_user (handle, ref x, ref y);
		}

		public void DeviceToUserDistance (ref double dx, ref double dy)
		{
			CheckDisposed ();
			NativeMethods.cairo_device_to_user_distance (handle, ref dx, ref dy);
		}

		public Matrix Matrix {
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_matrix (handle, value);
			}

			get {
				CheckDisposed ();
				Matrix m = new Matrix ();
				NativeMethods.cairo_get_matrix (handle, m);
				return m;
			}
		}

		public void SetFontSize (double scale)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_font_size (handle, scale);
		}

		public void IdentityMatrix ()
		{
			CheckDisposed ();
			NativeMethods.cairo_identity_matrix (handle);
		}

		[Obsolete ("Use SetFontSize() instead.")]
		public void FontSetSize (double scale)
		{
			SetFontSize (scale);
		}

		[Obsolete ("Use SetFontSize() instead.")]
		public double FontSize {
			set { SetFontSize (value); }
		}

		public Matrix FontMatrix {
			get {
				CheckDisposed ();
				Matrix m;
				NativeMethods.cairo_get_font_matrix (handle, out m);
				return m;
			}
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_font_matrix (handle, value);
			}
		}

		public FontOptions FontOptions {
			get {
				CheckDisposed ();
				FontOptions options = new FontOptions ();
				NativeMethods.cairo_get_font_options (handle, options.Handle);
				return options;
			}
			set {
				CheckDisposed ();
				NativeMethods.cairo_set_font_options (handle, value.Handle);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NativeGlyph_4byte_longs {
			public int index;
			public double x;
			public double y;

			public NativeGlyph_4byte_longs (Glyph source)
			{
				index = (int) source.index;
				x = source.x;
				y = source.y;
			}
		}

		static internal IntPtr FromGlyphToUnManagedMemory(Glyph [] glyphs)
		{
			IntPtr dest = Marshal.AllocHGlobal (native_glyph_size * glyphs.Length);
			long pos = dest.ToInt64();

			if (c_compiler_long_size == 8){
				foreach (Glyph g in glyphs){
					Marshal.StructureToPtr (g, (IntPtr)pos, false);
					pos += native_glyph_size;
				}
			} else {
				foreach (Glyph g in glyphs){
					NativeGlyph_4byte_longs n = new NativeGlyph_4byte_longs (g);

					Marshal.StructureToPtr (n, (IntPtr)pos, false);
					pos += native_glyph_size;
				}
			}

			return dest;
		}

		public void ShowGlyphs (Glyph[] glyphs)
		{
			CheckDisposed ();

			IntPtr ptr;

			ptr = FromGlyphToUnManagedMemory (glyphs);

			NativeMethods.cairo_show_glyphs (handle, ptr, glyphs.Length);

			Marshal.FreeHGlobal (ptr);
		}

		[Obsolete("The matrix argument was never used, use ShowGlyphs(Glyphs []) instead")]
		public void ShowGlyphs (Matrix matrix, Glyph[] glyphs)
		{
			ShowGlyphs (glyphs);
		}

		[Obsolete("The matrix argument was never used, use GlyphPath(Glyphs []) instead")]
		public void GlyphPath (Matrix matrix, Glyph[] glyphs)
		{
			GlyphPath (glyphs);
		}

		public void GlyphPath (Glyph[] glyphs)
		{
			CheckDisposed ();

			IntPtr ptr;

			ptr = FromGlyphToUnManagedMemory (glyphs);

			NativeMethods.cairo_glyph_path (handle, ptr, glyphs.Length);

			Marshal.FreeHGlobal (ptr);
		}

		public FontExtents FontExtents {
			get {
				CheckDisposed ();
				FontExtents f_extents;
				NativeMethods.cairo_font_extents (handle, out f_extents);
				return f_extents;
			}
		}

		public void CopyPage ()
		{
			CheckDisposed ();
			NativeMethods.cairo_copy_page (handle);
		}

		[Obsolete ("Use SelectFontFace() instead.")]
		public void FontFace (string family, FontSlant slant, FontWeight weight)
		{
			SelectFontFace (family, slant, weight);
		}

		[Obsolete("Use GetFontFace/SetFontFace")]
		public FontFace ContextFontFace {
			get {
				return GetContextFontFace ();
			}
			set {
				SetContextFontFace (value);
			}
		}

		public FontFace GetContextFontFace ()
		{
			CheckDisposed ();
			return Cairo.FontFace.Lookup (NativeMethods.cairo_get_font_face (handle), false);
		}

		public void SetContextFontFace (FontFace value)
		{
			CheckDisposed ();
			NativeMethods.cairo_set_font_face (handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public void SelectFontFace (string family, FontSlant slant, FontWeight weight)
		{
			CheckDisposed ();
			NativeMethods.cairo_select_font_face (handle, family, slant, weight);
		}

		public void ShowPage ()
		{
			CheckDisposed ();
			NativeMethods.cairo_show_page (handle);
		}

		private static byte[] TerminateUtf8(byte[] utf8)
		{
			if (utf8.Length > 0 && utf8[utf8.Length - 1] == 0)
				return utf8;
			var termedArray = new byte[utf8.Length + 1];
			Array.Copy(utf8, termedArray, utf8.Length);
			termedArray[utf8.Length] = 0;
			return termedArray;
		}

		private static byte[] TerminateUtf8(string s)
		{
			// compute the byte count including the trailing \0
			var byteCount = Encoding.UTF8.GetMaxByteCount(s.Length + 1);
			var bytes = new byte[byteCount];
			Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
			return bytes;
		}

		public void ShowText (string str)
		{
			CheckDisposed ();
			NativeMethods.cairo_show_text (handle, TerminateUtf8 (str));
		}

		public void ShowText (byte[] utf8)
		{
			CheckDisposed ();
			NativeMethods.cairo_show_text (handle, TerminateUtf8 (utf8));
		}

		public void TextPath (string str)
		{
			CheckDisposed ();
			NativeMethods.cairo_text_path (handle, TerminateUtf8 (str));
		}

		public void TextPath (byte[] utf8)
		{
			CheckDisposed ();
			NativeMethods.cairo_text_path (handle, TerminateUtf8 (utf8));
		}

		public TextExtents TextExtents (string s)
		{
			CheckDisposed ();
			TextExtents extents;
			NativeMethods.cairo_text_extents (handle, TerminateUtf8 (s), out extents);
			return extents;
		}

		public TextExtents TextExtents(byte[] utf8)
		{
			CheckDisposed ();
			TextExtents extents;
			NativeMethods.cairo_text_extents (handle, TerminateUtf8 (utf8), out extents);
			return extents;
		}

		public TextExtents GlyphExtents (Glyph[] glyphs)
		{
			CheckDisposed ();

			IntPtr ptr = FromGlyphToUnManagedMemory (glyphs);

			TextExtents extents;

			NativeMethods.cairo_glyph_extents (handle, ptr, glyphs.Length, out extents);

			Marshal.FreeHGlobal (ptr);

			return extents;
		}
	}
}
