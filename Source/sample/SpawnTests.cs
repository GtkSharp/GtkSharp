// SpawnTests.cs - Tests for GLib.Process.Spawn*
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.

namespace GtkSamples {

	using Gtk;
	using Gdk;
	using GLib;
	using System;

	public class SpawnTests  {

		static MainLoop ml;

		public static void Main (string[] args)
		{
			CommandLineSyncTest ();
			CommandLineAsyncTest ();
			SyncTest ();
			AsyncTest ();
			AsyncWithPipesTest ();
			ml = new MainLoop ();
			ml.Run ();
		}

		static void CommandLineAsyncTest ()
		{
			Console.WriteLine ("CommandLineAsyncTest:");
			try {
				GLib.Process.SpawnCommandLineAsync ("echo \"[CommandLineAsync running: `pwd`]\"");
			} catch (Exception e) {
				Console.WriteLine ("Exception in SpawnCommandLineAsync: " + e);
			}
			Console.WriteLine ("returning");
		}

		static void CommandLineSyncTest ()
		{
			Console.WriteLine ("CommandLineSyncTest:");
			try {
				string stdout, stderr;
				int exit_status;
				GLib.Process.SpawnCommandLineSync ("pwd", out stdout, out stderr, out exit_status);
				Console.Write ("pwd exit_status=" + exit_status + " output: " + stdout);
			} catch (Exception e) {
				Console.WriteLine ("Exception in SpawnCommandLineSync: " + e);
			}
			Console.WriteLine ("returning");
		}

		static void SyncTest ()
		{
			Console.WriteLine ("SyncTest:");
			try {
				string stdout, stderr;
				int exit_status;
				GLib.Process.SpawnSync ("/usr", new string[] {"pwd"}, null, SpawnFlags.SearchPath, null, out stdout, out stderr, out exit_status);
				Console.Write ("pwd exit_status=" + exit_status + " output: " + stdout);
			} catch (Exception e) {
				Console.WriteLine ("Exception in SpawnSync: " + e);
			}
			Console.WriteLine ("returning");
		}

		static void AsyncTest ()
		{
			Console.WriteLine ("AsyncTest:");
			try {
				Process proc;
				GLib.Process.SpawnAsync (null, new string[] {"echo", "[AsyncTest running]"}, null, SpawnFlags.SearchPath, null, out proc);
			} catch (Exception e) {
				Console.WriteLine ("Exception in SpawnSync: " + e);
			}
			Console.WriteLine ("returning");
		}

		static IOChannel channel;

		static void AsyncWithPipesTest ()
		{
			Console.WriteLine ("AsyncWithPipesTest:");
			try {
				Process proc;
				int stdin = Process.IgnorePipe;
				int stdout = Process.RequestPipe;
				int stderr = Process.IgnorePipe;
				GLib.Process.SpawnAsyncWithPipes (null, new string[] {"pwd"}, null, SpawnFlags.SearchPath, null, out proc, ref stdin, ref stdout, ref stderr);
				channel = new IOChannel (stdout);
				channel.AddWatch (0, IOCondition.In | IOCondition.Hup, new IOFunc (ReadStdout));
			} catch (Exception e) {
				Console.WriteLine ("Exception in SpawnSync: " + e);
			}
			Console.WriteLine ("returning");
		}

		static bool ReadStdout (IOChannel source, IOCondition condition)
		{
			if ((condition & IOCondition.In) == IOCondition.In) {
				string txt;
				if (source.ReadToEnd (out txt) == IOStatus.Normal)
					Console.WriteLine ("[AsyncWithPipesTest output] " + txt);
			}
			if ((condition & IOCondition.Hup) == IOCondition.Hup) {
				source.Dispose ();
				ml.Quit ();
				return true;
			}
			return true;
		}
	}
}
