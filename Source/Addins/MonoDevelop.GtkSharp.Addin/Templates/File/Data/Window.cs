using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace ${Namespace}
{
    class ${EscapedIdentifier} : Window
    {
        public ${EscapedIdentifier}() : this(new Builder("${Namespace}.${EscapedIdentifier}.glade")) { }

        private ${EscapedIdentifier}(Builder builder) : base(builder.GetObject("${EscapedIdentifier}").Handle)
        {
            builder.Autoconnect(this);

            DeleteEvent += OnDeleteEvent;
        }

        private void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}
