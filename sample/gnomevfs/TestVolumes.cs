using Gnome.Vfs;
using System;

namespace Test.Gnome.Vfs {
	public class TestVolumes {
		public TestVolumes ()
		{
			Gnome.Vfs.Vfs.Initialize ();
			
			VolumeMonitor monitor = VolumeMonitor.Get ();
			monitor.DriveConnected += OnDriveConnected;
			monitor.DriveDisconnected += OnDriveDisconnected;
			monitor.VolumeMounted += OnVolumeMounted;
			monitor.VolumeUnmounted += OnVolumeUnmounted;
			
			GLib.List vols = monitor.MountedVolumes;
			Console.WriteLine ("Mounted volumes:");
			foreach (Volume v in vols)
				PrintVolume (v);
			
			GLib.List drives = monitor.ConnectedDrives;
			Console.WriteLine ("\nConnected drives:");
			foreach (Drive d in drives)
				PrintDrive (d);
			
			Console.WriteLine ("\nWaiting for volume events...");
			GLib.MainLoop loop = new GLib.MainLoop ();
			loop.Run ();
			
			Gnome.Vfs.Vfs.Shutdown ();
		}
		
		public void PrintDrive (Drive d)
		{
			Console.WriteLine ("Drive:");
			Console.WriteLine ("  ActivationUri:  {0}", d.ActivationUri);
			Console.WriteLine ("  DevicePath:     {0}", d.DevicePath);
			Console.WriteLine ("  DeviceType:     {0}", d.DeviceType);
			Console.WriteLine ("  DisplayName:    {0}", d.DisplayName);
			Console.WriteLine ("  Icon:           {0}", d.Icon);
			Console.WriteLine ("  Id:             {0}", d.Id);
			Console.WriteLine ("  IsConnected:    {0}", d.IsConnected);
			Console.WriteLine ("  IsMounted:      {0}", d.IsMounted);
			Console.WriteLine ("  IsUserVisible:  {0}", d.IsUserVisible);
		}
		
		public void PrintVolume (Volume v)
		{
			Console.WriteLine ("Volume:");
			Console.WriteLine ("  ActivationUri:  {0}", v.ActivationUri);
			Console.WriteLine ("  DevicePath:     {0}", v.DevicePath);
			Console.WriteLine ("  DeviceType:     {0}", v.DeviceType);
			Console.WriteLine ("  DisplayName:    {0}", v.DisplayName);
			Console.WriteLine ("  FilesystemType: {0}", v.FilesystemType);
			Console.WriteLine ("  HandlesTrash:   {0}", v.HandlesTrash);
			Console.WriteLine ("  Icon:           {0}", v.Icon);
			Console.WriteLine ("  Id:             {0}", v.Id);
			Console.WriteLine ("  IsMounted:      {0}", v.IsMounted);
			Console.WriteLine ("  IsReadOnly:     {0}", v.IsReadOnly);
			Console.WriteLine ("  IsUserVisible:  {0}", v.IsUserVisible);
			Console.WriteLine ("  VolumeType:     {0}", v.VolumeType);
		}
	
		static void Main (string[] args)
		{
			new TestVolumes ();
		}
		
		public void OnDriveConnected (object o, DriveConnectedArgs args)
		{
			Console.WriteLine ("Drive connected:");
			PrintDrive (args.Drive);
		}
		
		public void OnDriveDisconnected (object o, DriveDisconnectedArgs args)
		{
			Console.WriteLine ("Drive disconnected:");
			PrintDrive (args.Drive);
		}
		
		public void OnVolumeMounted (object o, VolumeMountedArgs args)
		{
			Console.WriteLine ("Volume mounted:");
			PrintVolume (args.Volume);
		}
		
		public void OnVolumeUnmounted (object o, VolumeUnmountedArgs args)
		{
			Console.WriteLine ("Volume unmounted:");
			PrintVolume (args.Volume);
		}
	}
}
