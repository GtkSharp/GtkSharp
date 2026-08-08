using System.Collections.Generic;
using Gtk;

namespace GettingStarted.Pages
{
    /// <summary>
    /// "Text". TextBuffer, iterators, marks and tags -- plus the two pieces of
    /// behaviour that surprise everyone: which gravity keeps a mark still, and
    /// what ForwardWordEnd returns at the end of the buffer.
    /// </summary>
    public class TextPage : TourPage
    {
        private readonly TextView _view = new TextView();

        public TextPage() : base(
            "Text",
            "A TextBuffer holds the text, iterators point into it, marks survive edits and tags "
          + "carry formatting.")
        {
            _view.Buffer.Text = "The quick brown fox jumps over the lazy dog";
            _view.WrapMode = WrapMode.Word;

            Append(Group("A buffer, and a tag applied to a range", BufferDemo()));
            Append(Group("Mark gravity: the left-gravity mark is the one that stays put", GravityDemo()));
            Append(Group("ForwardWordEnd returns false at the end of the buffer", WordsDemo()));
        }

        private Widget BufferDemo()
        {
            var column = Column();
            column.Append(new Frame { Child = _view });

            var output = Output();
            var row = Row();

            var bold = new Button { Label = "Bold the first two words" };
            bold.Clicked += (o, e) =>
            {
                var buffer = _view.Buffer;

                // Look the tag up before creating it: adding a name that is
                // already in the table fails rather than updating it.
                var tag = buffer.TagTable.Lookup("bold");
                if (tag == null)
                {
                    // gtk_text_buffer_create_tag is variadic, so it is not bound:
                    // build the tag and register it. Weight is typed as
                    // Pango.Weight by a hand-written property, not as the gint
                    // the C API takes.
                    tag = new TextTag("bold")
                    {
                        Weight = Pango.Weight.Bold,
                        Foreground = "#c01c28"
                    };
                    buffer.TagTable.Add(tag);
                }

                buffer.ApplyTag(tag, buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(9));
                Report("Applied a TextTag over a range of the buffer.");
            };

            var read = new Button { Label = "Read the whole buffer" };
            read.Clicked += (o, e) =>
            {
                var buffer = _view.Buffer;
                buffer.GetBounds(out var start, out var end);

                // A buffer offset is in characters; C# lengths are in UTF-16
                // units and UTF-8 needs a third number again. They agree only
                // for ASCII, which is why the count is worth showing.
                var text = buffer.GetText(start, end, false);
                output.Text = $"{buffer.CharCount} characters, {text.Length} UTF-16 units:\n\"{text}\"";
            };

            row.Append(bold);
            row.Append(read);
            column.Append(row);
            column.Append(output);
            return column;
        }

        private Widget GravityDemo()
        {
            var column = Column();
            var output = Output();

            var button = new Button { Label = "Insert at both marks and compare", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                var buffer = new TextBuffer(new TextTagTable()) { Text = "AB" };
                var at = buffer.GetIterAtOffset(1);

                // The name describes which side of an insertion the mark ends up
                // on, not where it moves to -- so left gravity is the one that
                // does NOT move.
                var left = buffer.CreateMark("left", at, true);
                var right = buffer.CreateMark("right", at, false);

                var insertAt = buffer.GetIterAtOffset(1);
                buffer.Insert(ref insertAt, "xyz");

                output.Text =
                    $"inserted \"xyz\" at offset 1 of \"AB\" -> \"{Text(buffer)}\"\n"
                  + $"left-gravity mark  is at offset {buffer.GetIterAtMark(left).Offset} (stayed)\n"
                  + $"right-gravity mark is at offset {buffer.GetIterAtMark(right).Offset} (pushed along)";

                Report("Left gravity keeps the mark where it was.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private Widget WordsDemo()
        {
            var column = Column();
            var output = Output();

            var button = new Button { Label = "Count words the naive way", Halign = Align.Start };
            button.Clicked += (o, e) =>
            {
                var buffer = _view.Buffer;
                buffer.GetBounds(out _, out var end);

                var iter = buffer.GetIterAtOffset(0);
                var words = new List<string>();
                var start = 0;

                // The naive loop: it stops one word short, because the last word
                // ends at the end of the buffer and ForwardWordEnd reports false
                // there even though it moved.
                while (iter.ForwardWordEnd())
                {
                    words.Add(buffer.GetText(buffer.GetIterAtOffset(start), iter, false).Trim());
                    start = iter.Offset;
                }

                var actual = buffer.GetText(buffer.GetIterAtOffset(0), end, false)
                                   .Split(' ');

                output.Text = $"ForwardWordEnd loop found {words.Count} words: {string.Join(", ", words)}\n"
                            + $"the text actually has {actual.Length} -- the last one is missing";

                Report("ForwardWordEnd returns false at the end of the buffer.");
            };

            column.Append(button);
            column.Append(output);
            return column;
        }

        private static string Text(TextBuffer buffer)
        {
            buffer.GetBounds(out var start, out var end);
            return buffer.GetText(start, end, false);
        }
    }
}
