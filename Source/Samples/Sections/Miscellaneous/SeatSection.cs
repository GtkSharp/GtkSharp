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

        protected override void OnPressed()
        {
            base.OnPressed();

            var seat = Display.DefaultSeat;
            ApplicationOutput.WriteLine($"Default seat: {seat}");

            seat.Pointer.GetPosition(null, out int x, out int y);
            ApplicationOutput.WriteLine($"Position: ({x}, {y})");
        }
    }
}