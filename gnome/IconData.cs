// IconData.cs - Manual implementation of GnomeIconData struct in GTK+-2.4.
//
// Authors: Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
// Copyright (c) 2004 Novell, Inc.

namespace Gnome {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct IconData {

		public bool HasEmbeddedRect;
		public int X0;
		public int Y0;
		public int X1;
		public int Y1;
		private IntPtr _attach_points;

		public Gnome.IconDataPoint attach_points {
			get { return Gnome.IconDataPoint.New (_attach_points); }
		}
		public int NAttachPoints;
		public string DisplayName;

		public static Gnome.IconData Zero = new Gnome.IconData ();

		public static Gnome.IconData New(IntPtr raw) {
			if (raw == IntPtr.Zero) {
				return Gnome.IconData.Zero;
			}
			Gnome.IconData self = new Gnome.IconData();
			self = (Gnome.IconData) Marshal.PtrToStructure (raw, self.GetType ());
			return self;
		}

		[DllImport("gnomeui-2")]
		static extern void gnome_icon_data_free(ref Gnome.IconData raw);

		public void Free() {
			gnome_icon_data_free(ref this);
		}

		[DllImport("gnomeui-2")]
		static extern IntPtr gnome_icon_data_dup(ref Gnome.IconData raw);

		public Gnome.IconData Dup() {
			IntPtr raw_ret = gnome_icon_data_dup(ref this);
			Gnome.IconData ret = Gnome.IconData.New (raw_ret);
			return ret;
		}
	}
}
