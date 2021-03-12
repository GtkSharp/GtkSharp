using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(CompositeWidget), Category = Category.Widgets)]
    class CompositeWidgetSection : ListSection
    {
        public CompositeWidgetSection()
        {
            AddItem("CompositeWidget:", new CompositeWidget());
            AddItem("Other instance:", new CompositeWidget());
        }
    }

    [Template("CompositeWidget.glade")]
    [GLib.TypeName(nameof(CompositeWidget))]
    class CompositeWidget : Bin
    {
        [Child] Button button = null;
        [Child] Entry entry = null;

        public CompositeWidget()
        {
            button.Clicked += OnButtonClicked;
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            entry.Text = DateTime.Now.ToString();
        }
    }
}
