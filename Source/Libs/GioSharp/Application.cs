//
// Application.cs
//
// Author(s):
//	Antonius Riha <antoniusriha@gmail.com>
//
// Copyright (c) 2014 Antonius Riha
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

using System;
using System.Runtime.InteropServices;

namespace GLib
{
	public partial class Application
	{
		public Application () : this (null, ApplicationFlags.None)
		{
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_application_run(IntPtr raw, int argc, IntPtr argv);
		static d_g_application_run g_application_run = FuncLoader.LoadFunction<d_g_application_run>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gio), "g_application_run"));

		public int Run ()
		{
			return Run (null, null);
		}

		public int Run (string program_name, string[] args)
		{
			var argc = 0;
			var argv = IntPtr.Zero;
			if (program_name != null) {
				program_name = program_name.Trim ();
				if (program_name.Length == 0) {
					throw new ArgumentException ("program_name must not be empty.", "program_name");
				}

				if (args == null) {
					throw new ArgumentNullException ("args");
				}

				var prog_args = new string [args.Length + 1];
				prog_args [0] = program_name;
				args.CopyTo (prog_args, 1);

				argc = prog_args.Length;
				argv = new Argv (prog_args).Handle;
			}

			return g_application_run (Handle, argc, argv);
		}
	}
}

