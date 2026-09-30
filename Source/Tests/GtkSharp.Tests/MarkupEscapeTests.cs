using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.Markup.EscapeText</c>, the whole of `GLibSharp/Markup.cs`.
    /// </summary>
    /// <remarks>
    /// Sixteen lines and one method, and it is the function every caller reaches
    /// for before putting user text into Pango markup or a `.ui` file. Getting
    /// it wrong does not produce an error — it produces markup that the parser
    /// then reads as *structure*, which is the injection shape.
    ///
    /// The oracle is the XML specification's five predefined entities, and a
    /// round trip: text escaped by this method has to survive Pango's markup
    /// parser and come back as the original.
    /// </remarks>
    public class MarkupEscapeTests : GtkTestBase
    {
        public MarkupEscapeTests(GtkFixture fixture) : base(fixture) { }

        [Theory]
        [InlineData("&", "&amp;")]
        [InlineData("<", "&lt;")]
        [InlineData(">", "&gt;")]
        [InlineData("'", "&apos;")]
        [InlineData("\"", "&quot;")]
        public void Each_of_the_characters_XML_reserves_is_replaced(string raw, string escaped)
        {
            Run(() => Assert.Equal(escaped, GLib.Markup.EscapeText(raw)));
        }

        [Fact]
        public void The_ampersand_is_escaped_once_and_not_twice()
        {
            // The classic double-escaping bug: if & were replaced after the
            // other entities were introduced, "<" would come out as "&amp;lt;".
            Run(() =>
            {
                Assert.Equal("&lt;b&gt;", GLib.Markup.EscapeText("<b>"));
                Assert.Equal("&amp;lt;", GLib.Markup.EscapeText("&lt;"));
                Assert.Equal("&amp;amp;", GLib.Markup.EscapeText("&amp;"));
            });
        }

        [Fact]
        public void Text_with_nothing_to_escape_comes_back_unchanged()
        {
            Run(() =>
            {
                Assert.Equal("plain text 123", GLib.Markup.EscapeText("plain text 123"));
                Assert.Equal(string.Empty, GLib.Markup.EscapeText(string.Empty));
            });
        }

        [Fact]
        public void Null_is_an_empty_string_rather_than_a_crash()
        {
            // The wrapper's own guard, ahead of the native call. Without it the
            // marshaller would hand g_markup_escape_text a null pointer with a
            // length of -1.
            Run(() => Assert.Equal(string.Empty, GLib.Markup.EscapeText(null)));
        }

        [Fact]
        public void Non_ascii_text_is_left_alone()
        {
            // The length passed to g_markup_escape_text is -1, meaning
            // NUL-terminated, so the byte count never has to be right -- but the
            // *encoding* does, and multi-byte characters are where a wrong one
            // shows.
            Run(() =>
            {
                const string text = "àèìòù 日本語 \U0001F600";

                Assert.Equal(text, GLib.Markup.EscapeText(text));
            });
        }

        [Fact]
        public void Escaped_text_survives_the_markup_parser_as_text()
        {
            // The end-to-end oracle, and the reason the method exists: markup
            // built from escaped text parses to the original string, and the
            // angle brackets are content rather than a tag.
            Run(() =>
            {
                const string hostile = "<b>not bold</b> & \"quoted\"";

                var markup = "<span>" + GLib.Markup.EscapeText(hostile) + "</span>";

                Pango.Global.ParseMarkup(markup, '\0', out _, out var text, out _);

                Assert.Equal(hostile, text);
            });
        }

        [Fact]
        public void Unescaped_text_would_have_been_read_as_structure()
        {
            // The control. Without it the test above only shows that a parser
            // exists, not that escaping changed the outcome -- the same input
            // unescaped either parses to something different or fails outright,
            // and both are the point.
            Run(() =>
            {
                const string hostile = "<b>not bold</b> & \"quoted\"";

                string parsed = null;
                bool threw = false;
                try
                {
                    Pango.Global.ParseMarkup("<span>" + hostile + "</span>", '\0',
                                             out _, out parsed, out _);
                }
                catch (GLib.GException)
                {
                    threw = true;
                }

                Assert.True(threw || parsed != hostile,
                            "unescaped markup should not have round-tripped as text");
            });
        }
    }
}
