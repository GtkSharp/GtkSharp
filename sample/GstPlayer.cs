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
		Application.Init ();

		/* create a new bin to hold the elements */
		Pipeline bin = new Pipeline("pipeline");

		/* create a disk reader */
		Element filesrc = ElementFactory.Make ("filesrc", "disk_source");
		filesrc.SetProperty ("location", new GLib.Value (args[0]));

		/* now it's time to get the decoder */
		Element decoder = ElementFactory.Make ("vorbisdec", "decode");

		/* and an audio sink */
		Element osssink = ElementFactory.Make ("osssink", "play_audio");

		/* add objects to the main pipeline */
		bin.Add (filesrc);
		bin.Add (decoder);
		bin.Add (osssink);

		/* connect the elements */
		filesrc.Connect (decoder);
		decoder.Connect (osssink);

		/* start playing */
		bin.SetState (ElementState.Playing);

		while (bin.Iterate ());

		/* stop the bin */
		bin.SetState (ElementState.Null);
	}
}
