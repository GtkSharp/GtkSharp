using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace ${Namespace}
{
    class ${EscapedIdentifier} : Dialog
    {
        public MyDialog() : this(new Builder("${Namespace}.${EscapedIdentifier}.glade")) { }

        private MyDialog(Builder builder) : base(builder.GetObject("${EscapedIdentifier}").Handle)
        {
            builder.Autoconnect(this);
            DefaultResponse = ResponseType.Cancel;

            Response += OnResponse;
        }

        private void OnResponse(object o, ResponseArgs args)
        {
            Hide();
        }
    }
}
