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

        protected override void OnClicked()
        {
            base.OnClicked();

            Gdk.Display display = Gdk.Display.Default;

            // Gtk 4 exposes the monitors as a GListModel rather than an indexed
            // count, so the list can notify when one is plugged or unplugged
            // instead of having to be polled.
            var monitors = display.Monitors;
            uint monitorsCount = monitors.NItems;
            ApplicationOutput.WriteLine($"Monitors count: {monitorsCount}");
            for (uint i = 0; i < monitorsCount; i++)
            {
                Gdk.Monitor monitor = (Gdk.Monitor) monitors.GetObject(i);
                ApplicationOutput.WriteLine($"Monitor {i}:");
                // IsPrimary is gone: Gtk 4 has no notion of a primary monitor,
                // because Wayland has no such concept to report.
                ApplicationOutput.WriteLine($"\tConnector: {monitor.Connector}");
                ApplicationOutput.WriteLine($"\tManufacturer: {monitor.Manufacturer}");
                ApplicationOutput.WriteLine($"\tModel: {monitor.Model}");
                ApplicationOutput.WriteLine($"\tRefreshRate: {monitor.RefreshRate}");
                ApplicationOutput.WriteLine($"\tScaleFactor: {monitor.ScaleFactor}");
                ApplicationOutput.WriteLine($"\tWidthMm x HeightMm: {monitor.WidthMm} x {monitor.HeightMm}");
                ApplicationOutput.WriteLine($"\tGeometry: {monitor.Geometry}");
                // Workarea is gone too -- the area left free by panels and docks
                // is not something a Wayland client can be told.
            }
        }
    }
}
