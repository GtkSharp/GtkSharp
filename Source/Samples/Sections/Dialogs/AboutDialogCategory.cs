using System;
using Gtk;

namespace Samples
{
    [SectionAttribute(Name = "AboutDialog", Category = Category.Dialogs)]
    class AboutDialogCategory : Box
    {
        public AboutDialogCategory() : base(Orientation.Vertical, 0)
        {

        }
    }
}