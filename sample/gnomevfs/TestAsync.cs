using GLib;
using Gnome.Vfs;
using System;
using System.Text;
using System.Threading;

namespace Test.Gnome.Vfs {
	public class TestAsync {
		private static MainLoop loop;
		private static Handle handle;
	
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestAsync <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);
			handle = Async.Open (uri, OpenMode.Read,
					     (int)Async.Priority.Default,
					     new Gnome.Vfs.AsyncCallback (OnOpen));
			
			loop = new MainLoop ();
			loop.Run ();
			
			Gnome.Vfs.Vfs.Shutdown ();
		}


		private static void OnOpen (Handle handle, Result result)
		{
			Console.WriteLine ("Uri opened: {0}", result);
			if (result != Result.Ok) {
				loop.Quit ();
				return;
			}
			
			byte[] buffer = new byte[1024];
			Async.Read (handle, out buffer[0], (uint)buffer.Length,
				    new AsyncReadCallback (OnRead));
		}
		
		private static void OnRead (Handle handle, Result result, byte[] buffer,
					    ulong bytes_requested, ulong bytes_read)
		{
			Console.WriteLine ("Read: {0}", result);
			if (result != Result.Ok && result != Result.ErrorEof) {
				loop.Quit ();
				return;
			}

			UTF8Encoding utf8 = new UTF8Encoding ();
			Console.WriteLine ("read ({0} bytes) : '{1}'", bytes_read,
					   utf8.GetString (buffer, 0, (int)bytes_read));

			if (bytes_read != 0)
				Async.Read (handle, out buffer[0], (uint)buffer.Length,
					    new AsyncReadCallback (OnRead));
			else
				Async.Close (handle, new Gnome.Vfs.AsyncCallback (OnClose));
		}
		
		private static void OnClose (Handle handle, Result result)
		{
			Console.WriteLine ("Close: {0}", result);
			loop.Quit ();
		}
	}
}
