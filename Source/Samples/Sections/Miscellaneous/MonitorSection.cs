using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(MonitorDemo), Category = Category.Miscellaneous)]
    class MonitorSection : ListSection
    {
        public MonitorSection()
        {
            AddItem("Press button to get monitors information:", new MonitorDemo("Press me"));
        }
    }

    class MonitorDemo : Button
    {
        public MonitorDemo(string text) : base(text)
        {
        }

        protected override void OnPressed()
        {
            base.OnPressed();

            Gdk.Display display = Gdk.Display.Default;
            int monitorsCount = display.NMonitors;
            ApplicationOutput.WriteLine($"Monitors count: {monitorsCount}");
            for (int i = 0; i < monitorsCount; i++)
            {
                Gdk.Monitor monitor = display.GetMonitor(i);
                ApplicationOutput.WriteLine($"Monitor {i}:");
                ApplicationOutput.WriteLine($"\tIsPrimary: {monitor.IsPrimary}");
                ApplicationOutput.WriteLine($"\tManufacturer: {monitor.Manufacturer}");
                ApplicationOutput.WriteLine($"\tModel: {monitor.Model}");
                ApplicationOutput.WriteLine($"\tRefreshRate: {monitor.RefreshRate}");
                ApplicationOutput.WriteLine($"\tScaleFactor: {monitor.ScaleFactor}");
                ApplicationOutput.WriteLine($"\tWidthMm x HeightMm: {monitor.WidthMm} x {monitor.HeightMm}");
                ApplicationOutput.WriteLine($"\tGeometry: {monitor.Geometry}");
                ApplicationOutput.WriteLine($"\tWorkarea: {monitor.Workarea}");
            }
        }
    }
}
