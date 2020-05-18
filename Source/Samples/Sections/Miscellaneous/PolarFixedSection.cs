using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(PolarFixed), Category = Category.Miscellaneous)]
    class PolarFixedSection : ListSection
    {
        public PolarFixedSection()
        {
            AddItem(CreateClock());
            AddItem(CreateSpiral());
        }

        public (string, Widget) CreateClock()
        {
            double theta;

            // Clock
            PolarFixed pf = new PolarFixed();

            for (int hour = 1; hour <= 12; hour++)
            {
                theta = (Math.PI / 2) - hour * (Math.PI / 6);
                if (theta < 0)
                    theta += 2 * Math.PI;

                Label l = new Label("<big><b>" + hour.ToString() + "</b></big>");
                l.UseMarkup = true;
                pf.Put(l, theta, 50);
            }

            return ("Clock", pf);
        }

        public (string, Widget) CreateSpiral()
        {
            uint r;
            double theta;

            var pf = new PolarFixed();


            r = 0;
            theta = 0.0;

            foreach (string id in Gtk.Stock.ListIds())
            {
                StockItem item = Gtk.Stock.Lookup(id);
                if (item.Label == null)
                    continue;
                var icon = Gtk.Image.NewFromIconName(item.StockId, IconSize.SmallToolbar);

                pf.Put(icon, theta, r);

                // Logarithmic spiral: r = a*e^(b*theta)
                r += 1;
                theta = 10 * Math.Log(10 * r);
            }

            return ("Spiral", pf);
        }
    }
}