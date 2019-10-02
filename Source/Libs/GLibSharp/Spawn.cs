// glib/Spawn.cs : Spawn g_spawn API wrapper
//
// Author: Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
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
	using System.Runtime.InteropServices;
	
	public enum SpawnError {
		Fork, 
		Read,
		Chdir,
		Acces,
		Perm, 
		TooBig,
		NoExec,
		NameTooLong,
		NoEnt,     
		NoMem,    
		NotDir,  
		Loop,   
		TxtBusy,   
		IO,       
		NFile,   
		MFile,  
		Inval, 
		IsDir,
		LibBad,   
		Failed,  
	}

	[Flags]
	public enum SpawnFlags {
		LeaveDescriptorsOpen = 1 << 0,
		DoNotReapChild         = 1 << 1,
		SearchPath             = 1 << 2,
		StdoutToDevNull        = 1 << 3,
		StderrToDevNull        = 1 << 4,
		ChildInheritsStdin     = 1 << 5,
		FileAndArgvZero        = 1 << 6,
	}

	public delegate void SpawnChildSetupFunc ();

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SpawnChildSetupFuncNative (IntPtr gch);

	internal class SpawnChildSetupWrapper {

		SpawnChildSetupFunc handler;

		public SpawnChildSetupWrapper (SpawnChildSetupFunc handler)
		{
			if (handler == null)
				return;

			this.handler = handler;
			Data = (IntPtr) GCHandle.Alloc (this);
			NativeCallback = new SpawnChildSetupFuncNative (InvokeHandler);
		}

		public IntPtr Data;
		public SpawnChildSetupFuncNative NativeCallback;

		static void InvokeHandler (IntPtr data)
		{
			if (data == IntPtr.Zero)
				return;
			GCHandle gch = (GCHandle) data;
			(gch.Target as SpawnChildSetupWrapper).handler ();
			gch.Free ();
		}
	}

	public class Process {

		public const int IgnorePipe = Int32.MaxValue;
		public const int RequestPipe = 0;

		long pid;

		private Process (int pid) 
		{
			this.pid = pid;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_spawn_close_pid(int pid);
		static d_g_spawn_close_pid g_spawn_close_pid = FuncLoader.LoadFunction<d_g_spawn_close_pid>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_close_pid"));

		public void Close ()
		{
			g_spawn_close_pid ((int) pid);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_async(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out int pid, out IntPtr error);
		static d_g_spawn_async g_spawn_async = FuncLoader.LoadFunction<d_g_spawn_async>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_async"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_async_utf8(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out int pid, out IntPtr error);
		static d_g_spawn_async_utf8 g_spawn_async_utf8 = FuncLoader.LoadFunction<d_g_spawn_async_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_spawn_async_utf8"));

		public static bool SpawnAsync (string working_directory, string[] argv, string[] envp, SpawnFlags flags, SpawnChildSetupFunc child_setup, out Process child_process)
		{
			int pid;
			IntPtr error;
			IntPtr native_dir = Marshaller.StringToPtrGStrdup (working_directory);
			IntPtr[] native_argv = Marshaller.StringArrayToNullTermPointer (argv);
			IntPtr[] native_envp = Marshaller.StringArrayToNullTermPointer (envp);
			SpawnChildSetupWrapper wrapper = new SpawnChildSetupWrapper (child_setup);
			bool result;

			if (Global.IsWindowsPlatform)
				result = g_spawn_async_utf8 (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out pid, out error);
			else
				result = g_spawn_async (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out pid, out error);

			child_process = new Process (pid);
			Marshaller.Free (native_dir);
			Marshaller.Free (native_argv);
			Marshaller.Free (native_envp);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return result;
		}  
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_async_with_pipes(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out int pid, IntPtr stdin, IntPtr stdout, IntPtr stderr, out IntPtr error);
		static d_g_spawn_async_with_pipes g_spawn_async_with_pipes = FuncLoader.LoadFunction<d_g_spawn_async_with_pipes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_async_with_pipes"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_async_with_pipes_utf8(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out int pid, IntPtr stdin, IntPtr stdout, IntPtr stderr, out IntPtr error);
		static d_g_spawn_async_with_pipes_utf8 g_spawn_async_with_pipes_utf8 = FuncLoader.LoadFunction<d_g_spawn_async_with_pipes_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_spawn_async_with_pipes_utf8"));

		public static bool SpawnAsyncWithPipes (string working_directory, string[] argv, string[] envp, SpawnFlags flags, SpawnChildSetupFunc child_setup, out Process child_process, ref int stdin, ref int stdout, ref int stderr)
		{
			int pid;
			IntPtr error;
			IntPtr native_dir = Marshaller.StringToPtrGStrdup (working_directory);
			IntPtr[] native_argv = Marshaller.StringArrayToNullTermPointer (argv);
			IntPtr[] native_envp = Marshaller.StringArrayToNullTermPointer (envp);
			SpawnChildSetupWrapper wrapper = new SpawnChildSetupWrapper (child_setup);
			IntPtr in_ptr = stdin == IgnorePipe ? IntPtr.Zero : Marshal.AllocHGlobal (4);
			IntPtr out_ptr = stdout == IgnorePipe ? IntPtr.Zero : Marshal.AllocHGlobal (4);
			IntPtr err_ptr = stderr == IgnorePipe ? IntPtr.Zero : Marshal.AllocHGlobal (4);
			bool result;

			if (Global.IsWindowsPlatform)
				result = g_spawn_async_with_pipes_utf8 (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out pid, in_ptr, out_ptr, err_ptr, out error);
			else
				result = g_spawn_async_with_pipes (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out pid, in_ptr, out_ptr, err_ptr, out error);

			child_process = new Process (pid);
			if (in_ptr != IntPtr.Zero) {
				stdin = Marshal.ReadInt32 (in_ptr);
				Marshal.FreeHGlobal (in_ptr);
			}
			if (out_ptr != IntPtr.Zero) {
				stdout = Marshal.ReadInt32 (out_ptr);
				Marshal.FreeHGlobal (out_ptr);
			}
			if (err_ptr != IntPtr.Zero) {
				stderr = Marshal.ReadInt32 (err_ptr);
				Marshal.FreeHGlobal (err_ptr);
			}
			Marshaller.Free (native_dir);
			Marshaller.Free (native_argv);
			Marshaller.Free (native_envp);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return result;
		}  
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_sync(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out IntPtr stdout, out IntPtr stderr, out int exit_status, out IntPtr error);
		static d_g_spawn_sync g_spawn_sync = FuncLoader.LoadFunction<d_g_spawn_sync>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_sync"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_sync_utf8(IntPtr dir, IntPtr[] argv, IntPtr[] envp, int flags, SpawnChildSetupFuncNative func, IntPtr data, out IntPtr stdout, out IntPtr stderr, out int exit_status, out IntPtr error);
		static d_g_spawn_sync_utf8 g_spawn_sync_utf8 = FuncLoader.LoadFunction<d_g_spawn_sync_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_spawn_sync_utf8"));

		public static bool SpawnSync (string working_directory, string[] argv, string[] envp, SpawnFlags flags, SpawnChildSetupFunc child_setup, out string stdout, out string stderr, out int exit_status)
		{
			IntPtr native_stdout, native_stderr, error;
			IntPtr native_dir = Marshaller.StringToPtrGStrdup (working_directory);
			IntPtr[] native_argv = Marshaller.StringArrayToNullTermPointer (argv);
			IntPtr[] native_envp = Marshaller.StringArrayToNullTermPointer (envp);
			SpawnChildSetupWrapper wrapper = new SpawnChildSetupWrapper (child_setup);
			bool result;

			if (Global.IsWindowsPlatform)
				result = g_spawn_sync (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out native_stdout, out native_stderr, out exit_status, out error);
			else
				result = g_spawn_sync (native_dir, native_argv, native_envp, (int) flags, wrapper.NativeCallback, wrapper.Data, out native_stdout, out native_stderr, out exit_status, out error);

			Marshaller.Free (native_dir);
			Marshaller.Free (native_argv);
			Marshaller.Free (native_envp);
			stdout = Marshaller.PtrToStringGFree (native_stdout);
			stderr = Marshaller.PtrToStringGFree (native_stderr);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return result;
		}  
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_command_line_async(IntPtr cmdline, out IntPtr error);
		static d_g_spawn_command_line_async g_spawn_command_line_async = FuncLoader.LoadFunction<d_g_spawn_command_line_async>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_command_line_async"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_command_line_async_utf8(IntPtr cmdline, out IntPtr error);
		static d_g_spawn_command_line_async_utf8 g_spawn_command_line_async_utf8 = FuncLoader.LoadFunction<d_g_spawn_command_line_async_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_spawn_command_line_async_utf8"));

		public static bool SpawnCommandLineAsync (string command_line)
		{
			IntPtr error;
			IntPtr native_cmd = Marshaller.StringToPtrGStrdup (command_line);
			bool result;

			if (Global.IsWindowsPlatform)
				result = g_spawn_command_line_async_utf8 (native_cmd, out error);
			else
				result = g_spawn_command_line_async (native_cmd, out error);

			Marshaller.Free (native_cmd);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_command_line_sync(IntPtr cmdline, out IntPtr stdout, out IntPtr stderr, out int exit_status, out IntPtr error);
		static d_g_spawn_command_line_sync g_spawn_command_line_sync = FuncLoader.LoadFunction<d_g_spawn_command_line_sync>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_spawn_command_line_sync"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_spawn_command_line_sync_utf8(IntPtr cmdline, out IntPtr stdout, out IntPtr stderr, out int exit_status, out IntPtr error);
		static d_g_spawn_command_line_sync_utf8 g_spawn_command_line_sync_utf8 = FuncLoader.LoadFunction<d_g_spawn_command_line_sync_utf8>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_spawn_command_line_sync_utf8"));

		public static bool SpawnCommandLineSync (string command_line, out string stdout, out string stderr, out int exit_status)
		{
			IntPtr error, native_stdout, native_stderr;
			IntPtr native_cmd = Marshaller.StringToPtrGStrdup (command_line);
			bool result;

			if (Global.IsWindowsPlatform)
				result = g_spawn_command_line_sync_utf8 (native_cmd, out native_stdout, out native_stderr, out exit_status, out error);
			else
				result = g_spawn_command_line_sync (native_cmd, out native_stdout, out native_stderr, out exit_status, out error);

			Marshaller.Free (native_cmd);
			stdout = Marshaller.PtrToStringGFree (native_stdout);
			stderr = Marshaller.PtrToStringGFree (native_stderr);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return result;
		}
	}
}

