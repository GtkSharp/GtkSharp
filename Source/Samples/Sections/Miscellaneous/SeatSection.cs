using System;
using Gtk;

namespace Samples
{
    [Section(ContentType=typeof(SeatDemo), Category = Category.Miscellaneous)]
    class SeatSection : ListSection
    {
        public SeatSection()
        {
            AddItem("Press button to output mouse location:", new SeatDemo("Press me"));
        }
    }

    class SeatDemo : Button
    {
        public SeatDemo(string text) : base(text)
        {
        }

        protected override void OnClicked()
        {
            base.OnClicked();

            var seat = Display.DefaultSeat;
            ApplicationOutput.WriteLine($"Default seat: {seat}");

            // Gtk 4 removed gdk_device_get_position: a client cannot ask for
            // the pointer's location in root coordinates, because under Wayland
            // there are none. The position is only knowable relative to a
            // surface the client owns, which is what this reports.
            var surface = seat.Pointer.GetSurfaceAtPosition(out double x, out double y);
            if (surface != null)
                ApplicationOutput.WriteLine($"Position within {surface}: ({x}, {y})");
            else
                ApplicationOutput.WriteLine("Pointer is not over a surface of this application");
        }
    }
}