using GLib;
using Gnome.Vfs;
using System;
using System.Text;

namespace Test.Gnome.Vfs {
	public class TestMonitor {
		static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Usage: TestMonitor <uri>");
				return;
			}
		
			Gnome.Vfs.Vfs.Initialize ();

			Monitor monitor = new Monitor ();
			monitor.Changed += OnChanged;
			monitor.Deleted += OnDeleted;
			monitor.Created += OnCreated;
			monitor.MetadataChanged += OnMetadataChanged;
			
			monitor.Add (args[0], MonitorType.Directory);
			monitor.Add ("/tmp", MonitorType.Directory);
			
			new MainLoop ().Run ();
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		public static void OnChanged (string monitor, string uri)
		{
			Console.WriteLine ("Uri changed: {0}", uri);
		}
		
		public static void OnDeleted (string monitor, string uri)
		{
			Console.WriteLine ("Uri deleted: {0}", uri);
		}
		
		public static void OnCreated (string monitor, string uri)
		{
			Console.WriteLine ("Uri created: {0}", uri);
		}
		
		public static void OnMetadataChanged (string monitor, string uri)
		{
			Console.WriteLine ("Uri metadata changed: {0}", uri);
		}
	}
}
