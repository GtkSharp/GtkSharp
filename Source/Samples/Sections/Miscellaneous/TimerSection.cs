using System;
using System.Collections.Generic;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(TimerDemo), Category = Category.Miscellaneous)]
    class TimerSection : Box
    {
        public TimerSection() : base(Orientation.Horizontal, 3)
        {
            Valign = Align.Start;
            TimerDemo.Create(this);
        }
    }

    static class TimerDemo
    {
        public static void Create(Box box)
        {
            List<uint> timers = new List<uint>();
            bool removeByHandler = false;

            var btnAddTimer = new Button() { Label = "Add timer" };
            btnAddTimer.Clicked += delegate
            {
                uint id = 0;
                id = GLib.Timeout.Add(500, () =>
                {
                    ApplicationOutput.WriteLine("Timer tick " + id);

                    if (removeByHandler)
                    {
                        removeByHandler = false;
                        timers.Remove(id);
                        ApplicationOutput.WriteLine("Remove timer from handler " + id);
                        return false;
                    }

                    return true;
                });

                timers.Add(id);
                ApplicationOutput.WriteLine("Add timer " + id);
            };

            var btnRemoveTimer = new Button() { Label = "Remove timer" };
            btnRemoveTimer.Clicked += delegate
            {
                if (timers.Count == 0)
                    return;

                uint id = timers[0];
                timers.RemoveAt(0);
                GLib.Timeout.Remove(id);
                ApplicationOutput.WriteLine("Remove timer " + id);
            };

            var btnRemoveTimerByHandler = new Button() { Label = "Remove timer by handler" };
            btnRemoveTimerByHandler.Clicked += delegate
            {
                if (timers.Count == 0)
                    return;

                removeByHandler = true;
                ApplicationOutput.WriteLine("Remove timer by handler");
            };

            var btnGc = new Button() { Label = "GC" };
            btnGc.Clicked += delegate
            {
                ApplicationOutput.WriteLine("GC");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            };

            box.PackStart(btnAddTimer, false, false, 0);
            box.PackStart(btnRemoveTimer, false, false, 0);
            box.PackStart(btnRemoveTimerByHandler, false, false, 0);
            box.PackStart(btnGc, false, false, 0);
        }
    }
}
