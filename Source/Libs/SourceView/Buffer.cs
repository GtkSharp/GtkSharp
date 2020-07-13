namespace GtkSource
{
    using System;
    public partial class Buffer : Gtk.TextBuffer
    {
        public Buffer() : base(IntPtr.Zero)
        {
            owned = true;
            Raw = gtk_source_buffer_new(IntPtr.Zero);
        }
    }
}
