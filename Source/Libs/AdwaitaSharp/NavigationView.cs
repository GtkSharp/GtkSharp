// NavigationView.cs - Adw NavigationView class customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Adw {

	using System;
	using System.Runtime.InteropServices;

	public partial class NavigationView {

		// adw_navigation_view_replace takes AdwNavigationPage ** plus a count,
		// and adw_navigation_view_replace_with_tags a char ** plus a count. The
		// api.xml records the pointer-to-pointer type, but the length is a
		// separate parameter rather than a NULL terminator and codegen has no
		// rule for that, so both were emitted taking a single value. The callee
		// then read the first machine word of that value as element zero: for
		// Replace, a page's GTypeInstance class pointer; for ReplaceWithTags,
		// eight bytes of the tag's own characters, used as an address. Both are
		// hidden in AdwaitaSharp.metadata.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_adw_navigation_view_replace(IntPtr raw, IntPtr[] pages, int n_pages);
		static d_adw_navigation_view_replace adw_navigation_view_replace = FuncLoader.LoadFunction<d_adw_navigation_view_replace>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Adwaita), "adw_navigation_view_replace"));

		/// <summary>Replaces the whole navigation stack; the last page becomes the visible one.</summary>
		public void Replace (NavigationPage[] pages)
		{
			if (pages == null)
				throw new ArgumentNullException ("pages");

			var handles = new IntPtr [pages.Length];
			for (int i = 0; i < pages.Length; i++) {
				if (pages [i] == null)
					throw new ArgumentException ("a navigation stack cannot hold a null page", "pages");
				handles [i] = pages [i].Handle;
			}

			adw_navigation_view_replace (Handle, handles, pages.Length);

			GC.KeepAlive (pages);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_adw_navigation_view_replace_with_tags(IntPtr raw, IntPtr[] tags, int n_tags);
		static d_adw_navigation_view_replace_with_tags adw_navigation_view_replace_with_tags = FuncLoader.LoadFunction<d_adw_navigation_view_replace_with_tags>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Adwaita), "adw_navigation_view_replace_with_tags"));

		/// <summary>Replaces the whole navigation stack with the pages carrying these tags.</summary>
		public void ReplaceWithTags (string[] tags)
		{
			if (tags == null)
				throw new ArgumentNullException ("tags");

			var native = new IntPtr [tags.Length];
			try {
				for (int i = 0; i < tags.Length; i++)
					native [i] = GLib.Marshaller.StringToPtrGStrdup (tags [i]);

				adw_navigation_view_replace_with_tags (Handle, native, tags.Length);
			} finally {
				for (int i = 0; i < native.Length; i++)
					GLib.Marshaller.Free (native [i]);
			}
		}
	}
}
