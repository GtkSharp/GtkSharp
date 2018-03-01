using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace ${Namespace}
{
    public class ${EscapedIdentifier} : Box
    {
        public ${EscapedIdentifier}() : this(new Builder("${Namespace}.${EscapedIdentifier}.glade")) { }

        private ${EscapedIdentifier}(Builder builder) : base(builder.GetObject("${EscapedIdentifier}").Handle)
        {
            builder.Autoconnect(this);
        }
    }
}
