using GLib;
using Gnome.Vfs;
using System;
using System.Text;

namespace TestGnomeVfs {
	public class TestCallback {
		private static MainLoop loop;
	
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestCallback <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Gnome.Vfs.Uri uri = new Gnome.Vfs.Uri (args[0]);
			Handle handle;

			// Test 1: Attempt to access a URI requiring authentication w/o a callback registered.
			try {
				handle = Sync.Open (uri, OpenMode.Read);
				Sync.Close (handle);
				Console.WriteLine ("Uri '{0}' doesn't require authentication", uri);
				return;
			} catch (VfsException ex) {
				if (ex.Result != Result.ErrorAccessDenied)
					throw ex;
			}
			
			// Test 2: Attempt an open that requires authentication.
			ModuleCallbackFullAuthentication cb = new ModuleCallbackFullAuthentication ();
			cb.Callback += new ModuleCallbackHandler (OnAuthenticate);
			cb.SetDefault ();
			
			handle = Sync.Open (uri, OpenMode.Read);
			Sync.Close (handle);

			// Test 3: This call should not require any new authentication.
			Console.WriteLine ("File info: \n{0}", uri.GetFileInfo ());
			
			// Test 4: Attempt a call to the parent uri.
			FileInfo[] entries = Directory.GetEntries (uri.Parent);
			Console.WriteLine ("Directory '{0}' has {1} entries", uri.Parent, entries.Length);
			
			// Test 5: Pop the authentication callback and try again.
			cb.Pop ();
			try {
				handle = Sync.Open (uri, OpenMode.Read);
			} catch (VfsException ex) {
				if (ex.Result != Result.ErrorAccessDenied)
					throw ex;
			}
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		private static void OnAuthenticate (ModuleCallback cb)
		{
			ModuleCallbackFullAuthentication fcb = cb as ModuleCallbackFullAuthentication;
			Console.Write ("Enter your username ({0}): ", fcb.Username);
			string username = Console.ReadLine ();
			Console.Write ("Enter your password : ");
			string passwd = Console.ReadLine ();
			
			if (username.Length > 0)
				fcb.Username = username;
			fcb.Password = passwd;
		}
	}
}
