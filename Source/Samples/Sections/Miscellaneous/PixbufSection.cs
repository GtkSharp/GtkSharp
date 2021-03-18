using System;
using System.IO;
using System.Threading;
using Gdk;
using Gtk;

namespace Samples
{

	[Section (ContentType = typeof(PixbufDemo), Category = Category.Miscellaneous)]
	class PixbufSection : ListSection
	{

		public PixbufSection ()
		{
			AddItem ($"Press button to run / stop {nameof(PixbufDemo)} :", new PixbufDemo ("Press me"));
		}

	}

	class PixbufDemo : Button
	{

		public PixbufDemo (string text) : base (text) { }

		private bool running = false;

		public void DispatchPendingEvents ()
		{
			// The loop is limited to 1000 iterations as a workaround for an issue that some users
			// have experienced. Sometimes EventsPending starts return 'true' for all iterations,
			// causing the loop to never end.

			int n = 1000;
			Gdk.Threads.Enter ();

			while (Gtk.Application.EventsPending () && --n > 0) {
				Gtk.Application.RunIteration (false);
			}

			Gdk.Threads.Leave ();
		}

		protected override void OnPressed ()
		{
			base.OnPressed ();
			var count = 0;

			if (running) {
				running = false;

				return;
			}

			var startmem = GC.GetTotalMemory (true);
			var testfile = "Textpic.png";

			using var teststream = typeof(ImageSection).Assembly.GetManifestResourceStream("Testpic");
			using (var writeTestFile = new FileStream(testfile, FileMode.Create)) {
				teststream.CopyTo(writeTestFile);
			}

			using (var heatup = new Pixbuf (testfile)) {
				ApplicationOutput.WriteLine ($"{nameof(heatup)}.{nameof(Pixbuf.ByteLength)}\t{heatup.ByteLength:N0}");
			}

			startmem = GC.GetTotalMemory (true);
			ApplicationOutput.WriteLine ($"{nameof(GC.GetTotalMemory)} at start: {startmem:N}");
			running = true;

			var memAllocated = 0UL;

			while (running) {

				using (var source = new Pixbuf (typeof(ImageSection).Assembly, "Testpic")) {
					memAllocated += source.ByteLength;
					count++;
				}

				DispatchPendingEvents ();

				if (!running)
					break;

			}

			var endmem = GC.GetTotalMemory (true);
			ApplicationOutput.WriteLine ($"Leak:\t{(endmem - startmem):N0}\t{nameof(memAllocated)}");
			ApplicationOutput.WriteLine ($"{nameof(GC.GetTotalMemory)} at start: {startmem:N0}\tat end: {endmem:N0}\t{nameof(Pixbuf)} created: {count}");

		}

	}

}