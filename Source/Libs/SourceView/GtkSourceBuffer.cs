namespace Gtk.Source
{
    using System;
    public partial class GtkSourceBuffer : Gtk.TextBuffer
    {
        public GtkSourceBuffer() : base(IntPtr.Zero)
        {
            owned = true;
            Raw = gtk_source_buffer_new(IntPtr.Zero);
        }
    }
}
