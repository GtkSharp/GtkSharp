// Generated File.  Do not modify.
// <c> 2001-2002 Mike Kestner

namespace GtkSharp {
	using System;
	using System.Runtime.InteropServices;

	internal delegate void voidObjectAffineSVPintDelegate(IntPtr arg0, IntPtr arg1, ref Art.SVP arg2, int arg3, int key);

	internal class voidObjectAffineSVPintSignal : SignalCallback {

		private static voidObjectAffineSVPintDelegate _Delegate;

		private static void voidObjectAffineSVPintCallback(IntPtr arg0, IntPtr arg1, ref Art.SVP arg2, int arg3, int key)
		{
			if (!_Instances.Contains(key))
				throw new Exception("Unexpected signal key " + key);

			voidObjectAffineSVPintSignal inst = (voidObjectAffineSVPintSignal) _Instances[key];
			SignalArgs args = (SignalArgs) Activator.CreateInstance (inst._argstype);
			args.Args = new object[3];
			if (arg1 != IntPtr.Zero) {
				double[] affine = new double[6];
				Marshal.Copy (arg1, affine, 0, 6);
				args.Args[0] = affine;
			} else {
				args.Args[0] = null;
			}

			args.Args[1] = arg2;
			args.Args[2] = arg3;
			
			object[] argv = new object[2];
			argv[0] = inst._obj;
			argv[1] = args;
			inst._handler.DynamicInvoke(argv);
		}

		[DllImport("gobject-2.0")]		static extern void g_signal_connect_data(IntPtr obj, String name, voidObjectAffineSVPintDelegate cb, int key, IntPtr p, int flags);

		public voidObjectAffineSVPintSignal(GLib.Object obj, IntPtr raw, String name, MulticastDelegate eh, Type argstype) : base(obj, eh, argstype)
		{
			if (_Delegate == null) {
				_Delegate = new voidObjectAffineSVPintDelegate(voidObjectAffineSVPintCallback);
			}
			g_signal_connect_data(raw, name, _Delegate, _key, new IntPtr(0), 0);
		}

		~voidObjectAffineSVPintSignal()
		{
			_Instances.Remove(_key);
			if(_Instances.Count == 0) {
				_Delegate = null;
			}
		}
	}
}
