// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;
using System;
using System.Timers;

namespace Samples
{
    [Section(ContentType = typeof(ProgressBar), Category = Category.Widgets)]
    class ProgressBarSection : ListSection
    {
        public ProgressBarSection()
        {
            AddItem(CreateSimpleProgressBar());
            AddItem(CreateFractionProgressBar());
            AddItem(CreatePulseProgressBar());
        }

        public (string, Widget) CreateSimpleProgressBar()
        {
            var pb = new ProgressBar();

            // lets add a visible request size in our example
            pb.WidthRequest = 100;

            // add a text
            pb.Text = "Some progress...";

            // to show text it must be set to true
            pb.ShowText = true;

            // progressbar is used in percentage mode values between 0.0 and 1.0
            pb.Fraction = 0.60d;

            return ("Progress Bar:", pb);
        }

        public (string, Widget) CreateFractionProgressBar()
        {
            // this is used when application can report progress

            var pb = new ProgressBar();

            pb.WidthRequest = 200;

            pb.Text = "0%";
            pb.ShowText = true;
            pb.Fraction = 0d;

            // lets add a timer to demo it
            var timer = new Timer();
            timer.Interval = 1000;

            timer.Elapsed += (sender, e) =>
            {
                pb.Fraction += 0.1d;
                if (pb.Fraction >= 1d)
                    pb.Fraction = 0d;

                pb.Text = $"{Math.Truncate(pb.Fraction * 100)}%";
            };

            timer.Start();

            return ("Progress Bar with fraction:", pb);
        }

        public (string, Widget) CreatePulseProgressBar()
        {
            // this is used when application can't report progress

            var pb = new ProgressBar();

            pb.WidthRequest = 200;

            pb.Text = "Task time is unknown";
            pb.ShowText = true;

            // define how much is the pulse step
            pb.PulseStep = 0.1d;

            // lets add a timer to demo it
            var timer = new Timer();
            timer.Interval = 200;

            timer.Elapsed += (sender, e) => pb.Pulse();

            timer.Start();

            return ("Progress Bar with pulse:", pb);
        }

    }
}
