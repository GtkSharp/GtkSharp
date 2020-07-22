namespace GtkSource
{
    public partial class SourceView : Gtk.TextView
    {
        new public GtkSource.Buffer Buffer
        {
            get => base.Buffer as GtkSource.Buffer;
            set => base.Buffer = value;
        }
    }
}

