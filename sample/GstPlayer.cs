// GstPlayer.cs - a simple Vorbis media player using GStreamer
//
// Author: Alp Toker <alp@atoker.com>
//
// Copyright (c) 2002 Alp Toker

using System;
using Gst;

public class GstTest
{
	static void Main(string[] args)
	{
		if (args.Length != 1)
		{
			Console.WriteLine ("usage: Gst.Player.exe FILE.ogg");
			return;
		}
		
		Application.Init ();

		/* create a new bin to hold the elements */
		Pipeline bin = new Pipeline("pipeline");

		/* create a disk reader */
		Element filesrc = ElementFactory.Make ("filesrc", "disk_source");
		filesrc.SetProperty ("location", args[0]);

		/* now it's time to get the decoder */
		Element decoder = ElementFactory.Make ("vorbisfile", "decode");

		/* and an audio sink */
		Element osssink = ElementFactory.Make ("osssink", "play_audio");

		/* add objects to the main pipeline */
		bin.Add (filesrc);
		bin.Add (decoder);
		bin.Add (osssink);

		/* connect the elements */
		filesrc.Link (decoder);
		decoder.Link (osssink);

		/* start playing */
		bin.SetState (ElementState.Playing);

		while (bin.Iterate ());

		/* stop the bin */
		bin.SetState (ElementState.Null);
	}
}
